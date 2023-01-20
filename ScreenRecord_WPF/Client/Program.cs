using System.Drawing;
using System.Net.Sockets;
using System.Net;
using System.Text;

namespace Client
{
    internal class Program
    {
        static void Main(string[] args)
        {
            IPAddress ip = IPAddress.Loopback;
            var listener = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            var listenerEndP = new IPEndPoint(ip, 27001);
            listener.Bind(listenerEndP);

            var sndcam = new byte[ushort.MaxValue]; // buffer

            EndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);

            while (true)
            {
                var len = listener.ReceiveFrom(sndcam, ref endPoint);
                string request = Encoding.Default.GetString(sndcam, 0, len);
                if (request.ToLower() != "get")
                    continue;


                listener.SendTo(Encoding.Default.GetBytes("start"), SocketFlags.None, endPoint);

                Bitmap bitmap = new Bitmap(1920, 1080);

                Size size = new Size(bitmap.Width, bitmap.Height);
                Graphics graphics = Graphics.FromImage(bitmap);
                graphics.CopyFromScreen(0, 0, 0, 0, size);

                ImageConverter converter = new ImageConverter();
                var bytes = (byte[])converter.ConvertTo(bitmap, typeof(byte[]));

                int numberOfParts = (bytes.Length / 40_000) + 1;
                listener.SendTo(Encoding.Default.GetBytes(numberOfParts.ToString()), SocketFlags.None, endPoint);
                listener.ReceiveFrom(sndcam, ref endPoint);


                listener.SendTo(Encoding.Default.GetBytes(bytes.Length.ToString()), SocketFlags.None, endPoint);
                listener.ReceiveFrom(sndcam, ref endPoint);

                int count = 0;
                int sended = 0;
                for (int i = 0; i < numberOfParts; i++)
                {

                    sended += listener.SendTo(bytes.Skip(count).Take(40000).ToArray(), SocketFlags.None, endPoint);
                    listener.ReceiveFrom(sndcam, ref endPoint);
                    count += 40000;

                }
            }
        }
    }
}