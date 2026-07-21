using System;
using System.Collections.Generic;

namespace DotNet.SQLite.CrudGenerator.Events
{
    /// <summary>
    /// Domain events for entity lifecycle changes.
    /// Published when entities are created, updated, or deleted.
    /// Used for auditing, notifications, and cross-cutting concerns.
    /// </summary>
    public abstract class EntityChangedEvent<T> : DomainEvent where T : class
    {
        public T? Entity { get; protected set; }
        public string EntityType { get; protected set; } = typeof(T).Name;
    }

    public sealed class EntityCreatedEvent<T> : EntityChangedEvent<T> where T : class
    {
        public EntityCreatedEvent(object aggregateId, T entity)
        {
            AggregateId = aggregateId is Guid g ? g : Guid.NewGuid();
            Entity = entity;
            EventName = $"{typeof(T).Name}Created";
        }
    }

    public sealed class EntityUpdatedEvent<T> : EntityChangedEvent<T> where T : class
    {
        public T? OldEntity { get; set; }
        public Dictionary<string, (object? OldValue, object? NewValue)> Changes { get; set; } = new();

        public EntityUpdatedEvent(object aggregateId, T entity, T? oldEntity = null)
        {
            AggregateId = aggregateId is Guid g ? g : Guid.NewGuid();
            Entity = entity;
            OldEntity = oldEntity;
            EventName = $"{typeof(T).Name}Updated";
        }
    }

    public sealed class EntityDeletedEvent<T> : EntityChangedEvent<T> where T : class
    {
        public EntityDeletedEvent(object aggregateId, T? deletedEntity = null)
        {
            AggregateId = aggregateId is Guid g ? g : Guid.NewGuid();
            Entity = deletedEntity;
            EventName = $"{typeof(T).Name}Deleted";
        }
    }

    public sealed class BulkEntityChangedEvent<T> : DomainEvent where T : class
    {
        public int Count { get; set; }
        public string Operation { get; set; } = string.Empty;
        public List<T> Entities { get; set; } = new();

        public BulkEntityChangedEvent(int count, string operation, List<T> entities)
        {
            Count = count;
            Operation = operation;
            Entities = entities;
            AggregateId = Guid.NewGuid();
            EventName = $"Bulk{typeof(T).Name}Changed";
        }
    }

    /// <summary>
    /// Product-specific domain events.
    /// </summary>
    public sealed class ProductRestockedEvent : DomainEvent
    {
        public int ProductId { get; set; }
        public int QuantityAdded { get; set; }
        public int NewQuantity { get; set; }

        public ProductRestockedEvent(int productId, int quantityAdded, int newQuantity)
        {
            ProductId = productId;
            QuantityAdded = quantityAdded;
            NewQuantity = newQuantity;
            AggregateId = Guid.Parse($"00000000-0000-0000-0000-{productId:D12}");
            EventName = "ProductRestocked";
        }
    }

    public sealed class ProductSoldEvent : DomainEvent
    {
        public int ProductId { get; set; }
        public int QuantitySold { get; set; }
        public decimal Revenue { get; set; }
        public int RemainingQuantity { get; set; }

        public ProductSoldEvent(int productId, int quantitySold, decimal revenue, int remainingQuantity)
        {
            ProductId = productId;
            QuantitySold = quantitySold;
            Revenue = revenue;
            RemainingQuantity = remainingQuantity;
            AggregateId = Guid.Parse($"00000000-0000-0000-0000-{productId:D12}");
            EventName = "ProductSold";
        }
    }

    public sealed class LowStockWarningEvent : DomainEvent
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int CurrentStock { get; set; }
        public int ThresholdLevel { get; set; }

        public LowStockWarningEvent(int productId, string productName, int currentStock, int thresholdLevel)
        {
            ProductId = productId;
            ProductName = productName;
            CurrentStock = currentStock;
            ThresholdLevel = thresholdLevel;
            AggregateId = Guid.Parse($"00000000-0000-0000-0000-{productId:D12}");
            EventName = "LowStockWarning";
        }
    }

    /// <summary>
    /// Order-specific domain events.
    /// </summary>
    public sealed class OrderPlacedEvent : DomainEvent
    {
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public decimal TotalAmount { get; set; }
        public int ItemCount { get; set; }

        public OrderPlacedEvent(int orderId, int userId, decimal totalAmount, int itemCount)
        {
            OrderId = orderId;
            UserId = userId;
            TotalAmount = totalAmount;
            ItemCount = itemCount;
            AggregateId = Guid.Parse($"00000000-0000-0000-0000-{orderId:D12}");
            EventName = "OrderPlaced";
        }
    }

    public sealed class OrderCompletedEvent : DomainEvent
    {
        public int OrderId { get; set; }
        public DateTime CompletedAt { get; set; }

        public OrderCompletedEvent(int orderId)
        {
            OrderId = orderId;
            CompletedAt = DateTime.UtcNow;
            AggregateId = Guid.Parse($"00000000-0000-0000-0000-{orderId:D12}");
            EventName = "OrderCompleted";
        }
    }

    // ------------------------------------------------------------------------
    // New types required for the RecalculateTotalsAsync implementation
    // ------------------------------------------------------------------------

    /// <summary>
    /// Type of change that occurred to an entity.
    /// </summary>
    public enum ChangeType
    {
        Created,
        Updated,
        Deleted
    }

    /// <summary>
    /// Simple, non‑generic event used when only the entity type/id and change type are needed.
    /// </summary>
    public sealed class EntityChangedEvent
    {
        public string EntityType { get; set; } = default!;
        public int EntityId { get; set; }
        public ChangeType ChangeType { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
