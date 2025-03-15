using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using DanceSchool.Models;
using Moq;

namespace DanceSchool.Tests.Controllers
{
    public abstract class BaseControllerTest
    {
        protected Mock<DanceSchoolEntities> DbMock  { get; set; }
        
        protected Mock<HttpServerUtilityBase> HttpServerUtilityBaseMock { get; set; }
        protected Mock<HttpContextBase> HttpContextMock  { get; set; }
        protected RequestContext RequestContextMock { get; set; }
        protected Mock<HttpRequestBase> HttpRequestBaseMock { get; set; }
        protected Mock<ControllerContext> ControllerContextMock { get; set; }

        public virtual void Setup()
        {
            DbMock = new Mock<DanceSchoolEntities>();

            HttpServerUtilityBaseMock = new Mock<HttpServerUtilityBase>();
            HttpContextMock = new Mock<HttpContextBase>();
            RequestContextMock = new RequestContext() { HttpContext = HttpContextMock.Object };
            HttpRequestBaseMock = new Mock<HttpRequestBase>();
            ControllerContextMock = new Mock<ControllerContext>();
            
            HttpContextMock.Setup(c => c.Items).Returns(new Dictionary<string, object>()
            {
                {"owin.Environment", new Dictionary<string, object>() {{ "",""}}}
            });
            HttpContextMock.Setup(x => x.Request).Returns(HttpRequestBaseMock.Object);
            HttpContextMock.Setup(c => c.Server).Returns(HttpServerUtilityBaseMock.Object);
            
            HttpRequestBaseMock.Setup(rb => rb.RequestContext).Returns(RequestContextMock);
            
            ControllerContextMock.SetupGet(x => x.HttpContext).Returns(HttpContextMock.Object);
        }
    }
}