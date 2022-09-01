using System;
using System.Collections.ObjectModel;
using System.Net;
using System.Windows.Input;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace TestAppFox
{
    public class ScreenshotData
    {
        public int id { get; set; }
        public DateTime date { get; set; }
        public string screenshot { get; set; }
    }

    public class ScreenShotDataViewModel
    {
        public int Id { get; set; }
        public string Date { get; set; }
        public BitmapSource Screenshot { get; set; }
    }

    public class MainWindowViewModel : NotifyObject
    {
        private Uri MainUri { get; } = new Uri("http://45.84.226.180/");
        private string GetUri { get; } = "GetScreenshots";
        private string UploadUri { get; } = "UploadScreenshot";
        public ICommand GetScreenShotsCommand { get; }
        public ICommand SaveScreenShotCommand { get; }

        public ObservableCollection<ScreenShotDataViewModel> Screenshots { get; }

        public bool _loading;
        public bool Loading
        {
            get => _loading;
            set => RaisePropertyChanged(ref _loading, value);
        } 

        public bool _uploaded;
        public bool Uploaded
        {
            get => _uploaded;
            set => RaisePropertyChanged(ref _uploaded, value);
        }  
        
        public bool _uploading;
        public bool Uploading
        {
            get => _uploading;
            set => RaisePropertyChanged(ref _uploading, value);
        } 
        
        public DateTime _startDateSelected;
        public DateTime StartDateSelected
        {
            get => _startDateSelected;
            set => RaisePropertyChanged(ref _startDateSelected, value);
        } 
        
        public DateTime _finishDateSelected;
        public DateTime FinishDateSelected
        {
            get => _finishDateSelected;
            set => RaisePropertyChanged(ref _finishDateSelected, value);
        }

        public string _error;
        public string Error
        {
            get => _error;
            set => RaisePropertyChanged(ref _error, value);
        }

        public MainWindowViewModel()
        {
            Screenshots = new ObservableCollection<ScreenShotDataViewModel>();
            GetScreenShotsCommand = new CommandHandler(() => GetScreenshots().BreakIfFailed());
            SaveScreenShotCommand = new CommandHandler(UploadScreenshots);
            FinishDateSelected = DateTime.Now;
            StartDateSelected = DateTime.Now.AddDays(-1);
            Error = "";
            Loading = false;
        }

        //async запуск задачи получения скриншотов
        private async Task GetScreenshots()
        {
            try
            {
                Loading = true;
                Error = "";
                Screenshots.Clear();

                var shortDateStringFrom = StartDateSelected.ToString("yyyy.MM.dd");
                var dateStringTo = FinishDateSelected.AddDays(1).ToString("yyyy.MM.dd");
                var query = $"?startDate={shortDateStringFrom}&endDate={dateStringTo}";

                string address = MainUri + GetUri + query;
                using (WebClient client = new WebClient())
                {
                    client.DownloadStringCompleted += ClientOnDownloadStringCompleted;
                    await client.DownloadStringTaskAsync(address);
                    client.DownloadStringCompleted -= ClientOnDownloadStringCompleted;
                }
            }
            catch (Exception e)
            {
                Loading = false;
                Error = e.Message;
            }

        }

        //реакция на получение данных
        private void ClientOnDownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            try
            {
                Dispatcher dispatcher = Dispatcher.CurrentDispatcher;
                ScreenshotData[] data = JsonSerializer.Deserialize<ScreenshotData[]>(e.Result);
                foreach (var screenshotData in data)
                {
                    var base64 = Convert.FromBase64String(screenshotData.screenshot);
                    var image = BytesToImageConverter.Decode(base64);

                    dispatcher.Invoke(() =>
                    {
                        Screenshots.Add(new ScreenShotDataViewModel
                        {
                            Date = screenshotData.date.ToShortDateString(),
                            Id = screenshotData.id,
                            Screenshot = image
                        });
                    });
                }
                Loading = false;
            }
            catch (Exception exception)
            {
                Error = exception.Message;
                Loading = false;
            }
        }

        //async -Добавил чтобы чуть-чуть была анимация, можно убрать.
        private async void UploadScreenshots()
        {
            try
            {
                Uploaded = false;
                Uploading = true;
                ScreenCapturer s = new ScreenCapturer();
                var bitmap = s.Capture();
                string fileName = "fae4e9aa376346b.jpg";
                bitmap.Save(fileName);
                string URI = MainUri + UploadUri;
                using var webClient = new WebClient();
                webClient.Headers.Add(HttpRequestHeader.Accept, "application/octet-stream");
                var returned = webClient.UploadFile(URI, fileName);
                Uploaded = true;
            }
            catch (Exception e)
            {
                Uploaded = false;
                Error = e.Message;
            }
            finally
            {
                //Добавил чтобы чуть-чуть была анимация, можно убрать.
                await Task.Delay(1000);
                Uploading = false;
            }
        }
    }
}
