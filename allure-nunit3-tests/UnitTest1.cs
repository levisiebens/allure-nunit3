using NUnit.Framework;

namespace allure_nunit3_tests
{
    public class UnitTest1
    {
        [Test]
        public void PassingTest()
        {
        }

        [Test]
        public void FailingTest()
        {
            Assert.AreEqual(1, 2, "Are not equal!");
        }
    }
}
