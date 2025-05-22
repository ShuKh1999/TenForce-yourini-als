using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
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
    public class PlanetWithMoonsTempDto
    {
        public string PlanetName { get; set; }
        public int MoonCount { get; set; }
        public double? AverageMoonTemperature { get; set; }
    }

    /// <inheritdoc />
    public class PlanetService : IPlanetService
    {
        private readonly HttpClientService _httpClientService;
        private readonly BodyService _bodyService;

        public PlanetService(HttpClientService httpClientService)
        {
            _httpClientService = httpClientService;
        }

        private PlanetService(BodyService bodyService)
        {
            this._bodyService = bodyService;
        }

        public IEnumerable<Planet> GetAllPlanets()
        {
            var allPlanetsWithTheirMoons = new Collection<Planet>();

            var response = _httpClientService.Client
                .GetAsync(UriPath.GetAllPlanetsWithMoonsQueryParameters)
                .Result;

            //If the status code isn't 200-299, then the function returns an empty collection.
            if (!response.IsSuccessStatusCode)
            {
                Logger.Instance.Warn($"{LoggerMessage.GetRequestFailed}{response.StatusCode}");
                return allPlanetsWithTheirMoons;
            }

            var content = response.Content.ReadAsStringAsync().Result;

            //The JSON converter uses DTO's, that can be found in the DataTransferObjects folder, to deserialize the response content.
            var results = JsonConvert.DeserializeObject<JsonResult<PlanetDto>>(content);

            //The JSON converter can return a null object. 
            if (results == null) return allPlanetsWithTheirMoons;

            //If the planet doesn't have any moons, then it isn't added to the collection.
            foreach (var planet in results.Bodies)
            {
                if (planet.Moons != null)
                {
                    var newMoonsCollection = new Collection<MoonDto>();
                    foreach (var moon in planet.Moons)
                    {
                        var moonResponse = _httpClientService.Client
                            .GetAsync(UriPath.GetMoonByIdQueryParameters + moon.URLId)
                            .Result;
                        var moonContent = moonResponse.Content.ReadAsStringAsync().Result;
                        newMoonsCollection.Add(JsonConvert.DeserializeObject<MoonDto>(moonContent));
                    }
                    planet.Moons = newMoonsCollection;

                }
                allPlanetsWithTheirMoons.Add(new Planet(planet));
            }

            return allPlanetsWithTheirMoons;
        }

        private static string RemoveDiacritics(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder(capacity: normalizedString.Length);

            for (int i = 0; i < normalizedString.Length; i++)
            {
                char c = normalizedString[i];
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder
                .ToString()
                .Normalize(NormalizationForm.FormC);
        }

        public async Task<List<PlanetWithMoonsTempDto>> GetPlanetsWithMoonTempAsync()
        {
            var bodies = await _bodyService.GetBodiesAsync();

            var planetsWithMoons = bodies
                .Where(b => b.IsPlanet && b.Moons != null && b.Moons.Any())
                .ToList();

            var moons = bodies
                .Where(b => b.AroundPlanet != null)
                .ToList();

            var results = new List<PlanetWithMoonsTempDto>();

            foreach (var planet in planetsWithMoons)
            {
                var planetMoons = moons
                    .Where(m => m.AroundPlanet.Planet == planet.Id)
                    .ToList();

                double? avgTemp = null;

                if (planetMoons.Any(m => m.AvgTemp.HasValue))
                {
                    avgTemp = planetMoons
                        .Where(m => m.AvgTemp.HasValue)
                        .Select(m => m.AvgTemp.Value)
                        .Average();
                }

                results.Add(new PlanetWithMoonsTempDto
                {
                    PlanetName = planet.EnglishName,
                    MoonCount = planetMoons.Count,
                    AverageMoonTemperature = avgTemp
                });
            }

            return results;
        }
    }
}
