namespace MeF.Client
{
    internal static class ValidationHelper
    {

        internal static readonly string ErrorTextInvalidXmlRootElement = "The root Element '{0}' is the wrong format, the root element must be '{1}'.";
        internal static readonly string ErrorTextInvalidETIN = "This company name is the wrong format or contains invalid characters.";
        internal static readonly string ErrorTextInvalidAppSysID = "This company abbreviation is the wrong format or contains invalid characters.";
        internal static readonly string ErrorTextInvalidCertificateSubjectName = "This project name is the wrong format or contains invalid characters.";
        internal static readonly string ErrorTextInvalidFilePath = "The file path '{0}'is contains invalid characters or does not exist.";
        internal static readonly string ErrorTextInvalidSubmissionDataListEmpty = "Invalid SubmissionDataList, the list is empty.";
        internal static readonly string ErrorTextInvalidSubmissionDataListSubmissionID = "The SubmissionDataList contains a invalid SubmissionID, '{0}' is not in the correct format.";
        internal static readonly string ErrorTextInvalidSubmissionID = "The SubmissionID '{0}' is not in the correct format.";
        internal static readonly string ErrorTextInvalidSubmissionIDListEmpty = "Invalid SubmissionDataList, the list is empty.";
        internal static readonly string ErrorTextInvalidSubmissionIDListSubmissionID = "The SubmissionIDList contains a invalid SubmissionID, '{0}' is not in the correct format.";
    }
}