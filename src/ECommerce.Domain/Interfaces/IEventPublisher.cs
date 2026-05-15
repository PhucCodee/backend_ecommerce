using System.Threading.Tasks;

namespace ECommerce.Domain.Interfaces;

public interface IEventPublisher
{
    Task PublishAsync<TEvent>(TEvent @event)
        where TEvent : class;
}
