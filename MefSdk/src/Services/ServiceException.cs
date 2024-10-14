using System;
using System.Text;
using MeF.Client.Logging;
using System.Security.Permissions;
using System.Runtime.Serialization;

namespace MeF.Client.Exceptions
{
    /// <summary>
    /// Contains information related to any SoapException or WebException during the request or response.
    /// </summary>
    [Serializable()]
    public class ServiceException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceException"/> class.
        /// </summary>
        public ServiceException()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public ServiceException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="ex">The ex.</param>
        public ServiceException(string message, System.Exception ex)
            : base(message, ex)
        {
            try
            {
                StringBuilder trace = new StringBuilder();
                trace.AppendLine("----------------ServiceException Detail ----------------");
                trace.AppendLine("Declaring Type	: " + ex.TargetSite.DeclaringType.FullName.ToString());
                trace.AppendLine("Message	    : " + ex.Message.ToString());
                trace.AppendLine("Source		: " + ex.Source.ToString().Trim());
                trace.AppendLine("Method		: " + ex.TargetSite.Name.ToString());
                trace.AppendLine("Date		: " + DateTime.Now.ToShortDateString());
                trace.AppendLine("Time		: " + DateTime.Now.ToLongTimeString());

                trace.AppendLine("Error		: " + ex.Message.ToString().Trim());
                trace.AppendLine("Stack Trace	: " + ex.StackTrace.ToString().Trim());
                trace.AppendLine("---------------------------------------------------------");
                message = trace.ToString();
            }
            catch (System.IO.IOException ioEx)
            {
                throw new ToolkitException("Could not create/update Trace MeFLog file. Ensure folder/file has correct permissions and MeFLog values are set." + ioEx.Message);
            }

            Audit.Write(message);
            MeFLog.Write(message);
        }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter=true)]
        protected ServiceException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        
        }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter=true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

       
        }

    }
}