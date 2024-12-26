using System.Globalization;
using System.Text;
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
        List<CategoryModel> categories = await _dataContext.Categories.OrderByDescending(p => p.Id).ToListAsync();
        
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
            category.Slug = RemoveDiacritics(category.Name).Replace(" ", "-");
            var slug = await _dataContext.Categories.FirstOrDefaultAsync(c => c.Slug == category.Slug);
            if (slug != null)
            {
                ModelState.AddModelError("", "Danh mục đã tồn tại!");
                return View(category);
            }
            
            _dataContext.Add(category);
            await _dataContext.SaveChangesAsync();
            TempData["success"] = "Tạo danh mục thành công!";
            return RedirectToAction("Index");
        }
        return View(category);
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
            category.Slug = RemoveDiacritics(category.Name).Replace(" ", "-");
            var slug = await _dataContext.Categories.FirstOrDefaultAsync(c => c.Slug == category.Slug);
            if (slug != null)
            {
                ModelState.AddModelError("", "Danh mục đã tồn tại");
                return View(category);
            }

            _dataContext.Update(category);
            await _dataContext.SaveChangesAsync();
            TempData["success"] = "Chỉnh sửa danh mục thành công!";
            return RedirectToAction("Index");
        }
        return View(category);
    }

    public async Task<IActionResult> Delete(int id)
    {
        CategoryModel category = await _dataContext.Categories.FirstOrDefaultAsync(p => p.Id == id);
        _dataContext.Categories.Remove(category);
        await _dataContext.SaveChangesAsync();
        TempData["success"] = "Xoá danh mục thành công!";
        return RedirectToAction("Index");
    }
}