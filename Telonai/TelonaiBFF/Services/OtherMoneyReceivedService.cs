namespace TelonaiWebApi.Services;

using Amazon.Runtime.Internal;
using AutoMapper;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System;
using TelonaiWebApi.Entities;
using TelonaiWebApi.Helpers;
using TelonaiWebApi.Models;

public interface IOtherMoneyReceivedService
{
    OtherMoneyReceivedModel GetById(int id);
    List<OtherMoneyReceivedModel> GetByPayStubId(int payStubId, out int jobId);
    void Update(int payStubId, OtherMoneyReceivedModel model);
    void Create(int payStubId, OtherMoneyReceivedModel model);
    void Delete(int id);
}

public class OtherMoneyReceivedService : IOtherMoneyReceivedService
{
    private DataContext _context;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IScopedAuthorization _scopedAuthorization;

    public OtherMoneyReceivedService(DataContext context, IMapper mapper, IHttpContextAccessor httpContextAccessor, IScopedAuthorization scopedAuthorization)
    {
        _context = context;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
        _scopedAuthorization = scopedAuthorization;
    }

    public OtherMoneyReceivedModel GetById(int id)
    {
        var obj = _context.OtherMoneyReceived.Where(e => e.Id==id);
        
        if (obj == null)
            return null;

        return _mapper.Map<OtherMoneyReceivedModel>(obj);
    }
    public List<OtherMoneyReceivedModel> GetByPayStubId(int payStubId,  out int companyId )
    {
        var payStub = _context.PayStub.Include(e=>e.OtherMoneyReceived).Include(e=>e.Payroll)
                                      .Where(e=>e.Id== payStubId);

        companyId = payStub.Select(e=>e.Payroll.CompanyId).First();

        var obj = payStub.Select(e => e.OtherMoneyReceived)?.ToList();
        if (obj == null)
            return null;
        return  _mapper.Map<List<OtherMoneyReceivedModel>>(obj.ToList());
    }

    public void Create(int paystubId, OtherMoneyReceivedModel model)
    {
        var dtoPayStub = GetPayStub(paystubId) ?? throw new KeyNotFoundException("PayStub not found");
        _scopedAuthorization.ValidateByCompanyId(_httpContextAccessor.HttpContext.User, AuthorizationType.Admin, dtoPayStub.Payroll.CompanyId);

        dtoPayStub.GrossPay += model.CreditCardTips + model.CashTips + model.AdditionalOtherMoneyReceived?.Sum(e => e.Amount) ?? 0; 

        var objOtherMoney = _mapper.Map<OtherMoneyReceived>(model);


        if (model.AdditionalOtherMoneyReceived != null)
        {
            var objAdditional = _mapper.Map<List<AdditionalOtherMoneyReceived>>(model.AdditionalOtherMoneyReceived);
            objAdditional.ForEach(e => e.PayStubId = paystubId);
            _context.AdditionalOtherMoneyReceived.AddRange(objAdditional);
            _context.SaveChanges();
            objOtherMoney.AdditionalOtherMoneyReceivedId = objAdditional.Select(e => e.Id).ToArray();
        }
        _context.OtherMoneyReceived.Add(objOtherMoney);
        _context.SaveChanges();

        dtoPayStub.OtherMoneyReceivedId=objOtherMoney.Id;
        _context.PayStub.Update(dtoPayStub);
        _context.SaveChanges();
    }

    public void Update(int payStubId, OtherMoneyReceivedModel model)
    {
        var dtoPayStub = GetPayStub(payStubId) ?? throw new KeyNotFoundException("PayStub not found");

        dtoPayStub.GrossPay += model.CreditCardTips + model.CashTips + model.AdditionalOtherMoneyReceived?.Sum(e=>e.Amount)?? 0;

        var obj = GetOtherMoneyReceived(dtoPayStub.OtherMoneyReceivedId.Value);

        if (obj == null)
        {
            obj = _mapper.Map<OtherMoneyReceived>(model);
            if (model.AdditionalOtherMoneyReceived != null)
            {
                var obj2 = _mapper.Map<List<AdditionalOtherMoneyReceived>>(model.AdditionalOtherMoneyReceived);
                _context.AdditionalOtherMoneyReceived.AddRange(obj2);
                _context.SaveChanges();
                obj.AdditionalOtherMoneyReceivedId = obj2.Select(e => e.Id).ToArray();
            }
            _context.OtherMoneyReceived.Add(obj);
            _context.SaveChanges();
        }
        else
        {
            //cancell the existing one and create a new one
            obj.IsCancelled = true;
            _context.OtherMoneyReceived.Update(obj);

            var objNew = _mapper.Map<OtherMoneyReceived>(model);
            if (model.AdditionalOtherMoneyReceived != null)
            {
                var obj2New = _mapper.Map<List<AdditionalOtherMoneyReceived>>(model.AdditionalOtherMoneyReceived);
                _context.AdditionalOtherMoneyReceived.AddRange(obj2New);
                _context.SaveChanges();
                objNew.AdditionalOtherMoneyReceivedId = obj2New.Select(e => e.Id).ToArray();
            }

            _context.OtherMoneyReceived.Add(objNew);          
            _context.SaveChanges();
        }
        _context.PayStub.Update(dtoPayStub);
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
        var dto = _context.PayStub.Include(e=>e.OtherMoneyReceived).Include(e=>e.Payroll).FirstOrDefault(e=>e.Id==id);
        return dto;
    }
}

