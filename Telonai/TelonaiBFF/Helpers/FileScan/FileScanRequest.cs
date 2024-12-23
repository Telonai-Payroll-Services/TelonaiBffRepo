﻿using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using TelonaiWebApi.Helpers.Configuration;
using TelonaiWebApi.Helpers.Interface;
using TelonaiWebApi.Models.FileScan;

namespace TelonaiWebApi.Helpers.FileScan
{
    public class FileScanRequest : IFileScanRequest
    {
        private readonly FileScanSettings _fileScanSettings;
        private readonly FileScanAuthSettings _fileScanOptions;

        public FileScanRequest(IOptions<FileScanSettings> fileScanSettings, IOptions<FileScanAuthSettings> fileScanAuthSettings)
        {
            _fileScanOptions = fileScanAuthSettings.Value;          
            _fileScanSettings = fileScanSettings.Value;
        }

        public HttpClient GetHttpClient()
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(_fileScanSettings.BaseURL);
            return client;
        }

        public async Task<FileScanLoginResponse> GetAWSToken()
        {
            var httpClient = GetHttpClient();
            var url = httpClient.BaseAddress + _fileScanSettings.AuthURL;
            using (var requestMessage = new HttpRequestMessage(HttpMethod.Post,url))
            {

                var awsLoginResponse = await httpClient.SendAsync(requestMessage);
                if (awsLoginResponse.IsSuccessStatusCode)
                {
                    string jsonResponse = await awsLoginResponse.Content.ReadAsStringAsync();
                    var result = System.Text.Json.JsonSerializer.Deserialize<FileScanLoginResponse>(jsonResponse);
                    return result;
                }
                
                 return null;
               
            }
        }

        public async Task<FileScanResponse> ScanFile()
        {
            var authResponse = await GetAWSToken();
            if(authResponse != null)
            {
                var httpClient = GetHttpClient();
                var url = httpClient.BaseAddress + _fileScanSettings.ScanURL;
                using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, url))
                {
                    requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authResponse.accessToken);
                    requestMessage.Content.Headers.ContentType = new MediaTypeHeaderValue("multipart/form-data");
                    requestMessage.Headers.Add("Prefer", "respond-async");
                    var fileScanResponse = await httpClient.SendAsync(requestMessage);
                    if (fileScanResponse.IsSuccessStatusCode)
                    {
                        string jsonResponse = await fileScanResponse.Content.ReadAsStringAsync();
                        var result = System.Text.Json.JsonSerializer.Deserialize<FileScanResponse>(jsonResponse);
                        return result;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            else
            {
                return null;
            }
        }
    }
}
