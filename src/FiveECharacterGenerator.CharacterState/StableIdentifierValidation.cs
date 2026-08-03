namespace FiveECharacterGenerator.CharacterState;

internal static class StableIdentifierValidation
{
    private const int MaximumLength = 100;

    public static string Validate(
        string value,
        string parameterName)
    {
        ArgumentNullException.ThrowIfNull(value, parameterName);

        if (value.Length == 0)
        {
            throw new ArgumentException(
                "An identifier cannot be empty.",
                parameterName);
        }

        if (value.Length > MaximumLength)
        {
            throw new ArgumentException(
                $"An identifier cannot exceed {MaximumLength} characters.",
                parameterName);
        }

        if (!IsAsciiLetterOrDigit(value[0]) ||
            !IsAsciiLetterOrDigit(value[^1]))
        {
            throw new ArgumentException(
                "An identifier must begin and end with a lowercase ASCII letter or digit.",
                parameterName);
        }

        for (int index = 0; index < value.Length; index++)
        {
            char character = value[index];

            if (!IsAsciiLetterOrDigit(character) &&
                character != '-' &&
                character != '_' &&
                character != '.')
            {
                throw new ArgumentException(
                    "An identifier may contain only lowercase ASCII letters, digits, hyphens, underscores, and periods.",
                    parameterName);
            }
        }

        return value;
    }

    private static bool IsAsciiLetterOrDigit(char character)
    {
        return character is >= 'a' and <= 'z' or >= '0' and <= '9';
    }
}
