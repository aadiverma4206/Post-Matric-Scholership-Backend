namespace Scholarship.Api.Helpers;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string> Errors { get; set; } = new();

    public static ApiResponse<T> Ok(T data, string message = "Operation completed successfully.")
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data
        };
    }

    public static ApiResponse<T> Fail(string message, List<string>? errors = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Errors = errors ?? new List<string> { message }
        };
    }
}

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / (PageSize > 0 ? PageSize : 10));
}

public static class MaskingHelper
{
    public static string MaskAadhaar(string? aadhaar)
    {
        if (string.IsNullOrWhiteSpace(aadhaar) || aadhaar.Length < 4)
            return "XXXX-XXXX-XXXX";
        string clean = aadhaar.Replace("-", "").Replace(" ", "");
        if (clean.Length == 12)
            return $"XXXX-XXXX-{clean.Substring(8, 4)}";
        return $"XXXX-XXXX-{clean.Substring(Math.Max(0, clean.Length - 4))}";
    }

    public static string MaskBankAccount(string? account)
    {
        if (string.IsNullOrWhiteSpace(account) || account.Length < 4)
            return "XXXXXXXXXXXX";
        string last4 = account.Substring(account.Length - 4);
        return new string('X', Math.Max(account.Length - 4, 8)) + last4;
    }

    public static string MaskMobile(string? mobile)
    {
        if (string.IsNullOrWhiteSpace(mobile) || mobile.Length < 4)
            return "XXXXXX0000";
        string last4 = mobile.Substring(mobile.Length - 4);
        return $"XXXXXX{last4}";
    }

    public static string MaskEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            return "u***r@domain.com";
        var parts = email.Split('@');
        string name = parts[0];
        string domain = parts[1];
        if (name.Length <= 2)
            return $"{name[0]}*@{domain}";
        return $"{name[0]}***{name[^1]}@{domain}";
    }
}
