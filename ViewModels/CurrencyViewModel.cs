using LiveCharts;
using LiveCharts.Wpf;
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
using System.Security.Cryptography;
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
        const string filepath = "C:\\Users\\kkaza\\OneDrive\\Рабочий стол\\nbrb_api_data.txt";  // todo

        private SeriesCollection _seriesCollection;
        public SeriesCollection SeriesCollection
        {
            get => _seriesCollection;
            set
            {
                _seriesCollection = value;
                OnPropertyChanged(nameof(SeriesCollection));
            }
        }

        private string[] _labels;
        public string[] Labels
        {
            get => _labels;
            set
            {
                _labels = value;
                OnPropertyChanged(nameof(Labels));
            }
        }
        public Func<double, string> YFormatter { get; set; }    // todo убрать

        public CurrencyViewModel()
        {
            StartDate = DateTime.Today.AddDays(-7); // начальные значения дат
            EndDate = DateTime.Today;

            SeriesCollection = new SeriesCollection();
            Labels = new string[] { };

            CurrencyRates = new ObservableCollection<Rate>();
            LoadFromAPICommand = new RelayCommand(async (a) => await LoadFromAPIAsync());
            SaveToFileCommand = new RelayCommand(async (a) => await SaveToFileAsync(false));
            LoadFromFileCommand = new RelayCommand(async (a) => await LoadFromFileAsync());
        }

        public RelayCommand LoadFromAPICommand { get; }
        public RelayCommand SaveToFileCommand { get; }
        public RelayCommand LoadFromFileCommand { get; }

        private async Task<IEnumerable<Rate>> LoadFromAPIAsync()
        {
            if (StartDate > EndDate ||
                EndDate > DateTime.Today)
            {
                MessageBox.Show("Choose correct date period (start date no later than end date & end date no later than today)");
                return null;
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

        public async Task SaveToFileAsync(bool isDataChanged)  // todo
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

        public async Task LoadFromFileAsync()
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

            await UpdateChartDataAsync(CurrencyRates);

            MessageBox.Show("Data loaded from file!");
        }

        public async Task UpdateChartDataAsync(IEnumerable<Rate> Rates)
        {
            // Очистите существующие серии данных
            await Application.Current.Dispatcher.InvokeAsync(() => SeriesCollection.Clear());

            // Группируем данные по валютам
            var groupedRates = Rates.GroupBy(r => r.Cur_Abbreviation);

            foreach (var group in groupedRates)
            {
                // Создаем новую серию для каждой валюты
                var series = new LineSeries
                {
                    Title = group.Key,
                    Values = new ChartValues<decimal>(),
                    Fill = System.Windows.Media.Brushes.Transparent
                };

                // Добавляем значения в серию
                foreach (var rate in group)
                {
                    series.Values.Add(rate.Cur_OfficialRate);
                }

                // Добавляем серию в коллекцию серий
                await Application.Current.Dispatcher.InvokeAsync(() => SeriesCollection.Add(series));
            }

            // Обновляем метки для оси X
            Labels = Rates.Select(r => r.Date.ToString("dd/MM/yyyy")).Distinct().ToArray();

            // Уведомляем интерфейс об изменениях
            OnPropertyChanged(nameof(SeriesCollection));
            OnPropertyChanged(nameof(Labels));
        }

        public async Task DataGrid_CellEditEnding()
        {
            await SaveToFileAsync(true);
            await UpdateChartDataAsync(CurrencyRates);
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