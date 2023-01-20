using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Server
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public int Temp { get; set; } = 0;
        public byte[] SaveImage { get; set; } = null;

        public DispatcherTimer timer;

        public CancellationTokenSource cancelTokenSource;

        public MainWindow()
        {
            InitializeComponent();
        }
             

        private async void Button_Click(object sender, RoutedEventArgs e)
        {

            timer = new DispatcherTimer();
            timer.Tick += new EventHandler(ChangeRecord);
            timer.Interval = new TimeSpan(0, 0, 0, 0, 350);
            timer.Start();
            cancelTokenSource = new CancellationTokenSource();
            CancellationToken token = new CancellationToken();
            await PeriodicFooAsync(new TimeSpan(0, 0, 0, 0, 100), token);
        }


        public async Task PeriodicFooAsync(TimeSpan interval, CancellationToken cancellationToken)
        {
            while (true)
            {
                
                if(cancelTokenSource.IsCancellationRequested) return;
                await My();
                await Task.Delay(interval, cancellationToken);
                
            }
        }
        private async Task My()
        {
            var client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            string ip = "127.0.0.1";
            IPAddress.TryParse(ip, out var iP);
            byte[] buffer = new byte[ushort.MaxValue];
            EndPoint endPoint = new IPEndPoint(IPAddress.Loopback, 27001);

            client.SendTo(Encoding.Default.GetBytes("get"), SocketFlags.None, endPoint);

            var len = client.ReceiveFrom(buffer, SocketFlags.None, ref endPoint);

            string response = Encoding.Default.GetString(buffer, 0, len);

            if (response.ToLower() != "start")
                return;

            len = client.ReceiveFrom(buffer, SocketFlags.None, ref endPoint);
            int parts = int.Parse(Encoding.Default.GetString(buffer, 0, len));
            client.SendTo(Encoding.Default.GetBytes("received"), SocketFlags.None, endPoint);

            len = client.ReceiveFrom(buffer, SocketFlags.None, ref endPoint);
            int lenght = int.Parse(Encoding.Default.GetString(buffer, 0, len));
            client.SendTo(Encoding.Default.GetBytes("received"), SocketFlags.None, endPoint);

            byte[] responseImage = new byte[lenght];
            var received = 0;
            for (int i = 0; i < parts; i++)
            {
                len = client.ReceiveFrom(buffer, SocketFlags.None, ref endPoint);

                for (int j = received, k = 0; k < len; j++, k++)
                {
                    responseImage[j] = buffer[k];
                }

                client.SendTo(Encoding.Default.GetBytes("received"), SocketFlags.None, endPoint);

                received += len;
            }
            var image = ByteToImage(responseImage);
            screenImage.Stretch = Stretch.Fill;
            screenImage.Source = image;
            SaveImage = responseImage;
        }

        public static ImageSource ByteToImage(byte[] imageData)
        {
            BitmapImage biImg = new BitmapImage();
            MemoryStream ms = new MemoryStream(imageData);
            biImg.BeginInit();
            biImg.StreamSource = ms;
            biImg.EndInit();
            ImageSource imgSrc = biImg as ImageSource;
            return imgSrc;
        }

        public BitmapImage ToImage(byte[] array)
        {
            using (var ms = new System.IO.MemoryStream(array))
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad; // here
                image.StreamSource = ms;
                image.EndInit();
                return image;
            }
        }


        private void ChangeRecord(object sender,EventArgs e)
        {
            if(Temp == 0)
            {
                recordBorder.Background =new SolidColorBrush(Colors.Red);
                Temp = 1;
            }
            else
            {
                recordBorder.Background = new SolidColorBrush(Colors.White);
                Temp = 0;
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if(timer != null)
            {
                timer.Stop();
                timer = null;
                recordBorder.Background = new SolidColorBrush(Colors.White);
                cancelTokenSource.Cancel();
            }
        }
    }
}
