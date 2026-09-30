using CCAP.Web.Common.Models;
using CCAP.Web.Features.Authentication.Services;
using CCAP.Web.Features.MockData;
using CCAP.Web.Features.Patients.Models;

namespace CCAP.Web.Features.Patients.Services;

public sealed class PatientService
{
    private readonly CcapApiClient _api;
    private readonly MockDataStore _mock;
    private readonly MockDataOptions _options;

    public PatientService(CcapApiClient api, MockDataStore mock, MockDataOptions options)
    {
        _api = api; _mock = mock; _options = options;
    }

    public async Task<PagedResult<PatientListItem>> GetPatientsAsync(
        int pageNumber = 1, int pageSize = 20, PatientListQuery? query = null,
        CancellationToken cancellationToken = default)
    {
        query ??= new();
        if (_options.Enabled)
        {
            var all = _mock.Patients.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var s = query.Search.Trim();
                all = all.Where(x => x.Name.Contains(s, StringComparison.OrdinalIgnoreCase) || x.MRN.Contains(s, StringComparison.OrdinalIgnoreCase) || x.PrimaryDiagnosis.Contains(s, StringComparison.OrdinalIgnoreCase));
            }
            if (!string.IsNullOrWhiteSpace(query.Status)) all = all.Where(x => string.Equals(x.Status, query.Status, StringComparison.OrdinalIgnoreCase));
            if (query.ClinicianId.HasValue) all = all.Where(x => x.AssignedClinicianId == query.ClinicianId);
            all = Sort(all, query.SortBy, query.SortDescending);
            var total = all.Count();
            return new PagedResult<PatientListItem> { Items = all.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList(), PageNumber = pageNumber, PageSize = pageSize, TotalCount = total, TotalPages = (int)Math.Ceiling(total / (double)pageSize) };
        }

        var url = $"api/patients?pageNumber={pageNumber}&pageSize={pageSize}&sortBy={Uri.EscapeDataString(query.SortBy)}&sortDescending={query.SortDescending.ToString().ToLowerInvariant()}";
        if (!string.IsNullOrWhiteSpace(query.Search)) url += $"&search={Uri.EscapeDataString(query.Search.Trim())}";
        if (!string.IsNullOrWhiteSpace(query.Status)) url += $"&status={Uri.EscapeDataString(query.Status.Trim())}";
        if (query.ClinicianId.HasValue) url += $"&clinicianId={query.ClinicianId.Value}";
        return await _api.GetFromJsonAsync<PagedResult<PatientListItem>>(url, cancellationToken) ?? new();
    }

    public async Task<List<PatientListItem>> GetPatientsAsync(CancellationToken cancellationToken = default)
        => (await GetPatientsAsync(1, 100, new(), cancellationToken)).Items.ToList();

    private static IEnumerable<PatientListItem> Sort(IEnumerable<PatientListItem> source, string by, bool desc)
    {
        Func<PatientListItem, object?> key = by.ToLowerInvariant() switch { "mrn" => x => x.MRN, "status" => x => x.Status, "diagnosis" => x => x.PrimaryDiagnosis, "clinician" => x => x.AssignedClinician, _ => x => x.Name };
        return desc ? source.OrderByDescending(key) : source.OrderBy(key);
    }
}
