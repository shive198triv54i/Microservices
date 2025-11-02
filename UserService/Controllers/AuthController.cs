using Microsoft.AspNetCore.Mvc;
using UserService.Models;
using UserService.Repositories;
using UserService.Services;
using UserService.Messaging;
using BCrypt.Net;

namespace UserService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserRepository _repo;
        private readonly JwtService _jwt;
        private readonly RabbitMqPublisher _publisher;

        public AuthController(UserRepository repo, JwtService jwt, RabbitMqPublisher publisher)
        {
            _repo = repo;
            _jwt = jwt;
            _publisher = publisher;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(User request)
        {
            request.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.PasswordHash);
            await _repo.Add(request);

            _publisher.PublishUserRegistered($"New user: {request.Email}");
            return Ok("Registered");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(User request)
        {
            var user = await _repo.GetByEmail(request.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.PasswordHash, user.PasswordHash))
                return Unauthorized("Invalid credentials");

            var token = _jwt.GenerateToken(user);
            return Ok(new { Token = token });
        }
    }
}
