using ShopEZ.API.DTOs;

namespace ShopEZ.API.Services.Interfaces
{
    public interface IOrderService
    {
        Task<IEnumerable<OrderDTO>> GetAllOrdersAsync();
        Task<OrderDTO?> GetOrderByIdAsync(int id);
        Task<OrderDTO> CreateOrderAsync(CreateOrderDTO dto);
    }
}
