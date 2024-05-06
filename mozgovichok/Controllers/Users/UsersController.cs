using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using mozgovichok.Models.Users;
using mozgovichok.Services.Users;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using mozgovichok.Services.Organisations;
using mozgovichok.Models.Organisations;
using mozgovichok.Infrastructure;
using MongoDB.Bson;
using mozgovichok.Models.DTO;

namespace mozgovichok.Controllers.Users
{
    [ApiController]
    [Route("api/v1/users")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly UsersService _usersService;
        private readonly OrganisationsService _organisationsService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UsersController(UsersService usersServices, IHttpContextAccessor httpContextAccessor, OrganisationsService organisationsService)
        {
            _usersService = usersServices;
            _httpContextAccessor = httpContextAccessor;
            _organisationsService = organisationsService;
        }

        [HttpGet]
        [Authorize(Roles = "admin")]
        [Route("GetUsers")]
        public async Task<List<User>> Get() =>
           await _usersService.GetAsync();

        [HttpGet]
        [Route("GetUser/{id:length(24)}")]
        public async Task<ActionResult<User>> Get(string id)
        {
            var _user = _httpContextAccessor.HttpContext?.User;

            if (_user is null) { return NotFound("JWT error"); }
            var _admin = _user.IsInRole("admin");

            var user = await _usersService.GetAsync(id);

            if (user is null)
            {
                if (_admin)
                {
                    return NotFound();
                }
                else
                {
                    return Forbid();
                }
            }

            bool _authorised = (_user.FindFirstValue("UserID") == user.Id);
            if (_admin || _authorised) 
            {
                return user;
            }
            else
            {
                return Forbid();
            }
        }

        [HttpPut]
        [Route("Edit/{id:length(24)}")]
        public async Task<IActionResult> Update(string id, User updatedUser)
        {
            var _user = _httpContextAccessor.HttpContext?.User;

            if (_user is null) { return NotFound("JWT error"); }
            var _admin = _user.IsInRole("admin");

            var user = await _usersService.GetAsync(id);

            if (user is null)
            {
                if (_admin)
                {
                    return NotFound();
                }
                else
                {
                    return Forbid();
                }
            }

            bool _authorised = (_user.FindFirstValue("UserID") == user?.Id);
            if (_admin || _authorised)
            {
                updatedUser.Id = user.Id;
                updatedUser.Password = user.Password;
                updatedUser.Role = user.Role;
                await _usersService.UpdateAsync(id, updatedUser);
                updatedUser.Password = "";
                return new JsonResult(updatedUser);
            }
            else
            {
                return Forbid();
            }
        }

        [HttpDelete]
        [Route("Delete/{id:length(24)}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete(string id)
        {
            var admin = await _usersService.GetAsync(id);

            if (admin is null)
            {
                return NotFound();
            }

            await _usersService.RemoveAsync(id);

            return NoContent();
        }

        [HttpPost]
        [Route("sign-up")]
        [AllowAnonymous]
        public async Task<IActionResult> Post(SignupModel signupModel)
        {
            if (signupModel == null) { return BadRequest(); }
            if (_usersService.FindMailInBase(signupModel.Email).Result) { return BadRequest("User already registered"); }
            User newUser = new()
            {
                Email = signupModel.Email,
                Name = signupModel.Name,
                SurName = signupModel.SurName,
                Phone = signupModel.Phone,
                Goal = signupModel.Goal,
                Password = HashHandler.HashPasword(signupModel.Password),
                Id = ObjectId.GenerateNewId().ToString()
            };
            Organisation? _organisation = new Organisation();

            //при получении ид организации создаем и сохраняем юзерa и добавляем его ид в организацию
            if (signupModel.OrganisationId is not null) 
            {
                _organisation = await _organisationsService.GetAsync(signupModel.OrganisationId);
                if (_organisation is null) { return BadRequest("Wrong organisation ID"); }
                newUser.OrganisationId = signupModel.OrganisationId;
                if (signupModel.Role != "admin") {newUser.Role = signupModel.Role ?? "user";}
                else { newUser.Role = "user";}
                await _usersService.CreateAsync(newUser);
                _organisation.ActiveUsers.Add(newUser.Id);
                await _organisationsService.UpdateAsync(_organisation.Id, _organisation);
            }
            else
            {
                if(string.IsNullOrEmpty(signupModel.Inn))
                {
                    _organisation.Name = signupModel.Name;
                    _organisation.IsActive = true;
                    _organisation.City = signupModel.City;
                    _organisation.Sessions = new();
                    _organisation.ActivePupils = new();
                    _organisation.ActiveUsers.Add(newUser.Id);
                    _organisation.Payments = new();
                    _organisation.Balance = new();
                    await _organisationsService.CreateAsync(_organisation);

                    newUser.OrganisationId = _organisation.Id;
                    newUser.Role = "localadmin";
                    await _usersService.CreateAsync(newUser);
                }
                else
                {
                    _organisation.Name = signupModel.OrganizationName;
                    _organisation.IsActive = true;
                    _organisation.City = signupModel.City;
                    _organisation.Sessions = new();
                    _organisation.ActivePupils = new();
                    _organisation.ActiveUsers.Add(newUser.Id);
                    _organisation.Payments = new();
                    _organisation.Balance = new();
                    _organisation.Inn = signupModel.Inn;
                    _organisation.Requisites = signupModel.Requisites;
                    await _organisationsService.CreateAsync(_organisation);

                    newUser.OrganisationId = _organisation.Id;
                    newUser.Role = "localadmin";
                    await _usersService.CreateAsync(newUser);
                }
            }
            
            //добавляем пустую строку при отсутствии ссылок, TODO - инициализация в конструкторе?
            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("superSecretKey@345555555555555555"));
            var signingCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);
            var tokeOptions = new JwtSecurityToken(
                issuer: "https://localhost:44304",
                audience: "https://localhost:44304",
                claims: new List<Claim>()
                {
                        new Claim(ClaimsIdentity.DefaultNameClaimType, newUser.Email),
                        new Claim(ClaimTypes.Name, newUser.Name),
                        new Claim(ClaimsIdentity.DefaultRoleClaimType, newUser.Role),
                        new Claim("OrganisationID", newUser.OrganisationId),
                        new Claim("UserID", newUser.Id)
                },
                expires: DateTime.Now.AddDays(30),
                signingCredentials: signingCredentials
            );
            newUser.Password = "";        // удаление пароля в целях безопасности, TODO 
            var tokenString = new JwtSecurityTokenHandler().WriteToken(tokeOptions);
            //return Ok(new AuthenticatedResponse { Token = tokenString });
            var result = new
            {
                token = tokenString,
                user = newUser,
                organisation = _organisation
            };

            return new JsonResult(result);
        }
    }
}
