using System;
using System.Collections.Generic;

namespace WebBanNuocMVC.Data;

public partial class Category
{
    public int CategoryId { get; set; }

    public string? CategoryName { get; set; }

    public virtual ICollection<Drink> Drinks { get; set; } = new List<Drink>();
}
