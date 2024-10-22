namespace TelonaiWebApi.Models.FileScan
{
    public class FileScanResponse
    {
        public DateTime dateScanned { get; set; }
        public List<DetectedInfections> detectedInfections { get; set; }
        public string errorMessage { get; set; }
        public string result { get; set; }
    }
}
