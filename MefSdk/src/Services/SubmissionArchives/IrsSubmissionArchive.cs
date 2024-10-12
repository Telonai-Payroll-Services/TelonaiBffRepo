using System.Collections.Generic;

namespace MeF.Client.Services.InputComposition
{
    /// <summary>
    /// Contains properties and methods related to handling IRSSubmissionArchives.
    /// </summary>
    public class IRSSubmissionArchive : SubmissionArchive
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IRSSubmissionArchive"/> class.
        /// </summary>
        public IRSSubmissionArchive()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IRSSubmissionArchive"/> class.
        /// </summary>
        /// <param name="submissionId">The submission id.</param>
        /// <param name="submissionManifest">The submission manifest.</param>
        /// <param name="submissionXML">The submission XML.</param>
        /// <param name="binaryAttachments">The binary attachments.</param>
        /// <param name="archiveOutputLocation">The archive output location.</param>
        public IRSSubmissionArchive(string submissionId, SubmissionManifest submissionManifest,
            SubmissionXml submissionXML, List<BinaryAttachment> binaryAttachments, string archiveOutputLocation)
            : base(submissionId, submissionManifest, submissionXML, binaryAttachments, archiveOutputLocation)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IRSSubmissionArchive"/> class.
        /// </summary>
        /// <param name="submissionId">The submission id.</param>
        /// <param name="submissionManifest">The submission manifest.</param>
        /// <param name="submissionXML">The submission XML.</param>
        /// <param name="binaryAttachments">The binary attachments.</param>
        public IRSSubmissionArchive(string submissionId, SubmissionManifest submissionManifest,
            SubmissionXml submissionXML, List<BinaryAttachment> binaryAttachments)
            : base(submissionId, submissionManifest, submissionXML, binaryAttachments)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IRSSubmissionArchive"/> class.
        /// </summary>
        /// <param name="submissionId">The submission id.</param>
        /// <param name="submissionManifest">The submission manifest.</param>
        /// <param name="submissionXML">The submission XML.</param>
        /// <param name="archiveOutputLocation">The archive output location.</param>
        public IRSSubmissionArchive(string submissionId, SubmissionManifest submissionManifest,
                SubmissionXml submissionXML, string archiveOutputLocation)
            : base(submissionId, submissionManifest, submissionXML, null, archiveOutputLocation)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IRSSubmissionArchive"/> class.
        /// </summary>
        /// <param name="submissionId">The submission id.</param>
        /// <param name="submissionManifest">The submission manifest.</param>
        /// <param name="submissionXML">The submission XML.</param>
        public IRSSubmissionArchive(string submissionId, SubmissionManifest submissionManifest,
                SubmissionXml submissionXML)
            : base(submissionId, submissionManifest, submissionXML, null)
        {
        }
    }
}