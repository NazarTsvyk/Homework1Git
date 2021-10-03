using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Mulricast_Client
{
    public partial class Form1 : Form
    {
        private UdpClient client;
        private IPAddress groupAddress; // Адрес який буде використовуватись для групової рослилки
        private int localPort;
        private int remotePort;
        private int ttl; // час життя пакету
        private IPEndPoint remoteEP;
        private FileInfo[] fi;
        private Bitmap pict; // Для перетворення картинки
        public Form1()
        {
            InitializeComponent();
            groupAddress = IPAddress.Parse("234.5.5.11");
            localPort = 7778;
            remotePort = 7777;
            ttl = 32;
            client = new UdpClient(localPort);
            client.JoinMulticastGroup(groupAddress, ttl);
            Task.Run(() => 
            {
                while (true)
                {
                    IPEndPoint ep = null;
                    byte[] buffer = client.Receive(ref ep); // читаємо картинку
                    MemoryStream ms = new MemoryStream(buffer);
                    pict = new Bitmap(ms);
                    pictureBox1.Invoke(new Action(() => pictureBox1.Image = pict)); // Відображення картинки
                }
            });
        }
    }
}
