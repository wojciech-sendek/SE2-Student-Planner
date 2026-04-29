namespace StudentPlanner.Api.Services
{
    public class UsosApiException : Exception
    {
        public UsosApiException(string message) : base(message)
        {
        }

        public UsosApiException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    public class UsosAuthorizationRequiredException : Exception
    {
        public UsosAuthorizationRequiredException(string message) : base(message)
        {
        }
    }
}