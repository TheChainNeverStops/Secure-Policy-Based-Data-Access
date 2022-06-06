using System.Collections.Generic;

namespace SecurePolicyBasedDataAccess.Models
{
    public class PolicySet
    {
        public int MaxDelegationDepth { get; set; }
        public List<Policy> Policies { get; set; }
    }

    public class Policy
    {
        public PolicyTarget Target { get; set; }
        public List<PolicyRule> Rules { get; set; }
    }

    public class PolicyRule
    {
        public string Effect { get; set; } = "Permit";
        public PolicyRule(string effect)
        {
            Effect = effect;
        }
    }

    public class PolicyTarget
    {
        public PolicyTargetResource Resource { get; set; }

        public PolicyTargetEnvironment Environment { get; set; }

        public List<string> Actions { get; set; }
    }

    public class PolicyTargetResource
    {
        public string Type { get; set; }
        public List<string> Identifiers { get; set; }
        public List<string> Attributes { get; set; }

        public bool HasSameType(PolicyTargetResource target) => Type == target.Type;
    }

    public class PolicyTargetEnvironment
    {
        public List<string> ServiceProviders { get; set; }
    }
}
