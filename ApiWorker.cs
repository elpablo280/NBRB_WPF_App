using NBRB_WPF_App.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace NBRB_WPF_App
{
    class ApiWorker
    {
        private readonly HttpClient _httpClient;
        private const string _baseUrl = "https://api.nbrb.by/exrates/";

        public ApiWorker()
        {
            _httpClient = new HttpClient { BaseAddress = new Uri(_baseUrl) };
        }

        public async Task<IEnumerable<Currency>> LoadAllCurrencies()
        {
            string request = $"{_baseUrl}Currencies";
            string response = await _httpClient.GetStringAsync(request);
            var currencies = JsonConvert.DeserializeObject<IEnumerable<Currency>>(response);
            return currencies;
        }

        public async Task<IEnumerable<RateShort>> LoadRateByPeriod(Currency currency, DateTime startDate, DateTime endDate)
        {
            string request = $"{_baseUrl}Rates/Dynamics/{currency.Cur_Code}?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}";
            string response = await _httpClient.GetStringAsync(request);
            var rates = JsonConvert.DeserializeObject<IEnumerable<RateShort>>(response);
            return rates;
        }

        public async Task<IEnumerable<Rate>> LoadRatesByDate(DateTime date)
        {
            string request = $"{_baseUrl}Rates?ondate={date:yyyy-MM-dd}&periodicity=0";
            string response = await _httpClient.GetStringAsync(request);
            var rates = JsonConvert.DeserializeObject<IEnumerable<Rate>>(response);
            return rates;
        }
    }
}
