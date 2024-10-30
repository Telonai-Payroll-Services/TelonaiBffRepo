namespace TelonaiWebApi.Models.FileScan
{
    public class FileScanLoginResponse
    {
        public string accessToken { get; set; }
        public string tokenType { get; set; }
        public int expiresIn { get; set; }
    }
}
