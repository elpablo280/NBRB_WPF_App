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
using System.Windows;
using System.Windows.Controls;
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

        //private Currency _selectedCurrency;
        //public Currency SelectedCurrency
        //{
        //    get => _selectedCurrency;
        //    set
        //    {
        //        _selectedCurrency = value;
        //        OnPropertyChanged(nameof(SelectedCurrency));
        //    }
        //}

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

        private Rate _selectedRate;
        public Rate SelectedRate
        {
            get => _selectedRate;
            set
            {
                _selectedRate = value;
                OnPropertyChanged(nameof(SelectedRate));
            }
        }

        private IEnumerable<Rate> currencyRates;
        const string filepath = "C:\\Users\\kkaza\\OneDrive\\Рабочий стол\\nbrb_api_data.txt";

        public CurrencyViewModel()
        {
            StartDate = DateTime.Today.AddDays(-7); // начальные значения дат
            EndDate = DateTime.Today;

            CurrencyRates = new ObservableCollection<Rate>();
            LoadFromAPICommand = new RelayCommand(async (a) => await LoadFromAPI());
            SaveToFileCommand = new RelayCommand(async (a) => await SaveToFile(false));
            LoadFromFileCommand = new RelayCommand(async (a) => await LoadFromFile());
        }

        public RelayCommand LoadFromAPICommand { get; }
        public RelayCommand SaveToFileCommand { get; }
        public RelayCommand LoadFromFileCommand { get; }

        private async Task<IEnumerable<Rate>> LoadFromAPI()
        {
            if (StartDate > EndDate ||
                EndDate > DateTime.Today)
            {
                throw new Exception();  // todo (MessageBox.Show)
            }

            // todo описать в readme возможные способы решения задачи в плане оптимизации

            var ApiService = new ApiWorker();
            List<Rate> ratesByPeriod = new List<Rate>();
            for (DateTime date = StartDate; date <= EndDate; date = date.AddDays(1))
            {
                IEnumerable<Rate> ratesByDate = await ApiService.LoadRatesByDate(date);
                ratesByPeriod.AddRange(ratesByDate);
            }

            List<Rate> ratesByPeriod1 = ratesByPeriod.OrderBy(x => x.Cur_ID).ToList();

            currencyRates = ratesByPeriod1;

            MessageBox.Show("Data from API loaded!");

            // todo
            return ratesByPeriod1;
        }

        public async Task SaveToFile(bool isDataChanged)  // todo
        {
            string json = string.Empty;
            if (!isDataChanged)
            {
                json = JsonConvert.SerializeObject(currencyRates, Formatting.Indented);
            }
            else
            {
                json = JsonConvert.SerializeObject(CurrencyRates, Formatting.Indented);
            }
            using (StreamWriter writer = new StreamWriter(filepath, false))
            {
                await writer.WriteLineAsync(json);
            }

            MessageBox.Show("Data saved to file!");
        }

        public async Task LoadFromFile()
        {
            if (File.Exists(filepath))
            {
                string json = string.Empty;
                using (StreamReader reader = new StreamReader(filepath))    // todo решить проблему с доступом к потоку
                {
                    json = await reader.ReadToEndAsync();
                }
                var rates = JsonConvert.DeserializeObject<List<Rate>>(json);
                CurrencyRates = new ObservableCollection<Rate>(rates);
            }

            MessageBox.Show("Data loaded from file!");
        }

        public async Task DataGrid_CellEditEnding()
        {
            await SaveToFile(true);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
    }
}