namespace MeF.Client.Services
{
    public class SubmissionErrorItem
    {
        public string SubmissionId { get; set; }

        public string ErrorMessage { get; set; }

        public string ErrorClassification { get; set; }

        public string ErrorCode { get; set; }
    }
}