using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Http.Headers;
using WebScrapper_Prototype.Data;
using WebScrapper_Prototype.Models;
using WebScrapper_Prototype.Services;
using X.PagedList;
using static WebScrapper_Prototype.Models.ProductImage;

namespace WebScrapper_Prototype.Controllers
{
    public class BackendController : Controller
    {
        private readonly WebScrapper_PrototypeContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public BackendController(WebScrapper_PrototypeContext context, IWebHostEnvironment webHost, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _webHostEnvironment = webHost;
            _httpContextAccessor = httpContextAccessor;
        }

		[HttpPost]
        public async Task DownloadAndSaveImages(int id, string? url)
		{
			List<string> consoleLogs = new List<string>();
			try
			{
				using (var httpClient = new HttpClient())
				{
					consoleLogs.Add("Starting..."); ;
					consoleLogs.Add("Download and Save Image Task has Started!");
					consoleLogs.Add("---------------->> " + url);
					Console.WriteLine();
					var response = await httpClient.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
						consoleLogs.Add("---------------->> " + response);
                        consoleLogs.Add("Fetching Image Data...");
                        var imageData = await response.Content.ReadAsByteArrayAsync();
						consoleLogs.Add($"{imageData}" + " <<--------------");
                        var fileName = Path.GetFileName(url);
						consoleLogs.Add($"{fileName}" + " <<--------------");
                        var fileType = Path.GetExtension(url);
						consoleLogs.Add($"{fileType}" + " <<--------------");
                        consoleLogs.Add("Creating ProductImage...");
                        var Image = new ProductImage
                        {
                            ProductId = id,
                            FileName = fileName,
                            FileType = fileType,
                            Content = imageData
                        };
                        consoleLogs.Add("Updating Database...");
                        _context.ProductImages.Add(Image);
                        await _context.SaveChangesAsync();
                        GetImage(id);
                        consoleLogs.Add("DONE!");
                        Console.WriteLine("------------------------------------>>");
                    }
                    else
                    {
                        Console.WriteLine("FINISHED");                           
                    }
					foreach (var item in consoleLogs)
                    {
                        Console.WriteLine(item);
                        ViewBag.ConsoleLog = item;
                    }
                }
			}
			catch (NotSupportedException ex)
            {
                Console.WriteLine("------------>> " + "!COMPLETED!" + " <<--------------");
            };
		}
		[HttpGet]
        public async Task<IActionResult> GetImages(int startAutoDownload, int id, int clear)
        {
			if (clear > 0)
            {
                await ClearImages();
            }
            if (startAutoDownload > 0)
            {
                Queue<int> productId = new Queue<int>();
                Queue<string?> ImageURL = new Queue<string?>();

                foreach (var product in _context.Products.ToList())
                {
                    productId.Enqueue(product.ID);
                    ImageURL.Enqueue(product.ImageURL);
                }
                while (productId.Count > 0)
                {
					await DownloadAndSaveImages(productId.Dequeue(), ImageURL.Dequeue());                    
				}				
			}
			var images = _context.ProductImages.ToList();
			return View(images);
		}
		[HttpPost]
		public async Task ClearImages()
		{
			var imageModel = _context.ProductImages;
            foreach(var image in imageModel)
            {
                _context.ProductImages.Remove(image);                
            }
			await _context.SaveChangesAsync();
		}
		[HttpGet]
		public IActionResult GetImage(int id)
		{
			var imageModel = _context.ProductImages.FirstOrDefault(img => img.ProductId == id);
			if (imageModel != null)
			{
				return File(imageModel.Content, "image/jpeg");
			}
			else
			{
				return NotFound();
			}
		}
		[Authorize(Roles = "Administrator, Manager")]
        [HttpGet]
        public IActionResult AutoProductCreateTest()
        {
            AddCSV product = new AddCSV();
            return View(product);
        }
        [Authorize(Roles = "Administrator, Manager")]
        [HttpPost]
        public ActionResult AutoProductCreateTest(AddCSV addCSV)
        {
            var _productService = new ProductService();
            string uniqueCSVFileName = ProcessUploadedCSVFile(addCSV);
            var location = "wwwroot\\Uploads\\" + uniqueCSVFileName;
            var rowData = _productService.ReadCSVFile(location);
            foreach (Product item in rowData)
            {
                Product product = new Product();
                product.ProductName = item.ProductName;
                product.ProductStock = item.ProductStock;
                product.ProductDescription = item.ProductDescription;
                product.ProductStatus = "New";
                product.ProductCategory = item.ProductCategory;
                product.ProductBasePrice = item.ProductBasePrice;
                product.ProductSalePrice = item.ProductSalePrice;
                product.ImageURL = item.ImageURL;
                product.VendorSite = item.VendorSite;
                product.VendorProductURL = item.VendorProductURL;
                product.Visible = item.Visible;
                product.dataBatch = item.dataBatch;
                _context.Attach(product);
                _context.Entry(product).State = EntityState.Added;
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(Index));
        }
        [Authorize(Roles = "Administrator, Manager")]
        private string ProcessUploadedCSVFile(AddCSV model)
        {
            string uniqueFileName = "ERROR";
            string path = Path.Combine(_webHostEnvironment.WebRootPath, "Uploads");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            if (model.CSVFile != null)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Uploads");
                uniqueFileName = Guid.NewGuid().ToString() + "_" + model.CSVFile.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    model.CSVFile.CopyTo(fileStream);
                }
            }
            return uniqueFileName;
        }
		[Authorize(Roles = "Administrator, Manager")]
		[HttpPost]
		public IActionResult Index(int id)
		{
			var products = from p in _context.Products
						   select p;
			if (id > 0)
			{
				products = products.Where(a => a.ID == id);
			}
			return View(products);
		}
		[Authorize(Roles = "Administrator, Manager")]
		[HttpGet]
		public async Task<IActionResult> Index (int startDisplay, int productId, string view, string sortOrder, string findProduct, string currentFilter, string searchString, string dataBatch, int? page, int startImageDownload)
        {
            ViewBag.setting = view;
            var products = from p in _context.Products
                           select p;
            ViewBag.CurrentSort = sortOrder;
            ViewBag.ProductSortParm = String.IsNullOrEmpty(sortOrder) ? "all" : "";
            ViewBag.HiddenParm = sortOrder == "hidden" ? "hidden" : "hidden";
            ViewBag.VisibleParm = sortOrder == "visible" ? "visible" : "visible";
            ViewBag.SavingParm = sortOrder == "saving" ? "saving" : "saving";
            ViewBag.PriceDescParm = sortOrder == "price_desc" ? "price_desc" : "price_desc";
            ViewBag.AlphabetParm = sortOrder == "alphabet" ? "alphabet" : "alphabet";

            ViewBag.CurrentProduct = findProduct;
            ViewBag.ProductCatParm = String.IsNullOrEmpty(findProduct) ? "all" : "";
            ViewBag.LaptopParm = findProduct == "laptops" ? "laptops" : "laptops";
            ViewBag.DesktopParm = findProduct == "desktop" ? "desktop" : "desktop";
            ViewBag.HardwareParm = findProduct == "hardware" ? "hardware" : "hardware";
            ViewBag.AccessoriesParm = findProduct == "accessories" ? "accessories" : "accessories";
            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;

            }
            if (!String.IsNullOrEmpty(searchString))
            {
                products = products.Where(s => s.ProductCategory.Contains(searchString)
                       || s.ProductName.Contains(searchString));
            }
            switch (findProduct)
            {
                case "All":
                    products = products.OrderBy(s => s.ProductCategory);
                    break;
                case "laptops":
                    products = products.Where(s => s.ProductCategory.Contains("Laptops"));
                    break;
                case "desktop":
                    products = products.Where(s => s.ProductCategory.Contains("Pre-Built PC"));
                    break;
                case "hardware":
                    products = products.Where(s => s.ProductCategory.Contains("Hardware"));
                    break;
                case "accessories":
                    products = products.Where(s => s.ProductCategory.Contains("Accessories"));
                    break;
                default:
                    break;
            }
            switch (sortOrder)
            {
                case "All":
                    products = products.OrderBy(s => s.ProductSalePrice);
                    break;
                case "hidden":
                    products = products.Where(s => s.Visible.Equals("Hidden"));
                    ViewBag.setting = "visible";
                    break;
                case "visible":
                    products = products.Where(s => s.Visible.Equals("Visible"));
                    ViewBag.setting = "visible";
                    break;
                case "saving":
                    products = products.Where(s => s.ProductSalePrice < s.ProductBasePrice / 2);
                    break;
                case "price_desc":
                    products = products.OrderByDescending(s => s.ProductSalePrice);
                    break;
                case "alphabet":
                    products = products.OrderBy(s => s.ProductName);
                    break;
                default:
                    break;
            }
            if (!String.IsNullOrEmpty(view))
            {
                switch (view)
                {
                    case "Visible":
                        products = products.Where(q => q.Visible.Equals(view));
                        break;
                    case "Hidden":
                        products = products.Where(q => q.Visible.Equals(view));
                        break;
                    case "All":
                        return View(products);
                }
            }
            if (!String.IsNullOrEmpty(dataBatch))
            {
                products = products.Where(s => s.dataBatch.Equals(dataBatch));
                _context.AttachRange(products);
                _context.RemoveRange(products);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(products);
        }
        [Authorize(Roles = "Administrator, Manager")]
		[HttpGet]
        public IActionResult Create()
        {
            Product product = new Product();
            return View(product);
        }
		[Authorize(Roles = "Administrator, Manager")]
		[HttpPost]
        public ActionResult Create(Product product)
        {
            string uniqueFileName = ProcessUploadedImageFile(product);
            product.ImageURL = uniqueFileName;
            _context.Attach(product);
            _context.Entry(product).State = EntityState.Added;
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Products == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(m => m.ID == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Products == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product)
        {
            if (id != product.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                    ViewBag.status = "Successfully Updated";
                }
                catch (System.Data.Entity.Infrastructure.DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Products == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(m => m.ID == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }
		private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.ID == id);
        }
        private string ProcessUploadedImageFile(Product model)
        {
            string uniqueFileName = "ERROR";
            string path = Path.Combine(_webHostEnvironment.WebRootPath, "Uploads");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            if (model.ProductPic != null)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Uploads");
                uniqueFileName = Guid.NewGuid().ToString() + "_" + model.ProductPic.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    model.ProductPic.CopyTo(fileStream);
                }
            }
            return uniqueFileName;
        }
    }
}

