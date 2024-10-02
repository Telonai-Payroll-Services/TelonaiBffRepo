using Newtonsoft.Json;
using System;

namespace TelonaiWebApi.Models;


[JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
public class TimecardUsaUpdateModel
{
    public List<TimecardUsaNoteModel> TimecardUsaNoteModels { get; set; }
    public List<TimecardUsaModel> TimecardUsaModels { get; set; }
}