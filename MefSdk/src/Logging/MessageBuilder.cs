namespace MeF.Client.Logging
{
    using System;
    using System.Collections.Generic;
    using System.Collections;
    using System.IO;
    using System.Linq;
    using System.Net;
    using MeF.Client.Exceptions;
    using MeF.Client.Helpers;
    using MeFWCFClient.MeFMSIServices;
    using C = MeF.Client.Logging.Constants;
    using WCFClient;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Xml;

    internal static class MessageBuilder
    {
        internal static void AuditSoapFault(FaultException faultException) 
        {         
            var fault = faultException.CreateMessageFault();
            var doc = new XmlDocument();
            var nav = doc.CreateNavigator();
            if (nav != null)
            {
                using (var writer = nav.AppendChild())
                {
                    fault.WriteTo(writer, EnvelopeVersion.Soap11);
                }               
                string s = doc.InnerXml;

                Audit.Write("Soap Fault Information: " + s + "\n");
                MeFLog.Log("Soap Fault Information: " + s + "\n");
            }            
         }        

        internal static void BuildValidationException(String message, String errorCode, String method, String api,
                                                      String value)
        {
            var trace = new List<String>
                {
                    C.ErrorInfoHeader,
                    string.Format(C.LogErrorCode, errorCode),
                    string.Format(C.LogErrorMessage, message),
                    string.Format(C.LogApi, api),
                    string.Format(C.LogTimestamp, DateTime.Now),
                    string.Format(C.LogVersion, C.ToolkitVersion),
                    string.Format(C.LogMethod, method),
                    string.Format(C.LogParameter, value),
                    C.MessageLine
                };

            MeFLog.Log(trace.ToString());
        }

        internal static void AuditRequest(MeFHeaderType mefHeader)
        {
            AuditRequest(mefHeader,null);
        }


        internal static void AuditRequest(MeFHeaderType mefHeader, string requestData)
        {
            if (mefHeader == null)
                return;
            var trace = new List<string>
                {
                    C.MessageLine,
                    C.ClientReqHeader,
                    string.Format(C.LogApi, mefHeader.Action + "Client"),
                    string.Format(C.LogTimestamp, DateTime.Now),
                    C.DashLine,
                    string.Format(C.LogType, "Request"),
                    string.Format(C.LogService, mefHeader.Action),
                    string.Format(C.LogVersion, C.ToolkitVersion),
                    string.Format(C.LogMessageId, mefHeader.MessageID),
                    string.Format(C.LogTimestamp, mefHeader.MessageTs),
                    string.Format(C.LogAppSysId, mefHeader.AppSysID),
                    string.Format(C.LogEtin, mefHeader.ETIN),
                    string.Format(C.LogProd_Ind, mefHeader.TestCd)

                };
            trace.Add(C.DashLine);
            foreach (var s in trace)
            {
                Audit.Write(s);
                MeFLog.Log(s);
            }

            if (requestData == null)
            {
                return;
            }
            Audit.Write(@"*** Request Data:");
            Audit.Write(requestData);
            Audit.Write(@"*** End Request Data ***");
        }

        internal static void AuditRequestData(List<String> auditData)
        {
            if (auditData == null)
            {
                return;
            }
            Audit.Write("*** Request Data:");
            foreach (var s in auditData)
            {
                Audit.Write(s);
            }
            Audit.Write("*** End Request Data ***");
        }

        internal static void AuditResponse(MeFHeaderType mefHeader, ServiceContext serviceContext)
        {
            if (mefHeader == null)
                return;
            var trace = new List<string>
                {
                    C.MessageLine,
                    C.ServiceResHeader,
                    string.Format(C.LogApi, mefHeader.Action + "Client"),
                    string.Format(C.LogTimestamp, DateTime.Now),
                    C.DashLine,
                    string.Format(C.LogType, "Response"),
                    string.Format(C.LogService, mefHeader.Action),
                    string.Format(C.LogMessageId, mefHeader.MessageID),
                    string.Format(C.LogResponseTime, (mefHeader.MessageTs)),
                    string.Format(C.LogRelatesToId, (mefHeader.RelatesTo)),
                    string.Format(C.LogEtin, mefHeader.ETIN),
                    string.Format(C.LogProd_Ind, mefHeader.TestCd),
                    string.Format(C.LogResult, "Success"),
                    C.DashLine
                    
                };

            foreach (var s in trace)
            {
                Audit.Write(s);
                MeFLog.Write(s);
            }

           // AuditSecurityInfo(serviceContext);
        }

        //private static void AuditSecurityInfo(ServiceContext serviceContext)
        //{
        //    //var clientCert = serviceContext.GetSAMLToken();
        //   // if (clientCert == null) return;
        //    Audit.Write(C.SecurityLine);
        //    Audit.Write(string.Format(C.LogToken, clientCert));
        //    Audit.Write(string.Format(C.LogSmsession, clientCert.Id));
        //    Audit.Write(C.DashLine);
        //}

        internal static void ProcessUnexpectedException(Exception e)
        {
            MeFLog.Log(e.StackTrace);
            ProcessToolkitException(new ToolkitException("MeFClientSDK000029", ApiHelper.GetErrorInfo("MeFClientSDK000029"), e));
        }

        internal static void ProcessServiceException(WebException we)
        {
            if (we == null) return;
            var s = new List<string>
                {
                    C.MessageLine,
                    C.ErrorInfoHeader,
                    string.Format(C.LogTimestamp, DateTime.Now),
                    string.Format(C.LogType, we.GetType().Name),
                    string.Format(C.LogErrorMessage,we.Message.GetLogString()),
                    string.Format(C.LogMethod, we.TargetSite.ToString().GetLogString()),
                    string.Format(C.LogSource, we.Source.GetLogString()),
                    string.Format(C.LogStatusCode,we.Status)
                };
            if (we.Response != null )
            {
                //Lets dig deeper, the exception actually has the response details
                using (var response = we.Response)
                {
                    var httpResponse = (HttpWebResponse)response;
                    s.Add(C.InnerHttpLine);
                    s.Add(string.Format(C.LogHttpStatusCode, httpResponse.StatusCode));
                    s.Add(string.Format(C.LogStatusDescription, httpResponse.StatusDescription.GetLogString()));
                    using (var data = response.GetResponseStream())
                    {
                        if (data != null)
                        {
                            try {
                                var text = new StreamReader(data).ReadToEnd();
                                s.Add(string.Format(C.LogFaultDetail, text.GetLogString()));
                            } catch (Exception e) {}
                        }
                    }
                }
            }
            foreach (var l in s)
            {
                Audit.Write(l);
                MeFLog.Write(l);
            }
        }

        internal static void ProcessToolkitException(ToolkitException e)
        {
            if (e == null) return;
            var code = e.errorCode ?? "MeFClientSDK000029";
            var s = new List<String>();
            s.Add(C.ErrorInfoHeader);
            s.Add(string.Format(C.LogTimestamp, DateTime.Now));
            s.Add(string.Format(C.LogType, e.GetType().Name.GetLogString()));
            s.Add(string.Format(C.LogErrorCode, code));
            s.Add(string.Format(C.LogErrorMessage, ApiHelper.GetErrorInfo(code).GetLogString()));
            if (e.TargetSite != null)
            {
                s.Add(string.Format(C.LogMethod, e.TargetSite));
            }
            s.Add(string.Format(C.LogSource, e.Source.GetLogString()));

            WriteData(e.Data, s);
            WriteStackTrace(e.StackTrace, s);
            WriteInnerException(e.InnerException, s);

            foreach (var m in s)
            {
                Audit.Write(m);
                MeFLog.Log(m);
            }

        }

        private static void WriteStackTrace(string p, ICollection<string> s)
        {
            s.Add(C.StackLine);
            s.Add(p);
            s.Add(C.DashLine);
        }

        private static void WriteInnerException(Exception exception, ICollection<string> s)
        {
            if (exception == null) return;

            s.Add(C.InnerExLine);
            s.Add(exception.GetType().Name);
            s.Add(exception.Message);
            s.Add("--- End of Inner Exception ---");
        }

        private static void WriteData(IDictionary iDictionary, List<string> s)
        {
            if (iDictionary.Count == 0 || iDictionary == null) return;
            s.Add(C.AdditionalDataLine);
            s.AddRange(from DictionaryEntry i in iDictionary.Values
                       select string.Format("Data:'{0}'\tValue:'{1}'", i.Key, i.Value ?? "null"));
            s.Add("--- End Additional Data ---");
        }


        internal static string GetLogString(this string s)
        {
            return s ?? "[null]";
        }
    }
}