using System;

namespace TestProject.Common
{
    public abstract record Result
    {
        public sealed record Success : Result;
        public sealed record Failure(Exception Error) : Result;

        public static Result SuccessOf() => new Success();
        public static Result FailureOf(Exception error) => new Failure(error);

        public TResult Match<TResult>(
            Func<TResult> onSuccess,
            Func<Exception, TResult> onFailure) =>
            this switch
            {
                Success => onSuccess(),
                Failure f => onFailure(f.Error),
                _ => throw new InvalidOperationException()
            };

        public void Match(
            Action onSuccess,
            Action<Exception> onFailure)
        {
            Match(
                () => { onSuccess(); return (object)null; },
                ex => { onFailure(ex); return (object)null; }
            );
        }
    }

    public abstract record Result<T>
    {
        public sealed record Success(T Value) : Result<T>;
        public sealed record Failure(Exception Error) : Result<T>;

        public static Result<T> SuccessOf(T value) => new Success(value);
        public static Result<T> FailureOf(Exception error) => new Failure(error);

        public TResult Match<TResult>(
            Func<T, TResult> onSuccess,
            Func<Exception, TResult> onFailure) =>
            this switch
            {
                Success s => onSuccess(s.Value),
                Failure f => onFailure(f.Error),
                _ => throw new InvalidOperationException()
            };

        public void Match(
            Action<T> onSuccess,
            Action<Exception> onFailure)
        {
            Match(
                value => { onSuccess(value); return (object)null; },
                ex => { onFailure(ex); return (object)null; }
            );
        }
    }
}
