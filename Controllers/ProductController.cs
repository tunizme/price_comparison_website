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
        var product = _dataContext.Products.Include(p=>p.Prices).FirstOrDefault(p => p.Id == id);
        if (product == null)
        {
            return NotFound();
        }
        return View(product);
    }
}