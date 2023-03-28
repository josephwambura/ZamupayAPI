using System.ComponentModel;

namespace ZamuPay.API.Utils
{
    public enum PaymentIdTypeEnum
    {
        [Description("SystemConversationId")]
        SystemConversationId = 0,
        [Description("OriginatorConversationId")]
        OriginatorConversationId = 1,
    }
}