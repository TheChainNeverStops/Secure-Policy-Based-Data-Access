
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SecurePolicyBasedDataAccess.Infrastructure.Services;
using SecurePolicyBasedDataAccess.Infrastructure.Settings;
using Serilog;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace SecurePolicyBasedDataAccess.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccessTokensController : ControllerBase
    {
        private readonly IShareSettings _settings;
        private readonly HttpClient _client;

        public AccessTokensController(IOptions<IShareSettings> settingsAccessor, IHttpClientFactory httpClientFactory)
        {
            _settings = settingsAccessor.Value;
            _client = httpClientFactory.CreateClient("iSHARE-Poort8");
        }

        [HttpPost("myIdentifier")]
        public async Task<string> GetToken(IFormFile isharePublicKey, IFormFile isharePrivateKey, string myIdentifier)
        {
            var tokenService = new Poort8TokenService(_client);
            try
            {
                string audParty = _settings.TargetAudience;
                string issParty = myIdentifier;
                string privateKey = await tokenService.ReadFormFileAsync(isharePrivateKey);
                string publicKey = await tokenService.ReadFormFileAsync(isharePublicKey);
                string urlSchemas = _settings.UrlSchemeAuthorize;
                string urlGetToken = _settings.UrlPoort8GetToken;

                publicKey = publicKey.Replace("-----BEGIN CERTIFICATE-----", "");
                publicKey = publicKey.Replace("-----END CERTIFICATE-----", "");
                publicKey = publicKey.Replace("\r\n", "");
                publicKey = publicKey.Replace(" ", "");

                string clientAssertion = await tokenService.GetTokenFromSchemaAsync(audParty,
                    issParty, privateKey, publicKey, urlSchemas);

                string token = await tokenService.GetTokenAudienceAsync(urlGetToken, issParty, clientAssertion.Replace("\"", ""), _settings.Host);
                return token;
            }
            catch (Exception ex)
            {
                Log.Error($"Get token from schema has error {ex.Message}", ex);
                throw new Exception($"Get token from schema has error. Please try again later.");
            }
        }
    }
}
