namespace UserDirectory.Api.Contracts;

public sealed record ErrorResponse(
    string Message,
    Dictionary<string, string[]>? Errors = null);
