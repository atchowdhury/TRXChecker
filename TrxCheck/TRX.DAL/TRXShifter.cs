using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRX.DAO;

namespace TRX.DAL
{
    public class TRXShifter
    {
        public void ShiftTRX(Dictionary<int, Dictionary<int, List<TRXInfo>>> inputDictionary, List<TRXInfo> inputList)
        {
           // Telnet client=new Telnet();
            string possible="";
            string notPossible = "BSC,BCF,BTS,TRX,TGT PCM,TGT 1St TSL,LAPD,LAPD TSL,LAPD SSL,Feedback" + System.Environment.NewLine;
            string invalidTRX = "";

            string pcmDown = "BSC,BCF,BTS,TRX,TGT PCM,TGT 1St TSL,LAPD,LAPD TSL,LAPD SSL,Feedback" + System.Environment.NewLine;
            List<PCM> pcmListNew = new List<PCM>();
            List<PCMInfo> targetPCMList = new List<PCMInfo>();
            FetchDB fetchDb=new FetchDB();
            FetchPCM fetchPCM=new FetchPCM();
            List<TRXInfo> validInputList=new List<TRXInfo>();
            bool trxValid;
            foreach (TRXInfo trxInfo in inputList)
            {
                trxValid = fetchDb.CheckTRXValid(trxInfo.BSC, trxInfo.BCF, trxInfo.TRX);
                if (trxValid)
                {
                    validInputList.Add(trxInfo);
                }
                else
                {
                    invalidTRX += trxInfo.BSC + " " + trxInfo.BCF + " " + trxInfo.BTS + " " + trxInfo.TRX + " " + trxInfo.PCM + " " +
                            trxInfo.FirstTSL + " " + trxInfo.LAPD + " " + trxInfo.LapdTSL + " " + trxInfo.LapdSSL + System.Environment.NewLine;
                }
            }


            foreach (TRXInfo trxInfo in validInputList)
            {
                PCM aPcm = new PCM();
                aPcm.BSC = trxInfo.BSC;
                aPcm.PCMno = trxInfo.PCM;
                int index = pcmListNew.FindIndex(f => f.BSC == trxInfo.BSC && f.PCMno == trxInfo.PCM);
                if (index == -1)
                {
                    pcmListNew.Add(aPcm);
                    PCMInfo aPcmInfo = new PCMInfo();
                    aPcmInfo.BSC = aPcm.BSC;
                    aPcmInfo.PCMno = aPcm.PCMno;
                    aPcmInfo.PcmInfo = fetchDb.GetPCMInfo(aPcm.BSC, aPcm.PCMno);
                    targetPCMList.Add(aPcmInfo);
                }
            }


            foreach (TRXInfo info in validInputList)
            {
               // string[,] thisPCM = targetPCMList.Where(d => d.BSC == info.BSC && d.PCMno == info.PCM).First().PcmInfo;
                //int bRate = fetchDb.GetLAPDBitrate(info.BSC, info.LAPD);

                PCM aPcm = new PCM();

                aPcm =fetchDb.GetTRXPCM(info.BSC, info.BCF, info.TRX);
                if (aPcm != null) { 
                bool index = pcmListNew.Any(f => f.BSC == aPcm.BSC && f.PCMno == aPcm.PCMno);
                if (index)
                // if (pcmListNew.Contains(aPcm))
                {
                    string[,] thisPcmTgt =
                        targetPCMList.Where(d => d.BSC == aPcm.BSC && d.PCMno == aPcm.PCMno).First().PcmInfo;
                    int firstTSL = fetchDb.GetExistingPcmInfo(info.BSC, info.BCF, info.TRX);
                    for (int i = 0; i < 4; i++)
                    {
                        thisPcmTgt[i, firstTSL] = null;
                        thisPcmTgt[i, firstTSL + 1] = null;
                    }
                    LAPD aLapd = fetchDb.GetLAPDTSL(info.BSC, info.LAPD);
                    int bitRate = fetchDb.GetLAPDBitrate(info.BSC, info.LAPD);
                    if (bitRate < 64)
                    {
                        int step = bitRate/16;
                        thisPcmTgt[aLapd.subslot, aLapd.LapdTSL] = null;
                        for (int i = 0; i <= step - 1; i++)
                        {
                            thisPcmTgt[aLapd.subslot + i, aLapd.LapdTSL] = null;
                        }
                    }
                    //thisPCM1

                    targetPCMList.Where(d => d.BSC == info.BSC && d.PCMno == aPcm.PCMno).First().PcmInfo =
                        thisPcmTgt;
                }
            }
            }



            //foreach (TRXInfo info in validInputList)
            //{
            //    string[,] thisPcmTgt =
            //            targetPCMList.Where(d => d.BSC == info.BSC && d.PCMno == info.PCM).First().PcmInfo;
            //    int firstTSL = info.FirstTSL;
            //    bool avalShift = true;
            //    for (int i = 0; i < 4; i++)
            //    {
            //        if (thisPcmTgt[i, info.FirstTSL] != null) avalShift = false;
            //        if (thisPcmTgt[i, info.FirstTSL + 1] != null) avalShift = false;

            //    }

            //    int bitRate = fetchDb.GetLAPDBitrate(info.BSC, info.LAPD);
            //    if (bitRate < 64)
            //    {
            //        int step = bitRate / 16;
            //        if (thisPcmTgt[info.LapdSSL, info.LapdTSL] != null)
            //        {
            //            avalShift = false;
            //        }
            //        for (int i = 0; i <= step - 1; i++)
            //        {
            //            if (thisPcmTgt[info.LapdSSL + i, info.LapdTSL] != null)
            //            {
            //                avalShift = false;
            //            }
            //        }
            //    }
            //    if (avalShift)
            //    {
            //        for (int j = 0; j < 2; j++)
            //        {
            //            for (int i = 0; i < 4; i++)
            //            {
            //                thisPcmTgt[i, info.FirstTSL + j] = "BCF-" + info.BSC + "TRX-" + info.TRX;
            //            }

            //        }

            //        targetPCMList.Where(d => d.BSC == info.BSC && d.PCMno == info.PCM).First().PcmInfo = thisPcmTgt;

            //        possible += info.BSC + " " + info.BCF + " " + info.BTS + " " + info.TRX + " " + info.PCM + " " +
            //                   info.FirstTSL + " " + info.LAPD + " " + info.LapdTSL + " " + info.LapdSSL + "\n";
            //    }
            //    else
            //    {
            //        notPossible += info.BSC + " " + info.BCF + " " + info.BTS + " " + info.TRX + " " + info.PCM + " " +
            //                info.FirstTSL + " " + info.LAPD + " " + info.LapdTSL + " " + info.LapdSSL + "\n";
            //    }

            //  }

            Dictionary<int,List<int>> pcmStatusDictionary=new Dictionary<int, List<int>>();
            foreach (PCMInfo pcmInfo in targetPCMList)
            {
                if (pcmStatusDictionary.ContainsKey(pcmInfo.BSC))
                {
                   pcmStatusDictionary[pcmInfo.BSC].Add(pcmInfo.PCMno);
                   

                }
                else
                {
                    List<int> pcmList=new List<int>();
                    pcmList.Add(pcmInfo.PCMno);
                    pcmStatusDictionary.Add(pcmInfo.BSC,pcmList);
                }

                
            }


            //foreach (PCMInfo pcmInfo in targetPCMList)
            //{
            //    string circuitExist = "";
            //   bool pcmStatus=fetchPCM.GetPCMStatus(pcmInfo.BSC, pcmInfo.PCMno);
            //    //bool pcmStatus = true;

            //    List<TRXInfo> trxThisPCM =
            //        (List<TRXInfo>) validInputList.Where(d => d.BSC == pcmInfo.BSC && d.PCM == pcmInfo.PCMno).ToList();

            //    if (!pcmStatus)
            //    {
            //        foreach (TRXInfo info in trxThisPCM)
            //        {
            //            pcmDown += info.BSC + "," + info.BCF + "," + info.BTS + "," + info.TRX + "," + info.PCM +
            //                           "," +
            //                           info.FirstTSL + "," + info.LAPD + "," + info.LapdTSL + "," + info.LapdSSL + System.Environment.NewLine;
            //        }
            //    }
            //    else
            //    {

            //        foreach (TRXInfo info  in trxThisPCM)
            //        {

            //            string[,] thisPcmTgt =
            //                targetPCMList.Where(d => d.BSC == info.BSC && d.PCMno == info.PCM).First().PcmInfo;
            //            int firstTSL = info.FirstTSL;
            //            bool avalShift = true;
            //            for (int i = 0; i < 4; i++)
            //            {
            //                if (thisPcmTgt[i, info.FirstTSL] != null)
            //                {
            //                    avalShift = false;
            //                    if (circuitExist == "")
            //                    {
            //                        circuitExist += thisPcmTgt[i, info.FirstTSL];
            //                    }

            //                }
            //                if (thisPcmTgt[i, info.FirstTSL + 1] != null)
            //                {
            //                    avalShift = false;
            //                    if (circuitExist == "")
            //                    {
            //                        circuitExist += thisPcmTgt[i, info.FirstTSL + 1];
            //                    }
            //                }

            //            }

            //            int bitRate = fetchDb.GetLAPDBitrate(info.BSC, info.LAPD);
            //            if (bitRate < 64)
            //            {
            //                int step = bitRate/16;
            //                if (thisPcmTgt[info.LapdSSL, info.LapdTSL] != null)
            //                {
            //                    avalShift = false;
            //                    if (circuitExist == "")
            //                    {
            //                        circuitExist += thisPcmTgt[info.LapdSSL, info.LapdTSL];
            //                    }
            //                }
            //                for (int i = 0; i <= step - 1; i++)
            //                {
            //                    if (thisPcmTgt[info.LapdSSL + i, info.LapdTSL] != null)
            //                    {
            //                        avalShift = false;
            //                        if (circuitExist == "")
            //                        {

            //                            circuitExist += thisPcmTgt[info.LapdSSL + i, info.LapdTSL];
            //                        }
            //                    }
            //                }
            //            }
            //            if (avalShift)
            //            {
            //                for (int j = 0; j < 2; j++)
            //                {
            //                    for (int i = 0; i < 4; i++)
            //                    {
            //                        thisPcmTgt[i, info.FirstTSL + j] = "BCF-" + info.BSC + "TRX-" + info.TRX;
            //                    }

            //                }

            //                targetPCMList.Where(d => d.BSC == info.BSC && d.PCMno == info.PCM).First().PcmInfo =
            //                    thisPcmTgt;

            //                possible += info.BSC + " " + info.BCF + " " + info.BTS + " " + info.TRX + " " + info.PCM +
            //                            " " +
            //                            info.FirstTSL + " " + info.LAPD + " " + info.LapdTSL + " " + info.LapdSSL + System.Environment.NewLine;
            //            }
            //            else
            //            {
            //                notPossible += info.BSC + "," + info.BCF + "," + info.BTS + "," + info.TRX + "," + info.PCM +
            //                               "," +
            //                               info.FirstTSL + "," + info.LAPD + "," + info.LapdTSL + "," + info.LapdSSL + "," + circuitExist + "_Exist" + System.Environment.NewLine;
            //            }


            //        }
            //    }
            //}






            /////////////////////////////////////////////////////////////



            List<TRXInfo> validateTrxInfos=fetchPCM.GetAllPcmInStatus(validInputList, pcmStatusDictionary);
            foreach (TRXInfo trxInfo in validateTrxInfos)
            {
                string circuitExist = "";
                //bool pcmStatus = fetchPCM.GetPCMStatus(pcmInfo.BSC, pcmInfo.PCMno);
                bool pcmStatus = trxInfo.PCmStatus;

                List<TRXInfo> trxThisPCM =
                    (List<TRXInfo>)validInputList.Where(d => d.BSC == trxInfo.BSC && d.PCM == trxInfo.PCM).ToList();

                if (!pcmStatus)
                {
                    
                    
                        pcmDown += trxInfo.BSC + "," + trxInfo.BCF + "," + trxInfo.BTS + "," + trxInfo.TRX + "," + trxInfo.PCM +
                                       "," +
                                       trxInfo.FirstTSL + "," + trxInfo.LAPD + "," + trxInfo.LapdTSL + "," + trxInfo.LapdSSL + System.Environment.NewLine;
                    
                }
                else
                {

                   

                        string[,] thisPcmTgt =
                            targetPCMList.Where(d => d.BSC == trxInfo.BSC && d.PCMno == trxInfo.PCM).First().PcmInfo;
                        int firstTSL = trxInfo.FirstTSL;
                        bool avalShift = true;
                        for (int i = 0; i < 4; i++)
                        {
                            if (thisPcmTgt[i, trxInfo.FirstTSL] != null)
                            {
                                avalShift = false;
                                if (circuitExist == "")
                                {
                                    circuitExist += thisPcmTgt[i, trxInfo.FirstTSL];
                                }

                            }
                            if (thisPcmTgt[i, trxInfo.FirstTSL + 1] != null)
                            {
                                avalShift = false;
                                if (circuitExist == "")
                                {
                                    circuitExist += thisPcmTgt[i, trxInfo.FirstTSL + 1];
                                }
                            }

                        }

                        int bitRate = fetchDb.GetLAPDBitrate(trxInfo.BSC, trxInfo.LAPD);
                        if (bitRate < 64)
                        {
                            int step = bitRate / 16;
                            if (thisPcmTgt[trxInfo.LapdSSL, trxInfo.LapdTSL] != null)
                            {
                                avalShift = false;
                                if (circuitExist == "")
                                {
                                    circuitExist += thisPcmTgt[trxInfo.LapdSSL, trxInfo.LapdTSL];
                                }
                            }
                            for (int i = 0; i <= step - 1; i++)
                            {
                                if (thisPcmTgt[trxInfo.LapdSSL + i, trxInfo.LapdTSL] != null)
                                {
                                    avalShift = false;
                                    if (circuitExist == "")
                                    {

                                        circuitExist += thisPcmTgt[trxInfo.LapdSSL + i, trxInfo.LapdTSL];
                                    }
                                }
                            }
                        }
                        if (avalShift)
                        {
                            for (int j = 0; j < 2; j++)
                            {
                                for (int i = 0; i < 4; i++)
                                {
                                    thisPcmTgt[i, trxInfo.FirstTSL + j] = "BCF-" + trxInfo.BSC + "TRX-" + trxInfo.TRX;
                                }

                            }

                            targetPCMList.Where(d => d.BSC == trxInfo.BSC && d.PCMno == trxInfo.PCM).First().PcmInfo =
                                thisPcmTgt;

                            possible += trxInfo.BSC + " " + trxInfo.BCF + " " + trxInfo.BTS + " " + trxInfo.TRX + " " + trxInfo.PCM +
                                        " " +
                                        trxInfo.FirstTSL + " " + trxInfo.LAPD + " " + trxInfo.LapdTSL + " " + trxInfo.LapdSSL + System.Environment.NewLine;
                        }
                        else
                        {
                            notPossible += trxInfo.BSC + "," + trxInfo.BCF + "," + trxInfo.BTS + "," + trxInfo.TRX + "," + trxInfo.PCM +
                                           "," +
                                           trxInfo.FirstTSL + "," + trxInfo.LAPD + "," + trxInfo.LapdTSL + "," + trxInfo.LapdSSL + "," + circuitExist + "_Exist" + System.Environment.NewLine;
                        }


                    
                }
            }








            System.IO.File.WriteAllText(@"D:\input\Possible.txt", possible);
            System.IO.File.WriteAllText(@"D:\input\NotPossible.csv", notPossible);
            System.IO.File.WriteAllText(@"D:\input\InvalidTRX.txt", invalidTRX);
            System.IO.File.WriteAllText(@"D:\input\DwonPCM.csv", pcmDown);


        }
    }
}
