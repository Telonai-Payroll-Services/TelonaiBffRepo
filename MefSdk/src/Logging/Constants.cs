using System;

namespace MeF.Client.Logging
{
    using System.IO;
    using System.Reflection;

    internal static class Constants
    {
        #region ArchiveStructure
        internal static readonly string ManifestDir = @"\manifest\";
        internal static readonly string XmlDir = @"\xml\";
        internal static readonly string AttachmentDir = @"\attachment\";
        internal static readonly string IrsDir = @"\irs\";
        internal static readonly string IrsXmlDir = IrsDir + XmlDir;
        internal static readonly string IrsAttachmentDir = IrsDir + AttachmentDir;
        internal static readonly string ManifestFile = ManifestDir + "manifest.xml";
        internal static readonly string SubmissionDirectory = @"\SubmissionArchives\";
        //internal static readonly string DefaultSubmissionDirectory = AttachmentUtil.TempRoot + SubmissionDirectory;
        #endregion

        #region LogInformation
        //Trace&Diagnostics
        internal static readonly String TraceSend = @"Sending Request at '{0}'";
        internal static readonly String TraceReceive = @"Received Response at '{0}'";
        internal static readonly String TraceEnter = @" Entering method'{0}'";
        internal static readonly String TraceExit = @" Exiting method '{0}'";
        internal static readonly String TraceDuration = @" Call benchmark:'{0}'";
        internal static readonly String TraceTicks = @" Ticks:'{0}'";

        //Errors&Exceptions
        internal static readonly String LogErrorCode = @" ErrorCode     : '{0}'";
        internal static readonly String LogErrorMessage = @" ErrorMessage  : '{0}'";
        internal static readonly String LogServiceName = @" Service name  : '{0}'";
        internal static readonly String LogTimestamp = @" Timestamp     : '{0}'";
        internal static readonly String LogVersion = @" SDK Version   : '{0}'";
        internal static readonly String LogSource = @" Source        : '{0}'";
        internal static readonly String LogStatusCode = @" StatusCode    : '{0}'";
        internal static readonly String LogHttpStatusCode = @" HttpStatusCode : '{0}'";
        internal static readonly String LogStatusDescription = @" StatusDescription: '{0}'";
        internal static readonly String LogMethod = @" Method        : '{0}'";
        internal static readonly String LogParameter = @" Parameter     : '{0}'";
        //General
        internal static readonly String LogAppSysId = @" AppSysID      : '{0}'";
        internal static readonly String LogEtin = @" Etin          : '{0}'";
        internal static readonly String LogProd_Ind = @" Prod_Ind      : '{0}'";
        internal static readonly String LogType = @" Type          : '{0}'";
        internal static readonly String LogMessage = @" Message       : '{0}'";
        internal static readonly String LogDetail = @" Detail        : '{0}'";
        internal static readonly String LogFaultSource = @" Source : '{0}'";
        internal static readonly String LogFaultDetail = @" Detail : '{0}'";
        internal static readonly String LogFaultType = @"   Type : '{0}'";
        internal static readonly String LogFaultMessage = @"    Message : '{0}'";
        //Security

        internal static readonly String LogCertAlias = @" CertAlias     : '{0}'";
        internal static readonly String LogIssueInstant = @" IssueInstant  : '{0}'";
        internal static readonly String LogSmsession = @" SMSESSION     : '{0}'";
        internal static readonly String LogToken = @" SecurityToken : '{0}'";
        internal static readonly String LogResponseTime = @" ResponseTime  : '{0}'";
        //Headers&Messages
        internal static readonly String LogApi = @" Client API    : '{0}'";
        internal static readonly String LogService = @" Service       : '{0}'";
        internal static readonly String LogMessageId = @" Message ID    : '{0}'";
        internal static readonly String LogRelatesToId = @" RelatesTo ID  : '{0}'";
        internal static readonly String LogResult = @" Result        : '{0}'";
        //FileStuff
        internal static readonly String LogFileName = @" FileName      : '{0}'";
        internal static readonly String LogFolder = @" Folder        : '{0}'";
        internal static readonly String LogFileSize = @" FileSize      : '{0}'";
        //LogHeaders
        internal static readonly String VersionNumber = "2024v15.0";
        internal static readonly String ToolkitVersion = "DNet" + VersionNumber;
        internal static readonly String LogInfoHeader = @" [Info] ";
        internal static readonly String DebugInfoHeader = @" [Trace] ";
        internal static readonly String ErrorInfoHeader = @" [Error] ";
        internal static readonly String WarnInfoHeader = @" [Warn] ";
        internal static readonly String ClientReqHeader = @" [Client Request] ";
        internal static readonly String ServiceResHeader = @" [Service Response] ";

        internal static readonly String MessageLine = "***************************************";

        internal static readonly String DashLine = "---------------------------------------";

        internal static readonly String CertLine = @" *** Login Certificate ***";
        internal static readonly String InnerExLine = @" *** Inner Exception ***";
        internal static readonly String FileInfoLine = @" *** File Information ***";
        internal static readonly String SoapFaultLine = @" *** Soap Fault ***";
        internal static readonly String InnerHttpLine = @" *** Http Error Details ***";
        internal static readonly String SecurityLine = "*** Security Information ***";
        internal static readonly String StackLine = "*** Stack Trace ***";
        internal static readonly String AdditionalDataLine = @" *** Additional Exception Data ***";

        internal static readonly String AppSettingKeyFound = @"Found <AppSettings> key='{0}' in .config.  Using value '{1}'.";
        internal static readonly String AppSettingKeyNotFound = @"Unable to find <AppSettings> key='{0}' in .config or configuration pipeline.  Using default value '{1}' instead.";
        internal static readonly String AppSettingError = @"<AppSettings> key='{0}' in .config or configuration cannot be used or converted in its current format.  Using default value '{1}' instead.";
        #endregion LogInformation

        #region LogErrorMessages
        internal static readonly string Unknown = "MeFClientSDK000029";
        internal static readonly String ErrorTextInvalidXmlRootElement =
            @"The root Element '{0}' is the wrong format, expecting root element '{1}'.";

        internal static readonly String ErrorTextInvalidEtin =
            @"This ETIN '{0}' is the wrong format or contains invalid characters.";

        internal static readonly String ErrorTextInvalidAppSysId =
            @"This AppSysID '{0}' is the wrong format or contains invalid characters.";

        internal static readonly String ErrorTextInvalidCertificateSubjectName =
            @"This CertificateSubject '{0}' is the wrong format or contains invalid characters.";

        internal static readonly String ErrorTextInvalidFilePath =
            @"The file path '{0}'is contains invalid characters or does not exist.";

        internal static readonly String ErrorTextInvalidSubmissionDataListEmpty =
            @"Invalid SubmissionDataList, the list is empty.";

        internal static readonly String ErrorTextInvalidSubmissionDataListSubmissionId =
            @"The SubmissionDataList contains a invalid SubmissionID, '{0}' is not in the correct format.";

        internal static readonly String ErrorTextInvalidSubmissionId =
            @"The SubmissionID '{0}' is not in the correct format.";

        internal static readonly String ErrorTextInvalidSubmissionIdListEmpty =
            @"Invalid SubmissionDataList, the list is empty.";

        internal static readonly String ErrorTextInvalidSubmissionIdListSubmissionId =
            @"The SubmissionIDList contains a invalid SubmissionID, '{0}' is not in the correct format.";

        internal static readonly String ErrorTextInvalidDateTimeEndDate =
            @"The EndDate '{0}' cannot be earlier than the StartDate '{1}'.";

        internal static readonly String ErrorTextInvalidStartEndDate = @"The StartDate and/or EndDate are Invalid.";
        internal static readonly String ErrorTextNull = @"The '{0}' Cannot be null or Empty.";

        internal static readonly String ErrorTextMaxResults =
            @"The MaxResults '{0}' is invalid or not in the correct format.";

        internal static readonly String ErrorTextFileNotFound = @"The file '{0}' does not exist.";
        internal static readonly String ErrorTextInvalidMessageId = @"The MessageID '{0}' is not in the correct format.";

        internal static readonly String SoapException =
            @"A service exception has occurred, see the log for detailed information.";
        internal static readonly String ErrorInvalidSc =
            @"The service context is null or does not have a valid session established. Make sure you have logged in prior to calling this service.";
        #endregion LogErrorMessages

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

        internal static readonly String SoapEnvNS = "http://schemas.xmlsoap.org/soap/envelope/";
        internal static readonly String XmlSchemaInstanceNS = "http://www.w3.org/2001/XMLSchema-instance";
        internal static readonly String XmlSchemaNS = "http://www.w3.org/2001/XMLSchema";
        internal static readonly String AddressingNS = "http://schemas.xmlsoap.org/ws/2004/03/addressing";

        internal static readonly String WssecextNS =
            "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd";

        internal static readonly String WsutilityNS =
            "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd";

        internal static readonly String SamlAssertionNS = "urn:oasis:names:tc:SAML:1.0:assertion";

        internal static readonly String SAMLAssertion =
            "http://docs.oasis-open.org/wss/oasis-wss-saml-token-profile-1.1#SAMLV1.1";

        internal static readonly String XmlDsigNS = "http://www.w3.org/2000/09/xmldsig#";

        #endregion MefSoapNameSpaces

        #region Other Information

        internal static readonly String DefaultLogPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        internal static readonly String DefaultLogName = @"\trace.log";
        internal static readonly String DefaultAuditName = @"\audit.log";
        internal static readonly String DefaultLogFile = DefaultLogPath + DefaultLogName;
        internal static readonly String DefaultAuditFile = DefaultLogPath + DefaultAuditName;

        #endregion Other Information
    }
}