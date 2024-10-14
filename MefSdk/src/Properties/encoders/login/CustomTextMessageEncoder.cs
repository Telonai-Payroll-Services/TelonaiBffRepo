using System;
using System.IO;
using System.ServiceModel.Channels;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Net;
using System.Net.Security;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;
using System.Xml.Serialization;
using System.Reflection;
using System.Resources;
using System.Collections;
using System.ServiceModel;
using System.ServiceModel.Security;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel.Security.Tokens;
using System.ServiceModel.Description;
using System.ComponentModel;
using System.ServiceModel.Configuration;
using MeF.Client.Configuration;
using MeF.Client.Logging;

namespace MeFWCFClient.Login.CustomTextMessageEncoder
{
    public class CustomTextMessageEncoder : MessageEncoder
    {
        private CustomTextMessageEncoderFactory factory;
        private XmlWriterSettings writerSettings;
        private string contentType;
        private string samltoken;
        
        public CustomTextMessageEncoder(CustomTextMessageEncoderFactory factory)
        {
            this.factory = factory;                        
            this.writerSettings = new XmlWriterSettings();            
            this.contentType = this.factory.MediaType;
        }

        public override string ContentType
        {
            get
            {
                return this.contentType;
            }
        }

        public override string MediaType
        {
            get 
            {
                return factory.MediaType;
            }
        }

        public override MessageVersion MessageVersion
        {
            get 
            {
                return this.factory.MessageVersion;
            }
        }

        /// <summary>
        ///   SAML token that came with the response is extracted for further service calls
        /// </summary>
        public override Message ReadMessage(ArraySegment<byte> buffer, BufferManager bufferManager, string contentType)
        {
           
            byte[] msgContents = new byte[buffer.Count];

            Array.Copy(buffer.Array, buffer.Offset, msgContents, 0, buffer.Count);

            if (ClientConfiguration.MeFLogSoap.Equals("Y"))
            {
                MeFLog.LogInfo("Soap Response: " + Encoding.UTF8.GetString(msgContents));
            }

            bufferManager.ReturnBuffer(buffer.Array);



            MemoryStream stream = new MemoryStream(msgContents);

            stream = removeTokensFromStream(stream);

            Message Msg = ReadMessage(stream, int.MaxValue);

            Msg.Properties.Add("SAMLAssertion", samltoken);

            return Msg;
        }

        public override Message ReadMessage(Stream stream, int maxSizeOfHeaders, string contentType)
        {

            XmlReader reader = XmlReader.Create(stream);
            return Message.CreateMessage(reader, maxSizeOfHeaders, this.MessageVersion);
        }

        /// <summary>
        ///   Soap message is logged if indicated in configuration
        /// </summary>
        public override ArraySegment<byte> WriteMessage(Message message, int maxMessageSize, BufferManager bufferManager, int messageOffset)
        {
            
            MemoryStream stream = new MemoryStream();
            XmlWriter writer = XmlWriter.Create(stream, this.writerSettings);
            message.WriteMessage(writer);
            writer.Close();
            
            byte[] messageBytes = stream.GetBuffer();
            int messageLength = (int)stream.Position;
            stream.Close();

            int totalLength = messageLength + messageOffset;
            byte[] totalBytes = bufferManager.TakeBuffer(totalLength);
            Array.Copy(messageBytes, 0, totalBytes, messageOffset, messageLength);

            ArraySegment<byte> byteArray = new ArraySegment<byte>(totalBytes, messageOffset, messageLength);

            if (ClientConfiguration.MeFLogSoap.Equals("Y"))
            {                
                MeFLog.LogInfo("Soap Request: " + message.ToString());
            }

            return byteArray;
        }

        public override void WriteMessage(Message message, Stream stream)
        {
            XmlWriter writer = XmlWriter.Create(stream, this.writerSettings);
            message.WriteMessage(writer);
            writer.Close();
        }

        /// <summary>
        ///   Security headers and elements are stripped from the response since WCF client cannot handle it.
        /// </summary>
        private MemoryStream removeTokensFromStream(MemoryStream message)
        {

            MemoryStream outputStream = new MemoryStream();

            MemoryStream samlstream = new MemoryStream();

            message.Position = 0;

            XElement xmlMessage = XElement.Load(message);

            XNamespace sec = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd";

            XNamespace saml = "urn:oasis:names:tc:SAML:1.0:assertion";

            XNamespace wsu = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd";

            XElement samlAssertion = xmlMessage.Descendants(saml + "Assertion").FirstOrDefault();

            if (samlAssertion != null)
            {
                samltoken = samlAssertion.ToString();
            }
            else
            {
                samltoken = "NoSAMLFound";
            }

            xmlMessage.Descendants(sec + "UsernameToken").Remove();
            xmlMessage.Descendants(saml + "Assertion").Remove();
            xmlMessage.Descendants(sec + "BinarySecurityToken").Remove();
            xmlMessage.Descendants(wsu + "Timestamp").Remove();
            xmlMessage.Descendants(sec + "Security").Remove();

            xmlMessage.Save(outputStream);
            outputStream.Position = 0;

            return outputStream;

        }
    }
}
