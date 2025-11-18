using System;
using System.Collections.Generic;

namespace ECommerce.Domain.Entities;

public partial class ProcessedEvent
{
    public Guid EventId { get; set; }

    public string EventType { get; set; }

    public DateTime ProcessedAt { get; set; }

    public string ProcessedBy { get; set; }
}
