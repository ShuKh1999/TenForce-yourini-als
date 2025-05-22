using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Test_Taste_Console_Application.Constants;
using Test_Taste_Console_Application.Domain.DataTransferObjects;
using Test_Taste_Console_Application.Domain.DataTransferObjects.JsonObjects;
using Test_Taste_Console_Application.Domain.Objects;
using Test_Taste_Console_Application.Domain.Services.Interfaces;
using Test_Taste_Console_Application.Utilities;

namespace Test_Taste_Console_Application.Domain.Services
{
    public class BodyService : IGetBodiesAsync
    {
        private readonly HttpClientService _httpClientService;

        public BodyService(HttpClientService httpClientService)
        {
            _httpClientService = httpClientService;
           
        }

        public async Task<List<Body>> GetBodiesAsync()
        {
            var response = await _httpClientService.Client.GetAsync(UriPath.GetBodyForPlanetInfo);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<BodyResponse>(json);

            return data?.Bodies ?? new List<Body>();
        }
    }
}
