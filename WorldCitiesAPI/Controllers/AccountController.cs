using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Mail;
using WorldCitiesAPI.Data;
using WorldCitiesAPI.Data.Models;

namespace WorldCitiesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly JwtHandler _jwtHandler;

        public AccountController(
            ApplicationDbContext context,
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager,
            JwtHandler jwtHandler)
        {
            _context = context;
            _roleManager = roleManager;
            _userManager = userManager;
            _jwtHandler = jwtHandler;
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginRequest loginRequest)
        {
            var user = await _userManager.FindByNameAsync(loginRequest.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, loginRequest.Password))
                return Unauthorized(new LoginResult()
                {
                    Success = false,
                    Message = "Invalid Email or Password."
                });
            var secToken = await _jwtHandler.GetTokenAsync(user);
            var jwt = new JwtSecurityTokenHandler().WriteToken(secToken);
            return Ok(new LoginResult()
            {
                Success = true,
                Message = "Login successful",
                Token = jwt
            });
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register(LoginRequest loginRequest)
        {
            var user = await _userManager.FindByNameAsync(loginRequest.Email);
            if (user != null)
                return Unauthorized(new LoginResult()
                {
                    Success = false,
                    Message = "Email already in use"
                });

            string role_RegisteredUser = "RegisteredUser";

            if (await _roleManager.FindByNameAsync(role_RegisteredUser) == null)
                await _roleManager.CreateAsync(new IdentityRole(role_RegisteredUser));


            var user_User = new ApplicationUser()
            {
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = loginRequest.Email,
                Email = loginRequest.Email,
            };
            await _userManager.CreateAsync(user_User, loginRequest.Password);
            await _userManager.AddToRoleAsync(user_User, role_RegisteredUser);
            user_User.EmailConfirmed = true;
            user_User.LockoutEnabled = false;
            await _context.SaveChangesAsync();

            var secToken = await _jwtHandler.GetTokenAsync(user_User);
            var jwt = new JwtSecurityTokenHandler().WriteToken(secToken);
            return Ok(new LoginResult()
            {
                Success = true,
                Message = "Login successful",
                Token = jwt
            });
        }

        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword(MailResetRequest email)
        {
            var user = await _userManager.FindByNameAsync(email.Email);
            if (user == null)
                return Unauthorized(new BadRequestResult { });

            var mail = "pomocbezpiecznaaplikacja@outlook.com";
            var pw = "HaslodopomocySilnejaplikacji1";
            //var password = user.PasswordHash;
            string code = await _userManager.GeneratePasswordResetTokenAsync(user);
            //await _userManager.ResetPasswordAsync(user, code, password);
            var client = new SmtpClient("smtp-mail.outlook.com", 587)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(mail, pw)
            };

            await client.SendMailAsync(
                new MailMessage(from: mail,
                                to: email.Email,
                                "Password reset",
                                "Your password reset token: " + code));
            return Ok();
        }

        [HttpPost("NewPassword")]
        public async Task<IActionResult> NewPassword(NewPasswordRequest passwordRequest)
        {
            var user = await _userManager.FindByNameAsync(passwordRequest.Email);
            if (user == null || !await _userManager.VerifyUserTokenAsync(user, "Default", "ResetPassword", passwordRequest.Token))
                return Unauthorized(new LoginResult()
                {
                    Success = false,
                    Message = "Invalid Email or Token."
                });
            await _userManager.ResetPasswordAsync(user, passwordRequest.Token, passwordRequest.Password);
            await _context.SaveChangesAsync();
            var secToken = await _jwtHandler.GetTokenAsync(user);
            var jwt = new JwtSecurityTokenHandler().WriteToken(secToken);
            return Ok(new LoginResult()
            {
                Success = true,
                Message = "Login successful",
                Token = jwt
            });
        }

    }
}