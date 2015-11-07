using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRX.DAO
{
   public  class PCMInfo
    {
        public int BSC { set; get; }
        public int PCMno { get; set; }
        public string[,] PcmInfo { set; get; }
        public bool PCmStatus { set; get; }
        
    }
}
