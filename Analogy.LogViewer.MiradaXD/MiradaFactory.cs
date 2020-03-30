using Analogy.Interfaces;
using Analogy.Interfaces.Factories;
using System;
using System.Collections.Generic;

namespace Analogy.LogViewer.MiradaXD
{
    public class MiradaFactory : IAnalogyFactory
    {
        public Guid FactoryId { get; } = new Guid("C5EEF9AC-3E5A-4D84-9AE1-0D1BB5633EDD");
        public string Title { get; } = "Mirada Logs";

        public IEnumerable<IAnalogyChangeLog> ChangeLog { get; } = new List<IAnalogyChangeLog>
        {
            {
                new AnalogyChangeLog("Initial Version", AnalogChangeLogType.Improvement, "Lior Banai",
                    new DateTime(2019, 12, 04))
            }
        };
        public IEnumerable<string> Contributors { get; } = new List<string> { "Lior Banai" };
        public string About { get; } = "Created by Lior Banai";
    }

    public class MiradaDataSourceFactory : IAnalogyDataProvidersFactory
    {
        public Guid FactoryId { get; } = new Guid("C5EEF9AC-3E5A-4D84-9AE1-0D1BB5633EDD");
        public string Title { get; } = "Mirada Data Sources";
        public IEnumerable<IAnalogyDataProvider> DataProviders { get; }

        public MiradaDataSourceFactory()
        {
            DataProviders = new List<IAnalogyDataProvider>() { new OfflineMiradaLogs() };
        }
    }
}
