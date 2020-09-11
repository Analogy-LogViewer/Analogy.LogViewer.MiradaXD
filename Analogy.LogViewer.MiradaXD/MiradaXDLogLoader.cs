using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Analogy.Interfaces;

namespace Analogy.LogViewer.MiradaXD
{
    public abstract class LogLoader
    {
        public abstract Task<IEnumerable<AnalogyLogMessage>> ReadFromStream(Stream dataStream, CancellationToken token, ILogMessageCreatedHandler logWindow);

        public virtual async Task<IEnumerable<AnalogyLogMessage>> ReadFromFile(string filename, CancellationToken token, ILogMessageCreatedHandler logWindow)
        {

            if (!File.Exists(filename))
            {
                await Task.CompletedTask;
                return new List<AnalogyLogMessage>();
            }

            using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                return await ReadFromStream(fs, token, logWindow);
            }
        }
    }
    public class MiradaXDLogLoader : LogLoader
    {
        protected string FileName;
        public override async Task<IEnumerable<AnalogyLogMessage>> ReadFromFile(string filename,
            CancellationToken token, ILogMessageCreatedHandler logWindow)
        {
            FileName = filename;
            return await base.ReadFromFile(filename, token, logWindow);

        }
        public override async Task<IEnumerable<AnalogyLogMessage>> ReadFromStream(Stream dataStream, CancellationToken token, ILogMessageCreatedHandler logWindow)
        {
            if ((dataStream == null) || (logWindow == null))
            {
                return new List<AnalogyLogMessage>();
            }
            List<AnalogyLogMessage> messages = new List<AnalogyLogMessage>();

            try
            {
                using (StreamReader streamReader = new StreamReader(dataStream, Encoding.UTF8))
                {
                    while (!streamReader.EndOfStream)
                    {
                        string line = String.Empty;
                        try
                        {
                            line = await streamReader.ReadLineAsync();
                            int firstComma = line.IndexOf(',');
                            AnalogyLogMessage m = new AnalogyLogMessage();
                            m.Source = "Mirada Log";
                            m.Module = "Mirada Log";
                            string datetime = line.Substring(0, firstComma);
                            if (DateTime.TryParse(datetime, out DateTime dateVal))
                            {
                                m.Date = dateVal;
                            }
                            string sub = line.Substring(firstComma + 1);
                            int firsSpace = sub.IndexOf(' ');
                            string proccess = sub.Substring(0, firsSpace);
                            if (int.TryParse(proccess, out int p))
                                m.ProcessId = p;
                            string other = sub.Substring(firsSpace + 1);
                            if (other.StartsWith("INFO "))
                            {
                                m.Level = AnalogyLogLevel.Information;
                                m.Text = other.Replace("INFO ", "").Trim();
                            }
                            else if (other.StartsWith("ERROR "))
                            {
                                m.Level = AnalogyLogLevel.Error;
                                m.Text = other.Replace("ERROR ", "");
                            }
                            else
                            {
                                if (other.Contains(": INFO :"))
                                {
                                    other = other.Replace(": INFO :", "");
                                    m.Level = AnalogyLogLevel.Information;

                                    int indexOfSpace = other.IndexOf(' ');
                                    m.Source = other.Substring(0, indexOfSpace);
                                    m.Text = other.Substring(indexOfSpace + 1).Trim();
                                }
                                else if (other.Contains(": ERROR:"))
                                {
                                    other = other.Replace(": ERROR:", "");
                                    m.Level = AnalogyLogLevel.Error;
                                    int indexOfSpace = other.IndexOf(' ');
                                    m.Source = other.Substring(0, indexOfSpace);
                                    m.Text = other.Substring(indexOfSpace + 1).Trim();
                                }
                                else if (other.Contains(": DEBUG:"))
                                {
                                    other = other.Replace(": DEBUG:", "");
                                    m.Level = AnalogyLogLevel.Debug;
                                    int indexOfSpace = other.IndexOf(' ');
                                    m.Source = other.Substring(0, indexOfSpace);
                                    m.Text = other.Substring(indexOfSpace + 1).Trim();
                                }
                                else
                                {
                                    m.Level = AnalogyLogLevel.Information;
                                    m.Text = other.Trim();
                                }
                            }
                            messages.Add(m);
                            if (token.IsCancellationRequested)
                            {
                                string msg = "Processing cancelled by User.";
                                messages.Add(new AnalogyLogMessage(msg, AnalogyLogLevel.Information, AnalogyLogClass.General, "Analogy", "None"));
                                logWindow.AppendMessages(messages, GetFileNameAsDataSource(FileName));
                                return messages;
                            }
                        }
                        catch (Exception e)
                        {
                            string fail = "Failed To parse: " + line + " Error:" + e;
                            messages.Add(new AnalogyLogMessage(fail, AnalogyLogLevel.Critical, AnalogyLogClass.General, "Analogy", "None"));
                        }
                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e);

            }
            if (!messages.Any())
            {
                AnalogyLogMessage empty = new AnalogyLogMessage($"File {FileName} is empty or corrupted", AnalogyLogLevel.Error, AnalogyLogClass.General, "Analogy", "");
                empty.Source = "Analogy";
                empty.Module = Process.GetCurrentProcess().ProcessName;
                messages.Add(empty);

            }
            logWindow.AppendMessages(messages, GetFileNameAsDataSource(FileName));
            return messages;
        }

        public static string GetFileNameAsDataSource(string fileName)
        {
            string file = Path.GetFileName(fileName);
            return fileName.Equals(file) ? fileName : $"{file} ({fileName})";

        }
    }
}
