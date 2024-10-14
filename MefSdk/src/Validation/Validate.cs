using System;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using MeF.Client.Exceptions;
using MeF.Client.Helpers;
using MeF.Client.Logging;
using MeFWCFClient.MeFTransmitterServices;
using MeF.Client.Util;
using System.Runtime.Serialization;

namespace MeF.Client.Validation
{
    /// <summary>
    /// Validation exception class.
    /// </summary>
    internal class ValidationException : ArgumentException, ISerializable
    {
        public ValidationException(string message)
            : base(message)
        {
            MeFLog.Write(message);
        }

    }

    /// <summary>
    /// Validation utilities.
    /// </summary>
    internal static class Validate
    {
        

        public static void NotBlank(string str, string message)
        {
            if (StringUtils.IsBlank(str))
            {
                throw new ValidationException(message);
            }
        }

        public static bool IsValidFilePath(string path)
        {
            string pattern = @"^(([a-zA-Z]\:)|(\\))(\\{1}|((\\{1})[^\\]([^/:*?<>""|]*))+)$";
            Regex reg = new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            return reg.IsMatch(path);
        }

        public static void Positive(int value, string message)
        {
            if (value <= 0)
            {
                throw new ValidationException(message);
            }
        }

        public static void Positive(string value, string message)
        {
            int number;
            number = ConvertToInt(value);
            if (number <= 0)
            {
                throw new ValidationException(message);
            }
        }

        // Function to test for Positive Integers.
        public static bool IsNaturalNumber(String strNumber)
        {
            Regex objNotNaturalPattern = new Regex("[^0-9]");
            Regex objNaturalPattern = new Regex("0*[1-9][0-9]*");
            return !objNotNaturalPattern.IsMatch(strNumber) &&
            objNaturalPattern.IsMatch(strNumber);
        }

        public static void IsValidStartEnd(DateTime? start, DateTime? end, string message, string api)
        {
            MeFLog.LogInfo(string.Format("Validate.IsValidStartEnd() StartDate:'{0}', EndDate:'{1}' ", start.GetValueOrDefault(), end.GetValueOrDefault()));
            if (!start.HasValue || !end.HasValue)
            {
                CheckStartEnd(start, end, api);
                string format = string.Format("Invalid StartDate or EndDate Parameter");
                string logstring = GetLogString(format, "MeFClientSDK000019", "IsValidStartEnd", api, start.GetValueOrDefault().ToString());
                MeFLog.Write(logstring, LoggingCategory.Error,LoggingPriority.Low);
                throw new ToolkitException("MeFClientSDK000019", logstring);
            }
        }

        private static void CheckStartEnd(DateTime? start, DateTime? end, string api)
        {
            if (start.Value >= end.Value)
            {
                string format = string.Format("The StartDate:'{0}'must be less than the EndDate:'{1}'", start.GetValueOrDefault(), end.GetValueOrDefault());
                string logstring = GetLogString(format, "MeFClientSDK000019", "IsValidStartEnd", api, start.ToString());
                MeFLog.Write(logstring, LoggingCategory.Error,LoggingPriority.Low);
                throw new ToolkitException("MeFClientSDK000019", logstring);
            }
        }

        public static void IsValidMaxResults(string value, string message, string api)
        {
            MeFLog.LogInfo(string.Format("Validate.IsValidMaxResults()...Parameter:{0}", value));
            if (!IsNaturalNumber(value))
            {
                string format = string.Format("Invalid MaxResults Parameter: {0}", value);
                string logstring = GetLogString(format, "MeFClientSDK000015", "IsValidMaxResultsValid", api, value);
                MeFLog.Write(logstring, LoggingCategory.Error,LoggingPriority.Low);
                throw new ToolkitException("MeFClientSDK000015", logstring);
            }
        }

        public static void IsValidFilePath(string value, string message, string api)
        {
            MeFLog.LogInfo(string.Format("Validate.IsValidFilePath()...Parameter:{0}", value));
            if (!IsValidFilePath(value))
            {
                string format = string.Format("Invalid FilePath or Directory Parameter: {0}", value);
                string logstring = GetLogString(format, "MeFClientSDK000003", "IsValidFilePath", api, value);
                MeFLog.Write(logstring, LoggingCategory.Error,LoggingPriority.Low);
                throw new ToolkitException("MeFClientSDK000003", logstring);
            }
        }

        public static void Positive(long value, string message)
        {
            if (value <= 0)
            {
                throw new ValidationException(message);
            }
        }

        public static int ConvertToInt(string userInput)
        {
            return Convert.ToInt32(userInput);
        }

        public static void NotEmpty(ICollection collection, string message)
        {
            if (collection == null || collection.Count == 0)
            {
                throw new ValidationException(message);
            }
        }

        public static void Null(object obj, string message)
        {
            if (obj != null)
            {
                throw new ValidationException(message);
            }
        }

        public static void NotNull(object obj, string message)
        {
            if (obj == null)
            {
                throw new ValidationException(message);
            }
        }

        public static void True(bool value, string message)
        {
            if (value == false)
            {
                throw new ValidationException(message);
            }
        }

        public static void False(bool value, string message)
        {
            if (value)
            {
                throw new ValidationException(message);
            }
        }

        public static void IsSubmissionDataListValid(SubmissionDataType[] submissionDataList, string message, string api)
        {
            MeFLog.LogInfo("Validate.IsSubmissionDataListValid()");
            if (submissionDataList.Length == 0)
            {
                string format = "Invalid SubmissionDataList: List is empty";
                string logstring = GetLogString(format, "MeFClientSDK000011", "IsSubmissionIDValid", api, submissionDataList.GetType().Name);
                MeFLog.Write(logstring, LoggingCategory.Trace, LoggingPriority.High);
                throw ToolkitException.Format("MeFClientSDK000011", logstring);
            }
            else
                for (int i = 0; i < submissionDataList.Length; i++)
                {
                    if (!IsSubmissionIDValid(submissionDataList[i].SubmissionId))
                    {
                        string format = string.Format("Invalid SubmissionID in SubmissionDataList: {0}", submissionDataList[i].SubmissionId.ToString());
                        string logstring = GetLogString(format, "MeFClientSDK000011", "IsSubmissionIDValid", api, submissionDataList[i].SubmissionId.ToString());
                        MeFLog.Write(logstring, LoggingCategory.Error, LoggingPriority.Low);
                        throw ToolkitException.Format("MeFClientSDK000011", logstring);
                    }
                }
        }

        public static void IsSubmissionIDListValid(string[] submissionIdList, string message, string api)
        {
            MeFLog.LogInfo("Validate.IsSubmissionIDValid()");
            if (submissionIdList.Length == 0)
            {
                string format = "Invalid SubmissionIDList: List is empty";
                string logstring = GetLogString(format, "MeFClientSDK000011", "IsSubmissionIDValid", api, submissionIdList.GetType().Name);
                MeFLog.LogInfo(logstring);
                throw ToolkitException.Format("MeFClientSDK000011", logstring);
            }
            else
                for (int i = 0; i < submissionIdList.Length; i++)
                {
                    if (!IsSubmissionIDValid(submissionIdList[i]))
                    {
                        string format = string.Format("Invalid SubmissionID in SubmissionIDList: {0}", submissionIdList[i].ToString());
                        string logstring = GetLogString(format, "MeFClientSDK000011", "IsSubmissionIDValid", api, submissionIdList[i].ToString());
                        MeFLog.Write(logstring, LoggingCategory.Error,LoggingPriority.Low);
                        throw ToolkitException.Format("MeFClientSDK000011", logstring);
                    }
                }
        }

        public static bool IsSubmissionIDValid(string submissionId)
        {
            MeFLog.LogInfo(string.Format("Validate.IsSubmissionIDValid()...Parameter:{0}", submissionId));
            Regex rEx = new Regex("[0-9]{13}[a-z0-9]{7}");
            if (submissionId.Length != 20)
            {
                return false;
            }
            if (rEx.Match(submissionId).Success)
            {
                return true;
            }
            return false;
        }

        public static void IsSubmissionIDValid(string submissionId, string message, string api)
        {
            MeFLog.LogInfo(string.Format("Validate.IsSubmissionIDValid()...Parameter:{0}", submissionId));
            Regex rEx = new Regex("[0-9]{13}[a-z0-9]{7}");
            if (submissionId.Length != 20)
            {
                string format = string.Format("Invalid Submission ID: {0}", submissionId);
                string logstring = GetLogString(format, "MeFClientSDK000011", "IsSubmissionIDValid", api, submissionId);
                MeFLog.Write(logstring, LoggingCategory.Error,LoggingPriority.Low);

                throw ToolkitException.Format("MeFClientSDK000011", logstring);
            }
            if (!rEx.Match(submissionId).Success)
            {
                string format = string.Format("Invalid Submission ID: {0}", submissionId);
                string logstring = GetLogString(format, "MeFClientSDK000011", "IsSubmissionIDValid", api, submissionId);
                MeFLog.Write(logstring, LoggingCategory.Error,LoggingPriority.Low);

                throw ToolkitException.Format("MeFClientSDK000011", logstring);
            }
        }

        public static void IsMessageIDValid(string messageId, string message, string api)
        {
            MeFLog.LogInfo(string.Format("Validate.IsMessageIDValid()...Parameter:{0}", messageId));
            Regex rEx = new Regex("[0-9]{12}[a-z0-9]{8}");

            if (messageId.Length != 20)
            {
                string format = string.Format("Invalid Message ID: {0}", messageId);
                string logstring = GetLogString(format, "MeFClientSDK000008", "IsMessageIDValid", api, messageId);
                MeFLog.Write(logstring, LoggingCategory.Error,LoggingPriority.Low);
                throw ToolkitException.Format("MeFClientSDK000008", logstring);
            }
            if (!rEx.Match(messageId).Success)
            {
                string format = string.Format("Invalid Message ID: {0}", messageId);
                string logstring = GetLogString(format, "MeFClientSDK000008", "IsMessageIDValid", api, messageId);
                MeFLog.Write(logstring, LoggingCategory.Error,LoggingPriority.Low);
                throw ToolkitException.Format("MeFClientSDK000008", logstring);
            }
        }

        public static void IsEtinValid(string etin, string message, string api)
        {
            MeFLog.LogInfo(string.Format("Validate.IsEtinValid()...Parameter:{0}", etin));
            if (!IsNaturalNumber(etin))
            {
                string format = string.Format("Invalid ETIN: {0}", etin);
                string logstring = GetLogString(format, "MeFClientSDK000007", "IsEtinValid", api, etin);
                MeFLog.Write(logstring, LoggingCategory.Error,LoggingPriority.Low);
                throw ToolkitException.Format("MeFClientSDK000007", logstring);
            }
        }

        public static void IsValidXdoc(string filePath, string rootElement, string message, string api)
        {
            var xdoc = XDocument.Load(filePath);
            if (xdoc.Root.Name.LocalName != rootElement)
            {
                string format = string.Format("Invalid Root Element in Xml Document: {0} , Expecting: {1}", xdoc.Root.Name.LocalName, rootElement);
                string logstring = GetLogString(format, "MeFClientSDK000019", "IsValidXdoc", api, xdoc.Root.Name.LocalName.ToString());
                MeFLog.Write(logstring, LoggingCategory.Error,LoggingPriority.Low);
                throw ToolkitException.Format("MeFClientSDK000019", logstring);
            }
        }

        // Function to Check for AlphaNumeric.
        public static bool IsAlphaNumeric(String strToCheck)
        {
            Regex objAlphaNumericPattern = new Regex("[^a-zA-Z0-9]");
            return !objAlphaNumericPattern.IsMatch(strToCheck);
        }

        //public bool ValidFilePath(string path)
        //{
        //    String pattern = @"^(([a-zA-Z]\:)|(\\))(\\{1}|((\\{1})[^\\]([^/:*?<>""|]*))+)$";
        //    Regex reg = new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
        //    return reg.IsMatch(path);
        //}

        internal static string GetLogString(string message, string errorCode, string method, string api, string value)
        {
            StringBuilder trace = new StringBuilder();

            trace.AppendLine("---------------- ToolKitException ----------------");
            trace.AppendLine(String.Format("Service name: {0}  Timestamp: {1} ", api, DateTime.Now.ToString()));
            trace.AppendLine("**********************************************");
            trace.AppendLine(String.Format(" Toolkit Version: {0} ", ApiHelper.ApiVersion.ToString()));
            trace.AppendLine(String.Format(" ErrorCode: {0} ", errorCode));
            trace.AppendLine(String.Format(" Message: {0} ", message));
            trace.AppendLine(String.Format(" Source: {0} ", api));
            trace.AppendLine(String.Format(" Method: {0} ", method));
            trace.AppendLine(String.Format(" Parameter: {0} ", value));

            return trace.ToString();
        }
    }
}