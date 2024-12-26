using System.Globalization;
using System.Text;
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

    private string RemoveDiacritics(string text)
    {
        string normalizedString = text.Normalize(NormalizationForm.FormD);
        StringBuilder sb = new StringBuilder();

        foreach (char c in normalizedString)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            {
                sb.Append(c);
            }
        }

        return sb.ToString().Normalize(NormalizationForm.FormC);
    }
    
    public async Task<IActionResult> Index(int pg = 1)
    {
        List<BrandModel> brands = await _dataContext.Brands.OrderByDescending(p => p.Id).ToListAsync();

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
            brand.Slug = RemoveDiacritics(brand.Name).Replace(" ", "-");
            var slug = await _dataContext.Brands.FirstOrDefaultAsync(c => c.Slug == brand.Slug);
            if (slug != null)
            {
                ModelState.AddModelError("", "Thương hiệu đã tồn tại!");
                return View(brand);
            }
            _dataContext.Add(brand);
            await _dataContext.SaveChangesAsync();
            TempData["success"] = "Tạo thương hiệu thành công!";
            return RedirectToAction("Index");
        }
        return View(brand);
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
            brand.Slug = RemoveDiacritics(brand.Name).Replace(" ", "-");
            var slug = await _dataContext.Brands.FirstOrDefaultAsync(c => c.Slug == brand.Slug);
            if (slug != null)
            {
                ModelState.AddModelError(nameof(brand.Slug), "Thương hiệu đã tồn tại!");
                return View(brand);
            }
            
            TempData["success"] = "Chỉnh sửa thương hiệu thành công!";
            _dataContext.Update(brand);
            await _dataContext.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        return View(brand);
    }

    public async Task<IActionResult> Delete(int id)
    {
        BrandModel brand = await _dataContext.Brands.FirstOrDefaultAsync(p => p.Id == id);
        _dataContext.Brands.Remove(brand);
        await _dataContext.SaveChangesAsync();
        TempData["success"] = "Xoá thương hiệu thành công!";
        return RedirectToAction("Index");
    }
}