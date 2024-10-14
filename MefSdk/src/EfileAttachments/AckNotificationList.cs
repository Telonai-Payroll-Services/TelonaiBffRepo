using System;
using System.Linq;
using System.Xml.Serialization;
using MeF.Client.Exceptions;
using MeF.Client.Helpers;
using MeF.Client.Logging;
using MeF.Client.Validation;

namespace MeF.Client.EfileAttachments
{
    /// <summary>
    /// Contains an array of <see cref="AckNotification"/>, as well as a property indicating the count of the list.
    /// </summary>
    public partial class AckNotificationList : EntityBase<AckNotificationList>, IEfileAttachment
    {
        /// <summary>
        /// Gets the file path.
        /// </summary>
        [XmlIgnore]
        public string FilePath { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AckNotificationList"/> class.
        /// </summary>
        //public AckNotificationList()
        //{
        //    this.Count = 0;
        //    this.AckNotification = new List<AckNotification>();
        //}

        /// <summary>
        /// Initializes a new instance of the <see cref="AckNotificationList"/> class
        /// from a existing file.
        /// </summary>
        /// <param name="FilePath">The AckNotificationList file path.</param>
        public AckNotificationList(string FilePath)
        {
            Load(FilePath);
        }

        /// <summary>
        /// Loads an AckNotification list from the specified File.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns>AckNotificationList</returns>
        public AckNotificationList Load(string filePath)
        {
            MeFLog.LogDebug(string.Format("AckNotificationList.Load filePath: {0}", filePath));
            Validate.IsValidFilePath(filePath, "Invalid filePath", "Load");
            Validate.IsValidXdoc(filePath, "AckNotificationList", "", "AckNotificationList.Load");
            AckNotificationList ackList = LoadFromFile(filePath);

            return ackList;
        }

        public AckNotificationList Load(byte[] unzippedcontent)
        {
            AckNotificationList ackList = LoadFromBytes(unzippedcontent);
            return ackList;
           
        }

        /// <summary>
        /// Saves the AckNotification list.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        public void Save(string filePath)
        {
            MeFLog.LogDebug(string.Format("AckNotificationList.Save filePath: {0}", filePath));
            Validate.IsValidFilePath(filePath, "Invalid filePath", "Save");
            var doc = XmlHelper.SerializeToXmlDocument(this, typeof(AckNotificationList), "");
            SaveToFileAs(filePath, doc);
        }

        /// <summary>
        /// Gets an <see cref="AckNotification"/> from the AckNotificationList by submission id.
        /// </summary>
        /// <param name="submissionId">The submission id.</param>
        /// <returns>A Single AckNotification</returns>
        public AckNotification FindBySubmissionId(string submissionId)
        {
            MeFLog.LogDebug(string.Format("AckNotificationList.FindBySubmissionId SubmissionID: {0}", submissionId));
            Validate.IsSubmissionIDValid(submissionId, "Invalid Submission ID", "FindBySubmissionId");
            AckNotification ackNotification = (from s in this.AckNotification
                                               where s.SubmissionId == submissionId
                                               select s).SingleOrDefault() as AckNotification;

            return ackNotification;
        }

        /// <summary>
        /// Inserts an <see cref="AckNotification"/> into the AckNotificationList and updates the count.
        /// </summary>
        /// <param name="ackNotification">The ackNotification.</param>
        /// <returns>The SubmissionId of the new AckNotification</returns>
        public string Insert(AckNotification ackNotification)
        {
            MeFLog.LogDebug("AckNotificationList.Insert");
            Validate.IsSubmissionIDValid(ackNotification.SubmissionId, "Invalid Submission ID", "FindBySubmissionId");

            foreach (var ack in AckNotification)
            {
                if (ack.SubmissionId == ackNotification.SubmissionId)
                {
                    throw new ToolkitException("Unable to insert new AckNotification. List already contains a AckNotification with that submissionId");
                }
            }

            this.AckNotification.Add(ackNotification);
            this.Cnt = AckNotification.Count;
            return ackNotification.SubmissionId;
        }

        /// <summary>
        /// Updates an <see cref="AckNotification"/> in the AckNotificationList by its submissionId.
        /// </summary>
        /// <param name="submissionId">The submission id of the AckNotification to Modify.</param>
        /// <param name="ackNotification">The ackNotification.</param>
        public void Update(string submissionId, AckNotification ackNotification)
        {
            MeFLog.LogDebug(string.Format("AckNotificationList.Update SubmissionID: {0}", submissionId));
            this.Delete(submissionId);

            ackNotification.Ts = DateTime.Now;

            this.Insert(ackNotification);
        }

        /// <summary>
        /// Deletes the <see cref="AckNotification"/> from the AckNotificationList by its submissionId and updates the count.
        /// </summary>
        /// <param name="submissionId">The submission id.</param>
        public void Delete(string submissionId)
        {
            MeFLog.LogDebug(string.Format("AckNotificationList.Delete SubmissionID: {0}", submissionId));
            AckNotification ackNotification = this.FindBySubmissionId(submissionId);
            this.AckNotification.Remove(ackNotification);
            this.Cnt = AckNotification.Count;
        }
    }
}