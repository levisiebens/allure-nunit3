using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace allure_nunit3.Xml.Models
{
    [XmlType("test-case")]
    public class TestCaseEnd : AbstractTestCase
    {
        [XmlElement("assertions")]
        public AssertionsElement Assertions { get; set; }

        [XmlAttribute("classname")]
        public String ClassName { get; set; }

        [XmlElement("failure")]
        public FailureElement Failure { get; set; }

        [XmlAttribute("label")]
        public String Label { get; set; }

        [XmlAttribute("result")]
        public String Result { get; set; }

        [XmlAttribute("runstate")]
        public String RunState { get; set; }

        [XmlAttribute("seed")]
        public String Seed { get; set; }
    }
}
