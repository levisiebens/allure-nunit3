using System;
using System.Xml.Serialization;

namespace allure_nunit3.Xml.Models
{
    [XmlType("assertion")]
    public class AssertionElement
    {
        [XmlElement("message")]
        public String Message { get; set; }

        [XmlAttribute("result")]
        public String Result { get; set; }

        [XmlElement("stack-trace")]
        public String StackTrace { get; set; }
    }
}
