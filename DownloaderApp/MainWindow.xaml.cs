using System;
using System.Windows;
using System.Windows.Forms;
using System.Net;
using System.Threading;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DownloaderApp
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>

    public class FolderNotSelectedException : ApplicationException
    {
        public FolderNotSelectedException()
        {

        }

        public FolderNotSelectedException(string msg):base(msg)
        {

        }
    }

    public partial class MainWindow : Window
    {
        private ObservableCollection<Download> downloads = new ObservableCollection<Download>();

        private CancellationTokenSource ctsForDownload;

        private long prevBytes = 0;

        private DateTime lastUpdate;

        public MainWindow()
        {
            InitializeComponent();
            lstvDownloads.ItemsSource = downloads;
            lstvDownloads.SourceUpdated += LstvDownloads_SourceUpdatedEventHandler;
        }

        private void LstvDownloads_SourceUpdatedEventHandler(object sender, System.Windows.Data.DataTransferEventArgs e)
        {
            lstvDownloads.Items.Refresh();
        }

        private void BtnDownload_Click(object sender, RoutedEventArgs e)
        {
            using (var client = new WebClient())
            {
                Download download = new Download(GetFileName(txtInput.Text))
                {
                    DownloadStartTime = DateTime.Now,
                    Progress = 0,
                    Client = client,
                    State = "Downloading"
                };
                btnCancel.IsEnabled = true;
                txtOutput.Text = string.Empty;

                ctsForDownload = new CancellationTokenSource();

                ctsForDownload.Token.Register(() =>
                {
                    client.CancelAsync();
                    txtOutput.Text += "\nDownload Cancelled.";
                });

                try
                {
                    string inputText = txtInput.Text;

                    Uri url = new Uri(inputText);

                    string fileName = GetFileName(inputText);

                    string path = SelectFolder();

                    if (string.IsNullOrWhiteSpace(path) || string.IsNullOrEmpty(path))
                    {
                        downloads.Remove(download);
                        throw new FolderNotSelectedException(fileName);
                    }

                    string fullPath = $"{path}\\{fileName}";

                    client.DownloadProgressChanged += DownloadProgressChangedEventHandler;

                    client.DownloadFileCompleted += DownloadFinishedEventHandler;

                    downloads.Add(download);

                    lastUpdate = DateTime.Now;

                    client.DownloadFileAsync(url, fullPath);

                    txtOutput.Text = $"Downloading: {fileName}";
                }
                catch (UriFormatException)
                {
                    txtOutput.Text = "Incorrect URL";
                    downloads.Remove(download);
                }
                catch (WebException)
                {

                    throw;
                }
                catch (InvalidOperationException)
                {

                    throw;
                }
                catch (FolderNotSelectedException exception)
                {
                    ctsForDownload.Cancel();
                    ctsForDownload.Dispose();
                    downloads.Remove(download);
                }
                catch (Exception)
                {

                    throw;
                }
            }
        }

        private void DownloadFinishedEventHandler(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            WebClient client = (WebClient)sender;
            Download download = GetDownloadByClient(client);
            if (!e.Cancelled)
            {
                txtOutput.Text += $"\nDownload Finsished.";
                download.State = "Finished";
            }
            else
            {
                download.State = "Cancelled";
            }
            lstvDownloads.Items.Refresh();
        }

        private void DownloadProgressChangedEventHandler(object sender, DownloadProgressChangedEventArgs e)
        {
            WebClient client = (WebClient)sender;
            Download download = GetDownloadByClient(client);

            download.DownloadSpeed = CalculateSpeed(e.TotalBytesToReceive);

            if (download != null)
            {
                download.Progress = e.ProgressPercentage;
            }
            else
            {
                txtOutput.Text += "\nDownload not found!";
            }

            lstvDownloads.Items.Refresh();
        }

        private string GetFileName(string url)
        {
            string[] stringParts = url.Split('/');
            string fileName = string.Empty;

            if(stringParts.Length > 0)
            {
                fileName = stringParts[stringParts.Length - 1];
            }
            else
            {
                fileName = url;
            }

            return fileName;
        }

        private string SelectFolder()
        {
            using (var folderBrowserDialog = new FolderBrowserDialog())
            {
                DialogResult result = new DialogResult();

                result = folderBrowserDialog.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    return folderBrowserDialog.SelectedPath;
                }

                return string.Empty;
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            ctsForDownload.Cancel();
            ctsForDownload.Dispose();
        }

        private Download GetDownloadByClient(WebClient client)
        {
            foreach (var download in downloads)
            {
                if(download.Client == client)
                {
                    return download;
                }
            }
            return null;
        }

        private double CalculateSpeed(long BytesRecieved)
        {
            var time = DateTime.Now - lastUpdate;

            long bytes = BytesRecieved - prevBytes;

            long kb = bytes / 1024;

            long mb = kb / 1024;

            double speed = mb / time.TotalSeconds;

            lastUpdate = DateTime.Now;

            prevBytes = BytesRecieved;

            return speed;
        }
    }
}
