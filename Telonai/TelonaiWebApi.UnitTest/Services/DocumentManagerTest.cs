
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
        private readonly Random _random;

        public DocumentManagerTests()
        {
            _fixture = CustomFixture.Create();
            _transferUtilityMock = _fixture.Freeze<Mock<ITransferUtility>>();
            _documentManager = _fixture.Create<DocumentManager>();
            _random = new Random();
        }

        [Theory, CustomAutoData]
        public async Task CreatePayStubPdfAsync_ShouldCreateAndUploadPdf(
            PayStub payStub,
            List<AdditionalOtherMoneyReceived> additionalMoneyReceived)
        {
            SetupPayStub(payStub);

            var incomeTaxTypes = new List<string>
                {
                    "Social Security",
                    "Medicare",
                    "Federal Tax",
                    "State Tax",
                    "FUTA",
                    "SUTA",
                    "Additional Medicare"
                };

            var incomeTaxes = new List<IncomeTax>();
            foreach (var name in incomeTaxTypes)
            {
                var incomeTax = _fixture.Build<IncomeTax>()
                    .With(t => t.PayStub, payStub)
                    .With(t => t.IncomeTaxType, _fixture.Build<IncomeTaxType>()
                        .With(x => x.Name, name)
                        .Create())
                    .Create();
                incomeTaxes.Add(incomeTax);
            }

            var result = await _documentManager.CreatePayStubPdfAsync(payStub, additionalMoneyReceived, incomeTaxes);

            Assert.NotEqual(Guid.Empty, result);
        }

        [Theory, CustomAutoData]
        public async Task CreatePayStubPdfAsync_ShouldHandleNullAdditionalMoneyReceived(
            PayStub payStub,
            OtherMoneyReceived otherReceived)
        {
            SetupPayStub(payStub);

            var incomeTaxTypes = new List<string>
                {
                    "Social Security",
                    "Medicare",
                    "Federal Tax",
                    "State Tax",
                    "FUTA",
                    "SUTA",
                    "Additional Medicare"
                };

            var incomeTaxes = new List<IncomeTax>();
            foreach (var name in incomeTaxTypes)
            {
                var incomeTax = _fixture.Build<IncomeTax>()
                    .With(t => t.PayStub, payStub)
                    .With(t => t.IncomeTaxType, _fixture.Build<IncomeTaxType>()
                        .With(x => x.Name, name)
                        .Create())
                    .Create();
                incomeTaxes.Add(incomeTax);
            }

            var result = await _documentManager.CreatePayStubPdfAsync(payStub, null, incomeTaxes);

            Assert.NotEqual(Guid.Empty, result);
        }

        [Theory, CustomAutoData]
        public async Task CreatePayStubPdfAsync_ShouldHandleEmptyAdditionalMoneyReceived(
            PayStub payStub)
        {
            SetupPayStub(payStub);

            var incomeTaxTypes = new List<string>
                {
                    "Social Security",
                    "Medicare",
                    "Federal Tax",
                    "State Tax",
                    "FUTA",
                    "SUTA",
                    "Additional Medicare"
                };

            var incomeTaxes = new List<IncomeTax>();
            foreach (var name in incomeTaxTypes)
            {
                var incomeTax = _fixture.Build<IncomeTax>()
                    .With(t => t.PayStub, payStub)
                    .With(t => t.IncomeTaxType, _fixture.Build<IncomeTaxType>()
                        .With(x => x.Name, name)
                        .Create())
                    .Create();
                incomeTaxes.Add(incomeTax);
            }

            var result = await _documentManager.CreatePayStubPdfAsync(payStub, new List<AdditionalOtherMoneyReceived>(), incomeTaxes);

            Assert.NotEqual(Guid.Empty, result);
        }

        [Theory, CustomAutoData]
        public async Task CreatePayStubPdfAsync_ShouldHandleNullOtherMoneyReceived(
            PayStub payStub,
            List<AdditionalOtherMoneyReceived> additionalMoneyReceived)
        {
            SetupPayStub(payStub);

            var incomeTaxTypes = new List<string>
                {
                    "Social Security",
                    "Medicare",
                    "Federal Tax",
                    "State Tax",
                    "FUTA",
                    "SUTA",
                    "Additional Medicare"
                };

            var incomeTaxes = new List<IncomeTax>();
            foreach (var name in incomeTaxTypes)
            {
                var incomeTax = _fixture.Build<IncomeTax>()
                    .With(t => t.PayStub, payStub)
                    .With(t => t.IncomeTaxType, _fixture.Build<IncomeTaxType>()
                        .With(x => x.Name, name)
                        .Create())
                    .Create();
                incomeTaxes.Add(incomeTax);
            }

            var result = await _documentManager.CreatePayStubPdfAsync(payStub, additionalMoneyReceived, incomeTaxes);

            Assert.NotEqual(Guid.Empty, result);
        }

        [Theory, CustomAutoData]
        public async Task CreatePayStubPdfAsync_ShouldHandleValidData(
            PayStub payStub,
            List<AdditionalOtherMoneyReceived> additionalMoneyReceived,
            List<IncomeTax> incomeTaxes)
        {
            SetupPayStub(payStub);

            var incomeTaxTypes = new List<string>
                {
                    "Social Security",
                    "Medicare",
                    "Federal Tax",
                    "State Tax",
                    "FUTA",
                    "SUTA",
                    "Additional Medicare"
                };

            incomeTaxes.Clear();
            foreach (var name in incomeTaxTypes)
            {
                var incomeTax = _fixture.Build<IncomeTax>()
                    .With(t => t.PayStub, payStub)
                    .With(t => t.IncomeTaxType, _fixture.Build<IncomeTaxType>()
                        .With(x => x.Name, name)
                        .Create())
                    .Create();
                incomeTaxes.Add(incomeTax);
            }

            var result = await _documentManager.CreatePayStubPdfAsync(payStub, additionalMoneyReceived, incomeTaxes);

            Assert.NotEqual(Guid.Empty, result);
        }

        [Theory, CustomAutoData]
        public async Task CreatePayStubPdfAsync_ShouldHandleMinimumValidData(
            PayStub payStub)
        {
            SetupPayStub(payStub);

            var incomeTaxTypes = new List<string>
                {
                    "Social Security",
                    "Medicare",
                    "Federal Tax",
                    "State Tax",
                    "FUTA",
                    "SUTA",
                    "Additional Medicare"
                };

            var incomeTaxes = new List<IncomeTax>();
            foreach (var name in incomeTaxTypes)
            {
                var incomeTax = _fixture.Build<IncomeTax>()
                    .With(t => t.PayStub, payStub)
                    .With(t => t.IncomeTaxType, _fixture.Build<IncomeTaxType>()
                        .With(x => x.Name, name)
                        .Create())
                    .Create();
                incomeTaxes.Add(incomeTax);
            }

            var result = await _documentManager.CreatePayStubPdfAsync(payStub, new List<AdditionalOtherMoneyReceived>(), incomeTaxes);

            Assert.NotEqual(Guid.Empty, result);
        }

        [Theory, CustomAutoData]
        public async Task CreatePayStubPdfAsync_ShouldThrowAppExceptionWhenPayStubIsNull(
              OtherMoneyReceived otherReceived,
              List<AdditionalOtherMoneyReceived> additionalMoneyReceived,
              List<IncomeTax> incomeTaxes)
        {
            var exception = await Assert.ThrowsAsync<AppException>(() =>
                _documentManager.CreatePayStubPdfAsync(null, additionalMoneyReceived, incomeTaxes));

            Assert.Equal("PayStub not found", exception.Message);
        }

        [Theory, CustomAutoData]
        public async Task CreatePayStubPdfAsync_ShouldHandleEmptyIncomeTaxes(
            PayStub payStub,
            List<AdditionalOtherMoneyReceived> additionalMoneyReceived)
        {
            SetupPayStub(payStub);

            var result = await _documentManager.CreatePayStubPdfAsync(payStub, additionalMoneyReceived, new List<IncomeTax>());

            Assert.NotEqual(Guid.Empty, result);
        }

        [Theory, CustomAutoData]
        public async Task CreatePayStubPdfAsync_ShouldHandleAllOptionalParametersNull(
            PayStub payStub)
        {
            SetupPayStub(payStub);

            var result = await _documentManager.CreatePayStubPdfAsync(payStub, null, null);

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
