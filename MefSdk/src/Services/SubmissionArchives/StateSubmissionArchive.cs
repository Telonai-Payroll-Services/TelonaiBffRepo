using System;
using System.Collections.Generic;

namespace MeF.Client.Services.InputComposition
{
    /// <summary>
    /// Contains properties and methods related to handling StateSubmissionArchives.
    /// </summary>
    public class StateSubmissionArchive : SubmissionArchive
    {
        private SubmissionXml irsSubmissionXML;

        /// <summary>
        /// Gets or sets the irs submission XML.
        /// </summary>
        /// <value>The irs submission XML.</value>
        public SubmissionXml IrsSubmissionXML
        {
            get { return irsSubmissionXML; }
            set { irsSubmissionXML = value; }
        }

        private List<BinaryAttachment> irsBinaryAttachments;

        /// <summary>
        /// Gets or sets the irs binary attachments.
        /// </summary>
        /// <value>The irs binary attachments.</value>
        public List<BinaryAttachment> IrsBinaryAttachments
        {
            get { return irsBinaryAttachments; }
            set { irsBinaryAttachments = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StateSubmissionArchive"/> class.
        /// </summary>
        /// <param name="submissionId">The submission id.</param>
        /// <param name="submissionManifest">The submission manifest.</param>
        /// <param name="submissionXML">The submission XML.</param>
        /// <param name="binaryAttachments">The binary attachments.</param>
        /// <param name="archiveOutputLocation">The archive output location.</param>
        /// <param name="irsSubmissionXML">The irs submission XML.</param>
        /// <param name="irsBinaryAttachments">The irs binary attachments.</param>
        public StateSubmissionArchive(string submissionId, SubmissionManifest submissionManifest,
                SubmissionXml submissionXML, List<BinaryAttachment> binaryAttachments, string archiveOutputLocation,
                SubmissionXml irsSubmissionXML, List<BinaryAttachment> irsBinaryAttachments)
            : base(submissionId, submissionManifest, submissionXML, binaryAttachments, archiveOutputLocation)
        {
            this.irsSubmissionXML = irsSubmissionXML;
            this.irsBinaryAttachments = irsBinaryAttachments;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StateSubmissionArchive"/> class.
        /// </summary>
        /// <param name="submissionId">The submission id.</param>
        /// <param name="submissionManifest">The submission manifest.</param>
        /// <param name="submissionXML">The submission XML.</param>
        /// <param name="binaryAttachments">The binary attachments.</param>
        /// <param name="irsSubmissionXML">The irs submission XML.</param>
        /// <param name="irsBinaryAttachments">The irs binary attachments.</param>
        public StateSubmissionArchive(string submissionId, SubmissionManifest submissionManifest,
                SubmissionXml submissionXML, List<BinaryAttachment> binaryAttachments, 
                SubmissionXml irsSubmissionXML, List<BinaryAttachment> irsBinaryAttachments)
            : base(submissionId, submissionManifest, submissionXML, binaryAttachments)
        {
            this.irsSubmissionXML = irsSubmissionXML;
            this.irsBinaryAttachments = irsBinaryAttachments;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StateSubmissionArchive"/> class.
        /// </summary>
        /// <param name="submissionId">The submission id.</param>
        /// <param name="submissionManifest">The submission manifest.</param>
        /// <param name="submissionXML">The submission XML.</param>
        /// <param name="archiveOutputLocation">The archive output location.</param>
        /// <param name="irsSubmissionXML">The irs submission XML.</param>
        public StateSubmissionArchive(String submissionId, SubmissionManifest submissionManifest,
                SubmissionXml submissionXML, String archiveOutputLocation,
                SubmissionXml irsSubmissionXML)
            : base(submissionId, submissionManifest, submissionXML, null, archiveOutputLocation)
        {
            this.irsSubmissionXML = irsSubmissionXML;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StateSubmissionArchive"/> class.
        /// </summary>
        /// <param name="submissionId">The submission id.</param>
        /// <param name="submissionManifest">The submission manifest.</param>
        /// <param name="submissionXML">The submission XML.</param>
        /// <param name="irsSubmissionXML">The irs submission XML.</param>
        public StateSubmissionArchive(String submissionId, SubmissionManifest submissionManifest,
                SubmissionXml submissionXML, SubmissionXml irsSubmissionXML)
            : base(submissionId, submissionManifest, submissionXML, null)
        {
            this.irsSubmissionXML = irsSubmissionXML;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StateSubmissionArchive"/> class.
        /// </summary>
        public StateSubmissionArchive()
        {
        }
    }
}