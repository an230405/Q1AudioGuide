using System.Net.Http.Json;
using TourGuideAdmin.Models;

namespace TourGuideAdmin.Services
{
    public class ApiService
    {
        private readonly HttpClient _http;

        public ApiService()
        {
            _http = new HttpClient();
            _http.BaseAddress = new Uri("https://gwsmx4vm-7182.asse.devtunnels.ms/swagger/index.html"); // API của bạn
        }

        public async Task<List<POI>> GetPOIs()
        {
            return await _http.GetFromJsonAsync<List<POI>>("api/poi");
        }

        public async Task CreatePOI(POI poi)
        {
            await _http.PostAsJsonAsync("api/poi", poi);
        }

        public async Task DeletePOI(int id)
        {
            await _http.DeleteAsync($"api/poi/{id}");
        }
    }
}