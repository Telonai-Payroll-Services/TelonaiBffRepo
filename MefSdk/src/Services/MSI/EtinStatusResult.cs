using System.Collections.Generic;
using System.Collections.ObjectModel;
using MeFWCFClient.MeFMSIServices;
using System;

namespace MeF.Client.Services.MSIServices
{
    /// <summary>
    ///Response from EtinStatusClient that contains the count, status and list of ETINFormStatus.
    /// </summary>
    public class EtinStatusResult
    {
        /// <summary>
        /// Gets or sets the Etin.
        /// </summary>
        /// <value>The Etin.</value>
        public string ETIN { get; set; }

        /// <summary>
        /// Gets or sets the Status.
        /// </summary>
        /// <value>The Status.</value>
        public string StatusTxt { get; set; }
        public String MessageID { get; set; }
        public String RelatesTo { get; set; }

        /// <summary>
        /// Gets or sets the ETINFormStatus.
        /// </summary>
        /// <value>The ETINFormStatus.</value>
        public IList<FormStatusGrpType> FormStatusGrp { get; set; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="EtinStatusResult"/> class.
        /// </summary>
        public EtinStatusResult()
        {
            FormStatusGrp = new Collection<FormStatusGrpType>();
        }
    }
}