-- ====================================
-- E-COMMERCE SYSTEM
-- ====================================
-- ====================================
-- ====================================
-- 0. GLOBAL EXTENSIONS
-- ====================================
-- Enables fuzzy matching and similarity calculations (e.g. 'USA' <-> 'USB')
CREATE EXTENSION IF NOT EXISTS pg_trgm;

-- 1. USER MANAGEMENT TABLES
-- ====================================
-- Core user identity table
-- Stores fundamental user information separated from authentication credentials
-- Supports soft delete via deleted_at timestamp
CREATE TABLE
    users (
        user_id SERIAL PRIMARY KEY,
        email VARCHAR(255) UNIQUE NOT NULL,
        username VARCHAR(100) UNIQUE NOT NULL,
        created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
        updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
        email_verified BOOLEAN NOT NULL DEFAULT FALSE,
        email_verified_at TIMESTAMP,
        status SMALLINT NOT NULL DEFAULT 0,
        deleted_at TIMESTAMP -- Soft delete timestamp
    );

COMMENT ON TABLE users IS 'Core user identity table. Authentication credentials separated to user_credentials for security.';

COMMENT ON COLUMN users.status IS 'User account status - enforced by user_status_enum type';

COMMENT ON COLUMN users.deleted_at IS 'Soft delete timestamp - NULL means active';

-- User authentication credentials table
-- Security-critical table - never expose in API responses
-- Implements account lockout after failed login attempts
CREATE TABLE
    user_credentials (
        credential_id SERIAL PRIMARY KEY,
        user_id INTEGER UNIQUE NOT NULL REFERENCES users (user_id) ON DELETE CASCADE,
        password_hash VARCHAR(255) NOT NULL,
        password_salt VARCHAR(255) NOT NULL,
        password_updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
        failed_login_attempts INTEGER NOT NULL DEFAULT 0,
        last_failed_attempt_at TIMESTAMP,
        locked_until TIMESTAMP,
        last_login_at TIMESTAMP,
        last_login_ip VARCHAR(45),
        reset_token_hash VARCHAR(255),
        reset_token_expires_at TIMESTAMP,
        created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
        updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
    );

COMMENT ON TABLE user_credentials IS 'Separated authentication credentials for enhanced security. Never expose password_hash or password_salt in API responses.';

COMMENT ON COLUMN user_credentials.password_hash IS 'bcrypt hash with cost factor 12 or higher';

COMMENT ON COLUMN user_credentials.locked_until IS 'Account lockout expiry timestamp after 5 consecutive failed login attempts';

-- User profile information table
-- Contains personal and preference data separate from core identity
CREATE TABLE
    user_profiles (
        profile_id SERIAL PRIMARY KEY,
        user_id INTEGER UNIQUE NOT NULL REFERENCES users (user_id) ON DELETE CASCADE,
        first_name VARCHAR(100),
        last_name VARCHAR(100),
        phone VARCHAR(20),
        date_of_birth DATE,
        gender SMALLINT NOT NULL DEFAULT 0,
        avatar_url VARCHAR(500),
        bio TEXT,
        language SMALLINT DEFAULT 0,
        timezone VARCHAR(50) DEFAULT 'Asia/Ho_Chi_Minh',
        currency SMALLINT DEFAULT 0,
        created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
        updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
    );

COMMENT ON TABLE user_profiles IS 'User profile information separated from core identity for modularity and privacy.';

COMMENT ON COLUMN user_profiles.gender IS 'Gender identity - enforced by user_gender_enum type';

COMMENT ON COLUMN user_profiles.language IS 'User interface language - enforced by language_enum type';

COMMENT ON COLUMN user_profiles.currency IS 'Preferred currency for display - enforced by currency_enum type';

-- User roles table - supports multiple roles per user
-- Enables RBAC (Role-Based Access Control)
CREATE TABLE
    user_roles (
        user_role_id SERIAL PRIMARY KEY,
        user_id INTEGER NOT NULL REFERENCES users (user_id) ON DELETE CASCADE,
        role SMALLINT NOT NULL DEFAULT 0,
        granted_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
        granted_by INTEGER REFERENCES users (user_id),
        revoked_at TIMESTAMP,
        UNIQUE (user_id, role)
    );

COMMENT ON TABLE user_roles IS 'Supports multiple roles per user (e.g., user can be both buyer and seller). Implements RBAC pattern.';

COMMENT ON COLUMN user_roles.role IS 'Role type - enforced by user_role_enum type';

COMMENT ON COLUMN user_roles.granted_by IS 'User ID of admin who granted this role';

-- User addresses table
-- Stores shipping and billing addresses with geolocation support
CREATE TABLE
    user_addresses (
        address_id SERIAL PRIMARY KEY,
        user_id INTEGER NOT NULL REFERENCES users (user_id) ON DELETE CASCADE,
        address_type SMALLINT NOT NULL DEFAULT 0,
        is_default_shipping BOOLEAN NOT NULL DEFAULT FALSE,
        is_default_billing BOOLEAN NOT NULL DEFAULT FALSE,
        label VARCHAR(50),
        recipient_name VARCHAR(200) NOT NULL,
        phone VARCHAR(20) NOT NULL,
        address_line1 VARCHAR(255) NOT NULL,
        address_line2 VARCHAR(255),
        city VARCHAR(100) NOT NULL,
        state_province VARCHAR(100),
        postal_code VARCHAR(20),
        country VARCHAR(100) NOT NULL DEFAULT 'Vietnam',
        latitude DECIMAL(10, 8),
        longitude DECIMAL(11, 8),
        created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
        updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
    );

COMMENT ON TABLE user_addresses IS 'User addresses for shipping and billing. Supports geolocation for delivery route optimization.';

COMMENT ON COLUMN user_addresses.address_type IS 'Location type - enforced by address_type_enum type';

COMMENT ON COLUMN user_addresses.is_default_shipping IS 'Default address for shipping orders';

COMMENT ON COLUMN user_addresses.is_default_billing IS 'Default address for billing/invoices';

COMMENT ON COLUMN user_addresses.label IS 'User-friendly label like Home, Work, Lonely House';

COMMENT ON COLUMN user_addresses.latitude IS 'Latitude coordinate for delivery optimization (optional)';

-- User sessions table
-- Tracks all active and historical user sessions for security auditing
CREATE TABLE
    user_sessions (
        session_id SERIAL PRIMARY KEY,
        user_id INTEGER NOT NULL REFERENCES users (user_id) ON DELETE CASCADE,
        access_token_hash VARCHAR(255) NOT NULL,
        refresh_token_hash VARCHAR(255),
        ip_address VARCHAR(45),
        user_agent TEXT,
        device_type SMALLINT,
        device_name VARCHAR(100),
        created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
        last_activity_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
        expires_at TIMESTAMP NOT NULL,
        revoked_at TIMESTAMP
    );

COMMENT ON TABLE user_sessions IS 'Tracks all user sessions for security auditing. Implements token-based authentication with refresh mechanism.';

COMMENT ON COLUMN user_sessions.access_token_hash IS 'SHA256 hash of JWT access token - never store plain tokens';

COMMENT ON COLUMN user_sessions.device_type IS 'Device type - enforced by device_type_enum type';

COMMENT ON COLUMN user_sessions.revoked_at IS 'Timestamp when session was manually revoked (logout or security action)';

-- User login history table
-- Security audit log for all login attempts (successful and failed)
CREATE TABLE
    user_login_history (
        login_id SERIAL PRIMARY KEY,
        user_id INTEGER REFERENCES users (user_id) ON DELETE SET NULL,
        email VARCHAR(255) NOT NULL,
        login_status SMALLINT NOT NULL DEFAULT 0,
        failure_reason VARCHAR(100),
        ip_address VARCHAR(45),
        user_agent TEXT,
        location_country VARCHAR(100),
        location_city VARCHAR(100),
        is_suspicious BOOLEAN NOT NULL DEFAULT FALSE,
        created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
    );

COMMENT ON TABLE user_login_history IS 'Security audit log for all login attempts. Used for threat detection and forensic analysis.';

COMMENT ON COLUMN user_login_history.is_suspicious IS 'Flagged by security rules (unusual location, device, time, etc.)';

COMMENT ON COLUMN user_login_history.email IS 'Email used in login attempt - tracked even if user does not exist';

-- ====================================
-- 2. PRODUCT MANAGEMENT TABLES
-- ====================================
-- Product categories table with hierarchical support
-- Self-referencing for unlimited category depth
CREATE TABLE
    categories (
        category_id SERIAL PRIMARY KEY,
        category_name VARCHAR(100) NOT NULL,
        slug VARCHAR(100) UNIQUE NOT NULL,
        parent_category_id INTEGER REFERENCES categories (category_id) ON DELETE SET NULL,
        description TEXT,
        image_url VARCHAR(500),
        display_order INTEGER NOT NULL DEFAULT 0,
        is_core BOOLEAN NOT NULL DEFAULT TRUE,
        is_active BOOLEAN NOT NULL DEFAULT TRUE,
        created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
        updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
    );

COMMENT ON TABLE categories IS 'Product categories with hierarchical support (self-referencing parent_category_id). Supports unlimited nesting depth.';

COMMENT ON COLUMN categories.slug IS 'URL-friendly unique identifier for category';

COMMENT ON COLUMN categories.parent_category_id IS 'Parent category for hierarchical structure - NULL for root categories';

-- Products table (parent product)
-- Actual sellable items are in product_skus table
CREATE TABLE
    products (
        product_id SERIAL PRIMARY KEY,
        seller_id INTEGER NOT NULL REFERENCES users (user_id) ON DELETE CASCADE,
        product_name VARCHAR(255) NOT NULL,
        slug VARCHAR(255) UNIQUE NOT NULL,
        base_sku VARCHAR(100) UNIQUE NOT NULL,
        description TEXT,
        short_description VARCHAR(500),
        has_variants BOOLEAN NOT NULL DEFAULT FALSE,
        brand VARCHAR(100),
        weight_kg DECIMAL(8, 2),
        dimensions_cm VARCHAR(50),
        status SMALLINT NOT NULL DEFAULT 0,
        moderation_status SMALLINT NOT NULL DEFAULT 0,
        meta_title VARCHAR(200),
        meta_description VARCHAR(500),
        tags TEXT,
        view_count INTEGER NOT NULL DEFAULT 0,
        created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
        updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
        published_at TIMESTAMP,
        removed_at TIMESTAMP
    );

COMMENT ON TABLE products IS 'Parent product table. Actual sellable items (with pricing) are in product_skus table.';

COMMENT ON COLUMN products.has_variants IS 'If TRUE, product has multiple variants (color, size, etc.) and pricing comes from product_skus';

COMMENT ON COLUMN products.status IS 'Product lifecycle: draft, active, inactive, removed, archived';

COMMENT ON COLUMN products.moderation_status IS 'Admin moderation status: pending, approved, rejected';

CREATE INDEX trgm_idx_products_name ON products USING gin (product_name gin_trgm_ops);

CREATE INDEX trgm_idx_products_desc ON products USING gin (description gin_trgm_ops);

CREATE TABLE product_categories (
    product_id INTEGER NOT NULL REFERENCES products(product_id) ON DELETE CASCADE,
    category_id INTEGER NOT NULL REFERENCES categories(category_id) ON DELETE RESTRICT,
    is_primary BOOLEAN NOT NULL DEFAULT FALSE,
    PRIMARY KEY (product_id, category_id)
);

-- Product SKUs table (actual sellable items)
-- Represents individual sellable variations of a product
CREATE TABLE
    product_skus (
        sku_id SERIAL PRIMARY KEY,
        product_id INTEGER NOT NULL REFERENCES products (product_id) ON DELETE CASCADE,
        sku VARCHAR(100) UNIQUE NOT NULL,
        variant_attributes JSONB DEFAULT '{}',
        price DECIMAL(10, 2) NOT NULL,
        cost_price DECIMAL(10, 2),
        compare_at_price DECIMAL(10, 2),
        is_active BOOLEAN NOT NULL DEFAULT TRUE,
        is_default BOOLEAN NOT NULL DEFAULT FALSE,
        weight_kg DECIMAL(8, 2),
        dimensions_cm VARCHAR(50),
        created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
        updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
    );

COMMENT ON TABLE product_skus IS 'Represents actual sellable items. Products without variants have 1 SKU, products with variants have multiple SKUs.';

COMMENT ON COLUMN product_skus.variant_attributes IS 'JSONB field storing variant attributes like {"color": "red", "size": "L"}. NULL for non-variant products.';

COMMENT ON COLUMN product_skus.compare_at_price IS 'Original price before discount - used to display savings';

-- Product images table
-- Can be linked to product or specific SKU
CREATE TABLE
    product_images (
        image_id SERIAL PRIMARY KEY,
        sku_id INTEGER NOT NULL REFERENCES product_skus (sku_id) ON DELETE CASCADE,
        image_url VARCHAR(500) NOT NULL,
        thumbnail_url VARCHAR(500),
        alt_text VARCHAR(255),
        display_order INTEGER NOT NULL DEFAULT 0,
        is_primary BOOLEAN NOT NULL DEFAULT FALSE,
        is_deleted BOOLEAN NOT NULL DEFAULT FALSE,
        updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
    );

COMMENT ON TABLE product_images IS 'Product images. Can be associated with entire product or specific SKU variants.';

COMMENT ON COLUMN product_images.sku_id IS 'NULL if image applies to all SKUs, otherwise specific to this SKU variant';

-- Inventory tracking table
-- One record per SKU with quantity tracking
CREATE TABLE
    inventory (
        inventory_id SERIAL PRIMARY KEY,
        sku_id INTEGER UNIQUE NOT NULL REFERENCES product_skus (sku_id) ON DELETE CASCADE,
        quantity_available INTEGER NOT NULL DEFAULT 0,
        quantity_reserved INTEGER NOT NULL DEFAULT 0, -- Reserved during checkout process
        quantity_sold INTEGER NOT NULL DEFAULT 0,
        reorder_point INTEGER NOT NULL DEFAULT 10, -- Alert threshold
        reorder_quantity INTEGER NOT NULL DEFAULT 50,
        last_restocked_at TIMESTAMP,
        created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
        updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
    );

COMMENT ON TABLE inventory IS 'One inventory record per SKU. Tracks available, reserved, and sold quantities.';

COMMENT ON COLUMN inventory.quantity_reserved IS 'Quantity temporarily reserved during active checkout sessions';

COMMENT ON COLUMN inventory.reorder_point IS 'Automatic alert threshold for low stock notifications';

-- Inventory history table
-- Complete audit trail of all inventory changes
CREATE TABLE
    inventory_history (
        history_id SERIAL PRIMARY KEY,
        inventory_id INTEGER NOT NULL REFERENCES inventory (inventory_id) ON DELETE CASCADE,
        change_type VARCHAR(20) NOT NULL,
        quantity_change INTEGER NOT NULL,
        quantity_before INTEGER NOT NULL,
        quantity_after INTEGER NOT NULL,
        reference_type VARCHAR(50),
        reference_id INTEGER,
        notes TEXT,
        changed_by INTEGER REFERENCES users (user_id) ON DELETE SET NULL,
        created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
        updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
    );

COMMENT ON TABLE inventory_history IS 'Complete audit trail of all inventory changes for compliance and reconciliation.';

COMMENT ON COLUMN inventory_history.change_type IS 'Type of change: restock, sale, reserve, release, adjustment';

COMMENT ON COLUMN inventory_history.reference_type IS 'Context of change: order, manual, system, return';

-- ====================================
-- 3. SHOPPING CART TABLES
-- ====================================
-- Shopping carts table
-- Tracks cart metadata and lifecycle - supports multiple carts per user
CREATE TABLE
    carts (
        cart_id SERIAL PRIMARY KEY,
        user_id INTEGER REFERENCES users (user_id) ON DELETE CASCADE,
        status SMALLINT NOT NULL DEFAULT 0,
        subtotal DECIMAL(10, 2) NOT NULL DEFAULT 0,
        total_items INTEGER NOT NULL DEFAULT 0,
        session_id VARCHAR(255), -- For guest carts (future feature)
        ip_address VARCHAR(45),
        abandoned_at TIMESTAMP,
        created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
        updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
        CONSTRAINT cart_owner_check CHECK (
            (
                user_id IS NOT NULL
                AND session_id IS NULL
            )
            OR (
                user_id IS NULL
                AND session_id IS NOT NULL
            )
        )
    );

-- Partial unique index: only ONE active cart per authenticated user
CREATE UNIQUE INDEX idx_one_active_cart_per_user ON carts (user_id)
WHERE
    status = 0
    AND user_id IS NOT NULL;

-- Partial unique index: only ONE active cart per guest session
CREATE UNIQUE INDEX idx_one_active_cart_per_session ON carts (session_id)
WHERE
    status = 0
    AND session_id IS NOT NULL;

COMMENT ON TABLE carts IS 'Multiple carts per user for historical tracking. Only ONE active cart enforced by partial unique index. Converted carts preserved for analytics.';

COMMENT ON COLUMN carts.status IS '0=active (current shopping), 1=abandoned (inactive 24hrs), 2=converted (became an order)';

-- Shopping cart items table
CREATE TABLE
    cart_items (
        cart_item_id SERIAL PRIMARY KEY,
        cart_id INTEGER NOT NULL REFERENCES carts (cart_id) ON DELETE CASCADE,
        sku_id INTEGER NOT NULL REFERENCES product_skus (sku_id) ON DELETE CASCADE,
        quantity INTEGER NOT NULL DEFAULT 1,
        price_snapshot DECIMAL(10, 2) NOT NULL,
        added_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
        updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
        UNIQUE (cart_id, sku_id) -- One SKU per cart
    );

COMMENT ON TABLE cart_items IS 'Shopping cart line items. Price snapshot preserved for analytics and price change detection.';

COMMENT ON COLUMN cart_items.price_snapshot IS 'Price at time of adding to cart - used to detect price changes before checkout';

-- ====================================
-- 4. ORDERS & ORDER PROCESSING TABLES
-- ====================================
-- Orders table (core order information)
-- Related data (shipping, payment, fulfillment) separated per SRP
CREATE TABLE
    coupons (
        coupon_id SERIAL PRIMARY KEY,
        code VARCHAR(50) UNIQUE NOT NULL,
        description TEXT,
        discount_type SMALLINT NOT NULL,
        discount_value DECIMAL(10, 2) NOT NULL,
        min_order_amount DECIMAL(10, 2),
        usage_limit INTEGER,
        valid_from TIMESTAMP,
        valid_until TIMESTAMP,
        is_active BOOLEAN NOT NULL DEFAULT TRUE,
        created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
        updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
    );

COMMENT ON TABLE coupons IS 'Coupon/discount codes for promotional campaigns. Tracks global usage limit only.';

COMMENT ON COLUMN coupons.discount_type IS '0=percentage, 1=fixed_amount, 2=free_shipping';

CREATE TABLE
    orders (
        order_id SERIAL PRIMARY KEY,
        order_number VARCHAR(50) UNIQUE NOT NULL,
        user_id INTEGER NOT NULL REFERENCES users (user_id) ON DELETE RESTRICT,
        status SMALLINT NOT NULL DEFAULT 0,
        subtotal DECIMAL(10, 2) NOT NULL,
        shipping_fee DECIMAL(10, 2) NOT NULL DEFAULT 0,
        tax_amount DECIMAL(10, 2) NOT NULL DEFAULT 0,
        coupon_id INTEGER REFERENCES coupons (coupon_id) ON DELETE SET NULL,
        coupon_code VARCHAR(50),
        coupon_discount DECIMAL(10, 2) DEFAULT 0,
        total_amount DECIMAL(10, 2) NOT NULL,
        currency SMALLINT NOT NULL DEFAULT 0,
        customer_notes TEXT,
        admin_notes TEXT,
        created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
        updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
        cancelled_at TIMESTAMP
    );

COMMENT ON TABLE orders IS 'Core order data. Shipping, payment, and fulfillment information separated into dedicated tables per Single Responsibility Principle.';

COMMENT ON COLUMN orders.order_number IS 'Human-readable order identifier (e.g., ORD-2025-001234)';

COMMENT ON COLUMN orders.status IS 'Order lifecycle - enforced by order_status_enum type';

COMMENT ON COLUMN orders.currency IS 'Order currency - enforced by currency_enum type (reused from user_profiles)';

-- Order items table
-- Snapshot of product data at time of purchase
CREATE TABLE
    order_items (
        order_item_id SERIAL PRIMARY KEY,
        order_id INTEGER NOT NULL REFERENCES orders (order_id) ON DELETE CASCADE,
        sku_id INTEGER NOT NULL REFERENCES product_skus (sku_id) ON DELETE RESTRICT,
        product_name VARCHAR(255) NOT NULL,
        sku VARCHAR(100) NOT NULL,
        variant_description TEXT,
        seller_id INTEGER NOT NULL REFERENCES users (user_id) ON DELETE RESTRICT,
        quantity INTEGER NOT NULL,
        unit_price DECIMAL(10, 2) NOT NULL,
        subtotal DECIMAL(10, 2) NOT NULL,
        created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
    );

COMMENT ON TABLE order_items IS 'Order line items with snapshot data. Preserves historical data even if product is deleted. Populated via trigger.';

COMMENT ON COLUMN order_items.variant_description IS 'Human-readable variant description (e.g., "Red, Size L")';

-- Order shipping information
-- Separated per SRP - one record per order
CREATE TABLE
    order_shipping (
        shipping_id SERIAL PRIMARY KEY,
        order_id INTEGER UNIQUE NOT NULL REFERENCES orders (order_id) ON DELETE CASCADE,
        recipient_name VARCHAR(200) NOT NULL,
        phone VARCHAR(20) NOT NULL,
        address_line1 VARCHAR(255) NOT NULL,
        address_line2 VARCHAR(255),
        city VARCHAR(100) NOT NULL,
        state_province VARCHAR(100),
        postal_code VARCHAR(20),
        country VARCHAR(100) NOT NULL DEFAULT 'Vietnam',
        shipping_method SMALLINT,
        created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
    );

COMMENT ON TABLE order_shipping IS 'Separated shipping information per Single Responsibility Principle. One record per order.';

COMMENT ON COLUMN order_shipping.shipping_method IS 'Shipping method - enforced by shipping_method_enum type';

-- Order payments table
-- Supports multiple payment attempts per order
CREATE TABLE
    order_payments (
        payment_id SERIAL PRIMARY KEY,
        order_id INTEGER NOT NULL REFERENCES orders (order_id) ON DELETE CASCADE,
        payment_method SMALLINT NOT NULL,
        payment_status SMALLINT NOT NULL DEFAULT 0,
        amount DECIMAL(10, 2) NOT NULL,
        payment_gateway VARCHAR(50), -- stripe, paypal, vnpay, momo -- enum?
        transaction_id VARCHAR(255),
        gateway_response JSONB,
        paid_at TIMESTAMP,
        refunded_at TIMESTAMP,
        failure_reason TEXT,
        retry_count INTEGER NOT NULL DEFAULT 0,
        created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
        updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
    );

COMMENT ON TABLE order_payments IS 'Separated payment information. Supports multiple payment attempts per order for retry logic.';

COMMENT ON COLUMN order_payments.payment_method IS 'Payment method - enforced by payment_method_enum type';

COMMENT ON COLUMN order_payments.payment_status IS 'Payment status - enforced by payment_status_enum type';

COMMENT ON COLUMN order_payments.gateway_response IS 'Complete JSONB response from payment gateway for audit and debugging';

-- Order fulfillment table
-- Shipping and delivery tracking information
CREATE TABLE
    order_fulfillment (
        fulfillment_id SERIAL PRIMARY KEY,
        order_id INTEGER UNIQUE NOT NULL REFERENCES orders (order_id) ON DELETE CASCADE,
        tracking_number VARCHAR(100),
        carrier VARCHAR(100), -- enum?
        shipping_label_url VARCHAR(500),
        shipped_at TIMESTAMP,
        estimated_delivery_date DATE,
        delivered_at TIMESTAMP,
        delivery_proof_url VARCHAR(500),
        notes TEXT,
        created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
        updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
    );

COMMENT ON TABLE order_fulfillment IS 'Separated fulfillment and tracking information. One record per order.';

COMMENT ON COLUMN order_fulfillment.delivery_proof_url IS 'URL to photo proof of delivery (signature, package photo, etc.)';

-- Order status history table
-- Complete audit trail of status changes
CREATE TABLE
    order_status_history (
        history_id SERIAL PRIMARY KEY,
        order_id INTEGER NOT NULL REFERENCES orders (order_id) ON DELETE CASCADE,
        old_status SMALLINT,
        new_status SMALLINT NOT NULL,
        notes TEXT,
        changed_by INTEGER REFERENCES users (user_id) ON DELETE SET NULL,
        created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
        updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
    );

COMMENT ON TABLE order_status_history IS 'Complete audit trail for order status changes. Essential for compliance and customer service.';

COMMENT ON COLUMN order_status_history.old_status IS 'Previous status - enforced by order_status_enum type (NULL for initial creation)';

COMMENT ON COLUMN order_status_history.new_status IS 'New status - enforced by order_status_enum type';

COMMENT ON COLUMN order_status_history.changed_by IS 'User ID who initiated change, or NULL for system-triggered changes';

-- ====================================
-- 5. EVENT-DRIVEN PROCESSING TABLES
-- ====================================
-- Processed events table
-- Idempotency tracking for event processing
CREATE TABLE
    processed_events (
        event_id UUID NOT NULL,
        event_type VARCHAR(100) NOT NULL,
        processed_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
        processed_by VARCHAR(100) NOT NULL,
        PRIMARY KEY (event_id, processed_by)
    );

COMMENT ON TABLE processed_events IS 'Idempotency tracking ONLY. Small, fast table for duplicate event prevention in distributed systems.';

COMMENT ON COLUMN processed_events.event_id IS 'UUID v4 - ensures global uniqueness across distributed systems';

-- Event log table
-- Complete audit trail of all event processing
CREATE TABLE
    event_logs (
        log_id SERIAL PRIMARY KEY,
        event_id UUID NOT NULL,
        event_type VARCHAR(100) NOT NULL, -- enum?
        attempt_number INTEGER NOT NULL DEFAULT 1,
        status SMALLINT NOT NULL,
        worker_name VARCHAR(100), -- enum?
        order_id INTEGER REFERENCES orders (order_id) ON DELETE SET NULL,
        payload JSONB,
        error_message TEXT,
        error_code VARCHAR(50),
        stack_trace TEXT,
        started_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
        completed_at TIMESTAMP,
        processing_time_ms INTEGER
    );

COMMENT ON TABLE event_logs IS 'Complete audit trail of ALL event processing attempts (successes and failures). Critical for debugging distributed systems.';

COMMENT ON COLUMN event_logs.attempt_number IS 'Retry attempt number - increments with each retry';

-- Dead letter queue table
-- Failed events requiring manual intervention
CREATE TABLE
    dead_letter_queue (
        dlq_id SERIAL PRIMARY KEY,
        event_id UUID UNIQUE NOT NULL,
        event_type VARCHAR(100) NOT NULL, -- enum?
        original_queue VARCHAR(100) NOT NULL,
        payload JSONB NOT NULL,
        final_error_message TEXT NOT NULL,
        total_retry_attempts INTEGER NOT NULL,
        first_failed_at TIMESTAMP NOT NULL,
        moved_to_dlq_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
        resolution_status VARCHAR(20) NOT NULL DEFAULT 'pending',
        reprocessed_at TIMESTAMP,
        reprocessed_by INTEGER REFERENCES users (user_id) ON DELETE SET NULL,
        resolution_notes TEXT
    );

COMMENT ON TABLE dead_letter_queue IS 'Permanently failed events requiring manual admin intervention. Critical for error recovery and system reliability.';

COMMENT ON COLUMN dead_letter_queue.resolution_status IS 'Resolution state: pending, reprocessed, discarded, resolved';

-- ====================================
-- 6. REVIEWS & RATINGS TABLES
-- ====================================
-- Reviews table
-- Verified purchase reviews only
CREATE TABLE
    reviews (
        review_id SERIAL PRIMARY KEY,
        product_id INTEGER NOT NULL REFERENCES products (product_id) ON DELETE CASCADE,
        user_id INTEGER NOT NULL REFERENCES users (user_id) ON DELETE CASCADE,
        order_item_id INTEGER NOT NULL REFERENCES order_items (order_item_id) ON DELETE RESTRICT,
        rating INTEGER NOT NULL,
        title VARCHAR(200),
        comment TEXT,
        is_verified_purchase BOOLEAN NOT NULL DEFAULT TRUE,
        is_approved BOOLEAN NOT NULL DEFAULT TRUE,
        helpful_count INTEGER NOT NULL DEFAULT 0,
        unhelpful_count INTEGER NOT NULL DEFAULT 0,
        moderation_notes TEXT,
        moderated_by INTEGER REFERENCES users (user_id) ON DELETE SET NULL,
        created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
        updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
        UNIQUE (user_id, order_item_id) -- One review per order item
    );

COMMENT ON TABLE reviews IS 'Product reviews. Requires valid order_item_id - users can review each purchased item separately. order_item_id automatically validates user purchased the product.';

COMMENT ON COLUMN reviews.order_item_id IS 'REQUIRED - links to specific purchased item. Automatically verifies purchase and allows reviewing multiple products from same order.';

COMMENT ON COLUMN reviews.rating IS 'Rating from 1 to 5 stars';

-- Review images table
-- Multiple images per review
CREATE TABLE
    review_images (
        image_id SERIAL PRIMARY KEY,
        review_id INTEGER NOT NULL REFERENCES reviews (review_id) ON DELETE CASCADE,
        image_url VARCHAR(500) NOT NULL,
        thumbnail_url VARCHAR(500),
        display_order INTEGER NOT NULL DEFAULT 0,
        created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
    );

COMMENT ON TABLE review_images IS 'Customer-uploaded images attached to product reviews. Enhances review credibility.';

-- ====================================
-- 7. ADMIN & AUDIT LOGS TABLES
-- ====================================
-- System logs table
-- General application logging
CREATE TABLE
    system_logs (
        log_id SERIAL PRIMARY KEY,
        log_level VARCHAR(20) NOT NULL, -- enum?
        log_type VARCHAR(50) NOT NULL, -- enum?
        source VARCHAR(100),
        message TEXT NOT NULL,
        details JSONB,
        user_id INTEGER REFERENCES users (user_id) ON DELETE SET NULL,
        request_id UUID,
        ip_address VARCHAR(45),
        stack_trace TEXT,
        created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
    );

COMMENT ON TABLE system_logs IS 'General system logs for debugging and monitoring. Partition by date for performance. Retention policy recommended.';

COMMENT ON COLUMN system_logs.request_id IS 'Trace ID for correlating logs across distributed services';

-- ====================================
-- 8. NOTIFICATIONS TABLE
-- ====================================
-- Notifications table
-- Multi-channel notification system
CREATE TABLE
    notifications (
        notification_id SERIAL PRIMARY KEY,
        user_id INTEGER NOT NULL REFERENCES users (user_id) ON DELETE CASCADE,
        notification_type VARCHAR(50) NOT NULL,
        channel SMALLINT NOT NULL,
        priority SMALLINT NOT NULL DEFAULT 0,
        subject VARCHAR(200),
        message TEXT NOT NULL,
        status SMALLINT NOT NULL DEFAULT 0,
        related_entity_type SMALLINT,
        related_entity_id INTEGER,
        email_sent_at TIMESTAMP,
        read_at TIMESTAMP,
        created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
        retry_count INTEGER NOT NULL DEFAULT 0,
        last_retry_at TIMESTAMP
    );

COMMENT ON TABLE notifications IS 'Multi-channel notification system supporting email, SMS, in-app, and push notifications.';

COMMENT ON COLUMN notifications.notification_type IS 'Notification type - enforced by notification_type_enum type';

COMMENT ON COLUMN notifications.channel IS 'Delivery channel - enforced by notification_channel_enum type';

COMMENT ON COLUMN notifications.priority IS 'Priority level - enforced by notification_priority_enum type';

COMMENT ON COLUMN notifications.status IS 'Notification status - enforced by notification_status_enum type';

COMMENT ON COLUMN notifications.related_entity_type IS 'Related entity type - enforced by entity_type_enum type';

-- ====================================
-- 9. ANALYTICS & REPORTING TABLES
-- ====================================
-- User item interactions table
-- Raw data for recommendation engine
CREATE TABLE
    user_item_interactions (
        interaction_id SERIAL PRIMARY KEY,
        user_id INTEGER NOT NULL REFERENCES users (user_id) ON DELETE CASCADE,
        product_id INTEGER NOT NULL REFERENCES products (product_id) ON DELETE CASCADE,
        interaction_type VARCHAR(20) NOT NULL, -- view, click, add_to_cart, purchase, wishlist -- enum?
        weight INTEGER NOT NULL DEFAULT 1, -- Scoring: view=1, click=2, add_to_cart=3, purchase=5
        session_id VARCHAR(255),
        referrer_url VARCHAR(500),
        created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
    );

COMMENT ON TABLE user_item_interactions IS 'Raw interaction data for AI/ML recommendation engine. High-volume table - consider partitioning by date.';

COMMENT ON COLUMN user_item_interactions.weight IS 'Interaction weight for recommendation scoring: view=1, click=2, add_to_cart=3, purchase=5';

-- Product metrics table
-- Daily aggregated metrics for dashboards
CREATE TABLE
    product_metrics (
        metric_id SERIAL PRIMARY KEY,
        product_id INTEGER NOT NULL REFERENCES products (product_id) ON DELETE CASCADE,
        date DATE NOT NULL,
        view_count INTEGER NOT NULL DEFAULT 0,
        click_count INTEGER NOT NULL DEFAULT 0,
        add_to_cart_count INTEGER NOT NULL DEFAULT 0,
        purchase_count INTEGER NOT NULL DEFAULT 0,
        revenue DECIMAL(10, 2) NOT NULL DEFAULT 0,
        average_rating DECIMAL(3, 2),
        review_count INTEGER NOT NULL DEFAULT 0,
        created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
        UNIQUE (product_id, date) -- One record per product per day
    );

COMMENT ON TABLE product_metrics IS 'Daily aggregated product metrics for dashboards and reporting. Updated via scheduled cron job.';

COMMENT ON COLUMN product_metrics.date IS 'Metric date - one record per product per day for efficient querying';

-- MassTransit EF Outbox/Inbox tables
CREATE TABLE IF NOT EXISTS "InboxState" (
    "Id" BIGINT GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
    "MessageId" UUID NOT NULL,
    "ConsumerId" UUID NOT NULL,
    "LockId" UUID NOT NULL,
    "RowVersion" BYTEA,
    "Received" TIMESTAMPTZ NOT NULL,
    "ReceiveCount" INTEGER NOT NULL,
    "ExpirationTime" TIMESTAMPTZ NULL,
    "Consumed" TIMESTAMPTZ NULL,
    "Delivered" TIMESTAMPTZ NULL,
    "LastSequenceNumber" BIGINT NULL,
    CONSTRAINT "AK_InboxState_MessageId_ConsumerId" UNIQUE ("MessageId", "ConsumerId")
);

CREATE TABLE IF NOT EXISTS "OutboxState" (
    "OutboxId" UUID PRIMARY KEY,
    "LockId" UUID NOT NULL,
    "RowVersion" BYTEA,
    "Created" TIMESTAMPTZ NOT NULL,
    "Delivered" TIMESTAMPTZ NULL,
    "LastSequenceNumber" BIGINT NULL
);

CREATE TABLE IF NOT EXISTS "OutboxMessage" (
    "SequenceNumber" BIGINT GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
    "EnqueueTime" TIMESTAMPTZ NULL,
    "SentTime" TIMESTAMPTZ NOT NULL,
    "Headers" TEXT NULL,
    "Properties" TEXT NULL,
    "InboxMessageId" UUID NULL,
    "InboxConsumerId" UUID NULL,
    "OutboxId" UUID NULL,
    "MessageId" UUID NOT NULL,
    "ContentType" VARCHAR(256) NOT NULL,
    "MessageType" TEXT NOT NULL,
    "Body" TEXT NOT NULL,
    "ConversationId" UUID NULL,
    "CorrelationId" UUID NULL,
    "InitiatorId" UUID NULL,
    "RequestId" UUID NULL,
    "SourceAddress" VARCHAR(256) NULL,
    "DestinationAddress" VARCHAR(256) NULL,
    "ResponseAddress" VARCHAR(256) NULL,
    "FaultAddress" VARCHAR(256) NULL,
    "ExpirationTime" TIMESTAMPTZ NULL,
    CONSTRAINT "FK_OutboxMessage_InboxState_InboxMessageId_InboxConsumerId"
        FOREIGN KEY ("InboxMessageId", "InboxConsumerId")
        REFERENCES "InboxState" ("MessageId", "ConsumerId"),
    CONSTRAINT "FK_OutboxMessage_OutboxState_OutboxId"
        FOREIGN KEY ("OutboxId")
        REFERENCES "OutboxState" ("OutboxId")
);

CREATE INDEX IF NOT EXISTS "IX_InboxState_Delivered"
    ON "InboxState" ("Delivered");

CREATE INDEX IF NOT EXISTS "IX_OutboxMessage_EnqueueTime"
    ON "OutboxMessage" ("EnqueueTime");

CREATE INDEX IF NOT EXISTS "IX_OutboxMessage_ExpirationTime"
    ON "OutboxMessage" ("ExpirationTime");

CREATE UNIQUE INDEX IF NOT EXISTS "IX_OutboxMessage_InboxMessageId_InboxConsumerId_SequenceNumber"
    ON "OutboxMessage" ("InboxMessageId", "InboxConsumerId", "SequenceNumber");

CREATE UNIQUE INDEX IF NOT EXISTS "IX_OutboxMessage_OutboxId_SequenceNumber"
    ON "OutboxMessage" ("OutboxId", "SequenceNumber");

CREATE INDEX IF NOT EXISTS "IX_OutboxState_Created"
    ON "OutboxState" ("Created");