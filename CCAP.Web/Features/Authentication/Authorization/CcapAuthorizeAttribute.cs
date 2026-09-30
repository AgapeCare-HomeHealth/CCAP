namespace CCAP.Web.Features.Authentication.Authorization
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public sealed class CcapAuthorizeAttribute : Attribute
    {
        public string? Policy { get; set; }

        public string? Roles { get; set; }
    }
}
