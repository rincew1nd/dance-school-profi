using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Linq;
using Moq;

namespace DanceSchool.Tests.Extensions
{
    public static class DbExtensions
    {
        public static ObjectResult<T> GetDbEntitiesMock<T>(List<T> entities)
        {
            var test = entities.AsDbSetMock().Object;
            var objRes = new Mock<ObjectResult<T>>();
            objRes.Setup(o => o.GetEnumerator()).Returns(entities.GetEnumerator());
            //objRes.Setup(o => o.AsQueryable()).Returns();
            return objRes.Object;
        }
        
        public static Mock<IQueryable<T>> AsDbSetMock<T>(this IEnumerable<T> list) {
            IQueryable<T> queryableList = list.AsQueryable();
            Mock<IQueryable<T>> dbSetMock = new Mock<IQueryable<T>>();
            dbSetMock.As<IQueryable<T>>().Setup(x => x.Provider).Returns(queryableList.Provider);
            dbSetMock.As<IQueryable<T>>().Setup(x => x.Expression).Returns(queryableList.Expression);
            dbSetMock.As<IQueryable<T>>().Setup(x => x.ElementType).Returns(queryableList.ElementType);
            dbSetMock.As<IQueryable<T>>().Setup(x => x.GetEnumerator()).Returns(() => queryableList.GetEnumerator());
            return dbSetMock;
        }
    }
}