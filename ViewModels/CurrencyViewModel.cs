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

    private ObservableCollection<CurrencyShort> _currencyRates;
    private ObservableCollection<CurrencyShort> CurrencyRates
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
        StartDate = DateTime.Today.AddDays(-7);
        EndDate = DateTime.Today;

        CurrencyRates = new ObservableCollection<CurrencyShort>();
        LoadFromAPICommand = new RelayCommand(async (a) => await LoadFromAPI());
    }

    public RelayCommand LoadFromAPICommand { get; }

    private async Task<IEnumerable<CurrencyShort>> LoadFromAPI()
    {
        if (StartDate > EndDate ||
            EndDate > DateTime.Today)
        {
            throw new Exception();  // todo
        }

        var ApiService = new ApiService();
        IEnumerable<Currency> currencies = await ApiService.LoadAllCurrencies();
        // todo
        List<Currency> currencies1 = currencies.OrderBy(x => x.Cur_ParentID).ToList();
        List<RateShort> Rates = null;
        foreach (var currency in currencies)
        {
            IEnumerable<RateShort> rateShort = await ApiService.LoadRateByPeriod(currency, StartDate, EndDate);
        }
        IEnumerable<CurrencyShort> currenciesShort = null;
        return currenciesShort;
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

    public void SaveToFile(string filePath)
    {
        string json = JsonConvert.SerializeObject(CurrencyRates, Formatting.Indented);
        File.WriteAllText(filePath, json);
    }

    public void LoadFromFile(string filePath)
    {
        string json = File.ReadAllText(filePath);
        var rates = JsonConvert.DeserializeObject<List<CurrencyShort>>(json);
        CurrencyRates = new ObservableCollection<CurrencyShort>(rates);
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}