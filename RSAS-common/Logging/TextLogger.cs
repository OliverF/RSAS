using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RSAS.Logging
{
    public enum LogType { Information, Warning, Error };

    public delegate void TextLoggerMessageLoggedEventHandler(object sender, TextLoggerMessageLoggedEventArgs e);

    public static class TextLogger
    {
        public static event TextLoggerMessageLoggedEventHandler MessageLogged;

        public static void TimestampedLog(LogType type, string message)
        {
            string logMessage = "[" + type.ToString() + "] " + message + " " + System.DateTime.Now.ToString("H:m:s dd/MM/yyyy") + Environment.NewLine;

            if (MessageLogged != null)
                MessageLogged(null, new TextLoggerMessageLoggedEventArgs(logMessage));
        }
    }
}
