using System.Xml.Serialization;

namespace allure_nunit3.Xml.Models
{
    [XmlType("assertions")]
    public class AssertionsElement
    {
        [XmlElement("assertion")]
        public AssertionElement Assertion { get; set; }
    }
}
