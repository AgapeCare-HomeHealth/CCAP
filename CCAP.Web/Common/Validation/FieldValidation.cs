namespace CCAP.Web.Common.Validation;

public static class FieldValidation
{
    public static string? Phone(
        string? value,
        string fieldName,
        bool required = false)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return required
                ? $"{fieldName} is required."
                : null;
        }

        foreach (var character in value)
        {
            if (!char.IsDigit(character) &&
                character != ' ' &&
                character != '-' &&
                character != '(' &&
                character != ')' &&
                character != '+')
            {
                return $"{fieldName} contains invalid characters.";
            }
        }

        if (value.Contains('+') &&
            !value.StartsWith('+'))
        {
            return $"{fieldName} contains an invalid '+' position.";
        }

        var digitCount =
            value.Count(char.IsDigit);

        if (digitCount < 10 ||
            digitCount > 15)
        {
            return
                $"{fieldName} must contain between 10 and 15 digits.";
        }

        return null;
    }

    public static string InputClass(
        string? error)
    {
        return string.IsNullOrWhiteSpace(error)
            ? "form-control"
            : "form-control is-invalid";
    }
}