using System.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using price_comparison.Models;
using price_comparison.Repository;

namespace price_comparison.Areas.Admin.Controllers;

public class ScrapedProduct
{
    public Decimal Price { get; set; }
    public string ProductName { get; set; }
    public string ShopUrl { get; set; }
    public string ShopName { get; set; }
}

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
  
    public async Task<IActionResult> Index(int pg=1)
    {
        List<ProductModel> products = await _dataContext.Products.Include(p=>p.Brand).Include(p => p.Category).Include(p=>p.Prices).OrderByDescending(p => p.Id).ToListAsync();
        
        const int pageSize = 10;
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


    [HttpGet]
    public IActionResult Create()
    {
        ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name");
        ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name");
        return View();
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
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
                ModelState.AddModelError("", "Sản phẩm đã tồn tại");
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
            
            string apiUrl = $"http://127.0.0.1:5000/scrape?product_name={product.Name}";
            var productPrices = new List<ProductPriceModel>();
            
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(apiUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        string jsonResponse = await response.Content.ReadAsStringAsync();
                        var scrapedData =
                            System.Text.Json.JsonSerializer.Deserialize<List<ScrapedProduct>>(jsonResponse);
                        foreach (var item in scrapedData)
                        {
                                productPrices.Add(new ProductPriceModel
                                {
                                    ProductId = product.Id,
                                    Price = item.Price,
                                    ShopName = item.ShopName,
                                    ShopUrl = item.ShopUrl
                                });
                        }
                    }
                    else
                    {
                        TempData["error"] = "Không thể lấy dữ liệu giá từ API!";
                    }
                }
                catch (Exception ex)
                {
                    TempData["error"] = $"Lỗi trong quá trình scraping: {ex.Message}";
                }
            }

            product.Prices = productPrices;

            _dataContext.Add(product);
            await _dataContext.SaveChangesAsync();

            TempData["success"] = "Tạo sản phẩm thành công!";
            return RedirectToAction("Index");
        }

        TempData["error"] = "Đã xảy ra lỗi! Vui lòng kiểm tra lại.";
        return View(product);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        ProductModel product = await _dataContext.Products
            .Include(p => p.Prices)
            .FirstOrDefaultAsync(p => p.Id == id);
        
        ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name", product.CategoryId);
        ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name", product.BrandId);
        return View(product);
    }
    
 
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ProductModel product, string deletedPrices)
    {
        ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name", product.CategoryId);
        ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name", product.BrandId);
        
        var productExists = await _dataContext.Products
            .Include(p => p.Prices) 
            .FirstOrDefaultAsync(p => p.Id == id);

        if (productExists == null)
        {
            TempData["error"] = "Sản phẩm không tồn tại!";
            return RedirectToAction("Index");
        }
        
        if (ModelState.IsValid)
        {
           
            if (product.ImageUpload != null)
            {
                string upLoadPath = Path.Combine(_webHostEnvironment.WebRootPath, "media/products");
                string imageName = Guid.NewGuid().ToString() + "_" + product.ImageUpload.FileName;
                string filePath = Path.Combine(upLoadPath, imageName);
                string oldFilePath = Path.Combine(upLoadPath, productExists.Image);
                try
                {
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("", e.Message);
                }
                FileStream fs = new FileStream(filePath, FileMode.Create);
                await product.ImageUpload.CopyToAsync(fs);
                fs.Close();
                productExists.Image = imageName;
            }

            if (productExists.Prices == null || !productExists.Prices.Any())
            {
                string apiUrl = $"http://127.0.0.1:5000/scrape?product_name={product.Name}";
                var productPrices = new List<ProductPriceModel>();
                using (HttpClient client = new HttpClient())
                {
                    try
                    {
                        HttpResponseMessage response = await client.GetAsync(apiUrl);
                        if (response.IsSuccessStatusCode)
                        {
                            string jsonResponse = await response.Content.ReadAsStringAsync();
                            var scrapedData =
                                System.Text.Json.JsonSerializer.Deserialize<List<ScrapedProduct>>(jsonResponse);
                            
                            foreach (var item in scrapedData)
                            {
                                productPrices.Add(new ProductPriceModel
                                {
                                    ProductId = product.Id,
                                    Price = item.Price,
                                    ShopName = item.ShopName,
                                    ShopUrl = item.ShopUrl
                                });
                            }
                        }
                        else
                        {
                            TempData["error"] = "Không thể lấy dữ liệu giá từ API!";
                        }
                    }
                    catch (Exception ex)
                    {
                        TempData["error"] = $"Lỗi trong quá trình scraping: {ex.Message}";
                    }
                }

                productExists.Prices = productPrices;
            }
            
            var deletedPriceIds = !string.IsNullOrEmpty(deletedPrices)
                ? deletedPrices.Split(',').Select(int.Parse).ToList()
                : new List<int>();
            
            productExists.Prices = productExists.Prices
                .Where(price => !deletedPriceIds.Contains(price.Id))
                .ToList();

            productExists.Name = product.Name;
            productExists.Description = product.Description;
            productExists.CategoryId = product.CategoryId;
            productExists.BrandId = product.BrandId;


            _dataContext.Update(productExists);
            await _dataContext.SaveChangesAsync();

            TempData["success"] = "Chỉnh sửa sản phẩm thành công!";
            return RedirectToAction("Index");
        }

        TempData["error"] = "Đã xảy ra lỗi! Vui lòng kiểm tra lại.";
        return View(product);
    }

    public async Task<IActionResult> Delete(int id)
    {
        ProductModel product = await _dataContext.Products.FindAsync(id);
        if (!string.IsNullOrEmpty(product.Image))
        {
            String upLoadPath = Path.Combine(_webHostEnvironment.WebRootPath, "media/products");
            string oldFilePath = Path.Combine(upLoadPath, product.Image);
            if (System.IO.File.Exists(oldFilePath))
            {
                System.IO.File.Delete(oldFilePath);
            }
        }
        _dataContext.Products.Remove(product);
        await _dataContext.SaveChangesAsync();
        TempData["success"] = "Xoá sản phẩm thành công!";
        return RedirectToAction("Index");
    }
    
    public async Task<IActionResult> Search(string searchString, int pg=1)
    {
        List<ProductModel> products = await _dataContext.Products.Where(p=>p.Name.Contains(searchString) || p.Description.Contains(searchString)).Include(p=>p.Brand).Include(p => p.Category).Include(p=>p.Prices).ToListAsync();
            
        const int pageSize = 10;
        if (pg < 1)
        {
            pg = 1;
        }
        int recsCount = products.Count();
        var pager = new Paginate(recsCount,pg,pageSize);
        int recSkip = (pg - 1) * pageSize;
        var data = products.Skip(recSkip).Take(pager.PageSize).ToList();
        ViewBag.Pager = pager;
        ViewBag.searchString = searchString;
        return View(data);
    }
    
}