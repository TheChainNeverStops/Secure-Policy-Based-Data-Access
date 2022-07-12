    public class ThirdPartyTokenService
    {
        private readonly HttpClient _client;
        public TokenService(HttpClient client)
        {
            _client = client;
        }

        public async Task<string> ReadFormFileAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return await Task.FromResult((string)null);
            }

            var reader = new StreamReader(file.OpenReadStream());
            return await reader.ReadToEndAsync();
        }

        public async Task<string> GetTokenFromSchemaAsync(string audParty, string issParty,
            string isharePrivateKey, string isharePublicKey, string urlSchemas)
        {
            byte[] byteArray = Encoding.UTF8.GetBytes(isharePrivateKey);

            _client.BaseAddress = new Uri(urlSchemas);
            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));
            _client.DefaultRequestHeaders.Add("access-control-allow-origin", "*");
            _client.DefaultRequestHeaders.Add("alg", "RS256");
            _client.DefaultRequestHeaders.Add("typ", "JWT");
            _client.DefaultRequestHeaders.Add("aud", audParty);
            _client.DefaultRequestHeaders.Add("iss", issParty);
            _client.DefaultRequestHeaders.Add("x5c", isharePublicKey);

            ByteArrayContent bodyContent = new ByteArrayContent(byteArray);
            HttpResponseMessage response = await _client.PostAsync($"{urlSchemas}", bodyContent);
            response.EnsureSuccessStatusCode();
            var data = await response.Content.ReadAsStringAsync();
            return data;
        }

        public async Task<string> GetTokenAudienceAsync(string urlGetToken, string issParty, string clientAssertion, string host)
        {
            var postData = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type","client_credentials"),
                new KeyValuePair<string, string>("scope", "iSHARE"),
                new KeyValuePair<string, string>("client_id", issParty),
                new KeyValuePair<string, string>("client_assertion_type", "urn:ietf:params:oauth:client-assertion-type:jwt-bearer"),
                new KeyValuePair<string, string>("client_assertion", clientAssertion),
            };

            using HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var content = new FormUrlEncodedContent(postData);

            content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
            HttpResponseMessage response = await client.PostAsync($"{host}/{urlGetToken}", content);
            var token = await response.Content.ReadAsStringAsync();
            return token;
        }
        
        public PlaybookPolicy CreatePolicy(PolicyModel model)
        {
            string resourceType = GetResourceTypeByModel(model);
            return new Policy
            {
                Email = $"{model.Email}",
                Organization = $"{model.MyPartyId}",
                Username = $"{model.UserName}",
                Policy = new Policy(resourceType)
                {
                    Actor = $"{model.ActorPartyId}",
                    Issuer = $"{model.MyPartyId}",
                    Note = $"{model.Note}",
                    ContextRule = new ContextRule
                    {
                        ResourceIdentifier = $"{model.ResourceIdentifier}",
                        ServiceProvider = $"{model.ServiceProviders}",
                        NotBefore = int.Parse(DateTime.UtcNow.AddMinutes(1).ToEpoch()),
                        NotOnOrAfter = int.Parse(model.ToDate.AddDays(1).AddSeconds(-1).ToEpoch()),
                    }
                }
            };
        }
    }
