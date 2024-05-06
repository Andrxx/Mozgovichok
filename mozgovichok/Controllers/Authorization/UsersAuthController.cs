using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using mozgovichok.Infrastructure;
using mozgovichok.Models.DTO;
using mozgovichok.Models.Organisations;
using mozgovichok.Models.Users;
using mozgovichok.Services.Authorization;
using mozgovichok.Services.Autorization;
using mozgovichok.Services.Organisations;
using mozgovichok.Services.Users;
using Newtonsoft.Json;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace mozgovichok.Controllers.Authorization
{
    [ApiController]
    //[Route("api/v1/users/auth")]
    //[Route("VRapi/v1/users/auth")]
    public class UsersAuthController : ControllerBase
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UsersService _usersServices;
        private readonly OrganisationsService _organisationsService;
        //private readonly AuthorizationsService _authorizationsService;
        private List<User> UsersList;

        //private readonly JWTHandler _jwtHandler;

        public UsersAuthController(UsersService usersServices, OrganisationsService organisationsService, IHttpContextAccessor httpContextAccessor)
        {
            _usersServices = usersServices;
            _organisationsService = organisationsService;
            _httpContextAccessor = httpContextAccessor;
            UsersList = _usersServices.GetAsync().Result;
        }

        [HttpPost]
        [Route("api/v1/users/auth/login")]
        public async Task<IActionResult> Login([FromBody] LoginModel loginModel)
        {

            if (loginModel is null)
            {
                return BadRequest("Invalid client request");
            }
            string hash = HashHandler.HashPasword(loginModel.Password);
            User? _user = UsersList.Find(u => u.Email == loginModel.Email && u.Password == hash);
            if (_user != null)
            {
                //var signingCredentials = _jwtHandler.GetSigningCredentials();


                if (_user.OrganisationId is null) { _user.OrganisationId = ""; }                //добавляем пустую строку при отсутствии ссылок, TODO - инициализация в конструкторе?
                var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("superSecretKey@345555555555555555"));
                var signingCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);
                var tokeOptions = new JwtSecurityToken(
                    issuer: "https://localhost:44304",
                    audience: "https://localhost:44304",
                    claims: new List<Claim>()
                    {
                        new Claim(ClaimsIdentity.DefaultNameClaimType, _user.Email),
                        new Claim(ClaimTypes.Name, _user.Name),
                        new Claim(ClaimsIdentity.DefaultRoleClaimType, _user.Role),
                        new Claim("OrganisationID", _user.OrganisationId),
                        new Claim("UserID", _user.Id)
                    },
                    expires: DateTime.Now.AddDays(30),
                    signingCredentials: signingCredentials
                );
                _user.Password = "";        // удаление пароля в целях безопасности, TODO 
                var tokenString = new JwtSecurityTokenHandler().WriteToken(tokeOptions);
                var organisation = await _organisationsService.GetAsync(_user.OrganisationId);
                var result = new
                {
                    token = tokenString,
                    user = _user,
                    organisation = organisation
                };

                return new JsonResult(result);
            }
            else { return NotFound("User not found"); }
        }


        [HttpPost]
        [Route("VRapi/v1/users/auth/login")]
        public async Task<IActionResult> VRLogin([FromBody] LoginModel loginModel)
        {

            if (loginModel is null)
            {
                return BadRequest("Invalid client request");
            }
            string hash = HashHandler.HashPasword(loginModel.Password);
            User? _user = UsersList.Find(u => u.Email == loginModel.Email && u.Password == hash);
            if (_user != null)
            {
                //var signingCredentials = _jwtHandler.GetSigningCredentials();
                if (_user.OrganisationId is null) { _user.OrganisationId = ""; }                //добавляем пустую строку при отсутствии ссылок, TODO - инициализация в конструкторе?
                var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("superSecretKey@345555555555555555"));
                var signingCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);
                var tokeOptions = new JwtSecurityToken(
                    issuer: "https://localhost:44304",
                    audience: "https://localhost:44304",
                    claims: new List<Claim>()
                    {
                        new Claim(ClaimsIdentity.DefaultNameClaimType, _user.Email),
                        new Claim(ClaimTypes.Name, _user.Name),
                        new Claim(ClaimsIdentity.DefaultRoleClaimType, _user.Role),
                        new Claim("OrganisationID", _user.OrganisationId),
                        new Claim("UserID", _user.Id)
                    },
                    expires: DateTime.Now.AddDays(30),
                    signingCredentials: signingCredentials
                );
                _user.Password = "";        // удаление пароля в целях безопасности, TODO 
                
                var organisation = await _organisationsService.GetAsync(_user.OrganisationId);
                if (organisation is null) { return NotFound("No active organisation"); }
                List<VRSession> VRSessions = new();
                try { 
                    foreach (Session s in organisation?.Sessions)
                    {
                        VRSessions.Add(new VRSession {
                            SessionId = s?.Id,
                            PupilName = s?.ExaminedPupil?.Name,
                            PupilAge = s?.ExaminedPupil?.Age,
                            PupilId = s?.ExaminedPupil?.Id,
                            Exercise = s?.ExaminedPupil?.ActiveCourse?.Exercises?.FirstOrDefault(e => e.isFinished == false)
                        });
                    }
                }
                catch { return BadRequest("Wrong session data"); }
                var tokenString = new JwtSecurityTokenHandler().WriteToken(tokeOptions);
                var result = new
                {
                    token = tokenString,
                    sessions = VRSessions
                };

                return new JsonResult(result);
            }
            else { return NotFound("User not found"); }
        }

        //[HttpGet]
        //[Route("logout")]
        //public ActionResult Logout()
        //{
        //    return Unauthorized();
        //}
    }
}
