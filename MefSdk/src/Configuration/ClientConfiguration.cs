namespace MeF.Client.Configuration
{
    using System;
    using System.Configuration;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;

    internal static class ClientConfiguration
    {
        #region Public Properties

        /// <summary>
        ///   Gets the audit path.
        /// </summary>
        public static string AuditPath
        {
            get
            {
                return Parse("mefAuditPath", "");
            }
        }

        /// <summary>
        ///   Gets the find value.
        /// </summary>
        public static string FindValue
        {
            get
            {
                return Parse("findValue", "");
            }
        }

        /// <summary>
        ///   Gets the level.
        /// </summary>
        public static int Level
        {
            get
            {
                return Parse("logLevel", 3);
            }
        }

        /// <summary>
        ///   Gets the size of the max audit.
        /// </summary>
        /// <value>
        ///   The size of the max audit.
        /// </value>
        public static int MaxAuditSize
        {
            get
            {
                return Parse("maxAuditSizeKb", 150);
            }
        }

        /// <summary>
        ///   Gets the size of the max log.
        /// </summary>
        /// <value>
        ///   The size of the max log.
        /// </value>
        public static int MaxLogSize
        {
            get
            {
                return Parse("maxLogSizeKb", 150);
            }
        }

        /// <summary>
        ///   Gets mef log path.
        /// </summary>
        public static string MeFLogPath
        {
            get
            {
                return Parse("mefLogPath", "");
            }
        }

        /// <summary>
        ///   Gets mef log path.
        /// </summary>
        public static string MeFLogSoap
        {
            get
            {
                return Parse("logSoap", "");
            }
        }

        /// <summary>
        ///   Gets the store location.
        /// </summary>
        //public static StoreLocation StoreLocation
        //{
        //    get
        //    {
        //        return ParseEnum("storeLocation", StoreLocation.CurrentUser);
        //    }
        //}

        /// <summary>
        ///   Gets the name of the store.
        /// </summary>
        /// <value>
        ///   The name of the store.
        /// </value>
        //public static StoreName StoreName
        //{
        //    get
        //    {
        //        return ParseEnum("storeName", StoreName.My);
        //    }
        //}

        /// <summary>
        ///   Gets the proxy timeout (in seconds), default is 120(2 min).
        /// </summary>
        //public static int Timeout
        //{
        //    get
        //    {
        //        return Parse("timeout", 120);
        //    }
        //}

        /// <summary>
        ///   Gets the type of the X509 find.
        /// </summary>
        /// <value>
        ///   The type of the X509 find.
        /// </value>
        //public static X509FindType X509FindType
        //{
        //    get
        //    {
        //        return ParseEnum("x509FindType", X509FindType.FindBySubjectName);
        //    }
        //}

        #endregion

        #region Public Methods

        /// <summary>
        ///   Parses the specified setting name.
        /// </summary>
        /// <typeparam name = "T"></typeparam>
        /// <param name = "settingName">Name of the setting.</param>
        /// <param name = "defaultValue">The default value.</param>
        /// <returns></returns>
        public static T Parse<T>(string settingName, T defaultValue)
        {
            return AppSetting(settingName, defaultValue, false);
        }

        /// <summary>
        ///   Parses the enum.
        /// </summary>
        /// <typeparam name = "T"></typeparam>
        /// <param name = "settingName">Name of the setting.</param>
        /// <param name = "defaultValue">The default value.</param>
        /// <returns></returns>
        public static T ParseEnum<T>(string settingName, T defaultValue)
        {
            return AppSetting(settingName, defaultValue, true);
        }

        #endregion

        #region Methods

        /// <summary>
        ///   Return a formatted string of all application settings.  Each line is delimited by Environment.NewLine
        /// </summary>
        /// <returns>List of all AppSettings</returns>
        internal static string EffectiveAppSettings()
        {
            var sb = new StringBuilder();
            foreach (var key in ConfigurationManager.AppSettings.AllKeys)
            {
                sb.AppendLine(string.Format("AppSettings['{0}'] = '{1}'", key, ConfigurationManager.AppSettings[key]));
            }
            return sb.ToString();
        }

        /// <summary>
        ///   Get an optional value from AppSettings section of the configuration pipeline.  Expands any embedded config metadata tags
        /// </summary>
        /// <typeparam name = "T">Type to cast result to</typeparam>
        /// <param name = "key">Key in AppSettings section containing value</param>
        /// <param name = "defaultValue">default to use if key is not found in configuration file</param>
        /// <param name = "isEnum"></param>
        /// <returns>Converted type</returns>
        /// <exception cref = "ConfigurationErrorsException">Throw when configuration value is provided but cannot be converted into T</exception>
        private static T AppSetting<T>(string key, T defaultValue, bool isEnum)
        {
            var parsedValue = default(T);
            var settingValue = ConfigurationManager.AppSettings[key];
            try
            {
                if (settingValue != null)
                {
                    if (isEnum)
                    {
                        parsedValue = (T)Enum.Parse(typeof(T), settingValue);
                    }
                    else
                    {
                        parsedValue = (T)Convert.ChangeType(settingValue, typeof(T));
                    }
                }
                else
                {
                    return defaultValue;
                }
            }
            catch (Exception)
            {
                parsedValue = defaultValue;
            }
            return parsedValue;
        }

        #endregion
    }
}