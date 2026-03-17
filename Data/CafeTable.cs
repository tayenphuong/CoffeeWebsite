using System;
using System.Collections.Generic;

namespace WebBanNuocMVC.Data;

public partial class CafeTable
{
    public int TableId { get; set; }

    public string? TableName { get; set; }

    public string? Status { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
