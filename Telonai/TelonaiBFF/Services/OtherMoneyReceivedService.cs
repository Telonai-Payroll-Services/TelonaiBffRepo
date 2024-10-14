namespace TelonaiWebApi.Services;

using Amazon.Runtime.Internal;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using TelonaiWebApi.Entities;
using TelonaiWebApi.Helpers;
using TelonaiWebApi.Models;

public interface IOtherMoneyReceivedService
{
    OtherMoneyReceivedModel GetById(int id);
    List<OtherMoneyReceivedModel> GetByPayrollId(int payrollId, out int jobId);
    void Update(List<OtherMoneyReceivedModel> models);
    void Update(OtherMoneyReceivedModel model);
    void Create(List<OtherMoneyReceivedModel> models);
    void Create(OtherMoneyReceivedModel model);
    void Delete(int id);
}

public class OtherMoneyReceivedService : IOtherMoneyReceivedService
{
    private DataContext _context;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IScopedAuthorization _scopedAuthrorization;

    public OtherMoneyReceivedService(DataContext context, IMapper mapper, IHttpContextAccessor httpContextAccessor, IScopedAuthorization scopedAuthrorization)
    {
        _context = context;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
        _scopedAuthrorization = scopedAuthrorization;
    }

    public OtherMoneyReceivedModel GetById(int id)
    {
        var obj = _context.OtherMoneyReceived.Include(e => e.PayStub).ThenInclude(e => e.Payroll).Where(e => e.Id==id);
        
        if (obj == null)
            return null;

        return _mapper.Map<OtherMoneyReceivedModel>(obj);
    }
    public List<OtherMoneyReceivedModel> GetByPayrollId(int payrollId,  out int companyId )
    {
        var payroll = _context.Payroll.Find(payrollId);
        companyId = payroll.CompanyId;

        var obj = _context.OtherMoneyReceived.Where(e=>e.PayStub.PayrollId==payrollId);
        if (obj == null)
            return null;
        return  _mapper.Map<List<OtherMoneyReceivedModel>>(obj.ToList());
    }

    public void Create(OtherMoneyReceivedModel model)
    {
        var dtoPayStub = GetPayStub(model.PayStubId) ?? throw new KeyNotFoundException("PayStub not found");
        _scopedAuthrorization.ValidateByCompanyId(_httpContextAccessor.HttpContext.User, AuthorizationType.Admin, dtoPayStub.Payroll.CompanyId);

        dtoPayStub.GrossPay += model.CreditCardTips + model.CashTips + model.OtherPay;
        var objOtherMoney = _mapper.Map<OtherMoneyReceived>(model);

        _context.OtherMoneyReceived.Add(objOtherMoney);  
        _context.PayStub.Update(dtoPayStub);
        _context.SaveChanges();
    }
    public void Create(List<OtherMoneyReceivedModel> models)
    {
        List<OtherMoneyReceived> list = new();
        List<PayStub> stubs = new();

        foreach (var model in models)
        {
            stubs.Add(GetPayStub(model.PayStubId) ?? throw new KeyNotFoundException("PayStub not found"));
            stubs.Last().GrossPay += model.CreditCardTips + model.CashTips + model.OtherPay;
            list.Add(_mapper.Map<OtherMoneyReceived>(model));
        }
        if (list.Count > 0)
        {
            _context.OtherMoneyReceived.AddRange(list);
            stubs.ForEach(e => _context.PayStub.Update(e));

            _context.SaveChanges();
        }
    }

    public void Update(OtherMoneyReceivedModel model)
    {
        var dtoPayStub = GetPayStub(model.PayStubId) ?? throw new KeyNotFoundException("PayStub not found");
        dtoPayStub.GrossPay += model.CreditCardTips + model.CashTips + model.OtherPay;

        var obj = GetOtherMoneyReceived(model.Id);
        if (obj == null)
        {
            obj = _mapper.Map<OtherMoneyReceived>(model);
            _context.OtherMoneyReceived.Add(obj);
            _context.PayStub.Update(dtoPayStub);

        }
        else
        {
            obj.CashTips = model.CashTips;
            obj.CreditCardTips = model.CreditCardTips;
            obj.OtherPay = model.OtherPay;
            obj.Note = model.Note;
            _context.OtherMoneyReceived.Update(obj);
            _context.PayStub.Update(dtoPayStub);
        }
        
        _context.SaveChanges();
    }
    public void Update(List<OtherMoneyReceivedModel> models)
    {
        foreach (var model in models)
        {
            var stub = GetPayStub(model.PayStubId) ?? throw new KeyNotFoundException("PayStub not found");
            stub.GrossPay += model.CreditCardTips + model.CashTips + model.OtherPay;

            var obj = GetOtherMoneyReceived(model.Id);
            if (obj == null)
            {
                obj = _mapper.Map<OtherMoneyReceived>(model);
                _context.OtherMoneyReceived.Add(obj);
                _context.PayStub.Update(stub);

            }
            else
            {
                obj.CashTips = model.CashTips;
                obj.CreditCardTips = model.CreditCardTips;
                obj.OtherPay = model.OtherPay;
                obj.Note = model.Note;
                _context.OtherMoneyReceived.Update(obj);
                _context.PayStub.Update(stub);
            }
        }
        _context.SaveChanges();
    }

    public void Delete(int id)
    {
        var dto = GetOtherMoneyReceived(id);
        _context.OtherMoneyReceived.Remove(dto);
        _context.SaveChanges();
    }
   
    private OtherMoneyReceived GetOtherMoneyReceived(int id)
    {
        var dto = _context.OtherMoneyReceived.Find(id);
        return dto;
    }
    private PayStub GetPayStub(int id)
    {
        var dto = _context.PayStub.Find(id);
        return dto;
    }
}

