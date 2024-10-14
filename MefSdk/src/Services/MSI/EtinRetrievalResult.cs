using System.Collections.Generic;
using System.Collections.ObjectModel;
using MeFWCFClient.MeFMSIServices;
using System;

namespace MeF.Client.Services.MSIServices
{
    /// <summary>
    ///Response from EtinRetrievalClient that contains the count and list of ETINStatus.
    /// </summary>
    public class EtinRetrievalResult
    {
        /// <summary>
        /// Gets or sets the Count.
        /// </summary>
        /// <value>The Count.</value>
        public int Cnt { get; set; }
        public String MessageID { get; set; }
        public String RelatesTo { get; set; }

        /// <summary>
        /// Gets or sets the ETINStatus.
        /// </summary>
        /// <value>The ETINStatus.</value>
        public IList<ETINStatusGrpType> ETINStatusGrp { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EtinRetrievalResult"/> class.
        /// </summary>
        public EtinRetrievalResult()
        {
            ETINStatusGrp = new Collection<ETINStatusGrpType>();
        }
    }
}