namespace Project.Services
{
    public class ServiceResult
    {
        public bool Succeeded { get; }
        public string? ErrorMessage { get; }

        protected ServiceResult(bool succeeded, string? errorMessage)
        {
            Succeeded = succeeded;
            ErrorMessage = errorMessage;
        }

        public static ServiceResult Success() => new(true, null);

        public static ServiceResult Failure(string errorMessage) => new(false, errorMessage);
    }

    public sealed class ServiceResult<T> : ServiceResult
    {
        public T? Value { get; }

        private ServiceResult(bool succeeded, T? value, string? errorMessage)
            : base(succeeded, errorMessage)
        {
            Value = value;
        }

        public static ServiceResult<T> Success(T value) => new(true, value, null);

        public new static ServiceResult<T> Failure(string errorMessage) => new(false, default, errorMessage);
    }
}
