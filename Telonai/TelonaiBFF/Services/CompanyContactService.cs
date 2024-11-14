using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TelonaiWebApi.Entities;
using TelonaiWebApi.Helpers;
using TelonaiWebApi.Models;

namespace TelonaiWebApi.Services
{
    public interface ICompanyContactService
    {
        public Task<CompanyContactModel> GetByCompanyId(int companyId);

        public Task<bool> SaveCompanyContact(CompanyContactModel companyContact);
    }

    public class CompanyContactService : ICompanyContactService
    {
        private readonly IMapper _mapper;
        private readonly DataContext _context;
        public CompanyContactService(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public async Task<CompanyContactModel> GetByCompanyId(int companyId)
        {
            var companyContact = await _context.CompanyContact.Include(p => p.Person).ThenInclude(c => c.Company).FirstOrDefaultAsync(c => c.CompanyId == companyId);
            var companyContactModel = _mapper.Map<CompanyContactModel>(companyContact);
            return companyContactModel;
        }

        public async Task<bool> SaveCompanyContact(CompanyContactModel companyContact)
        {
            var companyContactEntity = _mapper.Map<CompanyContact>(companyContact);
            if (companyContactEntity != null)
            {
                await _context.CompanyContact.AddAsync(companyContactEntity);
                if (await _context.SaveChangesAsync() > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
    }
}

