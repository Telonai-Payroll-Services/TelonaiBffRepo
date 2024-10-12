using MeFWCFClient.MeFMSIServices;
using System;

namespace MeF.Client.Services.MSIServices
{
    /// <summary>
    ///Response from LogoutClient that contains the status.
    /// </summary>
    public class LogoutResult
    {
        private LogoutResponseType lResponse;

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        /// <value>The status.</value>
        public string StatusTxt { get; set; }
        public String MessageID { get; set; }
        public String RelatesTo { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogoutResult"/> class.
        /// </summary>
        public LogoutResult()
        {
            if (lResponse == null)
                lResponse = new LogoutResponseType();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogoutResult"/> class.
        /// </summary>
        /// <param name="resp">The resp.</param>
        public LogoutResult(LogoutResponseType resp)
        {
            lResponse = resp;
            this.StatusTxt = resp.StatusTxt;
        }
    }
}