using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using PrtScr_Server;

namespace PrtScr_Client
{
    public partial class Form1 : Form
    {
        UdpClient client;
        IPAddress groupAddress; // Адрес який буде використовуватись для групової рослилки
        int localPort;
        int ttl;  // час життя пакету
        bool isRunning = true;
        public Form1()
        {
            InitializeComponent();
            groupAddress = IPAddress.Parse("234.5.5.77");
            localPort = 7778;
            ttl = 32;
            client = new UdpClient(localPort);
            client.JoinMulticastGroup(groupAddress, ttl);
            Thread thread = new Thread(ListenThread);
            thread.IsBackground = true;
            thread.Start();
            
        }

        private void ListenThread()
        {
            List<DataPart> dataParts = new List<DataPart>();
            while (isRunning)
            {
                IPEndPoint iPEndPoint = null;
                byte[] buff = client.Receive(ref iPEndPoint);
                var bf = new BinaryFormatter();
                DataPart dataPart;
                using (var ms = new MemoryStream(buff))
                {
                    dataPart = bf.Deserialize(ms) as DataPart;
                }
                if(dataParts.Count == 0)
                    dataParts.Add(dataPart);
                else if(dataParts[0].Id == dataPart.Id)
                    dataParts.Add(dataPart);
                else
                {
                    if (dataParts.Count == dataPart.PartCount)
                    {
                        dataParts = dataParts.OrderBy(d => d.PartNum).ToList();
                        byte[] data = dataParts[0].Buffer;
                        for (int i = 1; i < dataParts.Count; i++)
                            data = data.Concat(dataParts[i].Buffer).ToArray();
                        Bitmap bitmap;
                        using (var ms = new MemoryStream(data))
                        {
                            bitmap = new Bitmap(ms);
                        }
                        pictureBox1.Invoke(new Action(() => pictureBox1.Image = bitmap));
                    }
                    dataParts.Clear();
                    dataParts.Add(dataPart);
                }
            }
                
        }
        
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            isRunning = false;
            client.Close();
        }
    }
}
