using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using price_comparison.Models;
using price_comparison.Repository;

namespace price_comparison.Areas.Admin.Controllers;

[Area("Admin")]
public class BrandController : Controller
{
    private readonly DataContext _dataContext;

    public BrandController(DataContext dataContext)
    {
        _dataContext = dataContext;
    }

    public async Task<IActionResult> Index(int pg = 1)
    {
        List<BrandModel> brands = await _dataContext.Brands.ToListAsync();

        const int pageSize = 10;
        if (pg < 1)
        {
            pg = 1;
        }

        int recsCount = brands.Count();
        var pager = new Paginate(recsCount, pg, pageSize);
        int recSkip = (pg - 1) * pageSize;
        var data = brands.Skip(recSkip).Take(pager.PageSize).ToList();
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
    public async Task<IActionResult> Create(BrandModel brand)
    {
        if (ModelState.IsValid)
        {
            brand.Slug = brand.Name.Replace(" ", "_");
            var slug = await _dataContext.Brands.FirstOrDefaultAsync(c => c.Slug == brand.Slug);
            if (slug != null)
            {
                ModelState.AddModelError(nameof(brand.Slug), "Brand slug already exists");
                return View(brand);
            }

            _dataContext.Add(brand);
            await _dataContext.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        else
        {
            return BadRequest(error: "Brand slug already exists");
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        BrandModel brand = await _dataContext.Brands
            .FirstOrDefaultAsync(p => p.Id == id);
        return View(brand);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(BrandModel brand)
    {
        if (ModelState.IsValid)
        {
            brand.Slug = brand.Name.Replace(" ", "_");
            var slug = await _dataContext.Brands.FirstOrDefaultAsync(c => c.Slug == brand.Slug);
            if (slug != null)
            {
                ModelState.AddModelError(nameof(brand.Slug), "Brand slug already exists");
                return View(brand);
            }

            _dataContext.Update(brand);
            await _dataContext.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        else
        {
            return BadRequest(error: "Brand slug already exists");
        }

        return View(brand);
    }

    public async Task<IActionResult> Delete(int id)
    {
        BrandModel brand = await _dataContext.Brands.FirstOrDefaultAsync(p => p.Id == id);
        _dataContext.Brands.Remove(brand);
        await _dataContext.SaveChangesAsync();
        return RedirectToAction("Index");
    }
}