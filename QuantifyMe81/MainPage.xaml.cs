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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=391641

namespace QuantifyMe81
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private App viewModel;
        
        private IBandClient bandClient;
        public MainPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;

            this.DataContext = this.viewModel = App.Current;

            App.Current.MainWindow = this;

            quantificationGrid.DataContext = App.Current.Quantification;


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
            this.viewModel.StatusMessage = "Stopped sensing.";
        }

        private async void start_Click(object sender, RoutedEventArgs e)
        {
            try
            {
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
                        this.viewModel.Quantification.AccelX = args.SensorReading.AccelerationX;
                        this.viewModel.Quantification.AccelY = args.SensorReading.AccelerationY;
                        this.viewModel.Quantification.AccelZ = args.SensorReading.AccelerationZ;
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
            }
            catch (Exception ex)
            {
                this.viewModel.StatusMessage = ex.ToString();
            }
        }
    }
}
