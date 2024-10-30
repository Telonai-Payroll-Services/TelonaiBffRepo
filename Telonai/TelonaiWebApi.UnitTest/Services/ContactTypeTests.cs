using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using TelonaiWebApi.Entities;
using TelonaiWebApi.Helpers;
using TelonaiWebApi.Services;

namespace TelonaiWebApi.UnitTest.Services
{
    public class ContactTypeTests
    {
        private readonly Mock<DataContext> _mockContext;
        private readonly ContactTypeService _service;
        public ContactTypeTests()
        {
            _mockContext = new Mock<DataContext>(MockBehavior.Default, new object[] { new Mock<IHttpContextAccessor>().Object });
            _service = new ContactTypeService(_mockContext.Object);

        }
        [Fact]
        public void GetAll_ReturnsAllContactTypes()
        {

            var contactTypes = new List<ContactType>
        {
            new ContactType { Id = 1, Value = "Type1" },
            new ContactType { Id = 2, Value = "Type2" }
        }.AsQueryable();

            var mockSet = new Mock<DbSet<ContactType>>();
            mockSet.As<IQueryable<ContactType>>().Setup(m => m.Provider).Returns(contactTypes.Provider);
            mockSet.As<IQueryable<ContactType>>().Setup(m => m.Expression).Returns(contactTypes.Expression);
            mockSet.As<IQueryable<ContactType>>().Setup(m => m.ElementType).Returns(contactTypes.ElementType);
            mockSet.As<IQueryable<ContactType>>().Setup(m => m.GetEnumerator()).Returns(contactTypes.GetEnumerator());
            _mockContext.Setup(c => c.ContactType).Returns(mockSet.Object);
            _mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var result = _service.GetAll();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count());            
        }
        [Fact]
        public void GetByValue_ReturnsCorrectContactType()
        {
           
            var contactTypes = new List<ContactType>
        {
            new ContactType { Id = 1, Value = "Email" },
            new ContactType { Id = 2, Value = "Phone" }
        }.AsQueryable();

            var mockSet = new Mock<DbSet<ContactType>>();
            mockSet.As<IQueryable<ContactType>>().Setup(m => m.Provider).Returns(contactTypes.Provider);
            mockSet.As<IQueryable<ContactType>>().Setup(m => m.Expression).Returns(contactTypes.Expression);
            mockSet.As<IQueryable<ContactType>>().Setup(m => m.ElementType).Returns(contactTypes.ElementType);
            mockSet.As<IQueryable<ContactType>>().Setup(m => m.GetEnumerator()).Returns(contactTypes.GetEnumerator());

            _mockContext.Setup(c => c.ContactType).Returns(mockSet.Object);

            
            var result = _service.GetByValue("Email");
           
            Assert.NotNull(result);
            Assert.Equal("Email", result.Value);
        }
        [Fact]
        public void Delete_RemovesContactType()
        {
           
            var contactType = new ContactType { Id = 1, Value = "Email" };

            var mockSet = new Mock<DbSet<ContactType>>();
            mockSet.Setup(m => m.Find(It.IsAny<int>())).Returns(contactType);

            _mockContext.Setup(c => c.ContactType).Returns(mockSet.Object);
            
            _service.Delete(1);
            
            mockSet.Verify(m => m.Remove(It.Is<ContactType>(ct => ct.Id == 1)), Times.Once);
            _mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }
    }
}
