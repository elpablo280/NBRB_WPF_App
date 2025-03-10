using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace NBRB_WPF_App.Models
{
    public class Rate : INotifyPropertyChanged
    {
        private int _cur_ID;
        [Key]
        public int Cur_ID
        {
            get => _cur_ID;
            set
            {
                _cur_ID = value;
                OnPropertyChanged(nameof(Cur_ID));
            }
        }

        private DateTime _date;
        public DateTime Date
        {
            get => _date;
            set
            {
                _date = value;
                OnPropertyChanged(nameof(Date));
            }
        }

        private string _cur_abbreviation;
        public string Cur_Abbreviation
        {
            get => _cur_abbreviation;
            set
            {
                _cur_abbreviation = value;
                OnPropertyChanged(nameof(Cur_Abbreviation));
            }
        }

        private int _cur_Scale;
        public int Cur_Scale
        {
            get => _cur_Scale;
            set
            {
                _cur_Scale = value;
                OnPropertyChanged(nameof(Cur_Scale));
            }
        }

        private string _cur_name;
        public string Cur_Name
        {
            get => _cur_name;
            set
            {
                _cur_name = value;
                OnPropertyChanged(nameof(Cur_Name));
            }
        }

        private decimal? _cur_officialRate;
        public decimal? Cur_OfficialRate
        {
            get => _cur_officialRate;
            set
            {
                _cur_officialRate = value;
                OnPropertyChanged(nameof(Cur_OfficialRate));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
