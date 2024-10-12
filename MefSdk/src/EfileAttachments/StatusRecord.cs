using System.Xml.Linq;
using System.Xml.Serialization;
using MeF.Client.Helpers;
using MeF.Client.Logging;
using MeF.Client.Validation;

namespace MeF.Client.EfileAttachments
{
    /// <summary>
    /// Contains properties related to an StatusRecordGrp.
    /// </summary>
    //[Serializable]
    //[XmlRoot("StatusRecordGrp")]
    public partial class StatusRecordGrp : EntityBase<StatusRecordGrp>, IEfileAttachment
    {

        /// <summary>
        /// Gets or sets the file path.
        /// </summary>
        /// <value>
        /// The file path.
        /// </value>
        [XmlIgnore]
        public string FilePath { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="StatusRecordGrp"/> class.
        /// </summary>
        public StatusRecordGrp()
        {
            MeFLog.LogInfo("StatusRecordGrp.ctor()");
        }

        //public IEnumerable<StatusRecordGrp> StatusRecordGrpFragments(XElement xml)
        //{
        //  return  EfileServices.LoadXStatusRecordGrp(xml);
        //}

        /// <summary>
        /// Initializes a new instance of the <see cref="StatusRecordGrp"/> class.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        public StatusRecordGrp(string filePath)
        {
            MeFLog.LogInfo(string.Format("StatusRecordGrp.ctor(FilePath: '{0}'", filePath));
            this.Load(filePath);
        }

        /// <summary>
        /// Gets the StatusRecordGrp.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns>The StatusRecordGrp</returns>
        public StatusRecordGrp Load(string filePath)
        {
            MeFLog.LogDebug(string.Format("StatusRecordGrp.Load filePath: {0}", filePath));
            Validate.IsValidFilePath(filePath, "Invalid filePath", "Load");
            Validate.IsValidXdoc(filePath, "StatusRecordGrp", "", "StatusRecordGrp.Load");
            StatusRecordGrp sr = LoadFromFile(filePath);

            return sr;
        }

        /// <summary>
        /// Saves the StatusRecordGrp.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        public void Save(string filePath)
        {
            MeFLog.LogDebug(string.Format("StatusRecordGrp.Save filePath: {0}", filePath));
            Validate.IsValidFilePath(filePath, "Invalid filePath", "Save");
            var doc = XmlHelper.SerializeToXmlDocument(this, typeof(StatusRecordGrp), "");
            doc.Save(filePath);
        }
    }
}