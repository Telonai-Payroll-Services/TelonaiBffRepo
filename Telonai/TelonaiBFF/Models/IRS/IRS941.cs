namespace TelonaiWebApi.Models.IRS
{
    public class IRS941
    {
        private string documentId{ get; set; }

        private string softwareId{ get; set; }

        private string softwareVersionNum{ get; set; }

        private string documentName{ get; set; }

        private string[] referenceDocumentId{ get; set; }

        private string referenceDocumentName{ get; set; }

        public IRS941()
        {
            this.documentName = "IRS941";
            this.referenceDocumentName = "BinaryAttachment GeneralDependencySmall FinalPayrollInformationStatement Transfer" +
                "OfBusinessStatement IRS941ScheduleR IRS941ScheduleD IRS941ScheduleB";
        }
    }
}
