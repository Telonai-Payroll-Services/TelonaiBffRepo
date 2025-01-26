using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Xunit;
using System.Linq.Expressions;
using TelonaiWebApi.Entities;
using TelonaiWebApi.Helpers;
using TelonaiWebApi.Models;
using TelonaiWebApi.Services;

namespace TelonaiWebApi.UnitTest.Services
{
    public class PersonServiceTests
    {
        private readonly Mock<DataContext> _mockContext;
        private readonly Mock<IMapper> _mockMapper;
        private readonly PersonService _service;
        private readonly DataContext _context;
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;

        public PersonServiceTests()
        {
            _mockContext = new Mock<DataContext>(MockBehavior.Default, new object[] { new Mock<IHttpContextAccessor>().Object });
            _mockMapper = new Mock<IMapper>();
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            _service = new PersonService(_mockContext.Object, _mockMapper.Object, _mockHttpContextAccessor.Object, null,null,null,null);
        }
        [Fact]
        public void GetByCompanyId_ReturnsCorrectData()
        {

            int companyId = 1;
            var persons = new List<Person>
            {
                new Person { Id = 1, CompanyId = companyId, Deactivated = false },
                new Person { Id = 2, CompanyId = companyId, Deactivated = false }
            }.AsQueryable();

            var mockSet = new Mock<DbSet<Person>>();
            mockSet.As<IQueryable<Person>>().Setup(m => m.Provider).Returns(persons.Provider);
            mockSet.As<IQueryable<Person>>().Setup(m => m.Expression).Returns(persons.Expression);
            mockSet.As<IQueryable<Person>>().Setup(m => m.ElementType).Returns(persons.ElementType);
            mockSet.As<IQueryable<Person>>().Setup(m => m.GetEnumerator()).Returns(persons.GetEnumerator());

            _mockContext.Setup(c => c.Person).Returns(mockSet.Object);

            var personModels = new List<PersonModel>
        {
            new PersonModel { Id = 1, CompanyId = companyId },
            new PersonModel { Id = 2, CompanyId = companyId }
             };

            _mockMapper.Setup(m => m.Map<IList<PersonModel>>(It.IsAny<IEnumerable<Person>>())).Returns(personModels);


            var result = _service.GetByCompanyId(companyId);

            Assert.Equal(2, result.Count);
            Assert.Equal(companyId, result[0].CompanyId);
            Assert.Equal(companyId, result[1].CompanyId);
        }
        [Fact]
        public void Get_ReturnsAllPersons()
        {

            var persons = new List<Person>
            {
                new Person { Id = 1, Deactivated = false },
                new Person { Id = 2, Deactivated = false }
            }.AsQueryable();

            var mockSet = new Mock<DbSet<Person>>();
            mockSet.As<IQueryable<Person>>().Setup(m => m.Provider).Returns(persons.Provider);
            mockSet.As<IQueryable<Person>>().Setup(m => m.Expression).Returns(persons.Expression);
            mockSet.As<IQueryable<Person>>().Setup(m => m.ElementType).Returns(persons.ElementType);
            mockSet.As<IQueryable<Person>>().Setup(m => m.GetEnumerator()).Returns(persons.GetEnumerator());

            _mockContext.Setup(c => c.Person).Returns(mockSet.Object);

            var personModels = new List<PersonModel>
            {
                new PersonModel { Id = 1 },
                new PersonModel { Id = 2 }
            };

            _mockMapper.Setup(m => m.Map<IList<PersonModel>>(It.IsAny<IEnumerable<Person>>())).Returns(personModels);


            var result = _service.Get();


            Assert.Equal(2, result.Count);
            Assert.Equal(1, result[0].Id);
            Assert.Equal(2, result[1].Id);
        }



        [Fact]
        public async Task GetByEmailAndCompanyIdAsync_ReturnsPersonModel_WhenFound()
        {


            string email = "test@example.com";
            int companyId = 1;
            var persons = new List<Person>
            {
                new Person { Id = 1, Email = email, Deactivated = false }
            };

            var mockSet = new Mock<DbSet<Person>>();
            mockSet.As<IQueryable<Person>>().Setup(m => m.Provider).Returns(new Mock<IQueryProvider>().Object);

            mockSet.As<IQueryable<Person>>().Setup(m => m.Provider).Returns(new Mock<IAsyncQueryProvider>().Object);
            mockSet.As<IQueryable<Person>>().Setup(m => m.Expression).Returns(Expression.Constant(persons.AsQueryable()));
            mockSet.As<IQueryable<Person>>().Setup(m => m.ElementType).Returns(typeof(Person));
            mockSet.As<IQueryable<Person>>().Setup(m => m.GetEnumerator()).Returns(persons.GetEnumerator());
            _mockContext.Setup(c => c.Person).Returns(mockSet.Object);
            var personModel = new PersonModel { Id = 1, Email = email };
            _mockMapper.Setup(m => m.Map<PersonModel>(It.IsAny<Person>())).Returns(personModel);


            var result = await _service.GetByEmailAndCompanyIdAsync(email, companyId);


            Assert.NotNull(result);
            Assert.Equal(personModel.Id, result.Id);
            Assert.Equal(personModel.Email, result.Email);
        }
        [Fact]
        public async Task GetByEmailAndCompanyIdAsync_ReturnsNull_WhenNotFound()
        {

            string email = "test@example.com";
            int companyId = 1;
            var mockSet = new Mock<DbSet<Person>>();
            var persons = new List<Person> { };

            mockSet.As<IQueryable<Person>>().Setup(m => m.Provider).Returns(new Mock<IQueryProvider>().Object);

            mockSet.As<IQueryable<Person>>().Setup(m => m.Provider).Returns(new Mock<IAsyncQueryProvider>().Object);
            mockSet.As<IQueryable<Person>>().Setup(m => m.Expression).Returns(Expression.Constant(persons.AsQueryable()));
            mockSet.As<IQueryable<Person>>().Setup(m => m.ElementType).Returns(typeof(Person));
            mockSet.As<IQueryable<Person>>().Setup(m => m.GetEnumerator()).Returns(persons.GetEnumerator());
            _mockContext.Setup(c => c.Person).Returns(mockSet.Object);


            var result = await _service.GetByEmailAndCompanyIdAsync(email, companyId);


            Assert.Null(result);
        }

        [Fact]
        public async Task GetByEmailAsync_ReturnsCorrectPerson()
        {

            string email = "test@example.com";
            var persons = new List<Person>
        {
            new Person { Id = 1, Email = email, Deactivated = false }
        }.AsQueryable();

            var mockSet = new Mock<DbSet<Person>>();
            mockSet.As<IQueryable<Person>>().Setup(m => m.Provider).Returns(persons.Provider);
            mockSet.As<IQueryable<Person>>().Setup(m => m.Provider).Returns(new Mock<IAsyncQueryProvider>().Object);
            mockSet.As<IQueryable<Person>>().Setup(m => m.Expression).Returns(persons.Expression);
            mockSet.As<IQueryable<Person>>().Setup(m => m.ElementType).Returns(persons.ElementType);
            mockSet.As<IQueryable<Person>>().Setup(m => m.GetEnumerator()).Returns(persons.GetEnumerator());

            _mockContext.Setup(c => c.Person).Returns(mockSet.Object);

            var personModel = new PersonModel { Id = 1, Email = email };

            _mockMapper.Setup(m => m.Map<PersonModel>(It.IsAny<Person>())).Returns(personModel);

            var result = await _service.GetByEmailAsync(email);


            Assert.NotNull(result);
            Assert.Equal(email, result.Email);
        }
        [Fact]
        public async Task GetDetailsById_ReturnsPersonModel_WhenFound()
        {
            int id = 1;
            string email = "test@example.com";
            var persons = new List<Person>
        {
            new Person { Id = id, Email = email, Deactivated = false ,
             Zipcode = new Zipcode
             {
                    Id = 3,
                    Code="NorthCarolina 123st",
                    City = new City { Id = 4, State = new State { Id = 5, Country = new Country { Id = 6 } } }
                }}
        }.AsQueryable();

            var mockSet = new Mock<DbSet<Person>>();
            mockSet.As<IQueryable<Person>>().Setup(m => m.Provider).Returns(persons.Provider);
            mockSet.As<IQueryable<Person>>().Setup(m => m.Provider).Returns(new Mock<IAsyncQueryProvider>().Object);
            mockSet.As<IQueryable<Person>>().Setup(m => m.Expression).Returns(persons.Expression);
            mockSet.As<IQueryable<Person>>().Setup(m => m.ElementType).Returns(persons.ElementType);
            mockSet.As<IQueryable<Person>>().Setup(m => m.GetEnumerator()).Returns(persons.GetEnumerator());

            _mockContext.Setup(c => c.Person).Returns(mockSet.Object);

            var personModel = new PersonModel
            {
                Id = 1,
                Email = email,
                Zipcode = "NorthCarolina 123st",
                ZipcodeId = 3,
                CityId = 4,
                StateId = 5,
                CountryId = 6,

            };

            _mockMapper.Setup(m => m.Map<PersonModel>(It.IsAny<Person>())).Returns(personModel);

            var result = _service.GetDetailsById(id);

            Assert.NotNull(result);
            Assert.Equal(personModel.Id, result.Id);
            Assert.NotNull(result.Zipcode);
            Assert.Equal(personModel.ZipcodeId, result.ZipcodeId);

        }

        [Fact]

        public async Task GetDetailsById_ReturnsNull_WhenNotFound()
        {
            int id = 1;
            var mockSet = new Mock<DbSet<Person>>();
            var persons = new List<Person> { };

            mockSet.As<IQueryable<Person>>().Setup(m => m.Provider).Returns(new Mock<IQueryProvider>().Object);

            mockSet.As<IQueryable<Person>>().Setup(m => m.Provider).Returns(new Mock<IAsyncQueryProvider>().Object);
            mockSet.As<IQueryable<Person>>().Setup(m => m.Expression).Returns(Expression.Constant(persons.AsQueryable()));
            mockSet.As<IQueryable<Person>>().Setup(m => m.ElementType).Returns(typeof(Person));
            mockSet.As<IQueryable<Person>>().Setup(m => m.GetEnumerator()).Returns(persons.GetEnumerator());
            _mockContext.Setup(c => c.Person).Returns(mockSet.Object);

            var result = _service.GetDetailsById(id);

            Assert.Null(result);
        }
        [Fact]
        public async Task GetById_ReturnsPersonModel_WhenFound()
        {


            int id = 1;
            var person = new Person { Id = id, Email = "test@example.com" };

            _mockContext.Setup(c => c.Person.Find(id)).Returns(person);
            var personModel = new PersonModel { Id = id, Email = "test@example.com" };
            _mockMapper.Setup(m => m.Map<PersonModel>(It.IsAny<Person>())).Returns(personModel);


            var result = _service.GetById(id);


            Assert.NotNull(result);
            Assert.Equal(personModel.Id, result.Id);
        }

        [Fact]
        public async Task GetById_ReturnsNull_WhenNotFound()
        {

            int id = 1;

            _mockContext.Setup(c => c.Person.Find(id)).Returns((Person)null);

            var result = _service.GetById(id);


            Assert.Null(result);
        }

        [Fact]
        public async Task CreateAsync_ReturnsPerson_WhenCreatedSuccessfully()
        {
            string email = "test@example.com";
            var persons = new List<Person>
        {
            new Person { Id = 1, Email = email, Deactivated = false ,Ssn="existingssn123"}
        }.AsQueryable();

            var mockSet = new Mock<DbSet<Person>>();
            mockSet.As<IQueryable<Person>>().Setup(m => m.Provider).Returns(persons.Provider);
            mockSet.As<IQueryable<Person>>().Setup(m => m.Provider).Returns(new Mock<IAsyncQueryProvider>().Object);
            mockSet.As<IQueryable<Person>>().Setup(m => m.Expression).Returns(persons.Expression);
            mockSet.As<IQueryable<Person>>().Setup(m => m.ElementType).Returns(persons.ElementType);
            mockSet.As<IQueryable<Person>>().Setup(m => m.GetEnumerator()).Returns(persons.GetEnumerator());

            _mockContext.Setup(c => c.Person).Returns(mockSet.Object);


            var personModel = new PersonModel { Email = "test@example.com", CompanyId = 1, Ssn = "newssn123" };
            var person = new Person { Email = "test@example.com", CompanyId = 1, Ssn = "newssn123" };


            _mockMapper.Setup(m => m.Map<Person>(It.IsAny<PersonModel>())).Returns(person);


            var result = await _service.CreateAsync(personModel);


            Assert.NotNull(result);
            Assert.Equal(personModel.Email, result.Email);
            Assert.Equal(personModel.Ssn, result.Ssn);

        }

        [Fact]
        public async Task CreateAsync_ThrowsAppException_WhenEmailAlreadyExists()
        {

            string email = "test@example.com";
            var companyExisting = 1;
            var persons = new List<Person>
    {
        new Person { Id = 1, Email = email, CompanyId = companyExisting, Deactivated = false }
    }.AsQueryable();

            var mockSet = new Mock<DbSet<Person>>();
            mockSet.As<IQueryable<Person>>().Setup(m => m.Provider).Returns(persons.Provider);
            mockSet.As<IQueryable<Person>>().Setup(m => m.Expression).Returns(persons.Expression);
            mockSet.As<IQueryable<Person>>().Setup(m => m.ElementType).Returns(persons.ElementType);
            mockSet.As<IQueryable<Person>>().Setup(m => m.GetEnumerator()).Returns(persons.GetEnumerator());


            _mockContext.Setup(c => c.Person).Returns(mockSet.Object);

            var personModel = new PersonModel { Email = email, CompanyId = companyExisting, Ssn = "newssn123" };
            _mockMapper.Setup(m => m.Map<Person>(It.IsAny<PersonModel>())).Returns(It.IsAny<Person>());


            await Assert.ThrowsAsync<AppException>(async () => await _service.CreateAsync(personModel));
        }

        [Fact]
        public async Task UpdateAsync_UpdatesPerson_AndEmployment_WhenSsnProvided()
        {
           
            int id = 1;
            var updatePersonModel = new PersonModel { Email = "newemail@example.com", Ssn = "newssn123" };
            var exstingPerson = new Person { Id = id, Email = "existingemail@example.com" };
            var exstingEmployment = new Employment { PersonId = id, SignUpStatusTypeId = (int)SignUpStatusTypeModel.UserProfileCreationStarted };

           

            _mockContext.Setup(c => c.Person.Find(id)).Returns(exstingPerson);
             var employments = new List<Employment>
                { new Employment { PersonId=id ,SignUpStatusTypeId = (int)SignUpStatusTypeModel.UserProfileCreationStarted, Deactivated = false },
           
                }.AsQueryable();
            var mockEmploymentSet = new Mock<DbSet<Employment>>();
            mockEmploymentSet.As<IQueryable<Employment>>().Setup(m => m.Provider).Returns(employments.Provider);
            mockEmploymentSet.As<IQueryable<Employment>>().Setup(m => m.Expression).Returns(employments.Expression);
            mockEmploymentSet.As<IQueryable<Employment>>().Setup(m => m.ElementType).Returns(employments.ElementType);
            mockEmploymentSet.As<IQueryable<Employment>>().Setup(m => m.GetEnumerator()).Returns(employments.GetEnumerator());
            _mockContext.Setup(c => c.Employment).Returns(mockEmploymentSet.Object);
         
            _mockMapper.Setup(m => m.Map(updatePersonModel, exstingPerson))
            .Callback<PersonModel, Person>((src, dest) => dest.Email = src.Email);

           
            await _service.UpdateAsync(id, updatePersonModel);

            Assert.Equal("newemail@example.com", exstingPerson.Email);
            Assert.Equal(updatePersonModel.Email, exstingPerson.Email);
          
        }
        [Fact]
        public async Task UpdateAsync_UpdatesPerson_WhenNoSsnProvided()
        {
          

            int id = 1;
            var updatePersonModel = new PersonModel { Email = "newemail@example.com" };
            var exstingPerson = new Person { Id = id, Email = "existingemail@example.com" };
          
            _mockContext.Setup(c => c.Person.Find(id)).Returns(exstingPerson);

            _mockMapper.Setup(m => m.Map(updatePersonModel, exstingPerson))
            .Callback<PersonModel, Person>((src, dest) => dest.Email = src.Email);


            await _service.UpdateAsync(id, updatePersonModel);

            Assert.Equal("newemail@example.com", exstingPerson.Email);
            Assert.Equal(updatePersonModel.Email, exstingPerson.Email);
            
          
        }
        [Fact]
        public async Task UpdateAsync_ThrowsAppException_WhenPersonNotFound()
        {
            // Arrange
            int id = 1;
            var personModel = new PersonModel { Email = "test@example.com" };
           

            _mockContext.Setup(c => c.Person.Find(id)).Returns((Person)null);
                    
            await Assert.ThrowsAsync<AppException>(async () => await _service.UpdateAsync(id, personModel));
        }

        [Fact]
        public async Task DeleteAsync_DeletesPerson_WhenFound()
        {           
            int id = 1;
            var user = new Person { Id = id, Deactivated = false };

            _mockContext.Setup(c => c.Person.Find(id)).Returns(user);

             _service.DeleteAsync(id);
       
            Assert.True(user.Deactivated); 
            
        }

        [Fact]
        public async Task DeleteAsync_ThrowsAppException_WhenNotFound()
        {
                   
            int id = 1;

            _mockContext.Setup(c => c.Person.Find(id)).Returns((Person)null);     
            
            await Assert.ThrowsAsync<AppException>(async () => await _service.DeleteAsync(id));
        }

        [Fact]
        public async Task IsEmployeeMinor_EnterPersonBirthDayForMinor_ReturnTrue()
        {
            //Arrange
            DateOnly dateofbirth = new DateOnly(2010, 2, 20);
            var person = new Person { Id = 1, DateOfBirth = dateofbirth };

            //Act
            var result = _service.IsEmployeeMinor(person.DateOfBirth.Value);


            //Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IsEmployeeMinor_EnterPersonBirthDayForMinor_ReturnFalse()
        {
            //Arrange
            DateOnly dateofbirth = new DateOnly(1984, 2, 20);
            var person = new Person { Id = 1, DateOfBirth = dateofbirth };

            //Act
            var result = _service.IsEmployeeMinor(person.DateOfBirth.Value);


            //Assert
            Assert.False(result);
        }
    }
}
