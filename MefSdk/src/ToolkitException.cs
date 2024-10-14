using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace MeF.Client.Exceptions
{
    /// <summary>
    /// Contains information related to an exception within the SDK including the errorcode and message for the exception.
    /// </summary>
    [Serializable]
    public class ToolkitException : Exception
    {
        /// <summary>
        /// Toolkit Exception
        /// </summary>
        public ToolkitException() : base() { }

        /// <summary>
        /// Toolkit Exception
        /// </summary>
        /// <param name="message">Message</param>
        public ToolkitException(string message) : base(message) { }

        /// <summary>
        /// Toolkit Exception
        /// </summary>
        /// <param name="message">Message</param>
        public ToolkitException(string message, string errorCode) : base(message) { }

        /// <summary>
        /// Toolkit Exception
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="innerException">Inner Exception</param>
        public ToolkitException(string message, Exception innerException) : base(message, innerException) { }

        /// <summary>
        /// Toolkit Exception
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="innerException">Inner Exception</param>
        public ToolkitException(string message, string errorCode, Exception innerException) : base(message, innerException) { }

        /// <summary>
        /// Protected constructor, used by serialization frameworks while
        /// deserializing an exception object.
        /// </summary>
        /// <param name="info">Info about the serialization context.</param>
        /// <param name="context">A streaming context that represents the
        /// serialization stream.</param>
        protected ToolkitException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            this.errorCode = (string)info.GetValue("ErrorCode", typeof(string));
        }

        /// <summary>
        /// This method is called by serialization frameworks while serializing
        /// an exception object.
        /// </summary>
        /// <param name="info">Info about the serialization context.</param>
        /// <param name="context">A streaming context that represents the
        /// serialization stream.</param>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("ErrorCode", errorCode, typeof(string));
        }

        /// <summary>
        /// Error code associated with this exception.
        /// </summary>
        public string errorCode;

        /// <summary>
        /// Formats the specified error code.
        /// </summary>
        /// <param name="errorCode">The error code.</param>
        /// <param name="message">The message.</param>
        /// <param name="args">The args.</param>
        /// <returns></returns>
        public static ToolkitException Format(string errorCode, string message, params object[] args)
        {
            return new ToolkitException(string.Format(errorCode, message, args));
        }
    }
}