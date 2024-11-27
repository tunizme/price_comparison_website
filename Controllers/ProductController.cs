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
    public IActionResult Index()
    {
        return View();
    }
    public IActionResult Details(int? Id)
    {
        var product = _dataContext.Products.Include(p=>p.Prices).FirstOrDefault(p => p.Id == Id);
        return View(product);
    }
}