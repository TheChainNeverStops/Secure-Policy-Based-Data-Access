[Route("api/[controller]")]
    [ApiController]
    public class DelegationEvidenceController : ControllerBase
    {
        private readonly HttpClient _client;
        private readonly IShareSettings _settings;
        private readonly ThirdPartyUtilities _utilities;
        private readonly IManagePolicyService _service;

        public DelegationEvidenceController(IOptions<IShareSettings> settingsAccessor,
            IManagePolicyService service, IHttpClientFactory httpClientFactory)
        {
            _settings = settingsAccessor.Value;
            _client = httpClientFactory.CreateClient("iSHARE-Poort8");
            _client.BaseAddress = new Uri(_settings.Host);
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _utilities = new ThirdPartyUtilities();
        }

        [HttpGet("MyPolicies")]
        public async Task<string> GetPolicyAsync([FromHeader] string token, [FromQuery] VerifyDataModel model, [FromQuery] bool showToken = false)
        {            
            string genericKey = await _service.GetDataInfoAsync(model.GenericKey, model.GenericType, model.Issuer, model.Actor);
            if (string.IsNullOrEmpty(genericKey))
            {
                Log.Error($"GetDataInfo is not found generic key");
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
                    var dataJson = JsonConvert.DeserializeObject<TokenDelegationModel>(jsonString);
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

        private string GetDelegationToken(TokenDelegationModel dataJson, bool showToken)
        {
            if (showToken)
            {
                return dataJson.delegation_token;
            }
            
            var data = _utilities.ParseDelegationToken(dataJson.delegation_token);
            var isDeny = true;
            if (data.PolicySets.Any())
            {
                long currentTime = DateTime.UtcNow.ToEpochNumber();
                if (currentTime >= data.NotBefore && currentTime <= data.NotOnOrAfter)
                {
                    isDeny = data.PolicySets.Any(x => x.Policies.Any(p => p.Rules[0].Effect == "Deny"));
                }
            }

            return isDeny ? "Deny" : "Permit";
        }
    }
