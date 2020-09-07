using Analogy.Interfaces;
using Analogy.Interfaces.Factories;
using System;
using System.Collections.Generic;
using System.Drawing;
using Analogy.LogViewer.MiradaXD.Properties;

namespace Analogy.LogViewer.MiradaXD
{
    public class MiradaFactory : IAnalogyFactory
    {
        public Guid FactoryId { get; set; } = new Guid("C5EEF9AC-3E5A-4D84-9AE1-0D1BB5633EDD");
        public string Title { get; set; } = "Mirada Logs";
        public Image SmallImage { get; set; } = null;
        public Image LargeImage { get; set; } = null;
        public IEnumerable<IAnalogyChangeLog> ChangeLog { get; set; } = new List<IAnalogyChangeLog>
        {
            {
                new AnalogyChangeLog("Initial Version", AnalogChangeLogType.Improvement, "Lior Banai",
                    new DateTime(2019, 12, 04))
            }
        };
        public IEnumerable<string> Contributors { get; set; } = new List<string> { "Lior Banai" };
        public string About { get; set; } = "Created by Lior Banai";
    }

    public class MiradaDataSourceFactory : IAnalogyDataProvidersFactory
    {
        public Guid FactoryId { get; set; } = new Guid("C5EEF9AC-3E5A-4D84-9AE1-0D1BB5633EDD");
        public string Title { get; set; } = "Mirada Data Sources";
        public IEnumerable<IAnalogyDataProvider> DataProviders { get; }

        public MiradaDataSourceFactory()
        {
            DataProviders = new List<IAnalogyDataProvider>() { new OfflineMiradaLogs() };
        }
    }
}
