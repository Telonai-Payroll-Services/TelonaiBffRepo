namespace TelonaiWebApi.Services;

using AutoMapper;
using BCrypt.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;
using System;
using System.ComponentModel.Design;
using TelonaiWebApi.Entities;
using TelonaiWebApi.Helpers;
using TelonaiWebApi.Models;

public interface IPayrollScheduleService
{    PayrollScheduleModel GetLatestByCompanyId(int id);
    List<PayrollScheduleModel> GetByCompanyId(int id);
    PayrollScheduleModel GetById(int id);
    void Create(PayrollScheduleModel model);
    void Update(int id, PayrollScheduleModel model);
    void Delete(int id);
}

public class PayrollScheduleService : IPayrollScheduleService
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;

    public PayrollScheduleService(DataContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
    public PayrollScheduleModel GetLatestByCompanyId(int id)
    {
        var obj = _context.PayrollSchedule.OrderByDescending(e=>e.StartDate).FirstOrDefault(e=>e.CompanyId == id &&
        (e.EndDate!=null || e.EndDate>  DateOnly.FromDateTime(DateTime.Now)));
        var result = _mapper.Map<PayrollScheduleModel>(obj);
        return result;
    }
    public List<PayrollScheduleModel> GetByCompanyId(int id)
    {
        var obj = _context.PayrollSchedule.OrderByDescending(e => e.StartDate).Where(e => e.CompanyId == id );
        var result = _mapper.Map<List<PayrollScheduleModel>>(obj);
        return result;
    }
    public PayrollScheduleModel GetById(int id)
    {
        var obj = GetPayrollSchedule(id);
        var result = _mapper.Map<PayrollScheduleModel>(obj);
        return result;
    }
    public void Create(PayrollScheduleModel model)
    {
        if (DateOnly.FromDateTime(model.StartDate) < DateOnly.FromDateTime(DateTime.Now))
        {
            throw new AppException("Payroll cannot be scheduled for the past");
        }
        if (model.EndDate.HasValue && (model.EndDate.Value <= model.StartDate))
        {
            throw new AppException("Invalid payroll start-date or end-date");
        }
        if (model.FirstRunDate < model.StartDate)
        {
            throw new AppException("Invalid payroll first-run date");
        }
        var now = DateOnly.FromDateTime(DateTime.Now);
        var currentSchedule = _context.PayrollSchedule.OrderByDescending(e=>e.StartDate).FirstOrDefault(e => e.CompanyId == model.CompanyId && (e.EndDate != null ||
        e.EndDate >= now));

        //update the currently active schedule  with end-date
        if (currentSchedule != null)
        {
            currentSchedule.EndDate = DateOnly.FromDateTime(model.StartDate.AddDays(-1));
            _context.PayrollSchedule.Update(currentSchedule);
        }
        else
        {  //check if this is the first schedule and if so update status
            var count = _context.PayrollSchedule.Count(e => e.CompanyId == model.CompanyId);
            if (count < 1)
            {
                var emp = _context.Employment.First(e => e.Job.CompanyId == model.CompanyId && !e.Deactivated);
                if (emp.SignUpStatusTypeId < (int)SignUpStatusTypeModel.PayrollScheduleCreationCompleted)
                {
                    emp.SignUpStatusTypeId = (int)SignUpStatusTypeModel.PayrollScheduleCreationCompleted;
                    _context.Employment.Update(emp);
                }
            }
        }
        var newSchedule = new PayrollSchedule
        {
            PayrollScheduleTypeId = (int)Enum.Parse(typeof(PayrollScheduleTypeModel), model.PayrollScheduleType),
            EndDate = model.EndDate.HasValue? DateOnly.FromDateTime(model.EndDate.Value):null,
            StartDate = DateOnly.FromDateTime(model.StartDate),
            FirstRunDate = DateOnly.FromDateTime(model.FirstRunDate),
            CompanyId = model.CompanyId
        };

        _context.PayrollSchedule.Add(newSchedule);
        _context.SaveChanges();
        //Create the first payroll for the new schedule.
        //If there is existing payroll to be run, close it
        var existingPayroll = _context.Payroll.OrderByDescending(e => e.ScheduledRunDate).FirstOrDefault();
        if (existingPayroll != null)
        {
            if (existingPayroll.ScheduledRunDate >= newSchedule.StartDate)
            {
                existingPayroll.ScheduledRunDate = newSchedule.StartDate.AddDays(-1);
            }
            if (existingPayroll.ScheduledRunDate >= newSchedule.StartDate)
            {
                existingPayroll.ScheduledRunDate = newSchedule.StartDate.AddDays(-1);
            }
        }

        var payroll = new Payroll
         {
             PayrollScheduleId = newSchedule.Id,
             StartDate = newSchedule.StartDate,
             ScheduledRunDate = newSchedule.FirstRunDate,
             CompanyId = newSchedule.CompanyId
        };
        _context.Payroll.Add(payroll);
        _context.SaveChanges();
        return;
    }

    public void Update(int id, PayrollScheduleModel model)
    {
        var dto = GetPayrollSchedule(id) ?? throw new AppException("Payroll Schedule not found");
        _mapper.Map(model, dto);

        _context.PayrollSchedule.Update(dto);
        _context.SaveChanges();
    }

    public void Delete(int id)
    {
        var dto = GetPayrollSchedule(id);
        _context.PayrollSchedule.Remove(dto);
        _context.SaveChanges();
    }

    private PayrollSchedule GetPayrollSchedule(int id)
    {
        var dto = _context.PayrollSchedule.Find(id);
        return dto;
    } 
}