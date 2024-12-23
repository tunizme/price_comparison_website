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
    
    public async Task<IActionResult> Index(String Slug = "", int pg = 1)
    {
        CategoryModel categoryModel = _dataContext.Categories.FirstOrDefault(c => c.Slug == Slug);
        if(categoryModel == null) return RedirectToAction("Index");
        List<ProductModel> products = await _dataContext.Products.Where(p => p.CategoryId == categoryModel.Id).Include(p=>p.Prices).ToListAsync();
        const int pageSize = 9;
        if (pg < 1)
        {
            pg = 1;
        }
        int recsCount = products.Count();
        var pager = new Paginate(recsCount,pg,pageSize);
        int recSkip = (pg - 1) * pageSize;
        var data = products.Skip(recSkip).Take(pager.PageSize).ToList();
        ViewBag.Pager = pager;
        return View(data);
    }
}