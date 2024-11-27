using System.ComponentModel.DataAnnotations;

namespace price_comparison.Models;

public class BrandModel
{
    [Key]
    public int Id { get; set; }
    
    [Required, MinLength(4, ErrorMessage = "Nhập tên thương hiệu!!")]
    public string Name { get; set; }
    
    [Required, MinLength(4, ErrorMessage = "Nhập mô tả thương hiệu!!")]
    public string Description { get; set; }
    
    [Required]
    public string Slug { get; set; }
    
    [Required]
    public int Status {get; set;}
}