using ECommerce.Domain.Interfaces;
using MassTransit;
using System.Threading.Tasks;

namespace ECommerce.Infrastructure.Services;

public class EventPublisher(IPublishEndpoint publishEndpoint) : IEventPublisher
{
    private readonly IPublishEndpoint _publishEndpoint = publishEndpoint;

    public Task PublishAsync<TEvent>(TEvent @event) where TEvent : class
    {
        return _publishEndpoint.Publish(@event);
    }
}