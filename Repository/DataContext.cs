using Microsoft.EntityFrameworkCore;
using price_comparison.Models;

namespace price_comparison.Repository;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
    }

    public DbSet<BrandModel> Brands { get; set; }
    public DbSet<CategoryModel> Categories { get; set; }
    public DbSet<ProductModel> Products { get; set; }
    


}