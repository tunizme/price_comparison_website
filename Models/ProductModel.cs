using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using price_comparison.Repository.Validation;

namespace price_comparison.Models;

public class ProductModel
{
    [Key] public int Id { get; set; }

    [Required(ErrorMessage = "Nhập tên sản phẩm!!")]
    public string Name { get; set; }

    [Required(ErrorMessage = "Nhập mô tả sản phẩm!!")]
    public string Description { get; set; }


    public string Image { get; set; }

    public string Slug { get; set; }

    [Required, Range(1, int.MaxValue, ErrorMessage = "Chọn một danh mục!!")]
    public int CategoryId { get; set; }

    [Required, Range(1, int.MaxValue, ErrorMessage = "Chọn một thương hiệu!!")]
    public int BrandId { get; set; }

    public CategoryModel Category { get; set; }

    public BrandModel Brand { get; set; }

    public List<ProductPriceModel> Prices { get; set; }

    [NotMapped] [FileExtension] public IFormFile ImageUpload { get; set; }
}