using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using price_comparison.Models;
using price_comparison.Repository;

namespace price_comparison.Areas.Admin.Controllers;

[Area("Admin")]
public class ProductController : Controller
{
    private readonly DataContext _dataContext;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public ProductController(DataContext dataContext, IWebHostEnvironment webHostEnvironment)
    {
        _dataContext = dataContext;
        _webHostEnvironment = webHostEnvironment;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _dataContext.Products.OrderByDescending(p => p.Id).Include(p => p.Category)
            .Include(p => p.Brand).ToListAsync());
    }

    [HttpGet]
    public IActionResult Create()
    {
        ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name");
        ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name");
        return View();
    }

    // [HttpPost]
    // [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductModel product)
    {
        ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name", product.CategoryId);
        ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name", product.BrandId);
        if (ModelState.IsValid)
        {
            product.Slug = product.Name.Replace(" ", "-");
            var slug = await _dataContext.Products.FirstOrDefaultAsync(p => p.Slug == product.Slug);
            if (slug != null)
            {
                ModelState.AddModelError("", "Product already exists");
                return View(product);
            }
        
            if (product.ImageUpload != null)
            {
                string upLoadPath = Path.Combine(_webHostEnvironment.WebRootPath, "media/products");
                string imageName = Guid.NewGuid().ToString() + "_" + product.ImageUpload.FileName;
                string filePath = Path.Combine(upLoadPath, imageName);
                FileStream fs = new FileStream(filePath, FileMode.Create);
                await product.ImageUpload.CopyToAsync(fs);
                fs.Close();
                product.Image = imageName;
            }
        
            _dataContext.Add(product);
            await _dataContext.SaveChangesAsync();
            TempData["success"] = "Product created successfully";
            return RedirectToAction("Index");
        }
        else
        {
            TempData["error"] = "Something went wrong!";
        }
       
        return View(product);
    }
}