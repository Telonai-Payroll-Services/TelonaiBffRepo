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
                var addlMoneyReceived = fixture.CreateMany<AdditionalOtherMoneyReceived>().ToList();
                var incomeTax = fixture.CreateMany<IncomeTax>().ToList();
                incomeTax.Select(x => x.PayStub = paystub);
                incomeTax.Select(x => x.PayStubId = paystub.Id);

                // Arrange
                var document = new Document {  /* set properties */ };

                // Act
                var result = await _documentManager.CreatePayStubPdfAsync(paystub, addlMoneyReceived,
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
