namespace Volur.Api.Models;

/// <summary>
/// Standard error response envelope.
/// </summary>
public sealed record ErrorResponse(string Code, string Message, string TraceId)
{
    public ErrorDetails Error => new(Code, Message, TraceId);
}

public sealed record ErrorDetails(string Code, string Message, string TraceId);

