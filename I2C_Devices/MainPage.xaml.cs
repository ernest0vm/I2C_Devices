using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.I2c;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.System;

// La plantilla de elemento Página en blanco está documentada en https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0xc0a

namespace I2C_Devices
{
    /// <summary>
    /// Página vacía que se puede usar de forma independiente o a la que se puede navegar dentro de un objeto Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        private I2cDevice Device;

        private Timer periodicTimer;

        byte[] ReadBuf = new byte[1];
        byte[] data_ = new byte[1];
        int iLed;
        
        byte[] Reg_ON = new byte[] { 0x53 };
        byte[] Reg_OFF = new byte[] { 0x54 };
        byte[] Reg_SHUTDOWN = new byte[] { 0x55 };

        bool displayalert = true;
        
        public MainPage()
        {
            this.InitializeComponent();

            Initi2c();

        }

        private async void Initi2c()

        {

            var settings = new I2cConnectionSettings(0x08); // Arduino address

            settings.BusSpeed = I2cBusSpeed.FastMode;

            string aqs = I2cDevice.GetDeviceSelector("I2C1");

            var dis = await DeviceInformation.FindAllAsync(aqs);

            Device = await I2cDevice.FromIdAsync(dis[0].Id, settings);

            periodicTimer = new Timer(this.TimerCallback, null, 0, 1000); // Create a timmer

        }

        private void TimerCallback(object state)

        {

            try

            {

               Device.Read(ReadBuf); // read the data
               data_ = ReadBuf;

              
            }
            catch (Exception f)
            { 

                Debug.WriteLine(f.Message);
                Log(f.Message);

            }


            if (data_.SequenceEqual(Reg_ON))
            {
                if (displayalert)
                {
                    Debug.WriteLine("Received: MUTE");
                    Log("Received: MUTE");
                    iLed = 3;
                }
                else
                {
                    Debug.WriteLine("Received: ON");
                    Log("Received: ON");
                    iLed = 1;
                }
            }
            else if (data_.SequenceEqual(Reg_OFF))
            {
                Debug.WriteLine("Received: OFF");
                Log("Received: OFF");
                iLed = 0;

            }
            else if (data_.SequenceEqual(Reg_SHUTDOWN))
            {
                Debug.WriteLine("Received: Shutdown");
                Log("Received: Shutdown");
                iLed = 2;

                var task_ = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    txtLog.Text = "Shutdowning device, please wait for a moment...";

                    ShutdownManager.BeginShutdown(ShutdownKind.Shutdown, TimeSpan.FromSeconds(3));

                });

                //new Task(() =>
                //{
                //    ShutdownManager.BeginShutdown(0, TimeSpan.FromSeconds(0.5));
                //}).Start();
            }

            var task = this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => 
            {
                switch (iLed)
                {
                    case 0:
                        Led.Fill = new SolidColorBrush(Colors.Red);
                        break;
                    case 1:
                        Led.Fill = new SolidColorBrush(Colors.Green);
                        break;
                    case 2:
                        Led.Fill = new SolidColorBrush(Colors.Orange);
                        break;
                    case 3:
                        Led.Fill = new SolidColorBrush(Colors.Blue);
                        break;
                }
               
            


            });


            data_ = null;
            
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Device.Write(Reg_SHUTDOWN);
        }

        private void Log(string msgData)
        {
            var task_ = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                txtLog.Text = msgData;
            });
        }

      
    }
        
}