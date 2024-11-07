namespace TelonaiWebApi.Services;

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.Ocsp;
using System;
using System.ComponentModel.Design;
using TelonaiWebApi.Entities;
using TelonaiWebApi.Helpers;
using TelonaiWebApi.Models;

public interface IPayStubService
{
    List<PayStubModel> GetCurrentByCompanyId(int companyId);
    List<PayStubModel> GetCurrentByCompanyIdAndPersonId(int companyId, int personId);
    List<PayStubModel> GetCurrentByPayrollId(int payrollId);
    List<PayStubModel> GetReport(int companyId, int personId, DateOnly from, DateOnly to);
    List<PayStubModel> GetReport(int companyId, DateOnly from, DateOnly to);
    Task GeneratePayStubPdfs(int payrollId, int companyId);
    PayStubModel GetModelById(int id);
    PayStub GetById(int id);
    Task<Stream> GetDocumentByDocumentId(Guid documentId);
    void Create(PayStubModel model);
    void Update(int id, PayStubModel model);
    bool Delete(int id);
}

public class PayStubService : IPayStubService
{
    private DataContext _context;
    private readonly IMapper _mapper;
    private int _numberOfPaymentsInYear = 12;
    private IStaticDataService _staticDataService;
    private List<IncomeTax> _newIncomeTaxesToHold = new();
    private int _countryId;
    private int _stateId;
    private int _currentYear;


    public PayStubService(DataContext context, IMapper mapper, IStaticDataService staticDataService)
    {
        _context = context;
        _mapper = mapper;
        _staticDataService = staticDataService;
    }

    public List<PayStubModel> GetCurrentByCompanyId(int CompanyId)
    {
        var obj = _context.PayStub
        .Where(c => c.Payroll.CompanyId == CompanyId)
        .GroupBy(c => c.PayrollId)
        .Select(g => g.OrderByDescending(c => c.PayrollId).First()).ToList();

        var result = _mapper.Map<List<PayStubModel>>(obj);
        return result;
    }

    public List<PayStubModel> GetCurrentByCompanyIdAndPersonId(int companyId, int personId)
    {
        var obj = _context.PayStub
        .Where(c => c.Employment.Job.CompanyId == companyId && c.Employment.PersonId == personId)
        .GroupBy(c => c.PayrollId)
        .Select(g => g.OrderByDescending(c => c.PayrollId).First()).ToList();

        var result = _mapper.Map<List<PayStubModel>>(obj);
        return result;
    }

    public List<PayStubModel> GetCurrentByPayrollId(int payrollId)
    {
        var obj = _context.PayStub.Where(e => e.PayrollId == payrollId).ToList();
        var result = _mapper.Map<List<PayStubModel>>(obj);
        return result;
    }

    public List<PayStubModel> GetReport(int companyId, int personId, DateOnly from, DateOnly to)
    {
        var obj = _context.PayStub.Where(e => e.Payroll.CompanyId == companyId && e.Employment.PersonId == personId &&
        e.Payroll.ScheduledRunDate >= from && e.Payroll.ScheduledRunDate < to.AddDays(1)).ToList();
        var result = _mapper.Map<List<PayStubModel>>(obj);
        return result;
    }
    public List<PayStubModel> GetReport(int companyId, DateOnly from, DateOnly to)
    {
        var obj = _context.PayStub.Where(e => e.Payroll.CompanyId == companyId && e.Payroll.ScheduledRunDate >= from &&
        e.Payroll.ScheduledRunDate < to.AddDays(1)).ToList();
        var result = _mapper.Map<List<PayStubModel>>(obj);
        return result;
    }

    public PayStub GetById(int payStubId)
    {
        return GetPayStub(payStubId);
    }

    public PayStubModel GetModelById(int payStubId)
    {
        var obj = GetPayStub(payStubId);
        if (obj == null)
            return null;

        return _mapper.Map<PayStubModel>(obj);
    }

    public async Task<Stream> GetDocumentByDocumentId(Guid documentId)
    {
        var pdfManager = new DocumentManager();
        var result = await pdfManager.GetPayStubByIdAsync(documentId.ToString());
        return result;
    }

    public async Task GeneratePayStubPdfs(int payrollId, int companyId)
    {
        var dTime = DateTime.UtcNow;
        var pdfManager = new DocumentManager();
        var payroll = _context.Payroll.Include(e => e.Company).ThenInclude(e => e.Zipcode).ThenInclude(e => e.City)
            .ThenInclude(e => e.State).FirstOrDefault(e=>e.Id==payrollId && e.CompanyId==companyId)
            ?? throw new AppException("Payroll not found");

        _currentYear = payroll.ScheduledRunDate.Year;
        var state = payroll.Company.Zipcode.City.State;
        _stateId = state.Id;
        _countryId = state.CountryId;

        var schedule = (PayrollScheduleTypeModel)payroll.PayrollScheduleId;
        if (schedule == PayrollScheduleTypeModel.Weekly) { _numberOfPaymentsInYear = 52; }
        else if (schedule == PayrollScheduleTypeModel.Monthly) { _numberOfPaymentsInYear = 12; }
        else if (schedule == PayrollScheduleTypeModel.SemiMonthly) { _numberOfPaymentsInYear = 24; }
        else if (schedule == PayrollScheduleTypeModel.Biweekly) { _numberOfPaymentsInYear = 26; }


        var payStubs = _context.PayStub.Include(e=>e.OtherMoneyReceived).Include(e => e.Employment).ThenInclude(e => e.Person)
                .ThenInclude(e => e.Zipcode).ThenInclude(e => e.City)
                .Where(e => e.PayrollId == payrollId) ?? throw new AppException("Pay stubs not found");

        foreach (var payStub in payStubs.ToList())
        {
            //Calculate Federal and State Taxes

            if (!payStub.Employment.IsTenNinetyNine)
            {
                await CalculateFederalWitholdingsAsync(payStub);
                await CalculateStateWitholdingAsync(payStub);
                await _context.SaveChangesAsync();
            }

            //Create PDFs
            var docId = await pdfManager.CreatePayStubPdfAsync(payStub, payStub.OtherMoneyReceived, _newIncomeTaxesToHold.ToList());
            var doc = new Document { Id = docId, FileName = string.Format("PayStub-" + payStub.Id + "-" + dTime.ToString("yyyyMMddmmss") + ".pdf") };
            _context.Document.Add(doc);

            payStub.DocumentId = docId;
            _context.PayStub.Update(payStub);
            await _context.SaveChangesAsync();
        }

        payroll.EmployeesOwed = payStubs.Select(e => e.NetPay).Sum();
        payroll.StatesOwed = _newIncomeTaxesToHold.Where(e => e.IncomeTaxType.StateId!=null).Select(e=>e.Amount).Sum();
        payroll.FederalOwed = _newIncomeTaxesToHold.Where(e => e.IncomeTaxType.StateId == null).Select(e => e.Amount).Sum();
    }

    public void Create(PayStubModel model)
    {
        throw new NotImplementedException();
    }

    public void Update(int id, PayStubModel model)
    {
        var dto = GetPayStub(id) ?? throw new KeyNotFoundException("PayStub not found");
        var payStub = _mapper.Map<PayStub>(model);
        if (payStub.PayrollId != dto.PayrollId)
        {
            dto.PayrollId = payStub.PayrollId;
        }
        if (payStub.EmploymentId != dto.EmploymentId)
        {
            dto.EmploymentId = payStub.EmploymentId;    
        }
        if(payStub.OtherMoneyReceived != dto.OtherMoneyReceived)
        {
            dto.OtherMoneyReceived = payStub.OtherMoneyReceived;
        }
        if(payStub.RegularHoursWorked != dto.RegularHoursWorked)
        {
            dto.RegularHoursWorked = payStub.RegularHoursWorked;    
        }
        if(payStub.OverTimeHoursWorked != dto.OverTimeHoursWorked)
        {
            dto.OverTimeHoursWorked = dto.OverTimeHoursWorked;
        }
        if(payStub.GrossPay != payStub.GrossPay)
        {
            dto.GrossPay = payStub.GrossPay;
        }
        if(payStub.NetPay != dto.NetPay)
        {
            dto.NetPay = payStub.NetPay;
        }
        if(payStub.OverTimePay != dto.OverTimePay)
        {
            dto.OverTimePay = payStub.OverTimePay;
        }
        if(payStub.RegularPay != dto.RegularPay)
        {
            dto.RegularPay = payStub.RegularPay;
        }

        _context.PayStub.Update(dto);
        _context.SaveChanges();
    }

    public bool Delete(int id)
    {
        var dto = GetPayStub(id);
        if (dto != null)
        {
            _context.PayStub.Remove(dto);
            return _context.SaveChanges() > 0 ? true : false;
        }
        else
        {
            return false;
        }
    }

    private PayStub GetPayStub(int id)
    {
        var dto = _context.PayStub.Find(id);
        return dto;
    }

    private async Task CalculateFederalWitholdingsAsync(PayStub stub)
    {
        try
        {
            var numberOfPaymentsInYear = 12;
            var incomeTaxRates = _staticDataService.GetIncomeTaxRatesByCountryId(_countryId);

            var withHolingForms = _context.EmployeeWithholding.Where(e => e.EmploymentId == stub.EmploymentId && e.Field.WithholdingYear == DateTime.Now.Year).ToList();
            var w4Form = withHolingForms.Where(e => e.Field.DocumentTypeId == (int)DocumentTypeModel.WFour).ToList();

            var fedFilingStatus = FilingStatusTypeModel.Single;

            var w4OneA = w4Form.First(e => e.Field.FieldNme == "1a")?.FieldValue;
            var w4OneC = w4Form.First(e => e.Field.FieldNme == "1c")?.FieldValue;
            if (!Enum.TryParse(w4OneC, out fedFilingStatus))
                throw new InvalidDataException($"Invalid filing status [{w4OneC}]");

            var w4TwoC = w4Form.First(e => e.Field.FieldNme == "2c")?.FieldValue;
            var w4Three = w4Form.First(e => e.Field.FieldNme == "3")?.FieldValue;
            var w4FourA = w4Form.First(e => e.Field.FieldNme == "4a")?.FieldValue;
            var w4FourB = w4Form.First(e => e.Field.FieldNme == "4a")?.FieldValue;
            var w4FourC = w4Form.First(e => e.Field.FieldNme == "4a")?.FieldValue;
            var w4FourD = w4Form.First(e => e.Field.FieldNme == "4a")?.FieldValue;

            var employeeFederalRates = incomeTaxRates.Where(e => e.IncomeTaxType.ForEmployee && e.IncomeTaxType.StateId == null && e.FilingStatusId == (int)fedFilingStatus).ToList();
            var employerFederalRates = incomeTaxRates.Where(e => !e.IncomeTaxType.ForEmployee && e.IncomeTaxType.StateId == null).ToList();

            //Calculate Federal Tax to withhold
            var hasMultipleJobs = !string.IsNullOrWhiteSpace(w4TwoC);
            var anualAmount = stub.GrossPay * numberOfPaymentsInYear + double.Parse(w4FourA);
            var deduction = double.Parse(w4FourB) * (hasMultipleJobs ? 0 : w4OneC.Contains("Jointly") ? 12900 : 8600);
            var adjustedAnnualWageAmount = anualAmount - deduction;

            var rate = employeeFederalRates.First(e => e.IncomeTaxType.Name.StartsWith("Federal") &&
            e.Minimum < Math.Round(adjustedAnnualWageAmount) && e.Maximum < Math.Round(adjustedAnnualWageAmount));

            var previousTax = _context.IncomeTax.OrderByDescending(e => e.CreatedDate).FirstOrDefault(e => !e.PayStub.IsCancelled && e.IncomeTaxTypeId == rate.IncomeTaxTypeId);

            var tentativeWithholdingAmt = ((adjustedAnnualWageAmount - rate.Minimum) * rate.Rate +
                (rate.TentativeAmount ?? 0)) / numberOfPaymentsInYear;

            //Credits
            var credits = double.Parse(w4Three) / numberOfPaymentsInYear;
            tentativeWithholdingAmt = Math.Max(tentativeWithholdingAmt - credits, 0);

            var finalWithholdingAmount = tentativeWithholdingAmt + double.Parse(w4FourC);
            _newIncomeTaxesToHold.Add(
                new IncomeTax
                {
                    Amount = Math.Round(finalWithholdingAmount),
                    IncomeTaxTypeId = rate.IncomeTaxTypeId,
                    PayStubId = stub.Id,
                    YtdAmount = finalWithholdingAmount + previousTax?.YtdAmount ?? 0
                }
            );

            //Other Federal Withholdings
            var otherFederalWithholdings = employeeFederalRates.Where(e => !e.IncomeTaxType.Name.StartsWith("Federal") &&
            e.Minimum < Math.Round(stub.GrossPay) && e.Maximum < Math.Round(stub.GrossPay));
            foreach (var item in otherFederalWithholdings)
            {
                var amount = stub.GrossPay * item.Rate;
                var previous = _context.IncomeTax.OrderByDescending(e => e.CreatedDate).FirstOrDefault(e => !e.PayStub.IsCancelled && e.IncomeTaxTypeId == rate.IncomeTaxTypeId);

                _newIncomeTaxesToHold.Add(
                    new IncomeTax
                    {
                        Amount = amount,
                        IncomeTaxTypeId = rate.IncomeTaxTypeId,
                        PayStubId = stub.Id,
                        YtdAmount = finalWithholdingAmount + previous?.YtdAmount ?? 0
                    }
                );
            }
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    private async Task CalculateStateWitholdingAsync(PayStub stub)
    {
        var numberOfPaymentsInYear = 12;
        var incomeTaxRates = _staticDataService.GetIncomeTaxRatesByCountryId(_countryId);
        var withHolingForms = _context.EmployeeWithholding.Where(e => e.EmploymentId == stub.EmploymentId && e.Field.WithholdingYear == DateTime.Now.Year);
        var nc4 = withHolingForms.Where(e => e.Field.DocumentTypeId == (int)DocumentTypeModel.NCFour);
        var ncAnnualStandardDeductions = _staticDataService.GetStateStandardDeductionsByStateId(_stateId, _currentYear);
        var ncFilingStatus = FilingStatusTypeModel.Single;

        var ncOne = nc4.First(e => e.Field.FieldNme == "totalnumberofallowances")?.FieldValue;
        var ncTwo = nc4.First(e => e.Field.FieldNme == "additionalamount")?.FieldValue;
        var status = nc4.First(e => e.Field.FieldNme == "maritalstatus")?.FieldValue;


        if (!Enum.TryParse(status, out ncFilingStatus))
            throw new InvalidDataException($"Invalid filing status [{status}]");

        var annualDeduction = ncAnnualStandardDeductions.First(e => e.FilingStatusId == (int)ncFilingStatus);

        var employeeStateRate = incomeTaxRates.First(e => e.IncomeTaxType.ForEmployee && e.IncomeTaxType.StateId != null);
        var employerStateRate = incomeTaxRates.First(e => !e.IncomeTaxType.ForEmployee && e.IncomeTaxType.StateId != null);

        var previous = await _context.IncomeTax.OrderByDescending(e => e.CreatedDate).FirstOrDefaultAsync(e => !e.PayStub.IsCancelled 
        && e.IncomeTaxTypeId == employeeStateRate.IncomeTaxTypeId);

        //Calculate Tax
        var annualWage = stub.GrossPay * numberOfPaymentsInYear;
        var allowance = 2500 * double.Parse(ncOne);
        var deduction = allowance + annualDeduction.Amount;
        var netAnnualWage = annualWage - deduction;
        var netAnnualTax = netAnnualWage * employeeStateRate.Rate;
        var netCurrentTax = netAnnualTax / _numberOfPaymentsInYear;
        _newIncomeTaxesToHold.Add(
               new IncomeTax
               {
                   Amount = netCurrentTax,
                   IncomeTaxTypeId = employeeStateRate.IncomeTaxTypeId,
                   PayStubId = stub.Id,
                   YtdAmount = netCurrentTax + previous?.YtdAmount ?? 0
               }
           );
    }
}

