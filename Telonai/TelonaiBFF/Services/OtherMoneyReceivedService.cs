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
    Task<bool> Update(int payStubId, OtherMoneyReceivedModel model);
    Task<bool> Create(int payStubId, OtherMoneyReceivedModel model);
    Task<bool> Delete(int id);
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
        var obj = _context.OtherMoneyReceived.Find(id);
        
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

    public async Task<bool> Create(int paystubId, OtherMoneyReceivedModel model)
    {
        var currentPayStub = GetPayStub(paystubId) ?? throw new KeyNotFoundException("PayStub not found");
        var companyId = currentPayStub.Payroll.CompanyId;
        _scopedAuthorization.ValidateByCompanyId(_httpContextAccessor.HttpContext.User, AuthorizationType.Admin, companyId);

        currentPayStub.GrossPay += model.CreditCardTips + model.CashTips + model.AdditionalOtherMoneyReceived?.Sum(e => e.Amount) ?? 0;

        var previousPayStub = _context.PayStub.Include(e => e.OtherMoneyReceived).Include(e => e.Payroll).FirstOrDefault(e => e.Id < currentPayStub.Id
        && e.Payroll.CompanyId == companyId);

        //Now get the additional money received. Not that we avoided calculating the YTD intentionally.
        //We will not display on the paystubs the YTD for each additional money received.
        //Instead, we display the total Additional money received for the current paystub

        List<AdditionalOtherMoneyReceived> currentAdditional = null;

        if (model.AdditionalOtherMoneyReceived != null)
        {
            currentAdditional = _mapper.Map<List<AdditionalOtherMoneyReceived>>(model.AdditionalOtherMoneyReceived);
            _context.AdditionalOtherMoneyReceived.AddRange(currentAdditional);
            _context.SaveChanges();
        }

        OtherMoneyReceived dtoOtherMoney = null;

        if (currentPayStub.OtherMoneyReceived == null)
        {
            dtoOtherMoney = _mapper.Map<OtherMoneyReceived>(model);
            dtoOtherMoney.YtdCreditCardTips = previousPayStub?.OtherMoneyReceived?.YtdCreditCardTips ?? dtoOtherMoney.CreditCardTips;
            dtoOtherMoney.YtdCashTips = previousPayStub?.OtherMoneyReceived?.YtdCashTips ?? dtoOtherMoney.CreditCardTips;
            dtoOtherMoney.YtdReimbursement = previousPayStub?.OtherMoneyReceived?.YtdReimbursement ?? dtoOtherMoney.CreditCardTips;

            if (currentAdditional.Any())
                dtoOtherMoney.AdditionalOtherMoneyReceivedId = currentAdditional.Select(e => e.Id).ToArray();

            _context.OtherMoneyReceived.Add(dtoOtherMoney);
        }
        else
        {
            dtoOtherMoney = currentPayStub.OtherMoneyReceived;
            dtoOtherMoney.YtdCreditCardTips = previousPayStub?.OtherMoneyReceived?.YtdCreditCardTips ?? dtoOtherMoney.CreditCardTips;
            dtoOtherMoney.YtdCashTips = previousPayStub?.OtherMoneyReceived?.YtdCashTips ?? dtoOtherMoney.CreditCardTips;
            dtoOtherMoney.YtdReimbursement = previousPayStub?.OtherMoneyReceived?.YtdReimbursement ?? dtoOtherMoney.CreditCardTips;

            if (currentAdditional.Any())
                dtoOtherMoney.AdditionalOtherMoneyReceivedId = currentAdditional.Select(e => e.Id).ToArray();

            _context.OtherMoneyReceived.Update(dtoOtherMoney);
        }
       
        _context.SaveChanges();

        currentPayStub.OtherMoneyReceivedId= dtoOtherMoney.Id;
        _context.PayStub.Update(currentPayStub);
        return  await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> Update(int payStubId, OtherMoneyReceivedModel model)
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
            else
            {
                _context.OtherMoneyReceived.Add(obj);
                _context.SaveChanges();
            }
            
            _context.PayStub.Update(dtoPayStub);
            return await _context.SaveChangesAsync() > 0;
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
            else
            {
                _context.OtherMoneyReceived.Add(objNew);
                _context.SaveChanges();
            }
            _context.PayStub.Update(dtoPayStub);
            return await _context.SaveChangesAsync() > 0;
        }
       
    }

    public async Task<bool> Delete(int id)
    {
        var dto = GetOtherMoneyReceived(id);
        if (dto != null)
        {
            _context.OtherMoneyReceived.Remove(dto);
            await _context.SaveChangesAsync();
            return true;
        }
        else
        {
            return false;
        }
    }
   
    private OtherMoneyReceived GetOtherMoneyReceived(int id)
    {
        var dto = _context.OtherMoneyReceived.Find(id);
        return dto;
    }
    public PayStub GetPayStub(int id)
    {
        var dto = _context.PayStub.Include(e=>e.OtherMoneyReceived).Include(e=>e.Payroll).FirstOrDefault(e=>e.Id==id);
        return dto;
    }
}

