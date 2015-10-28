using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using Microsoft.Band;
using Microsoft.AspNet.SignalR.Client;
using System.Net.Http;
using Windows.System.Display;


// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=391641

namespace QuantifyMe81
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        HubConnection _connection;
        IHubProxy _myHub;
        //Endpoint=sb://vjquantifyme.servicebus.windows.net/;SharedAccessKeyName=device;SharedAccessKey=N/mCWx+Bgj2HiAkYNhgVeB/2olm6zl4rN8NboTRZWaw=
        //https://NAMESPACE.servicebus.windows.net/EVENTHUB-NAME/publishers/PUBLISHER-NAME/messages
        //https://vjquantifyme.servicebus.windows.net/vjquantifyme/publishers/device/messages
        //string sas = "SharedAccessSignature sr=https%3a%2f%2fvjtechdays.servicebus.windows.net%2fvjhubtest%2fpublishers%2fdevicea%2fmessages&sig=tiqpFPj2swppQH6d%2bdCqkDS5nbbMhgK6654eWxwJNN4%3d&se=1436347815&skn=adminpolicy";

        // string sas = "SharedAccessSignature sr=https%3a%2f%2fvjquantifyme.servicebus.windows.net%2fvjquantifyme%2fpublishers%2fdevice%2fmessages&sig=tiqpFPj2swppQH6d%2bdCqkDS5nbbMhgK6654eWxwJNN4%3d&se=1436347815&skn=device";
        //string sas = "SharedAccessSignature sr=rFFAB3DYXhJTr6NrSbEwkZdagdPDHaBzY1sZyFpGLAk=";
        //string serviceNamespace = "vjquantifyme";
        //string hubName = "quantifymehub";
        //string deviceName = "device";
        HttpClient httpClient;

        private App viewModel;

        private IBandClient bandClient;
        public MainPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;

            this.DataContext = this.viewModel = App.Current;

            App.Current.MainWindow = this;

            quantificationGrid.DataContext = App.Current.Quantification;

            var dr = new DisplayRequest();
            dr.RequestActive();



            _connection = new HubConnection("http://quantifymewebhub.azurewebsites.net/");
            _myHub = _connection.CreateHubProxy("QuantifyMeHub");

            _connection.Start().ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    //Console.WriteLine("There was an error opening the connection:{0}",
                    //                  task.Exception.GetBaseException());
                }
                else
                {

                    _connection.Send("Phone");
                }

            }).Wait();

            //httpClient = new HttpClient();
            //httpClient.BaseAddress = new Uri(String.Format("https://{0}.servicebus.windows.net/", serviceNamespace));
            //httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", sas);


        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // TODO: Prepare page for display here.

            // TODO: If your application contains multiple pages, ensure that you are
            // handling the hardware Back button by registering for the
            // Windows.Phone.UI.Input.HardwareButtons.BackPressed event.
            // If you are using the NavigationHelper provided by some templates,
            // this event is handled for you.
        }

        private async void stop_Click(object sender, RoutedEventArgs e)
        {
            await bandClient.SensorManager.HeartRate.StopReadingsAsync();
            await bandClient.SensorManager.Accelerometer.StopReadingsAsync();
            await bandClient.SensorManager.SkinTemperature.StopReadingsAsync();
            this.viewModel.StatusMessage = "Stopped sensing.";

            stop.Visibility = Visibility.Collapsed;
            start.Visibility = Visibility.Visible;
        }

        private async void start_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                start.IsEnabled = false;
                this.viewModel.StatusMessage = "Starting...";

                IBandInfo[] pairedBands = await BandClientManager.Instance.GetBandsAsync();
                bandClient = await BandClientManager.Instance.ConnectAsync(pairedBands[0]);

                if (pairedBands.Length < 1)
                {
                    this.viewModel.StatusMessage = "Connect your band and click the start button again.";
                    return;
                }

                bool heartRateConsentGranted;

                // Check whether the user has granted access to the HeartRate sensor.
                if (bandClient.SensorManager.HeartRate.GetCurrentUserConsent() == UserConsent.Granted)
                {
                    heartRateConsentGranted = true;
                }
                else
                {
                    heartRateConsentGranted = await bandClient.SensorManager.HeartRate.RequestUserConsentAsync();
                }

                if (!heartRateConsentGranted)
                {
                    this.viewModel.StatusMessage = "Access to the heart rate sensor is denied.";

                }
                else
                {
                    // Subscribe to HeartRate data.
                    bandClient.SensorManager.HeartRate.ReadingChanged += (s, args) =>
                   {
                       this.viewModel.StatusMessage = args.SensorReading.HeartRate.ToString();
                       this.viewModel.Quantification.HeartRate = args.SensorReading.HeartRate;

                       var eventData = new
                       {
                           HeartRate = args.SensorReading.HeartRate,
                           Quality = args.SensorReading.Quality == Microsoft.Band.Sensors.HeartRateQuality.Locked ? 1 : 0
                       };

                       // await httpClient.PostAsJsonAsync(String.Format("{0}/publishers/{1}/messages", hubName, deviceName), eventData);
                       _myHub.Invoke("Send", "Phone",this.viewModel.Quantification.HeartRate);
                    };
                    await bandClient.SensorManager.HeartRate.StartReadingsAsync();
                }

                bool gyroConsentGranted;

                // Check whether the user has granted access to the HeartRate sensor.
                if (bandClient.SensorManager.Accelerometer.GetCurrentUserConsent() == UserConsent.Granted)
                {
                    gyroConsentGranted = true;
                }
                else
                {
                    gyroConsentGranted = await bandClient.SensorManager.Accelerometer.RequestUserConsentAsync();
                }

                if (!gyroConsentGranted)
                {
                    this.viewModel.StatusMessage = "Access to the accelerometer is denied.";
                }
                else
                {
                    // Subscribe to HeartRate data.
                    bandClient.SensorManager.Accelerometer.ReadingChanged += (s, args) =>
                    {
                        this.viewModel.StatusMessage = args.SensorReading.AccelerationX.ToString();
                        this.viewModel.Quantification.AccelX = Math.Round(args.SensorReading.AccelerationX, 2);
                        this.viewModel.Quantification.AccelY = Math.Round(args.SensorReading.AccelerationY, 2);
                        this.viewModel.Quantification.AccelZ = Math.Round(args.SensorReading.AccelerationZ, 2);
                    };

                    await bandClient.SensorManager.Accelerometer.StartReadingsAsync();
                }

                bool skintempConsentGranted;

                // Check whether the user has granted access to the HeartRate sensor.
                if (bandClient.SensorManager.SkinTemperature.GetCurrentUserConsent() == UserConsent.Granted)
                {
                    skintempConsentGranted = true;
                }
                else
                {
                    skintempConsentGranted = await bandClient.SensorManager.SkinTemperature.RequestUserConsentAsync();
                }

                if (!skintempConsentGranted)
                {
                    this.viewModel.StatusMessage = "Access to the skin temp is denied.";
                }
                else
                {
                    // Subscribe to HeartRate data.
                    bandClient.SensorManager.SkinTemperature.ReadingChanged += (s, args) =>
                    {
                        // this.viewModel.StatusMessage = args.SensorReading.AccelerationX.ToString();
                        this.viewModel.Quantification.SkinTemp = args.SensorReading.Temperature;
                    };

                    await bandClient.SensorManager.Accelerometer.StartReadingsAsync();
                }

                start.Visibility = Visibility.Collapsed;
                stop.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                this.viewModel.StatusMessage = ex.ToString();
            }
        }
    }
}
