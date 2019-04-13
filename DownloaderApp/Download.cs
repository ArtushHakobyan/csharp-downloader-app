using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DownloaderApp
{
    public class Download
    {
        public string FileName { get; set; }
        public DateTime DownloadStartTime { get; set; }
        public int Progress { get; set; }
        public WebClient Client { get; set; }
        public string State { get; set; }
        public double DownloadSpeed { get; set; }

        public string DownloadSpeedString => $"{DownloadSpeed} mb/s";

        public Download(string fileName)
        {
            FileName = fileName;
            DownloadStartTime = DateTime.Now;
        }

        public override string ToString()
        {
            return $"{FileName}, {DownloadStartTime}";
        }
    }
}
