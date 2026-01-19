using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExchangeRateUpdater.Application.DTOs
{
    public class ExchangeRateDTO
    {
        public DateTime ValidFor { get; set; }
        public int Order { get; set; }
        public string Country { get; set; }
        public string Currency { get; set; }
        public int Amount { get; set; }
        public string CurrencyCode { get; set; }
        public decimal Rate { get; set; }
    }
}
