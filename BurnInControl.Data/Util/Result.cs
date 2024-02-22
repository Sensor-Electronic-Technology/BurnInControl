namespace BurnIn.Data.Util;

public class Result {
    public bool IsSuccess { get; }
    public string? Error { get; }
    public string? Message { get; }

    internal protected Result(bool isSuccess, string? error=null,string? message=null) {
        this.IsSuccess = isSuccess;
        this.Error = error;
        this.Message = message;
    }
}

public class Result<TValue> : Result {
    public TValue Value { get; }

    internal protected Result(TValue value, bool isSuccess, string? error = null,string? message=null)
        :base(isSuccess,error,message) {
        this.Value = value;
    }
}

public static class ResultFactory {
    public static Result Success(string? message=null) => new Result(true,message:message);
    public static Result Error(string? error = null) => new Result(false, error: error);
    public static Result<TValue> Success<TValue>(TValue value, string? message = null) => new Result<TValue>(value, true, message: message);
    public static Result<TValue> Error<TValue>(TValue value, string? error) => new Result<TValue>(value, false, error: error);
}