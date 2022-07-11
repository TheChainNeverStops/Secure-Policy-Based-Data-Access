    [Route("api/[controller]")]
    [ApiController]
    public class PoliciesController : ControllerBase
    {
        private readonly HttpClient _client;
        private readonly IShareSettings _settings;
        private readonly ThirdPartyUtilities _utilities;

        public PoliciesController(IOptions<IShareSettings> settingsAccessor, IHttpClientFactory httpClientFactory)
        {
            _settings = settingsAccessor.Value;
            _client = httpClientFactory.CreateClient("iSHARE");
            _client.BaseAddress = new Uri(_settings.Host);
            _utilities = new ThirdPartyUtilities();
        }

        [HttpGet]
        public async Task<IEnumerable<PolicyItem>> GetAsync([FromHeader] string token, [FromQuery] string myPartyId)
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
                    return JsonConvert.DeserializeObject<List<PolicyItem>>(jsonString);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Get policies of {myPartyId} has error {ex.Message}", ex);
                throw new Exception($"When get policies has error. Please try again later.");
            }

            return new List<PolicyItem>();
        }

        [HttpPost("Policy")]
        public async Task<string> PostAsync([FromHeader] string token, [FromBody] PolicyModel model)
        {
            var policy = _utilities.CreatePolicy(model);
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
