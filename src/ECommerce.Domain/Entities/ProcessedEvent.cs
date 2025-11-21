using System;
using System.Collections.Generic;

namespace ECommerce.Domain.Entities;

public partial class ProcessedEvent
{
    public Guid EventId { get; set; }

    public required string EventType { get; set; } = string.Empty;

    public DateTime ProcessedAt { get; set; }

    public required string ProcessedBy { get; set; } = string.Empty;
}
