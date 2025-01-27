namespace TelonaiWebApi.Services;

using Amazon.SQS.Model;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.Ocsp;
using System;
using System.ComponentModel.Design;
using TelonaiWebApi.Entities;
using TelonaiWebApi.Helpers;
using TelonaiWebApi.Models;
using static iTextSharp.text.pdf.AcroFields;

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
    Task<List<PayStubModel>> GetCurrentOwnPayStubByCompanyId(int companyId, int count, int skip);
}

public class PayStubService : IPayStubService
{
    private DataContext _context;
    private readonly IMapper _mapper;
    private int _numberOfPaymentsInYear;
    private IStaticDataService _staticDataService;
    private List<IncomeTax> _newIncomeTaxesToHold = new();
    private int _countryId;
    private int _stateId;
    private int _currentYear;
    private readonly IPersonService<PersonModel, Person> _personService;
    private readonly ILogger<PayStubService> _logger;
    private readonly IDocumentManager _documentManager;

    public PayStubService(DataContext context, IMapper mapper, IStaticDataService staticDataService
        , IPersonService<PersonModel, Person> personService, ILogger<PayStubService> logger, IDocumentManager documentManager)
    {
        _context = context;
        _mapper = mapper;
        _staticDataService = staticDataService;
        _personService = personService;
        _logger = logger;
        _documentManager = documentManager;
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
    public async Task<List<PayStubModel>> GetCurrentOwnPayStubByCompanyId(int companyId, int count, int skip)
    {
        var person = await _personService.GetCurrentUserAsync();
        var obj = _context.PayStub.OrderByDescending(c => c.Id)
        .Where(c => c.Employment.Job.CompanyId == companyId && c.Employment.PersonId == person.Id
        && c.Payroll.TrueRunDate != null)
        .Skip(skip).Take(count).ToList();

        var result = _mapper.Map<List<PayStubModel>>(obj);
        return result;
    }

    public List<PayStubModel> GetCurrentByPayrollId(int payrollId)
    {
        var obj = _context.PayStub.Where(e => e.PayrollId == payrollId).Include(e => e.Payroll)
            .Include(e => e.Employment).ToList();
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
        var pdfManager = new DocumentManager(_context);

        var result = await pdfManager.GetPayStubByIdAsync(documentId.ToString());
        return result;
    }

    public async Task GeneratePayStubPdfs(int payrollId, int companyId)
    {
        var today = DateTime.UtcNow;
        var payroll = _context.Payroll.Include(e => e.PayrollSchedule).Include(e => e.Company)
            .ThenInclude(e => e.Zipcode).ThenInclude(e => e.City).ThenInclude(e => e.State)
            .FirstOrDefault(e => e.Id == payrollId && e.CompanyId == companyId)
            ?? throw new AppException("Payroll not found");

        _currentYear = payroll.ScheduledRunDate.Year;

        var docId = new Guid();
        var state = payroll.Company.Zipcode.City.State;
        _stateId = state.Id;
        _countryId = state.CountryId;

        var schedule = (PayrollScheduleTypeModel)payroll.PayrollSchedule.PayrollScheduleTypeId;

        if (schedule == PayrollScheduleTypeModel.Weekly) { _numberOfPaymentsInYear = 52; }
        else if (schedule == PayrollScheduleTypeModel.Monthly) { _numberOfPaymentsInYear = 12; }
        else if (schedule == PayrollScheduleTypeModel.SemiMonthly) { _numberOfPaymentsInYear = 24; }
        else if (schedule == PayrollScheduleTypeModel.Biweekly) { _numberOfPaymentsInYear = 26; }

        var payStubs = _context.PayStub.Include(e => e.OtherMoneyReceived).Include(e => e.Employment)
            .ThenInclude(e => e.Person).ThenInclude(e => e.Zipcode).ThenInclude(e => e.City)
            .Where(e => e.Employment.PayRateBasisId!=null && e.PayrollId == payrollId).ToList()
            ?? throw new AppException("Pay stubs not found");

        var additionalMoneyReceived = new List<AdditionalOtherMoneyReceived>();

        foreach (var payStub in payStubs)
        {
            //To Do: We should add a validation that an income tax is created or not.
            //TO DO: Cerate paystub only for those employees a paystub is not created
            try
            {
                //Calculate Federal and State Taxes
                var additionalMoneyReceivedIds = payStub.OtherMoneyReceived?.AdditionalOtherMoneyReceivedId;
                additionalMoneyReceived = _context.AdditionalOtherMoneyReceived.Where(e => additionalMoneyReceivedIds.Contains(e.Id)).ToList();
                
                var paymentExemptFromFutaTax = additionalMoneyReceived.Where(e => e.ExemptFromFutaTaxTypeId > 0);
               
                payStub.NetPay = payStub.GrossPay;
                _newIncomeTaxesToHold = new();
                var is2percentShareHolder = payStub.Employment.Person.IsTwopercentshareholder;
                if (!payStub.Employment.IsTenNinetyNine && !is2percentShareHolder) //Phase 2: probably add a second flag to show if this individual still want to get paid like any other employee 
                {
                     await CalculateFederalWitholdingsAsync(payStub, additionalMoneyReceived.Where(e => e.ExemptFromFutaTaxTypeId > 0).ToList());
                     await CalculateStateWitholdingAsync(payStub, additionalMoneyReceived.Where(e => e.ExemptFromFutaTaxTypeId > 0).ToList());
                    _context.IncomeTax.AddRange(_newIncomeTaxesToHold);
                    _context.SaveChanges();                  
                }
                //Create PDFs 
                docId = await _documentManager.CreatePayStubPdfAsync(payStub, additionalMoneyReceived, _newIncomeTaxesToHold.ToList());
                var doc = new Document { Id = docId, DocumentTypeId = (int)DocumentTypeModel.PayStub, FileName = string.Format("PayStub-" + payStub.Id + "-" + today.ToString("yyyyMMddmmss") + ".pdf") };
                _context.Document.Add(doc);

                payStub.DocumentId = docId;

                _context.PayStub.Update(payStub);
                _context.SaveChanges();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }
        }
        payroll.EmployeesOwed = payStubs.Sum(e => e.NetPay);
        payroll.StatesOwed = _newIncomeTaxesToHold.Where(e => e.IncomeTaxType.StateId != null).Select(e => e.Amount).Sum();
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
        if (payStub.OtherMoneyReceived != dto.OtherMoneyReceived)
        {
            dto.OtherMoneyReceived = payStub.OtherMoneyReceived;
        }
        if (payStub.RegularHoursWorked != dto.RegularHoursWorked)
        {
            dto.RegularHoursWorked = payStub.RegularHoursWorked;
        }
        if (payStub.OverTimeHoursWorked != dto.OverTimeHoursWorked)
        {
            dto.OverTimeHoursWorked = dto.OverTimeHoursWorked;
        }
        if (payStub.GrossPay != payStub.GrossPay)
        {
            dto.GrossPay = payStub.GrossPay;
        }
        if (payStub.NetPay != dto.NetPay)
        {
            dto.NetPay = payStub.NetPay;
        }
        if (payStub.OverTimePay != dto.OverTimePay)
        {
            dto.OverTimePay = payStub.OverTimePay;
        }
        if (payStub.RegularPay != dto.RegularPay)
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
        var dto = _context.PayStub.Include(e => e.Payroll).Include(e => e.Employment).FirstOrDefault(e => e.Id == id);
        return dto;
    }

    private async Task<PayStub> CalculateFederalWitholdingsAsync(PayStub stub, List<AdditionalOtherMoneyReceived> additionalMoney)
    {
        //First let's get tax rates and withholding forms
        var incomeTaxRates = _staticDataService.GetIncomeTaxRatesByCountryIdAndPayrollYear(_countryId, _currentYear);
        var previousIncomeTaxes = _context.IncomeTax.OrderByDescending(e => e.CreatedDate).Where(
            e => e.CreatedDate.Year == _currentYear && !e.PayStub.IsCancelled && e.PayStub.EmploymentId == stub.EmploymentId);

        var withHolingForms = _context.EmployeeWithholding.Include(e => e.Field).Where(e => e.EmploymentId == stub.EmploymentId 
        && e.Field.WithholdingYear == _currentYear).ToList();

        // we use w4unsigned because the original employee withholding document is w4-unsigned  
        //TO DO: Do the necessary investigation and change the DocumentType from WFourUnsigned to WFour
        var w4Form = withHolingForms.Where(e => e.Field.DocumentTypeId == (int)DocumentTypeModel.WFourUnsigned).ToList(); 

        var fedFilingStatus = FilingStatusTypeModel.SingleOrMarriedFilingSeparately;

        var w4OneA = w4Form.Find(e => e.Field.FieldName == "1a") ?? new EmployeeWithholding { FieldValue = "" };
        var w4OneC = w4Form.Find(e => e.Field.FieldName == "1c") ?? new EmployeeWithholding { FieldValue = "Single" };

        if (!Enum.TryParse(w4OneC?.FieldValue, out fedFilingStatus))
        {
            fedFilingStatus = FilingStatusTypeModel.SingleOrMarriedFilingSeparately;
            _logger.LogError($"Invalid filing status [W-4 OneC] for employee [{stub.Employment.Person.FirstName} {stub.Employment.Person.LastName}]");
        }

        var w4TwoC = w4Form.Find(e => e.Field.FieldName == "2c") ?? new EmployeeWithholding { FieldValue = "0" };
        var w4Three = w4Form.Find(e => e.Field.FieldName == "3")?? new EmployeeWithholding { FieldValue="0" };
        var w4FourA = w4Form.Find(e => e.Field.FieldName == "4a") ?? new EmployeeWithholding { FieldValue = "0" };
        var w4FourB = w4Form.Find(e => e.Field.FieldName == "4b") ?? new EmployeeWithholding { FieldValue = "0" };
        var w4FourC = w4Form.Find(e => e.Field.FieldName == "4c") ?? new EmployeeWithholding { FieldValue = "0" };

        var employeeFederalRates = incomeTaxRates.Where(e => e.IncomeTaxType.ForEmployee &&
        e.EffectiveYear == _currentYear && e.IncomeTaxType.StateId == null &&
        (e.FilingStatusId == (int)fedFilingStatus || e.FilingStatusId == null)).ToList();

        var employerFederalRates = incomeTaxRates.Where(e => !e.IncomeTaxType.ForEmployee &&
        e.EffectiveYear == _currentYear && e.IncomeTaxType.StateId == null).ToList();

        //************************Lets calculate taxes for employees now*******************//

        //Calculate Federal Tax to withhold
        var hasMultipleJobs = w4TwoC!= null && !string.IsNullOrWhiteSpace(w4TwoC.FieldValue);

        var reimbursement = stub.OtherMoneyReceived?.Reimbursement;        
        var grossPayAfterReimbursement = stub.GrossPay - (reimbursement?? 0);

        var annualAmount = grossPayAfterReimbursement * _numberOfPaymentsInYear + double.Parse(w4FourA.FieldValue);

        var deductionMarriedFlilingJointlyValue = _context.CountrySpecificFieldValue.
                                                   FirstOrDefault(cf => cf.CountrySpecificField.FieldName == "DeductionsMarriedFlilingJointly" && cf.EffectiveYear == _currentYear);
        var deductionForSingle = _context.CountrySpecificFieldValue.
                                                  FirstOrDefault(cf => cf.CountrySpecificField.FieldName == "DeductionsSingleOrMarriedFilingSeparately" && cf.EffectiveYear == _currentYear);
        var reportingStatus = w4OneC.FieldValue.Contains("Jointly") ? Convert.ToDouble(deductionMarriedFlilingJointlyValue.FieldValue) : Convert.ToDouble(deductionForSingle.FieldValue);

        var deduction = double.Parse(w4FourB.FieldValue) * (hasMultipleJobs ? 0 : reportingStatus);
        var adjustedAnnualWageAmount = annualAmount - deduction;

        var rate = employeeFederalRates.FirstOrDefault(e => e.IncomeTaxType.Name.StartsWith("Federal") && 
        e.Minimum < Math.Round(adjustedAnnualWageAmount) && e.Maximum > Math.Round(adjustedAnnualWageAmount));

        if (rate != null)
        {
            var previousIncomeTax = previousIncomeTaxes.FirstOrDefault(e => e.IncomeTaxTypeId == rate.IncomeTaxTypeId);

            var tentativeWithholdingAmt = ((adjustedAnnualWageAmount - rate.Minimum) * rate.Rate +
                (rate.TentativeAmount ?? 0)) / _numberOfPaymentsInYear;

            //Credits
            var credits = double.Parse(w4Three.FieldValue) / _numberOfPaymentsInYear;
            tentativeWithholdingAmt = Math.Max(tentativeWithholdingAmt - credits, 0);

            var finalWithholdingAmount = tentativeWithholdingAmt + double.Parse(w4FourC.FieldValue);
            stub.NetPay = stub.NetPay - finalWithholdingAmount;
            _newIncomeTaxesToHold.Add(
                new IncomeTax
                {
                    Amount = Math.Round(finalWithholdingAmount),
                    IncomeTaxTypeId = rate.IncomeTaxTypeId,
                    PayStubId = stub.Id,
                    YtdAmount = finalWithholdingAmount + previousIncomeTax?.YtdAmount ?? 0
                }
            );
        }

        //Other Federal Withholdings
        var otherFederalWithholdings = employeeFederalRates.Where(e => !e.IncomeTaxType.Name.StartsWith("Federal") &&
        e.Minimum < Math.Round(stub.GrossPay) && e.Maximum > Math.Round(stub.GrossPay)).ToList();

        foreach (var item in otherFederalWithholdings)
        {
            var previous = previousIncomeTaxes.FirstOrDefault(e => e.IncomeTaxTypeId == item.IncomeTaxTypeId);

            if (item.IncomeTaxType.Name == "Additional Medicare")
            {
                var addlAmount = stub.AmountSubjectToAdditionalMedicareTax * item.Rate;
                stub.NetPay = stub.NetPay - addlAmount;

                _newIncomeTaxesToHold.Add(
                    new IncomeTax
                    {
                        Amount = addlAmount,
                        IncomeTaxTypeId = item.IncomeTaxTypeId,
                        PayStubId = stub.Id,
                        YtdAmount = addlAmount + previous?.YtdAmount ?? 0
                    });
                continue;
            }

            var amount = grossPayAfterReimbursement * item.Rate; 
            stub.NetPay = stub.NetPay - amount;

            _newIncomeTaxesToHold.Add(
                new IncomeTax
                {
                    Amount = amount,
                    IncomeTaxTypeId = item.IncomeTaxTypeId,
                    PayStubId = stub.Id,
                    YtdAmount = amount + previous?.YtdAmount ?? 0
                }
            );
        }


        //************************Lets calculate taxes for employer now*******************//

        var employerRates = employerFederalRates.Where(e => e.Minimum < Math.Round(grossPayAfterReimbursement) && e.Maximum > Math.Round(grossPayAfterReimbursement));
        var paymentExemptFromFuta = additionalMoney.Where(e => e.ExemptFromFutaTaxTypeId > 0).Sum(e => e.Amount);
        var paymentExemptFromSocialAndMedi = additionalMoney.Where(e =>
        e.ExemptFromFutaTaxTypeId == (int)ExemptFromFutaTaxTypeModel.DependentCare ||
        e.ExemptFromFutaTaxTypeId == (int)ExemptFromFutaTaxTypeModel.GroupTermLifeInsurance ||
        e.ExemptFromFutaTaxTypeId == (int)ExemptFromFutaTaxTypeModel.RetirementOrPension).Sum(e => e.Amount);

        foreach (var item in employerRates)
        {
            var previous = previousIncomeTaxes.FirstOrDefault(e => e.IncomeTaxTypeId == item.IncomeTaxTypeId);

            switch (item.IncomeTaxType.Name)
            {
                case "FUTA":
                    var futaPay = Math.Max(item.Maximum - stub.YtdGrossPay - paymentExemptFromFuta, 0);
                    var futaTax = futaPay * item.Rate;

                    _newIncomeTaxesToHold.Add(
                        new IncomeTax
                        {
                            Amount = futaTax,
                            IncomeTaxTypeId = item.IncomeTaxTypeId,
                            PayStubId = stub.Id,
                            YtdAmount = futaTax + previous?.YtdAmount ?? 0
                        });
                    break;

                case "Social Security":
                case "Medicare":
                    var personInfo =  _personService.GetPersonById(stub.Employment.PersonId);
                    var isMinor = false;
                    if (personInfo.DateOfBirth.HasValue)
                    {
                       isMinor = _personService.IsEmployeeMinor(personInfo.DateOfBirth.Value);
                    }
                    if (!isMinor)
                    {
                        var ssOrMediPay = Math.Max(item.Maximum - stub.YtdGrossPay - paymentExemptFromSocialAndMedi, 0);
                        var ssOrMediTax = ssOrMediPay * item.Rate;
                        var daysOffPayAmount = 200;
                        var daysOffPayAmountTax = daysOffPayAmount * item.Rate ;

                        ssOrMediTax = ssOrMediTax - daysOffPayAmountTax;
                        _newIncomeTaxesToHold.Add(new IncomeTax
                            {
                                Amount = ssOrMediTax,
                                IncomeTaxTypeId = item.IncomeTaxTypeId,
                                PayStubId = stub.Id,
                                YtdAmount = ssOrMediTax + previous?.YtdAmount ?? 0
                            });
                    }
                    break;
                default:
                    break;
            }
        }

        return stub;
    }

    private async Task<PayStub> CalculateStateWitholdingAsync(PayStub stub, List<AdditionalOtherMoneyReceived> additionalMoney)
    {
        try
        {
            var stateId = stub.Payroll.Company.Zipcode.City.StateId;

            var incomeTaxRates = _staticDataService.GetIncomeTaxRatesByCountryIdAndPayrollYear(_countryId, _currentYear);
            incomeTaxRates= incomeTaxRates.Where(e => e.IncomeTaxType.StateId != null && e.IncomeTaxType.StateId == stateId).ToList();
            
            var withHolingForms = _context.EmployeeWithholding.Where(e => e.Field.StateId != null
                && e.EmploymentId == stub.EmploymentId && e.Field.WithholdingYear == _currentYear);

            var nc4 = withHolingForms.Where(e => e.Field.DocumentTypeId == (int)DocumentTypeModel.NCFourUnsigned).ToList();
            var ncAnnualStandardDeductions = _staticDataService.GetStateStandardDeductionsByStateId(_stateId, _currentYear);
            var ncFilingStatus = FilingStatusTypeModel.SingleOrMarriedFilingSeparately;

            var ncOne = nc4.Find(e => e.Field.FieldName == "totalnumberofallowances")??
                new EmployeeWithholding{ FieldValue = "0" };
            var ncTwo = nc4.Find(e => e.Field.FieldName == "additionalamount") ?? 
                new EmployeeWithholding { FieldValue = "0" };            
            var status = nc4.Find(e => e.Field.FieldName == "maritalstatus")??
                new EmployeeWithholding { FieldValue = "Single" };

            if (!Enum.TryParse(status.FieldValue, out ncFilingStatus))
            {
                ncFilingStatus = FilingStatusTypeModel.SingleOrMarriedFilingSeparately;
                _logger.LogError($"Invalid filing status [NC-4 Marital Status] for employee"
                    + $"[{stub.Employment.Person.FirstName} {stub.Employment.Person.LastName}]");
            }

            var annualDeduction = ncAnnualStandardDeductions.First(e => e.FilingStatusId == (int)ncFilingStatus);

            var employeeStateRates = incomeTaxRates.Where(e => e.IncomeTaxType.ForEmployee).ToList();
            var employerStateRates = incomeTaxRates.Where(e => !e.IncomeTaxType.ForEmployee).ToList();

            var reimbursement = stub.OtherMoneyReceived?.Reimbursement;
            var grossPayAfterReimbursement = stub.GrossPay - (reimbursement?? 0);

            //calculate employee taxes
            foreach (var item in employeeStateRates)
            {
                var previous = _context.IncomeTax.Include(i => i.IncomeTaxType).OrderByDescending(e => e.Id).FirstOrDefault(e =>
                !e.PayStub.IsCancelled && e.IncomeTaxTypeId == item.IncomeTaxTypeId && e.PayStub.EmploymentId == stub.EmploymentId);

                //Calculate Tax
                var annualWage = grossPayAfterReimbursement * _numberOfPaymentsInYear;
                var allowance = 2500 * double.Parse(ncOne.FieldValue); 
                var deduction = allowance + annualDeduction.Amount;
                var netAnnualWage = Math.Max( annualWage - deduction,0);
                var netAnnualTax = netAnnualWage * item.Rate;
                var netCurrentTax = netAnnualTax / _numberOfPaymentsInYear;

                stub.NetPay = stub.NetPay - netCurrentTax;

                _newIncomeTaxesToHold.Add(
                        new IncomeTax
                        {
                            Amount = netCurrentTax,
                            IncomeTaxTypeId = item.IncomeTaxTypeId,
                            PayStubId = stub.Id,
                            YtdAmount = netCurrentTax + Math.Max(previous?.YtdAmount ?? 0,0)
                        }
                    );
            }

            //Calculate employer taxes now
            foreach (var item in employerStateRates)
            {
                var previous = _context.IncomeTax.OrderByDescending(e => e.Id).FirstOrDefault(e =>
                !e.PayStub.IsCancelled && e.IncomeTaxTypeId == item.IncomeTaxTypeId && e.PayStub.EmploymentId == stub.EmploymentId);

                if (item.IncomeTaxType.Name == "SUTA")
                {
                    var paymentExemptFromSuta = additionalMoney.Where(e => e.ExemptFromFutaTaxTypeId > 0).Sum(e => e.Amount);
                    var sutaPay = Math.Max(item.Maximum - stub.YtdGrossPay, 0);
                    var sutaTax = Math.Round(sutaPay * item.Rate,2);

                    _newIncomeTaxesToHold.Add(
                        new IncomeTax
                        {
                            Amount = sutaTax,
                            IncomeTaxTypeId = item.IncomeTaxTypeId,
                            PayStubId = stub.Id,
                            YtdAmount = sutaTax + previous?.YtdAmount ?? 0
                        });
                    continue;
                }
            }

            return stub;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Cannot generate Pay Stub for EmploymentId: {stub.EmploymentId}. {ex.Message}");
            return null;
        }   
    }
}

