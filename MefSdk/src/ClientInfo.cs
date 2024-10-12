namespace MeF.Client
{
    using System;
    using WCFClient;
    

   // using MeF.Client.Proxy.Wse3;

    /// <summary>
    ///   Stores client information that is used for making calls to MeF.
    /// </summary>
    public class ClientInfo
    {
        #region Constructors and Destructors

        /// <summary>
        ///   Initializes a new instance of the <see cref = "ClientInfo" /> class.
        /// </summary>
        /// <param name = "etin">
        ///   The etin
        /// </param>
        /// <param name = "appSysId">
        ///   The app sys ID
        /// </param>
        /// <remarks>
        ///   Constructs the ClientInfo object with only the etin and appSysId parameters.
        /// </remarks>
        public ClientInfo(string etin, string appSysId)
            : this(etin, appSysId, TestCdType.T)
        {
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref = "ClientInfo" /> class.
        /// </summary>
        /// <param name = "etin">
        ///   The etin
        /// </param>
        /// <param name = "appSysId">
        ///   The app sys Id
        /// </param>
        /// <param name = "testIndicator">
        ///   The TestIndicator enum value
        /// </param>
        public ClientInfo(
            string etin, string appSysId, TestCdType testIndicator)
        {
            this.Etin = etin;
            this.AppSysID = appSysId;
            this.TestIndicator = testIndicator;
        }


        #endregion

        #region Public Properties

        /// <summary>
        ///   Gets the client AppSysID.
        /// </summary>
        public string AppSysID { get; private set; }

        /// <summary>
        ///   Gets the client Etin.
        /// </summary>
        public string Etin { get; private set; }

        /// <summary>
        ///   Gets the session indicator.
        /// </summary>
        internal SessionKeyCdType SessionIndicator { get; set; }

        /// <summary>
        ///   Gets a value indicating whether [session indicator specified].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [session indicator specified]; otherwise, <c>false</c>.
        /// </value>
        internal bool SessionIndicatorSpecified
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        ///   Gets the test indicator.
        /// </summary>
        public TestCdType TestIndicator { get; private set; }

        /// <summary>
        ///   The TestIndicator
        /// </summary>
        /// <value>
        ///   <c>true</c> if [test indicator specified]; otherwise, <c>false</c>.
        /// </value>
        internal bool TestIndicatorSpecified
        {
            get
            {
                return true;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///   Sets the app sys Id.
        /// </summary>
        /// <param name = "appSysId">The app sys Id.</param>
        public void SetAppSysID(string appSysId)
        {
            this.AppSysID = appSysId;
        }

        /// <summary>
        ///   Sets the etin.
        /// </summary>
        /// <param name = "etin">The ETIN.</param>
        public void SetEtin(string etin)
        {
            this.Etin = etin;
        }

        /// <summary>
        ///   Sets the test indicator.
        /// </summary>
        /// <param name="testIndicator">The test indicator</param>
        public void SetTestIndicator(TestCdType testIndicator)
        {
            this.TestIndicator = testIndicator;
        }

        #endregion
    }
}