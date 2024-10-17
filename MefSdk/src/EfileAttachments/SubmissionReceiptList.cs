using System.Linq;
using MeF.Client.Exceptions;
using MeF.Client.Helpers;
using MeF.Client.Logging;
using MeF.Client.Validation;

namespace MeF.Client.EfileAttachments
{
    /// <summary>
    /// Contains an array of <see cref="SubmissionReceipt"/>, as well as a property indicating the count of the list.
    /// </summary>
    //[Serializable]
    //[XmlType(AnonymousType = true, Namespace = "http://www.irs.gov/efile")]
    //[XmlRoot(Namespace = "http://www.irs.gov/efile", IsNullable = false)]
    public partial class SubmissionReceiptList : EntityBase<SubmissionReceiptList>, IEfileAttachment
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SubmissionReceiptList"/> class.
        /// </summary>
        //public SubmissionReceiptList()
        //{
        //}

        /// <summary>
        /// Gets or sets the count.
        /// </summary>
        /// <value>The count.</value>
        //[XmlElement]
        //public decimal Count { get; set; }

        /// <summary>
        /// Gets or sets the List of <see cref="SubmissionReceipt"/>.
        /// </summary>
        /// <value>The List of <see cref="SubmissionReceipt"/>.</value>
        //[XmlElement("SubmissionReceipt")]
        //public List<SubmissionReceipt> SubmissionReceipt { get; set; }

        /// <summary>
        /// Loads an SubmissionReceipt list from the specified File.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns>SubmissionReceiptList</returns>
        public SubmissionReceiptList Load(string filePath)
        {
            MeFLog.LogDebug(string.Format("SubmissionReceiptList.Load filePath: {0}", filePath));
            Validate.IsValidFilePath(filePath, "Invalid filePath", "Load");
            Validate.IsValidXdoc(filePath, "SubmissionReceiptList", "", "SubmissionReceiptList.Load");
            var srlist = LoadFromFile(filePath);
            return srlist;
        }

        public SubmissionReceiptList Load(byte[] unzipped)
        {
            var srlist = LoadFromBytes(unzipped);
            return srlist;
        }

        /// <summary>
        /// Saves the SubmissionReceipt list.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        public void Save(string filePath)
        {
            MeFLog.LogDebug(string.Format("SubmissionReceiptList.Save filePath: {0}", filePath));
            Validate.IsValidFilePath(filePath, "Invalid filePath", "Save");
            var doc = XmlHelper.SerializeToXmlDocument(this, typeof(SubmissionReceiptList), "");

            SaveToFileAs(filePath, doc);
        }

        /// <summary>
        /// Gets an <see cref="SubmissionReceipt"/> from the SubmissionReceiptList by submission id.
        /// </summary>
        /// <param name="submissionId">The submission id.</param>
        /// <returns>A Single SubmissionReceipt</returns>
        public SubmissionReceiptGrp FindBySubmissionId(string submissionId)
        {
            MeFLog.LogDebug(string.Format("SubmissionReceiptList.FindBySubmissionId SubmissionID: {0}", submissionId));
            Validate.IsSubmissionIDValid(submissionId, "Invalid Submission ID", "FindBySubmissionId");
            SubmissionReceiptGrp submissionReceipt = (from s in this.SubmissionReceiptGrp
                                                   where s.SubmissionId == submissionId
                                                      select s).SingleOrDefault() as SubmissionReceiptGrp;

            return submissionReceipt;
        }

        /// <summary>
        /// Inserts an <see cref="SubmissionReceipt"/> into the SubmissionReceiptList and updates the count.
        /// </summary>
        /// <param name="submissionReceipt">The submissionReceipt.</param>
        /// <returns>The SubmissionId of the new SubmissionReceipt</returns>
        public string Insert(SubmissionReceiptGrp submissionReceipt)
        {
            MeFLog.LogDebug("SubmissionReceiptList.Insert");
            Validate.IsSubmissionIDValid(submissionReceipt.SubmissionId, "Invalid Submission ID", "FindBySubmissionId");

            foreach (var sr in SubmissionReceiptGrp)
            {
                if (sr.SubmissionId == submissionReceipt.SubmissionId)
                {
                    throw new ToolkitException("Unable to insert new SubmissionReceipt. List already contains a SubmissionReceipt with that submissionId");
                }
            }

            this.SubmissionReceiptGrp.Add(submissionReceipt);
            this.Cnt = SubmissionReceiptGrp.Count;
            return submissionReceipt.SubmissionId;
        }

        /// <summary>
        /// Updates an <see cref="SubmissionReceipt"/> in the SubmissionReceiptList by its submissionId.
        /// </summary>
        /// <param name="submissionReceipt">The submissionReceipt.</param>
        public void Update(SubmissionReceiptGrp submissionReceipt)
        {
            MeFLog.LogDebug(string.Format("SubmissionReceiptList.Update SubmissionID: {0}", submissionReceipt.SubmissionId));
            this.Delete(submissionReceipt.SubmissionId);

            this.Insert(submissionReceipt);
        }

        /// <summary>
        /// Deletes the <see cref="SubmissionReceipt"/> from the SubmissionReceiptList by its submissionId and updates the count.
        /// </summary>
        /// <param name="submissionId">The submission id.</param>
        public void Delete(string submissionId)
        {
            MeFLog.LogDebug(string.Format("SubmissionReceiptList.Delete SubmissionID: {0}", submissionId));
            SubmissionReceiptGrp submissionReceipt = this.FindBySubmissionId(submissionId);
            this.SubmissionReceiptGrp.Remove(submissionReceipt);
            this.Cnt = SubmissionReceiptGrp.Count;
        }
    }
}