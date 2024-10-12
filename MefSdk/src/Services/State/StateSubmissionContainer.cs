using System.Collections.Generic;
using MeFWCFClient.MeFStateServices;

namespace MeF.Client.Services.StateServices
{
    /// <summary>
    /// Contains information relating SubmissionArchives and their respective IRS Data as well as a list of errors.
    /// </summary>
    public class StateSubmissionContainer
    {
        /// <summary>
        /// Gets or sets the error list.
        /// </summary>
        /// <value>The error list.</value>
        public List<SubmissionErrorType> ErrorList { get; private set; }

        public List<StateSubmissionData> StateSubmissionDataList { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="StateSubmissionContainer"/> class.
        /// </summary>
        public StateSubmissionContainer()
        {
            this.StateSubmissionDataList = new List<StateSubmissionData>();
            this.ErrorList = new List<SubmissionErrorType>();
        }

        public void AddStateSubmissionData(StateSubmissionData stateSubmissionData)
        {
            this.StateSubmissionDataList.Add(stateSubmissionData);
        }
    }
}