using System.ComponentModel;

namespace TaskMind.Applications.Commons
{
    public enum ResultStatus
    {
        [Description("None")]
        None,

        [Description("Success")]
        Success,

        [Description("Not Found")]
        NotFound,

        [Description("Failed")]
        Failed,

        [Description("Error")]
        Error
    }


    public class ServiceResult
    {
        public bool IsSuccess => Status == ResultStatus.Success;
        public bool IsFailed => Status == ResultStatus.Failed;
        public bool IsError => Status == ResultStatus.Error;
        public bool IsNotFound => Status == ResultStatus.NotFound;

        public string Message { get; private set; } = string.Empty;
        public ResultStatus Status { get; private set; }

        public static ServiceResult Success(string message = "Success")
        {
            return new()
            {
                Message = message,
                Status = ResultStatus.Success
            };
        }

        public static ServiceResult Failed(string message = "Failed")
        {
            return new()
            {
                Message = message,
                Status = ResultStatus.Failed
            };
        }

        public static ServiceResult Error(string message = "Error")
        {
            return new()
            {
                Message = message,
                Status = ResultStatus.Error
            };
        }

        public static ServiceResult NotFound(string message = "Not found")
        {
            return new()
            {
                Message = message,
                Status = ResultStatus.NotFound
            };
        }
    }

    public class ServiceResult<T>
    {
        public bool IsSuccess => Status == ResultStatus.Success;
        public bool IsFailed => Status == ResultStatus.Failed;
        public bool IsError => Status == ResultStatus.Error;
        public bool IsNotFound => Status == ResultStatus.NotFound;

        public string Message { get; private set; } = string.Empty;
        public ResultStatus Status { get; private set; }
        public T? Data { get; private set; }

        public static ServiceResult<T> Success(T data, string message = "Success")
        {
            return data is IEnumerable<object> list
                ? list.Any()
                    ? new ServiceResult<T>
                    {
                        Data = data,
                        Message = message,
                        Status = ResultStatus.Success
                    }
                    : new ServiceResult<T>
                    {
                        Data = default,
                        Message = "Empty data",
                        Status = ResultStatus.NotFound
                    }
                : data == null
                    ? new ServiceResult<T>
                    {
                        Data = default,
                        Message = "Empty data",
                        Status = ResultStatus.NotFound
                    }
                    : new ServiceResult<T>
                    {
                        Data = data,
                        Message = message,
                        Status = ResultStatus.Success
                    };
        }

        public static ServiceResult<T> Failed(string message = "Failed")
        {
            return new()
            {
                Data = default,
                Message = message,
                Status = ResultStatus.Failed
            };
        }

        public static ServiceResult<T> Error(string message = "Error")
        {
            return new()
            {
                Data = default,
                Message = message,
                Status = ResultStatus.Error
            };
        }

        public static ServiceResult<T> NotFound(string message = "Not found")
        {
            return new()
            {
                Message = message,
                Status = ResultStatus.NotFound
            };
        }
    }
}