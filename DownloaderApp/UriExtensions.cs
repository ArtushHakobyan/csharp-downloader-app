using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DownloaderApp
{
    public static class UriExtensions
    {
        public static string GetFileName(this Uri uri)
        {
            string str = uri.OriginalString;
            string[] stringParts = str.Split('/');
            string fileName = string.Empty;

            if (stringParts.Length > 0)
            {
                fileName = stringParts[stringParts.Length - 1];
            }
            else
            {
                fileName = str;
            }

            return fileName;
        }
    }
}
