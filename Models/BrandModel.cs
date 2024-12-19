using System.ComponentModel.DataAnnotations;

namespace price_comparison.Models;

public class BrandModel
{
    [Key]
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Nhập tên thương hiệu!!")]
    public string Name { get; set; }
    
    [Required(ErrorMessage = "Nhập mô tả thương hiệu!!")]
    public string Description { get; set; }
    
    public string Slug { get; set; }
    
    [Required]
    public int Status {get; set;}
}