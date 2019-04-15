using System;
using System.IO;
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
    
    public partial class MainWindow : Window
    {
        public static ObservableCollection<Download> downloads = new ObservableCollection<Download>();

        public static List<WebClient> clients = new List<WebClient>();

        private CancellationTokenSource ctsForDownload;

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

        private async void BtnDownload_Click(object sender, RoutedEventArgs e)
        {
            using (var client = new WebClient())
            {
                clients.Add(client);
                
                ctsForDownload = new CancellationTokenSource();

                ctsForDownload.Token.Register(() =>
                {
                    client.CancelAsync();
                    txtOutput.Text += "\nDownload Cancelled.";
                });

                string path = SelectFolder();

                Download download = new Download(GetFileName(txtInput.Text), path)
                {
                    DownloadStartTime = DateTime.Now,
                    Progress = 0,
                    Client = client,
                    State = States.Downloading
                };

                try
                {
                    string inputText = txtInput.Text;

                    Uri url = new Uri(inputText);

                    string fileName = url.GetFileName();
                    
                    if (string.IsNullOrWhiteSpace(path) || string.IsNullOrEmpty(path))
                    {
                        downloads.Remove(download);
                        throw new FolderNotSelectedException(fileName);
                    }

                    string fullPath = $"{path}\\{fileName}";

                    client.DownloadProgressChanged += DownloadProgressChangedEventHandler;

                    client.DownloadFileCompleted += DownloadFinishedEventHandler;

                    downloads.Add(download);

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

            Download download = client.GetDownload();

            if (!e.Cancelled)
            {
                txtOutput.Text += $"\nDownload Finsished.";
                download.State = States.Finsished;
            }
            else
            {
                download.State = States.Cancelled;
                File.Delete(Path.Combine(download.Path, download.FileName));
            }
            lstvDownloads.Items.Refresh();
        }

        private void DownloadProgressChangedEventHandler(object sender, DownloadProgressChangedEventArgs e)
        {
            WebClient client = (WebClient)sender;

            Download download = client.GetDownload();

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
            Download download = lstvDownloads.SelectedItem as Download;

            if (download.State != States.Cancelled)
            {
                download.Cts.Cancel();                
                download.Cts.Dispose();            
            }

            //ctsForDownload.Cancel();
            //ctsForDownload.Dispose();
        }

        private void LstvDownloads_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            btnCancel.IsEnabled = true;
        }
    }

    public class FolderNotSelectedException : ApplicationException
    {
        public FolderNotSelectedException()
        {

        }

        public FolderNotSelectedException(string msg) : base(msg)
        {

        }
    }
}
