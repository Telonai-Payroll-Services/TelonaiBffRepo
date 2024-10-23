namespace MefClientSdkConsole
{
    using System;
    using MeF.Client;
    using MeF.Client.Services.MSIServices;
    using MeF.Client.Services.ETECTransmitterServices;
    using MeF.Client.Services.StateServices;
    using MeF.Client.Services.TransmitterServices;
    using System.Collections.Generic;

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
        private static string etin;
        private static string appSysId;

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
                        CallGetNewSubmissionsStatus();
                        break;
                        //Send submission
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
            Util.WriteInput("Enter Your selection (0 to exit): ");
        }


        private static void CreateServiceContext()
        {
            Util.WriteMessage("Creating a new ServiceContext");
            try
            {
                Util.WriteInput("  Enter your Etin:");
                etin = Console.ReadLine();
                Util.WriteInput("  Enter your AppSysId:");
                appSysId = Console.ReadLine();
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
                Util.WriteError(e.Message);
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
                var client = new GetNewSubmissionsStatusClient(Environment.CurrentDirectory + "\\");
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

        private static void CallSendSubmissionsClient()
        {
            try
            {
                Util.Write("  Enter the maxResults:");
                Util.WriteInput("Enter Your selection (0 to cancel): ");

                var list = new List<MeF.Client.Services.InputComposition.SubmissionArchive> { };
                var container = new SubmissionContainer(list,"");
                
                //Create the client using the current directory for simplicity
                var client = new SendSubmissionsClient(Environment.CurrentDirectory + "\\");
                var result = client.Invoke(context, container);
                Util.WriteMessage("Result:");

                Util.WriteResult(string.Format(" Attachment Path: {0}", result.AttachmentFilePath));
                Util.WriteResult(string.Format(" Deposite ID: {0}", result.DepositId));
                Util.WriteResult(string.Format(" Deposite ID: {0}", result.MessageID));
                Util.WriteResult(string.Format(" Deposite ID: {0}", result.FileName));
                
                var unzippedContent= result.unzippedcontent;

                System.Text.UTF8Encoding enc = new System.Text.UTF8Encoding();
                string xmlString = enc.GetString(unzippedContent);

                Util.WriteResult(string.Format(" XML Content: {0}", xmlString));

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
