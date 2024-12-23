﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace price_comparison.Models;

public class ProductPriceModel
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int ProductId { get; set; } 

    [ForeignKey("ProductId")]
    public ProductModel Product { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Price { get; set; } 

    [Required]
    public string ShopName { get; set; }

    [Required]
    public string ShopUrl { get; set; } // Đường dẫn sản phẩm tại shop
}