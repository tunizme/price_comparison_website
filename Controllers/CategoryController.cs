using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using price_comparison.Models;
using price_comparison.Repository;

namespace price_comparison.Controllers;

public class CategoryController : Controller
{
    private readonly DataContext _dataContext;

    public CategoryController(DataContext dataContext)
    {
        _dataContext = dataContext;
    }

    public async Task<IActionResult> Index(String Slug = "")
    {
        CategoryModel categoryModel = _dataContext.Categories.FirstOrDefault(c => c.Slug == Slug);
        if(categoryModel == null) return RedirectToAction("Index");
        var products = _dataContext.Products.Where(p => p.CategoryId == categoryModel.Id);
        return View(await products.OrderByDescending(p => p.Id).ToListAsync());
    }
}