using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRX.DAO;

namespace TRX.DAL
{
    public class DBGateway
    {
       // string connectionStr = ConfigurationManager.ConnectionStrings["connectionStringForEmployeDB"].ConnectionString;
        string connectionStr = string.Format("Server=localhost; database={0}; UID=root; password=''", "myflexml");
        private SqlConnection aSqlConnection;
        private SqlCommand aSqlCommand;
        public DBGateway()
        {
            aSqlConnection = new SqlConnection(connectionStr);
            
        }

        public int[,] GetPCMInfo(int bsc,int pcmNo)
        {
            int[,] pcmAllocation=new int[4,31];
            string qCSDAP = string.Format("select bsc,csdap,pcmCircuit_ID,firstTSL,lastTSL from csdap where bsc={0} and pcmCircuit_ID={1}", bsc, pcmNo);
            string qTRX =string.Format("select bsc,bcf,bts,trx,channel0Tsl,channel0PCM from trx where bsc={0} and channel0PCM={1}",bsc,pcmNo);
            string qDAP =string.Format("select bsc,dap,pcmCircuit_ID,firstTSL,lastTSL from dap where bsc={0} and pcmCircuit_ID={1}", bsc,pcmNo);
            string qLAPD =string.Format("SELECT lapd.bsc,name,abissigchannelTimeslotPcm,abissigchannelTimeslotTSL,abissigchannelSubSlot,bitrate,lapd.logicalbcsuAddress FROM lapd where lapd.bsc={0} and abissigchannelTimeslotPcm={1}",bsc, pcmNo);
            aSqlConnection.Open();
            aSqlCommand = new SqlCommand(qCSDAP, aSqlConnection);
            SqlDataReader aDataReaderCSDAP = aSqlCommand.ExecuteReader();

            List<DynamicAbisPool> abisPoolList = new List<DynamicAbisPool>();

            while (aDataReaderCSDAP.Read())
            {
                DynamicAbisPool abisPool = new DynamicAbisPool();
                abisPool.BSC = Convert.ToInt32(aDataReaderCSDAP["bsc"]);
                abisPool.PoolID = Convert.ToInt32(aDataReaderCSDAP["csdap"]);
                abisPool.PCM = Convert.ToInt32(aDataReaderCSDAP["pcmCircuit_ID"]);
                abisPool.FirstTSL = Convert.ToInt32(aDataReaderCSDAP["firstTSL"]);
                abisPool.LastTSL = Convert.ToInt32(aDataReaderCSDAP["lastTSL"]);
                abisPoolList.Add(abisPool);
            }
            aDataReaderCSDAP.Close();


            aSqlCommand = new SqlCommand(qDAP, aSqlConnection);
            SqlDataReader aDataReaderDAP = aSqlCommand.ExecuteReader();


            while (aDataReaderDAP.Read())
            {
                DynamicAbisPool abisPool = new DynamicAbisPool();
                abisPool.BSC = Convert.ToInt32(aDataReaderDAP["bsc"]);
                abisPool.PoolID = Convert.ToInt32(aDataReaderDAP["dap"]);
                abisPool.PCM = Convert.ToInt32(aDataReaderDAP["pcmCircuit_ID"]);
                abisPool.FirstTSL = Convert.ToInt32(aDataReaderDAP["firstTSL"]);
                abisPool.LastTSL = Convert.ToInt32(aDataReaderDAP["lastTSL"]);
                abisPoolList.Add(abisPool);
            }
            aDataReaderDAP.Close();

            foreach (DynamicAbisPool pool in abisPoolList)
            {
                for (int i = pool.FirstTSL; i <= pool.LastTSL; i++)
                {
                    pcmAllocation[0,i] = 1;
                    pcmAllocation[1, i] = 1;
                    pcmAllocation[2, i] = 1;
                    pcmAllocation[3, i] = 1;
                }


            }



            aSqlCommand = new SqlCommand(qLAPD, aSqlConnection);
            SqlDataReader aDataReaderLAPD = aSqlCommand.ExecuteReader();
            List<LAPD> lapdList=new List<LAPD>();


            while (aDataReaderLAPD.Read())
            {
                LAPD aLapd=new LAPD();
                aLapd.BSC = Convert.ToInt32(aDataReaderLAPD["bsc"]);
                aLapd.PCM = Convert.ToInt32(aDataReaderLAPD["abissigchannelTimeslotPcm"]);
                aLapd.LapdTSL = Convert.ToInt32(aDataReaderLAPD["abissigchannelTimeslotTSL"]);
                if (aDataReaderLAPD["abissigchannelSubSlot"] != null)
                {
                    aLapd.subslot = Convert.ToInt32(aDataReaderLAPD["abissigchannelSubSlot"]);
                }
                aLapd.Bitrate = Convert.ToInt32(aDataReaderLAPD["bitrate"]);
                lapdList.Add(aLapd);
            }
            aDataReaderLAPD.Close();

            foreach (LAPD lapd in lapdList)
            {
                if (lapd.Bitrate == 32)
                {
                    pcmAllocation[lapd.subslot, lapd.LapdTSL] = 1;
                    pcmAllocation[lapd.subslot+1, lapd.LapdTSL] = 1;
                }
                else if (lapd.Bitrate == 64)
                {
                    pcmAllocation[0, lapd.LapdTSL] = 1;
                    pcmAllocation[1, lapd.LapdTSL] = 1;
                    pcmAllocation[2, lapd.LapdTSL] = 1;
                    pcmAllocation[3, lapd.LapdTSL] = 1;

                }
                else if (lapd.Bitrate == 16)
                {
                    pcmAllocation[lapd.subslot, lapd.LapdTSL] = 1;
                }


            }





            aSqlCommand = new SqlCommand(qTRX, aSqlConnection);
            SqlDataReader aDataReaderTRX = aSqlCommand.ExecuteReader();
            List<TRXInfo> aTrxInfosList=new List<TRXInfo>();

            while (aDataReaderDAP.Read())
            {
                TRXInfo aTRXInfo=new TRXInfo();
                aTRXInfo.BSC = Convert.ToInt32(aDataReaderTRX["bsc"]);
                aTRXInfo.BCF = Convert.ToInt32(aDataReaderTRX["bcf"]);
                aTRXInfo.PCM = Convert.ToInt32(aDataReaderTRX["channel0PCM"]);
                aTRXInfo.FirstTSL = Convert.ToInt32(aDataReaderTRX["channel0Tsl"]);

                aTrxInfosList.Add(aTRXInfo);
            }
            aDataReaderTRX.Close();

            foreach (TRXInfo info in aTrxInfosList)
            {
                pcmAllocation[0, info.FirstTSL] = 1;
                pcmAllocation[1, info.FirstTSL] = 1;
                pcmAllocation[2, info.FirstTSL] = 1;
                pcmAllocation[3, info.FirstTSL] = 1;


                pcmAllocation[0, info.FirstTSL+1] = 1;
                pcmAllocation[1, info.FirstTSL+1] = 1;
                pcmAllocation[2, info.FirstTSL+1] = 1;
                pcmAllocation[3, info.FirstTSL+1] = 1;
            }



            aSqlConnection.Close();

            return pcmAllocation;

        }


       
    }
}
