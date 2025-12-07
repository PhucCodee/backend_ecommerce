namespace ECommerce.Application.Common.Responses
{
    public interface IApiResponse<T> : IApiResult
    {
        T? Data { get; }
    }
}