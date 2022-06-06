using SecurePolicyBasedDataAccess.Models;
using SecurePolicyBasedDataAccess.Models.Playbook;
using System;

namespace SecurePolicyBasedDataAccess.Infrastructure.Extensions
{
    public class Poort8Utilities
    {
        private readonly string _serviceProviders = "EU.EORI.TCNS";
        private readonly string _purpose = "Unattended delivery or return";
        public Poort8Utilities()
        {

        }

        public PlaybookPolicy CreatePolicy(Poort8PolicyModel model)
        {
            string resourceType = GetResourceTypeByModel(model.GenericType);
            return new PlaybookPolicy
            {
                Email = $"{model.Email}",
                Organization = $"{model.Actor}",
                Username = $"{model.UserName}",
                Policy = new Poort8Policy(resourceType)
                {
                    Actor = $"{model.Actor}",
                    Issuer = $"{model.Issuer}",
                    Note = $"{model.Note}",
                    ContextRule = new PlaybookContextRule
                    {
                        Purpose = _purpose,
                        ResourceAttribute = "*",
                        ResourceIdentifier = $"{model.GenericKey}",
                        ServiceProvider = _serviceProviders,
                        NotBefore = int.Parse(DateTime.UtcNow.AddMinutes(1).ToEpoch()),
                        NotOnOrAfter = int.Parse(model.ToDate.Date.AddDays(1).AddSeconds(-1).ToEpoch()),
                    }
                }
            };
        }

        private string GetResourceTypeByModel(int genericType)
        {
            //TODO get Resource Type
            throw new NotImplementedException();
        }

        public Poort8Delegation CreateDelegationEvidence(string genericKey, int genericType, string issuer, string actor)
        {
            //TODO implement code insert to your database
            throw new NotImplementedException();
        }

        public P8AccessPolicyModel ParseDelegationToken(string delegation_token)
        {
            //TODO read data from your database
            throw new NotImplementedException();
        }
    }
}
