using Microsoft.Extensions.Configuration;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataProjectCsharp.Data
{
    public class AlphaVantageSecurityData
    {
        public DateTime Timestamp { get; set; }
        public decimal Close { get; set; }
    }

    public class AlphaVantageConnection
    {

        private readonly string _apiKey;

        public AlphaVantageConnection(IConfiguration configuration)
        {
            this._apiKey = configuration.GetValue<string>("ExternalAPIs:AlphaVantageAPI");
        }


        public List<AlphaVantageSecurityData> GetDailyPrices(string ticker)
        {
            const string function = "TIME_SERIES_DAILY";
            string connectionString = "https://" + $@"www.alphavantage.co/query?function={function}&symbol={ticker}&apikey={this._apiKey}&datatype=csv";
            List<AlphaVantageSecurityData> priceData = connectionString.GetStringFromUrl().FromCsv<List<AlphaVantageSecurityData>>();
            return priceData;
        }
    }
}
