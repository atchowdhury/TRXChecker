using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRX.DAO;
namespace TRX.DAL
{
    public class TrxManager
    {
        public void CheckTRX(Dictionary<int, Dictionary<int, List<TRXInfo>>> inputDictionary,List<TRXInfo> inputList )
        {
            string possibleShift = string.Empty;
            string impossibleShift = string.Empty;
            List<PCM> pcmListOld = new List<PCM>();
            List<PCM> pcmListNew=new List<PCM>();

            List<PCMInfo> existingPCMList = new List<PCMInfo>();
            List<PCMInfo> targetPCMList = new List<PCMInfo>();
            FetchDB fetchDb = new FetchDB();
            foreach (TRXInfo trxInfo in inputList)
            {
                PCM aPcm=new PCM();
                aPcm.BSC = trxInfo.BSC;
                aPcm.PCMno = trxInfo.PCM;
                int index = pcmListNew.FindIndex(f => f.BSC == trxInfo.BSC && f.PCMno == trxInfo.PCM);
                if (index==-1)
                {
                    pcmListNew.Add(aPcm);
                    PCMInfo aPcmInfo=new PCMInfo();
                    aPcmInfo.BSC = aPcm.BSC;
                    aPcmInfo.PCMno = aPcm.PCMno;
                    aPcmInfo.PcmInfo = fetchDb.GetPCMInfo(aPcm.BSC, aPcm.PCMno);
                    targetPCMList.Add(aPcmInfo);
                }
            }

            foreach (TRXInfo trxInfo in inputList)
            {
                
                PCM aPcm = new PCM();

                aPcm = fetchDb.GetTRXPCM(trxInfo.BSC, trxInfo.BCF, trxInfo.TRX);


                if (!pcmListOld.Contains(aPcm))
                {
                    pcmListOld.Add(aPcm);
                    PCMInfo aPcmInfo = new PCMInfo();
                    aPcmInfo.BSC = aPcm.BSC;
                    aPcmInfo.PCMno = aPcm.PCMno;
                    aPcmInfo.PcmInfo = fetchDb.GetPCMInfo(aPcm.BSC, aPcm.PCMno);
                    existingPCMList.Add(aPcmInfo);
                }
            }

            foreach (TRXInfo info in inputList)
            {
                string[,] thisPCM = targetPCMList.Where(d => d.BSC == info.BSC && d.PCMno == info.PCM).First().PcmInfo;
                int bRate=fetchDb.GetLAPDBitrate(info.BSC, info.LAPD);
                bool avalShift = true;
                for (int i = 0; i < 4; i++)
                {
                    if (thisPCM[i, info.FirstTSL] != null) avalShift = false;
                    if (thisPCM[i, info.FirstTSL+1] != null) avalShift = false;

                }
                if (bRate == 32)
                {
                    if (avalShift && thisPCM[info.LapdSSL, info.LapdTSL] == null)
                    {
                        PCM aPcm = new PCM();

                        aPcm = fetchDb.GetTRXPCM(info.BSC, info.BCF, info.TRX);
                        int bsc = aPcm.BSC;
                        int pcm = aPcm.PCMno;
                        //aPcmInfo.PcmInfo = fetchDb.GetPCMInfo(info.BSC, info.PCM);
                        bool index = pcmListNew.Any(f => f.BSC == bsc && f.PCMno == pcm);
                        if (index)
                       // if (pcmListNew.Contains(aPcm))
                        {
                            string[,] thisPCMold =
                                targetPCMList.Where(d => d.BSC == aPcm.BSC && d.PCMno == aPcm.PCMno).First().PcmInfo;
                            int firstTSL = fetchDb.GetExistingPcmInfo(info.BSC, info.BCF, info.TRX);
                            for (int i = 0; i < 4; i++)
                            {
                                thisPCMold[i, firstTSL] = null;
                                thisPCMold[i, firstTSL + 1] = null;
                            }
                            LAPD aLapd = fetchDb.GetLAPDTSL(info.BSC, info.LAPD);
                            thisPCMold[aLapd.subslot, aLapd.LapdTSL] = null;
                            //thisPCM1

                            targetPCMList.Where(d => d.BSC == info.BSC && d.PCMno == info.PCM).First().PcmInfo =
                                thisPCMold;
                        }

                        string[,] thisPCMNew =targetPCMList.Where(d => d.BSC == aPcm.BSC && d.PCMno == aPcm.PCMno).First().PcmInfo;
                        int firstTsl = info.FirstTSL;
                        for (int i = 0; i < 4; i++)
                        {
                            thisPCMNew[i, firstTsl] = null;
                            thisPCMNew[i, firstTsl + 1] = null;
                        }
                        
                        thisPCMNew[info.LapdSSL, info.LapdTSL] = null;
                        //thisPCM1

                        targetPCMList.Where(d => d.BSC == info.BSC && d.PCMno == info.PCM).First().PcmInfo =
                            thisPCMNew;


                    }
                    else
                    {
                        
                    }
                }


                // targetPCMList.Where(d => d.BSC == info.BSC && d.PCMno==info.PCM).First().PcmInfo = existingPCMList.First().PcmInfo;
            }






            foreach (KeyValuePair<int, Dictionary<int, List<TRXInfo>>> valuePair in inputDictionary)
            {
                int bsc = valuePair.Key;
                foreach (KeyValuePair<int,List<TRXInfo>> VARIABLE in valuePair.Value)
                {
                    int pcm = VARIABLE.Key;
                    FetchDB aFetchDb = new FetchDB();
                    string[,] pcmInfo=aFetchDb.GetPCMInfo(bsc, pcm);

                    foreach (TRXInfo trxInfo in VARIABLE.Value)
                    {
                        if (pcmInfo[0, trxInfo.FirstTSL] == null && pcmInfo[trxInfo.LapdSSL, trxInfo.LapdTSL] == null)
                        {
                            possibleShift += trxInfo.BSC + " " + trxInfo.BCF + " " + trxInfo.BTS + " " + trxInfo.TRX +
                                             " " + trxInfo.PCM + " " + trxInfo.FirstTSL + " " + trxInfo.LAPD + " " +
                                             trxInfo.LapdTSL + " " + trxInfo.LapdSSL + "\n";


                            pcmInfo[0, trxInfo.FirstTSL] = "TRX-" + trxInfo.TRX;
                            pcmInfo[1, trxInfo.FirstTSL] = "TRX-" + trxInfo.TRX;
                            pcmInfo[2, trxInfo.FirstTSL] = "TRX-" + trxInfo.TRX;
                            pcmInfo[3, trxInfo.FirstTSL] = "TRX-" + trxInfo.TRX;


                            pcmInfo[0, trxInfo.FirstTSL + 1] = "TRX-" + trxInfo.TRX;
                            pcmInfo[1, trxInfo.FirstTSL + 1] = "TRX-" + trxInfo.TRX;
                            pcmInfo[2, trxInfo.FirstTSL + 1] = "TRX-" + trxInfo.TRX;
                            pcmInfo[3, trxInfo.FirstTSL + 1] = "TRX-" + trxInfo.TRX;
                            int bitRate=aFetchDb.GetLAPDBitrate(bsc, trxInfo.LAPD);
                            if (bitRate == 16)
                            {
                                pcmInfo[trxInfo.LapdSSL, trxInfo.LapdTSL] = "TRX-";
                            }

                        }
                        else
                        {
                            impossibleShift += trxInfo.BSC + " " + trxInfo.BCF + " " + trxInfo.BTS + " " + trxInfo.TRX + " " + trxInfo.PCM + " " + trxInfo.FirstTSL + " " + trxInfo.LAPD + " " + trxInfo.LapdTSL + " " + trxInfo.LapdSSL + "\n";
                        }
                         
                    }
                }
            }
        }
    }
}
