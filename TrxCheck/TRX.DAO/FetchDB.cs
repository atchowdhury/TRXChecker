using System;
using System.Collections.Generic;
//using System.Data.SqlClient;
using System.Data.Odbc;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using TRX.DAO;



namespace TRX.DAL
{
    public class FetchDB
    {
        string server = "localhost";
        string database = "myflexml";
        string uid = "root";
       string  password = "";
        

        // string connectionStr = ConfigurationManager.ConnectionStrings["connectionStringForEmployeDB"].ConnectionString;
        string connectionStr = string.Format("Server=localhost; database={0}; UID=root;", "myflexml");
        private MySqlConnection aSqlConnection;
        private MySqlCommand aSqlCommand;
        public FetchDB()
        {
            string connectionString = "SERVER=" + server + ";" + "DATABASE=" +
                database + ";" + "UID=" + uid + ";" + "Pwd=" + password + "; Port=3306;";

           string conn="Server = localhost;  Database = myflexml; Uid = root;Pwd = '';";
            //aSqlConnection = new SqlConnection(connectionStr);
           aSqlConnection = new MySqlConnection(conn);

        }

        public string[,] GetPCMInfo(int bsc, int pcmNo)
        {
            string[,] pcmAllocation = new string[4, 32];
            string qCSDAP = string.Format("select bsc,csdap,pcmCircuit_ID,firstTSL,lastTSL from csdap where bsc={0} and pcmCircuit_ID={1}", bsc, pcmNo);
            string qTRX = string.Format("select bsc,bcf,bts,trx,channel0Tsl,channel0PCM from trx where bsc={0} and channel0PCM={1}", bsc, pcmNo);
            string qDAP = string.Format("select bsc,dap,pcmCircuit_ID,firstTSL,lastTSL from dap where bsc={0} and pcmCircuit_ID={1}", bsc, pcmNo);
            string qLAPD = string.Format("SELECT lapd.bsc,name,abissigchannelTimeslotPcm,abissigchannelTimeslotTSL,abissigchannelSubSlot,bitrate,lapd.logicalbcsuAddress,tei,sapi FROM lapd where lapd.bsc={0} and abissigchannelTimeslotPcm={1}", bsc, pcmNo);
            aSqlConnection.Open();
            aSqlCommand = new MySqlCommand(qCSDAP, aSqlConnection);
            MySqlDataReader aDataReaderCSDAP = aSqlCommand.ExecuteReader();

            List<DynamicAbisPool> abisPoolList = new List<DynamicAbisPool>();
            List<DynamicAbisPool> csdapPoolList = new List<DynamicAbisPool>();

            while (aDataReaderCSDAP.Read())
            {
                DynamicAbisPool abisPool = new DynamicAbisPool();
                abisPool.BSC = Convert.ToInt32(aDataReaderCSDAP["bsc"]);
                abisPool.PoolID = Convert.ToInt32(aDataReaderCSDAP["csdap"]);
                abisPool.PCM = Convert.ToInt32(aDataReaderCSDAP["pcmCircuit_ID"]);
                abisPool.FirstTSL = Convert.ToInt32(aDataReaderCSDAP["firstTSL"]);
                abisPool.LastTSL = Convert.ToInt32(aDataReaderCSDAP["lastTSL"]);
                csdapPoolList.Add(abisPool);
            }
            aDataReaderCSDAP.Close();


            aSqlCommand = new MySqlCommand(qDAP, aSqlConnection);
            MySqlDataReader aDataReaderDAP = aSqlCommand.ExecuteReader();


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
                    pcmAllocation[0, i] = "dap_"+pool.PoolID.ToString();
                    pcmAllocation[1, i] = "dap_"+pool.PoolID.ToString();
                    pcmAllocation[2, i] = "dap_"+pool.PoolID.ToString();
                    pcmAllocation[3, i] = "dap_"+pool.PoolID.ToString();
                }


            }

            foreach (DynamicAbisPool pool in csdapPoolList)
            {
                for (int i = pool.FirstTSL; i <= pool.LastTSL; i++)
                {
                    pcmAllocation[0, i] = "CSDAP_" + pool.PoolID.ToString();
                    pcmAllocation[1, i] = "CSDAP_" + pool.PoolID.ToString();
                    pcmAllocation[2, i] = "CSDAP_" + pool.PoolID.ToString();
                    pcmAllocation[3, i] = "CSDAP_" + pool.PoolID.ToString();
                }


            }



            aSqlCommand = new MySqlCommand(qLAPD, aSqlConnection);
            MySqlDataReader aDataReaderLAPD = aSqlCommand.ExecuteReader();
            List<LAPD> lapdList = new List<LAPD>();


            while (aDataReaderLAPD.Read())
            {
                LAPD aLapd = new LAPD();
                aLapd.BSC = Convert.ToInt32(aDataReaderLAPD["bsc"]);
                aLapd.PCM = Convert.ToInt32(aDataReaderLAPD["abissigchannelTimeslotPcm"]);
                aLapd.LapdTSL = Convert.ToInt32(aDataReaderLAPD["abissigchannelTimeslotTSL"]);
                string aa = aDataReaderLAPD["abissigchannelSubSlot"].ToString();
                if (aa != "")
                {
                    aLapd.subslot = Convert.ToInt32(aDataReaderLAPD["abissigchannelSubSlot"]);
                }
                aLapd.Bitrate = Convert.ToInt32(aDataReaderLAPD["bitrate"]);
                aLapd.SAPI = Convert.ToInt32(aDataReaderLAPD["sapi"]);
                aLapd.Tei = Convert.ToInt32(aDataReaderLAPD["tei"]);
                lapdList.Add(aLapd);
            }
            aDataReaderLAPD.Close();

            foreach (LAPD lapd in lapdList)
            {
                string content = "";
                if (lapd.SAPI == 62)
                {
                    content = "OMU-SIG";
                }
                else
                {
                    content = "TRXSig-" + lapd.Tei;
                }

                if (lapd.Bitrate == 32)
                {
                   
                   
                    pcmAllocation[lapd.subslot, lapd.LapdTSL] = content;
                    pcmAllocation[lapd.subslot + 1, lapd.LapdTSL] = content;

                }
                else if (lapd.Bitrate == 64)
                {
                    pcmAllocation[0, lapd.LapdTSL] = content;
                    pcmAllocation[1, lapd.LapdTSL] = content;
                    pcmAllocation[2, lapd.LapdTSL] = content;
                    pcmAllocation[3, lapd.LapdTSL] = content;

                }
                else if (lapd.Bitrate == 16)
                {
                    pcmAllocation[lapd.subslot, lapd.LapdTSL] = content;
                }


            }





            aSqlCommand = new MySqlCommand(qTRX, aSqlConnection);
            MySqlDataReader aDataReaderTRX = aSqlCommand.ExecuteReader();
            List<TRXInfo> aTrxInfosList = new List<TRXInfo>();

            while (aDataReaderTRX.Read())
            {
                TRXInfo aTRXInfo = new TRXInfo();
                aTRXInfo.BSC = Convert.ToInt32(aDataReaderTRX["bsc"]);
                aTRXInfo.BCF = Convert.ToInt32(aDataReaderTRX["bcf"]);
                aTRXInfo.TRX = Convert.ToInt32(aDataReaderTRX["trx"]);
                aTRXInfo.PCM = Convert.ToInt32(aDataReaderTRX["channel0PCM"]);
                aTRXInfo.FirstTSL = Convert.ToInt32(aDataReaderTRX["channel0Tsl"]);

                aTrxInfosList.Add(aTRXInfo);
            }
            aDataReaderTRX.Close();

            foreach (TRXInfo info in aTrxInfosList)
            {
                pcmAllocation[0, info.FirstTSL] ="BCF"+info.BCF+ "-TRX-"+info.TRX;
                pcmAllocation[1, info.FirstTSL] = "BCF" + info.BCF + "-TRX-" + info.TRX;
                pcmAllocation[2, info.FirstTSL] = "BCF" + info.BCF + "-TRX-" + info.TRX;
                pcmAllocation[3, info.FirstTSL] = "BCF" + info.BCF + "-TRX-" + info.TRX;


                pcmAllocation[0, info.FirstTSL + 1] = "BCF" + info.BCF + "-TRX-" + info.TRX;
                pcmAllocation[1, info.FirstTSL + 1] = "BCF" + info.BCF + "-TRX-" + info.TRX;
                pcmAllocation[2, info.FirstTSL + 1] = "BCF" + info.BCF + "-TRX-" + info.TRX;
                pcmAllocation[3, info.FirstTSL + 1] = "BCF" + info.BCF + "-TRX-" + info.TRX;
            }



            aSqlConnection.Close();

            return pcmAllocation;

        }

        public int GetLAPDBitrate(int bsc, string lapd)
        {
            string qLapd = string.Format("select bsc,lapd,bitrate from lapd where bsc={0} and name='{1}'", bsc, lapd);
            aSqlConnection.Open();
            aSqlCommand = new MySqlCommand(qLapd, aSqlConnection);
            MySqlDataReader aDataReaderLAPD = aSqlCommand.ExecuteReader();
            List<LAPD> lapdList = new List<LAPD>();

            while (aDataReaderLAPD.Read())
            {
                LAPD aLapd = new LAPD();
                aLapd.BSC = Convert.ToInt32(aDataReaderLAPD["bsc"]);
                aLapd.Bitrate = Convert.ToInt32(aDataReaderLAPD["bitrate"]);

                lapdList.Add(aLapd);
            }
            
            aDataReaderLAPD.Close();
            aSqlConnection.Close();
            return lapdList.First().Bitrate;
        }

        public LAPD GetLAPDTSL(int bsc, string lapd)
        {
            string qLapd = string.Format("select bsc,lapd,bitrate,abisSigChannelTimeSlotTsl,abisSigChannelSubSlot from lapd where bsc={0} and name='{1}'", bsc, lapd);
            aSqlConnection.Open();
            aSqlCommand = new MySqlCommand(qLapd, aSqlConnection);
            MySqlDataReader aDataReaderLAPD = aSqlCommand.ExecuteReader();
            List<LAPD> lapdList = new List<LAPD>();

            while (aDataReaderLAPD.Read())
            {
                LAPD aLapd = new LAPD();
                aLapd.BSC = Convert.ToInt32(aDataReaderLAPD["bsc"]);
                aLapd.Bitrate = Convert.ToInt32(aDataReaderLAPD["bitrate"]);
                aLapd.LapdTSL = Convert.ToInt32(aDataReaderLAPD["abisSigChannelTimeSlotTsl"]);
                aLapd.subslot = Convert.ToInt32(aDataReaderLAPD["abisSigChannelSubSlot"]);

                lapdList.Add(aLapd);
            }

            aDataReaderLAPD.Close();
            aSqlConnection.Close();
            return lapdList.First();
        }



        public PCM GetTRXPCM(int bsc, int bcf, int trx )
        {
            string qLapd = string.Format("select bsc,bcf,bts,trx,channel0PCM from trx where bsc={0} and bcf={1} and trx={2}", bsc, bcf,trx);
            aSqlConnection.Open();
            aSqlCommand = new MySqlCommand(qLapd, aSqlConnection);
            MySqlDataReader aDataReaderLAPD = aSqlCommand.ExecuteReader();
            List<PCM> pcmList = new List<PCM>();

            while (aDataReaderLAPD.Read())
            {
                PCM aPCM = new PCM();
                aPCM.BSC = Convert.ToInt32(aDataReaderLAPD["bsc"]);
                aPCM.PCMno = Convert.ToInt32(aDataReaderLAPD["channel0PCM"]);

                pcmList.Add(aPCM);
            }

            aDataReaderLAPD.Close();
            aSqlConnection.Close();
            
                return pcmList.First();
            
            
        }


        public int GetExistingPcmInfo(int bsc, int bcf, int trx)
        {
            string qLapd = string.Format("select bsc,bcf,bts,trx,channel0PCM,channel0Tsl from trx where bsc={0} and bcf={1} and trx={2}", bsc, bcf, trx);
            aSqlConnection.Open();
            aSqlCommand = new MySqlCommand(qLapd, aSqlConnection);
            MySqlDataReader aDataReaderLAPD = aSqlCommand.ExecuteReader();
            List<PCM> pcmList = new List<PCM>();
            int firstTSL = 0;
            while (aDataReaderLAPD.Read())
            {
                firstTSL = Convert.ToInt32(aDataReaderLAPD["channel0Tsl"]);
            }

            aDataReaderLAPD.Close();
            aSqlConnection.Close();
            return firstTSL;
        }

        public bool CheckTRXValid(int bsc,int bcf, int trx)
        {
            bool valid = false;
            string qLapd = string.Format("select count(*) from trx where bsc={0} and bcf={1} and trx={2}", bsc, bcf, trx);
            aSqlConnection.Open();
            aSqlCommand = new MySqlCommand(qLapd, aSqlConnection);
            //int count = aSqlCommand.ExecuteNonQuery();
            MySqlDataReader aDataReader  =aSqlCommand.ExecuteReader();
            int count=0;

            while (aDataReader.Read())
            {
                count = Convert.ToInt32(aDataReader[0]);
            }
            //aDataReaderLAPD.Close();
            aSqlConnection.Close();
            if (count > 0)
            {
                valid = true;
            }
            return valid;
        }
    }
}
