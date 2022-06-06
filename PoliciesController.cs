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
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace SecurePolicyBasedDataAccess.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PoliciesController : ControllerBase
    {
        private readonly HttpClient _client;
        private readonly IShareSettings _settings;
        private readonly Poort8Utilities _poort8Utilities;

        public PoliciesController(IOptions<IShareSettings> settingsAccessor, IHttpClientFactory httpClientFactory)
        {
            _settings = settingsAccessor.Value;
            _client = httpClientFactory.CreateClient("iSHARE-Poort8");
            _client.BaseAddress = new Uri(_settings.Host);
            _poort8Utilities = new Poort8Utilities();
        }

        [HttpGet("Policies")]
        public async Task<IEnumerable<Poort8PolicyItem>> GetAsync([FromHeader] string token, [FromQuery] string myPartyId)
        {
            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("ContentType", "application/json");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            try
            {
                var response = await _client.GetAsync($"{_settings.LinkPolicy}/?issuer={myPartyId}");
                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var dataJson = JsonConvert.DeserializeObject<List<Poort8PolicyItem>>(jsonString);
                    return dataJson;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Get policies of {myPartyId} has error {ex.Message}", ex);
                throw new Exception($"When get policies has error {ex.Message}");
            }

            return new List<Poort8PolicyItem>();
        }

        [HttpPost("Policy")]
        public async Task<string> PostAsync([FromHeader] string token, [FromBody] Poort8PolicyModel model)
        {
            var policy = _poort8Utilities.CreatePolicy(model);
            string dataRaw = JsonConvert.SerializeObject(policy, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });

            try
            {
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var content = new StringContent(dataRaw, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await _client.PostAsync($"{_settings.LinkPolicy}", content);
                response.EnsureSuccessStatusCode();

                var data = await response.Content.ReadAsStringAsync();
                Log.Information($"Your policy has been created.");

                var service = new ManagePolicyService(_settings.ConnectionStrings);
                await service.SavePolicyAsync(model, policyId: data);
                return data;
            }
            catch (Exception ex)
            {
                Log.Error($"Create new policy has error {ex.Message}", ex);
                throw new Exception($"Your policy has error when created. Please try again later.");
            }
        }
    }
}
