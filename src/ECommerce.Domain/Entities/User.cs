using System;
using System.Collections.Generic;
using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Entities;

public partial class User
{
    public int UserId { get; set; }

    public required string Email { get; set; } = string.Empty;

    public required string Username { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public bool EmailVerified { get; set; } = false;

    public DateTime? EmailVerifiedAt { get; set; }

    public UserStatus Status { get; set; } = UserStatus.Active;

    public DateTime? DeletedAt { get; set; }

    public virtual ICollection<CartItem> CartItems { get; set; } = [];

    public virtual ICollection<DeadLetterQueue> DeadLetterQueues { get; set; } = [];

    public virtual ICollection<InventoryHistory> InventoryHistories { get; set; } = [];

    public virtual ICollection<Notification> Notifications { get; set; } = [];

    public virtual ICollection<OrderItem> OrderItems { get; set; } = [];

    public virtual ICollection<OrderStatusHistory> OrderStatusHistories { get; set; } = [];

    public virtual ICollection<Order> Orders { get; set; } = [];

    public virtual ICollection<Product> Products { get; set; } = [];

    public virtual ICollection<Review> ReviewModeratedByNavigations { get; set; } = [];

    public virtual ICollection<Review> ReviewUsers { get; set; } = [];

    public virtual ICollection<SystemLog> SystemLogs { get; set; } = [];

    public virtual ICollection<UserAddress> UserAddresses { get; set; } = [];

    public virtual ICollection<UserItemInteraction> UserItemInteractions { get; set; } = [];

    public virtual ICollection<UserLoginHistory> UserLoginHistories { get; set; } = [];

    public virtual ICollection<UserRole> UserRoleGrantedByNavigation { get; set; } = [];

    public virtual ICollection<UserRole> UserRoleUsers { get; set; } = [];

    public virtual ICollection<UserSession> UserSessions { get; set; } = [];

    public virtual UserCredential? UserCredential { get; set; }

    public virtual UserProfile? UserProfile { get; set; }
}
