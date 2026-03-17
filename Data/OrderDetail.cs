using System;
using System.Collections.Generic;

namespace WebBanNuocMVC.Data;

public partial class OrderDetail
{
    public int OrderDetailId { get; set; }

    public int? OrderId { get; set; }

    public int? DrinkId { get; set; }

    public int? Quantity { get; set; }

    public decimal? UnitPrice { get; set; }

    public virtual Drink? Drink { get; set; }

    public virtual Order? Order { get; set; }
}
