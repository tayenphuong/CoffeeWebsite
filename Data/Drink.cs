using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebBanNuocMVC.Data;

public partial class Drink
{
    public int DrinkId { get; set; }

    [Required(ErrorMessage = "Product name is required")]
    [StringLength(100, ErrorMessage = "Product name cannot exceed 100 characters")]
    public string? DrinkName { get; set; }

    [Required(ErrorMessage = "Category is required")]
    public int? CategoryId { get; set; }

    [Required(ErrorMessage = "Price is required")]
    [Range(0.01, 999999999, ErrorMessage = "Price must be greater than 0")]
    public decimal? Price { get; set; }

    [Required(ErrorMessage = "Description is required")]
    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Image is required")]
    public string? Image { get; set; }

    public virtual Category? Category { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}
