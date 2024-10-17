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
using MeFWCFClient.MeFTransmitterServices;


namespace MeFWCFClient.Services.CustomTextMessageEncoderMTOM
{
    public class CustomTextMessageEncoder : MessageEncoder
    {        

        protected readonly MeFWCFClient.Services.CustomTextMessageEncoder.MimeParser mParser = 
            new MeFWCFClient.Services.CustomTextMessageEncoder.MimeParser();            

        private CustomTextMessageEncoderFactory factory;
        private XmlWriterSettings writerSettings;

        [ThreadStaticAttribute()]
        private static string contentType;

        private readonly MessageEncoder textEncoder;
        private readonly MessageEncoder mtomEncoder;

        //for mtom message
        [ThreadStaticAttribute()]
        private static string boundary;

        [ThreadStaticAttribute()]
        private static string startUri;

        [ThreadStaticAttribute()]
        private static bool mtomEncode=false;

        [ThreadStaticAttribute()]
        private static byte[] bytes;


        //set mtom message related parameters
        public static void setParameters(string boundary, string startUri, string contentType, bool mtomEncode, byte[] bytes)
        {
            CustomTextMessageEncoder.boundary = boundary;
            CustomTextMessageEncoder.startUri = startUri;
            CustomTextMessageEncoder.contentType = contentType;
            CustomTextMessageEncoder.mtomEncode = mtomEncode;
            CustomTextMessageEncoder.bytes = bytes;
        }

        public CustomTextMessageEncoder(CustomTextMessageEncoderFactory factory)
        {
            this.factory = factory;
            this.writerSettings = new XmlWriterSettings();
            CustomTextMessageEncoder.contentType = this.factory.MediaType;

            textEncoder = new TextMessageEncodingBindingElement(MessageVersion.Soap11, Encoding.UTF8).CreateMessageEncoderFactory().Encoder;
            mtomEncoder = new MtomMessageEncodingBindingElement(MessageVersion.Soap11, Encoding.UTF8).CreateMessageEncoderFactory().Encoder;
        }

        public override string ContentType
        {
            get
            {
                return CustomTextMessageEncoder.contentType;
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
                    //MeFLog.LogInfo("Response: " + soapmesage);
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
                //find the boundary
                string msg_boundary = "";
                string[] lineArr = contentType.Split(';');

                //parse the line
                for (int i = 0; i < lineArr.Length; i++)
                {
                    if (lineArr[i] != null)
                        lineArr[i] = lineArr[i].Trim();

                    if (lineArr[i].StartsWith("boundary="))
                    {
                        string[] valArr = lineArr[i].Substring("boundary=".Length).Split('"');
                        for (int j = 0; j < valArr.Length; j++)
                        {
                            if (!valArr[j].Equals(""))
                            {
                                msg_boundary = valArr[j];
                                break;
                            }
                        }
                    }
                }
                int endIndex = mStr.IndexOf("--"+msg_boundary, startIndex);           

                string newString = mStr.Substring(startIndex, endIndex - startIndex);
                stream = new MemoryStream(custEnc.GetBytes(newString));
                stream = removeTokensFromStream(stream);

                MeFWCFClient.Services.CustomTextMessageEncoder.MimeContent Content = mParser.DeserializeMimeContent(contentType, msgContents);
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
        ///   Soap message is logged if indicated in configuration           
        ///   SAML token that was received from login response is attached to request.
        ///   For large message, use MTOM encoding and create multi-parts request message
        /// </summary>
        public override ArraySegment<byte> WriteMessage(Message message, int maxMessageSize, BufferManager bufferManager, int messageOffset)
        {                       
            //variables
            MemoryStream stream = new MemoryStream();
            XmlReader reader = null;
            string samlstring = "";
            XmlWriter writer = null;

            try
            {
                if (ClientConfiguration.MeFLogSoap.Equals("Y"))
                {
                    MeFLog.LogInfo("Soap Request: " + message.ToString());
                }

                //saml string
                if (message.Properties.ContainsKey("SAMLAssertion"))
                {
                    samlstring = (String)message.Properties["SAMLAssertion"];
                }

                if (mtomEncode)
                {
                    //mtom string
                    mtomEncode = false;
                    ArraySegment<byte> mtomMsg = this.mtomEncoder.WriteMessage(message, maxMessageSize, bufferManager, messageOffset);
                    string mtomStr = Encoding.UTF8.GetString(mtomMsg.Array);

                    //variables
                    string aLine="";
                    string mtom_boundary = "";
                    StringBuilder aParagraph = new StringBuilder();
                    StringReader strReader = new StringReader(mtomStr);
                    string xmlStr = "";
                    bool startXML = false;
                    bool done = false;
                    bool isStartUri = true;

                    //loop through string 
                    while (true)
                    {
                        aLine = strReader.ReadLine();
                        if (aLine != null)
                        {
                            if (mtom_boundary.Equals(""))
                            {
                                if (aLine.StartsWith("MIME-Version: "))
                                {
                                }
                                else if (aLine.StartsWith("Content-Type: "))
                                {
                                    //read the line
                                    string tmpLine = "Content-Type: ";
                                    string line = aLine.Substring("Content-Type: ".Length);
                                    string[] lineArr = line.Split(';');

                                    //parse the line
                                    for (int i = 0; i < lineArr.Length; i++)
                                    {
                                        if (lineArr[i] != null)
                                            lineArr[i] = lineArr[i].Trim();

                                        if (lineArr[i].StartsWith ("boundary="))
                                        {
                                            string[] valArr = lineArr[i].Substring("boundary=".Length ).Split('"');
                                            for (int j = 0; j < valArr.Length; j++)
                                            {
                                                if (!valArr[j].Equals(""))
                                                {
                                                    mtom_boundary = valArr[j];
                                                    break;
                                                }
                                            }

                                            //use the bounday generated instead of mtom
                                            tmpLine = tmpLine + "boundary=\"" + boundary + "\""; ;
                                        }                                            
                                        else if (lineArr[i].StartsWith("start="))
                                        {
                                            tmpLine = tmpLine + "start=" + "\"<" + startUri + ">\"";
                                        }
                                        else
                                        {
                                            tmpLine = tmpLine + lineArr[i];
                                        }

                                        if(i<lineArr.Length-1)
                                            tmpLine = tmpLine + ";";
                                    }

                                    //assign back the result
                                    aLine = tmpLine;
                                }      
                            }
                            else
                            {
                                if (aLine.Equals("--"+mtom_boundary))
                                {
                                    if (startXML)
                                    {
                                        //set flag
                                        startXML = false;
                                            
                                        //insert saml
                                        XElement xmlMessage = XElement.Parse(xmlStr);
                                        XElement samlXml = XDocument.Parse(samlstring).Root;
                                        XNamespace sec = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd";
                                        xmlMessage.Descendants(sec + "Security").First().Add(samlXml);

                                        //append the XML
                                        string newXmlStr = "";
                                        string aLineXml="";
                                        StringReader xmlReader = new StringReader(xmlMessage.ToString());
                                        while (true)
                                        {
                                            aLineXml = xmlReader.ReadLine();
                                            if(aLineXml==null )
                                                break;
                                            newXmlStr = newXmlStr + aLineXml.Trim();
                                        }
                                        aParagraph = aParagraph.Append("\r\n").Append(newXmlStr).Append("\r\n");
                                    }
                                    
                                    //append the line
                                    aParagraph = aParagraph.Append("--"+boundary).Append("\r\n");
                                }
                                else
                                {
                                    if (aLine .StartsWith ("Content-ID: "))
                                    {
                                        if (isStartUri)
                                        {
                                            aParagraph = aParagraph.Append("Content-ID: ").Append("<" + startUri + ">").Append("\r\n");
                                            isStartUri = false;
                                        }
                                        else
                                            aParagraph = aParagraph.Append(aLine).Append("\r\n");
                                    }
                                    else if (aLine.StartsWith("Content-Transfer-Encoding: "))
                                    {
                                        aParagraph = aParagraph.Append(aLine).Append("\r\n");
                                    }
                                    else if (aLine.StartsWith("Content-Type: "))
                                    {
                                        //find the content type
                                        string line = aLine.Substring("Content-Type: ".Length);
                                        string[] lineArr = line.Split(';');
                                        for (int i = 0; i < lineArr.Length; i++)
                                        {
                                            string[] itemArr = lineArr[i].Split('=');
                                            if (itemArr[0].ToLower ().Equals("type"))
                                            {
                                                if (itemArr[1].ToLower ().IndexOf ("text/xml") >-1)
                                                    startXML = true;
                                            }
                                        }
                                        aParagraph = aParagraph.Append(aLine).Append("\r\n");

                                        //read in the rest of the string
                                        if (!startXML)
                                        {
                                            aParagraph.Append("\r\n");                                            
                                            done = true;
                                        }
                                    }                                    
                                    else
                                    {
                                        if (startXML)
                                            xmlStr = xmlStr+ aLine ;
                                        else
                                            aParagraph = aParagraph.Append(aLine).Append("\r\n");
                                    }
                                }
                            }
                        }
                        else
                        {
                            break;
                        }

                        if (done)
                            break;
                    }

                    //return value
                    string newStr = aParagraph.ToString ();
                    int messageLength = newStr.Length  ;
                    byte[] buf = Encoding.UTF8.GetBytes(newStr);
                    int totalLength = messageLength + messageOffset + bytes.Length ;
                    byte[] totalBytes = bufferManager.TakeBuffer(totalLength);
                    System.Buffer.BlockCopy(buf, 0, totalBytes, messageOffset, messageLength);
                    System.Buffer.BlockCopy(bytes, 0, totalBytes, messageOffset + messageLength, bytes.Length);
                    ArraySegment<byte> byteArray = new ArraySegment<byte>(totalBytes, messageOffset, totalLength);
                    return byteArray;
                }
                else
                {                    
                    writer = XmlWriter.Create(stream, this.writerSettings);
                    message.WriteMessage(writer);
                    writer.Close();
                    stream.Position = 0;

                    XElement xmlMessage = XElement.Load(stream);
                    XElement samlXml = XDocument.Parse(samlstring).Root;
                    XNamespace sec = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd";
                    xmlMessage.Descendants(sec + "Security").First().Add(samlXml);
                    stream.Position = 0;
                    xmlMessage.Save(stream);
                    byte[] messageBytes = stream.GetBuffer();
                    int messageLength = (int)stream.Position;
                    stream.Position = 0;
                    reader = XmlReader.Create(stream);
                    Message newMessage = Message.CreateMessage(reader, int.MaxValue, MessageVersion.Soap11);

                    return this.textEncoder.WriteMessage(newMessage, maxMessageSize, bufferManager, messageOffset);
                }
            }
            catch (Exception e)
            {
                MessageBuilder.ProcessUnexpectedException(e);
                throw new ToolkitException("MeFClientSDK000029", e);
            }
            
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

            try
            {
                // find and persist the SAML Assertion
                XElement samlAssertion = xmlMessage.Descendants(saml + "Assertion").FirstOrDefault();

                // remove the unexpected security header

                xmlMessage.Descendants(sec + "UsernameToken").Remove();
                xmlMessage.Descendants(saml + "Assertion").Remove();
                xmlMessage.Descendants(sec + "BinarySecurityToken").Remove();
                xmlMessage.Descendants(wsu + "Timestamp").Remove();
                xmlMessage.Descendants(sec + "Security").Remove();
            }
            catch (Exception e)
            {
                MeFLog.LogInfo(e.ToString ());
            }
            xmlMessage.Save(outputStream);
            outputStream.Position = 0;

            return outputStream;
        }

    }
}


