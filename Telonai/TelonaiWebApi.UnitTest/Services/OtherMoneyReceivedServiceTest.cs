using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.ConstrainedExecution;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using TelonaiWebApi.Entities;
using TelonaiWebApi.Helpers;
using TelonaiWebApi.Models;
using TelonaiWebApi.Services;
using Xunit;

namespace TelonaiWebAPI.UnitTest.Services
{
    public class OtherMoneyReceivedServiceTest
    {
        private readonly Mock<DataContext> _mockDataContext;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private readonly Mock<IScopedAuthorization> _mockScopedAuthorization;
        private readonly Mock<IOtherMoneyReceivedService> _mockOtherMoneyReceivedService;
        private readonly OtherMoneyReceivedService _OtherMoneyReceivedService;

        public OtherMoneyReceivedServiceTest()
        {
            _mockDataContext = new Mock<DataContext>();
            _mockMapper = new Mock<IMapper>();
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            _mockScopedAuthorization = new Mock<IScopedAuthorization>();
            _mockOtherMoneyReceivedService = new Mock<IOtherMoneyReceivedService>();
            _OtherMoneyReceivedService = new OtherMoneyReceivedService(_mockDataContext.Object, _mockMapper.Object, _mockHttpContextAccessor.Object, _mockScopedAuthorization.Object);
        }

        #region Get Other Money Received By Id

        [Fact]
        public async void GetById_whenOtherIncomeExistWithIdProvided_ReturnsOtherIncomeDetails()
        {
            int otherIncomeId = 1;
            var otherIncome = new OtherMoneyReceived()
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
            var otherIncomeList = new List<OtherMoneyReceived> {
                otherIncome,
            }.AsQueryable();

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
                AdditionalOtherMoneyReceived = new List<AdditionalOtherMoneyReceived>()
                {
                    It.IsAny<AdditionalOtherMoneyReceived>()
                }
            };

            var mockSet = new Mock<DbSet<OtherMoneyReceived>>();
            mockSet.As<IQueryable<OtherMoneyReceived>>().Setup(m => m.Provider).Returns(otherIncomeList.Provider);
            mockSet.As<IQueryable<OtherMoneyReceived>>().Setup(m => m.Expression).Returns(otherIncomeList.Expression);
            mockSet.As<IQueryable<OtherMoneyReceived>>().Setup(m => m.ElementType).Returns(otherIncomeList.ElementType);
            mockSet.As<IQueryable<OtherMoneyReceived>>().Setup(m => m.GetEnumerator()).Returns(otherIncomeList.GetEnumerator());
            
            _mockMapper.Setup(m => m.Map<OtherMoneyReceivedModel>(otherIncome)).Returns(otherIncomeModel);
            _mockDataContext.Setup(c => c.OtherMoneyReceived).Returns(mockSet.Object);
            _mockDataContext.Setup(o => o.OtherMoneyReceived.Find(otherIncome.Id)).Returns(otherIncome);
                       
            // Act
            var result = _OtherMoneyReceivedService.GetById(otherIncomeId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(otherIncomeId, result.Id);
        }

        [Fact]
        public async void GetById_WhenSendingNonExistingIdAsParameter_ReturnsNull()
        {
            int otherIncomeId = 1;
            var otherIncome = new OtherMoneyReceived()
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
            var otherIncomeList = new List<OtherMoneyReceived> {
                otherIncome,
            }.AsQueryable();

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
                //AdditionalOtherMoneyReceived = new List<AdditionalOtherMoneyReceived>()
                //{
                //    It.IsAny<AdditionalOtherMoneyReceived>()
                //}
            };

            var mockSet = new Mock<DbSet<OtherMoneyReceived>>();
            mockSet.As<IQueryable<OtherMoneyReceived>>().Setup(m => m.Provider).Returns(otherIncomeList.Provider);
            mockSet.As<IQueryable<OtherMoneyReceived>>().Setup(m => m.Expression).Returns(otherIncomeList.Expression);
            mockSet.As<IQueryable<OtherMoneyReceived>>().Setup(m => m.ElementType).Returns(otherIncomeList.ElementType);
            mockSet.As<IQueryable<OtherMoneyReceived>>().Setup(m => m.GetEnumerator()).Returns(otherIncomeList.GetEnumerator());

            _mockDataContext.Setup(c => c.OtherMoneyReceived).Returns(mockSet.Object);
            _mockMapper.Setup(m => m.Map<OtherMoneyReceivedModel>(It.IsAny<OtherMoneyReceived>())).Returns(otherIncomeModel);

            // Act
            var result = _OtherMoneyReceivedService.GetById(12);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async void GetByPayStubId_WhenPassingCorrectCompanyIdAndPaysStubId_ReturnOtherIncomeDetail()
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

            var otherReceivedMoney = new OtherMoneyReceived()
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
            var otherRecivedMoneyList = new List<OtherMoneyReceived>()
            {
               otherReceivedMoney
            }.AsQueryable();

            var payStubs = new List<PayStub>()
            {
                new PayStub()
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
                    OtherMoneyReceived = otherReceivedMoney,
                },
                new PayStub()
                {
                    Id = 2,
                    PayrollId = 12,
                    EmploymentId = 14,
                    OtherMoneyReceivedId = 25,
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
                    OtherMoneyReceived = otherReceivedMoney,
                },
                new PayStub()
                {
                    Id = 3,
                    PayrollId = 12,
                    EmploymentId = 15,
                    OtherMoneyReceivedId = 25,
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
                    OtherMoneyReceived = otherReceivedMoney,
                },
            }.AsQueryable();
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
            var mockSet = new Mock<DbSet<OtherMoneyReceived>>();
            mockSet.As<IQueryable<OtherMoneyReceived>>().Setup(m => m.Provider).Returns(otherRecivedMoneyList.Provider);
            mockSet.As<IQueryable<OtherMoneyReceived>>().Setup(m => m.Expression).Returns(otherRecivedMoneyList.Expression);
            mockSet.As<IQueryable<OtherMoneyReceived>>().Setup(m => m.ElementType).Returns(otherRecivedMoneyList.ElementType);
            mockSet.As<IQueryable<OtherMoneyReceived>>().Setup(m => m.GetEnumerator()).Returns(otherRecivedMoneyList.GetEnumerator());
            _mockMapper.Setup(m => m.Map<List<OtherMoneyReceivedModel>>(It.IsAny<List<OtherMoneyReceived>>())).Returns(otherRecivedMoneyModelList);
            _mockDataContext.Setup(c => c.OtherMoneyReceived).Returns(mockSet.Object);

            //Act
            int companyid = 0;
            var test = _OtherMoneyReceivedService.GetByPayStubId(1, out companyid);

            //Assert
            Assert.NotNull(test);   
        }

        #endregion
        
        #region Delete Other Money Received

        [Fact]
        public async void DeleteOtherIncome_WhenPassingExistingId_ReturnsOtherIncomeListWithOutDeletedIncome()
        {
            var otherIncomeList = new List<OtherMoneyReceived>()
            {
                new OtherMoneyReceived()
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
                },
                new OtherMoneyReceived()
                {
                    Id = 2,
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
            var mockSet = new Mock<DbSet<OtherMoneyReceived>>();
            mockSet.As<IQueryable<OtherMoneyReceived>>().Setup(m => m.Provider).Returns(otherIncomeList.Provider);
            mockSet.As<IQueryable<OtherMoneyReceived>>().Setup(m => m.Expression).Returns(otherIncomeList.Expression);
            mockSet.As<IQueryable<OtherMoneyReceived>>().Setup(m => m.ElementType).Returns(otherIncomeList.ElementType);
            mockSet.As<IQueryable<OtherMoneyReceived>>().Setup(m => m.GetEnumerator()).Returns(otherIncomeList.GetEnumerator());
            _mockDataContext.Setup(c => c.OtherMoneyReceived).Returns(mockSet.Object);
            _mockDataContext.Setup(c => c.OtherMoneyReceived.Find(1)).Returns(otherIncomeList.First());

            //Act
            var result = await _OtherMoneyReceivedService.Delete(1);


            //Assert
            Assert.True(result);
        }
        [Fact]
        public async void DeleteOtherIncome_WhenPassingExistingId_ReturnsAllOtherIncomeLists()
        {
            var otherIncomeList = new List<OtherMoneyReceived>()
            {
                new OtherMoneyReceived()
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
                },
                new OtherMoneyReceived()
                {
                    Id = 2,
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
            var mockSet = new Mock<DbSet<OtherMoneyReceived>>();
            mockSet.As<IQueryable<OtherMoneyReceived>>().Setup(m => m.Provider).Returns(otherIncomeList.Provider);
            mockSet.As<IQueryable<OtherMoneyReceived>>().Setup(m => m.Expression).Returns(otherIncomeList.Expression);
            mockSet.As<IQueryable<OtherMoneyReceived>>().Setup(m => m.ElementType).Returns(otherIncomeList.ElementType);
            mockSet.As<IQueryable<OtherMoneyReceived>>().Setup(m => m.GetEnumerator()).Returns(otherIncomeList.GetEnumerator());
            _mockDataContext.Setup(c => c.OtherMoneyReceived).Returns(mockSet.Object);
            _mockDataContext.Setup(c => c.OtherMoneyReceived.Find(12)).Returns((OtherMoneyReceived)null);

            //Act
            _OtherMoneyReceivedService.Delete(12);


            //Assert
            Assert.Equal(otherIncomeList.Count(), 2);
        }

        #endregion
    }
}
