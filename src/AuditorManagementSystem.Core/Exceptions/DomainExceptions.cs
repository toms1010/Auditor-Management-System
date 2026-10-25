namespace AuditorManagementSystem.Core.Exceptions;

/// <summary>
/// Thrown when business validation fails (e.g. duplicate reference number,
/// invalid amount, missing required field).
/// </summary>
public class ValidationException : Exception
{
    public ValidationException(string message) : base(message) { }
    public ValidationException(string message, Exception inner) : base(message, inner) { }
}

/// <summary>
/// Thrown when a requested entity is not found in the database.
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}

/// <summary>
/// Thrown when the current user is not authorized to perform an action.
/// </summary>
public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message) : base(message) { }
}

/// <summary>
/// Thrown when a financial operation would violate accounting rules
/// (e.g. voiding an already-voided transaction).
/// </summary>
public class BusinessRuleException : Exception
{
    public BusinessRuleException(string message) : base(message) { }
}
