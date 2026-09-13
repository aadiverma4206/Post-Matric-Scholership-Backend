namespace Scholarship.Api.Exceptions;

public class AppException : Exception
{
    public int StatusCode { get; }

    public AppException(string message, int statusCode = 400) : base(message)
    {
        StatusCode = statusCode;
    }
}

public class NotFoundException : AppException
{
    public NotFoundException(string message) : base(message, 404) { }
}

public class UnauthorizedAppException : AppException
{
    public UnauthorizedAppException(string message) : base(message, 401) { }
}

public class ForbiddenAppException : AppException
{
    public ForbiddenAppException(string message) : base(message, 403) { }
}

public class ValidationAppException : AppException
{
    public List<string> Errors { get; }

    public ValidationAppException(string message, List<string>? errors = null) : base(message, 422)
    {
        Errors = errors ?? new List<string> { message };
    }
}
