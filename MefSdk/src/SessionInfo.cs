using System;
using System.Runtime.Serialization;
//using MeF.Client.Security.Tokens;
using MeF.Client.Util;
using System.Net;
using System.Collections.Generic;
//using Microsoft.Web.Services3.Security.Tokens;

namespace MeF.Client
{
    using MeF.Client.Configuration;
    using MeF.Client.Logging;

    /// <summary>Encapsulates information about an authenticated user session within a MeF application.</summary>
    [Serializable]
    public sealed class SessionInfo
    {

        /// <summary>
        /// Gets or sets the J session id.
        /// </summary>
        /// <value>The J session id.</value>
        //public String JSessionId { get; private set; }

        Dictionary<string, string> CookieColl;
        public string getCookieColl()
        {
            Boolean firstTime = true;
            string cookie = "";
            if (CookieColl != null)
            {
                foreach (var pair in CookieColl)
                {
                    if (!firstTime)
                    {
                        cookie = cookie + ";";
                    }

                    cookie = cookie + pair.Key + "=" + pair.Value;
                    firstTime = false;
                }
            }
            return cookie;
        }

        /// <summary>
        /// Gets or sets the saml token.
        /// </summary>
        /// <value>The saml token.</value>
        public string SamlToken { get; private set; }

        internal SessionInfo()
        {
     
        }

        /// <summary>
        /// Sets the J session ID.
        /// </summary>
        /// <param name="jSessionId">The j session ID.</param>
        /// <remarks></remarks>
        //internal void SetJSessionId(String jSessionId)
        //{
        //    this.JSessionId = jSessionId;
        //}

        /// <summary>
        /// Sets the cookie collection
        /// </summary>
        /// <param name="newColl">The Cookie Collection</param>
        /// <remarks></remarks>
        internal void SetCookieColl(string newColl)
        {
            string val;
            if (this.CookieColl == null)
            {
                CookieColl = new Dictionary<string,string>();
            }

            String[] cookieArr = newColl.Split(',');

            foreach (string value in cookieArr)
            {
                String[] cookieStrings = value.Split(';');
                String nameValuePair = cookieStrings[0];
                String[] keyValue = nameValuePair.Split('=');
                

                if (CookieColl.TryGetValue(keyValue[0], out val))
                {
                    CookieColl[keyValue[0]] = keyValue[1];
                }
                else
                {                    
                    CookieColl.Add(keyValue[0], keyValue[1]);
                }
            }        
        }

        /// <summary>
        /// Sets the saml token assertion.
        /// </summary>
        /// <param name="saml">The _saml token.</param>
        /// <remarks></remarks>
        internal void SetSamlTokenAssertion(string saml)
        {
            this.SamlToken = saml;
        }


        /// <summary>
        /// Gets the saml token.
        /// </summary>
        /// <returns></returns>
        internal string GetSamlToken()
        {
            return SamlToken;
        }


    }
}