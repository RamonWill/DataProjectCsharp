using DataProjectCsharp.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Xunit;

namespace DataProjectCsharp.Tests.DataObjectsTesting
{

    public class AlphaVantageTests
    {
        private static IConfiguration InitConfiguration()
        {
            IConfiguration config = new ConfigurationBuilder()
                    .AddJsonFile("appsettingsTest.json", optional: false, reloadOnChange: true)
                    .AddEnvironmentVariables()
                    .Build();
            return config;
        }

        private readonly string testSymbol = "MSFT";
        private readonly IConfiguration config = InitConfiguration();
        private readonly AlphaVantageConnection avConnection;
        
        public AlphaVantageTests()
        {
            this.avConnection = new AlphaVantageConnection(this.config);
        }

        [Fact]
        public void TestConnection()
        {
            List<AlphaVantageSecurityData> data = this.avConnection.GetDailyPrices(testSymbol);
            Assert.Equal(100, data.Count);
        }

    }
}
