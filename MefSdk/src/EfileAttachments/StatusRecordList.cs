using System.Linq;
using MeF.Client.Exceptions;
using MeF.Client.Helpers;
using MeF.Client.Logging;
using MeF.Client.Validation;

namespace MeF.Client.EfileAttachments
{
    /// <summary>
    /// Contains an array of <see cref="StatusRecord"/>, as well as a property indicating the count of the list.
    /// </summary>
    public partial class StatusRecordList : EntityBase<StatusRecordList>, IEfileAttachment
    {
        /// <summary>
        /// Loads an StatusRecord list from the specified File.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns>StatusRecordList</returns>
        public StatusRecordList Load(string filePath)
        {
            MeFLog.LogDebug(string.Format("StatusRecordList.Load filePath: {0}", filePath));
            Validate.IsValidFilePath(filePath, "Invalid filePath", "Load");
            Validate.IsValidXdoc(filePath, "StatusRecordList", "", "StatusRecordList.Load");
            StatusRecordList srlist = LoadFromFile(filePath);
            return srlist;
        }        

        public StatusRecordList Load(byte[] unzippedcontent)
        {           
            StatusRecordList srlist = LoadFromBytes(unzippedcontent);
            return srlist;
        }

        /// <summary>
        /// Saves the StatusRecord list.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        public void Save(string filePath)
        {
            MeFLog.LogDebug(string.Format("StatusRecordList.Save filePath: {0}", filePath));
            Validate.IsValidFilePath(filePath, "Invalid filePath", "Save");
            var doc = XmlHelper.SerializeToXmlDocument(this, typeof(StatusRecordList), "");
            SaveToFileAs(filePath, doc);
        }

        /// <summary>
        /// Gets an <see cref="StatusRecord"/> from the StatusRecordList by submission id.
        /// </summary>
        /// <param name="submissionId">The submission id.</param>
        /// <returns>A Single StatusRecord</returns>
        public StatusRecordGrp FindBySubmissionId(string submissionId)
        {
            MeFLog.LogDebug(string.Format("StatusRecordList.FindBySubmissionId SubmissionID: {0}", submissionId));
            Validate.IsSubmissionIDValid(submissionId, "Invalid Submission ID", "FindBySubmissionId");
            StatusRecordGrp statusRecord = (from s in this.StatusRecordGrp
                                         where s.SubmissionId == submissionId
                                            select s).SingleOrDefault() as StatusRecordGrp;

            return statusRecord;
        }

        /// <summary>
        /// Inserts an <see cref="StatusRecord"/> into the StatusRecordList and updates the count.
        /// </summary>
        /// <param name="statusRecord">The statusRecord.</param>
        /// <returns>The SubmissionId of the new StatusRecord</returns>
        public string Insert(StatusRecordGrp statusRecord)
        {
            MeFLog.LogDebug("StatusRecordList.Insert");
            Validate.IsSubmissionIDValid(statusRecord.SubmissionId, "Invalid Submission ID", "FindBySubmissionId");

            foreach (var sr in StatusRecordGrp)
            {
                if (sr.SubmissionId == statusRecord.SubmissionId)
                {
                    throw new ToolkitException("Unable to insert new StatusRecord. List already contains a StatusRecord with that submissionId");
                }
            }

            this.StatusRecordGrp.Add(statusRecord);
            this.Cnt = StatusRecordGrp.Count;
            return statusRecord.SubmissionId;
        }

        /// <summary>
        /// Updates an <see cref="StatusRecord"/> in the StatusRecordList by its submissionId.
        /// </summary>
        /// <param name="submissionId">The submission id of the StatusRecord to Modify.</param>
        /// <param name="statusRecord">The statusRecord.</param>
        public void Update(string submissionId, StatusRecordGrp statusRecord)
        {
            MeFLog.LogDebug(string.Format("StatusRecordList.Update SubmissionID: {0}", submissionId));
            this.Delete(submissionId);

            this.Insert(statusRecord);
        }

        /// <summary>
        /// Deletes the <see cref="StatusRecord"/> from the StatusRecordList by its submissionId and updates the count.
        /// </summary>
        /// <param name="submissionId">The submission id.</param>
        public void Delete(string submissionId)
        {
            MeFLog.LogDebug(string.Format("StatusRecordList.Delete SubmissionID: {0}", submissionId));
            StatusRecordGrp statusRecord = this.FindBySubmissionId(submissionId);
            this.StatusRecordGrp.Remove(statusRecord);
            this.Cnt = StatusRecordGrp.Count;
        }
    }
}