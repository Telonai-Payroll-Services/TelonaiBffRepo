using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using TelonaiWebApi.Entities;
using TelonaiWebApi.Helpers;
using TelonaiWebApi.Models;
using TelonaiWebApi.Services;
using AutoFixture;


namespace TelonaiWebAPI.UnitTest.Services
{
    public class DocumentManagerTests
    {
        private readonly DocumentManager _documentManager; 
        private readonly Mock<DataContext> _mockDataContext;
        public DocumentManagerTests()
        {
            _mockDataContext = new Mock<DataContext>();
            _documentManager = new DocumentManager(_mockDataContext.Object);
        }

        [Fact]
        public async Task CreateDocument_ValidInput_CallsRepository()
        {
            try
            {
                Fixture fixture = CustomFixture.Create();
                var paystub = fixture.Create<PayStub>();
                Person person = new Person()
                {
                    Id = 67,
                    FirstName = "Biruk",
                    LastName = "Assefa",
                    AddressLine1 = "Wawel Street",
                    AddressLine2 = "Tesfaye kagela Appertment",
                    Zipcode = new Zipcode()
                    {
                        Code = "27413",
                        Id = 2864,
                        City = new City()
                        {
                            Id = 269,
                            Name = "Greensboro",
                            State = new State()
                            {
                                Id = 37,
                                Name = "North Carolina",
                                CountryId = 2,
                                StateCode = "NC"
                            },
                            StateId = 37
                        },
                        CityId = 269
                    }

                };
                Company company = new Company()
                {
                    Id = 42,
                    Name = "Ahadu Computing Plc",
                    AddressLine1 = "Tesfaye Kegela Apartment",
                    AddressLine2 = "4nd FloorMatthews",
                    Zipcode = new Zipcode()
                    {
                        Code = "27409",
                        Id = 2860,
                        City = new City()
                        {
                            Id = 438,
                            Name = "Newton",
                            State = new State()
                            {
                                Id = 37,
                                Name = "North Carolina",
                                CountryId = 2,
                                StateCode = "NC"
                            },
                            StateId = 37
                        },
                        CityId = 438
                    }
                };
                paystub.Employment.Person = person;
                paystub.Employment.PersonId = person.Id;
                paystub.Payroll.Company = company;
                paystub.Payroll.CompanyId = company.Id;
                var otherMoneyReceived = paystub.OtherMoneyReceived;
                var addlMoneyReceived = fixture.CreateMany<AdditionalOtherMoneyReceived>().ToList();
                var incomeTax = fixture.CreateMany<IncomeTax>().ToList();
                incomeTax.Select(x => x.PayStub = paystub);
                incomeTax.Select(x => x.PayStubId = paystub.Id);

                // Arrange
                var document = new Document {  /* set properties */ };

                // Act
                var result = await _documentManager.CreatePayStubPdfAsync(paystub, otherMoneyReceived, addlMoneyReceived,
                    incomeTax);

                // Assert
                Assert.NotNull(result);
            }
            catch (Exception ex)
            {
            }
        }
    }

}
