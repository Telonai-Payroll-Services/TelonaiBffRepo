namespace TelonaiWebApi.Helpers;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

public class ErrorHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger _logger;

    public ErrorHandlerMiddleware(RequestDelegate next, ILogger<ErrorHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception error)
        {
            var result = "";
            var response = context.Response;
            response.ContentType = "application/json";
            

            switch (error)
            {
                case AppException e:
                    // custom application error            
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    result = JsonSerializer.Serialize(new { message = error?.Message });
                    _logger.LogError(error.ToString());
                    break;
                case KeyNotFoundException e:
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    result = JsonSerializer.Serialize(new { message = error?.Message });
                    _logger.LogError(HttpStatusCode.NotFound + ". " + error.ToString());
                    break;
                default:
                    // unhandled error
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    result = JsonSerializer.Serialize(new { message = "We encountered a snag. Please try again later." });
                    _logger.LogError(HttpStatusCode.InternalServerError + ". " + error.ToString());
                    break;
            }

            await response.WriteAsync(result);
        }
    }
}