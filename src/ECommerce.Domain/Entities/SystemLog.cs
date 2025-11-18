using System;
using System.Collections.Generic;

namespace ECommerce.Domain.Entities;

public partial class SystemLog
{
    public int LogId { get; set; }

    public string LogLevel { get; set; }

    public string LogType { get; set; }

    public string Source { get; set; }

    public string Message { get; set; }

    public string Details { get; set; }

    public int? UserId { get; set; }

    public Guid? RequestId { get; set; }

    public string IpAddress { get; set; }

    public string StackTrace { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User User { get; set; }
}
