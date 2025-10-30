namespace Volur.Domain.Entities;

/// <summary>
/// SQL Server entity for Exchange.
/// </summary>
public sealed class ExchangeEntity : BaseEntity
{
    /// <summary>
    /// Gets or sets the exchange code (primary key).
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the exchange name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the operating MIC code.
    /// </summary>
    public string? OperatingMic { get; set; }

    /// <summary>
    /// Gets or sets the country where the exchange operates.
    /// </summary>
    public string Country { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the currency used on this exchange.
    /// </summary>
    public string Currency { get; set; } = string.Empty;
}

