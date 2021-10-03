using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Multicast_Sample1
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
            groupAddress = IPAddress.Parse("234.5.5.11"); // Діапазон адрес класу D
            localPort = 7777;
            remotePort = 7778;
            ttl = 32; // 32 стрибки 
            client = new UdpClient(localPort);
            client.JoinMulticastGroup(groupAddress, ttl);
            remoteEP = new IPEndPoint(groupAddress, remotePort); // Кінцева точка
            DirectoryInfo di = new DirectoryInfo("Pictures");
            fi = di.GetFiles("*.jpg"); // Читаємо всі вайли розширення яких jpg
            foreach (var i in fi)
            {
                comboBox1.Items.Add(i.Name); // Добавляємо імена картинок в comboBox1
            }

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            pict = new Bitmap("Pictures\\" + comboBox1.SelectedItem.ToString());// Буде створена Bitmap
            Info info = new Info() { Pict = new Bitmap(pict), RemoteEP = remoteEP };
            pictureBox1.Image = pict;
            Task.Factory.StartNew(new Action<Object>((x) =>
            {
                Info tmp = x as Info;
                using (MemoryStream ms = new MemoryStream())
                {
                    tmp.Pict.Save(ms, ImageFormat.Jpeg);
                    byte[] data = ms.ToArray();
                    client.Send(data,data.Length,tmp.RemoteEP);
                }
            }
            ), info);
        }
    }
}
