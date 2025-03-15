using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Routing;
using DanceSchool.Controllers;
using DanceSchool.Models;
using DanceSchool.Tests.Extensions;

namespace DanceSchool.Tests.Controllers
{
    [TestClass]
    public class AccountControllerTests : BaseControllerTest
    {
        private AccountController AccountControllerMock { get; set; }
        
        [TestInitialize]
        public override void Setup()
        {
            base.Setup();
            
            AccountControllerMock = new AccountController(DbMock.Object)
            {
                ControllerContext = ControllerContextMock.Object,
                Url = new UrlHelper(RequestContextMock, new RouteCollection())
            };
        }

        [TestMethod]
        public void Login_GET_AuthenticatedUser_RedirectsToReturnUrl()
        {
            // Arrange
            var identity = new GenericIdentity("test");
            HttpContextMock.Setup(x => x.User.Identity).Returns(identity);
            HttpContextMock.Setup(x => x.Request.IsAuthenticated).Returns(true);

            // Act
            var result = AccountControllerMock.Login("/test") as RedirectResult;

            // Assert
            Assert.AreEqual("/test", result?.Url);
        }

        [TestMethod]
        public async Task Login_POST_ValidCredentials_SignsInAndRedirects()
        {
            // Arrange
            var model = new LoginViewModel { Email = "test@test.com", Password = "pass" };
            var user = new User_Login_Result { Id = 1, FirstName = "John", LastName = "Doe", RoleId = 2 };
            
            DbMock.Setup(db => db.User_Login(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(DbExtensions.GetDbEntitiesMock(new List<User_Login_Result>() { user }));

            // Act
            var result = await AccountControllerMock.Login(model, "/test") as RedirectResult;

            // Assert
            Assert.AreEqual("/test", result?.Url);
        }

        [TestMethod]
        public async Task Login_POST_InvalidCredentials_ReturnsViewWithError()
        {
            // Arrange
            var model = new LoginViewModel { Email = "wrong@test.com", Password = "wrong" };
            DbMock.Setup(db => db.User_Login(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(DbExtensions.GetDbEntitiesMock(new List<User_Login_Result>()));

            // Act
            var result = await AccountControllerMock.Login(model, null) as ViewResult;

            // Assert
            Assert.IsFalse(AccountControllerMock.ModelState.IsValid);
            Assert.IsTrue(AccountControllerMock.ModelState[string.Empty].Errors.Count > 0);
        }

        [TestMethod]
        public async Task Register_POST_ValidModel_CreatesUserAndRedirects()
        {
            // Arrange
            var model = new RegisterViewModel 
            { 
                Email = "new@test.com", 
                Password = "Pass123!", 
                ConfirmPassword = "Pass123!",
                FirstName = "John",
                LastName = "Doe"
            };

            DbMock.Setup(db => db.Insert_User(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .Returns(DbExtensions.GetDbEntitiesMock(new List<int?>() { 1 }));

            // Act
            var result = await AccountControllerMock.Register(model) as RedirectToRouteResult;

            // Assert
            Assert.AreEqual("Login", result?.RouteValues["action"]);
            DbMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [TestMethod]
        public async Task Register_POST_DuplicateEmail_ReturnsViewWithError()
        {
            // Arrange
            var model = new RegisterViewModel { Email = "exists@test.com" };
            DbMock.Setup(db => db.Insert_User(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .Returns(DbExtensions.GetDbEntitiesMock(new List<int?>() { -1 }));
            
            // Act
            var result = await AccountControllerMock.Register(model) as ViewResult;

            // Assert
            Assert.IsFalse(AccountControllerMock.ModelState.IsValid);
            Assert.IsTrue(result?.ViewName == "");
        }

        [TestMethod]
        public void LogOff_POST_SignsOutAndRedirects()
        {
            // Act
            var result = AccountControllerMock.LogOff() as RedirectToRouteResult;

            // Assert
            Assert.AreEqual("Login", result?.RouteValues["action"]);
        }

        [TestMethod]
        public async Task Login_POST_DatabaseError_ReturnsViewWithError()
        {
            // Arrange
            var model = new LoginViewModel { Email = "test@test.com", Password = "pass" };
            DbMock.Setup(db => db.User_Login(It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new Exception("Database error"));

            // Act
            var result = await AccountControllerMock.Login(model, null) as ViewResult;

            // Assert
            Assert.IsTrue(AccountControllerMock.ModelState[string.Empty].Errors.Any(e => e.ErrorMessage.Contains("Database error")));
        }
    }
}