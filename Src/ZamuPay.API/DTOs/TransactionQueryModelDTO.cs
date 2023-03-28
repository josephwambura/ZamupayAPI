using ZamuPay.API.Utils;

namespace ZamuPay.API.DTOs
{
    public class TransactionQueryModelDTO
    {
        public string Id { get; set; } = default!;

        public PaymentIdTypeEnum IdType { get; set; }
    }
}
