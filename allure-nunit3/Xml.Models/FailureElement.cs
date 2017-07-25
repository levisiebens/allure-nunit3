using System;
using System.Xml.Serialization;

namespace allure_nunit3.Xml.Models
{
    [XmlType("failure")]
    public class FailureElement
    {
        [XmlElement("message")]
        public String Message { get; set; }

        [XmlElement("stack-trace")]
        public String StackTrace { get; set; }
    }
}
