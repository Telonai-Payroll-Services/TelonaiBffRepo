using System.Collections.Generic;
using System.Collections.ObjectModel;
using MeFWCFClient.MeFMSIServices;
using System;

namespace MeF.Client.Services.MSIServices
{
    /// <summary>
    ///Response from GetStateParticipantsListClient that contains the count and list of StateParticipants.
    /// </summary>
    public class GetStateParticipantsListResult
    {
        /// <summary>
        /// Gets or sets the Count.
        /// </summary>
        /// <value>The Count.</value>
        public int Cnt { get; set; }

        public String MessageID { get; set; }
        public String RelatesTo { get; set; }

        /// <summary>
        /// Gets or sets the StateParticipant.
        /// </summary>
        /// <value>The StateParticipant.</value>
        public IList<StateParticipantGrpType> StateParticipantGrp { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GetStateParticipantsListResult"/> class.
        /// </summary>
        public GetStateParticipantsListResult()
        {
            StateParticipantGrp = new Collection<StateParticipantGrpType>();
        }
    }
}