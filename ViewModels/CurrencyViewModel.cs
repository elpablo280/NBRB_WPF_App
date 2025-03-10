using LiveCharts;
using LiveCharts.Wpf;
using NBRB_WPF_App.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
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

        private IEnumerable<Rate> currencyRates;
        private readonly string filepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "nbrb_api_data.txt");

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

            var api = new ApiWorker();
            List<Rate> ratesByPeriod = new List<Rate>();
            for (DateTime date = StartDate; date <= EndDate; date = date.AddDays(1))
            {
                IEnumerable<Rate> ratesByDate = await api.LoadRatesByDate(date);
                ratesByPeriod.AddRange(ratesByDate);
            }

            List<Rate> ratesByPeriod1 = ratesByPeriod.OrderBy(x => x.Cur_ID).ToList();

            currencyRates = ratesByPeriod1;

            MessageBox.Show("Data from API loaded!");

            return ratesByPeriod1;
        }

        public async Task SaveToFileAsync(bool isDataChanged)
        {
            string json;
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
                using (StreamReader reader = new StreamReader(filepath))
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
            await Application.Current.Dispatcher.InvokeAsync(() => SeriesCollection.Clear());
            var groupedRates = Rates.GroupBy(r => r.Cur_Abbreviation);
            foreach (var group in groupedRates)
            {
                var series = new LineSeries
                {
                    Title = group.Key,
                    Values = new ChartValues<decimal>(),
                    Fill = System.Windows.Media.Brushes.Transparent
                };

                foreach (var rate in group)
                {
                    series.Values.Add(rate.Cur_OfficialRate);
                }

                await Application.Current.Dispatcher.InvokeAsync(() => SeriesCollection.Add(series));
            }
            Labels = Rates.Select(r => r.Date.ToString("dd/MM/yyyy")).Distinct().ToArray();

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