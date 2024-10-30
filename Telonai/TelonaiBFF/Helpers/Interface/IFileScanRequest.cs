using TelonaiWebApi.Models.FileScan;

namespace TelonaiWebApi.Helpers.Interface
{
    public interface IFileScanRequest
    {
        Task<FileScanLoginResponse> GetAWSToken();

        Task<FileScanResponse> ScanFile();
    }
}
