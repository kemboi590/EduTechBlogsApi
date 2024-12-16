using EduTechBlogsApi.Models;
using EduTechBlogsApi.Models.Helpers;
using EduTechBlogsApi.Models.ViewModels;
using EduTechBlogsApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;

namespace EduTechBlogsApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly ITokenService _tokenService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AuthenticationController> _logger;

        //ctor
        public AuthenticationController(ITokenService tokenService,
                                     UserManager<ApplicationUser> userManager,
                                     ILogger<AuthenticationController> logger)
        {
            _tokenService = tokenService;
            _userManager = userManager;
            _logger = logger;
        }

        [HttpPost("register-user")]
        public async Task<IActionResult> Register([FromBody]RegisterVM registerVM)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Please, Provide all the required fields!");
            }

            var userExist = await _userManager.FindByEmailAsync(registerVM.Email);
            if(userExist != null)
            {
                return BadRequest($"User {registerVM.Email} already exists!");
            }

            ApplicationUser newUser = new ApplicationUser()
            {
                FirstName = registerVM.FirstName,
                LastName = registerVM.LastName,
                Email = registerVM.Email,
                UserName = registerVM.UserName,
                SecurityStamp = Guid.NewGuid().ToString(),
            };

            var result = await _userManager.CreateAsync(newUser, registerVM.Password);

            if(result.Succeeded)
            {
                switch(registerVM.Role)
                {
                    case UserRoles.Author:
                        await _userManager.AddToRoleAsync(newUser, UserRoles.Author);
                        break;
                    case UserRoles.Reader:
                        await _userManager.AddToRoleAsync(newUser, UserRoles.Reader);
                        break;
                    default:                    
                        break;
                }
                return Ok("User created successfully!");
            }

            //log errors
            foreach (var error in result.Errors)
            {
                _logger.LogError($"Error: {error.Description}"); 
            }
            return BadRequest("User could not be created, try again");
        }

        [HttpPost("login-user")]
        public async Task<IActionResult> Login([FromBody] LoginVM loginVM)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Please provide all required fields.");
            }

            var user = await _userManager.FindByEmailAsync(loginVM.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, loginVM.Password))
            {
                return Unauthorized("Invalid credentials.");
            }

            var tokenValue = await _tokenService.GenerateJWTToken(user);
            return Ok(tokenValue);
        }


        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] TokenRequestVM tokenRequestVM)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Please provide all required fields.");
            }

            var result = await _tokenService.VerifyAndGenerateTokenAsync(tokenRequestVM);
            return Ok(result);
        }

    }
}
