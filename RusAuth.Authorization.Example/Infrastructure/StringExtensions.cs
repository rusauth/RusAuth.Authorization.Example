namespace RusAuth.Authorization.Example.Infrastructure;

using Contracts;

public static class StringExtensions
{
    public static string ToDisplayString(this string? phoneNumber) =>
        phoneNumber ?? "не задан";

    public static string ToDisplayString(this CallConfirmationStatus status) =>
        status switch
        {
            CallConfirmationStatus.Unhandled => "ожидается подтверждение",
            CallConfirmationStatus.Success   => "подтверждено",
            CallConfirmationStatus.Failed    => "не подтверждено",
            CallConfirmationStatus.Expired   => "срок истёк",
            _                                => status.ToString()
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