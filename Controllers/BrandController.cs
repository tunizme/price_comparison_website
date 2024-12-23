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
    public async Task<IActionResult> Index(String Slug = "", int pg=1)
    {
        BrandModel brandModel = _dataContext.Brands.FirstOrDefault(c => c.Slug == Slug);
        if(brandModel == null) return RedirectToAction("Index");
        List<ProductModel> products = await _dataContext.Products.Where(p => p.BrandId == brandModel.Id)
            .Include(p => p.Prices).ToListAsync();
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