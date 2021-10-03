using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PrtScr_Server
{
    [Serializable]
    public class DataPart
    {
        public string Id { get; set; }
        public int PartCount { get; set; }
        public int PartNum { get; set; }
        public byte[] Buffer { get; set; }
    }
}
