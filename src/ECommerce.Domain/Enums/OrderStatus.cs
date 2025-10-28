namespace ECommerce.Domain.Enums
{
    public enum OrderStatus
    {
        Pending,        // Order has been created but not yet processed
        Confirmed,      // Order has been confirmed by the seller
        Shipped,        // Order has been shipped to the customer
        Delivered,      // Order has been delivered to the customer
        Cancelled,      // Order has been cancelled
        Returned        // Order has been returned by the customer
    }
}