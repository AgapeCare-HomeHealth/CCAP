using CCAP.Data.DTOs.Users;
using CCAP.Data.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CCAP.API.Controllers
{
    [ApiController]
    [Route("api/users")]
    //[Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _userService.GetAllAsync();

            return Ok(users);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetUser(Guid id)
        {
            var user = await _userService.GetByIdAsync(id);

            if (user == null)
                return NotFound();

            return Ok(user);
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(CreateUserRequest request)
        {
            var user = await _userService.CreateAsync(request);

            return CreatedAtAction(
                nameof(GetUser),
                new { id = user.UserId },
                user);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateUser(
            Guid id,
            UpdateUserRequest request)
        {
            await _userService.UpdateAsync(id, request);

            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            await _userService.DeleteAsync(id);

            return NoContent();
        }

        [HttpPatch("{id:guid}/activate")]
        public async Task<IActionResult> ActivateUser(Guid id)
        {
            await _userService.ActivateAsync(id);

            return NoContent();
        }

        [HttpPatch("{id:guid}/deactivate")]
        public async Task<IActionResult> DeactivateUser(Guid id)
        {
            await _userService.DeactivateAsync(id);

            return NoContent();
        }
    }

}
