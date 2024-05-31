using Fiorello_Db.Areas.Admin.Helpers;
using Fiorello_Db.Areas.Admin.Helpers.Extentions;
using Fiorello_Db.Areas.Admin.ViewModel.Product;
using Fiorello_Db.Models;
using Fiorello_Db.Services.Interfaces;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.VisualBasic;

namespace Fiorello_Db.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IWebHostEnvironment _env;
        public ProductController(IProductService productService, ICategoryService categoryService, IWebHostEnvironment env)
        {
            _productService = productService;
            _categoryService = categoryService;
            _env = env;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int page = 1)
        {


            var paginationData = await _productService.GetAllPaginationAsync(page);
            var mappedDatas = _productService.GetMappedDatas(paginationData);
            int pageCount = await GetPageCountAsync(4);
            Paginate<ProductVM> model = new(mappedDatas, pageCount, page)
          ;
            ViewBag.currentPage = page;
            return View(model);
        }

        private async Task<int> GetPageCountAsync(int take)
        {
            int count = await _productService.GetCountAsync();
            return (int)Math.Ceiling((decimal)count / take);
        }

        [HttpGet]
        public async Task<IActionResult> Detail(int? id)
        {
            if (id is null) return BadRequest();
            Product product = await _productService.GetByIdAsync((int)id);
            if (product == null) return NotFound();
            List<ProductImageVM> productImages = new();
            foreach (var item in product.ProductImages) {
                productImages.Add(new ProductImageVM
                {
                    Image = item.Name,
                    IsMain = item.IsMain

                });
            }
            ProductDetailVM model = new()
            {
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Category = product.Category.Name,
                Images = productImages
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.categories = await _categoryService.GetAllBySelectAsync();
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductCreateVM request)
        {
            ViewBag.categories = await _categoryService.GetAllBySelectAsync();
            if (!ModelState.IsValid)
            {
                return View();
            }

            foreach (var item in request.Images)
            {
                if (!item.CheckFileSize(500))
                {
                    ModelState.AddModelError("Images", "Image size be must max 500 kb");
                    return View();
                }
                if (!item.CheckFileType("image/"))
                {
                    ModelState.AddModelError("Images", "File type must be only image");
                    return View();
                }
            }

            List<ProductImage> images = new();

            foreach (var item in request.Images)
            {
                string filaName = Guid.NewGuid().ToString() + "-" + item.FileName;
                string path = Path.Combine(_env.WebRootPath, "img", filaName);
                await item.SaveFileToLocalAsync(path);
                images.Add(new ProductImage
                {
                    Name = filaName,
                });
            }
            images.FirstOrDefault().IsMain = true;
            Product product = new()
            {
                Name = request.Name,
                Description = request.Description,
                Price = float.Parse(request.Price.Replace(".", ",")),
                CategoryId = request.CategoryId,
                ProductImages = images
            };

            await _productService.CreateAsync(product);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null) return BadRequest();
            Product product = await _productService.GetByIdAsync((int)id);
            if (product == null) return NotFound();

            foreach (var item in product.ProductImages)
            {
                string path = Path.Combine(_env.WebRootPath, "img", item.Name);
                path.DeleteFileFromLocal();
            }

            await _productService.DeleteAsync(product);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {


            if(id is null) return BadRequest();

            Product product = await _productService.GetByIdAsync((int)id);
            if (product == null) return NotFound();

            ViewBag.categories = await _categoryService.GetAllBySelectAsync();

            ProductEditVM model = new ProductEditVM
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                CategoryId = product.CategoryId,  
                Images = product.ProductImages.Select(m => new ProductImageVM
                {
                    Image = m.Name,
                    IsMain = m.IsMain
                }).ToList()
            };

            return View(model);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Edit(int? id, ProductEditVM editProduct)
        {
            ViewBag.categories = await _categoryService.GetAllBySelectAsync();

            if (id is null) return BadRequest();

            if (!ModelState.IsValid)
            {
                return View(editProduct);
            }

            Product dbProduct = await _productService.GetByIdAsync((int)id);
            if (dbProduct == null) return NotFound();
            foreach (var item in dbProduct.ProductImages)
            {
                string path = Path.Combine(_env.WebRootPath, "img", item.Name);
                path.DeleteFileFromLocal();
            }
            List<ProductImage> images = new();
           
            foreach (var item in editProduct.NewImages)
            {
                string filaName = Guid.NewGuid().ToString() + "-" + item.FileName;
                string path = Path.Combine(_env.WebRootPath, "img", filaName);
                await item.SaveFileToLocalAsync(path);
                images.Add(new ProductImage
                {
                    Name = filaName,
                });
            }
       
            dbProduct.ProductImages = images;
            await _productService.EditAsync(dbProduct, editProduct);

            return RedirectToAction(nameof(Index));

        }

    }
}

