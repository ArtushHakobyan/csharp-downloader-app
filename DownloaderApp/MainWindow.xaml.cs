using System;
using System.Windows;
using System.Windows.Forms;
using System.Net;
using System.Threading;
using System.Collections.Generic;

namespace DownloaderApp
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public class Download
    {
        public string FileName { get; set; }
    }

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
        public MainWindow()
        {
            InitializeComponent();
        }

        private List<WebClient> clients = new List<WebClient>();

        CancellationTokenSource ctsForDownload;

        private void BtnDownload_Click(object sender, RoutedEventArgs e)
        {
            using (var client = new WebClient())
            {
                clients.Add(client);
                lstvDownloads.Items.Add(GetFileName(txtInput.Text));
                btnDownload.IsEnabled = false;
                btnCancel.IsEnabled = true;
                txtInput.IsEnabled = false;
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
                        throw new FolderNotSelectedException(fileName);
                    }

                    string fullPath = $"{path}\\{fileName}";

                    client.DownloadProgressChanged += DownloadProgressChangedEventHandler;

                    client.DownloadFileCompleted += DownloadFinishedEventHandler;

                    client.DownloadFileAsync(url, fullPath);

                    txtOutput.Text = $"Downloading: {fileName}";
                }
                catch (UriFormatException)
                {
                    txtOutput.Text = "Incorrect URL";
                    EnableDownload();
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
                    EnableDownload();
                }
                catch (Exception)
                {

                    throw;
                }
            }
        }

        private void DownloadFinishedEventHandler(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            EnableDownload();
            prgDownload.Value = 0;
            if (!e.Cancelled)
            {
                txtOutput.Text += $"\nDownload Finsished.";
            }
        }

        private void DownloadProgressChangedEventHandler(object sender, DownloadProgressChangedEventArgs e)
        {
            prgDownload.Value = e.ProgressPercentage;
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

        private void EnableDownload()
        {
            txtInput.IsEnabled = true;
            btnDownload.IsEnabled = true;
            btnCancel.IsEnabled = false;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            ctsForDownload.Cancel();
            ctsForDownload.Dispose();
            EnableDownload();
            prgDownload.Value = 0;
        }
    }
}
