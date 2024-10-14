namespace WCFClient
{
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("svcutil", "3.0.4506.2152")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.irs.gov/a2a/mef/MeFHeader.xsd")]
    public partial class MeFHeaderType : object, System.ComponentModel.INotifyPropertyChanged
    {

        private string messageIDField;

        private string relatesToField;

        private string actionField;

        private System.DateTime messageTsField;

        private string eTINField;

        private SessionKeyCdType sessionKeyCdField;

        private bool sessionKeyCdFieldSpecified;

        private TestCdType testCdField;

        private bool testCdFieldSpecified;

        private MeFNotificationResponse notificationResponseField;

        private string appSysIDField;

        private string wSDLVersionNumField;

        private string clientSoftwareTxtField;

        private string idField;

        private System.Xml.XmlAttribute[] anyAttrField;

        public MeFHeaderType()
        {
            this.wSDLVersionNumField = "10.8";
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order = 0)]
        public string MessageID
        {
            get
            {
                return this.messageIDField;
            }
            set
            {
                this.messageIDField = value;
                this.RaisePropertyChanged("MessageID");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order = 1)]
        public string RelatesTo
        {
            get
            {
                return this.relatesToField;
            }
            set
            {
                this.relatesToField = value;
                this.RaisePropertyChanged("RelatesTo");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order = 2)]
        public string Action
        {
            get
            {
                return this.actionField;
            }
            set
            {
                this.actionField = value;
                this.RaisePropertyChanged("Action");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order = 3)]
        public System.DateTime MessageTs
        {
            get
            {
                return this.messageTsField;
            }
            set
            {
                this.messageTsField = value;
                this.RaisePropertyChanged("MessageTs");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order = 4)]
        public string ETIN
        {
            get
            {
                return this.eTINField;
            }
            set
            {
                this.eTINField = value;
                this.RaisePropertyChanged("ETIN");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order = 5)]
        public SessionKeyCdType SessionKeyCd
        {
            get
            {
                return this.sessionKeyCdField;
            }
            set
            {
                this.sessionKeyCdField = value;
                this.RaisePropertyChanged("SessionKeyCd");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool SessionKeyCdSpecified
        {
            get
            {
                return this.sessionKeyCdFieldSpecified;
            }
            set
            {
                this.sessionKeyCdFieldSpecified = value;
                this.RaisePropertyChanged("SessionKeyCdSpecified");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order = 6)]
        public TestCdType TestCd
        {
            get
            {
                return this.testCdField;
            }
            set
            {
                this.testCdField = value;
                this.RaisePropertyChanged("TestCd");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool TestCdSpecified
        {
            get
            {
                return this.testCdFieldSpecified;
            }
            set
            {
                this.testCdFieldSpecified = value;
                this.RaisePropertyChanged("TestCdSpecified");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order = 7)]
        public MeFNotificationResponse NotificationResponse
        {
            get
            {
                return this.notificationResponseField;
            }
            set
            {
                this.notificationResponseField = value;
                this.RaisePropertyChanged("NotificationResponse");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order = 8)]
        public string AppSysID
        {
            get
            {
                return this.appSysIDField;
            }
            set
            {
                this.appSysIDField = value;
                this.RaisePropertyChanged("AppSysID");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order = 9)]
        public string WSDLVersionNum
        {
            get
            {
                return this.wSDLVersionNumField;
            }
            set
            {
                this.wSDLVersionNumField = value;
                this.RaisePropertyChanged("WSDLVersionNum");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order = 10)]
        public string ClientSoftwareTxt
        {
            get
            {
                return this.clientSoftwareTxtField;
            }
            set
            {
                this.clientSoftwareTxtField = value;
                this.RaisePropertyChanged("ClientSoftwareTxt");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "ID")]
        public string Id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
                this.RaisePropertyChanged("Id");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAnyAttributeAttribute()]
        public System.Xml.XmlAttribute[] AnyAttr
        {
            get
            {
                return this.anyAttrField;
            }
            set
            {
                this.anyAttrField = value;
                this.RaisePropertyChanged("AnyAttr");
            }
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
        {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null))
            {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("svcutil", "3.0.4506.2152")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.irs.gov/a2a/mef/MeFHeader.xsd")]
    public enum SessionKeyCdType
    {

        /// <remarks/>
        Y,

        /// <remarks/>
        N,
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("svcutil", "3.0.4506.2152")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.irs.gov/a2a/mef/MeFHeader.xsd")]
    public enum TestCdType
    {

        /// <remarks/>
        T,

        /// <remarks/>
        P,
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("svcutil", "3.0.4506.2152")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.irs.gov/a2a/mef/MeFHeader.xsd")]
    public partial class MeFNotificationResponse : object, System.ComponentModel.INotifyPropertyChanged
    {

        private string notificationDescField;

        private System.DateTime notificationDtField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order = 0)]
        public string NotificationDesc
        {
            get
            {
                return this.notificationDescField;
            }
            set
            {
                this.notificationDescField = value;
                this.RaisePropertyChanged("NotificationDesc");
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order = 1)]
        public System.DateTime NotificationDt
        {
            get
            {
                return this.notificationDtField;
            }
            set
            {
                this.notificationDtField = value;
                this.RaisePropertyChanged("NotificationDt");
            }
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
        {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null))
            {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
}