using System;
using System.Collections.Generic;
using System.Text;

namespace TaskMind.Domain.Commons.Result
{
    public class Result<T>
    {
        public string Message { get; private set; } = string.Empty;
        public bool IsSuccess { get; private set; }
        public T Data { get; private set; } = default!;

        public static Result<T> Success(T data, string message = "Success")
        {
            return new Result<T>
            {
                IsSuccess = true,
                Data = data,
                Message = message
            };
        }

        public static Result<T> Failure(string message)
        {
            return string.IsNullOrWhiteSpace(message)
                ? throw new ArgumentException("Failure message cannot be empty.", nameof(message))
                : new Result<T>
                {
                    IsSuccess = false,
                    Data = default!,
                    Message = message
                };
        }
    }

    /// <summary>Kết quả không cần trả về data (void-like).</summary>
    public class Result
    {
        public string Message { get; private set; } = string.Empty;
        public bool IsSuccess { get; private set; }

        public static Result Success(string message = "Success")
        {
            return new() { IsSuccess = true, Message = message };
        }

        public static Result Failure(string message)
        {
            return string.IsNullOrWhiteSpace(message)
                ? throw new ArgumentException("Failure message cannot be empty.", nameof(message))
                : new() { IsSuccess = false, Message = message };
        }
    }
}
