using Amazon.S3;
using Amazon.SQS;
using log4net;
using log4net.Config;
using System;
using System.IO;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Threading;
using System.Timers;

namespace LogGenerator
{
    class Program
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        static System.Timers.Timer timer = new System.Timers.Timer(1000);
        static Thread thread = null;
        static int threadCount = 0;

        static void Main(string[] args)
        {
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));

            log.Info("Application started");
            timer.Elapsed += ThreadStart;
            timer.Enabled = true;

            int loopCnt = 1;
            while (timer.Enabled)
            {
                log.Info("Loop count : " + loopCnt.ToString());
                Thread.Sleep(1000);
                loopCnt++;
            }

            log.Info("Application ended");
        }

        private static void ThreadStart(Object source, ElapsedEventArgs e)
        {
            if (thread == null || thread.IsAlive == false)
            {
                threadCount++;
                thread = new Thread(new ThreadStart(Log));
                thread.Start();
            }
            else
                log.Warn(string.Format("Thread {0} already running..", threadCount.ToString()));
        }

        private static void Log()
        {
            log.Info(string.Format("Thread {0} started..", threadCount.ToString()));
            switch (threadCount)
            {
                case 1:
                    LogNullReferenceException();
                    break;
                case 2:
                    LogDivideByZeroException();
                    break;
                case 3:
                    LogS3Exception();
                    break;
                case 4:
                    LogSQSException();
                    break;
                case 5:
                    LogFileNotFoundException();
                    break;
                case 6:
                    LogDirectoryNotFoundException();
                    break;
                default:
                    timer.Enabled = false;
                    break;
            }
            Thread.Sleep(2000);
            log.Info(string.Format("Thread {0} ended..", threadCount.ToString()));
        }

        private static void LogNullReferenceException()
        {
            try
            {
                string nullStr = null;
                nullStr.Contains('a');
            }
            catch (NullReferenceException ex)
            {
                log.Error(string.Format("Exception occured in LogNullReferenceException() method. Exception message: {0}", ex.Message));
            }
        }

        private static void LogDivideByZeroException()
        {
            try 
            {
                int zero = 0;
                int num = 1 / zero;
            }
            catch (DivideByZeroException ex)
            {
                log.Error(string.Format("Exception occured in LogDivideByZeroException() method. Exception message: {0}", ex.Message));
            }
        }

        private static void LogSQSException()
        {
            try
            {
                AWSHelper.SendResponseToSQSQueue("", "", "", "", "");
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Exception occured in LogSQSException() method. Exception message: {0}", ex.Message));
            }
        }

        private static void LogS3Exception()
        {
            try
            {
                AWSHelper.SendResponseToS3Bucket("", "", "", "", "", "");
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Exception occured in LogS3Exception() method. Exception message: {0}", ex.Message));
            }
        }

        private static void LogFileNotFoundException()
        {
            try
            {
                File.ReadAllText("abc.txt");
            }
            catch (FileNotFoundException ex)
            {
                log.Error(string.Format("Exception occured in LogFileNotFoundException() method. Exception message: {0}", ex.Message));
            }
        }

        private static void LogDirectoryNotFoundException()
        {
            try
            {
                Directory.GetFiles("abc");
            }
            catch (DirectoryNotFoundException ex)
            {
                log.Error(string.Format("Exception occured in LogDirectoryNotFoundException() method. Exception message: {0}", ex.Message));
            }
        }
    }
}
