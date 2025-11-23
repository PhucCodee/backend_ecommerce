using System;
using System.Collections.Generic;

namespace ECommerce.Domain.Entities;

public partial class DeadLetterQueue
{
    public int DlqId { get; set; }

    public Guid EventId { get; set; }

    public string? EventType { get; set; }

    public string? OriginalQueue { get; set; }

    public string? Payload { get; set; }

    public string? FinalErrorMessage { get; set; }

    public int TotalRetryAttempts { get; set; }

    public DateTime FirstFailedAt { get; set; }

    public DateTime MovedToDlqAt { get; set; }

    public string? ResolutionStatus { get; set; }

    public DateTime? ReprocessedAt { get; set; }

    public int? ReprocessedBy { get; set; }

    public string? ResolutionNotes { get; set; }

    public virtual User? ReprocessedByNavigation { get; set; }
}
