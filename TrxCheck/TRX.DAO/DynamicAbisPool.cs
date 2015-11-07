using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRX.DAO
{
    public class DynamicAbisPool
    {
        public int BSC { set; get; }
        public int PoolID { get; set; }
        public int PCM { get; set; }
        public int FirstTSL { get; set; }
        public int LastTSL { get; set; }
    }
}
