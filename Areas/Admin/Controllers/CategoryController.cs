using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using price_comparison.Models;
using price_comparison.Repository;

namespace price_comparison.Areas.Admin.Controllers;

[Area("Admin")]
public class CategoryController : Controller
{
    private readonly DataContext _dataContext;

    public CategoryController(DataContext dataContext)
    {
        _dataContext = dataContext;
    }

    public async Task<IActionResult> Index(int pg = 1)
    {
        List<CategoryModel> categories = await _dataContext.Categories.ToListAsync();
        
        const int pageSize = 10;
        if (pg < 1)
        {
            pg = 1;
        }
        int recsCount = categories.Count();
        var pager = new Paginate(recsCount,pg,pageSize);
        int recSkip = (pg - 1) * pageSize;
        var data = categories.Skip(recSkip).Take(pager.PageSize).ToList();
        ViewBag.Pager = pager;
        return View(data);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CategoryModel category)
    {
        if (ModelState.IsValid)
        {
            category.Slug = category.Slug ?? category.Name.Replace(" ", "_");
            var slug = await _dataContext.Categories.FirstOrDefaultAsync(c => c.Slug == category.Slug);
            if (slug != null)
            {
                ModelState.AddModelError(nameof(category.Slug), "Category slug already exists");
                return View(category);
            }

            _dataContext.Add(category);
            await _dataContext.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        else
        {
            return BadRequest(error: "Category slug already exists");
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        CategoryModel category = await _dataContext.Categories
            .FirstOrDefaultAsync(p => p.Id == id);
        return View(category);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(CategoryModel category)
    {
        if (ModelState.IsValid)
        {
            category.Slug = category.Name.Replace(" ", "_");
            var slug = await _dataContext.Categories.FirstOrDefaultAsync(c => c.Slug == category.Slug);
            if (slug != null)
            {
                ModelState.AddModelError(nameof(category.Slug), "Category slug already exists");
                return View(category);
            }

            _dataContext.Update(category);
            await _dataContext.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        else
        {
            return BadRequest(error: "Category slug already exists");
        }
        return View(category);
    }

    public async Task<IActionResult> Delete(int id)
    {
        CategoryModel category = await _dataContext.Categories.FirstOrDefaultAsync(p => p.Id == id);
        _dataContext.Categories.Remove(category);
        await _dataContext.SaveChangesAsync();
        return RedirectToAction("Index");
    }
}