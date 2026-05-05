using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShopEZ.API.DTOs;
using ShopEZ.API.Exceptions;
using ShopEZ.API.Services.Interfaces;

namespace ShopEZ.API.Controllers
{
    [Route("api/[controller]")]
    [Produces("application/json")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // POST /api/auth/register
        [HttpPost("register")]
        [ProducesResponseType(typeof(AuthResponseDTO), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(409)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Register([FromBody] RegisterDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, errors = ModelState });

            try
            {
                AuthResponseDTO response = await _authService.RegisterAsync(dto);
                return StatusCode(201, new { success = true, data = response });
            }
            catch (AppException ex)
            {
                return StatusCode(ex.StatusCode, new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An unexpected error occurred.", detail = ex.Message });
            }
        }

        // POST /api/auth/login
        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResponseDTO), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Login([FromBody] LoginDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, errors = ModelState });

            try
            {
                AuthResponseDTO response = await _authService.LoginAsync(dto);
                return Ok(new { success = true, data = response });
            }
            catch (AppException ex)
            {
                return StatusCode(ex.StatusCode, new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An unexpected error occurred.", detail = ex.Message });
            }
        }
    }
}
