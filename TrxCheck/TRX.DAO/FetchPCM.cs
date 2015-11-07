using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using MinimalisticTelnet;

namespace TRX.DAO
{
    public class FetchPCM
    {
        public bool GetPCMStatus(int bsc, int pcm)
        {
            //string line;
            string bscMap;
            BscMap aMap=new BscMap();
            bool pcmUP = false;
            bool connected;

            System.IO.StreamReader ipMap=new StreamReader(@"D:\input\ip_map.csv");

            while ((bscMap = ipMap.ReadLine()) != null)
            {

                if (bscMap.Contains(bsc.ToString()))
                {
                    string[] map=bscMap.Split(',');
                    aMap.Name = map[1];
                    aMap.IPaddress = map[2];
                    aMap.BSCuser = map[3];
                    aMap.BSCpass = map[4];
                }
            }
            try
            {
                TelnetConnection aTelnetConnection = new TelnetConnection(aMap.IPaddress, 23);
                aTelnetConnection.Login(aMap.BSCuser, aMap.BSCpass, 100, out connected);
                string output = aTelnetConnection.ExecuteCommand("ZDTI:::PCM=" + pcm + ";");

                if (output.Contains("WO-EX"))
                {
                    pcmUP = true;
                  
                    //break;
                }
            }
            catch
            {
                pcmUP = false;}
            //string[] outputArray = output.Split('\n');
            //foreach (string s in outputArray)
            //{
            //    if (s.Contains("NETW"))
            //    {
            //        s.Remove('"');
            //        s.Remove(' ');
            //      //  string[] aLAPD = s.Split();
            //        if (s.Contains("WO-EX"))
            //        {
            //            pcmUP = true;
            //            break;
            //        }
            //    }
            //}

            //System.IO.StreamReader file =
            //    new System.IO.StreamReader(output);
            //while ((line = file.ReadLine()) != null)
            //{
            //    if (line.Contains("NETW"))
            //    {
            //        string [] aLAPD=line.Split();
            //        if (aLAPD[6] == "WO")
            //        {
            //            pcmUP = true;
            //            break;
            //        }
            //    }
            //}
            return pcmUP;

        }

        public List<TRXInfo> GetAllPcmInStatus(List<TRXInfo> trxThisPCM,
            Dictionary<int, List<int>> pcmStatusDictionary)
        {
            BscMap aMap = new BscMap();
            string bscMap;
            bool connected;

            List<PCMInfo> apcmList = new List<PCMInfo>();

            foreach (KeyValuePair<int, List<int>> pcmInfo in pcmStatusDictionary)
            {

                System.IO.StreamReader ipMap = new StreamReader(@"D:\input\ip_map.csv");
                while ((bscMap = ipMap.ReadLine()) != null)
                {

                    if (bscMap.Contains(pcmInfo.Key.ToString()))
                    {
                        string[] map = bscMap.Split(',');
                        aMap.Name = map[1];
                        aMap.IPaddress = map[2];
                        aMap.BSCuser = map[3];
                        aMap.BSCpass = map[4];

                        ipMap.Close();
                        break;

                    }


                }
                TelnetConnection aTelnetConnection = new TelnetConnection(aMap.IPaddress, 23);
                aTelnetConnection.Login(aMap.BSCuser, aMap.BSCpass, 100, out connected);
                foreach (int eachPCM in pcmInfo.Value)
                {
                    string output = aTelnetConnection.ExecuteCommand("ZDTI:::PCM=" + eachPCM + ";");

                    if (output.Contains("WO-EX"))
                    {
                        //pcmUP = true;
                        PCMInfo aPcmInfo = new PCMInfo();
                        aPcmInfo.BSC = pcmInfo.Key;
                        aPcmInfo.PCMno = eachPCM;
                        aPcmInfo.PCmStatus = true;
                        apcmList.Add(aPcmInfo);


                        //break;
                    }
                    else
                    {
                        PCMInfo aPcmInfo = new PCMInfo();
                        aPcmInfo.BSC = pcmInfo.Key;
                        aPcmInfo.PCMno = eachPCM;
                        aPcmInfo.PCmStatus = false;
                        apcmList.Add(aPcmInfo);

                    }
                }
                aTelnetConnection.ExecuteCommand("Z;");


    

    

                

               

                
             

            }

             foreach (TRXInfo aTrxInfo in trxThisPCM)
                {
                    aTrxInfo.PCmStatus =
                        apcmList.Where(d => d.BSC == aTrxInfo.BSC && d.PCMno == aTrxInfo.PCM).First().PCmStatus;

                }

            return trxThisPCM;
        }

    } 

    }

