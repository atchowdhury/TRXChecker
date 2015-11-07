using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRX.DAO
{
   // lapd.bsc,name,abissigchannelTimeslotPcm,abissigchannelTimeslotTSL,abissigchannelSubSlot,bitrate,lapd.logicalbcsuAddress
   public class LAPD
    {
       public int BSC { set; get; }
       public int PCM { get; set; }
       public int Bitrate { set; get; }
       public int LapdTSL { set; get; }
       public int subslot { set; get; }
       public int Tei { set; get; }
       public int SAPI { set; get; }
       public int TRX { set; get; }
   }
}
