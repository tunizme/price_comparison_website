using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using price_comparison.Models;
using price_comparison.Repository;

namespace price_comparison.Controllers;

public class BrandController : Controller
{
    private readonly DataContext _dataContext;

    public BrandController(DataContext dataContext)
    {
        _dataContext = dataContext;
    }
    public async Task<IActionResult> Index(String Slug = "")
    {
        BrandModel brandModel = _dataContext.Brands.FirstOrDefault(c => c.Slug == Slug);
        if(brandModel == null) return RedirectToAction("Index");
        var products = _dataContext.Products.Where(p => p.BrandId == brandModel.Id);
        return View(await products.OrderByDescending(p => p.Id).ToListAsync());
    }
}