using System;
using Nancy;
using Nancy.Responses;

namespace Ingress.Util
{
    class ErrorBuilder
    {
        public static Nancy.Response ErrorResponse(string url, string verb, HttpStatusCode code, string errorMessage)
        {
            ErrorBody e = new ErrorBody
            {
                Url = url,
                Operation = verb,
                Message = errorMessage
            };

            // Build and return an object that the Nancy server knows about.
            Nancy.Response response = new Nancy.Responses.JsonResponse<ErrorBody>(e, new DefaultJsonSerializer());
            response.StatusCode = code;
            return response;
        }

    }

    // useful info to return in an error
    public class ErrorBody
    {
        public string Url { get; set; }
        public string Operation { get; set; }
        public string Message { get; set; }
    }

}