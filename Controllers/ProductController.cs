using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using price_comparison.Repository;

namespace price_comparison.Controllers;

public class ProductController : Controller
{
    private readonly DataContext _dataContext;

    public ProductController(DataContext dataContext)
    {
        _dataContext = dataContext;
    }

    public IActionResult Details(int id)
    {
        var product = _dataContext.Products.Include(p => p.Prices).FirstOrDefault(p => p.Id == id);
        if (product == null)
        {
            return NotFound();
        }

        var recommendedItems = _dataContext.Products
            .Where(p => p.CategoryId == product.CategoryId && p.Id != product.Id).Include(p => p.Prices)
            .Take(6)
            .ToList();
        
        ViewData["RecommendedItems"] = recommendedItems;

        return View(product);
    }
}