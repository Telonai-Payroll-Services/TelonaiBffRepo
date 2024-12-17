using AutoMapper;
using iTextSharp.text.pdf.parser.clipper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.Design;
using TelonaiWebApi.Entities;
using TelonaiWebApi.Helpers;
using TelonaiWebApi.Models;

namespace TelonaiWebApi.Services
{
    public interface IMobileAppVersionService
    {
        Task<MobileAppVersion> CreateAsync(MobileAppVersionModel model);
        Task<IList<MobileAppVersionModel>> GetAll();
        Task UpdateAsync(int id, MobileAppVersionModel model);
        Task<MobileAppVersionModel> GetLatestAppVersion(int platform);
        Task DeleteAsync(int id);
    }

    public class MobileAppVersionService : IMobileAppVersionService
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public MobileAppVersionService(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<MobileAppVersionModel> GetLatestAppVersion(int platform)
        {
            var versions = _context.MobileAppVersion.Where((version) => version.Platform == platform).ToList();
            versions.OrderByDescending(ver => ver.Id).ToList();// createdDate
            return _mapper.Map<MobileAppVersionModel>(versions.FirstOrDefault());
        }

        public async Task<MobileAppVersion> CreateAsync(MobileAppVersionModel model)
        {
            var version = _context.MobileAppVersion.FirstOrDefault(e => e.AppVersion == model.AppVersion && e.BuildNumber == model.BuildNumber && e.Platform == (int) model.Platform);
            var result = _mapper.Map<MobileAppVersion>(model);

            if (version == null)
            {
                _context.MobileAppVersion.Add(result);
                await _context.SaveChangesAsync();
            }
            return result;
        }

        public async Task<IList<MobileAppVersionModel>> GetAll()
        {
            var appVersions = _context.MobileAppVersion.ToList();
            return _mapper.Map<IList<MobileAppVersionModel>>(appVersions);
        }

        public async Task UpdateAsync(int id, MobileAppVersionModel model)
        {
            var result = await _context.MobileAppVersion.FindAsync(id) ?? throw new AppException("Mobile app version not found");
            _mapper.Map(model, result);
            _context.MobileAppVersion.Update(result);
            await _context.SaveChangesAsync();
        }
        
        public async Task DeleteAsync(int id)
        {
            var result = await _context.MobileAppVersion.FindAsync(id) ?? throw new AppException("Mobile app version not found");
            _context.MobileAppVersion.Remove(result);
            _context.SaveChanges();
        }

        public IList<MobileAppVersionModel> Get()
        {
            var dto = _context.MobileAppVersion;
            return _mapper.Map<IList<MobileAppVersionModel>>(dto);
        }

        public MobileAppVersionModel GetById(int id)
        {
            throw new NotImplementedException();
        }
    }
}
