using Microsoft.VisualStudio.TestTools.UnitTesting;
using eMotive.Repository.Objects.Users;

namespace eMotive.Core.Tests
{
    //http://www.codeproject.com/Articles/5019/Advanced-Unit-Testing-Part-I-Overview
    //http://www.peterprovost.org/blog/2012/04/15/visual-studio-11-fakes-part-1
    [TestClass]
    public class UserTest
    {
        private User user;

        [TestInitialize]
        public void Initialize()
        {
            user = new User();
        }

        [TestMethod]
        public void ConstructorInitialization()
        {
            Assert.AreEqual(user.Id, 0);
            Assert.AreEqual(user.Username, null);
            Assert.AreEqual(user.Forename, null);
            Assert.AreEqual(user.Surname, null);
            Assert.AreEqual(user.Email, null);
            Assert.AreEqual(user.Archived, false);
            Assert.AreEqual(user.Enabled, false);

        }

        [TestMethod]
        public void Username()
        {
            user.Username = "ted";
            Assert.AreEqual(user.Username, "ted");
        }

        [TestCleanup]
        public void Terminate()
        {
        }
    }
}
