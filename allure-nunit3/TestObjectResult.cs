using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using allure_nunit3.Xml.Models;

namespace allure_nunit3
{
    public class TestObjectResult
    {
        public AbstractTestCase TestCaseObject { get; set; }

        public State State { get; set; }
    }
}
