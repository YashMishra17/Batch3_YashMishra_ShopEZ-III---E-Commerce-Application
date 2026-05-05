using Microsoft.EntityFrameworkCore;
using ShopEZ.API.Data;
using ShopEZ.API.DTOs;
using ShopEZ.API.Exceptions;
using ShopEZ.API.Models;
using ShopEZ.API.Repositories.Interfaces;
using ShopEZ.API.Services.Interfaces;

namespace ShopEZ.API.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;
        private readonly ApplicationDbContext _context;

        public OrderService(
            IOrderRepository orderRepository,
            IProductRepository productRepository,
            ApplicationDbContext context)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _context = context;
        }

        // ─────────────────────────────────────────────────────────────────────
        // GET ALL ORDERS
        // ─────────────────────────────────────────────────────────────────────
        public async Task<IEnumerable<OrderDTO>> GetAllOrdersAsync()
        {
            var orders = await _orderRepository.GetAllAsync();
            return orders.Select(o => MapToDTO(o)).ToList();
        }

        // ─────────────────────────────────────────────────────────────────────
        // GET ORDER BY ID
        // ─────────────────────────────────────────────────────────────────────
        public async Task<OrderDTO?> GetOrderByIdAsync(int id)
        {
            if (id <= 0)
                throw new AppException("Order ID must be a positive integer.", 400);

            var order = await _orderRepository.GetByIdAsync(id);

            if (order == null)
                return null;

            return MapToDTO(order);
        }

        // ─────────────────────────────────────────────────────────────────────
        // CREATE ORDER  —  FULL ASYNC TRANSACTION
        // ─────────────────────────────────────────────────────────────────────
        public async Task<OrderDTO> CreateOrderAsync(CreateOrderDTO dto)
        {
            // ── Step 1: Validate payload ──────────────────────────────────────
            if (dto == null)
                throw new AppException("Order data cannot be null.", 400);

            if (dto.CartItems == null || !dto.CartItems.Any())
                throw new AppException("Cart must contain at least one item.", 400);

            if (dto.UserId <= 0)
                throw new AppException("UserId must be a positive integer.", 400);

            // ── Step 2: Validate user exists ──────────────────────────────────
            bool userExists = await _context.Users
                .AnyAsync(u => u.UserId == dto.UserId);

            if (!userExists)
                throw new AppException(
                    $"User with ID {dto.UserId} does not exist.", 404);

            // ── Step 3: Validate quantity > 0 for each cart item ──────────────
            foreach (CartItemDTO cartItem in dto.CartItems)
            {
                if (cartItem.Quantity <= 0)
                    throw new AppException(
                        $"Quantity for ProductId {cartItem.ProductId} must be greater than zero.", 400);
            }

            // ── Step 4: Load all required products in a single DB query ───────
            List<int> productIds = dto.CartItems
                .Select(ci => ci.ProductId)
                .Distinct()
                .ToList();

            List<Product> products = await _context.Products
                .Where(p => productIds.Contains(p.ProductId))
                .ToListAsync();

            // ── Step 5: Validate product existence and stock availability ──────
            foreach (CartItemDTO cartItem in dto.CartItems)
            {
                Product? product = products
                    .FirstOrDefault(p => p.ProductId == cartItem.ProductId);

                if (product == null)
                    throw new AppException(
                        $"Product with ID {cartItem.ProductId} does not exist.", 404);

                if (product.Stock < cartItem.Quantity)
                    throw new AppException(
                        $"Insufficient stock for '{product.Name}'. " +
                        $"Available: {product.Stock}, Requested: {cartItem.Quantity}.", 400);
            }

            // ── Step 6: Build OrderItems list from cart ───────────────────────
            List<OrderItem> orderItems = dto.CartItems.Select(cartItem =>
            {
                Product product = products.First(p => p.ProductId == cartItem.ProductId);
                return new OrderItem
                {
                    ProductId = product.ProductId,
                    Quantity = cartItem.Quantity,
                    Price = product.Price    // snapshot price at time of purchase
                };
            }).ToList();

            // ── Step 7: Calculate total amount using LINQ ─────────────────────
            decimal totalAmount = orderItems.Sum(oi => oi.Price * oi.Quantity);

            // ── Step 8: Construct the Order entity ────────────────────────────
            var order = new Order
            {
                UserId = dto.UserId,
                OrderDate = DateTime.UtcNow,
                TotalAmount = totalAmount,
                OrderItems = orderItems
            };

            // ── Step 9: Begin async transaction ───────────────────────────────
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Deduct stock for every product in the cart
                foreach (CartItemDTO cartItem in dto.CartItems)
                {
                    Product product = products
                        .First(p => p.ProductId == cartItem.ProductId);

                    product.Stock -= cartItem.Quantity;
                    _context.Products.Update(product);
                }

                // Persist order + order items (cascade via navigation property)
                await _context.Orders.AddAsync(order);

                // Flush all changes to the database
                await _context.SaveChangesAsync();

                // ── CommitAsync: make all changes permanent ────────────────────
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                // ── RollbackAsync: revert every change if anything fails ────────
                await transaction.RollbackAsync();
                throw new AppException(
                    $"Order creation failed and was rolled back. Reason: {ex.Message}", 500);
            }

            // ── Step 10: Reload the saved order with full navigation data ──────
            Order? created = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.OrderId == order.OrderId);

            if (created == null)
                throw new AppException(
                    "Order was saved but could not be retrieved.", 500);

            return MapToDTO(created);
        }

        // ─────────────────────────────────────────────────────────────────────
        // PRIVATE MAPPING HELPER
        // ─────────────────────────────────────────────────────────────────────
        private static OrderDTO MapToDTO(Order o)
        {
            return new OrderDTO
            {
                OrderId = o.OrderId,
                UserId = o.UserId,
                UserName = o.User != null ? o.User.Name : string.Empty,
                OrderDate = o.OrderDate,
                TotalAmount = o.TotalAmount,
                OrderItems = o.OrderItems.Select(oi => new OrderItemDTO
                {
                    OrderItemId = oi.OrderItemId,
                    ProductId = oi.ProductId,
                    ProductName = oi.Product != null ? oi.Product.Name : string.Empty,
                    Quantity = oi.Quantity,
                    Price = oi.Price,
                    Subtotal = oi.Price * oi.Quantity
                }).ToList()
            };
        }
    }
}
