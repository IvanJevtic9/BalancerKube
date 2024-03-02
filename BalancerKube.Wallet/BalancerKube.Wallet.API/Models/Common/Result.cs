namespace BalancerKube.Wallet.API.Models.Common
{
    public record Result<T>
    {
        private readonly T? _value;
        private readonly Exception? _exception;

        public Result(T value) => _value = value;
        public Result(Exception exception) => _exception = exception;

        public bool IsFaulted => _exception is not null;

        public bool IsSuccess => _value is not null;

        public T? Value
        {
            get
            {
                if (IsSuccess)
                {
                    return _value;
                }

                return default;
            }
        }

        public Exception? Exception
        {
            get
            {
                if (IsFaulted)
                {
                    return _exception;
                }

                return null;
            }
        }
    }
}
