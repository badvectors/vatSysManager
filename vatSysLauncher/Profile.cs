namespace vatSysManager
{
    // NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class Profile
    {

        private ProfileVersion versionField;

        private ProfileServers serversField;

        private string nameField;

        private string fullNameField;

        /// <remarks/>
        public ProfileVersion Version
        {
            get
            {
                return this.versionField;
            }
            set
            {
                this.versionField = value;
            }
        }

        /// <remarks/>
        public ProfileServers Servers
        {
            get
            {
                return this.serversField;
            }
            set
            {
                this.serversField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string FullName
        {
            get
            {
                return this.fullNameField;
            }
            set
            {
                this.fullNameField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class ProfileVersion
    {

        private string aIRACField;

        private string revisionField;

        private uint publishDateField;

        private string updateURLField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string AIRAC
        {
            get
            {
                return this.aIRACField;
            }
            set
            {
                this.aIRACField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Revision
        {
            get
            {
                return this.revisionField;
            }
            set
            {
                this.revisionField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public uint PublishDate
        {
            get
            {
                return this.publishDateField;
            }
            set
            {
                this.publishDateField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string UpdateURL
        {
            get
            {
                return this.updateURLField;
            }
            set
            {
                this.updateURLField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class ProfileServers
    {

        private ProfileServersVATSIMStatus vATSIMStatusField;

        private ProfileServersGRIB gRIBField;

        private ProfileServersG2G g2GField;

        private ProfileServersSweatBox sweatBoxField;

        private ProfileServersSweatBox2 sweatBox2Field;

        /// <remarks/>
        public ProfileServersVATSIMStatus VATSIMStatus
        {
            get
            {
                return this.vATSIMStatusField;
            }
            set
            {
                this.vATSIMStatusField = value;
            }
        }

        /// <remarks/>
        public ProfileServersGRIB GRIB
        {
            get
            {
                return this.gRIBField;
            }
            set
            {
                this.gRIBField = value;
            }
        }

        /// <remarks/>
        public ProfileServersG2G G2G
        {
            get
            {
                return this.g2GField;
            }
            set
            {
                this.g2GField = value;
            }
        }

        /// <remarks/>
        public ProfileServersSweatBox SweatBox
        {
            get
            {
                return this.sweatBoxField;
            }
            set
            {
                this.sweatBoxField = value;
            }
        }

        /// <remarks/>
        public ProfileServersSweatBox2 SweatBox2
        {
            get
            {
                return this.sweatBox2Field;
            }
            set
            {
                this.sweatBox2Field = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class ProfileServersVATSIMStatus
    {

        private string urlField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string url
        {
            get
            {
                return this.urlField;
            }
            set
            {
                this.urlField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class ProfileServersGRIB
    {

        private string urlField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string url
        {
            get
            {
                return this.urlField;
            }
            set
            {
                this.urlField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class ProfileServersG2G
    {

        private string urlField;

        private ushort portField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string url
        {
            get
            {
                return this.urlField;
            }
            set
            {
                this.urlField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public ushort port
        {
            get
            {
                return this.portField;
            }
            set
            {
                this.portField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class ProfileServersSweatBox
    {

        private string urlField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string url
        {
            get
            {
                return this.urlField;
            }
            set
            {
                this.urlField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class ProfileServersSweatBox2
    {

        private string urlField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string url
        {
            get
            {
                return this.urlField;
            }
            set
            {
                this.urlField = value;
            }
        }
    }


}
