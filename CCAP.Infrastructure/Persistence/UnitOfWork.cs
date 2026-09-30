using CCAP.Application.Abstractions.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CCAP.Infrastructure.Persistence;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            /*
             * A concurrency exception without a configured concurrency
             * token means that one of the tracked UPDATE/DELETE entities
             * no longer exists in the database.
             *
             * Detach those missing entities and retry the remaining
             * database changes.
             *
             * This is important for workflow records because a task may
             * have been removed while the Patient record still exists.
             */
            var entries = _context.ChangeTracker
                .Entries()
                .Where(x =>
                    x.State == EntityState.Modified ||
                    x.State == EntityState.Deleted)
                .ToList();

            var detached = false;

            foreach (var entry in entries)
            {
                var databaseValues =
                    await entry.GetDatabaseValuesAsync(
                        cancellationToken);

                if (databaseValues == null)
                {
                    entry.State = EntityState.Detached;
                    detached = true;
                }
            }

            if (!detached)
                throw;

            return await _context.SaveChangesAsync(
                cancellationToken);
        }
    }
}