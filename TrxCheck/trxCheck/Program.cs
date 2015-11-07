using System;
using System.Collections.Generic;
using TRX.DAO;
using TRX.DAL;

namespace trxCheck
{
    class Program
    {
        static void Main(string[] args)
        {
           

           Dictionary<int, Dictionary<int, List<TRXInfo>>> aDictionary = new Dictionary<int, Dictionary<int, List<TRXInfo>>>();
            
            int counter = 0;
            string line;

            // Read the file and display it line by line.
            System.IO.StreamReader file =
                new System.IO.StreamReader(@"D:\input\input1.txt");
            List<TRXInfo> inputList=new List<TRXInfo>();
            while ((line = file.ReadLine()) != null)
            {
                string[] words = line.Split(' ');
                TRXInfo aInfo = new TRXInfo();
                aInfo.BCF = Convert.ToInt32(words[1]);
                aInfo.BSC = Convert.ToInt32(words[0]);
                aInfo.BTS = Convert.ToInt32(words[2]);
                aInfo.TRX = Convert.ToInt32(words[3]);
                aInfo.PCM = Convert.ToInt32(words[4]);
                aInfo.FirstTSL=Convert.ToInt32(words[5]);
                aInfo.LapdTSL=Convert.ToInt32(words[7]);
                aInfo.LapdSSL = Convert.ToInt32(words[8]);
                aInfo.LAPD = words[6];
                inputList.Add(aInfo);
                


                Dictionary<int, List<TRXInfo>> pcmDictionary = new Dictionary<int, List<TRXInfo>>();
                List<TRXInfo> aList = new List<TRXInfo>();
               
         
               // Console.WriteLine(words[0]);
                if (aDictionary.ContainsKey(Convert.ToInt32(words[0])))
                {
                    
                    //a1Dictionary = aDictionary[Convert.ToInt32(words[0])];
                    if (aDictionary[Convert.ToInt32(words[0])].ContainsKey(Convert.ToInt32(words[4])))
                    {
                       

                        aDictionary[Convert.ToInt32(words[0])][Convert.ToInt32(words[4])].Add(aInfo);
                    }
                    else
                    {
                       
                        aList.Add(aInfo);
                        aDictionary[Convert.ToInt32(words[0])].Add(Convert.ToInt32(words[4]),aList);
                        
                    }

                }
                else
                {

                    
                    //List<TRXInfo> aList = new List<TRXInfo>();
                    aList.Add(aInfo);
                    int pcm = Convert.ToInt32(words[4]);
                    Dictionary<int, List<TRXInfo>> anotherDic = new Dictionary<int, List<TRXInfo>>();
                    anotherDic.Add(pcm,aList);
                    aDictionary.Add(Convert.ToInt32(words[0]),anotherDic);


                }

               // System.Console.WriteLine(line.Split());
                counter++;
            }

            file.Close();
            //System.Console.WriteLine("There were {0} lines.", counter);
            //// Suspend the screen.
            //System.Console.ReadLine();
            TRX.DAL.TrxManager aTrxManager=new TrxManager();
            //aTrxManager.CheckTRX(aDictionary,inputList);
            TRXShifter aTrxShifter=new TRXShifter();
            aTrxShifter.ShiftTRX(aDictionary, inputList);
        }
    }
}
