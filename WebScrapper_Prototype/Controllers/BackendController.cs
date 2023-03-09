using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Text;
using WebScrapper_Prototype.Data;
using WebScrapper_Prototype.Models;
using WebScrapper_Prototype.Services;
using X.PagedList;
using static System.Net.Mime.MediaTypeNames;

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
        /// <summary>
        /// Downloads and Image is URL. Saves Downloaded Image to Database
        /// </summary>
		[HttpPost]
        public async Task<List<string>> DownloadAndSaveImage(int id, string? url)
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
                    }
                }
			}
			catch (NotSupportedException ex)
            {
				Console.WriteLine(ex.Message);
                Console.WriteLine("------------>> " + "!COMPLETED!" + " <<--------------");
            };
            return consoleLogs;
		}
		/// <summary>
		/// View method for Downloading and Saving Images. Gets and passes image URL to correct methods
		/// </summary>
		[HttpGet]
        public async Task<IActionResult> GetImages(int startAutoDownload, int clear)
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
                    productId.Enqueue(product.Id);
                    ImageURL.Enqueue(product.ImageURL);
                }
                while (productId.Count > 0)
                {
					await DownloadAndSaveImage(productId.Dequeue(), ImageURL.Dequeue());                    
				}				
			}
			var images = _context.ProductImages.ToList();
			return View(images);
		}
        /// <summary>
        /// Method called to Delete all images from Database
        /// </summary>
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
        /// <summary>
        /// Gets Image. Method called.
        /// </summary>
		[HttpGet]
		public IActionResult GetImage(int id)
		{
			var imageModel = _context.ProductImages.FirstOrDefault(img => img.ProductId == id);
			if (imageModel != null && imageModel.Content != null)
				return File(imageModel.Content, "image/jpeg");
			else return NotFound();
		}
		[HttpPost]
		public async Task<ActionResult> Gpt3Recipe(Gpt3 ingredients)
		{
			string recipe = string.Empty;
			string prompt = string.Empty;
			prompt = "Please give me a recipe based on the following ingredients:"
				+ " " + ingredients.Ingredient1
				+ " " + ingredients.Ingredient2
				+ " " + ingredients.Ingredient3
				+ " " + ingredients.Ingredient4
				+ " " + ingredients.Ingredient5;
			Console.WriteLine(prompt);
			HttpClient client = new();
			client.DefaultRequestHeaders.Add("Authorization", "Bearer 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");
			var content = new StringContent("{\"model\": \"text-davinci-001\", \"prompt\": \"" + prompt + "\",\"temperature\": 1,\"max_tokens\": 250}",
				Encoding.UTF8, "application/json");

			HttpResponseMessage response = await client.PostAsync("https://api.openai.com/v1/completions", content);

			string? responseString = await response.Content.ReadAsStringAsync();
			Console.WriteLine(responseString);
			if (responseString != null)
			{
				dynamic? responseObj = JsonConvert.DeserializeObject(responseString);
				if (responseObj != null)
				{
					if (responseObj.choices.Count > 0)
					{
						recipe = responseObj.choices[0].text;
						Console.WriteLine(recipe);
						recipe = recipe.Replace("\n\n", "</p><p style='text-align:center; padding:15px 0;'>");
						recipe = recipe.Replace("\n", "<br style='text-align:center; padding:15px 0;'>");
						recipe = "<p style='text-align:center; padding:15px 0;'>" + recipe + "</p>";
						ViewBag.recipe = recipe;
						return RedirectToAction("Gpt3", new RouteValueDictionary(new { text = recipe }));
					}
					else
					{
						ViewBag.recipe = "No recipe found.";
						return RedirectToAction("Gpt3", new RouteValueDictionary(new { recipe = "No recipe found." }));

					}
				}
			}
			return RedirectToAction("Gpt3", new RouteValueDictionary(new { text = recipe }));
		}
		[HttpGet]
		public async Task<ActionResult> Gpt3(string text)
		{
			ViewBag.recipe = text;
			return View();
		}
		/// <summary>
		/// Automates Adding Products from csv file. Calls Services.
		/// </summary>
		[Authorize(Roles = "Administrator, Manager")]
        [HttpGet]
        public IActionResult AutoProductCreateTest()
        {
            AddCSV product = new AddCSV();
            return View(product);
        }
		/// <summary>
		/// Automates Adding Products from csv file. Calls Services.
		/// </summary>
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
        /// <summary>
        /// Method is called to process CSV File
        /// </summary>
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
        /// <summary>
        /// Index View for Backend
        /// </summary>
		[Authorize(Roles = "Administrator, Manager")]
		[HttpPost]
		public IActionResult Index(int id)
		{
			var products = from p in _context.Products
						   select p;
			if (id > 0)
			{
				products = products.Where(a => a.Id == id);
			}
			return View(products);
		}
		/// <summary>
		/// Index View for Backend
		/// </summary>
		[Authorize(Roles = "Administrator, Manager")]
		[HttpGet]
		public IActionResult Index(int startDisplay, int productId, string view, string sortOrder, string findProduct, string currentFilter, string searchString, string dataBatch, int? page, int startImageDownload)
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
			if (!String.IsNullOrEmpty(searchString))
			{
				products = products.Where(s => s.ProductCategory != null && s.ProductCategory.Contains(searchString)
					   || s.ProductName != null && s.ProductName.Contains(searchString));
			}
			switch (findProduct)
			{
				case "All":
					products = products.OrderBy(s => s.ProductCategory);
					break;
				case "laptops":
					products = products.Where(s => s.ProductCategory != null && s.ProductCategory.Contains("Laptops"));
					break;
				case "desktop":
					products = products.Where(s => s.ProductCategory != null && s.ProductCategory.Contains("Pre-Built PC"));
					break;
				case "hardware":
					products = products.Where(s => s.ProductCategory != null && s.ProductCategory.Contains("Hardware"));
					break;
				case "accessories":
					products = products.Where(s => s.ProductCategory != null && s.ProductCategory.Contains("Accessories"));
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
					products = products.Where(s => s.Visible != null && s.Visible.Equals("Hidden"));
					ViewBag.setting = "visible";
					break;
				case "visible":
					products = products.Where(s => s.Visible != null && s.Visible.Equals("Visible"));
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
						products = products.Where(q => q.Visible != null && q.Visible.Equals(view));
						break;
					case "Hidden":
						products = products.Where(q => q.Visible != null && q.Visible.Equals(view));
						break;
					case "All":
						return View(products);
				}
			}
			if (!String.IsNullOrEmpty(dataBatch))
			{
				products = products.Where(s => s.dataBatch != null && s.dataBatch.Equals(dataBatch));
				_context.AttachRange(products);
				_context.RemoveRange(products);
				_context.SaveChanges();
				return RedirectToAction(nameof(Index));
			}
			return View(products);
		}
	}
}

