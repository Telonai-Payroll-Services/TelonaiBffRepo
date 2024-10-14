using Config = MeF.Client.Configuration.ClientConfiguration;

namespace MeF.Client.Logging
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text;

    using MeF.Client.Extensions;
    using MeF.Client.Helpers;

    using Microsoft.Practices.EnterpriseLibrary.Logging;
    using Microsoft.Practices.EnterpriseLibrary.Logging.Filters;
    using Microsoft.Practices.EnterpriseLibrary.Logging.Formatters;
    using Microsoft.Practices.EnterpriseLibrary.Logging.TraceListeners;

    internal sealed class MeFLog
    {
        #region Constants and Fields

        public static LogWriter _traceWriter;

        private static MeFLog _traceInstance;

        private static int level;

        private static string logName;

        private static string logPath;

        private static int rollSize;

        #endregion

        #region Constructors and Destructors

        internal MeFLog()
        {
            logPath = ApiHelper.GetWithBackslash(Config.MeFLogPath);
            logName = logPath + Constants.DefaultLogName;
            rollSize = Config.MaxLogSize;
            level = Config.Level;

            if (string.IsNullOrEmpty(logPath) || !ApiHelper.IsValidFilePath(logPath))
            {
                logName = Constants.DefaultLogFile;
            }

            var formatter = new TextFormatter("{message}");
            var listener = new RollingFlatFileTraceListener(
                logName, "", "", formatter, rollSize, "mm-dd-yyyy", RollFileExistsBehavior.Increment, RollInterval.Day);
            var source = new LogSource("MainLogSource", SourceLevels.All);
            source.Listeners.Add(listener);

            IDictionary<string, LogSource> traceSources = new Dictionary<string, LogSource>();
            traceSources.Add("Info", source);
            traceSources.Add("Error", source);
            traceSources.Add("Debug", source);
            traceSources.Add("Warning", source);
            traceSources.Add("Trace", source);
            var empty = new LogSource("Empty");
            _traceWriter = new LogWriter(new ILogFilter[0], traceSources, empty, empty, source, "Error", true, true);
            WriteNewLogInfo();
        }

        #endregion

        #region Public Methods

        public static bool CheckInstance()
        {
            if (_traceInstance == null)
            {
                return false;
            }
            return true;
        }

        public static MeFLog Instance()
        {
            return _traceInstance ?? (_traceInstance = new MeFLog());
        }

        public static void Log(string message)
        {
            Write(message, LoggingCategory.Info, LoggingPriority.Normal);
        }

        public static void LogDebug(string message)
        {
            CheckLog();
            var debugString = BuildMessage(message, "Trace");
            Write(debugString, LoggingCategory.Trace, LoggingPriority.Highest);
        }

        public static void LogDebug(string message, bool customHeader)
        {
            CheckLog();
            Write(message, LoggingCategory.Trace, LoggingPriority.Highest);
        }

        public static void LogError(string errorCode, string message, Exception ex)
        {
            CheckLog();
            var errorstring = logError(errorCode, message, ex);
            Write(errorstring, LoggingCategory.Error, LoggingPriority.Lowest);
        }

        public static void LogFatal(string message, Exception ex)
        {
            CheckLog();
            Write(message, LoggingCategory.Error, LoggingPriority.Lowest);
        }

        public static void LogInfo(string message)
        {
            CheckLog();
            var infoString = BuildMessage(message, "Info");
            Write(infoString, LoggingCategory.Info, LoggingPriority.Low);
        }

        ///Writes an Error to the log.
        ///
        ///Error Message
        public static void Write(string message)
        {
            Write(message, LoggingCategory.Trace, LoggingPriority.Normal);
        }

        public static void Write(string message, LoggingCategory loggingCategory, LoggingPriority loggingPriority)
        {
            if (!ShouldLog(loggingPriority))
            {
                return;
            }

            var entry = new LogEntry { Message = message };
            entry.Categories.Add(loggingCategory.ToString());
            entry.Priority = (int)Enum.Parse(typeof(LoggingPriority), loggingPriority.ToString());
            _traceWriter.Write(entry);
        }

        public static string logError(string errorCode, string message, Exception exception)
        {
            try
            {
                var trace = new StringBuilder();
                trace.AppendLine("-------------- Error --------------");
                trace.AppendLine("Error Code	: " + errorCode);
                trace.AppendLine("Error Message	: " + message);
                if (exception.Source != null)
                {
                    trace.AppendLine("Source		: " + exception.Source.Trim());
                }
                if (exception.TargetSite != null)
                {
                    trace.AppendLine("Method		: " + exception.TargetSite.Name);
                }
                trace.AppendLine("Date		: " + DateTime.Now.ToShortDateString());
                trace.AppendLine("Time		: " + DateTime.Now.ToLongTimeString());

                trace.AppendLine("Type		: " + exception.GetType().FullName);
                if (exception.StackTrace != null)
                {
                    trace.AppendLine("Stack Trace	: " + exception.StackTrace.Trim());
                }
                trace.AppendLine("-------------------------------------------------------------------");
                trace.AppendLine(exception.StackTrace);
                message = trace.ToString();
            }
            catch
            {

            }

            return message;
        }

        #endregion

        #region Methods

        private static string BuildMessage(string message, string loggingCategory)
        {
            var trace = new StringBuilder();
            trace.AppendLine(string.Format("[{0}]\t Time:{1}\t Message: {2}", loggingCategory, DateTime.Now, message));
            return trace.ToString();
        }

        private static void CheckLog()
        {
            if (!CheckInstance())
            {
                Instance();
            }
        }

        private static bool ShouldLog(LoggingPriority loggingPriority)
        {
            var priority = (int)Enum.Parse(typeof(LoggingPriority), loggingPriority.ToString());
            if (level == 0 || level > 3)
            {
                return true;
            }
            if (priority == 1)
            {
                return true;
            }
            return priority <= level;
        }

        private static void WriteNewLogInfo()
        {
            Write("***** Toolkit Logging Settings *****");
            Write(string.Format("Log FileName: '{0}'", logPath));
            Write(string.Format("Roll interval: '{0}'", rollSize));
            Write(string.Format("Log level: '{0}'", level));
        }

        #endregion
    }
}