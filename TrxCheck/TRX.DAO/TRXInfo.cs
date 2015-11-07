using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRX.DAO
{
   public class TRXInfo
    {
        public int BSC { set; get; }
        public int BCF { set; get; }
        public int BTS { set; get; }
        public int TRX { get; set; }
        public int PCM  { get; set; }
        public int FirstTSL { set; get; }
        public string LAPD { set; get; }
        public int  LapdTSL { set; get; }
        public int LapdSSL { set; get; }
        public bool PCmStatus { set; get; }
    }
}
