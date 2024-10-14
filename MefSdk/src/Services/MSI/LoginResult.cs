using MeFWCFClient.MeFMSIServices;
using System;

namespace MeF.Client.Services.MSIServices
{
    /// <summary>
    ///Response from LoginClient that contains the status.
    /// </summary>
    public class LoginResult
    {
        private LoginResponseType _loginResponseType;

        /// <summary>
        /// Gets or sets the Status.
        /// </summary>
        /// <value>The Status.</value>
        public string StatusTxt { get; set; }

        public String MessageID { get; set; }
        public String RelatesTo { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LoginResult"/> class.
        /// </summary>
        /// <param name="loginResponse">The login response.</param>
        public LoginResult(LoginResponseType loginResponse)
        {
            _loginResponseType = loginResponse;
            this.StatusTxt = loginResponse.StatusTxt;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LoginResult"/> class.
        /// </summary>
        public LoginResult()
        {
            if (_loginResponseType == null)
                _loginResponseType = new LoginResponseType();
        }
    }
}