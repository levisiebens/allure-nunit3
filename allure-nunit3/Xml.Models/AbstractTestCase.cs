using System;
using System.Xml.Serialization;

namespace allure_nunit3.Xml.Models
{
    public abstract class AbstractTestCase
    {
        [XmlAttribute("id")]
        public String Id { get; set; }

        [XmlAttribute("parentId")]
        public String ParentId { get; set; }

        [XmlAttribute("name")]
        public String Name { get; set; }

        [XmlAttribute("fullname")]
        public String FullName { get; set; }
    }
}
