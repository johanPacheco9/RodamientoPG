using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Domain.Generics;
public static class EnumExtensions
{
    /// <summary>
    /// Obtiene el valor del Display Name de cualquier Enum.
    /// Si el atributo no existe, devuelve el nombre del enum por defecto.
    /// </summary>
    public static string GetDisplayName(this Enum value)
    {
        if (value == null) return string.Empty;

        Type type = value.GetType();
        string? name = Enum.GetName(type, value);

        if (name != null)
        {
            FieldInfo? field = type.GetField(name);
            if (field != null)
            {
                var attr = field.GetCustomAttribute<DisplayAttribute>();
                if (attr != null)
                {
                    return attr.Name ?? name;
                }
            }
        }

        return value.ToString();
    }
}

public sealed record Error(string Code, string Message)
{
    public static readonly Error None = new(string.Empty, string.Empty);
    public static readonly Error NullValue = new("Error.NullValue", "El valor proporcionado es nulo.");

    public static Error Failure(string code, string message) => new(code, message);
    public static Error NotFound(string code, string message) => new(code, message);
    public static Error Validation(string code, string message) => new(code, message);
    public static Error Conflict(string code, string message) => new(code, message);
}

public class Result
{
    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None ||
            !isSuccess && error == Error.None)
        {
            throw new ArgumentException("Invalid error", nameof(error));
        }
        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    public static Result Success() => new(true, Error.None);
    public static Result Failure(Error error) => new(false, error);

    public static Result Try(Action action, Func<Exception, Error> onException)
    {
        try
        {
            action();
            return Success();
        }
        catch (Exception ex)
        {
            return Failure(onException(ex));
        }
    }

    public static async Task<Result> TryAsync(Func<Task> action, Func<Exception, Error> onException)
    {
        try
        {
            await action();
            return Success();
        }
        catch (Exception ex)
        {
            return Failure(onException(ex));
        }
    }
}

public class Result<T> : Result
{
    private readonly T? _value;

    private Result(bool isSuccess, Error error, T? value = default)
        : base(isSuccess, error)
    {
        _value = value;
    }

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access value of failed result");

    public static Result<T> Success(T value) => new(true, Error.None, value);
    public new static Result<T> Failure(Error error) => new(false, error);

    public static Result<T> Try(Func<T> func, Func<Exception, Error> onException)
    {
        try
        {
            var value = func();
            return Success(value);
        }
        catch (Exception ex)
        {
            return Failure(onException(ex));
        }
    }

    public static async Task<Result<T>> TryAsync(Func<Task<T>> func, Func<Exception, Error> onException)
    {
        try
        {
            var value = await func();
            return Success(value);
        }
        catch (Exception ex)
        {
            return Failure(onException(ex));
        }
    }
}