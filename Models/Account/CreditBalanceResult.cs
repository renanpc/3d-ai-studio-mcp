using System.Text.Json.Serialization;

namespace ThreeDAiStudioMcp.Models.Account;

public sealed record CreditBalanceResult(
    [property: JsonPropertyName("balance")] string Balance);
