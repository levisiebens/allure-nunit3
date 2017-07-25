using System;
using System.Xml.Serialization;

namespace allure_nunit3.Xml.Models
{
    [XmlType("start-run")]
    public class TestRunStart
    {
        [XmlAttribute("count")]
        public String Count { get; set; }
    }
}
