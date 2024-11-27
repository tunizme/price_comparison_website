using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using price_comparison.Models;
using price_comparison.Repository;

namespace price_comparison.Areas.Admin.Controllers;

public class ScrapedProduct
{
    public string Price { get; set; }
    public string ProductName { get; set; }
    public string ShopUrl { get; set; }
    public string ShopeName { get; set; }
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

            // Scraping dữ liệu giá từ API
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
                            System.Console.WriteLine(item.Price);
                            System.Console.WriteLine(item.ShopeName);
                            System.Console.WriteLine(item.ShopUrl);
                            string rawPrice = item.Price.ToString()
                                .Replace("&nbsp", "")
                                .Replace("₫", "")
                                .Replace(".", "")
                                .Trim();

                            System.Console.WriteLine(rawPrice);
                            if (decimal.TryParse(rawPrice, out decimal priceValue))
                            {
                                System.Console.WriteLine("Sucess----------------------");
                                productPrices.Add(new ProductPriceModel
                                {
                                    ProductId = product.Id, // Gắn với sản phẩm vừa lưu
                                    Price = priceValue,
                                    ShopName = item.ShopeName,
                                    ShopUrl = item.ShopUrl
                                });
                            }
                        }
                    }
                    else
                    {
                        System.Console.WriteLine("Bad request");
                        TempData["error"] = "Không thể lấy dữ liệu giá từ API!";
                    }
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine("sdfjvsdjgsevhj");
                    TempData["error"] = $"Lỗi trong quá trình scraping: {ex.Message}";
                }
            }

            // Gắn danh sách giá vào sản phẩm
            product.Prices = productPrices;
            System.Console.WriteLine(productPrices);
            // Lưu sản phẩm và danh sách giá cùng lúc
            _dataContext.Add(product);
            await _dataContext.SaveChangesAsync();

            TempData["success"] = "Tạo sản phẩm và cập nhật giá thành công!";
            return RedirectToAction("Index");
        }

        TempData["error"] = "Đã xảy ra lỗi! Vui lòng kiểm tra lại.";
        return View(product);
    }
    public async Task<IActionResult> Edit(int? id)
    {
        ProductModel product = await _dataContext.Products.FirstOrDefaultAsync(p => p.Id == id);
        ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name", product.CategoryId);
        ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name", product.BrandId);
        
        return View(product);
        
    }
   
}