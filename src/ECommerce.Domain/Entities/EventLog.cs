using System;
using System.Collections.Generic;
using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Entities;

public partial class EventLog
{
    public int LogId { get; set; }

    public Guid EventId { get; set; }

    public required string EventType { get; set; } = string.Empty;

    public int AttemptNumber { get; set; }

    public EventStatus Status { get; set; }

    public required string WorkerName { get; set; }

    public int? OrderId { get; set; }

    public required string Payload { get; set; }

    public string? ErrorMessage { get; set; }

    public string? ErrorCode { get; set; }

    public string? StackTrace { get; set; }

    public DateTime StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public int? ProcessingTimeMs { get; set; }

    public virtual Order? Order { get; set; }
}
