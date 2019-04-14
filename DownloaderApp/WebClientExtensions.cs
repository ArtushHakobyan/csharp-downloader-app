using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace DownloaderApp
{
    public static class WebClientExtensions
    {
        public static Download GetDownload(this WebClient client)
        {
            foreach (var download in MainWindow.downloads)
            {
                if(download.Client == client)
                {
                    return download;
                }
            }

            return null;
        }
    }
}
