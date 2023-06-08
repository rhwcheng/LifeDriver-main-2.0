using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;

namespace StyexFleetManagement.Salus.Services
{
    public class BaseSalusService
    {
        private readonly HttpClient _client;

        private const string BaseUrl = "https://vygilance-platform.com/api/Avatar_api";

        public BaseSalusService()
        {
            _client = new HttpClient
            {
                BaseAddress = new Uri(BaseUrl, UriKind.Absolute)
            };
        }

        protected async Task<T> PostAsync<T>(string requestUri, object request)
        {
            try
            {
                var json = JsonConvert.SerializeObject(request);
                var dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

                var httpContent = new FormUrlEncodedContent(dictionary);

                var response = await _client.PostAsync($"{BaseUrl}/{requestUri}", httpContent);

                //TODO: Handle
                if (!response.IsSuccessStatusCode) return default;

                var responseString = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(responseString);
            }
            catch (Exception exception)
            {
                Serilog.Log.Error(exception, exception.Message);
                throw;
            }
        }

        protected async Task<T> GetAsync<T>(string requestUri, object request)
        {
            try
            {
                var json = JsonConvert.SerializeObject(request);
                var dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

                var url =
                    $"{BaseUrl}/{requestUri}?{string.Join("&", dictionary.Select(kvp => $"{kvp.Key}={kvp.Value}"))}";

                var response = await _client.GetAsync(url);

                //TODO: Handle
                if (!response.IsSuccessStatusCode) return default;

                var responseString = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(responseString);
            }
            catch (Exception exception)
            {
                Serilog.Log.Error(exception, exception.Message);
                throw;
            }
        }
    }
}