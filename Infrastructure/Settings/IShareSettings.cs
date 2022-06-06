namespace SecurePolicyBasedDataAccess.Infrastructure.Settings
{
    public class IShareSettings
    {
        public string Host { get; set; }
        public string LinkPolicy { get; set; }
        public string LinkDelegationEvidence { get; set; }
        public string TargetAudience { get; set; } = "EU.EORI. Your Audience";
        public string UrlSchemeAuthorize { get; set; } = "testing/generate-authorize-request";
        public string UrlPoort8GetToken { get; set; } = "ar-preview/ishare/connect/token";
        public string ConnectionStrings { get; set; }
    }
}
