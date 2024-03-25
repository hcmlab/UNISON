using System;

namespace Backend
{
    public class ParameterizedResponse<T> : Response
    {
        public T Data { get; }

        public ParameterizedResponse(string action, string requestUrl, string message) : base(
            action, requestUrl, message, false)
        {
            Data = default;
        }

        public ParameterizedResponse(string action, string requestUrl, string message, Func<string, T> convertFunction, bool success = true) :
            base(action, requestUrl, message, success)
        {
            Data = IsSuccess() ? convertFunction(message) : default;
        }
    }
}