using System;
using System.Collections.Generic;
using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Entities;

public partial class Notification
{
    public int NotificationId { get; set; }

    public int UserId { get; set; }

    public string NotificationType { get; set; }

    public NotificationChannel Channel { get; set; }

    public NotificationPriority Priority { get; set; }

    public string Subject { get; set; }

    public string Message { get; set; }

    public NotificationStatus Status { get; set; }

    public EntityType? RelatedEntityType { get; set; }

    public int? RelatedEntityId { get; set; }

    public DateTime? EmailSentAt { get; set; }

    public DateTime? ReadAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public int RetryCount { get; set; }

    public DateTime? LastRetryAt { get; set; }

    public virtual User User { get; set; }
}
