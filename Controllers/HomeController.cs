using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using price_comparison.Models;
using price_comparison.Repository;

namespace price_comparison.Controllers;

public class HomeController : Controller
{
    private readonly DataContext _dataContext;
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger, DataContext dataContext)
    {
        _logger = logger;
        _dataContext = dataContext;
    }

    public IActionResult Index()
    {
        var products = _dataContext.Products.Include(p => p.Prices).Take(8);
        var smartphone = _dataContext.Products
            .Where(p => p.Category.Name == "Điện thoại")
            .Include(p => p.Prices)
            .OrderBy(p => Guid.NewGuid())  
            .Take(16)
            .ToList();
        
        var laptop = _dataContext.Products
            .Where(p => p.Category.Name == "Laptop")
            .Include(p => p.Prices)
            .OrderBy(p => Guid.NewGuid())
            .Take(16)
            .ToList();
        
        var screen = _dataContext.Products
            .Where(p => p.Category.Name == "Màn hình")
            .Include(p => p.Prices)
            .OrderBy(p => Guid.NewGuid())
            .Take(16)
            .ToList();
        
        var smartwatch = _dataContext.Products
            .Where(p => p.Category.Name == "Đồng hồ")
            .Include(p => p.Prices)
            .OrderBy(p => Guid.NewGuid())
            .Take(16)
            .ToList();
        
         var tivi = _dataContext.Products
            .Where(p => p.Category.Name == "Tivi")
            .Include(p => p.Prices)
            .OrderBy(p => Guid.NewGuid())
            .Take(16)
            .ToList();
        
        ViewData["smartphone"] = smartphone;
        ViewData["laptop"] = laptop;
        ViewData["screen"] = screen;
        ViewData["smartwatch"] = smartwatch;
        ViewData["tivi"] = tivi;

        return View(products);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
