using System;
using System.Collections;
using System.Collections.Concurrent;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Threading;

namespace SerialSniffer
{
    class SerialData
    {
        public String strPrefix;
        public byte[] i8Data;
        public int i32DataSize;
        public SerialData(String strPrefIn, byte[] i8DataIn,int i32DataSizeIn)
        {
            strPrefix = strPrefIn;
            i8Data = i8DataIn;
            i32DataSize = i32DataSizeIn;
        }
    }
    class Program
    {
        static bool bStop = false;
        static int i32MessageSize = 8;
        static ConcurrentQueue<SerialData> qMsgQ;
        static String strPathToLog = "ComLog.txt";
        static int i32RX_Count = 0;
        static byte[] ai8RX_Buffer = new byte[i32MessageSize];
        static int i32TX_Count = 0;
        static byte[] ai8TX_Buffer = new byte[i32MessageSize];
        static long i64InitTimestamp;
        static long i64PrevTime;

        private static void RX_PORT_DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            i32RX_Count += sp.Read(ai8RX_Buffer, i32RX_Count, i32MessageSize - i32RX_Count);
            if (i32RX_Count < i32MessageSize)
                return;
            i32RX_Count = 0;
            qMsgQ.Enqueue(new SerialData("[RX]", ai8RX_Buffer, i32MessageSize));
        }

        private static void TX_PORT_DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            i32TX_Count += sp.Read(ai8TX_Buffer, i32TX_Count, i32MessageSize - i32TX_Count);
            if (i32TX_Count < i32MessageSize)
                return;
            i32TX_Count = 0;
            qMsgQ.Enqueue(new SerialData("[TX]", ai8TX_Buffer, i32MessageSize));
        }

        public static void LoggerThread()
        {
            SerialData pData;
            StreamWriter pSw;
            long i64Temp = 0;
            qMsgQ = new ConcurrentQueue<SerialData>();
            if (!File.Exists(strPathToLog))
            {
                pSw = File.CreateText(strPathToLog);
            }
            else
            {
                pSw = File.AppendText(strPathToLog);
            }
            pSw.WriteLine("New sesion " + DateTime.Now);
            pSw.WriteLine();
            while (true)
            {
                if (qMsgQ.Count > 0)
                {
                    if(qMsgQ.TryDequeue(out pData))
                    {
                        if (pData != null)
                        {
                            i64Temp = (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond);
                            if (pData.strPrefix.Equals("[RX]"))
                            {
                                Console.Write("Diff: " + (i64Temp - i64PrevTime).ToString() + " ");
                                i64PrevTime = i64Temp;
                            }
                            i64Temp -= i64InitTimestamp;
                            Console.Write("T:" + i64Temp.ToString() + "  " + pData.strPrefix + " : ");
                            pSw.Write("T:" + i64Temp.ToString() + "  " + pData.strPrefix + " : ");
                            foreach (byte value in pData.i8Data)
                            {
                                Console.Write(value + ",");
                                pSw.Write(value + ",");
                            }
                            Console.WriteLine();
                            pSw.WriteLine();
                            pSw.Flush();

                        }
                    }
                    else
                    {
                        Console.WriteLine("Fail to dequeue");
                    }
                }
                Thread.Sleep(1);
                if(bStop)
                {
                    return;
                }    
            }
        }
        static void Main(string[] args)
        {
            Console.WriteLine("Start SerialSniffer");
            Console.WriteLine("For terminate application press any key....");
            if(args.Count() != 3)
            {
                Console.WriteLine("Wrong params. Correct params is: RX_Port TX_Port  Baudrate");
                return;
            }
            Thread pLogerTh = new Thread(LoggerThread);
            pLogerTh.Start();
            Thread.Sleep(2);
            SerialPort pPortRX = new SerialPort(args[0], Int32.Parse(args[2]), Parity.None, 8, StopBits.One);
            pPortRX.DataReceived += new SerialDataReceivedEventHandler(RX_PORT_DataReceivedHandler);
            pPortRX.ReceivedBytesThreshold = i32MessageSize;
            pPortRX.Open();
            pPortRX.DiscardInBuffer();
            SerialPort pPortTX = new SerialPort(args[1], Int32.Parse(args[2]), Parity.None, 8, StopBits.One);
            pPortTX.DataReceived += new SerialDataReceivedEventHandler(TX_PORT_DataReceivedHandler);
            pPortTX.ReceivedBytesThreshold = i32MessageSize;
            i64InitTimestamp = (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond);
            i64PrevTime = i64InitTimestamp;
            pPortTX.Open();
            pPortTX.DiscardInBuffer();
            Console.ReadKey();
            bStop = true;
            pLogerTh.Join();
            Console.WriteLine("End");
        }
    }
}
