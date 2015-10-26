using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;

namespace QuantifyMe81
{
    public class Quantification : INotifyPropertyChanged
    {
        private int _heartRate;
        public int HeartRate
        {
            get { return _heartRate; }
            set
            {
                _heartRate = value;
                NotifyPropertyChanged();
            }
        }
        private double _accelX;
        public double AccelX
        {
            get { return _accelX; }
            set
            {
                _accelX = value;
                NotifyPropertyChanged();
            }
        }
        private double _accelY;
        public double AccelY
        {
            get { return _accelY; }
            set
            {
                _accelY = value;
                NotifyPropertyChanged();
            }
        }
        private double _accelZ;
        public double AccelZ
        {
            get { return _accelZ; }
            set
            {
                _accelZ = value;
                NotifyPropertyChanged();
            }
        }

        private double _skinTemp;

        public double SkinTemp
        {
            get { return _skinTemp; }
            set
            {
                _skinTemp = value;
                NotifyPropertyChanged();
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        private async void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                });
            }
        }

    }
}
