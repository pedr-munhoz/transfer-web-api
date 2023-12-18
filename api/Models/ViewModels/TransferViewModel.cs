namespace api.Models.ViewModels
{
    public class TransferViewModel
    {
        public long DestinationAccountId { get; set; }
        public long OriginAccountId { get; set; }
        public double Amount { get; set; }
    }
}