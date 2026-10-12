namespace WindFreeControl.Core.Setup;

public static class TokenValidation
{
    public static string Normalize(string? token) => (token ?? "").Trim();

    public static IReadOnlyList<TokenProblem> Validate(string? token)
    {
        var normalized = Normalize(token);
        if (normalized.Length == 0)
        {
            return [TokenProblem.Empty];
        }

        return normalized.Any(char.IsWhiteSpace) ? [TokenProblem.ContainsSpaces] : [];
    }
}
