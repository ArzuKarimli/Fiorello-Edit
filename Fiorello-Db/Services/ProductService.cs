using Fiorello_Db.Areas.Admin.ViewModel.Product;
using Fiorello_Db.Data;
using Fiorello_Db.Models;
using Fiorello_Db.Services.Interfaces;
using MessagePack;
using Microsoft.EntityFrameworkCore;

namespace Fiorello_Db.Services
{
    public class ProductService : IProductService
    {
        private readonly AppDbContext _context;
        public ProductService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Product>> GetAllAsync()
        {
            return await _context.Products.Include(m => m.Category)
                                          .Include(m => m.ProductImages)
                                          .ToListAsync();
        }

        public async Task<List<Product>> GetAllPaginationAsync(int page, int take = 4)
        {
            return await _context.Products.Include(m => m.Category).Include(m => m.ProductImages).Skip((page - 1) * take).Take(take).ToListAsync();   
        }

        public async Task<List<Product>> GetAllWithImagesAsync()
        {
           return await _context.Products.Include(m => m.ProductImages).ToListAsync();
        }

        public async Task<Product> GetByIdAsync(int id)
        {
            return await _context.Products.Where(m => !m.SoftDeleted).Include(m => m.ProductImages).Include(m=>m.Category).FirstOrDefaultAsync(m => m.Id == id);
                                                     
        }

        public async Task<int> GetCountAsync()
        {
           return await _context.Products.CountAsync();
        }

        public List<ProductVM> GetMappedDatas(List<Product> products)
        {
            return products.Select(m => new ProductVM
            {
                Id = m.Id,
                Name = m.Name,
                Description = m.Description,
                Price = m.Price,
                Image = m.ProductImages.FirstOrDefault(m=>m.IsMain)?.Name,
                Category = m.Category.Name
            })
            .ToList();
        }

        public async Task CreateAsync(Product product)
        {
           await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Product product)
        {
           _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistProductAsync(int id)
        {
            return await _context.Products.AnyAsync(m => m.Id == id);
        }

        public async Task EditAsync(Product product, ProductEditVM editProduct)
        {
            product.Name=editProduct.Name;
            product.Description=editProduct.Description;
            product.Price=editProduct.Price;
            product.Category.Id=editProduct.CategoryId;
          
            await _context.SaveChangesAsync();

        }
    }
}
