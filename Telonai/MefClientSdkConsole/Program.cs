namespace MefClientSdkConsole
{
    using System;
    using MeF.Client;
    using MeF.Client.Services.MSIServices;
    using MeF.Client.Services.ETECTransmitterServices;
    using MeF.Client.Services.StateServices;
    using MeF.Client.Services.TransmitterServices;
    using System.Collections.Generic;
    using System.IO;

    ///<summary>
    /// This demo app that is meant to get you familiar with some common api's within the MefClientSdk. 
    /// 
    /// 1) See how the config file works.
    /// 2) Create a ServiceContext and use it to login.
    /// 3) Call a few MsiService apis.
    /// 4) Call EtinStatus.
    /// 5) Also look at invoking api's for state & transmitter services.
    ///</summary>

    class Program
    {
        private static ServiceContext context = new ServiceContext();
        private static string etin="84689";
        private static string appSysId="69321899";

        static void Main(string[] args)
        {
            Util.Write("*** MefClientSdk Simple Setup Demo ***");
            
            bool done = false;
            do
            {

                WriteMenu();


                var iSel = 0;
                try
                {
                    var strSelection = Console.ReadLine();
                    if (strSelection != null)
                    {
                        iSel = int.Parse(strSelection);
                    }

                }
                catch (FormatException fe)
                {
                    Util.WriteError("Unable to process selection...");
                    Util.WriteError(fe.Message);
                }

                switch (iSel)
                {
                    case 0:
                        done = true;
                        break;
                    case 1:
                        CreateServiceContext();
                        break;                    
                    case 2:
                        WriteDetails();
                        break;
                    case 3:
                        CallLogin();
                        break;
                    case 4:
                        CallEtinStatus();
                        break;
                    case 5:
                        CallLogout();
                        break;
                    case 6:
                        CallGetNewAckNotifications();
                        break;
                    case 7:
                        CallGetNewSubmissionsStatus();
                        break;
                    case 8:
                        CallSendSubmissionsFor941Test1();
                        break;
                    case 9:
                        CallSendSubmissionsFor941Test2();
                        break;
                    case 10:
                        CallSendSubmissionsClientFor941Multiple();
                        break;
                    case 11:
                        CallSendSubmissionsFor940Test1();
                        break;
                    case 12:
                        CallSendSubmissionsFor940Test2();
                        break;
                    case 13:
                        CallSendSubmissionsClientFor940Multiple();
                        break;
                    default:
                        Util.Write(string.Format("You selected an invalid number: {0}\r\n", iSel));
                        continue;
                }
                Util.Write("");
            } while (!done);

            Util.Write("\nGoodbye!");
        }


        private static void WriteMenu()
        {
            Util.Write("Select one of the following:");
            Util.WriteResult("ServiceContext & Settings");
            Util.Write("  1) Create a new ServiceContext.");            
            Util.Write("  2) Show current ServiceContext Details.");
            Util.WriteResult("MsiServices");
            Util.Write("  3) Call LoginClient.");
            Util.Write("  4) Call EtinStatusClient. ");
            Util.Write("  5) Call LogoutClient. ");
            Util.WriteResult("StateServices");
            Util.Write("  6) Call GetNewAckNotificationsClient. ");
            Util.WriteResult("TransmitterServices");
            Util.Write("  7) Call GetNewSubmissionsStatusClient. ");        
            Util.Write("  8) Call CallSendSubmissionsClient Test1. ");
            Util.Write("  9) Call CallSendSubmissionsClient Test2. ");
            Util.Write("  10) Call CallSendSubmissionsClient Multiple. ");
            Util.WriteInput("Enter Your selection (0 to exit): ");
        }


        private static void CreateServiceContext()
        {
            Util.WriteMessage("Creating a new ServiceContext");
            try
            {
               // Util.WriteInput("  Enter your Etin:");
               // etin = Console.ReadLine();
              //  Util.WriteInput("  Enter your AppSysId:");
               // appSysId = Console.ReadLine();
                //Create a new service context with the etin and appsysid input
                context = new ServiceContext(new ClientInfo(etin, appSysId));
                Util.WriteResult("ServiceContext has been sucessfully created.");
                WriteDetails();
            }
            catch (Exception e)
            {
                Util.WriteError("An error has occured check the log for details...");
                Util.WriteError(e.Message);
            }
        }



        private static void CallLogin()
        {
            try
            {
                
                var client = new LoginClient();
                var result = client.Invoke(context);
                Util.WriteResult(result.StatusTxt);
                Pause();
            }
            catch (Exception e)
            {
                Util.WriteError("An error has occured check the log for details...");
                Util.WriteError(e.ToString());
            }
        }

        private static void CallEtinStatus()
        {
         
            try
            {

                Util.Write("  Enter the Etin:");
                Util.WriteInput("Enter Your selection (0 to cancel): ");
                var etinC = Console.ReadLine();
                if (etinC == "0") return;
                var client = new EtinStatusClient();
                var result = client.Invoke(context, etinC);

                Util.WriteResult(string.Format("Count: {0}", result.FormStatusGrp.Count));
                Util.WriteResult(string.Format("Etin: {0}", result.ETIN));
                Util.WriteResult(string.Format("Status: {0}", result.StatusTxt));
                Pause();
            }
            catch (Exception e)
            {
                Util.WriteError("An error has occured check the log for details...");
                Util.WriteError(e.Message);
            }
        }

        private static void CallLogout()
        {
           
            try
            {
                var client = new LogoutClient();
                //invoke login using the serviceContext which has hopefully been initialized
                var result = client.Invoke(context);
                Util.WriteResult(result.StatusTxt);
                Pause();
            }
            catch (Exception e)
            {
                Util.WriteError("An error has occured check the log for details...");
                Util.WriteError(e.Message);
            }
        }

        private static void CallGetNewAckNotifications()
        {
            try
            {
                Util.Write("  Enter the maxResults:");
                Util.WriteInput("Enter Your selection (0 to cancel): ");
                var maxResults = Console.ReadLine();
                if (maxResults == "0") return;
                //Create the client using the current directory for simplicity
                var client = new GetNewAckNotificationsClient(Environment.CurrentDirectory + "\\");
                var result = client.Invoke(context, maxResults);
                Util.WriteMessage("Result:");
                Util.WriteResult(string.Format(" Attachment Path: {0}", result.AttachmentFilePath));
                Util.WriteResult(string.Format(" More Available: {0}", result.MoreAvailableInd));
                var list = result.GetAckNotificationList();
                Util.WriteResult(string.Format(" AckNotificationList Count: {0}", list.Cnt));
                Pause();
            }
            catch (Exception e)
            {
                Util.WriteError("An error has occured check the log for details...");
                Util.WriteError(e.Message);
            }
        }

        private static void CallGetNewSubmissionsStatus()
        {
            try
            {
                Util.Write("  Enter the maxResults:");
                Util.WriteInput("Enter Your selection (0 to cancel): ");
                var maxResults = Console.ReadLine();
                if (maxResults == "0") return;
                //Create the client using the current directory for simplicity
                var client = new GetNewSubmissionsStatusClient(Environment.CurrentDirectory + "\\Files\\");
                var result = client.Invoke(context, maxResults);
                Util.WriteMessage("Result:");
                Util.WriteResult(string.Format(" Attachment Path: {0}", result.AttachmentFilePath));
                Util.WriteResult(string.Format(" More Available: {0}", result.MoreAvailableInd));
                var list = result.GetStatusRecordList();
                
                Util.WriteResult(string.Format(" StatusRecordList Count: {0}", list.Cnt));
                Pause();
            }
            catch (Exception e)
            {
                Util.WriteError("An error has occured check the log for details...");
                Util.WriteError(e.Message);
            }
        }

        private static void CallSendSubmissionsFor941Test1()
        {
            try
            {
                Util.Write("  Enter the maxResults:");
                Util.WriteInput("Enter Your selection (0 to cancel): ");

                var list = new List<MeF.Client.Services.InputComposition.PostmarkedSubmissionArchive>();

                var path = Environment.CurrentDirectory + "\\Files1\\";
                var path2 = Environment.CurrentDirectory + "\\Files1Result\\";
                var submissionArch = new MeF.Client.Services.InputComposition.SubmissionArchive
                {  
                    isFileBased =false,
                    submissionId = "69321820243050000011",
                    submissionManifest = new MeF.Client.Services.InputComposition.SubmissionManifest(
                    path + "manifest.xml"),                    
                    submissionXml = new MeF.Client.Services.InputComposition.SubmissionXml(
                        path + "941.xml")
                };

                list.Add(new MeF.Client.Services.InputComposition.PostmarkedSubmissionArchive(submissionArch, DateTime.Now));

                var container = new SubmissionContainer(list);

                //Create the client using the current directory for simplicity
                var client = new SendSubmissionsClient(path2);
                
                var result = client.Invoke(context, container);
                Util.WriteMessage("Result:");

                Util.WriteResult(string.Format(" Attachment Path: {0}", result.AttachmentFilePath));
                Util.WriteResult(string.Format(" DepositId ID: {0}", result.DepositId));
                Util.WriteResult(string.Format(" MessageID ID: {0}", result.MessageID));
                Util.WriteResult(string.Format(" FileNameD: {0}", result.FileName));
                
                var unzippedContent= result.unzippedcontent;

                if (unzippedContent != null)
                {
                    System.Text.UTF8Encoding enc = new System.Text.UTF8Encoding();
                    string xmlString = enc.GetString(unzippedContent);

                    Util.WriteResult(string.Format(" XML Content: {0}", xmlString));
                }
                    var submissionRecieptlist = result.GetSubmissionReceiptList();

                    Util.WriteResult(string.Format(" Submission Receipt List Count: {0}", submissionRecieptlist.Cnt));
                
                Pause();
            }
            catch (Exception e)
            {
                Util.WriteError("An error has occured check the log for details...");
                Util.WriteError(e.Message);
            }
        }

        private static void CallSendSubmissionsFor941Test2()
        {
            try
            {
                Util.Write("  Enter the maxResults:");
                Util.WriteInput("Enter Your selection (0 to cancel): ");

                var list = new List<MeF.Client.Services.InputComposition.PostmarkedSubmissionArchive>();

                var path = Environment.CurrentDirectory + "\\Files2\\";
                var path2 = Environment.CurrentDirectory + "\\Files2Result\\";
                
                var fileStream1 = File.ReadAllBytes(path + "8453-EMP.pdf");
                var fileStream2 = File.ReadAllBytes(path + "ScheduleB.pdf");

                var submissionArch = new MeF.Client.Services.InputComposition.SubmissionArchive
                {
                    isFileBased = false,
                    submissionId = "69321820243050000012",
                    submissionManifest = new MeF.Client.Services.InputComposition.SubmissionManifest(
                    path + "manifest.xml"),
                    submissionXml = new MeF.Client.Services.InputComposition.SubmissionXml(
                        path + "941.xml"), 
                    binaryAttachments = new List<MeF.Client.Services.InputComposition.BinaryAttachment>
                    {
                        new MeF.Client.Services.InputComposition.BinaryAttachment("8453-EMP.pdf",fileStream1),
                        new MeF.Client.Services.InputComposition.BinaryAttachment("ScheduleB.pdf",fileStream2),
                    }
                };

                list.Add(new MeF.Client.Services.InputComposition.PostmarkedSubmissionArchive(submissionArch, DateTime.Now));

                var container = new SubmissionContainer(list);

                //Create the client using the current directory for simplicity
                var client = new SendSubmissionsClient(path2);

                var result = client.Invoke(context, container);
                Util.WriteMessage("Result:");

                Util.WriteResult(string.Format(" Attachment Path: {0}", result.AttachmentFilePath));
                Util.WriteResult(string.Format(" DepositId ID: {0}", result.DepositId));
                Util.WriteResult(string.Format(" MessageID ID: {0}", result.MessageID));
                Util.WriteResult(string.Format(" FileNameD: {0}", result.FileName));

                var unzippedContent = result.unzippedcontent;

                if (unzippedContent != null)
                {
                    System.Text.UTF8Encoding enc = new System.Text.UTF8Encoding();
                    string xmlString = enc.GetString(unzippedContent);

                    Util.WriteResult(string.Format(" XML Content: {0}", xmlString));
                }
                var submissionRecieptlist = result.GetSubmissionReceiptList();

                Util.WriteResult(string.Format(" Submission Receipt List Count: {0}", submissionRecieptlist.Cnt));

                Pause();
            }
            catch (Exception e)
            {
                Util.WriteError("An error has occured check the log for details...");
                Util.WriteError(e.Message);
            }
        }

        private static void CallSendSubmissionsFor941TestMultiple()
        {
            try
            {
                Util.Write("  Enter the maxResults:");
                Util.WriteInput("Enter Your selection (0 to cancel): ");

                var list = new List<MeF.Client.Services.InputComposition.PostmarkedSubmissionArchive>();

                //first submission
                var path = Environment.CurrentDirectory + "\\Files1\\";
                var submissionArch = new MeF.Client.Services.InputComposition.SubmissionArchive
                {
                    isFileBased = false,
                    submissionId = "69321820243050000014",
                    submissionManifest = new MeF.Client.Services.InputComposition.SubmissionManifest(
                    path + "manifest.xml"),
                    submissionXml = new MeF.Client.Services.InputComposition.SubmissionXml(
                        path + "941.xml")
                };

                //second submission
                var path2 = Environment.CurrentDirectory + "\\Files2\\";
                var path3 = Environment.CurrentDirectory + "\\FilesResultMultiple\\";

                var fileStream1 = File.ReadAllBytes(path2 + "8453-EMP.pdf");
                var fileStream2 = File.ReadAllBytes(path2 + "ScheduleB.pdf");

                var submissionArch2 = new MeF.Client.Services.InputComposition.SubmissionArchive
                {
                    isFileBased = false,
                    submissionId = "69321820243050000015",
                    submissionManifest = new MeF.Client.Services.InputComposition.SubmissionManifest(
                    path + "manifest.xml"),
                    submissionXml = new MeF.Client.Services.InputComposition.SubmissionXml(
                        path + "941.xml"),
                    binaryAttachments = new List<MeF.Client.Services.InputComposition.BinaryAttachment>
                    {
                        new MeF.Client.Services.InputComposition.BinaryAttachment("8453-EMP.pdf",fileStream1),
                        new MeF.Client.Services.InputComposition.BinaryAttachment("ScheduleB.pdf",fileStream2),
                    }
                };

                //add both submissions
                list.Add(new MeF.Client.Services.InputComposition.PostmarkedSubmissionArchive(submissionArch, DateTime.Now));
                list.Add(new MeF.Client.Services.InputComposition.PostmarkedSubmissionArchive(submissionArch2, DateTime.Now));

                var container = new SubmissionContainer(list);

                //Create the client using the current directory for simplicity
                var client = new SendSubmissionsClient(path2);

                var result = client.Invoke(context, container);
                Util.WriteMessage("Result:");

                Util.WriteResult(string.Format(" Attachment Path: {0}", result.AttachmentFilePath));
                Util.WriteResult(string.Format(" DepositId ID: {0}", result.DepositId));
                Util.WriteResult(string.Format(" MessageID ID: {0}", result.MessageID));
                Util.WriteResult(string.Format(" FileNameD: {0}", result.FileName));

                var unzippedContent = result.unzippedcontent;

                if (unzippedContent != null)
                {
                    System.Text.UTF8Encoding enc = new System.Text.UTF8Encoding();
                    string xmlString = enc.GetString(unzippedContent);

                    Util.WriteResult(string.Format(" XML Content: {0}", xmlString));
                }
                var submissionRecieptlist = result.GetSubmissionReceiptList();

                Util.WriteResult(string.Format(" Submission Receipt List Count: {0}", submissionRecieptlist.Cnt));

                Pause();
            }
            catch (Exception e)
            {
                Util.WriteError("An error has occured check the log for details...");
                Util.WriteError(e.Message);
            }
        }

        private static void CallSendSubmissionsFor940Test1()
        {
            try
            {
                Util.Write("  Enter the maxResults:");
                Util.WriteInput("Enter Your selection (0 to cancel): ");

                var list = new List<MeF.Client.Services.InputComposition.PostmarkedSubmissionArchive>();

                var path = Environment.CurrentDirectory + "\\940\\Files1\\";
                var path2 = Environment.CurrentDirectory + "\\940\\Files1Result\\";
                var submissionArch = new MeF.Client.Services.InputComposition.SubmissionArchive
                {
                    isFileBased = false,
                    submissionId = "69321820243050000011",
                    submissionManifest = new MeF.Client.Services.InputComposition.SubmissionManifest(
                    path + "manifest.xml"),
                    submissionXml = new MeF.Client.Services.InputComposition.SubmissionXml(
                        path + "940.xml")
                };

                list.Add(new MeF.Client.Services.InputComposition.PostmarkedSubmissionArchive(submissionArch, DateTime.Now));

                var container = new SubmissionContainer(list);

                //Create the client using the current directory for simplicity
                var client = new SendSubmissionsClient(path2);

                var result = client.Invoke(context, container);
                Util.WriteMessage("Result:");

                Util.WriteResult(string.Format(" Attachment Path: {0}", result.AttachmentFilePath));
                Util.WriteResult(string.Format(" DepositId ID: {0}", result.DepositId));
                Util.WriteResult(string.Format(" MessageID ID: {0}", result.MessageID));
                Util.WriteResult(string.Format(" FileNameD: {0}", result.FileName));

                var unzippedContent = result.unzippedcontent;

                if (unzippedContent != null)
                {
                    System.Text.UTF8Encoding enc = new System.Text.UTF8Encoding();
                    string xmlString = enc.GetString(unzippedContent);

                    Util.WriteResult(string.Format(" XML Content: {0}", xmlString));
                }
                var submissionRecieptlist = result.GetSubmissionReceiptList();

                Util.WriteResult(string.Format(" Submission Receipt List Count: {0}", submissionRecieptlist.Cnt));

                Pause();
            }
            catch (Exception e)
            {
                Util.WriteError("An error has occured check the log for details...");
                Util.WriteError(e.Message);
            }
        }

        private static void CallSendSubmissionsFor940Test2()
        {
            try
            {
                Util.Write("  Enter the maxResults:");
                Util.WriteInput("Enter Your selection (0 to cancel): ");

                var list = new List<MeF.Client.Services.InputComposition.PostmarkedSubmissionArchive>();

                var path = Environment.CurrentDirectory + "\\940\\Files2\\";
                var path2 = Environment.CurrentDirectory + "\\940\\Files2Result\\";

                var fileStream1 = File.ReadAllBytes(path + "8453-EMP.pdf");
                var fileStream2 = File.ReadAllBytes(path + "ScheduleB.pdf");

                var submissionArch = new MeF.Client.Services.InputComposition.SubmissionArchive
                {
                    isFileBased = false,
                    submissionId = "69321820243050000012",
                    submissionManifest = new MeF.Client.Services.InputComposition.SubmissionManifest(
                    path + "manifest.xml"),
                    submissionXml = new MeF.Client.Services.InputComposition.SubmissionXml(
                        path + "940.xml"),
                    binaryAttachments = new List<MeF.Client.Services.InputComposition.BinaryAttachment>
                    {
                        new MeF.Client.Services.InputComposition.BinaryAttachment("8453-EMP.pdf",fileStream1),
                        new MeF.Client.Services.InputComposition.BinaryAttachment("ScheduleB.pdf",fileStream2),
                    }
                };

                list.Add(new MeF.Client.Services.InputComposition.PostmarkedSubmissionArchive(submissionArch, DateTime.Now));

                var container = new SubmissionContainer(list);

                //Create the client using the current directory for simplicity
                var client = new SendSubmissionsClient(path2);

                var result = client.Invoke(context, container);
                Util.WriteMessage("Result:");

                Util.WriteResult(string.Format(" Attachment Path: {0}", result.AttachmentFilePath));
                Util.WriteResult(string.Format(" DepositId ID: {0}", result.DepositId));
                Util.WriteResult(string.Format(" MessageID ID: {0}", result.MessageID));
                Util.WriteResult(string.Format(" FileNameD: {0}", result.FileName));

                var unzippedContent = result.unzippedcontent;

                if (unzippedContent != null)
                {
                    System.Text.UTF8Encoding enc = new System.Text.UTF8Encoding();
                    string xmlString = enc.GetString(unzippedContent);

                    Util.WriteResult(string.Format(" XML Content: {0}", xmlString));
                }
                var submissionRecieptlist = result.GetSubmissionReceiptList();

                Util.WriteResult(string.Format(" Submission Receipt List Count: {0}", submissionRecieptlist.Cnt));

                Pause();
            }
            catch (Exception e)
            {
                Util.WriteError("An error has occured check the log for details...");
                Util.WriteError(e.Message);
            }
        }

        private static void CallSendSubmissionsFor940TestMultiple()
        {
            try
            {
                Util.Write("  Enter the maxResults:");
                Util.WriteInput("Enter Your selection (0 to cancel): ");

                var list = new List<MeF.Client.Services.InputComposition.PostmarkedSubmissionArchive>();

                //first submission
                var path = Environment.CurrentDirectory + "\\940\\Files1\\";
                var submissionArch = new MeF.Client.Services.InputComposition.SubmissionArchive
                {
                    isFileBased = false,
                    submissionId = "69321820243050000021",
                    submissionManifest = new MeF.Client.Services.InputComposition.SubmissionManifest(
                    path + "manifest.xml"),
                    submissionXml = new MeF.Client.Services.InputComposition.SubmissionXml(
                        path + "940.xml")
                };

                //second submission
                var path2 = Environment.CurrentDirectory + "\\940\\Files2\\";
                var path3 = Environment.CurrentDirectory + "\\940\\FilesResultMultiple\\";

                var fileStream1 = File.ReadAllBytes(path2 + "8453-EMP.pdf");
                var fileStream2 = File.ReadAllBytes(path2 + "ScheduleB.pdf");

                var submissionArch2 = new MeF.Client.Services.InputComposition.SubmissionArchive
                {
                    isFileBased = false,
                    submissionId = "69321820243050000022",
                    submissionManifest = new MeF.Client.Services.InputComposition.SubmissionManifest(
                    path + "manifest.xml"),
                    submissionXml = new MeF.Client.Services.InputComposition.SubmissionXml(
                        path + "940.xml"),
                    binaryAttachments = new List<MeF.Client.Services.InputComposition.BinaryAttachment>
                    {
                        new MeF.Client.Services.InputComposition.BinaryAttachment("8453-EMP.pdf",fileStream1),
                        new MeF.Client.Services.InputComposition.BinaryAttachment("ScheduleB.pdf",fileStream2),
                    }
                };

                //add both submissions
                list.Add(new MeF.Client.Services.InputComposition.PostmarkedSubmissionArchive(submissionArch, DateTime.Now));
                list.Add(new MeF.Client.Services.InputComposition.PostmarkedSubmissionArchive(submissionArch2, DateTime.Now));

                var container = new SubmissionContainer(list);

                //Create the client using the current directory for simplicity
                var client = new SendSubmissionsClient(path2);

                var result = client.Invoke(context, container);
                Util.WriteMessage("Result:");

                Util.WriteResult(string.Format(" Attachment Path: {0}", result.AttachmentFilePath));
                Util.WriteResult(string.Format(" DepositId ID: {0}", result.DepositId));
                Util.WriteResult(string.Format(" MessageID ID: {0}", result.MessageID));
                Util.WriteResult(string.Format(" FileNameD: {0}", result.FileName));

                var unzippedContent = result.unzippedcontent;

                if (unzippedContent != null)
                {
                    System.Text.UTF8Encoding enc = new System.Text.UTF8Encoding();
                    string xmlString = enc.GetString(unzippedContent);

                    Util.WriteResult(string.Format(" XML Content: {0}", xmlString));
                }
                var submissionRecieptlist = result.GetSubmissionReceiptList();

                Util.WriteResult(string.Format(" Submission Receipt List Count: {0}", submissionRecieptlist.Cnt));

                Pause();
            }
            catch (Exception e)
            {
                Util.WriteError("An error has occured check the log for details...");
                Util.WriteError(e.Message);
            }
        }

        private static void Pause()
        {
            Util.WriteMessage("Press any key to continue....");
            Console.ReadLine();
        }

        
        private static void WriteDetails()
        {
            if (context == null)
            {
                return;
            }
            Util.WriteResult("");
            Util.WriteResult("  Current ServiceContext details:");
            Util.WriteResult(string.Format("  AppSysId: {0}", context.GetClientInfo().AppSysID));
            Util.WriteResult(string.Format("  Etin: {0}", context.GetClientInfo().Etin));
           
            if (context.GetSessionInfo().getCookieColl() != null)
                Util.WriteResult(string.Format("   Cookie: {0}", context.GetSessionInfo().getCookieColl()));

            Pause();
        }
    }
}
