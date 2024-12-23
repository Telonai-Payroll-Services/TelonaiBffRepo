using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using TelonaiWebApi.Models;
using TelonaiWebApi.Helpers;
using Microsoft.EntityFrameworkCore.Metadata;
using System.ComponentModel;
using Microsoft.OpenApi.Extensions;
namespace TelonaiWebApi.Services
{
    public interface IDayOffTypeService
    {
        List<DayOffTypeModel> GetAllDayOffType();

        DayOffTypeModel GetDayOffTypeById(int id);
    }
    public class DayOffTypeService : IDayOffTypeService
    {
        public DayOffTypeModel GetDayOffTypeById(int id)
        {
            var dayOffType = new DayOffTypeModel();
            var DayOffTypes = DayOffTypeService.ConvertEnumToList();
            dayOffType = DayOffTypes.Find(x => x.Id == id);
            if (dayOffType != null)
            {
                return dayOffType;
            }
            else
            {
                return null;
            }
        }

        public List<DayOffTypeModel> GetAllDayOffType()
        {
            var dayOffTypes = DayOffTypeService.ConvertEnumToList();
            if (dayOffTypes != null)
            {
                return dayOffTypes;
            }
            else
            {
                return null;
            }
        }

        private static List<DayOffTypeModel> ConvertEnumToList()
        {
            var DayOffTypes = Enum.GetValues(typeof(DayOffTypes))
                                    .Cast<DayOffTypes>()
                                    .Select(e => new DayOffTypeModel
                                    {
                                        Id = (int)e,
                                        DisplayName = GetEnumDescription(e),
                                        Name = e.ToString()
                                    }).ToList();
            return DayOffTypes;
        }

        private static string GetEnumDescription(Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attribute = field.GetCustomAttributes(typeof(DescriptionAttribute), false)
                             .FirstOrDefault() as DescriptionAttribute;
            return attribute?.Description ?? value.ToString();
        }
    }
}
