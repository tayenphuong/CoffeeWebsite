using System;
using System.Collections.Generic;

namespace WebBanNuocMVC.Data;

public partial class Order
{
    public int OrderId { get; set; }

    public int? TableId { get; set; }

    public int? CustomerId { get; set; }

    public int? AccountId { get; set; }

    public DateTime? OrderDate { get; set; }

    public decimal? TotalAmount { get; set; }

    public string? Status { get; set; }

    public virtual Account? Account { get; set; }

    public virtual Customer? Customer { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual CafeTable? Table { get; set; }
}
