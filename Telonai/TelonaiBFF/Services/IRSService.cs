using System.IO.Compression;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using TelonaiWebApi.Models.IRS;
namespace TelonaiWebApi.Services
{
    public interface IIRSService
    {
        public string GenerateIRS491XML(IRS941 irs941Data);
        public void ZipXMLData(string xml);
    }
    public class IRSService : IIRSService
    {
        public string GenerateIRS491XML(IRS941 irs941Data)
        {
           XmlWriterSettings settings = new XmlWriterSettings();
           settings.OmitXmlDeclaration = false;
           var irs491Serializer = new XmlSerializer(typeof(IRS941));
           var irs491XmlData = new StringBuilder();
           var irs491Writer = XmlWriter.Create(irs491XmlData, settings);
           var xmlns = new XmlSerializerNamespaces();
           xmlns.Add(string.Empty, string.Empty);
           irs491Serializer.Serialize(irs491Writer, irs941Data, xmlns);
           
           return irs491XmlData.ToString();
        }

        public void ZipXMLData(string xml)
        {
            string zipFilePath = @"C:\Test\output.zip";

            // File content to zip (simulated using a MemoryStream)
            using (var fileStream = new MemoryStream())
            using (var writer = new StreamWriter(fileStream))
            {
                writer.Write(xml);
                writer.Flush();  // Flush the StreamWriter to ensure content is written to the stream
                fileStream.Position = 0;  // Reset the stream position for reading

                // Create a new zip file and add the file stream to it
                using (var zipStream = new FileStream(zipFilePath, FileMode.Create))
                using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create))
                {
                    var zipEntry = archive.CreateEntry("file.txt");

                    // Copy the file stream content to the zip entry
                    using (var entryStream = zipEntry.Open())
                    {
                        fileStream.CopyTo(entryStream);
                    }
                }
            }
        }
    }
}