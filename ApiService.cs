using NBRB_WPF_App.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Threading.Tasks;

namespace NBRB_WPF_App
{
    class ApiService
    {
        private readonly HttpClient _httpClient;
        private const string _baseUrl = "https://api.nbrb.by/exrates/";

        public ApiService()
        {
            _httpClient = new HttpClient { BaseAddress = new Uri(_baseUrl) };
        }

        //public async Task<IEnumerable<Currency>> GetCurrenciesAsync(DateTime startDate, DateTime endDate)
        //{
        //    string response = string.Empty;
        //    string queue = $"{BaseUrl}Rates/Dynamics/{startDate:yyyy-MM-dd}/{endDate:yyyy-MM-dd}";
        //    try
        //    {
        //        response = await client.GetStringAsync($"{BaseUrl}currencies");
        //        //response = await client.GetStringAsync($"{BaseUrl}Rates/Dynamics/{startDate:yyyy-MM-dd}/{endDate:yyyy-MM-dd}");
        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //    return JsonConvert.DeserializeObject<Currency[]>(response);
        //}

        public async Task<IEnumerable<Currency>> LoadAllCurrencies()
        {
            string response = await _httpClient.GetStringAsync($"{_baseUrl}Currencies");
            var currencies = JsonConvert.DeserializeObject<IEnumerable<Currency>>(response);
            return currencies;
        }

        public async Task<IEnumerable<RateShort>> LoadRateByPeriod(Currency currency, DateTime startDate, DateTime endDate)
        {
            string request = $"{_baseUrl}Rates/Dynamics/{currency.Cur_Code}?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}";
            string response = await _httpClient.GetStringAsync($"{_baseUrl}Rates/Dynamics/{currency.Cur_Code}?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}");
            var rate = JsonConvert.DeserializeObject<IEnumerable<RateShort>>(response);
            return rate;
        }
    }
}
