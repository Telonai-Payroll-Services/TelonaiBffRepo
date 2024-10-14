using System.Linq;
using MeF.Client.Exceptions;
using MeF.Client.Helpers;
using MeF.Client.Logging;
using MeF.Client.Validation;

namespace MeF.Client.EfileAttachments
{
    /// <summary>
    /// Contains an List of <see cref="Acknowledgement"/>, as well as a property indicating the count of the list.
    /// </summary>
    public partial class AcknowledgementList : EntityBase<AcknowledgementList>, IEfileAttachment
    {
        /// <summary>
        /// Loads an Acknowledgement list from the specified File.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns>AcknowledgementList</returns>
        public AcknowledgementList Load(string filePath)
        {
            MeFLog.LogDebug(string.Format("AcknowledgementList.Load filePath: {0}", filePath));
            Validate.IsValidFilePath(filePath, "Invalid filePath", "Load");
            Validate.IsValidXdoc(filePath, "AcknowledgementList", "", "AcknowledgementList.Load");
            AcknowledgementList ackList = LoadFromFile(filePath);
            return ackList;
        }

        public AcknowledgementList Load(byte[] unzippedcontent)
        {
            AcknowledgementList ackList = LoadFromBytes(unzippedcontent);
            return ackList;
        }

        /// <summary>
        /// Saves the Acknowledgement list.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        public void Save(string filePath)
        {
            MeFLog.LogDebug(string.Format("AcknowledgementList.Save filePath: {0}", filePath));
            Validate.IsValidFilePath(filePath, "Invalid filePath", "Save");
            var doc = XmlHelper.SerializeToXmlDocument(this, typeof(AcknowledgementList), "");
            SaveToFileAs(filePath, doc);
        }

        /// <summary>
        /// Gets an <see cref="Acknowledgement"/> from the AcknowledgementList by submission id.
        /// </summary>
        /// <param name="submissionId">The submission id.</param>
        /// <returns>A Single Acknowledgement</returns>
        public Acknowledgement FindBySubmissionId(string submissionId)
        {
            MeFLog.LogDebug(string.Format("AcknowledgementList.FindBySubmissionId SubmissionID: {0}", submissionId));
            Validate.IsSubmissionIDValid(submissionId, "Invalid Submission ID", "FindBySubmissionId");
            Acknowledgement acknowledgement = (from s in this.Acknowledgement
                                               where s.SubmissionId == submissionId
                                               select s).SingleOrDefault() as Acknowledgement;

            return acknowledgement;
        }

        /// <summary>
        /// Inserts an <see cref="Acknowledgement"/> into the AcknowledgementList and updates the count.
        /// </summary>
        /// <param name="acknowledgement">The acknowledgement.</param>
        /// <returns>The SubmissionId of the new Acknowledgement</returns>
        public string Insert(Acknowledgement acknowledgement)
        {
            MeFLog.LogDebug("AcknowledgementList.Insert");
            Validate.IsSubmissionIDValid(acknowledgement.SubmissionId, "Invalid Submission ID", "FindBySubmissionId");

            foreach (var ack in Acknowledgement)
            {
                if (ack.SubmissionId == acknowledgement.SubmissionId)
                {
                    throw new ToolkitException("Unable to insert new Acknowledgement. List already contains a Acknowledgement with that submissionId");
                }
            }

            this.Acknowledgement.Add(acknowledgement);
            this.Cnt = Acknowledgement.Count;
            return acknowledgement.SubmissionId;
        }

        /// <summary>
        /// Updates an <see cref="Acknowledgement"/> in the AcknowledgementList by its submissionId.
        /// </summary>
        /// <param name="submissionId">The submission id of the Acknowledgement to Modify.</param>
        /// <param name="acknowledgement">The acknowledgement.</param>
        public void Update(string submissionId, Acknowledgement acknowledgement)
        {
            MeFLog.LogDebug(string.Format("AcknowledgementList.Update SubmissionID: {0}", submissionId));
            this.Delete(submissionId);

            this.Insert(acknowledgement);
        }

        /// <summary>
        /// Deletes the <see cref="Acknowledgement"/> from the AcknowledgementList by its submissionId and updates the count.
        /// </summary>
        /// <param name="submissionId">The submission id.</param>
        public void Delete(string submissionId)
        {
            MeFLog.LogDebug(string.Format("AcknowledgementList.Delete SubmissionID: {0}", submissionId));
            Acknowledgement acknowledgement = this.FindBySubmissionId(submissionId);
            this.Acknowledgement.Remove(acknowledgement);
            this.Cnt = Acknowledgement.Count;
        }
    }
}