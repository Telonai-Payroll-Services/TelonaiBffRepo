using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TelonaiWebApi.Entities;
using TelonaiWebApi.Helpers;
using TelonaiWebApi.Models;

namespace TelonaiWebApi.Services
{
    public interface IIncomeTaxService
    {
        IList<IncomeTaxModel> GetByPayStubId(int payStubId);
    }
    public class IncomeTaxService : IIncomeTaxService
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public IncomeTaxService(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async  Task<IncomeTax> CreateAsync(IncomeTaxModel model)
        {
            var incomeTax = _mapper.Map<IncomeTax>(model);
            await _context.IncomeTax.AddAsync(incomeTax);
            _context.SaveChanges();
            return incomeTax;
        }

        public async Task DeleteAsync(int id)
        {
            var incomeTax = await _context.IncomeTax.FindAsync(id);
            if (incomeTax != null)
            {
                _context.Remove(incomeTax);
                await _context.SaveChangesAsync();
            }
        }

        public IList<IncomeTaxModel> GetByPayStubId(int payStubId)
        {
            List<IncomeTaxModel> incomeTaxModel = new List<IncomeTaxModel>();
            IList<IncomeTax> incomeTax =  _context.IncomeTax.Where(x=>x.PayStubId == payStubId).ToList();
            if(incomeTax != null)
            {
                incomeTaxModel = _mapper.Map<List<IncomeTaxModel>>(incomeTax);
                return incomeTaxModel;
            }
            else
            {
                return null;
            }
        }

        public async Task<IncomeTaxModel> GetById(int id)
        {
            var incomeTax = await _context.IncomeTax.FindAsync(id);
            var incomeTaxModel = _mapper.Map<IncomeTaxModel>(incomeTax);
            return incomeTaxModel;
        }


        public async Task<bool> UpdateAsync(int id, IncomeTaxModel model)
        {
            var incomeTax = await _context.IncomeTax.FindAsync(id);
            if (incomeTax != null)
            {
                if (incomeTax.IncomeTaxTypeId != model.IncomeTaxTypeId)
                {
                    incomeTax.IncomeTaxTypeId = model.IncomeTaxTypeId;
                }
                if (incomeTax.DepositedAmount != model.DepositedAmount)
                {
                    incomeTax.DepositedAmount = model.DepositedAmount;
                }
                if (incomeTax.PayStubId != model.PayStubId)
                {
                    incomeTax.PayStubId = model.PayStubId;
                }
                if (incomeTax.Amount != model.Amount)
                {
                    incomeTax.Amount = model.Amount;
                }
                if (incomeTax.YtdAmount != model.YtdAmount)
                {
                    incomeTax.YtdAmount = model.YtdAmount;
                }
                _context.Update(incomeTax);
                await _context.SaveChangesAsync();
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
