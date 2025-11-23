using System;
using System.Collections.Generic;

namespace ECommerce.Domain.Entities;

public partial class SystemLog
{
    public int LogId { get; set; }

    public required string LogLevel { get; set; } = string.Empty;

    public required string LogType { get; set; } = string.Empty;

    public required string Source { get; set; } = string.Empty;

    public required string Message { get; set; } = string.Empty;

    public string? Details { get; set; }

    public int? UserId { get; set; }

    public Guid? RequestId { get; set; }

    public required string IpAddress { get; set; } = string.Empty;

    public string? StackTrace { get; set; }

    public DateTime CreatedAt { get; set; }

    public required virtual User User { get; set; }
}
