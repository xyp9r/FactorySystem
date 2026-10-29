namespace FactorySystem.Models;

public enum ErrorType
{
    None,
    NotFound,
    Validator,
    Conflict,
    Failure,
    Forbidden,
    Unauthorized
}

    public record Error(ErrorType Type, string Message);

    public class Result
    {
        public bool IsSuccess { get; }
        public Error Error { get; }
    
        // Конструктор для успеха
        protected Result()
        {
            IsSuccess = true;
            Error = new Error(ErrorType.None, string.Empty);
        }
    
        // Конструктор для ошибки
        protected Result(Error error)
        {
            IsSuccess = false;
            Error = error;
        }
    
        // Статические методы
        public static Result Success() => new Result();
        public static Result Failure(Error error) => new Result(error);
    }
    
    // Результат с данными для get post put
    public class Result<TValue> : Result
    {
        public TValue Value { get; }

        private Result(TValue value) : base() // Базовый конструктор успеха
        {
            Value = value;
        }

        private Result(Error error) : base(error) // Базовый конструктор успеха
        {
            Value = default;
        } 
        
        // C# вредный оч, поэтому говорим ему кто есть кто
        public static Result<TValue> Success(TValue value) => new Result<TValue>(value);
        public static new Result<TValue> Failure(Error error) => new Result<TValue>(error);
    }