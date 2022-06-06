using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SecurePolicyBasedDataAccess.Infrastructure.Extensions;
using SecurePolicyBasedDataAccess.Infrastructure.Services;
using SecurePolicyBasedDataAccess.Infrastructure.Settings;
using SecurePolicyBasedDataAccess.Models;
using Serilog;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace SecurePolicyBasedDataAccess.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DelegationEvidenceController : ControllerBase
    {
        private readonly HttpClient _client;
        private readonly IShareSettings _settings;
        private readonly Poort8Utilities _poort8Utilities;

        public DelegationEvidenceController(IOptions<IShareSettings> settingsAccessor, IHttpClientFactory httpClientFactory)
        {
            _settings = settingsAccessor.Value;
            _client = httpClientFactory.CreateClient("iSHARE-Poort8");
            _client.BaseAddress = new Uri(_settings.Host);
            _poort8Utilities = new Poort8Utilities();
        }

        [HttpGet("MyPolicies")]
        public async Task<string> GetPolicyAsync([FromHeader] string token, [FromQuery] Poort8VerifyDataModel model, [FromQuery] bool showToken = false)
        {
            var service = new ManagePolicyService(_settings.ConnectionStrings);
            string genericKey = await service.GetDataInfoAsync(model.GenericKey, model.GenericType, model.Issuer, model.Actor);

            if (string.IsNullOrEmpty(genericKey))
            {
                Log.Error($"GetDataInfoAsync is not found genericKey");
                genericKey = model.GenericKey;
            }

            var delegation = _poort8Utilities.CreateDelegationEvidence(genericKey, model.GenericType, model.Issuer, model.Actor);
            string dataRaw = JsonConvert.SerializeObject(delegation, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Formatting = Formatting.Indented
            });

            try
            {
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var content = new StringContent(dataRaw, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await _client.PostAsync($"{_settings.LinkDelegationEvidence}", content);
                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var dataJson = JsonConvert.DeserializeObject<Poort8DelegationModel>(jsonString);
                    return GetDelegationToken(dataJson, showToken);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Get policy of {model.Issuer} has error {ex.Message}", ex);
                throw new Exception($"When get policy has error {ex.Message}");
            }

            return showToken ? "Not Found" : "Deny";
        }

        private string GetDelegationToken(Poort8DelegationModel dataJson, bool showToken)
        {
            var data = _poort8Utilities.ParseDelegationToken(dataJson.delegation_token);
            var isDeny = true;
            if (data.PolicySets.Any())
            {
                long currentTime = DateTime.UtcNow.ToEpochNumber();
                if (currentTime >= data.NotBefore && currentTime <= data.NotOnOrAfter)
                {
                    isDeny = data.PolicySets.Any(x => x.Policies.Any(p => p.Rules[0].Effect == "Deny"));
                }
            }

            if (showToken)
            {
                return dataJson.delegation_token;
            }

            return isDeny ? "Deny" : "Permit";
        }
    }

}
