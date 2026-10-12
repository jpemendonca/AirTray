using System.Diagnostics.CodeAnalysis;

namespace WindFreeControl.Core.SmartThings;

public sealed class SmartThingsResult<T>
{
    private SmartThingsResult(T? value, SmartThingsError? error)
    {
        Value = value;
        Error = error;
    }

    public T? Value { get; }

    public SmartThingsError? Error { get; }

    [MemberNotNullWhen(true, nameof(Value))]
    [MemberNotNullWhen(false, nameof(Error))]
    public bool IsSuccess => Error is null;

    public static SmartThingsResult<T> Success(T value) => new(value, null);

    public static SmartThingsResult<T> Failure(SmartThingsError error) => new(default, error);
}
