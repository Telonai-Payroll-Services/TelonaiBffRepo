namespace MeF.Client.Util
{
    /// <summary>
    /// Constants class.
    /// </summary>
    public class Constants
    {
        // Namespaces
        public const string WSAddressing2004Namespace =
            "http://schemas.xmlsoap.org/ws/2004/08/addressing";
        public const string WSAddressing10Namespace =
            "http://www.w3.org/2005/08/addressing";
        public const string AnonymousNamespace2004 =
            "http://schemas.xmlsoap.org/ws/dd2004/08/addressing/role/anonymous";
        public const string W3AnonymousNamespace2005 =
            "http://www.w3.org/2005/08/addressing/anonymous";
        public const string WSSecurityUtilityNamespace =
            "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd";
       
        public const string XmlEncSha256Namespace =
            "http://www.w3.org/2001/04/xmlenc#sha256";
        public const string SoapEnvelopeNamespace =
            "http://www.w3.org/2003/05/soap-envelope";

        // WS-Security constants
        public const string WSSecurityIdAttributeName = "Id";

        // WS-Addressing constants
        public const string WSAddressingToElementName = "To";
        public const string WSAddressingReplyToElementName = "ReplyTo";
        public const string WSAddressingFromElementName = "From";
    }
}