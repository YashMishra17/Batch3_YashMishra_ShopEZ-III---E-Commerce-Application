using System.ComponentModel.DataAnnotations;

namespace ShopEZ.API.DTOs
{
    public class CreateOrderDTO
    {
        [Required(ErrorMessage = "UserId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "UserId must be a positive integer.")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Cart items are required.")]
        public List<CartItemDTO> CartItems { get; set; } = new List<CartItemDTO>();
    }
}
