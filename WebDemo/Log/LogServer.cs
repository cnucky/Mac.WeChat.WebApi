using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebDemo
{
    public class LogServer
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public static void Info(string Msg)
        {
            logger.Info(Msg);
        }

        public static void Error(string Msg)
        {
            logger.Error(Msg);
        }
    }
}