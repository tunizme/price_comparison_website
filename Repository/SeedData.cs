using Microsoft.EntityFrameworkCore;
using price_comparison.Models;

namespace price_comparison.Repository;

public class SeedData
{
    public static void SeedingData(DataContext context)
    {
        context.Database.Migrate();
        if (!context.Products.Any())
        {
            CategoryModel smartphone = new CategoryModel{Name = "smartphone", Slug  = "smartphone", Description = "smartphone smartphone", Status = 1};
            CategoryModel pc = new CategoryModel{Name = "PC", Slug  = "PC", Description = "PC PC", Status = 1};
            BrandModel apple = new BrandModel{Name = "apple", Slug = "apple", Description = "apple apple", Status = 1};
            BrandModel asus = new BrandModel{Name = "asus", Slug = "asus", Description = "asus asus", Status = 1};
            context.Products.AddRange(
            
                new ProductModel
                    { Name = "Iphone 16",Slug = "iphone16",Description = "Iphone 16 van den", Image = "1.png", Category = smartphone, Brand = apple},
                new ProductModel
                    { Name = "Iphone 12", Slug = "iphone12",Description = "Iphone 12 van den", Image = "1.png", Category = smartphone, Brand = apple},
                new ProductModel
                    { Name = "Iphone 13", Slug = "iphone13",Description = "Iphone 13 van den", Image = "1.png", Category = smartphone, Brand = apple},
                new ProductModel
                    { Name = "PC gaming asus", Slug = "pc-gaming",Description = "PC gaming asus", Image = "1.png", Category = pc, Brand = asus}
            );
            context.SaveChanges();
        }
    }
}