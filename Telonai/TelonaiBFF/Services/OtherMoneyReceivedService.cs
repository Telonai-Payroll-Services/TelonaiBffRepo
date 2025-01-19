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
    Task<bool> CreateOrUpdate(int paystubId, OtherMoneyReceivedModel model);
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

    public async Task<bool> CreateOrUpdate(int paystubId, OtherMoneyReceivedModel model)
    {
        var currentPayStub = GetPayStub(paystubId) ?? throw new KeyNotFoundException("PayStub not found");
        var companyId = currentPayStub.Payroll.CompanyId;
        _scopedAuthorization.ValidateByCompanyId(_httpContextAccessor.HttpContext.User, AuthorizationType.Admin, companyId);

        currentPayStub.GrossPay += model.CreditCardTips + model.CashTips + model.Reimbursement + model.AdditionalOtherMoneyReceived?.Sum(e => e.Amount) ?? 0;

        var previousPayStub = _context.PayStub.Include(e => e.OtherMoneyReceived).Include(e => e.Payroll).FirstOrDefault(e => e.Id < currentPayStub.Id
        && e.Payroll.CompanyId == companyId);

        //First get the additional money received. Note that we intentionally don't calculate the ytd for additional money received.
        //because the ytd is not displayed to the user at all. Instead, we get the total additional money received for each pay stub
        List<AdditionalOtherMoneyReceived> currentAdditionalMoney = null;
        if (model.AdditionalOtherMoneyReceived != null)
        {
            currentAdditionalMoney = _mapper.Map<List<AdditionalOtherMoneyReceived>>(model.AdditionalOtherMoneyReceived);
            _context.AdditionalOtherMoneyReceived.AddRange(currentAdditionalMoney);
            _context.SaveChanges();
        }

        //Now add the other-money-received passed as parameter. If there is existing one, cancel it
        var dtoNewOtherMoney = _mapper.Map<OtherMoneyReceived>(model);
        dtoNewOtherMoney.YtdCreditCardTips = previousPayStub?.OtherMoneyReceived?.YtdCreditCardTips > 0 ? previousPayStub.OtherMoneyReceived.YtdCreditCardTips
            + dtoNewOtherMoney.CreditCardTips : dtoNewOtherMoney.CreditCardTips;

        dtoNewOtherMoney.YtdCashTips = previousPayStub?.OtherMoneyReceived?.YtdCashTips > 0 ? previousPayStub.OtherMoneyReceived.YtdCashTips +
            dtoNewOtherMoney.CreditCardTips : dtoNewOtherMoney.CreditCardTips;

        dtoNewOtherMoney.YtdReimbursement = previousPayStub?.OtherMoneyReceived?.YtdReimbursement > 0 ? previousPayStub.OtherMoneyReceived.YtdReimbursement
            + dtoNewOtherMoney.CreditCardTips : dtoNewOtherMoney.CreditCardTips;

        if (currentAdditionalMoney.Any())
            dtoNewOtherMoney.AdditionalOtherMoneyReceivedId = currentAdditionalMoney.Select(e => e.Id).ToArray();

        _context.OtherMoneyReceived.Add(dtoNewOtherMoney);
        _context.SaveChanges();

        currentPayStub.OtherMoneyReceivedId = dtoNewOtherMoney.Id;
        _context.PayStub.Update(currentPayStub);

        return await _context.SaveChangesAsync() > 0;
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

