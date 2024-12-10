
using Amazon.S3.Transfer;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit2;
using Moq;
using TelonaiWebApi.Entities;
using TelonaiWebApi.Helpers;
using TelonaiWebAPI.UnitTest.Helper;
using Xunit;

namespace UnitTests
{
    public class DocumentManagerTests
    {
        private readonly Fixture _fixture;
        private readonly Mock<ITransferUtility> _transferUtilityMock;
        private readonly DocumentManager _documentManager;

        public DocumentManagerTests()
        {
            _fixture = CustomFixture.Create();       
            _transferUtilityMock = _fixture.Freeze<Mock<ITransferUtility>>();
            _documentManager = _fixture.Create<DocumentManager>();
        }

        [Theory, CustomAutoData]
        public async Task CreatePayStubPdfAsync_ShouldCreateAndUploadPdf(
            PayStub payStub,
            OtherMoneyReceived otherReceived,
            List<AdditionalOtherMoneyReceived> additionalMoneyReceived)
        {
         
              
                payStub.Employment.Person.FirstName = _fixture.Create<string>();
                payStub.Employment.Person.LastName = _fixture.Create<string>();
                payStub.Payroll.Company.Name = _fixture.Create<string>();


            payStub.Payroll.StartDate = _fixture.Create<DateOnly>();
            payStub.Payroll.ScheduledRunDate = _fixture.Create<DateOnly>();

            // Create IncomeTaxes with the PayStub correctly set
            var incomeTaxes = _fixture.Build<IncomeTax>()
                                          .With(t => t.PayStub, payStub)
                                          .CreateMany(5)
                                          .ToList();             
                var result = await _documentManager.CreatePayStubPdfAsync(payStub, otherReceived, additionalMoneyReceived, incomeTaxes);
      
                Assert.NotEqual(Guid.Empty, result);
             
        }
    }
}
