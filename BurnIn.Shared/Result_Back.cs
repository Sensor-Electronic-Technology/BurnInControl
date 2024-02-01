/*namespace BurnIn.Shared;



public abstract class Result {
    public bool Success { get; protected set; }
    public bool Failure => !Success;
}

public abstract class Result<T> : Result {
    private T _data;
    protected Result(T data) {
        this.Data = data;
    }

    public T Data {
        get => this.Success ? this._data : 
            throw new Exception($"You can't access {nameof(Data)} when {nameof(Success)} is false");
        set => this._data=value;
    }
}

public class SuccessResult : Result,ISuccessResult {
    public string Message { get; }
    public SuccessResult(string? message=null) {
        this.Message = message ?? "";
        this.Success = true;
    }
}

public class SuccessResult<T> : Result<T>,ISuccessResult {
    public string Message { get; }
    public SuccessResult(T data,string? message=null) : base(data) {
        this.Message = message ?? "";
        this.Success = true;
    }
}

public class ErrorResult : Result, IErrorResult {
    public string Message { get; }
    public IReadOnlyCollection<Error> Errors { get; }
    public ErrorResult(string message, IReadOnlyCollection<Error> errors) {
        this.Message = message;
        this.Success = false;
        this.Errors = errors ?? Array.Empty<Error>();
    }
    public ErrorResult(string message):this(message,Array.Empty<Error>()){
        
    }
}

public class ErrorResult<T>:Result<T>,IErrorResult{
    public string Message { get; }
    public IReadOnlyCollection<Error> Errors { get; }
    public ErrorResult(string message, IReadOnlyCollection<Error> errors) : base(default) {
        this.Message = message;
        this.Success = false;
        this.Errors = errors ?? Array.Empty<Error>();
    }
    public ErrorResult(string message) : this(message, Array.Empty<Error>()) {
        
    }
}


public class Error {
    public string? Code { get; }
    public string Details { get; }
    public Error(string details) : this(null, details) {
        
    }

    public Error(string? code, string details) {
        this.Code = code;
        this.Details = details;
    }
}

internal interface IErrorResult {
    string Message { get; }
    IReadOnlyCollection<Error> Errors { get; }
}

internal interface ISuccessResult {
    string Message { get; }
}*/