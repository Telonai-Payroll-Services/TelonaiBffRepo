using AutoMapper;
using Castle.Components.DictionaryAdapter.Xml;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TelonaiWebApi.Controllers;
using TelonaiWebApi.Entities;
using TelonaiWebApi.Helpers;
using TelonaiWebApi.Models;
using TelonaiWebApi.Services;
using Xunit;

namespace TelonaiWebAPI.UnitTest.Controllers
{
    public class OtherMoneyReceivedControllerTest
    {
        private readonly Mock<IScopedAuthorization> _mockScopedAuthorization;
        private readonly Mock<IOtherMoneyReceivedService> _mockOtherMoneyReceivedService;
        private readonly OtherIncomeController _otherMoneyReceivedController;
        private readonly Mock<HttpContext> _mockHttpContext;
        private readonly Mock<IMapper> _mockMapper;
        public ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal();
        public DefaultHttpContext context = new DefaultHttpContext();
        public OtherMoneyReceivedControllerTest()
        {
            _mockMapper = new Mock<IMapper>();
            _mockHttpContext = new Mock<HttpContext>();
            _mockScopedAuthorization = new  Mock<IScopedAuthorization>();
            _mockOtherMoneyReceivedService = new Mock<IOtherMoneyReceivedService>();
            _otherMoneyReceivedController = new OtherIncomeController(_mockOtherMoneyReceivedService.Object, _mockScopedAuthorization.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = _mockHttpContext.Object
                }
            };
            claimsPrincipal.AddIdentity(new ClaimsIdentity(new Claim[]
            {
                    new Claim("custom:scope", "Admin")
            }));
            context.User = claimsPrincipal;
        }

        [Fact]
        public async void GetCurrentByPayrollId_ProvidingCorrectOtherIncomeForPayStub_ReturnsPaystubWithOtherIncome()
        {
            var otherIncomeId = 12;
            var companyId = 1;
            var payroll = new Payroll()
            {
                Id = 112,
                CompanyId = companyId,
                PayrollScheduleId = 2,
                ScheduledRunDate = DateOnly.FromDateTime(DateTime.Now),
                StartDate = DateOnly.FromDateTime(DateTime.Now),
                TrueRunDate = DateTime.Now
            };
            var otherRecivedMoneyList = new List<OtherMoneyReceived>()
            {
                new OtherMoneyReceived()
                {
                    Id = otherIncomeId,
                    IsCancelled = false,
                    CreditCardTips = 0,
                    YtdCreditCardTips = 32,
                    AdditionalOtherMoneyReceivedId = new[] { 12, 67, 300 },
                    CashTips = 200,
                    YtdCashTips = 200,
                    Reimbursement = 100,
                    YtdReimbursement = 100,
                }
            }.AsQueryable();

            var payStub = new PayStub()
            {
                Id = 1,
                PayrollId = 12,
                EmploymentId = 13,
                OtherMoneyReceivedId = otherIncomeId,
                RegularHoursWorked = 160,
                OverTimeHoursWorked = 64,
                GrossPay = 2500,
                RegularPay = 2250,
                AmountSubjectToAdditionalMedicareTax = 120,
                YtdRegularHoursWorked = 1920,
                YtdOverTimeHoursWorked = 768,
                YtdGrossPay = 27000,
                YtdNetPay = 20500,
                YtdOverTimePay = 0,
                YtdRegularPay = 20500,
                IsCancelled = false,
                Payroll = payroll,
            };
         
            var otherIncomeModelList  = new List<OtherMoneyReceivedModel>()
            {
                new OtherMoneyReceivedModel()
                {
                    Id = 1,
                    IsCancelled = false,
                    CreditCardTips = 0,
                    YtdCreditCardTips = 32,
                    AdditionalOtherMoneyReceivedId = new[] { 12, 67, 300 },
                    CashTips = 200,
                    YtdCashTips = 200,
                    Reimbursement = 100,
                    YtdReimbursement = 100,
                }
            };

            _mockHttpContext.Setup(i => i.Request.HttpContext.User).Returns(claimsPrincipal);
            _mockScopedAuthorization.Setup(x => x.ValidateByCompanyId(It.IsAny<ClaimsPrincipal>(), AuthorizationType.Admin, companyId)).Callback(() => { });
            _mockOtherMoneyReceivedService.Setup(service => service.GetByPayStubId(payStub.Id, out companyId)).Returns(otherIncomeModelList);
            

            //Act 
            var result = _otherMoneyReceivedController.GetCurrentByPayrollId(1);

            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async void GetCurrentByPayrollId_ProvidingNotExistingPayStub_ReturnsNullOtherIncome()
        {
            var otherIncomeId = 12;
            var companyId = 1;
            var payroll = new Payroll()
            {
                Id = 112,
                CompanyId = companyId,
                PayrollScheduleId = 2,
                ScheduledRunDate = DateOnly.FromDateTime(DateTime.Now),
                StartDate = DateOnly.FromDateTime(DateTime.Now),
                TrueRunDate = DateTime.Now
            };
            var otherRecivedMoneyList = new List<OtherMoneyReceived>()
            {
                new OtherMoneyReceived()
                {
                    Id = otherIncomeId,
                    IsCancelled = false,
                    CreditCardTips = 0,
                    YtdCreditCardTips = 32,
                    AdditionalOtherMoneyReceivedId = new[] { 12, 67, 300 },
                    CashTips = 200,
                    YtdCashTips = 200,
                    Reimbursement = 100,
                    YtdReimbursement = 100,
                }
            }.AsQueryable();

            var payStub = new PayStub()
            {
                Id = 1,
                PayrollId = 12,
                EmploymentId = 13,
                OtherMoneyReceivedId = otherIncomeId,
                RegularHoursWorked = 160,
                OverTimeHoursWorked = 64,
                GrossPay = 2500,
                RegularPay = 2250,
                AmountSubjectToAdditionalMedicareTax = 120,
                YtdRegularHoursWorked = 1920,
                YtdOverTimeHoursWorked = 768,
                YtdGrossPay = 27000,
                YtdNetPay = 20500,
                YtdOverTimePay = 0,
                YtdRegularPay = 20500,
                IsCancelled = false,
                Payroll = payroll,
            };

            var otherRecivedMoneyModelList = new List<OtherMoneyReceivedModel>()
            {
                new OtherMoneyReceivedModel()
                {
                    Id = 1,
                    IsCancelled = false,
                    CreditCardTips = 0,
                    YtdCreditCardTips = 32,
                    AdditionalOtherMoneyReceivedId = new[] { 12, 67, 300 },
                    CashTips = 200,
                    YtdCashTips = 200,
                    Reimbursement = 100,
                    YtdReimbursement = 100,
                    AdditionalOtherMoneyReceived = new List<AdditionalOtherMoneyReceived>()
                    {
                        It.IsAny<AdditionalOtherMoneyReceived>()
                    }
                }
            };

            var otherIncomeModelList = new List<OtherMoneyReceivedModel>()
            {
                new OtherMoneyReceivedModel()
                {
                    Id = 1,
                    IsCancelled = false,
                    CreditCardTips = 0,
                    YtdCreditCardTips = 32,
                    AdditionalOtherMoneyReceivedId = new[] { 12, 67, 300 },
                    CashTips = 200,
                    YtdCashTips = 200,
                    Reimbursement = 100,
                    YtdReimbursement = 100,
                }
            };

            _mockHttpContext.Setup(i => i.Request.HttpContext.User).Returns(claimsPrincipal);
            _mockScopedAuthorization.Setup(x => x.ValidateByCompanyId(It.IsAny<ClaimsPrincipal>(), AuthorizationType.Admin, companyId)).Callback(() => { });
            _mockOtherMoneyReceivedService.Setup(service => service.GetByPayStubId(payStub.Id, out companyId)).Returns(otherIncomeModelList);


            //Act 
            var result = _otherMoneyReceivedController.GetCurrentByPayrollId(112);

            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Null(okResult.Value);
        }

        [Fact]
        public async void GetOtherIncomeReceivedById_WhenProvidingExistingOtheringIncomeId_ReturnsHttpOkResult()
        {
            var otherIncomeId = 12;

            var otherRecivedMoney = new OtherMoneyReceived()
            {
                Id = otherIncomeId,
                IsCancelled = false,
                CreditCardTips = 0,
                YtdCreditCardTips = 32,
                AdditionalOtherMoneyReceivedId = new[] { 12, 67, 300 },
                CashTips = 200,
                YtdCashTips = 200,
                Reimbursement = 100,
                YtdReimbursement = 100,
            };
            
            var otherIncomeModel =  new OtherMoneyReceivedModel()
            {
                Id = 1,
                IsCancelled = false,
                CreditCardTips = 0,
                YtdCreditCardTips = 32,
                AdditionalOtherMoneyReceivedId = new[] { 12, 67, 300 },
                CashTips = 200,
                YtdCashTips = 200,
                Reimbursement = 100,
                YtdReimbursement = 100,
            };

            _mockOtherMoneyReceivedService.Setup(service => service.GetById(otherIncomeId)).Returns(otherIncomeModel);

            //Act 
            var result = _otherMoneyReceivedController.GetById(otherIncomeId);

            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
            var otherIncomeReceived = (OtherMoneyReceivedModel)okResult.Value;
            Assert.Equal(otherIncomeModel.Id, otherIncomeReceived.Id);
        }
       
        [Fact]
        public async void GetOtherIncomeReceivedById_WhenProvidingNonExistingOtheringIncomeId_ReturnsHttpNotFoundResult()
        {
            var otherIncomeId = 12;

            var otherRecivedMoneyList = new OtherMoneyReceived()
            {
                Id = otherIncomeId,
                IsCancelled = false,
                CreditCardTips = 0,
                YtdCreditCardTips = 32,
                AdditionalOtherMoneyReceivedId = new[] { 12, 67, 300 },
                CashTips = 200,
                YtdCashTips = 200,
                Reimbursement = 100,
                YtdReimbursement = 100,
            };

            var otherIncomeModel = new OtherMoneyReceivedModel()
            {
                Id = 1,
                IsCancelled = false,
                CreditCardTips = 0,
                YtdCreditCardTips = 32,
                AdditionalOtherMoneyReceivedId = new[] { 12, 67, 300 },
                CashTips = 200,
                YtdCashTips = 200,
                Reimbursement = 100,
                YtdReimbursement = 100,
            };

            _mockOtherMoneyReceivedService.Setup(service => service.GetById(otherIncomeId)).Returns(otherIncomeModel);

            //Act 
            var result = _otherMoneyReceivedController.GetById(122);

            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Null(okResult.Value);
        }

        [Fact]
        public async void UpdateOtherIncomeReceived_WhenPassingExistingOtherIncomeUpdatedInfo_ReturnsOkResult()
        {
            var otherIncomeId = 12;
            var companyId = 1;
            var payroll = new Payroll()
            {
                Id = 112,
                CompanyId = companyId,
                PayrollScheduleId = 2,
                ScheduledRunDate = DateOnly.FromDateTime(DateTime.Now),
                StartDate = DateOnly.FromDateTime(DateTime.Now),
                TrueRunDate = DateTime.Now
            };

            var otherRecivedMoneyList = new OtherMoneyReceived()
            {
                Id = otherIncomeId,
                IsCancelled = false,
                CreditCardTips = 0,
                YtdCreditCardTips = 32,
                AdditionalOtherMoneyReceivedId = new[] { 12, 67, 300 },
                CashTips = 200,
                YtdCashTips = 200,
                Reimbursement = 100,
                YtdReimbursement = 100,
            };

            var payStub = new PayStub()
            {
                Id = 1,
                PayrollId = 12,
                EmploymentId = 13,
                OtherMoneyReceivedId = otherIncomeId,
                RegularHoursWorked = 160,
                OverTimeHoursWorked = 64,
                GrossPay = 2500,
                RegularPay = 2250,
                AmountSubjectToAdditionalMedicareTax = 120,
                YtdRegularHoursWorked = 1920,
                YtdOverTimeHoursWorked = 768,
                YtdGrossPay = 27000,
                YtdNetPay = 20500,
                YtdOverTimePay = 0,
                YtdRegularPay = 20500,
                IsCancelled = false,
                Payroll = payroll,
            };

            var otherRecivedMoneyModel =  new OtherMoneyReceivedModel()
            {
                Id = 1,
                IsCancelled = false,
                CreditCardTips = 0,
                YtdCreditCardTips = 32,
                AdditionalOtherMoneyReceivedId = new[] { 12, 67, 300 },
                CashTips = 200,
                YtdCashTips = 200,
                Reimbursement = 100,
                YtdReimbursement = 100,
                AdditionalOtherMoneyReceived = new List<AdditionalOtherMoneyReceived>()
                {
                    It.IsAny<AdditionalOtherMoneyReceived>()
                }
            };

            _mockOtherMoneyReceivedService.Setup(service => service.CreateOrUpdate(payStub.Id, otherRecivedMoneyModel)).ReturnsAsync(true);

            //Act
            var result = await _otherMoneyReceivedController.Update(payStub.Id, otherRecivedMoneyModel);

            //Assert
            Assert.IsType<OkObjectResult>(result); // Assert the result of SaveDataAsync
        }

        public async void UpdateOtherIncomeReceived_WhenPassingNonExistingOtherIncomeUpdatedInfo_ReturnsNotFoundResult()
        {
            var otherIncomeId = 12;
            var companyId = 1;
            var payroll = new Payroll()
            {
                Id = 112,
                CompanyId = companyId,
                PayrollScheduleId = 2,
                ScheduledRunDate = DateOnly.FromDateTime(DateTime.Now),
                StartDate = DateOnly.FromDateTime(DateTime.Now),
                TrueRunDate = DateTime.Now
            };

            var otherRecivedMoneyList = new OtherMoneyReceived()
            {
                Id = otherIncomeId,
                IsCancelled = false,
                CreditCardTips = 0,
                YtdCreditCardTips = 32,
                AdditionalOtherMoneyReceivedId = new[] { 12, 67, 300 },
                CashTips = 200,
                YtdCashTips = 200,
                Reimbursement = 100,
                YtdReimbursement = 100,
            };

            var payStub = new PayStub()
            {
                Id = 1,
                PayrollId = 12,
                EmploymentId = 13,
                OtherMoneyReceivedId = otherIncomeId,
                RegularHoursWorked = 160,
                OverTimeHoursWorked = 64,
                GrossPay = 2500,
                RegularPay = 2250,
                AmountSubjectToAdditionalMedicareTax = 120,
                YtdRegularHoursWorked = 1920,
                YtdOverTimeHoursWorked = 768,
                YtdGrossPay = 27000,
                YtdNetPay = 20500,
                YtdOverTimePay = 0,
                YtdRegularPay = 20500,
                IsCancelled = false,
                Payroll = payroll,
            };

            var otherRecivedMoneyModel = new OtherMoneyReceivedModel()
            {
                Id = 1,
                IsCancelled = false,
                CreditCardTips = 0,
                YtdCreditCardTips = 32,
                AdditionalOtherMoneyReceivedId = new[] { 12, 67, 300 },
                CashTips = 200,
                YtdCashTips = 200,
                Reimbursement = 100,
                YtdReimbursement = 100,
                AdditionalOtherMoneyReceived = new List<AdditionalOtherMoneyReceived>()
                {
                    It.IsAny<AdditionalOtherMoneyReceived>()
                }
            };

            _mockOtherMoneyReceivedService.Setup(service => service.CreateOrUpdate(12, otherRecivedMoneyModel)).ReturnsAsync(false);

            //Act
            var result = await _otherMoneyReceivedController.Update(12, otherRecivedMoneyModel);

            //Assert
            Assert.IsType<NotFoundObjectResult>(result); // Assert the result of SaveDataAsync
            var response = (NotFoundObjectResult)result;
            Assert.Equal(response.Value, "Not able to update other income.");

        }


        [Fact]
        public async void DeleteOtherIncomeReceived_WhenProvidingExisitingOtherIncomeId_OtherIncomeDeleted()
        {
            var otherIncomeId = 12;
            var otherRecivedMoneyModel = new OtherMoneyReceivedModel()
            {
                Id = otherIncomeId,
                IsCancelled = false,
                CreditCardTips = 0,
                YtdCreditCardTips = 32,
                AdditionalOtherMoneyReceivedId = new[] { 12, 67, 300 },
                CashTips = 200,
                YtdCashTips = 200,
                Reimbursement = 100,
                YtdReimbursement = 100,
                AdditionalOtherMoneyReceived = new List<AdditionalOtherMoneyReceived>()
                {
                    It.IsAny<AdditionalOtherMoneyReceived>()
                }
            };

            _mockOtherMoneyReceivedService.Setup(service => service.Delete(12)).ReturnsAsync(true);
            //Act
            var result = await _otherMoneyReceivedController.Delete(12);

            //Assert
            Assert.IsType<OkObjectResult>(result);
            _mockOtherMoneyReceivedService.Verify(service => service.Delete(12), Times.Once);
        }

        [Fact]
        public async void DeleteOtherIncomeReceived_WhenProvidingNonExisitingOtherIncomeId_OtherIncomeWillNotDeleted()
        {
            var otherIncomeId = 12;
            

            var otherRecivedMoney=  new OtherMoneyReceived()
            {
                Id = 1,
                IsCancelled = false,
                CreditCardTips = 0,
                YtdCreditCardTips = 32,
                AdditionalOtherMoneyReceivedId = new[] { 12, 67, 300 },
                CashTips = 200,
                YtdCashTips = 200,
                Reimbursement = 100,
                YtdReimbursement = 100,
            };

            _mockOtherMoneyReceivedService.Setup(service => service.Delete(12)).ReturnsAsync(false);


            //Act
            var result = await _otherMoneyReceivedController.Delete(12);

            //Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async void CreateOtherIncomeReceived_SaveTheOtherIncomeInPayStub_ReturnsOkResult()
        {
            var otherIncomeId = 12;
            var companyId = 1;
            var payroll = new Payroll()
            {
                Id = 112,
                CompanyId = companyId,
                PayrollScheduleId = 2,
                ScheduledRunDate = DateOnly.FromDateTime(DateTime.Now),
                StartDate = DateOnly.FromDateTime(DateTime.Now),
                TrueRunDate = DateTime.Now
            };

            var otherRecivedMoneyModel = new OtherMoneyReceivedModel()
            {
                Id = otherIncomeId,
                IsCancelled = false,
                CreditCardTips = 0,
                YtdCreditCardTips = 32,
                AdditionalOtherMoneyReceivedId = new[] { 12, 67, 300 },
                CashTips = 200,
                YtdCashTips = 200,
                Reimbursement = 100,
                YtdReimbursement = 100,
            };

            var payStub = new PayStub()
            {
                Id = 1,
                PayrollId = 12,
                EmploymentId = 13,
                OtherMoneyReceivedId = otherIncomeId,
                RegularHoursWorked = 160,
                OverTimeHoursWorked = 64,
                GrossPay = 2500,
                RegularPay = 2250,
                AmountSubjectToAdditionalMedicareTax = 120,
                YtdRegularHoursWorked = 1920,
                YtdOverTimeHoursWorked = 768,
                YtdGrossPay = 27000,
                YtdNetPay = 20500,
                YtdOverTimePay = 0,
                YtdRegularPay = 20500,
                IsCancelled = false,
                Payroll = payroll,
            };

            _mockOtherMoneyReceivedService.Setup(service => service.CreateOrUpdate(payStub.Id, otherRecivedMoneyModel)).ReturnsAsync(true);

            //Act 
            var result = await _otherMoneyReceivedController.Create(1, otherRecivedMoneyModel);

            //Assert
            Assert.IsType<OkObjectResult>(result); // Assert the result of SaveDataAsync
            var response = (OkObjectResult)result;
            Assert.Equal(response.Value, "Other income for the provide paystub is registered successfully.");
        }

        [Fact]
        public async void CreateOtherIncomeReceived_SaveTheOtherIncomeWithNonExistingPayStub_ReturnsNotFoundResult()
        {
            var otherIncomeId = 12;
            var companyId = 1;
            var payroll = new Payroll()
            {
                Id = 112,
                CompanyId = companyId,
                PayrollScheduleId = 2,
                ScheduledRunDate = DateOnly.FromDateTime(DateTime.Now),
                StartDate = DateOnly.FromDateTime(DateTime.Now),
                TrueRunDate = DateTime.Now
            };

            var otherRecivedMoneyModel = new OtherMoneyReceivedModel()
            {
                Id = otherIncomeId,
                IsCancelled = false,
                CreditCardTips = 0,
                YtdCreditCardTips = 32,
                AdditionalOtherMoneyReceivedId = new[] { 12, 67, 300 },
                CashTips = 200,
                YtdCashTips = 200,
                Reimbursement = 100,
                YtdReimbursement = 100,
            };

            var payStub = new PayStub()
            {
                Id = 1,
                PayrollId = 12,
                EmploymentId = 13,
                OtherMoneyReceivedId = otherIncomeId,
                RegularHoursWorked = 160,
                OverTimeHoursWorked = 64,
                GrossPay = 2500,
                RegularPay = 2250,
                AmountSubjectToAdditionalMedicareTax = 120,
                YtdRegularHoursWorked = 1920,
                YtdOverTimeHoursWorked = 768,
                YtdGrossPay = 27000,
                YtdNetPay = 20500,
                YtdOverTimePay = 0,
                YtdRegularPay = 20500,
                IsCancelled = false,
                Payroll = payroll,
            };

            _mockOtherMoneyReceivedService.Setup(service => service.CreateOrUpdate(51, otherRecivedMoneyModel)).ReturnsAsync(false);

            //Act 
            var result = await _otherMoneyReceivedController.Create(51, otherRecivedMoneyModel);

            //Assert
            Assert.IsType<NotFoundObjectResult>(result); // Assert the result of SaveDataAsync
            var response = (NotFoundObjectResult)result;
            Assert.Equal(response.Value, "There is not paystub registered with provided payStubId.");
        }
    }
}
