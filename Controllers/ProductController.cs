using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using price_comparison.Models;
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
            .OrderBy(p => Guid.NewGuid())
            .Take(10)
            .ToList();

        ViewData["RecommendedItems"] = recommendedItems;

        return View(product);
    }

    public async Task<IActionResult> Search(string searchString, int pg = 1)
    {
        List<ProductModel> products = await _dataContext.Products
            .Where(p => p.Name.Contains(searchString) || p.Description.Contains(searchString)).Include(p => p.Prices)
            .ToListAsync();
        const int pageSize = 9;
        if (pg < 1)
        {
            pg = 1;
        }

        int recsCount = products.Count();
        var pager = new Paginate(recsCount, pg, pageSize);
        int recSkip = (pg - 1) * pageSize;
        var data = products.Skip(recSkip).Take(pager.PageSize).ToList();
        ViewBag.Pager = pager;
        ViewBag.searchString = searchString;
        return View(data);
    }
}