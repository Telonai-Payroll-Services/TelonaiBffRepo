using MeF.Client.Helpers;
using MeF.Client.Logging;
using MeF.Client.Validation;
using System.Xml.Linq;

namespace MeF.Client.EfileAttachments
{
    /// <summary>
    /// Represents an Acknowledgement
    /// </summary>
    /// <remarks/>
    /// <remarks/>
    public partial class Acknowledgement : EntityBase<Acknowledgement>
    {
        //future use of explicit casting
        //public static IEnumerable<Acknowledgement> LoadAcknowledgements(XElement x)
        //{
        //    var acks = EfileServices.LoadXAcknowledgement(x);
        //    return acks;
        //}

        //static Acknowledgement FromXml(XElement x)
        //{
        //    return EfileServices.GetAcknowledgementFromXElement(x);
        //}

        /// <summary>
        /// Gets the Acknowledgement.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns>The Acknowledgement</returns>
        public Acknowledgement(string filePath)
        {
            MeFLog.LogDebug(string.Format("Acknowledgement.ctor filePath: {0}", filePath));
            Validate.IsValidFilePath(filePath, "Invalid filePath", "Load");
            Validate.IsValidXdoc(filePath, "Acknowledgement", "", "Acknowledgement.Load");
            Acknowledgement ack = LoadFromFile(filePath);
        }

        /// <summary>
        /// Saves the Acknowledgement.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        public void Save(string filePath)
        {
            var ack = this;
            MeFLog.LogDebug(string.Format("Acknowledgement.Save filePath: {0}", filePath));
            Validate.IsValidFilePath(filePath, "Invalid filePath", "Save");
            var doc = XmlHelper.SerializeToXmlDocument(this, typeof(Acknowledgement), "");
            doc.Save(filePath);
        }

        /// <summary>
        /// Gets the Acknowledgement.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns>The Acknowledgement</returns>
        public Acknowledgement Load(string filePath)
        {
            MeFLog.LogDebug(string.Format("Acknowledgement.Load filePath: {0}", filePath));
            Validate.IsValidFilePath(filePath, "Invalid filePath", "Load");
            Validate.IsValidXdoc(filePath, "Acknowledgement", "", "Acknowledgement.Load");
            Acknowledgement ack = LoadFromFile(filePath);
            return ack;
        }
    }
}