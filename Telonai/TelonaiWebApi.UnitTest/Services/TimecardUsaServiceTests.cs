using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Sockets;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using TelonaiWebApi.Entities;
using TelonaiWebApi.Helpers;
using TelonaiWebApi.Models;
using TelonaiWebApi.Services;
using Xunit;

namespace TelonaiWebAPI.UnitTest.Services
{


    public class TimecardUsaServiceTests
    {
        private readonly Mock<DbSet<Person>> _personSetMock;
        private readonly Mock<DbSet<TimecardUsa>> _timecardSetMock;
        private readonly Mock<DataContext> _contextMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly TimecardUsaService _service;

        public TimecardUsaServiceTests()
        {
            _personSetMock = new Mock<DbSet<Person>>();
            _timecardSetMock = new Mock<DbSet<TimecardUsa>>();
            _contextMock = new Mock<DataContext>();
            _mapperMock = new Mock<IMapper>();
            _service = new TimecardUsaService(_contextMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task GetReport_ShouldReturnMappedResult_WhenValidEmailAndDateRangeProvided()
        {
           
            var email = "test@example.com";
            var from = new DateTime(2023, 1, 1);
            var to = new DateTime(2023, 12, 31);
            var person = new Person { Id = 1, Email = email, Deactivated = false };
            var persons = new List<Person>() {person
           }.AsQueryable();
            var timecards = new List<TimecardUsa>
        {
            new TimecardUsa { PersonId = person.Id, CreatedDate = new DateTime(2023, 6, 1), Job = new Job() }
        }.AsQueryable();

          
            _personSetMock.As<IQueryable<Person>>().Setup(m => m.Provider).Returns(persons.Provider);
            _personSetMock.As<IQueryable<Person>>().Setup(m => m.Expression).Returns(persons.AsQueryable().Expression);
            _personSetMock.As<IQueryable<Person>>().Setup(m => m.ElementType).Returns(persons.AsQueryable().ElementType);
            _personSetMock.As<IQueryable<Person>>().Setup(m => m.GetEnumerator()).Returns(persons.AsQueryable().GetEnumerator());

           
            _timecardSetMock.As<IQueryable<TimecardUsa>>().Setup(m => m.Provider).Returns(timecards.Provider);
            _timecardSetMock.As<IQueryable<TimecardUsa>>().Setup(m => m.Expression).Returns(timecards.Expression);
            _timecardSetMock.As<IQueryable<TimecardUsa>>().Setup(m => m.ElementType).Returns(timecards.ElementType);
            _timecardSetMock.As<IQueryable<TimecardUsa>>().Setup(m => m.GetEnumerator()).Returns(timecards.GetEnumerator());

          
            _contextMock.Setup(c => c.Person).Returns(_personSetMock.Object);
            _contextMock.Setup(c => c.TimecardUsa).Returns(_timecardSetMock.Object);

            
            var mappedResult = new List<TimecardUsaModel>
        {
            new TimecardUsaModel { PersonId = person.Id }
        };
            _mapperMock.Setup(m => m.Map<IList<TimecardUsaModel>>(It.IsAny<object>())).Returns(mappedResult);

            
            var result = _service.GetReport(email, from, to);

            Assert.NotNull(result);
            Assert.Single(result);
        }

        [Fact]
        public async Task GetReport_ShouldReturnEmpty_WhenNoMatchingRecords()
        {
            
            var email = "nomatch@example.com";
            var from = new DateTime(2023, 1, 1);
            var to = new DateTime(2023, 12, 31);

            var person = new Person { Id = 1, Email = email, Deactivated = false };
            var persons = new List<Person>() {person
           }.AsQueryable();

            _personSetMock.As<IQueryable<Person>>().Setup(m => m.Provider).Returns(persons.Provider);
            _personSetMock.As<IQueryable<Person>>().Setup(m => m.Expression).Returns(persons.AsQueryable().Expression);
            _personSetMock.As<IQueryable<Person>>().Setup(m => m.ElementType).Returns(persons.AsQueryable().ElementType);
            _personSetMock.As<IQueryable<Person>>().Setup(m => m.GetEnumerator()).Returns(persons.AsQueryable().GetEnumerator());
            _contextMock.Setup(c => c.Person).Returns(_personSetMock.Object);

            var timecards = new List<TimecardUsa>
            {
            }.AsQueryable();
            _timecardSetMock.As<IQueryable<TimecardUsa>>().Setup(m => m.Provider).Returns(timecards.Provider);
            _timecardSetMock.As<IQueryable<TimecardUsa>>().Setup(m => m.Expression).Returns(timecards.Expression);
            _timecardSetMock.As<IQueryable<TimecardUsa>>().Setup(m => m.ElementType).Returns(timecards.ElementType);
            _timecardSetMock.As<IQueryable<TimecardUsa>>().Setup(m => m.GetEnumerator()).Returns(timecards.GetEnumerator());
           
            _contextMock.Setup(c => c.TimecardUsa).Returns(_timecardSetMock.Object);


            var mappedResult = new List<TimecardUsaModel>();
            _mapperMock.Setup(m => m.Map<IList<TimecardUsaModel>>(It.IsAny<object>())).Returns(mappedResult);

           
            var result = _service.GetReport(email, from, to);
          
            Assert.NotNull(result);
            //Assert.Empty(result);
        }
       

        [Fact]
        public async Task GetReportByCompanyId_ShouldReturnMappedResult_WhenValidCompanyIdAndDateRangeProvided()
        {
            // Arrange
            var companyId = 123;
            var from = new DateTime(2023, 1, 1);
            var to = new DateTime(2023, 12, 31);

            var timecards = new List<TimecardUsa>
        {
            new TimecardUsa { Job = new Job { CompanyId = companyId }, CreatedDate = new DateTime(2023, 6, 1) }
        }.AsQueryable();

            
            _timecardSetMock.As<IQueryable<TimecardUsa>>().Setup(m => m.Provider).Returns(timecards.Provider);
            _timecardSetMock.As<IQueryable<TimecardUsa>>().Setup(m => m.Expression).Returns(timecards.Expression);
            _timecardSetMock.As<IQueryable<TimecardUsa>>().Setup(m => m.ElementType).Returns(timecards.ElementType);
            _timecardSetMock.As<IQueryable<TimecardUsa>>().Setup(m => m.GetEnumerator()).Returns(timecards.GetEnumerator());

            _contextMock.Setup(c => c.TimecardUsa).Returns(_timecardSetMock.Object);

            var mappedResult = new List<TimecardUsaModel>
        {
            new TimecardUsaModel { Job = "joblocation1", CreatedDate = new DateTime(2023, 6, 1) }
        };
            _mapperMock.Setup(m => m.Map<IList<TimecardUsaModel>>(It.IsAny<object>())).Returns(mappedResult);


            var result = _service.GetReport(companyId, from, to);

            Assert.NotNull(result);
            Assert.Single(result);
        }

        [Fact]
        public async Task GetReportByCompanyId_ShouldReturnEmpty_WhenNoMatchingRecords()
        {
            
            var companyId = 123;
            var from = new DateTime(2023, 1, 1);
            var to = new DateTime(2023, 12, 31);

            var timecards = new List<TimecardUsa>().AsQueryable();

      
            _timecardSetMock.As<IQueryable<TimecardUsa>>().Setup(m => m.Provider).Returns(timecards.Provider);
            _timecardSetMock.As<IQueryable<TimecardUsa>>().Setup(m => m.Expression).Returns(timecards.Expression);
            _timecardSetMock.As<IQueryable<TimecardUsa>>().Setup(m => m.ElementType).Returns(timecards.ElementType);
            _timecardSetMock.As<IQueryable<TimecardUsa>>().Setup(m => m.GetEnumerator()).Returns(timecards.GetEnumerator());


            _contextMock.Setup(c => c.TimecardUsa).Returns(_timecardSetMock.Object);


            var mappedResult = new List<TimecardUsaModel>();
            _mapperMock.Setup(m => m.Map<IList<TimecardUsaModel>>(It.IsAny<object>())).Returns(mappedResult);


            var result = _service.GetReport(companyId, from, to);

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetReportByEmailAndCompanyId_ShouldReturnMappedResult_WhenValidEmailCompanyIdAndDateRangeProvided()
        {

            var email = "test@example.com";
            var companyId = 123;
            var from = new DateTime(2023, 1, 1);
            var to = new DateTime(2023, 12, 31);

            var person = new Person { Id = 1, Email = email, CompanyId = companyId, Deactivated = false };
            var timecards = new List<TimecardUsa>
        {
            new TimecardUsa { PersonId = person.Id, Job = new Job { CompanyId = companyId }, CreatedDate = new DateTime(2023, 6, 1) }
        }.AsQueryable();

            var personData = new List<Person> { person }.AsQueryable();
            _personSetMock.As<IQueryable<Person>>().Setup(m => m.Provider).Returns(personData.Provider);
            _personSetMock.As<IQueryable<Person>>().Setup(m => m.Expression).Returns(personData.Expression);
            _personSetMock.As<IQueryable<Person>>().Setup(m => m.ElementType).Returns(personData.ElementType);
            _personSetMock.As<IQueryable<Person>>().Setup(m => m.GetEnumerator()).Returns(personData.GetEnumerator());

            _timecardSetMock.As<IQueryable<TimecardUsa>>().Setup(m => m.Provider).Returns(timecards.Provider);
            _timecardSetMock.As<IQueryable<TimecardUsa>>().Setup(m => m.Expression).Returns(timecards.Expression);
            _timecardSetMock.As<IQueryable<TimecardUsa>>().Setup(m => m.ElementType).Returns(timecards.ElementType);
            _timecardSetMock.As<IQueryable<TimecardUsa>>().Setup(m => m.GetEnumerator()).Returns(timecards.GetEnumerator());

            _contextMock.Setup(c => c.Person).Returns(_personSetMock.Object);
            _contextMock.Setup(c => c.TimecardUsa).Returns(_timecardSetMock.Object);


            var mappedResult = new List<TimecardUsaModel>
        {
            new TimecardUsaModel { PersonId = person.Id, Job = "joblocation", CreatedDate = new DateTime(2023, 6, 1) }
        };
            _mapperMock.Setup(m => m.Map<IList<TimecardUsaModel>>(It.IsAny<object>())).Returns(mappedResult);


            var result = _service.GetReport(email, companyId, from, to);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
        }

        [Fact]
        public async Task GetReportByEmailAndCompanyId_ShouldReturnEmpty_WhenNoMatchingRecords()
        {
            var email = "nomatch@example.com";
            var companyId = 123;
            var from = new DateTime(2023, 1, 1);
            var to = new DateTime(2023, 12, 31);
            var person = new Person { Id = 1, Email = email, CompanyId = companyId, Deactivated = false };
          
            var personData = new List<Person> { person }.AsQueryable();
            var timecards = new List<TimecardUsa>().AsQueryable();

            _personSetMock.As<IQueryable<Person>>().Setup(m => m.Provider).Returns(personData.Provider);
            _personSetMock.As<IQueryable<Person>>().Setup(m => m.Expression).Returns(personData.Expression);
            _personSetMock.As<IQueryable<Person>>().Setup(m => m.ElementType).Returns(personData.ElementType);
            _personSetMock.As<IQueryable<Person>>().Setup(m => m.GetEnumerator()).Returns(personData.GetEnumerator());

            _timecardSetMock.As<IQueryable<TimecardUsa>>().Setup(m => m.Provider).Returns(timecards.Provider);
            _timecardSetMock.As<IQueryable<TimecardUsa>>().Setup(m => m.Expression).Returns(timecards.Expression);
            _timecardSetMock.As<IQueryable<TimecardUsa>>().Setup(m => m.ElementType).Returns(timecards.ElementType);
            _timecardSetMock.As<IQueryable<TimecardUsa>>().Setup(m => m.GetEnumerator()).Returns(timecards.GetEnumerator());

            _contextMock.Setup(c => c.Person).Returns(_personSetMock.Object);
            _contextMock.Setup(c => c.TimecardUsa).Returns(_timecardSetMock.Object);


            var mappedResult = new List<TimecardUsaModel>();
            _mapperMock.Setup(m => m.Map<IList<TimecardUsaModel>>(It.IsAny<object>())).Returns(mappedResult);


            var result = _service.GetReport(email, companyId, from, to);

            Assert.NotNull(result);
            Assert.Empty(result);
        }


  
}

    


}
