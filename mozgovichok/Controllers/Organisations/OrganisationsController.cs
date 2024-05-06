using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using mozgovichok.Infrastructure;
using mozgovichok.Models.Courses;
using mozgovichok.Models.Organisations;
using mozgovichok.Models.Users;
using mozgovichok.Services.Organisations;
using System.Security.Claims;
using System.Security.Cryptography;

namespace mozgovichok.Controllers.Organisations
{
    [ApiController]
    [Route("api/v1/org/")]
    public class OrganisationsController : ControllerBase
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly OrganisationsService _organisationsService;
        private readonly PupilsService _pupilsService;

        public OrganisationsController(OrganisationsService organisationService, PupilsService pupilsService, IHttpContextAccessor httpContextAccessor)
        {
            _organisationsService = organisationService;
            _pupilsService = pupilsService;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet]
        [Route("GetOrganisations")]
        [Authorize(Roles = "admin")]
        public async Task<List<Organisation>> Get() =>
            await _organisationsService.GetAsync();

        [HttpGet]
        [Route("GetOrganisation/{id:length(24)?}")]
        [Authorize]
        public async Task<ActionResult<Organisation>> Get(string? id)
        {
            var _user = _httpContextAccessor.HttpContext?.User;
            
            if(_user is null) { return NotFound("JWT error"); }
            var _admin = _user.IsInRole("admin");
            string? _id = id ?? _user.FindFirstValue("OrganisationID");
            if(string.IsNullOrEmpty(_id)) { return NotFound("Wrong orgId or user is not associated with organisation"); }
            var organisation = await _organisationsService.GetAsync(_id);

            if (_admin)
            {
                if (organisation is null)
                {
                    return NotFound();
                }
                return organisation;
            }

            if (_user.FindFirstValue("UserID") != organisation?.ActiveUsers.Find(s => s == _user.FindFirstValue("UserID")))
            {
                return BadRequest("User is not added to organisation");
            }

            if (organisation is null)
            {
                return NotFound();
            }

            return organisation;
        }

        [HttpPost]
        [Route("Add")]
        [Authorize(Roles = "admin, localadmin")]
        public async Task<IActionResult> Post(Organisation newOrganisation)
        {
            await _organisationsService.CreateAsync(newOrganisation);

            return CreatedAtAction(nameof(Get), new { id = newOrganisation.Id }, newOrganisation);
        }


        [HttpPut]
        [Route("Edit/{id:length(24)}")]
        [Authorize(Roles = "admin, localadmin")]
        public async Task<IActionResult> Update(string id, Organisation updatedOrganisation)
        {
            var _user = _httpContextAccessor.HttpContext?.User;

            if (_user is null) { return NotFound("JWT error"); }
            bool _admin = _user.IsInRole("admin");

            var _organisation = await _organisationsService.GetAsync(id);

            if (_organisation is null)
            {
                if (_admin)
                {
                    return NotFound();
                }
                else {
                    return Forbid();
                }
            }

            bool _localadmin = _user.FindFirstValue("UserID") == _organisation?.ActiveUsers.Find(s => s == _user.FindFirstValue("UserID"));
            if (_localadmin || _admin)
            {
                updatedOrganisation.Id = _organisation.Id;

                await _organisationsService.UpdateAsync(id, updatedOrganisation);
                return Ok();
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
            var admin = await _organisationsService.GetAsync(id);

            if (admin is null)
            {
                return NotFound();
            }

            await _organisationsService.RemoveAsync(id);

            return NoContent();
        }

        //методы добавления тарифа к организации, использовать тарифы из списка БД

        [HttpGet]
        [Route("GetTariff/{orgId:length(24)}")]
        [Authorize(Roles = "admin, localadmin")]
        public async Task<IActionResult> GetTariff(string orgId)
        {
            var _user = _httpContextAccessor.HttpContext?.User;

            if (_user is null) { return NotFound("JWT error"); }
            bool _admin = _user.IsInRole("admin");

            var _organisation = await _organisationsService.GetAsync(orgId);

            if (_organisation is null)
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

            bool _localadmin = _user.FindFirstValue("UserID") == _organisation?.ActiveUsers.Find(s => s == _user.FindFirstValue("UserID"));
            if (_localadmin || _admin)
            {
                var _tariff = _organisation.Tariff;
                if (_tariff is null) { return new JsonResult("[]"); }
                return new JsonResult(_tariff);
            }
            else
            {
                return Forbid();
            }

        }

        [HttpPut]
        [Route("AddTariff/{orgId:length(24)}")]
        [Authorize(Roles = "admin, localadmin")]
        public async Task<IActionResult> Post([FromRoute] string orgId, Tariff newTariff)
        {
            if (newTariff is null)
            {
                return BadRequest("Wrong Tariff");
            }
            var _user = _httpContextAccessor.HttpContext?.User;

            if (_user is null) { return NotFound("JWT error"); }

            bool _admin = _user.IsInRole("admin");

            var _organisation = await _organisationsService.GetAsync(orgId);
            if (_organisation is null)
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

            bool _localadmin = _user.FindFirstValue("UserID") == _organisation?.ActiveUsers.Find(s => s == _user.FindFirstValue("UserID"));
            if (_localadmin || _admin)
            {               
                newTariff.Id = ObjectId.GenerateNewId().ToString();
                _organisation.Tariff = newTariff;
                if (_organisation.Balance != null)
                {
                    Balance balance = BalanceHandler.Count(_organisation?.Payments?.Last(), newTariff, _organisation.Balance);
                    _organisation.Balance = balance;
                }

                await _organisationsService.UpdateAsync(_organisation.Id, _organisation);

                return Ok();
            }
            else
            {
                return Forbid();
            }
        }

        [HttpPut]
        [Route("ChangeTariff/{orgId:length(24)}")]
        [Authorize(Roles = "admin, localadmin")]
        public async Task<IActionResult> ChangeTariff([FromRoute] string orgId, Tariff newTariff)
        {
            var _user = _httpContextAccessor.HttpContext?.User;
            if (_user is null) { return NotFound("JWT error"); }

            if (newTariff is null)
            {
                return BadRequest("Wrong Tariff");
            }
            
            bool _admin = _user.IsInRole("admin");

            var _organisation = await _organisationsService.GetAsync(orgId);

            if (_organisation is null)
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

            bool _localadmin = _user.FindFirstValue("UserID") == _organisation?.ActiveUsers.Find(s => s == _user.FindFirstValue("UserID"));
            if (_localadmin || _admin)
            {
                newTariff.Id = ObjectId.GenerateNewId().ToString();
                _organisation.Tariff = newTariff;
                if (_organisation.Balance != null)
                {
                    Balance balance = BalanceHandler.Count(_organisation?.Payments?.Last(), newTariff, _organisation.Balance);
                    _organisation.Balance = balance;
                }

                await _organisationsService.UpdateAsync(_organisation.Id, _organisation);

                return Ok();
            }
            else
            {
                return Forbid();
            }
        }

        [HttpPut]
        [Route("RemoveTariff/{orgId:length(24)}")]
        [Authorize(Roles = "admin, localadmin")]
        public async Task<IActionResult> RemoveTariff([FromRoute] string orgId)
        {
            var _user = _httpContextAccessor.HttpContext?.User;
            if (_user is null) { return NotFound("JWT error"); }

            bool _admin = _user.IsInRole("admin");

            var _organisation = await _organisationsService.GetAsync(orgId);

            if (_organisation is null)
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

            bool _localadmin = _user.FindFirstValue("UserID") == _organisation?.ActiveUsers.Find(s => s == _user.FindFirstValue("UserID"));
            if (_localadmin || _admin)
            {
                
                _organisation.Tariff = null;

                await _organisationsService.UpdateAsync(_organisation.Id, _organisation);

                return Ok();
            }
            else
            {
                return Forbid();
            }
        }
    }
}
