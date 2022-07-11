    [Route("api/[controller]")]
    [ApiController]
    public class AccessTokensController : ControllerBase
    {
        private readonly IShareSettings _settings;
        private readonly HttpClient _client;

        public AccessTokensController(IOptions<IShareSettings> settingsAccessor, IHttpClientFactory httpClientFactory)
        {
            _settings = settingsAccessor.Value;
            _client = httpClientFactory.CreateClient("iSHARE");
        }

        [HttpPost("{myIshareId}")]
        public async Task<string> GetToken(IFormFile isharePublicKey, IFormFile isharePrivateKey, string myIshareId)
        {            
            try
            {                
                string privateKey = await tokenService.ReadFormFileAsync(isharePrivateKey);
                string publicKey = await tokenService.ReadFormFileAsync(isharePublicKey);                 
                string issParty = myIshareId;
                
                if (string.IsNullOrEmpty(privateKey))
                {
                    return Ok(new
                    {
                        isError = true,
                        ErrorMsg = $"Private key is not correct, please try again later."
                    });
                }

                if (string.IsNullOrEmpty(publicKey))
                {
                    return Ok(new
                    {
                        isError = true,
                        ErrorMsg = $"Public key is not correct, please try again later."
                    });
                }
                
                //Verify iSHARE ID with Certificate
                bool isValid = VerifyCertificateWithIshareId(privateKey, publicKey, myIshareId);
                if(!isValid)
                {
                    return Ok(new
                    {
                        isError = true,
                        ErrorMsg = $"PublicKey is not correct with your Identifier, please try again later."
                    });
                }
                
                string audParty = _settings.TargetAudience;
                string urlSchemas = _settings.UrlSchemas;
                string urlGetToken = _settings.UrlGetToken;
                string clientAssertion = await tokenService.GetTokenFromSchemaAsync(audParty, issParty, privateKey, publicKey, urlSchemas);                    
                var tokenService = new ThirdPartyTokenService(_client);
                return await tokenService.GetTokenAudienceAsync(urlGetToken, issParty, clientAssertion.Replace("\"", ""), _settings.Host);
            }
            catch (Exception ex)
            {
                Log.Error($"Get token from schema has error {ex.Message}", ex);
                throw new Exception($"Get token from schema has error. Please try again later.");
            }
        }

        private bool VerifyCertificateWithIshareId(privateKey, publicKey, myIshareId)
        {
            publicKey = publicKey.Replace("-----BEGIN CERTIFICATE-----", "");
            publicKey = publicKey.Replace("-----END CERTIFICATE-----", "");
            publicKey = publicKey.Replace("\r\n", "");
            publicKey = publicKey.Replace(" ", "");

            var signingCertificate = new X509Certificate2(Convert.FromBase64String(publicKey));
            string subject = signingCertificate.Subject;
            if (string.IsNullOrEmpty(subject) || !subject.Contains(myIshareId))
            {
                return false;
            }
            
            return true;
        }
    }
