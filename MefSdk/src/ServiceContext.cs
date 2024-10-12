using System;
using System.Net;

namespace MeF.Client
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class ServiceContext
    {
        private ClientInfo clientInfo;
        private SessionInfo sessionInfo;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceContext"/> class.
        /// </summary>
        /// <remarks></remarks>
        public ServiceContext()
        {
            this.clientInfo = new ClientInfo("", "");
            this.sessionInfo = new SessionInfo();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceContext"/> class.
        /// </summary>
        /// <param name="clientInfo">The client info.</param>
        /// <remarks></remarks>
        public ServiceContext(ClientInfo clientInfo)
        {
            this.clientInfo = clientInfo;
            this.sessionInfo = new SessionInfo();
        }

        /// <summary>
        /// Sets the client info.
        /// </summary>
        /// <param name="client">The client info.</param>
        /// <remarks></remarks>
        public void SetClientInfo(ClientInfo client)
        {
            this.clientInfo = client;
        }

        /// <summary>
        /// Sets the session info.
        /// </summary>
        /// <param name="session">The session info.</param>
        /// <remarks></remarks>
        public void SetSessionInfo(SessionInfo session)
        {
            this.sessionInfo = session;
        }

        /// <summary>
        /// Gets the client info.
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public ClientInfo GetClientInfo()
        {
            return this.clientInfo;
        }

        /// <summary>
        /// Gets the session info.
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public SessionInfo GetSessionInfo()
        {
            return this.sessionInfo;
        }

        /// <summary>
        /// Sets the J session ID.
        /// </summary>
        /// <param name="cookie">The cookie.</param>
        /// <remarks></remarks>
        //public void SetJSessionID(String cookie)
        //{
        //    this.sessionInfo.SetJSessionId(cookie);
        //}

        /// <summary>
        /// Sets the cookie collection
        /// </summary>
        /// <param name="cookieColl">The cookie.</param>
        /// <remarks></remarks>
        public void SetCookies(string cookieColl)
        {
            this.sessionInfo.SetCookieColl(cookieColl);
        }

        

        /// <summary>
        /// Sets the saml token.
        /// </summary>
        /// <param name="samlToken">The saml token.</param>
        /// <remarks></remarks>
        public void SetSamlToken(string samlToken)
        {
            this.sessionInfo.SetSamlTokenAssertion(samlToken);
        }

        /// <summary>
        /// Gets the SAML token.
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public string GetSAMLToken()
        {
            return this.sessionInfo.GetSamlToken();
        }

        internal bool IsValidSession()
        {
            return this.sessionInfo.SamlToken != null && this.sessionInfo.getCookieColl() != null;
        }

    }
}