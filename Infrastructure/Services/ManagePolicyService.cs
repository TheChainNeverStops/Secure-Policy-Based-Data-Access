using Dapper;
using MySql.Data.MySqlClient;
using SecurePolicyBasedDataAccess.Models;
using Serilog;
using System;
using System.Threading.Tasks;

namespace SecurePolicyBasedDataAccess.Infrastructure.Services
{
    public interface IManagePolicyService
    {
        Task SavePolicyAsync(Poort8PolicyModel model, string policyId);
        Task<string> GetDelegationInfoAsync(string deliveryNumber);
        Task<string> GetDataInfoAsync(string genericKey, int genericType, string issuer, string actor);
    }

    public class ManagePolicyService
    {
        private string _connectionString;
        public ManagePolicyService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task SavePolicyAsync(Poort8PolicyModel model, string policyId)
        {
            try
            {
                using var db = new MySqlConnection(_connectionString);
                await db.OpenAsync();
                string query = $"INSERT INTO YourTable Values ()";

                var dp = new DynamicParameters();
                dp.Add("@policyId", policyId);
                dp.Add("@fromDate", DateTime.UtcNow);
                dp.Add("@toDate", model.ToDate);

                int res = db.Execute(query, dp);
                if (res > 0)
                {
                    Log.Information("[Save Policy] Row inserted");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Inserted has error {ex.Message}", ex);
            }

            await Task.CompletedTask;
        }

        public async Task<string> GetDelegationInfoAsync(string genericKey)
        {
            

            string condition = "";

            string query = $"Select * from YourTable " +
                $"Where {condition}" +
                $"And DATE(fromDate) <= @today And DATE(toDate) >= @today";

            return await GetAvailableKeyByConditionAsync(query, genericKey);
            
        }
        
        public async Task<string> GetDataInfoAsync(string genericKey, int genericType, string issuer, string actor)
        {
            using var db = new MySqlConnection(_connectionString);
            await db.OpenAsync();

            string condition = GetConditionWhere(genericType, issuer, actor);

            string query = $"Select * from YourTable " +
                $"Where {condition}" +
                $"And DATE(fromDate) <= @today And DATE(toDate) >= @today";

            return await GetAvailableKeyByConditionAsync(query, genericKey);
        }

        private string GetConditionWhere(int genericType, string issuer, string actor)
        {
            //TODO get data from yout database
            throw new NotImplementedException();
        }

        private async Task<string> GetAvailableKeyByConditionAsync(string query, string genericKey)
        {
            using var db = new MySqlConnection(_connectionString);
            await db.OpenAsync();

            try
            {
                var genericKeys = await db.QueryAsync(query, new
                {
                    today = DateTime.UtcNow.Date
                });


                foreach (var itemKey in genericKeys)
                {
                    string availableKey = (string)itemKey.GenericKey;
                    if (availableKey == "*") return availableKey;

                    if (availableKey.EndsWith("*"))
                    {
                        string matches = availableKey.Substring(0, genericKey.Length - 1);
                        if (genericKey.StartsWith(matches))
                        {
                            return availableKey;
                        }
                    }

                    if (availableKey.StartsWith("*"))
                    {
                        string matches = availableKey.Substring(1);
                        if (genericKey.EndsWith(matches))
                        {
                            return availableKey;
                        }
                    }

                    if (availableKey.Contains("*"))
                    {
                        var str = availableKey.Split("*");
                        if (str.Length == 2)
                        {
                            string startMatches = str[0];
                            string endMatches = str[1];

                            if (genericKey.StartsWith(startMatches) && genericKey.EndsWith(endMatches))
                            {
                                return availableKey;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error when get policy {ex.Message}", ex);
            }

            return "";
        }

    }
}
