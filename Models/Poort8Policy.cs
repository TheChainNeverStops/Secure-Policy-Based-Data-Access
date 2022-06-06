using SecurePolicyBasedDataAccess.Models.Playbook;

namespace SecurePolicyBasedDataAccess.Models
{
    public class Poort8Policy
    {
        public Poort8Policy(string resourceType = "Delivery order data")
        {
            ResourceType = resourceType;
        }

        public string PolicyFormat { get; set; } = "logistics";
        public string Playbook { get; set; } = "SmartDelivery";
        public string PlaybookVersion { get; set; } = "0.1.0";
        public string Note { get; set; } = "";
        public string Issuer { get; set; }
        public string Actor { get; set; }
        public string ResourceType { get; set; }
        public string Action { get; set; } = "Read";
        public string Effect { get; set; } = "allow";
        public PlaybookContextRule ContextRule { get; set; }
    }
}
