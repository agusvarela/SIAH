using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SIAH.Controllers;

namespace UnitTestProject
{
    [TestClass]
    public class UnitTest1
    {
        //[TestMethod]
        //public void TestMethod1()
        //{
        //    var controller = new PedidosController();

        //    var result = controller.Details(28) as ViewResult;
        //    Assert.AreEqual("Details", result.ViewName);
        //}

        [TestMethod]
        public void TestMethod2()
        {
            var controller = new PedidosController();

            var result = controller.Create() as ViewResult;
            Assert.AreEqual("Create", result.ViewName);
        }
    }
}
