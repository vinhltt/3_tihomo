namespace Ocelot.Gateway.Configuration
{
    public class InternalServiceSettings
    {
        public const string SectionName = "InternalServiceSettings";
        public string? IdentityService { get; set; }
        public string? CoreFinanceService { get; set; }
    }
}
