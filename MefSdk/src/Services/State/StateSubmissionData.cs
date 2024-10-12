using MeF.Client.Services.InputComposition;
using MeFWCFClient.MeFStateServices;

namespace MeF.Client.Services.StateServices
{
    /// <summary>
    /// This class holds Submission Archives and their respective IRS Data.
    /// </summary>
    public class StateSubmissionData
    {
        //private StateSubmissionArchive stateSubmissionArchive;

        /// <summary>
        /// Gets or sets the state submission archive.
        /// </summary>
        /// <value>The state submission archive.</value>
        public StateSubmissionArchive StateSubmissionArchive { get; set; }

        //private IRSDataForStateSubmissionType irsData;

        /// <summary>
        /// Gets or sets the irs data.
        /// </summary>
        /// <value>The irs data.</value>
        public IRSDataForStateSubmissionType IrsData { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="StateSubmissionData"/> class.
        /// </summary>
        public StateSubmissionData()
        {
            this.StateSubmissionArchive = new StateSubmissionArchive();
            this.IrsData = new IRSDataForStateSubmissionType();
        }

        public StateSubmissionData(IRSDataForStateSubmissionType irsData, StateSubmissionArchive stateSubmissionArchive)
        {
            this.StateSubmissionArchive = stateSubmissionArchive;
            this.IrsData = irsData;
        }
    }
}