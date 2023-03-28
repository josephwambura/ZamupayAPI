using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ZamuPay.API.DTOs
{
    public class ZamupayRoutesDTO
    {
        [property: JsonPropertyName("routes")]
        public IReadOnlyList<RouteDTO> Routes { get; set; } = default!;
    }

}