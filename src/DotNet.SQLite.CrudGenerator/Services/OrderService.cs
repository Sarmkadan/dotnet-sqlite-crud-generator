// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNet.SQLite.CrudGenerator.Enums;
using DotNet.SQLite.CrudGenerator.Exceptions;
using DotNet.SQLite.CrudGenerator.Interfaces;
using DotNet.SQLite.CrudGenerator.Models;
using DotNet.SQLite.CrudGenerator.Data;

namespace DotNet.SQLite.CrudGenerator.Services;

/// <summary>
/// Service for managing order operations and order lifecycle.
/// </summary>
public class OrderService : IService<Order, int>
{
    private readonly IRepository<Order, int> _orderRepository;
    private readonly IRepository<User, int> _userRepository;
    private readonly IRepository<AuditLog, int> _auditLogRepository;

    public OrderService(
        IRepository<Order, int> orderRepository,
        IRepository<User, int> userRepository,
        IRepository<AuditLog, int> auditLogRepository)
    {
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _auditLogRepository = auditLogRepository ?? throw new ArgumentNullException(nameof(auditLogRepository));
    }

    public async Task<Order?> GetAsync(int id, CancellationToken cancellationToken = default)
    {
        if (id <= 0)
            throw new ArgumentException("Order ID must be greater than 0", nameof(id));

        return await _orderRepository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<IEnumerable<Order>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _orderRepository.GetAllAsync(cancellationToken);
    }

    public async Task<Order> CreateAsync(Order entity, CancellationToken cancellationToken = default)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        if (!Validate(entity))
            throw new ValidationException("Order validation failed. Check required fields.");

        var user = await _userRepository.GetByIdAsync(entity.UserId, cancellationToken);
        if (user == null)
            throw new ValidationException($"User with ID {entity.UserId} does not exist.");

        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;

        var created = await _orderRepository.AddAsync(entity, cancellationToken);

        await RecordAuditLogAsync(nameof(Order), created.Id, OperationType.Create, null, entity.ToString(), "New order created", cancellationToken);

        return created;
    }

    public async Task<Order> UpdateAsync(Order entity, CancellationToken cancellationToken = default)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        if (entity.Id <= 0)
            throw new ArgumentException("Invalid order ID", nameof(entity));

        if (!Validate(entity))
            throw new ValidationException("Order validation failed. Check required fields.");

        var existing = await GetAsync(entity.Id, cancellationToken);
        if (existing == null)
            throw RepositoryException.EntityNotFound(nameof(Order), entity.Id);

        entity.UpdatedAt = DateTime.UtcNow;
        var updated = await _orderRepository.UpdateAsync(entity, cancellationToken);

        await RecordAuditLogAsync(nameof(Order), entity.Id, OperationType.Update, existing.ToString(), updated.ToString(), "Order updated", cancellationToken);

        return updated;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        if (id <= 0)
            throw new ArgumentException("Order ID must be greater than 0", nameof(id));

        var order = await GetAsync(id, cancellationToken);
        if (order == null)
            throw RepositoryException.EntityNotFound(nameof(Order), id);

        var result = await _orderRepository.DeleteAsync(id, cancellationToken);

        if (result)
            await RecordAuditLogAsync(nameof(Order), id, OperationType.Delete, order.ToString(), null, "Order deleted", cancellationToken);

        return result;
    }

    public bool Validate(Order entity)
    {
        if (entity == null) return false;
        return entity.Validate();
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _orderRepository.ExistsAsync(id, cancellationToken);
    }

    /// <summary>
    /// Gets orders for a specific user.
    /// </summary>
    public async Task<IEnumerable<Order>> GetUserOrdersAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await ((OrderRepository)_orderRepository).GetByUserAsync(userId, cancellationToken);
    }

    /// <summary>
    /// Gets all pending orders.
    /// </summary>
    public async Task<IEnumerable<Order>> GetPendingOrdersAsync(CancellationToken cancellationToken = default)
    {
        return await ((OrderRepository)_orderRepository).GetPendingAsync(cancellationToken);
    }

    /// <summary>
    /// Ships an order if possible.
    /// </summary>
    public async Task<bool> ShipOrderAsync(int orderId, string trackingNumber, CancellationToken cancellationToken = default)
    {
        var order = await GetAsync(orderId, cancellationToken);
        if (order == null)
            return false;

        if (!order.CanShip())
            throw new ValidationException("Order cannot be shipped. It must be pending and have a shipping address.");

        order.UpdateStatus(EntityStatus.Shipped);
        await UpdateAsync(order, cancellationToken);
        return true;
    }

    /// <summary>
    /// Marks an order as delivered.
    /// </summary>
    public async Task<bool> MarkDeliveredAsync(int orderId, CancellationToken cancellationToken = default)
    {
        var order = await GetAsync(orderId, cancellationToken);
        if (order == null)
            return false;

        if (order.Status != EntityStatus.Shipped)
            throw new ValidationException("Only shipped orders can be marked as delivered.");

        order.UpdateStatus(EntityStatus.Delivered);
        await UpdateAsync(order, cancellationToken);
        return true;
    }

    /// <summary>
    /// Calculates order metrics.
    /// </summary>
    public async Task<OrderMetrics> GetMetricsAsync(CancellationToken cancellationToken = default)
    {
        var orders = (await GetAllAsync(cancellationToken)).ToList();
        var pending = await GetPendingOrdersAsync(cancellationToken);
        var delivered = orders.Where(o => o.Status == EntityStatus.Delivered).ToList();

        return new OrderMetrics
        {
            TotalOrders = orders.Count,
            PendingOrders = pending.Count(),
            DeliveredOrders = delivered.Count,
            TotalRevenue = orders.Sum(o => o.CalculateFinalTotal()),
            AverageOrderValue = orders.Any() ? orders.Average(o => (double)o.TotalAmount) : 0,
            AverageTaxAmount = orders.Any() ? orders.Average(o => (double)o.TaxAmount) : 0,
            TotalDiscounts = orders.Sum(o => o.DiscountAmount)
        };
    }

    private async Task RecordAuditLogAsync(string entityType, int entityId, OperationType operationType,
        string? oldValues, string? newValues, string? reason, CancellationToken cancellationToken = default)
    {
        var auditLog = new AuditLog
        {
            EntityType = entityType,
            EntityId = entityId,
            OperationType = operationType,
            ChangedByUserId = 1,
            OldValues = oldValues,
            NewValues = newValues,
            ChangeReason = reason,
            Timestamp = DateTime.UtcNow
        };

        await _auditLogRepository.AddAsync(auditLog, cancellationToken);
    }
}

/// <summary>
/// Metrics about order operations.
/// </summary>
public class OrderMetrics
{
    public int TotalOrders { get; set; }
    public int PendingOrders { get; set; }
    public int DeliveredOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public double AverageOrderValue { get; set; }
    public double AverageTaxAmount { get; set; }
    public decimal TotalDiscounts { get; set; }
}
