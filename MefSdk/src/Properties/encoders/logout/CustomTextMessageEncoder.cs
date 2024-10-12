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

namespace MeFWCFClient.Logout.CustomTextMessageEncoder
{
    public class CustomTextMessageEncoder : MessageEncoder
    {
        private CustomTextMessageEncoderFactory factory;
        private XmlWriterSettings writerSettings;
        private string contentType;
        
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
        ///   Soap message is logged if indicated in configuration
        /// </summary>
        public override Message ReadMessage(ArraySegment<byte> buffer, BufferManager bufferManager, string contentType)
        {
            byte[] msgContents = new byte[buffer.Count];
	                Array.Copy(buffer.Array, buffer.Offset, msgContents, 0, msgContents.Length);

                    if (ClientConfiguration.MeFLogSoap.Equals("Y"))
                    {
                        MeFLog.LogInfo("Soap Response: " + Encoding.UTF8.GetString(msgContents));
                    }

	                bufferManager.ReturnBuffer(buffer.Array);
	    
	                MemoryStream stream = new MemoryStream(msgContents);
            return ReadMessage(stream, int.MaxValue);
        }

        public override Message ReadMessage(Stream stream, int maxSizeOfHeaders, string contentType)
        {
            XmlReader reader = XmlReader.Create(stream);
            return Message.CreateMessage(reader, maxSizeOfHeaders, this.MessageVersion);
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
                    samlstring = (String) message.Properties["SAMLAssertion"];                    
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

        
    }
}
