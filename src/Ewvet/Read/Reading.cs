namespace Ewvet;

public record Reading(
    int End,
    int[]? Result,
    IReadOnlyList<string> Codes,
    IReadOnlyList<string> Details,
    string? Charset,
    char Enc,
    string? Lang);
