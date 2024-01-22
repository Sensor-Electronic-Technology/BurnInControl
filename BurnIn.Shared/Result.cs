namespace BurnIn.Shared;

public readonly struct Result<TValue,TError> {
    private readonly TValue? _value;
    private readonly TError? _error;
    
    public bool IsError { get; }

    private Result(TValue value) {
        this.IsError = false;
        this._value = value;
        this._error = default;
    }

    private Result(TError error) {
        this.IsError = true;
        this._value = default;
        this._error = error;
    }

    public static implicit operator Result<TValue, TError>(TValue value) => new(value);
    public static implicit operator Result<TValue, TError>(TError error) => new(error);

    public TResult Match<TResult>(
        Func<TValue, TResult> success,
        Func<TError, TResult> failure) =>
        !this.IsError ? success(this._value!) : failure(this._error!);


}