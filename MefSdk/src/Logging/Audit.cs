using Config = MeF.Client.Configuration.ClientConfiguration;

namespace MeF.Client.Logging
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    using MeF.Client.Extensions;
    using MeF.Client.Helpers;

    using Microsoft.Practices.EnterpriseLibrary.Logging;
    using Microsoft.Practices.EnterpriseLibrary.Logging.Filters;
    using Microsoft.Practices.EnterpriseLibrary.Logging.Formatters;
    using Microsoft.Practices.EnterpriseLibrary.Logging.TraceListeners;

    internal sealed class Audit
    {
        #region Constants and Fields

        public static LogWriter _auditWriter;

        private static Audit _auditInstance;

        private static string auditName;

        private static string auditPath;

        private static int rollSize;

        #endregion

        #region Constructors and Destructors

        internal Audit()
        {
            auditPath = ApiHelper.GetWithBackslash(Config.AuditPath);
            auditName = auditPath + Constants.DefaultAuditName;
            rollSize = Config.MaxAuditSize;

            if (string.IsNullOrEmpty(auditPath) || !ApiHelper.IsValidFilePath(auditPath))
            {
                auditName = Constants.DefaultAuditFile;
            }

            var formatter = new TextFormatter("{message}");
            var listener = new RollingFlatFileTraceListener(
                auditName, "", "", formatter, rollSize, "mm-dd-yyyy", RollFileExistsBehavior.Increment, RollInterval.Day);
            var source = new LogSource("MainAuditSource", SourceLevels.All);
            source.Listeners.Add(listener);

            IDictionary<string, LogSource> traceSources = new Dictionary<string, LogSource>();
            traceSources.Add("Info", source);
            traceSources.Add("Error", source);
            traceSources.Add("Debug", source);
            traceSources.Add("Warning", source);
            traceSources.Add("Trace", source);
            var empty = new LogSource("Empty");
            _auditWriter = new LogWriter(new ILogFilter[0], traceSources, empty, empty, source, "Error", true, true);
        }

        #endregion

        #region Public Methods

        public static bool CheckInstance()
        {
            return _auditInstance != null;
        }

        public static Audit Instance()
        {
            return _auditInstance ?? (_auditInstance = new Audit());
        }

        public static void Write(ICollection<string> messages)
        {
            foreach (var m in messages)
            {
                Write(m);
            }
        }

        public static void Write(string message)
        {
            CheckLog();
            var entry = new LogEntry { Message = message };
            entry.Categories.Add("Error");
            entry.Priority = (int)Enum.Parse(typeof(LoggingPriority), LoggingPriority.Normal.ToString());
            _auditWriter.Write(entry);
        }

        #endregion

        #region Methods

        private static void CheckLog()
        {
            if (!CheckInstance())
            {
                Instance();
            }
        }

        #endregion
    }
}