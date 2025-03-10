﻿using System.ComponentModel.DataAnnotations;

namespace NBRB_WPF_App.Models
{
    public class RateShort
    {
        public int Cur_ID { get; set; }
        [Key]
        public System.DateTime Date { get; set; }
        public decimal? Cur_OfficialRate { get; set; }
    }
}
