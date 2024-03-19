namespace BalancerKube.Wallet.API.Models.Common
{
    public record Result<T>
    {
        private readonly T? _value;
        private readonly Exception? _exception;

        public Result(T value)
        {
            _value = value;
            IsSuccess = true;
        }
        public Result(Exception exception)
        {
            _exception = exception;
            IsFaulted = true;
        }

        public bool IsFaulted { get; private set; } = false;

        public bool IsSuccess { get; private set; } = false;

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
