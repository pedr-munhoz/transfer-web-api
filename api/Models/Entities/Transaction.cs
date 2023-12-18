namespace api.Models.Entities
{
    public class Transaction
    {
        public long Id { get; set; }
        public double Amount { get; set; }
        public long DestinationAccountId { get; set; }
        public long? OriginAccountId { get; set; }
    }
}