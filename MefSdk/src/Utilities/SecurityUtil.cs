namespace MeF.Client.Util
{
    using System;
    using System.Linq;
    using System.Security.Cryptography.X509Certificates;
    using MeF.Client.Exceptions;
  //  using MeF.Client.Security.Tokens;
 //   using Microsoft.Web.Services3;
 //   using Microsoft.Web.Services3.Security.Tokens;
    using Config = MeF.Client.Configuration.ClientConfiguration;

    internal static class SecurityUtil
    {
        /// <exception cref="MeF.Client.Exceptions.ToolkitException" />
        internal static X509Certificate2 LookupCertificate(StoreName storeName, StoreLocation storeLocation,
                                                           X509FindType findType, String findValue)
        {
            X509Store store = null;
            try
            {
                store = new X509Store(storeName, storeLocation);
                store.Open(OpenFlags.ReadOnly);
                X509Certificate2Collection certs = store.Certificates.Find(findType, findValue, false);
                if (certs.Count != 1)
                {
                    throw new ToolkitException(String.Format("Certificate {0} not found or more than one certificate found",
                                                      findValue));
                }
                return certs[0];
            }
            finally
            {
                if (store != null) store.Close();
            }
        }

        internal static X509SecurityToken GetX509Token()
        {
            var cert = LookupCertificate(Config.StoreName, Config.StoreLocation, Config.X509FindType, Config.FindValue);
            var token = new X509SecurityToken(cert);
            return token;
        }

        internal static SecurityToken GetSamlToken(SoapContext context)
        {
            SecurityToken samlToken = null;
#pragma warning disable 612,618
            foreach (SamlToken tok in context.Security.Tokens.OfType<SamlToken>())
            {
                samlToken = tok as SamlToken;
            }
            return samlToken;
        }
    }
}