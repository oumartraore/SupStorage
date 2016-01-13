using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace WCFServiceWebRole1
{
    [DataContract]
    public class BlobInformation
    {
        private string _name;
        private string _uri;

        public BlobInformation(string name, string uri)
        {
            _name = name;
            _uri = uri;
        }

        [DataMember]
        public string Name
        {
            get { return _name;  }
            set { _name = value; } 
        }

        [DataMember]
        public string Uri
        {
            get { return _uri; }
            set { _uri = value;  }
        }
    }
}