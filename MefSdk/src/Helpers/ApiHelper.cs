using System;
using System.Xml.Linq;

namespace MeF.Client.Helpers
{
    using System.Collections.Generic;
    using System.IO;
    using System.Text.RegularExpressions;

 //   using Microsoft.Web.Services3;

    internal static class ApiHelper
    {
        internal static readonly String ApiVersion = "DNet"+MeF.Client.Logging.Constants .VersionNumber;

        #region MefTypeNameSpaces

        internal static readonly String EfileNS = "http://www.irs.gov/efile";
        internal static readonly String EtecTransmitterNS = "http://www.irs.gov/a2a/mef/ETECTransmitterService";
        internal static readonly String EtecStateNS = "http://www.irs.gov/a2a/etec/ETECStateService";
        internal static readonly String TransmitterNS = "http://www.irs.gov/a2a/mef/MeFTransmitterService";
        internal static readonly String StateNS = "http://www.irs.gov/a2a/mef/MeFStateService";
        internal static readonly String MsiNS = "http://www.irs.gov/a2a/mef/MeFMSIServices";
        internal static readonly String MefHeaderNS = "http://www.irs.gov/a2a/mef/MeFHeader.xsd";
        internal static readonly String FaultsNS = "http://www.irs.gov/a2a/mef";

        #endregion MefTypeNameSpaces

        #region MefSoapNameSpaces

        internal static readonly String soapEnvNS = "http://schemas.xmlsoap.org/soap/envelope/";
        internal static readonly String xmlSchemaInstanceNS = "http://www.w3.org/2001/XMLSchema-instance";
        internal static readonly String xmlSchemaNS = "http://www.w3.org/2001/XMLSchema";
        internal static readonly String addressingNS = "http://schemas.xmlsoap.org/ws/2004/03/addressing";
        internal static readonly String wssecextNS = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd";
        internal static readonly String wsutilityNS = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd";
        internal static readonly String samlAssertionNS = "urn:oasis:names:tc:SAML:1.0:assertion";
        internal static readonly String xmlDsigNS = "http://www.w3.org/2000/09/xmldsig#";

        #endregion MefSoapNameSpaces

        #region MefErrorMessages
        internal enum MeFErrorCode
        {
            MeFClientSDK000001,
            MeFClientSDK000002,
            MeFClientSDK000003,
            MeFClientSDK000004,
            MeFClientSDK000005,
            MeFClientSDK000006,
            MeFClientSDK000007,
            MeFClientSDK000008,
            MeFClientSDK000009,
            MeFClientSDK000010,
            MeFClientSDK000011,
            MeFClientSDK000012,
            MeFClientSDK000013,
            MeFClientSDK000014,
            MeFClientSDK000015,
            MeFClientSDK000016,
            MeFClientSDK000017,
            MeFClientSDK000018,
            MeFClientSDK000019,
            MeFClientSDK000020,
            MeFClientSDK000021,
            MeFClientSDK000022,
            MeFClientSDK000023,
            MeFClientSDK000024,
            MeFClientSDK000025,
            MeFClientSDK000026,
            MeFClientSDK000027,
            MeFClientSDK000028,
            MeFClientSDK000029,
            MeFClientSDK000030,
            MeFClientSDK000031,
            MeFClientSDK000032,
            MeFClientSDK000033,
            MeFClientSDK000034,
            MeFClientSDK000035,
            MeFClientSDK000036,
            MeFClientSDK000037,
            MeFClientSDK000038
        }

        private static readonly Dictionary<string, string> ErrorInfo
            = new Dictionary<string, string>(StringComparer.Ordinal)
                  {
                      {"MeFClientSDK000001", "A submission archive for a submission id cannot be found"},
                      {"MeFClientSDK000002", "Audit Log not found"},
                      {"MeFClientSDK000003", "Directory location not specified or invalid."},
                      {"MeFClientSDK000004", "File not found"},
                      {"MeFClientSDK000005", "Invalid EFIN"},
                      {"MeFClientSDK000006", "Invalid EIN"},
                      {"MeFClientSDK000007", "Invalid ETIN"},
                      {"MeFClientSDK000008", "Invalid Message ID"},
                      {"MeFClientSDK000009", "Invalid Postmark"},
                      {"MeFClientSDK000010", "Invalid reporting period"},
                      {"MeFClientSDK000011", "Invalid Submission ID"},
                      {"MeFClientSDK000012", "Invalid VIN"},
                      {"MeFClientSDK000013", "Invalid XML Schema"},
                      {"MeFClientSDK000014", "Logging configuration file was not found"},
                      {"MeFClientSDK000015", "MaxResults parameter is invalid"},
                      {"MeFClientSDK000016", "MaxResults size exceeds max limit"},
                      {"MeFClientSDK000017", "Mismatch in IRSData versus actual response attachment contents"},
                      {"MeFClientSDK000018", "Number of specified VINs exceeds max limit"},
                      {"MeFClientSDK000019", "Parser Error"},
                      {"MeFClientSDK000020", "The endpoint URL configuration file was not found"},
                      {"MeFClientSDK000021","The SOAP message security configuration descriptor file could not be found"},
                      {"MeFClientSDK000022", "Toolkit operation can not proceed with logging level OFF"},
                      {"MeFClientSDK000023", "Unable to access file"},
                      {"MeFClientSDK000024", "Unable to access logging configuration file"},
                      {"MeFClientSDK000025", "Unable to create directory"},
                      {"MeFClientSDK000026", "Unable to load endpoint URLs from the properties file"},
                      {"MeFClientSDK000027", "Unable to save file"},
                      {"MeFClientSDK000028", "Unable to create zip archive"},
                      {"MeFClientSDK000029", "Unexpected Error"},
                      {"MeFClientSDK000030", "File attachment exceeds maximum size"},
                      {"MeFClientSDK000031", "Invalid parameter value"},
                      {"MeFClientSDK000032", "Client security configuration error"},
                      {"MeFClientSDK000033", "Message level security error"},
                      {"MeFClientSDK000034", "Unable to create security processor factory"},
                      {"MeFClientSDK000035","File attachment exceeds maximum size supported by the physical platform of the operating system"},
                      {"MeFClientSDK000036", "Unable to create MeF Header"},
                      {"MeFClientSDK000037", "List of postmarked submissions archive can not be empty"},
                      {"MeFClientSDK000038", "Successful login is required prior to using this service"}
                  };

      
        internal static string GetErrorInfo(MeFErrorCode errorCode)
        {
            return GetErrorInfo(errorCode.ToString());
        }

        internal static string GetErrorInfo(string errorCode)
        {
            string result;
            return ErrorInfo.TryGetValue(errorCode, out result) ? result : Unknown;
        }

        internal static readonly string Unknown = MeFErrorCode.MeFClientSDK000029.ToString();



        #endregion MefErrorMessages

        //internal static bool HasSoapFault(this WebServicesClientProtocol proxy)
        //{
        //    return proxy!= null && proxy.ResponseSoapContext != null && proxy.ResponseSoapContext.Envelope.Fault != null;
        //}

        internal static string GetWithBackslash(string path)
        {
            if (!path.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                path += Path.DirectorySeparatorChar.ToString();
            }

            return path;
        }

        internal static bool IsValidFilePath(string path)
        {
            const string pattern = @"^(([a-zA-Z]\:)|(\\))(\\{1}|((\\{1})[^\\]([^/:*?<>""|]*))+)$";
            var reg = new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            return reg.IsMatch(path);
        }
    }
}