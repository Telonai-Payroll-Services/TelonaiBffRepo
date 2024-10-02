namespace TelonaiWebApi.Services;

using AutoMapper;
using BCrypt.Net;
using TelonaiWebApi.Entities;
using TelonaiWebApi.Helpers;
using TelonaiWebApi.Models;

public interface IDataService<Tmodel, Tdto>
{
    IList<Tmodel> Get();
    Tmodel GetById(int id);
    Task<Tdto> CreateAsync(Tmodel model);
    Task UpdateAsync(int id, Tmodel model);
    Task DeleteAsync(int id);
}