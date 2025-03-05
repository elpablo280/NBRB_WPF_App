using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NBRB_WPF_App.Models
{
    class CurrencyShort
    {
        public CurrencyShort(DateTime date, string abbreviation, string name, decimal? officialRate)
        {
            Date = date;
            Abbreviation = abbreviation;
            Name = name;
            OfficialRate = officialRate;
        }

        public DateTime Date { get; set; }
        public string Abbreviation { get; set; }
        public string Name { get; set; }
        public decimal? OfficialRate { get; set; }
    }
}
