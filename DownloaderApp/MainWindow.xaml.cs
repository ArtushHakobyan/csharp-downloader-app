using System;
using System.Windows;
using System.Windows.Forms;
using System.Net;

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
        public MainWindow()
        {
            InitializeComponent();
        }

        private void BtnDownload_Click(object sender, RoutedEventArgs e)
        {
            using (WebClient client = new WebClient())
            {
                btnDownload.IsEnabled = false;
                txtInput.IsEnabled = false;
                txtOutput.Text = string.Empty;

                try
                {
                    string inputText = txtInput.Text;

                    Uri url = new Uri(inputText);

                    string fileName = GetFileName(inputText);

                    string path = SelectFolder();

                    if (string.IsNullOrWhiteSpace(path))
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
                    txtOutput.Text += $"\nDownload Canceled: {exception.Message}";
                    EnableDownload();
                    prgDownload.Value = 0;
                }
                catch (Exception)
                {

                    throw;
                }
            }
        }

        private void DownloadFinishedEventHandler(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            txtOutput.Text += $"\nDownload Finsished.";
            EnableDownload();
            prgDownload.Value = 0;
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

                return folderBrowserDialog.SelectedPath;
            }
        }

        private void EnableDownload()
        {
            txtInput.IsEnabled = true;
            btnDownload.IsEnabled = true;
        }
    }
}
