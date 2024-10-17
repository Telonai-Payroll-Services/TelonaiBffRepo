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
using MeF.Client.Logging;
using MeF.Client.Exceptions;
using MeF.Client.Configuration;

namespace MeFWCFClient.Services.CustomTextMessageEncoder
{
    public class CustomTextMessageEncoder : MessageEncoder
    {        
        protected readonly MimeParser mParser = new MimeParser();

        private CustomTextMessageEncoderFactory factory;
        private XmlWriterSettings writerSettings;
        private string contentType;

          private readonly MessageEncoder textEncoder;
          private readonly MessageEncoder mtomEncoder;
          

        public CustomTextMessageEncoder(CustomTextMessageEncoderFactory factory)
        {
            this.factory = factory;
            this.writerSettings = new XmlWriterSettings();
            this.contentType = this.factory.MediaType;          

            textEncoder = new TextMessageEncodingBindingElement(MessageVersion.Soap11, Encoding.UTF8).CreateMessageEncoderFactory().Encoder;
            mtomEncoder = new MtomMessageEncodingBindingElement(MessageVersion.Soap11, Encoding.UTF8).CreateMessageEncoderFactory().Encoder;
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

        public override bool IsContentTypeSupported(string contentType)
        {            
            return this.mtomEncoder.IsContentTypeSupported(contentType);
        }

        /// <summary>
        ///   Soap message is logged if indicated in configuration.
        ///   If the response has mime multipart attachment, attachment is parsed and stored as message properties.
        /// </summary>
        public override Message ReadMessage(ArraySegment<byte> buffer, BufferManager bufferManager, string contentType)
        {
            MemoryStream stream = null;

            Encoding custEnc = new UTF8Encoding(false);

            byte[] msgContents = new byte[buffer.Count];

            Array.Copy(buffer.Array, buffer.Offset, msgContents, 0, buffer.Count);

            if (ClientConfiguration.MeFLogSoap.Equals("Y"))
            {
                try
                {
                    string soapmesage = System.Text.Encoding.Default.GetString(msgContents);
                    int startIndex = soapmesage.IndexOf("<?xml");
                    if (startIndex == -1)
                    {
                        MeFLog.LogInfo("startIndex <?xml : " + startIndex);
                        startIndex = soapmesage.IndexOf("<soap:Envelope");
                    }
                    int endIndex = soapmesage.IndexOf("Envelope>");

                    MeFLog.LogInfo("startIndex : " + startIndex);
                    MeFLog.LogInfo("endIndex Envelope> : " + endIndex);
                    MeFLog.LogInfo("soapmesage length : " + soapmesage.Length);

                    if (startIndex == -1)
                    {
                        MeFLog.LogInfo("Soap Response could not be logged ");
                    }
                    else
                    {
                        MeFLog.LogInfo("Soap Response: " + soapmesage.Substring(startIndex, endIndex + 9 - startIndex));
                    }
                    
                }
                catch (Exception e)
                {
                    MeFLog.LogError("MeFClientSDK000029", "Soap Message could not be logged due to an error", e);
                }
            }

            bufferManager.ReturnBuffer(buffer.Array);

            string mStr = custEnc.GetString(msgContents);
            
            MeFLog.LogInfo("Content Type :" + contentType);

            if (contentType != null && contentType.Contains("multipart/related"))
            {                   
                int startIndex = mStr.IndexOf("<?xml");
                if (startIndex == -1)
                {
                    startIndex = mStr.IndexOf("<soap:Envelope");
                }    
                int endIndex = mStr.IndexOf("--MIME", startIndex);           

                string newString = mStr.Substring(startIndex, endIndex - startIndex);
                stream = new MemoryStream(custEnc.GetBytes(newString));
                stream = removeTokensFromStream(stream);

                MimeContent Content = mParser.DeserializeMimeContent(contentType, msgContents);
                if (Content.Parts.Count > 0)
                {
                    MeFLog.LogInfo("Soap response has multipart Attachment"); 
                    Message Msg = ReadMessage(stream, int.MaxValue);
                    Msg.Properties.Add("SOAP_Attachment", Content.Parts[0].Content);
                    return Msg;
                }
                else
                {
                    MeFLog.LogInfo("Soap response has no multipart Attachment"); 
                }
                
            }
            else
            {
                stream = new MemoryStream(custEnc.GetBytes(mStr));
                stream = removeTokensFromStream(stream);
            }            
            
            return textEncoder.ReadMessage(stream, int.MaxValue);            
            
        }

        public override Message ReadMessage(Stream stream, int maxSizeOfHeaders, string contentType)
        {
            return this.textEncoder.ReadMessage(stream, maxSizeOfHeaders, contentType);

        }

        /// <summary>
        ///   Soap request message is logged if indicated in configuration.
        ///   SAML token that was received from login response is attached to logout request.
        /// </summary>
        public override ArraySegment<byte> WriteMessage(Message message, int maxMessageSize, BufferManager bufferManager, int messageOffset)
        {   
            MemoryStream stream = new MemoryStream();
            string samlstring = "";
            try
            {
                if (message.Properties.ContainsKey("SAMLAssertion"))
                {
                    samlstring = (String)message.Properties["SAMLAssertion"];                    
                }               

                XmlWriter writer = XmlWriter.Create(stream, this.writerSettings);

                message.WriteMessage(writer);

                writer.Close();

                stream.Position = 0;

                XElement xmlMessage = XElement.Load(stream);

                XElement samlXml = XDocument.Parse(samlstring).Root;

                XNamespace sec = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd";

                xmlMessage.Descendants(sec + "Security").First().Add(samlXml);

                stream.Position = 0;

                xmlMessage.Save(stream);               

            }
            catch (Exception e)
            {
                MessageBuilder.ProcessUnexpectedException(e);
                throw new ToolkitException("MeFClientSDK000029", e);
            }

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
            
            // find and persist the SAML Assertion

            XElement samlAssertion = xmlMessage.Descendants(saml + "Assertion").FirstOrDefault();

            // remove the unexpected security header

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
