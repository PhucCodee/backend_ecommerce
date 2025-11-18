using System;
using System.Collections.Generic;

namespace ECommerce.Domain.Entities;

public partial class Review
{
    public int ReviewId { get; set; }

    public int ProductId { get; set; }

    public int UserId { get; set; }

    public int OrderId { get; set; }

    public int Rating { get; set; }

    public string Title { get; set; }

    public string Comment { get; set; }

    public bool IsVerifiedPurchase { get; set; }

    public bool IsApproved { get; set; }

    public int HelpfulCount { get; set; }

    public int UnhelpfulCount { get; set; }

    public string ModerationNotes { get; set; }

    public DateTime? ModeratedAt { get; set; }

    public int? ModeratedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual User ModeratedByNavigation { get; set; }

    public virtual Order Order { get; set; }

    public virtual Product Product { get; set; }

    public virtual ICollection<ReviewImage> ReviewImages { get; set; } = new List<ReviewImage>();

    public virtual User User { get; set; }
}
