using NBRB_WPF_App;
using NBRB_WPF_App.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NBRB_WPF_App.ViewModels
{
    public class CurrencyViewModel : INotifyPropertyChanged
    {
        private DateTime _startDate;
        public DateTime StartDate
        {
            get => _startDate;
            set
            {
                _startDate = value;
                OnPropertyChanged(nameof(StartDate));
            }
        }

        private DateTime _endDate;
        public DateTime EndDate
        {
            get => _endDate;
            set
            {
                _endDate = value;
                OnPropertyChanged(nameof(EndDate));
            }
        }

        private Currency _selectedCurrency;
        public Currency SelectedCurrency
        {
            get => _selectedCurrency;
            set
            {
                _selectedCurrency = value;
                OnPropertyChanged(nameof(SelectedCurrency));
            }
        }

        private ObservableCollection<Rate> _currencyRates;
        public ObservableCollection<Rate> CurrencyRates
        {
            get => _currencyRates;
            set
            {
                _currencyRates = value;
                OnPropertyChanged(nameof(CurrencyRates));
            }
        }

        public CurrencyViewModel()
        {
            StartDate = DateTime.Today.AddDays(-7); // начальные значения дат
            EndDate = DateTime.Today;

            CurrencyRates = new ObservableCollection<Rate>();
            LoadFromAPICommand = new RelayCommand(async (a) => await LoadFromAPI());
            SaveCommand = new RelayCommand(async (a) => await SaveToFile());
        }

        public RelayCommand LoadFromAPICommand { get; }
        public RelayCommand SaveCommand { get; }

        private async Task<IEnumerable<Rate>> LoadFromAPI()
        {
            if (StartDate > EndDate ||
                EndDate > DateTime.Today)
            {
                throw new Exception();  // todo
            }

            // todo описать в readme возможные способы решения задачи в плане оптимизации

            var ApiService = new ApiWorker();
            //IEnumerable<Currency> currencies = await ApiService.LoadAllCurrencies();    // в любом случае загружаем весь список валют
            //List<Currency> currencies1 = currencies.OrderBy(x => x.Cur_DateStart).ToList();
            //List<RateShort> allCurrenciesRatesShort = new List<RateShort>();
            //List<CurrencyShort> currenciesShort = new List<CurrencyShort>();
            //foreach (var currency in currencies)
            //{
            //    IEnumerable<RateShort> cyrrencyRatesShort = await ApiService.LoadRateByPeriod(currency, StartDate, EndDate);
            //    allCurrenciesRatesShort.AddRange(cyrrencyRatesShort);
            //}
            //foreach (var currencyRateShort in allCurrenciesRatesShort)
            //{
            //    Currency currency = currencies.Where(x => x.Cur_ID == currencyRateShort.Cur_ID).First();
            //    CurrencyShort currencyShort = new CurrencyShort(
            //        currencyRateShort.Date,
            //        currency.Cur_Abbreviation,
            //        currency.Cur_Name,
            //        currencyRateShort.Cur_OfficialRate);
            //    currenciesShort.Add(currencyShort);
            //}
            List<Rate> ratesByPeriod = new List<Rate>();
            for (DateTime date = StartDate; date <= EndDate; date = date.AddDays(1))
            {
                IEnumerable<Rate> ratesByDate = await ApiService.LoadRatesByDate(date);
                ratesByPeriod.AddRange(ratesByDate);
            }

            List<Rate> ratesByPeriod1 = ratesByPeriod.OrderBy(x => x.Cur_ID).ToList();

            foreach (var item in ratesByPeriod1)
            {
                CurrencyRates.Add(item);
            }

            // todo
            return ratesByPeriod;
        }

        public class RelayCommand : ICommand
        {
            private readonly Action<object> _execute;
            private readonly Func<object, bool> _canExecute;

            public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
            {
                _execute = execute;
                _canExecute = canExecute;
            }

            public bool CanExecute(object parameter)
            {
                return _canExecute == null || _canExecute(parameter);
            }

            public void Execute(object parameter)
            {
                _execute(parameter);
            }

            public event EventHandler CanExecuteChanged
            {
                add { CommandManager.RequerySuggested += value; }
                remove { CommandManager.RequerySuggested -= value; }
            }
        }

        public async Task SaveToFile()  // todo
        {
            string filepath = "C:\\Users\\kkaza\\OneDrive\\Рабочий стол\\nbrb_api_data.txt";
            string json = JsonConvert.SerializeObject(CurrencyRates, Formatting.Indented);
            using (StreamWriter writer = new StreamWriter(filepath, false))
            {
                await writer.WriteLineAsync(json);
            }
        }

        //public void LoadFromFile(string filePath)
        //{
        //    string json = File.ReadAllText(filePath);
        //    var rates = JsonConvert.DeserializeObject<List<CurrencyShort>>(json);
        //    CurrencyRates = new ObservableCollection<CurrencyShort>(rates);
        //}

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}