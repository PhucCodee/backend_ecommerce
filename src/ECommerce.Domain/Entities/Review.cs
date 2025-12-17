using System;
using System.Collections.Generic;

namespace ECommerce.Domain.Entities;

public partial class Review
{
    public int ReviewId { get; set; }

    public int ProductId { get; set; }

    public int UserId { get; set; }

    public int OrderItemId { get; set; }

    public int Rating { get; set; }

    public required string Title { get; set; }

    public required string Comment { get; set; }

    public bool IsVerifiedPurchase { get; set; }

    public bool IsApproved { get; set; }

    public int HelpfulCount { get; set; }

    public int UnhelpfulCount { get; set; }

    public string? ModerationNotes { get; set; }

    public DateTime? ModeratedAt { get; set; }

    public int? ModeratedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual User? ModeratedByNavigation { get; set; }

    public virtual ICollection<ReviewImage> ReviewImages { get; set; } = [];

    public required virtual OrderItem OrderItem { get; set; }

    public required virtual Product Product { get; set; }

    public required virtual User User { get; set; }
}
