using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TelonaiWebApi.Entities;
using TelonaiWebApi.Services;
using TelonaiWebApi.Models;
using TelonaiWebApi.Helpers;
using Moq;
using Xunit;

namespace TelonaiWebAPI.UnitTest.Services
{
    public class PayStubServiceTests
    {
        private readonly Mock<DataContext> _mockDataContext;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IStaticDataService> _mockIStaticDataService;
        private readonly PayStubService _mockPayStubService;
        public PayStubServiceTests()
        {
            _mockDataContext = new Mock<DataContext>();
            _mockMapper = new Mock<IMapper>();
            _mockIStaticDataService = new Mock<IStaticDataService>();
            _mockPayStubService = new PayStubService(_mockDataContext.Object, _mockMapper.Object, _mockIStaticDataService.Object);
        }

        #region GetCurrentByCompanyId

        [Fact]
        public async void GetCurrentByCompanyId_WhenPayStubExistForCompany_ReturnsTheFirstPayStubForCompany()
        {
            //Arrange
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

            var payStubs = new List<PayStub>()
            {
                new PayStub()
                {
                    Id = 1,
                    PayrollId = 12,
                    EmploymentId = 13,
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
                },
            }.AsQueryable();

            var mockSet = new Mock<DbSet<PayStub>>();
            mockSet.As<IQueryable<PayStub>>().Setup(m => m.Provider).Returns(payStubs.Provider);
            mockSet.As<IQueryable<PayStub>>().Setup(m => m.Expression).Returns(payStubs.Expression);
            mockSet.As<IQueryable<PayStub>>().Setup(m => m.ElementType).Returns(payStubs.ElementType);
            mockSet.As<IQueryable<PayStub>>().Setup(m => m.GetEnumerator()).Returns(payStubs.GetEnumerator());


            var payStubModel = new List<PayStubModel>()
            {
                new PayStubModel()
                {
                    Id = 1,
                    PayrollId = 12,
                    EmploymentId = 13,
                    OtherMoneyReceivedId = 25,
                    RegularHoursWorked = 160,
                    OverTimeHoursWorked = 64,
                    GrossPay = 2500,
                    RegularPay = 2250
                }
            };
            _mockMapper.Setup(m => m.Map<List<PayStubModel>>(It.IsAny<List<PayStub>>())).Returns(payStubModel);
            _mockDataContext.Setup(c => c.PayStub).Returns(mockSet.Object);

            //Act 
            var result = _mockPayStubService.GetCurrentByCompanyId(companyId);

            //Assert 
            Assert.Equal(payStubModel.First().Id, result.First().Id);
        }

        [Fact]
        public async void GetCurrentByCompanyId_WhenPayStubNonExistForCompany_ReturnsNull()
        {
            //Arrange
            var companyId = 3;
            var payroll = new Payroll()
            {
                Id = 112,
                CompanyId = companyId,
                PayrollScheduleId = 2,
                ScheduledRunDate = DateOnly.FromDateTime(DateTime.Now),
                StartDate = DateOnly.FromDateTime(DateTime.Now),
                TrueRunDate = DateTime.Now
            };

            var payStubs = new List<PayStub>()
            {
                new PayStub()
                {
                    Id = 1,
                    PayrollId = 12,
                    EmploymentId = 13,
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
                },
            }.AsQueryable();
            var mockSet = new Mock<DbSet<PayStub>>();
            mockSet.As<IQueryable<PayStub>>().Setup(m => m.Provider).Returns(payStubs.Provider);
            mockSet.As<IQueryable<PayStub>>().Setup(m => m.Expression).Returns(payStubs.Expression);
            mockSet.As<IQueryable<PayStub>>().Setup(m => m.ElementType).Returns(payStubs.ElementType);
            mockSet.As<IQueryable<PayStub>>().Setup(m => m.GetEnumerator()).Returns(payStubs.GetEnumerator());

            var payStubModel = new List<PayStubModel>();
            _mockMapper.Setup(m => m.Map<List<PayStubModel>>(It.IsAny<List<PayStub>>())).Returns(payStubModel);
            _mockDataContext.Setup(c => c.PayStub).Returns(mockSet.Object);

            //Act 
            var result = _mockPayStubService.GetCurrentByCompanyId(1);

            //Assert 
            Assert.Equal(0, result.Count());
        }

        #endregion

        #region GetCurrentByCompanyIdAndPersonId

        [Fact]
        public async void GetCurrentByCompanyIdAndPersonId_WhenPayStubContainsJobIdandCompanyId_ReturnsTheFirstPayStub()
        {
            var companyId = 1;
            var presonId = 21;
            var jobId = 20;

            var payroll = new Payroll()
            {
                Id = 112,
                CompanyId = companyId,
                PayrollScheduleId = 2,
                ScheduledRunDate = DateOnly.FromDateTime(DateTime.Now),
                StartDate = DateOnly.FromDateTime(DateTime.Now),
                TrueRunDate = DateTime.Now
            };

            var employment = new Employment()
            {
                Id = 20,
                PersonId = presonId,
                JobId = jobId,
                PayRateBasisId = 11,
                PayRate = 90,
                IsPayrollAdmin = false,
                IsSalariedOvertimeExempt = false,
                IsTenNinetyNine = false,
                StartDate = DateOnly.FromDateTime(DateTime.Now),
                Job = new Job()
                {
                    Id = jobId,
                    CompanyId = companyId,
                    AddressLine1 = "Bole Getu Commercial Center",
                    AddressLine2 = "Kebede Building",
                    AddressLine3 = "Second Floor",
                    ZipcodeId = 232
                }
            };

            var payStubs = new List<PayStub>()
            {
                new PayStub()
                {
                    Id = 1,
                    PayrollId = 12,
                    EmploymentId = 13,
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
                    Employment = employment,
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
                    Employment = employment,
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
                    Employment = employment
                },
            }.AsQueryable();

            var mockSet = new Mock<DbSet<PayStub>>();
            mockSet.As<IQueryable<PayStub>>().Setup(m => m.Provider).Returns(payStubs.Provider);
            mockSet.As<IQueryable<PayStub>>().Setup(m => m.Expression).Returns(payStubs.Expression);
            mockSet.As<IQueryable<PayStub>>().Setup(m => m.ElementType).Returns(payStubs.ElementType);
            mockSet.As<IQueryable<PayStub>>().Setup(m => m.GetEnumerator()).Returns(payStubs.GetEnumerator());


            var payStubModel = new List<PayStubModel>()
            {
                new PayStubModel()
                {
                    Id = 1,
                    PayrollId = 12,
                    EmploymentId = 13,
                    OtherMoneyReceivedId = 25,
                    RegularHoursWorked = 160,
                    OverTimeHoursWorked = 64,
                    GrossPay = 2500,
                    RegularPay = 2250
                }
            };
            _mockMapper.Setup(m => m.Map<List<PayStubModel>>(It.IsAny<List<PayStub>>())).Returns(payStubModel);
            _mockDataContext.Setup(c => c.PayStub).Returns(mockSet.Object);

            //Act 
            var result = _mockPayStubService.GetCurrentByCompanyIdAndPersonId(companyId, presonId);

            //Assert 
            Assert.Equal(payStubModel.First().Id, result.First().Id);
        }
        [Fact]
        public async void GetCurrentByCompanyIdAndPersonId_WhenPayStubContainsNonExistingJobIdandCompanyId_ReturnsNull()
        {
            var companyId = 1;
            var presonId = 21;
            var jobId = 20;

            var payroll = new Payroll()
            {
                Id = 112,
                CompanyId = companyId,
                PayrollScheduleId = 2,
                ScheduledRunDate = DateOnly.FromDateTime(DateTime.Now),
                StartDate = DateOnly.FromDateTime(DateTime.Now),
                TrueRunDate = DateTime.Now
            };

            var employment = new Employment()
            {
                Id = 20,
                PersonId = presonId,
                JobId = jobId,
                PayRateBasisId = 11,
                PayRate = 90,
                IsPayrollAdmin = false,
                IsSalariedOvertimeExempt = false,
                IsTenNinetyNine = false,
                StartDate = DateOnly.FromDateTime(DateTime.Now),
                Job = new Job()
                {
                    Id = jobId,
                    CompanyId = companyId,
                    AddressLine1 = "Bole Getu Commercial Center",
                    AddressLine2 = "Kebede Building",
                    AddressLine3 = "Second Floor",
                    ZipcodeId = 232
                }
            };

            var payStubs = new List<PayStub>()
            {
                new PayStub()
                {
                    Id = 1,
                    PayrollId = 12,
                    EmploymentId = 13,
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
                    Employment = employment,
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
                    Employment = employment,
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
                    Employment = employment
                },
            }.AsQueryable();

            var mockSet = new Mock<DbSet<PayStub>>();
            mockSet.As<IQueryable<PayStub>>().Setup(m => m.Provider).Returns(payStubs.Provider);
            mockSet.As<IQueryable<PayStub>>().Setup(m => m.Expression).Returns(payStubs.Expression);
            mockSet.As<IQueryable<PayStub>>().Setup(m => m.ElementType).Returns(payStubs.ElementType);
            mockSet.As<IQueryable<PayStub>>().Setup(m => m.GetEnumerator()).Returns(payStubs.GetEnumerator());


            var payStubModel = new List<PayStubModel>();
            _mockMapper.Setup(m => m.Map<List<PayStubModel>>(It.IsAny<List<PayStub>>())).Returns(payStubModel);
            _mockDataContext.Setup(c => c.PayStub).Returns(mockSet.Object);

            //Act 
            var result = _mockPayStubService.GetCurrentByCompanyIdAndPersonId(100, 200);

            //Assert 
            Assert.Equal(0, result.Count());
        }

        #endregion

        #region GetCurrentByPayrollId
        [Fact]
        public async void GetCurrentByPayrollId_WhenProvidingExistingPayrollId_ReturnsAllListOfPayStubs()
        {
            var companyId = 1;
            var payrollId = 12;
            var payroll = new Payroll()
            {
                Id = 112,
                CompanyId = companyId,
                PayrollScheduleId = 2,
                ScheduledRunDate = DateOnly.FromDateTime(DateTime.Now),
                StartDate = DateOnly.FromDateTime(DateTime.Now),
                TrueRunDate = DateTime.Now
            };

            var payStubs = new List<PayStub>()
            {
                new PayStub()
                {
                    Id = 1,
                    PayrollId = payrollId,
                    EmploymentId = 13,
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
                },
                new PayStub()
                {
                    Id = 2,
                    PayrollId = payrollId,
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
                }
            }.AsQueryable();

            var mockSet = new Mock<DbSet<PayStub>>();
            mockSet.As<IQueryable<PayStub>>().Setup(m => m.Provider).Returns(payStubs.Provider);
            mockSet.As<IQueryable<PayStub>>().Setup(m => m.Expression).Returns(payStubs.Expression);
            mockSet.As<IQueryable<PayStub>>().Setup(m => m.ElementType).Returns(payStubs.ElementType);
            mockSet.As<IQueryable<PayStub>>().Setup(m => m.GetEnumerator()).Returns(payStubs.GetEnumerator());


            var payStubModel = new List<PayStubModel>()
            {
                new PayStubModel()
                {
                    Id = 1,
                    PayrollId = payrollId,
                    EmploymentId = 13,
                    OtherMoneyReceivedId = 25,
                    RegularHoursWorked = 160,
                    OverTimeHoursWorked = 64,
                    GrossPay = 2500,
                    RegularPay = 2250
                },
                new PayStubModel()
                {
                    Id = 2,
                    PayrollId = payrollId,
                    EmploymentId = 13,
                    OtherMoneyReceivedId = 25,
                    RegularHoursWorked = 160,
                    OverTimeHoursWorked = 64,
                    GrossPay = 2500,
                    RegularPay = 2250
                }
            };
            _mockMapper.Setup(m => m.Map<List<PayStubModel>>(It.IsAny<List<PayStub>>())).Returns(payStubModel);
            _mockDataContext.Setup(c => c.PayStub).Returns(mockSet.Object);

            //Act 
            var result = _mockPayStubService.GetCurrentByPayrollId(payrollId);

            //Assert 
            Assert.Equal(payStubModel.Count(), result.Count());
        }
        [Fact]
        public async void GetCurrentByPayrollId_WhenProvidingNonExistingPayrollId_ReturnsListWithNoItems()
        {
            var companyId = 1;
            var payrollId = 12;
            var payroll = new Payroll()
            {
                Id = 112,
                CompanyId = companyId,
                PayrollScheduleId = 2,
                ScheduledRunDate = DateOnly.FromDateTime(DateTime.Now),
                StartDate = DateOnly.FromDateTime(DateTime.Now),
                TrueRunDate = DateTime.Now
            };

            var payStubs = new List<PayStub>()
            {
                new PayStub()
                {
                    Id = 1,
                    PayrollId = payrollId,
                    EmploymentId = 13,
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
                },
                new PayStub()
                {
                    Id = 2,
                    PayrollId = payrollId,
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
                }
            }.AsQueryable();

            var mockSet = new Mock<DbSet<PayStub>>();
            mockSet.As<IQueryable<PayStub>>().Setup(m => m.Provider).Returns(payStubs.Provider);
            mockSet.As<IQueryable<PayStub>>().Setup(m => m.Expression).Returns(payStubs.Expression);
            mockSet.As<IQueryable<PayStub>>().Setup(m => m.ElementType).Returns(payStubs.ElementType);
            mockSet.As<IQueryable<PayStub>>().Setup(m => m.GetEnumerator()).Returns(payStubs.GetEnumerator());


            var payStubModel = new List<PayStubModel>();
            _mockMapper.Setup(m => m.Map<List<PayStubModel>>(It.IsAny<List<PayStub>>())).Returns(payStubModel);
            _mockDataContext.Setup(c => c.PayStub).Returns(mockSet.Object);

            //Act 
            var result = _mockPayStubService.GetCurrentByPayrollId(900);

            //Assert 
            Assert.Equal(payStubModel.Count(), result.Count());
        }

        #endregion

        #region GetReport using companyId, personId, datarange
        [Fact]
        public async void GetReport_WhenCompanyIdAndPersonIdExistAndPayrollScheduleWithRange_ReturnsPayStub()
        {
            var companyId = 1;
            var personId = 21;
            var jobId = 20;
            var payroll = new Payroll()
            {
                Id = 112,
                CompanyId = companyId,
                PayrollScheduleId = 2,
                ScheduledRunDate = DateOnly.FromDateTime(DateTime.Today),
                StartDate = DateOnly.FromDateTime(DateTime.Now),
                TrueRunDate = DateTime.Now
            };

            var employment = new Employment()
            {
                Id = 20,
                PersonId = personId,
                JobId = jobId,
                PayRateBasisId = 11,
                PayRate = 90,
                IsPayrollAdmin = false,
                IsSalariedOvertimeExempt = false,
                IsTenNinetyNine = false,
                StartDate = DateOnly.FromDateTime(DateTime.Now),
                Job = new Job()
                {
                    Id = jobId,
                    CompanyId = companyId,
                    AddressLine1 = "Bole Getu Commercial Center",
                    AddressLine2 = "Kebede Building",
                    AddressLine3 = "Second Floor",
                    ZipcodeId = 232
                }
            };

            var payStubs = new List<PayStub>()
            {
                new PayStub()
                {
                    Id = 1,
                    PayrollId = 12,
                    EmploymentId = 13,
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
                    Employment = employment,
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
                    Employment = employment,
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
                    Employment = employment
                },
            }.AsQueryable();

            var mockSet = new Mock<DbSet<PayStub>>();
            mockSet.As<IQueryable<PayStub>>().Setup(m => m.Provider).Returns(payStubs.Provider);
            mockSet.As<IQueryable<PayStub>>().Setup(m => m.Expression).Returns(payStubs.Expression);
            mockSet.As<IQueryable<PayStub>>().Setup(m => m.ElementType).Returns(payStubs.ElementType);
            mockSet.As<IQueryable<PayStub>>().Setup(m => m.GetEnumerator()).Returns(payStubs.GetEnumerator());

            var payStubModel = new List<PayStubModel>()
            {
                new PayStubModel()
                {
                    Id = 1,
                    PayrollId = 12,
                    EmploymentId = 13,
                    OtherMoneyReceivedId = 25,
                    RegularHoursWorked = 160,
                    OverTimeHoursWorked = 64,
                    GrossPay = 2500,
                    RegularPay = 2250
                }
            };

            _mockMapper.Setup(m => m.Map<List<PayStubModel>>(It.IsAny<List<PayStub>>())).Returns(payStubModel);
            _mockDataContext.Setup(c => c.PayStub).Returns(mockSet.Object);
            DateOnly from = DateOnly.FromDateTime(DateTime.Today.AddMonths(-3));
            DateOnly to = DateOnly.FromDateTime(DateTime.Today);

            //Act 
            var result = _mockPayStubService.GetReport(companyId, personId, from, to);

            //Assert 
            Assert.Equal(payStubModel.Count(), result.Count());
        }

        [Fact]
        public async void GetReport_WhenCompanyIdNotExistAndPersonIdExistAndPayrollScheduleWithRange_ReturnsEmptyPayStubList()
        {
            var companyId = 1;
            var personId = 21;
            var jobId = 20;
            var payroll = new Payroll()
            {
                Id = 112,
                CompanyId = companyId,
                PayrollScheduleId = 2,
                ScheduledRunDate = DateOnly.FromDateTime(DateTime.Today),
                StartDate = DateOnly.FromDateTime(DateTime.Now),
                TrueRunDate = DateTime.Now
            };

            var employment = new Employment()
            {
                Id = 20,
                PersonId = personId,
                JobId = jobId,
                PayRateBasisId = 11,
                PayRate = 90,
                IsPayrollAdmin = false,
                IsSalariedOvertimeExempt = false,
                IsTenNinetyNine = false,
                StartDate = DateOnly.FromDateTime(DateTime.Now),
                Job = new Job()
                {
                    Id = jobId,
                    CompanyId = companyId,
                    AddressLine1 = "Bole Getu Commercial Center",
                    AddressLine2 = "Kebede Building",
                    AddressLine3 = "Second Floor",
                    ZipcodeId = 232
                }
            };

            var payStubs = new List<PayStub>()
            {
                new PayStub()
                {
                    Id = 1,
                    PayrollId = 12,
                    EmploymentId = 13,
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
                    Employment = employment,
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
                    Employment = employment,
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
                    Employment = employment
                },
            }.AsQueryable();

            var mockSet = new Mock<DbSet<PayStub>>();
            mockSet.As<IQueryable<PayStub>>().Setup(m => m.Provider).Returns(payStubs.Provider);
            mockSet.As<IQueryable<PayStub>>().Setup(m => m.Expression).Returns(payStubs.Expression);
            mockSet.As<IQueryable<PayStub>>().Setup(m => m.ElementType).Returns(payStubs.ElementType);
            mockSet.As<IQueryable<PayStub>>().Setup(m => m.GetEnumerator()).Returns(payStubs.GetEnumerator());

            var payStubModel = new List<PayStubModel>();
            _mockMapper.Setup(m => m.Map<List<PayStubModel>>(It.IsAny<List<PayStub>>())).Returns(payStubModel);
            _mockDataContext.Setup(c => c.PayStub).Returns(mockSet.Object);
            DateOnly from = DateOnly.FromDateTime(DateTime.Today.AddMonths(-3));
            DateOnly to = DateOnly.FromDateTime(DateTime.Today);

            //Act 
            var result = _mockPayStubService.GetReport(104, personId, from, to);

            //Assert 
            Assert.Equal(payStubModel.Count(), result.Count());
        }

        [Fact]
        public async void GetReport_WhenCompanyIdExistAndPersonIdNotExistAndPayrollScheduleWithRange_ReturnsEmptyPayStubList()
        {
            var companyId = 1;
            var personId = 21;
            var jobId = 20;
            var payroll = new Payroll()
            {
                Id = 112,
                CompanyId = companyId,
                PayrollScheduleId = 2,
                ScheduledRunDate = DateOnly.FromDateTime(DateTime.Today),
                StartDate = DateOnly.FromDateTime(DateTime.Now),
                TrueRunDate = DateTime.Now
            };

            var employment = new Employment()
            {
                Id = 20,
                PersonId = personId,
                JobId = jobId,
                PayRateBasisId = 11,
                PayRate = 90,
                IsPayrollAdmin = false,
                IsSalariedOvertimeExempt = false,
                IsTenNinetyNine = false,
                StartDate = DateOnly.FromDateTime(DateTime.Now),
                Job = new Job()
                {
                    Id = jobId,
                    CompanyId = companyId,
                    AddressLine1 = "Bole Getu Commercial Center",
                    AddressLine2 = "Kebede Building",
                    AddressLine3 = "Second Floor",
                    ZipcodeId = 232
                }
            };

            var payStubs = new List<PayStub>()
            {
                new PayStub()
                {
                    Id = 1,
                    PayrollId = 12,
                    EmploymentId = 13,
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
                    Employment = employment,
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
                    Employment = employment,
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
                    Employment = employment
                },
            }.AsQueryable();

            var mockSet = new Mock<DbSet<PayStub>>();
            mockSet.As<IQueryable<PayStub>>().Setup(m => m.Provider).Returns(payStubs.Provider);
            mockSet.As<IQueryable<PayStub>>().Setup(m => m.Expression).Returns(payStubs.Expression);
            mockSet.As<IQueryable<PayStub>>().Setup(m => m.ElementType).Returns(payStubs.ElementType);
            mockSet.As<IQueryable<PayStub>>().Setup(m => m.GetEnumerator()).Returns(payStubs.GetEnumerator());

            var payStubModel = new List<PayStubModel>();
            _mockMapper.Setup(m => m.Map<List<PayStubModel>>(It.IsAny<List<PayStub>>())).Returns(payStubModel);
            _mockDataContext.Setup(c => c.PayStub).Returns(mockSet.Object);
            DateOnly from = DateOnly.FromDateTime(DateTime.Today.AddMonths(-3));
            DateOnly to = DateOnly.FromDateTime(DateTime.Today);

            //Act 
            var result = _mockPayStubService.GetReport(companyId, 8, from, to);

            //Assert 
            Assert.Equal(payStubModel.Count(), result.Count());
        }

        [Fact]
        public async void GetReport_WhenCompanyIdExistAndPersonIdExistAndPayrollScheduleNotWithRange_ReturnsEmptyPayStubList()
        {
            var companyId = 1;
            var personId = 21;
            var jobId = 20;
            var payroll = new Payroll()
            {
                Id = 112,
                CompanyId = companyId,
                PayrollScheduleId = 2,
                ScheduledRunDate = DateOnly.FromDateTime(DateTime.Today),
                StartDate = DateOnly.FromDateTime(DateTime.Now),
                TrueRunDate = DateTime.Now
            };

            var employment = new Employment()
            {
                Id = 20,
                PersonId = personId,
                JobId = jobId,
                PayRateBasisId = 11,
                PayRate = 90,
                IsPayrollAdmin = false,
                IsSalariedOvertimeExempt = false,
                IsTenNinetyNine = false,
                StartDate = DateOnly.FromDateTime(DateTime.Now),
                Job = new Job()
                {
                    Id = jobId,
                    CompanyId = companyId,
                    AddressLine1 = "Bole Getu Commercial Center",
                    AddressLine2 = "Kebede Building",
                    AddressLine3 = "Second Floor",
                    ZipcodeId = 232
                }
            };

            var payStubs = new List<PayStub>()
            {
                new PayStub()
                {
                    Id = 1,
                    PayrollId = 12,
                    EmploymentId = 13,
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
                    Employment = employment,
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
                    Employment = employment,
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
                    Employment = employment
                },
            }.AsQueryable();

            var mockSet = new Mock<DbSet<PayStub>>();
            mockSet.As<IQueryable<PayStub>>().Setup(m => m.Provider).Returns(payStubs.Provider);
            mockSet.As<IQueryable<PayStub>>().Setup(m => m.Expression).Returns(payStubs.Expression);
            mockSet.As<IQueryable<PayStub>>().Setup(m => m.ElementType).Returns(payStubs.ElementType);
            mockSet.As<IQueryable<PayStub>>().Setup(m => m.GetEnumerator()).Returns(payStubs.GetEnumerator());

            var payStubModel = new List<PayStubModel>();
            _mockMapper.Setup(m => m.Map<List<PayStubModel>>(It.IsAny<List<PayStub>>())).Returns(payStubModel);
            _mockDataContext.Setup(c => c.PayStub).Returns(mockSet.Object);
            DateOnly from = DateOnly.FromDateTime(DateTime.Today.AddMonths(-3));
            DateOnly to = DateOnly.FromDateTime(DateTime.Today.AddMonths(-4));

            //Act 
            var result = _mockPayStubService.GetReport(companyId, personId, from, to);

            //Assert 
            Assert.Equal(payStubModel.Count(), result.Count());
        }

        #endregion

        #region Get Report using companyId, datarange

        [Fact]
        public async void GetReport_WhenCompanyIdAndPayrollScheduleWithRange_ReturnsPayStub()
        {
            var companyId = 1;
            var personId = 21;
            var jobId = 20;
            var payroll = new Payroll()
            {
                Id = 112,
                CompanyId = companyId,
                PayrollScheduleId = 2,
                ScheduledRunDate = DateOnly.FromDateTime(DateTime.Today),
                StartDate = DateOnly.FromDateTime(DateTime.Now),
                TrueRunDate = DateTime.Now
            };

            var employment = new Employment()
            {
                Id = 20,
                PersonId = personId,
                JobId = jobId,
                PayRateBasisId = 11,
                PayRate = 90,
                IsPayrollAdmin = false,
                IsSalariedOvertimeExempt = false,
                IsTenNinetyNine = false,
                StartDate = DateOnly.FromDateTime(DateTime.Now),
                Job = new Job()
                {
                    Id = jobId,
                    CompanyId = companyId,
                    AddressLine1 = "Bole Getu Commercial Center",
                    AddressLine2 = "Kebede Building",
                    AddressLine3 = "Second Floor",
                    ZipcodeId = 232
                }
            };

            var payStubs = new List<PayStub>()
            {
                new PayStub()
                {
                    Id = 1,
                    PayrollId = 12,
                    EmploymentId = 13,
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
                    Employment = employment,
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
                    Employment = employment,
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
                    Employment = employment
                },
            }.AsQueryable();

            var mockSet = new Mock<DbSet<PayStub>>();
            mockSet.As<IQueryable<PayStub>>().Setup(m => m.Provider).Returns(payStubs.Provider);
            mockSet.As<IQueryable<PayStub>>().Setup(m => m.Expression).Returns(payStubs.Expression);
            mockSet.As<IQueryable<PayStub>>().Setup(m => m.ElementType).Returns(payStubs.ElementType);
            mockSet.As<IQueryable<PayStub>>().Setup(m => m.GetEnumerator()).Returns(payStubs.GetEnumerator());

            var payStubModel = new List<PayStubModel>()
            {
                new PayStubModel()
                {
                    Id = 1,
                    PayrollId = 12,
                    EmploymentId = 13,
                    OtherMoneyReceivedId = 25,
                    RegularHoursWorked = 160,
                    OverTimeHoursWorked = 64,
                    GrossPay = 2500,
                    RegularPay = 2250
                }
            };

            _mockMapper.Setup(m => m.Map<List<PayStubModel>>(It.IsAny<List<PayStub>>())).Returns(payStubModel);
            _mockDataContext.Setup(c => c.PayStub).Returns(mockSet.Object);
            DateOnly from = DateOnly.FromDateTime(DateTime.Today.AddMonths(-3));
            DateOnly to = DateOnly.FromDateTime(DateTime.Today);

            //Act 
            var result = _mockPayStubService.GetReport(companyId, from, to);

            //Assert 
            Assert.Equal(payStubModel.Count(), result.Count());
        }

        [Fact]
        public async void GetReport_WhenCompanyIdNotExistingAndPayrollScheduleWithRange_ReturnsEmptyPayStubList()
        {
            var companyId = 1;
            var personId = 21;
            var jobId = 20;
            var payroll = new Payroll()
            {
                Id = 112,
                CompanyId = companyId,
                PayrollScheduleId = 2,
                ScheduledRunDate = DateOnly.FromDateTime(DateTime.Today),
                StartDate = DateOnly.FromDateTime(DateTime.Now),
                TrueRunDate = DateTime.Now
            };

            var employment = new Employment()
            {
                Id = 20,
                PersonId = personId,
                JobId = jobId,
                PayRateBasisId = 11,
                PayRate = 90,
                IsPayrollAdmin = false,
                IsSalariedOvertimeExempt = false,
                IsTenNinetyNine = false,
                StartDate = DateOnly.FromDateTime(DateTime.Now),
                Job = new Job()
                {
                    Id = jobId,
                    CompanyId = companyId,
                    AddressLine1 = "Bole Getu Commercial Center",
                    AddressLine2 = "Kebede Building",
                    AddressLine3 = "Second Floor",
                    ZipcodeId = 232
                }
            };

            var payStubs = new List<PayStub>()
            {
                new PayStub()
                {
                    Id = 1,
                    PayrollId = 12,
                    EmploymentId = 13,
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
                    Employment = employment,
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
                    Employment = employment,
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
                    Employment = employment
                },
            }.AsQueryable();

            var mockSet = new Mock<DbSet<PayStub>>();
            mockSet.As<IQueryable<PayStub>>().Setup(m => m.Provider).Returns(payStubs.Provider);
            mockSet.As<IQueryable<PayStub>>().Setup(m => m.Expression).Returns(payStubs.Expression);
            mockSet.As<IQueryable<PayStub>>().Setup(m => m.ElementType).Returns(payStubs.ElementType);
            mockSet.As<IQueryable<PayStub>>().Setup(m => m.GetEnumerator()).Returns(payStubs.GetEnumerator());

            var payStubModel = new List<PayStubModel>();
            _mockMapper.Setup(m => m.Map<List<PayStubModel>>(It.IsAny<List<PayStub>>())).Returns(payStubModel);
            _mockDataContext.Setup(c => c.PayStub).Returns(mockSet.Object);
            DateOnly from = DateOnly.FromDateTime(DateTime.Today.AddMonths(-3));
            DateOnly to = DateOnly.FromDateTime(DateTime.Today);

            //Act 
            var result = _mockPayStubService.GetReport(89, from, to);

            //Assert 
            Assert.Equal(payStubModel.Count(), result.Count());
        }

        [Fact]
        public async void GetReport_WhenCompanyIdExistingAndPayrollScheduleNotWithRange_ReturnsEmptyPayStubList()
        {
            var companyId = 1;
            var personId = 21;
            var jobId = 20;
            var payroll = new Payroll()
            {
                Id = 112,
                CompanyId = companyId,
                PayrollScheduleId = 2,
                ScheduledRunDate = DateOnly.FromDateTime(DateTime.Today),
                StartDate = DateOnly.FromDateTime(DateTime.Now),
                TrueRunDate = DateTime.Now
            };

            var employment = new Employment()
            {
                Id = 20,
                PersonId = personId,
                JobId = jobId,
                PayRateBasisId = 11,
                PayRate = 90,
                IsPayrollAdmin = false,
                IsSalariedOvertimeExempt = false,
                IsTenNinetyNine = false,
                StartDate = DateOnly.FromDateTime(DateTime.Now),
                Job = new Job()
                {
                    Id = jobId,
                    CompanyId = companyId,
                    AddressLine1 = "Bole Getu Commercial Center",
                    AddressLine2 = "Kebede Building",
                    AddressLine3 = "Second Floor",
                    ZipcodeId = 232
                }
            };

            var payStubs = new List<PayStub>()
            {
                new PayStub()
                {
                    Id = 1,
                    PayrollId = 12,
                    EmploymentId = 13,
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
                    Employment = employment,
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
                    Employment = employment,
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
                    Employment = employment
                },
            }.AsQueryable();

            var mockSet = new Mock<DbSet<PayStub>>();
            mockSet.As<IQueryable<PayStub>>().Setup(m => m.Provider).Returns(payStubs.Provider);
            mockSet.As<IQueryable<PayStub>>().Setup(m => m.Expression).Returns(payStubs.Expression);
            mockSet.As<IQueryable<PayStub>>().Setup(m => m.ElementType).Returns(payStubs.ElementType);
            mockSet.As<IQueryable<PayStub>>().Setup(m => m.GetEnumerator()).Returns(payStubs.GetEnumerator());

            var payStubModel = new List<PayStubModel>();
            _mockMapper.Setup(m => m.Map<List<PayStubModel>>(It.IsAny<List<PayStub>>())).Returns(payStubModel);
            _mockDataContext.Setup(c => c.PayStub).Returns(mockSet.Object);
            DateOnly from = DateOnly.FromDateTime(DateTime.Today.AddMonths(-3));
            DateOnly to = DateOnly.FromDateTime(DateTime.Today.AddDays(-5));

            //Act 
            var result = _mockPayStubService.GetReport(89, from, to);

            //Assert 
            Assert.Equal(payStubModel.Count(), result.Count());
        }

        #endregion

        #region Get PayStub

        [Fact]
        public async void GetPayStub_WhenThereIsExistingPayStub_ReturnsPayStubDetailInfo()
        {
            var companyId = 1;
            var personId = 21;
            var jobId = 20;
            var payroll = new Payroll()
            {
                Id = 112,
                CompanyId = companyId,
                PayrollScheduleId = 2,
                ScheduledRunDate = DateOnly.FromDateTime(DateTime.Today),
                StartDate = DateOnly.FromDateTime(DateTime.Now),
                TrueRunDate = DateTime.Now
            };

            var employment = new Employment()
            {
                Id = 20,
                PersonId = personId,
                JobId = jobId,
                PayRateBasisId = 11,
                PayRate = 90,
                IsPayrollAdmin = false,
                IsSalariedOvertimeExempt = false,
                IsTenNinetyNine = false,
                StartDate = DateOnly.FromDateTime(DateTime.Now),
                Job = new Job()
                {
                    Id = jobId,
                    CompanyId = companyId,
                    AddressLine1 = "Bole Getu Commercial Center",
                    AddressLine2 = "Kebede Building",
                    AddressLine3 = "Second Floor",
                    ZipcodeId = 232
                }
            };
            //Arrange
            var payStub = new PayStub()
            {
                Id = 1,
                PayrollId = 112,
                EmploymentId = 20,
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
                Employment = employment,
            };
            var mockSet = new Mock<DbSet<PayStub>>();
            _mockDataContext.Setup(c => c.PayStub).Returns(mockSet.Object);
            _mockDataContext.Setup(c => c.PayStub.Find(payStub.Id)).Returns(payStub);

            //Act
            var result = _mockPayStubService.GetById(1);

            //Assert
            Assert.Equal(payStub, result);
        }

        [Fact]
        public async void GetPayStub_WhenIdNotExistingPayStub_ReturnsNull()
        {
            //Arrange
            var payStub = new PayStub()
            {
                Id = 1,
                PayrollId = 12,
                EmploymentId = 13,
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
            };
            var mockSet = new Mock<DbSet<PayStub>>();
            _mockDataContext.Setup(c => c.PayStub).Returns(mockSet.Object);
            _mockDataContext.Setup(c => c.PayStub.Find(payStub.Id)).Returns((PayStub)null);

            //Act
            var result = _mockPayStubService.GetById(16);

            //Assert
            Assert.Null(result);
        }

        #endregion

        #region GetModelById

        [Fact]
        public async void GetModelById_WhenThereIsExistingPayStub_ReturnsPayStubDetailInfo()
        {
            //Arrange
            var payStub = new PayStub()
            {
                Id = 1,
                PayrollId = 12,
                EmploymentId = 13,
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
            };
            var payStubModel = new PayStubModel()
            {
                Id = 1,
                PayrollId = 12,
                EmploymentId = 13,
                OtherMoneyReceivedId = 25,
                RegularHoursWorked = 160,
                OverTimeHoursWorked = 64,
                GrossPay = 2500,
                RegularPay = 2250
            };
            var mockSet = new Mock<DbSet<PayStub>>();
            _mockDataContext.Setup(c => c.PayStub).Returns(mockSet.Object);
            _mockMapper.Setup(m => m.Map<PayStubModel>(It.IsAny<PayStub>())).Returns(payStubModel);
            _mockDataContext.Setup(c => c.PayStub.Find(payStub.Id)).Returns(payStub);

            //Act
            var result = _mockPayStubService.GetModelById(1);

            //Assert
            Assert.Equal(payStubModel, result);
        }

        [Fact]
        public async void GetModelById_WhenThereIsNoExistingPayStub_ReturnsNull()
        {
            //Arrange
            var payStub = new PayStub()
            {
                Id = 1,
                PayrollId = 12,
                EmploymentId = 13,
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
            };
            var payStubModel = new PayStubModel()
            {
                Id = 1,
                PayrollId = 12,
                EmploymentId = 13,
                OtherMoneyReceivedId = 25,
                RegularHoursWorked = 160,
                OverTimeHoursWorked = 64,
                GrossPay = 2500,
                RegularPay = 2250
            };
            var mockSet = new Mock<DbSet<PayStub>>();
            _mockDataContext.Setup(c => c.PayStub).Returns(mockSet.Object);
            _mockMapper.Setup(m => m.Map<PayStubModel>(It.IsAny<PayStub>())).Returns(payStubModel);
            _mockDataContext.Setup(c => c.PayStub.Find(120)).Returns((PayStub)null);

            //Act
            var result = _mockPayStubService.GetModelById(10);

            //Assert
            Assert.Null(result);
        }

        #endregion

        #region Update PayStub
        [Fact]
        public async void UpdatePayStub_ForExistingPayStub_ReturnTheUpdatePayStub()
        {
            //Arrange
            var payStub = new PayStub()
            {
                Id = 1,
                PayrollId = 12,
                EmploymentId = 13,
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
            };
            var payStubModel = new PayStubModel()
            {
                Id = 1,
                PayrollId = 15,
                EmploymentId = 13,
                OtherMoneyReceivedId = 25,
                RegularHoursWorked = 160,
                OverTimeHoursWorked = 64,
                GrossPay = 2500,
                RegularPay = 2250
            };
            var updatedPayStub = new PayStub()
            {
                Id = 1,
                PayrollId = 15,
                EmploymentId = 13,
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
            };
            var mockSet = new Mock<DbSet<PayStub>>();
            _mockDataContext.Setup(c => c.PayStub).Returns(mockSet.Object);
            _mockMapper.Setup(m => m.Map<PayStub>(It.IsAny<PayStubModel>())).Returns(updatedPayStub);
            _mockDataContext.Setup(c => c.PayStub.Find(1)).Returns(payStub);

            //Act
            _mockPayStubService.Update(1, payStubModel);

            //Assert
            _mockDataContext.Verify(m => m.SaveChanges(), Times.Once);
        }
        [Fact]
        public async void UpdatePayStub_ForNotExistingPayStub_ReturnTheUpdatePayStub()
        {
            //Arrange
            var payStub = new PayStub()
            {
                Id = 1,
                PayrollId = 12,
                EmploymentId = 13,
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
            };
            var payStubModel = new PayStubModel()
            {
                Id = 1,
                PayrollId = 15,
                EmploymentId = 13,
                OtherMoneyReceivedId = 25,
                RegularHoursWorked = 160,
                OverTimeHoursWorked = 64,
                GrossPay = 2500,
                RegularPay = 2250
            };
            var updatedPayStub = new PayStub()
            {
                Id = 1,
                PayrollId = 15,
                EmploymentId = 13,
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
            };
            var mockSet = new Mock<DbSet<PayStub>>();
            _mockDataContext.Setup(c => c.PayStub).Returns(mockSet.Object);
            _mockMapper.Setup(m => m.Map<PayStub>(It.IsAny<PayStubModel>())).Returns(updatedPayStub);
            _mockDataContext.Setup(c => c.PayStub.Find(1)).Returns(payStub);

            //Act
            var exception = Assert.Throws<KeyNotFoundException>(() => 
                _mockPayStubService.Update(2, payStubModel)
             );

            //Assert
            Assert.Equal("PayStub not found", exception.Message);
        }

        #endregion

        #region  Delete PayStub


        [Fact]
        public async void DeletePayStub_WhenPassingExistingPayStubID_ReturnListThatExcludeTheDeletedPayStub()
        {
            var payStubs = new List<PayStub>()
            {
                new PayStub()
                {
                    Id = 1,
                    PayrollId = 12,
                    EmploymentId = 13,
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
                    IsCancelled = false
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
                    IsCancelled = false
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
                    IsCancelled = false
                },
            }.AsQueryable();

            var mockSet = new Mock<DbSet<PayStub>>();
            mockSet.As<IQueryable<PayStub>>().Setup(m => m.Provider).Returns(payStubs.Provider);
            mockSet.As<IQueryable<PayStub>>().Setup(m => m.Expression).Returns(payStubs.Expression);
            mockSet.As<IQueryable<PayStub>>().Setup(m => m.ElementType).Returns(payStubs.ElementType);
            mockSet.As<IQueryable<PayStub>>().Setup(m => m.GetEnumerator()).Returns(payStubs.GetEnumerator());

            _mockDataContext.Setup(c => c.PayStub).Returns(mockSet.Object);
            _mockDataContext.Setup(c => c.PayStub.Find(1)).Returns(payStubs.First());

            //Act
            _mockPayStubService.Delete(1);

            //Assert
            _mockDataContext.Verify(m => m.SaveChanges(), Times.Once);
        }

        [Fact]
        public async void DeletePayStub_WhenPassingNonExistingPayStubID_NoPayStubDeleted()
        {
            var payStubs = new List<PayStub>()
            {
                new PayStub()
                {
                    Id = 1,
                    PayrollId = 12,
                    EmploymentId = 13,
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
                    IsCancelled = false
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
                    IsCancelled = false
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
                    IsCancelled = false
                },
            }.AsQueryable();

            var mockSet = new Mock<DbSet<PayStub>>();
            mockSet.As<IQueryable<PayStub>>().Setup(m => m.Provider).Returns(payStubs.Provider);
            mockSet.As<IQueryable<PayStub>>().Setup(m => m.Expression).Returns(payStubs.Expression);
            mockSet.As<IQueryable<PayStub>>().Setup(m => m.ElementType).Returns(payStubs.ElementType);
            mockSet.As<IQueryable<PayStub>>().Setup(m => m.GetEnumerator()).Returns(payStubs.GetEnumerator());

            _mockDataContext.Setup(c => c.PayStub).Returns(mockSet.Object);
            _mockDataContext.Setup(c => c.PayStub.Find(5)).Returns(payStubs.First());

            //Act
            _mockPayStubService.Delete(1);

            //Assert
            Assert.Equal(3, payStubs.Count());
        }

        #endregion
    }
}
