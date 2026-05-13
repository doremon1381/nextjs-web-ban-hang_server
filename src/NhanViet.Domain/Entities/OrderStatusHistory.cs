using NhanViet.Domain.Enums;

namespace NhanViet.Domain.Entities;

public class OrderStatusHistory
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public OrderStatus Status { get; set; }
    public DateTime Timestamp { get; set; }
    public string? Reason { get; set; }
}
