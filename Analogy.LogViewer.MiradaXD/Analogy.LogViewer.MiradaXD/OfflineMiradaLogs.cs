using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Analogy.Interfaces;

namespace Analogy.LogViewer.MiradaXD
{
    public class OfflineMiradaLogs : IAnalogyOfflineDataProvider
    {
        public string OptionalTitle { get; } = "ICAP Mirada Offline Parser";
        public bool IsConnected { get; set; }
        public Guid ID { get; } = new Guid("3D753B16-D542-4386-9403-4E88B597ECFA");
        public bool CanSaveToLogFile { get; } = false;
        public string FileOpenDialogFilters { get; } = "Mirada XD log|XD.log|Mirada XD Debug file|XDDebug.log";
        public string FileSaveDialogFilters { get; } = string.Empty;
        public IEnumerable<string> SupportFormats { get; } = new[] { "XD.log", "XDDebug.log" };
        public string InitialFolderFullPath { get; } = String.Empty;

        public Task InitializeDataProviderAsync(IAnalogyLogger logger)
        {
            return Task.CompletedTask;
        }

        public void MessageOpened(AnalogyLogMessage message)
        {
            //nop
        }

        public async Task<IEnumerable<AnalogyLogMessage>> Process(string fileName, CancellationToken token, ILogMessageCreatedHandler messagesHandler)
        {
            if (CanOpenFile(fileName))
            {
                var logLoader = new MiradaXDLogLoader();
                var messages = await logLoader.ReadFromFile(fileName, token, messagesHandler).ConfigureAwait(false);
                return messages;
            }
            else
            {
                AnalogyLogMessage m = new AnalogyLogMessage();
                m.Text = $"Unsupported file: {fileName}. Skipping file";
                m.Level = AnalogyLogLevel.Critical;
                m.Source = "Analogy";
                m.Module = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
                m.ProcessID = System.Diagnostics.Process.GetCurrentProcess().Id;
                m.Class = AnalogyLogClass.General;
                m.User = Environment.UserName;
                m.Date = DateTime.Now;
                messagesHandler.AppendMessage(m, Environment.MachineName);
                return new List<AnalogyLogMessage>() { m };
            }
        }

        public IEnumerable<FileInfo> GetSupportedFiles(DirectoryInfo dirInfo, bool recursiveLoad)
        {
            return GetSupportedFilesInternal(dirInfo, recursiveLoad);
        }

        public Task SaveAsync(List<AnalogyLogMessage> messages, string fileName)
        {
            throw new NotImplementedException();
        }

        public bool CanOpenFile(string fileName)
            => fileName.EndsWith("XD.log", StringComparison.InvariantCultureIgnoreCase) ||
               fileName.EndsWith("XDDebug.log", StringComparison.InvariantCultureIgnoreCase);
        public bool CanOpenAllFiles(IEnumerable<string> fileNames) => fileNames.All(CanOpenFile);

        public static List<FileInfo> GetSupportedFilesInternal(DirectoryInfo dirInfo, bool recursive)
        {
            List<FileInfo> files = dirInfo.GetFiles("XD.log").Concat(dirInfo.GetFiles("XDDebug.log")).ToList();
            if (!recursive)
                return files;
            try
            {
                foreach (DirectoryInfo dir in dirInfo.GetDirectories())
                {
                    files.AddRange(GetSupportedFilesInternal(dir, true));
                }
            }
            catch (Exception)
            {
                return files;
            }

            return files;
        }
    }
}
