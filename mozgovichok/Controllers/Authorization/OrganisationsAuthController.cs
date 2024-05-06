using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using mozgovichok.Models.Auth;
using mozgovichok.Models.Organisations;
using mozgovichok.Models.Users;
using mozgovichok.Services.Autorization;
using mozgovichok.Services.Organisations;
using mozgovichok.Services.Users;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace mozgovichok.Controllers.Authorization
{
    [ApiController]
    [Route("api/v1/organisations/auth")]
    //[Route("VRapi/v1/organisations/auth")]
    public class OrganisationsAuthController : ControllerBase
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly OrganisationsService _organisationssServices;
        private readonly AuthorizationsService _authorizationsService;
        private List<Organisation> OrganisationsList;

        //private readonly JWTHandler _jwtHandler;

        public OrganisationsAuthController(OrganisationsService organisationsService, AuthorizationsService authorizationsService, 
            IHttpContextAccessor httpContextAccessor)
        {
            _organisationssServices = organisationsService;
            _authorizationsService = authorizationsService;
            _httpContextAccessor = httpContextAccessor;
            OrganisationsList = _organisationssServices.GetAsync().Result;
        }

        [HttpPost]
        [Route("login")]
        public ActionResult Login([FromBody] LoginModel loginModel)
        {

            if (loginModel is null)
            {
                return BadRequest("Invalid client request");
            }
            Organisation? _organisation = OrganisationsList.Find(o => o.Name == loginModel.Email && o.PasswordHash == loginModel.Password);
            if (_organisation != null)
            {
                //var signingCredentials = _jwtHandler.GetSigningCredentials();

                var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("superSecretKey@345555555555555555"));
                var signingCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);
                var tokeOptions = new JwtSecurityToken(
                    issuer: "https://localhost:44304",
                    audience: "https://localhost:44304",
                    claims: new List<Claim>(),
                    //{
                    //    new Claim(ClaimTypes.NameIdentifier, "1122"),
                    //    new Claim(ClaimTypes.Name, "johndoe"),
                    //    new Claim(ClaimTypes.Role, "Admin"),
                    //    new Claim("firstname", "John"),
                    //    new Claim("lastname", "Doe")
                    //},
                    expires: DateTime.Now.AddMinutes(120),
                    signingCredentials: signingCredentials
                );


                var tokenString = new JwtSecurityTokenHandler().WriteToken(tokeOptions);
                return Ok(new AuthenticatedResponse { Token = tokenString });
            }
            return NotFound();
        }

        [HttpGet]
        [Route("logout")]
        public ActionResult Logout()
        {
            return Unauthorized();
        }
    }
}
