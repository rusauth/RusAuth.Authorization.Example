namespace RusAuth.Authorization.Example.Infrastructure;

using Contracts.Rest;

public static class StringExtensions
{
    public static string ToDisplayString(this RusAuthPhoneNumber? phoneNumber) =>
        phoneNumber is null ? "не задан" : phoneNumber.ToString();

    public static string ToDisplayString(this RusAuthConfirmationStatus status) =>
        status switch
        {
            RusAuthConfirmationStatus.Unhandled => "ожидается подтверждение",
            RusAuthConfirmationStatus.Success   => "подтверждено",
            RusAuthConfirmationStatus.Failed    => "не подтверждено",
            RusAuthConfirmationStatus.Expired   => "срок истёк",
            _                                   => status.ToString()
        };

    public static string MaskSecret(this string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "не задан";

        if (value.Length <= 18)
            return value;

        return $"{value[..10]}...{value[^6..]}";
    }

    public static string OrDefaultText(this string? value, string fallback = "не задано") =>
        string.IsNullOrWhiteSpace(value) ? fallback : value;
}