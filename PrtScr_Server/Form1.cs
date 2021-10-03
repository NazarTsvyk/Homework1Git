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
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PrtScr_Server
{
    public partial class Form1 : Form
    {
        UdpClient client;
        IPAddress groupAddress; // Адрес який буде використовуватись для групової рослилки
        int localPort;
        int remotePort;
        int ttl; // час життя пакету
        IPEndPoint remoteEP;

        public Form1()
        {
            InitializeComponent();
            
            groupAddress = IPAddress.Parse("234.5.5.77"); // Діапазон адрес класу D
            localPort = 7777;
            remotePort = 7778;
            ttl = 32; // 32 стрибки 
            client = new UdpClient(localPort);
            client.JoinMulticastGroup(groupAddress, ttl);
            remoteEP = new IPEndPoint(groupAddress, remotePort); // Кінцева точка

            timer1.Start();

        }

        private byte[][] BufferSplit(byte[] buffer, int blockSize)
        {
            byte[][] blocks = new byte[(buffer.Length + blockSize - 1) / blockSize][];
            for (int i = 0, j = 0; i < blocks.Length; i++, j += blockSize)
            {
                blocks[i] = new byte[Math.Min(blockSize, buffer.Length - j)];
                Array.Copy(buffer, j, blocks[i], 0, blocks[i].Length);
            }
            return blocks;
        }

        private string GenerateId()
        {
            return Guid.NewGuid().ToString("N");
        }
        private void timer1_Tick(object sender, EventArgs e)
        {

            Bitmap bitmap = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height, PixelFormat.Format32bppArgb);
            Graphics graphics = Graphics.FromImage(bitmap);
            graphics.CopyFromScreen(0, 0, 0, 0, Screen.PrimaryScreen.Bounds.Size, CopyPixelOperation.SourceCopy);
            byte[] data;
            using (var memoryStream = new MemoryStream())
            {
                bitmap.Save(memoryStream, ImageFormat.Jpeg);
                data = memoryStream.ToArray();
            }
            byte[][] bufferArray = BufferSplit(data, 61440);
            string id = GenerateId();
            var binaryFormatter = new BinaryFormatter();
            for (int i = 0; i < bufferArray.Length; i++)
            {
                DataPart dataPart = new DataPart() { Id = id, PartCount = bufferArray.Length, PartNum = i, Buffer = bufferArray[i] };
                byte[] dataPartArr;
                using (var memoryStream = new MemoryStream())
                {
                    binaryFormatter.Serialize(memoryStream, dataPart);
                    dataPartArr = memoryStream.ToArray();
                }
                client.Send(dataPartArr, dataPartArr.Length, remoteEP);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            timer1.Stop();
            client.Close();
        }
    }
}
