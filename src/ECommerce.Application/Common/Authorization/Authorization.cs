namespace ECommerce.Application.Common.Authorization
{
    public static class Policies
    {
        public const string AdminOnly = "AdminOnly";
        public const string SellerOrAdmin = "SellerOrAdmin";
        public const string Authenticated = "Authenticated";
    }

    public static class Roles
    {
        public const string Admin = "admin";
        public const string Seller = "seller";
        public const string Buyer = "buyer";
    }
}