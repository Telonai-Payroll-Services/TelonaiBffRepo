
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

            SetupPayStub(payStub);

            // Create IncomeTaxes with the PayStub correctly set
            var incomeTaxes = _fixture.Build<IncomeTax>()
                                          .With(t => t.PayStub, payStub)
                                          .CreateMany(5)
                                          .ToList();             
                var result = await _documentManager.CreatePayStubPdfAsync(payStub, otherReceived, additionalMoneyReceived, incomeTaxes);
      
                Assert.NotEqual(Guid.Empty, result);
             
        }
        [Theory, CustomAutoData]
        public async Task CreatePayStubPdfAsync_ShouldHandleNullAdditionalMoneyReceived(
            PayStub payStub,
            OtherMoneyReceived otherReceived)
        {
            SetupPayStub(payStub);

            var incomeTaxes = _fixture.Build<IncomeTax>()
                                      .With(t => t.PayStub, payStub)
                                      .CreateMany(5)
                                      .ToList();
       
            var result = await _documentManager.CreatePayStubPdfAsync(payStub, otherReceived, null, incomeTaxes);
       
            Assert.NotEqual(Guid.Empty, result);
           
        }
        [Theory, CustomAutoData]
        public async Task CreatePayStubPdfAsync_ShouldHandleEmptyAdditionalMoneyReceived(
            PayStub payStub,
            OtherMoneyReceived otherReceived)
        {
            SetupPayStub(payStub);

            var incomeTaxes = _fixture.Build<IncomeTax>()
                                      .With(t => t.PayStub, payStub)
                                      .CreateMany(5)
                                      .ToList();


            var result = await _documentManager.CreatePayStubPdfAsync(payStub, otherReceived, new List<AdditionalOtherMoneyReceived>(), incomeTaxes);

            Assert.NotEqual(Guid.Empty, result);
          
        }
        [Theory, CustomAutoData]
        public async Task CreatePayStubPdfAsync_ShouldHandleNullOtherMoneyReceived(
            PayStub payStub,
            List<AdditionalOtherMoneyReceived> additionalMoneyReceived)
        {
            SetupPayStub(payStub);

            var incomeTaxes = _fixture.Build<IncomeTax>()
                                      .With(t => t.PayStub, payStub)
                                      .CreateMany(5)
                                      .ToList();

            var result = await _documentManager.CreatePayStubPdfAsync(payStub, null, additionalMoneyReceived, incomeTaxes);

            Assert.NotEqual(Guid.Empty, result);
        }
        [Theory, CustomAutoData]
        public async Task CreatePayStubPdfAsync_ShouldHandleValidData(
            PayStub payStub,
            OtherMoneyReceived otherReceived,
            List<AdditionalOtherMoneyReceived> additionalMoneyReceived,
            List<IncomeTax> incomeTaxes)
        {
            SetupPayStub(payStub);

            var result = await _documentManager.CreatePayStubPdfAsync(payStub, otherReceived, additionalMoneyReceived, incomeTaxes);

            Assert.NotEqual(Guid.Empty, result);         
        }
        [Theory, CustomAutoData]
        public async Task CreatePayStubPdfAsync_ShouldHandleMinimumValidData(
            PayStub payStub)
        {
            SetupPayStub(payStub);

            var result = await _documentManager.CreatePayStubPdfAsync(payStub, null, new List<AdditionalOtherMoneyReceived>(), new List<IncomeTax>());

            Assert.NotEqual(Guid.Empty, result);         
        }
        [Theory, CustomAutoData]
        public async Task CreatePayStubPdfAsync_ShouldThrowAppExceptionWhenPayStubIsNull(
              OtherMoneyReceived otherReceived,
              List<AdditionalOtherMoneyReceived> additionalMoneyReceived,
              List<IncomeTax> incomeTaxes)
        {
            var exception = await Assert.ThrowsAsync<AppException>(() =>
                _documentManager.CreatePayStubPdfAsync(null, otherReceived, additionalMoneyReceived, incomeTaxes));

            Assert.Equal("PayStub not found", exception.Message);
        }

        [Theory, CustomAutoData]
        public async Task CreatePayStubPdfAsync_ShouldHandleEmptyIncomeTaxes(
            PayStub payStub,
            OtherMoneyReceived otherReceived,
            List<AdditionalOtherMoneyReceived> additionalMoneyReceived)
        {
            SetupPayStub(payStub);

            var result = await _documentManager.CreatePayStubPdfAsync(payStub, otherReceived, additionalMoneyReceived, new List<IncomeTax>());

            Assert.NotEqual(Guid.Empty, result);         
        }
        [Theory, CustomAutoData]
        public async Task CreatePayStubPdfAsync_ShouldHandleAllOptionalParametersNull(
            PayStub payStub)
        {
            SetupPayStub(payStub);

            var result = await _documentManager.CreatePayStubPdfAsync(payStub, null, null, null);

            Assert.NotEqual(Guid.Empty, result);         
        }
        private void SetupPayStub(PayStub payStub)
        { 
            payStub.Employment.Person.FirstName = _fixture.Create<string>();
            payStub.Employment.Person.LastName = _fixture.Create<string>(); 
            payStub.Payroll.Company.Name = _fixture.Create<string>(); 
            payStub.Payroll.StartDate = _fixture.Create<DateOnly>();
            payStub.Payroll.ScheduledRunDate = _fixture.Create<DateOnly>(); 
        }

    }
}
