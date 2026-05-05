using ShopEZ.API.DTOs;
using ShopEZ.API.Exceptions;
using ShopEZ.API.Models;
using ShopEZ.API.Repositories.Interfaces;
using ShopEZ.API.Services.Interfaces;

namespace ShopEZ.API.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;

        public ProductService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<IEnumerable<ProductDTO>> GetAllProductsAsync()
        {
            var products = await _productRepository.GetAllAsync();
            return products.Select(p => MapToDTO(p)).ToList();
        }

        public async Task<ProductDTO?> GetProductByIdAsync(int id)
        {
            if (id <= 0)
                throw new AppException("Product ID must be a positive integer.", 400);

            var product = await _productRepository.GetByIdAsync(id);

            if (product == null)
                return null;

            return MapToDTO(product);
        }

        public async Task<ProductDTO> CreateProductAsync(CreateProductDTO dto)
        {
            if (dto == null)
                throw new AppException("Product data cannot be null.", 400);

            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new AppException("Product name is required.", 400);

            if (dto.Price <= 0)
                throw new AppException("Product price must be greater than zero.", 400);

            if (dto.Stock < 0)
                throw new AppException("Stock cannot be negative.", 400);

            var product = new Product
            {
                Name = dto.Name.Trim(),
                Description = dto.Description != null ? dto.Description.Trim() : string.Empty,
                Price = dto.Price,
                ImageUrl = dto.ImageUrl != null ? dto.ImageUrl.Trim() : string.Empty,
                Stock = dto.Stock
            };

            var created = await _productRepository.CreateAsync(product);
            return MapToDTO(created);
        }

        public async Task<ProductDTO?> UpdateProductAsync(int id, UpdateProductDTO dto)
        {
            if (id <= 0)
                throw new AppException("Product ID must be a positive integer.", 400);

            if (dto == null)
                throw new AppException("Update data cannot be null.", 400);

            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new AppException("Product name is required.", 400);

            if (dto.Price <= 0)
                throw new AppException("Product price must be greater than zero.", 400);

            if (dto.Stock < 0)
                throw new AppException("Stock cannot be negative.", 400);

            var exists = await _productRepository.ExistsAsync(id);
            if (!exists)
                return null;

            var product = new Product
            {
                Name = dto.Name.Trim(),
                Description = dto.Description != null ? dto.Description.Trim() : string.Empty,
                Price = dto.Price,
                ImageUrl = dto.ImageUrl != null ? dto.ImageUrl.Trim() : string.Empty,
                Stock = dto.Stock
            };

            var updated = await _productRepository.UpdateAsync(id, product);

            if (updated == null)
                return null;

            return MapToDTO(updated);
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            if (id <= 0)
                throw new AppException("Product ID must be a positive integer.", 400);

            return await _productRepository.DeleteAsync(id);
        }

        private static ProductDTO MapToDTO(Product p)
        {
            return new ProductDTO
            {
                ProductId = p.ProductId,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                ImageUrl = p.ImageUrl,
                Stock = p.Stock
            };
        }
    }
}
