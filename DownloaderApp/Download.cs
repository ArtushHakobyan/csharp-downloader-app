using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace DownloaderApp
{
    public enum States
    {
        Downloading,
        Finsished,
        Cancelled
    }

    public class Download
    {
        private static int _count = 0;

        public int ID { get; set; }
        public string FileName { get; set; }
        public DateTime DownloadStartTime { get; set; }
        public int Progress { get; set; }
        public WebClient Client { get; set; }
        public States State { get; set; }
        public double DownloadSpeed { get; set; }
        public CancellationTokenSource Cts = new CancellationTokenSource();

        public string DownloadSpeedString => $"{DownloadSpeed} mb/s";

        public Download(string fileName)
        {
            FileName = fileName;
            DownloadStartTime = DateTime.Now;
            _count++;
            ID = _count;
            Cts.Token.Register(() => 
            {
                Client.CancelAsync();
            });
        }

        public override string ToString()
        {
            return $"{FileName}, {DownloadStartTime}";
        }
    }
}
