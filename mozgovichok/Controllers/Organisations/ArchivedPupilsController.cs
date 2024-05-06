using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using mozgovichok.Models.Courses;
using mozgovichok.Models.DTO;
using mozgovichok.Models.Organisations;
using mozgovichok.Services.Organisations;
using System.Security.Claims;

namespace mozgovichok.Controllers.Organisations
{
    [ApiController]
    [Route("api/v1/archive")]
    [Authorize]
    public class ArchivedPupilsController : ControllerBase
    {
        private readonly ArchivedPupilsService _archivedPupilsService;
        private readonly PupilsService _pupilsService;
        private readonly OrganisationsService _organisationsService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ArchivedPupilsController(ArchivedPupilsService archivedPupilsService, OrganisationsService organisationsService, IHttpContextAccessor httpContextAccessor
            , PupilsService pupilsService)
        {
            _archivedPupilsService = archivedPupilsService;
            _pupilsService = pupilsService;
            _organisationsService = organisationsService;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet]
        [Route("GetPupils")]
        public async Task<IActionResult> GetPupils()
        {
            var _user = _httpContextAccessor.HttpContext?.User;

            if (_user is null) { return NotFound("JWT error"); }

            string? orgId = _user.FindFirstValue("OrganisationID");


            if (!string.IsNullOrWhiteSpace(orgId))
            {
                List<Pupil> pupils = await _archivedPupilsService.GetAsyncForOrg(orgId);
                pupils.Reverse();
                return new JsonResult(pupils);
            }
            else
            {
                if (_user.IsInRole("admin"))
                {
                    List<Pupil> pupils = await _archivedPupilsService.GetAsync();
                    pupils.Reverse();
                    return new JsonResult(pupils);
                }
                else
                {
                    return NotFound("User is not associated with organisation");
                }
            }
        }


        [HttpGet]
        [Route("GetPupil/{id:length(24)}")]
        public async Task<ActionResult<Pupil>> Get(string id)
        {
            var _user = _httpContextAccessor.HttpContext?.User;

            if (_user is null) { return NotFound("JWT error"); }
            if (_user.IsInRole("admin"))
            {
                return new JsonResult(await _archivedPupilsService.GetAsync(id));
            }

            string? orgId = _user.FindFirstValue("OrganisationID");

            if (!string.IsNullOrEmpty(orgId))
            {
                var _organisation = await _organisationsService.GetAsync(orgId);
                if (_organisation is null) return BadRequest("No associated organisation found");
                bool _authorized = _user.FindFirstValue("UserID") == _organisation?.ActiveUsers.Find(s => s == _user.FindFirstValue("UserID"));
                bool _isPupil = id == _organisation.ActivePupils.Find(s => s == id);
                if (_authorized & _isPupil)
                {
                    return new JsonResult(await _archivedPupilsService.GetAsync(id));
                }
                else { return Forbid(); }
            }
            else
            {
                return Forbid();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="archivedPupil"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("AddPupil")]
        public async Task<IActionResult> Post(Pupil archivedPupil)
        {
            var _user = _httpContextAccessor.HttpContext?.User;
            if (_user is null) { return NotFound("JWT error"); }
            string? orgId = archivedPupil.Organisation ??= _user.FindFirstValue("OrganisationID");
            if (orgId == null) { return BadRequest("Pupil or user is not associated with organisation"); }
            var _organisation = await _organisationsService.GetAsync(orgId);
            if (_organisation is null) { return BadRequest("Organisation not found"); }            

            bool _authorized = _user.FindFirstValue("UserID") == _organisation?.ActiveUsers.Find(s => s == _user.FindFirstValue("UserID"));
            if (_authorized || _user.IsInRole("admin"))
            {
                try
                {
                    archivedPupil.Organisation = orgId;
                    archivedPupil.IsActive = false;
                    await _archivedPupilsService.CreateAsync(archivedPupil);
                    _organisation.ArchivedPupils.Add(archivedPupil.Id);
                    await _organisationsService.UpdateAsync(_organisation.Id, _organisation);
                    return CreatedAtAction(nameof(Get), new { id = archivedPupil.Id }, archivedPupil);
                }
                catch (Exception ex)
                {
                    //var mes = ex.Message.;
                    return BadRequest(ex.Message);
                }
            }
            else { return Forbid(); }
        }


        [HttpDelete]
        [Route("DeletePupil/{pupilId:length(24)}")]
        [Authorize]
        public async Task<IActionResult> Delete(string pupilId)
        {
            var _user = _httpContextAccessor.HttpContext?.User;
            if (_user is null) { return NotFound("JWT error"); }

            var _organisation = await _organisationsService.GetAsync(_user.FindFirstValue("OrganisationID"));
            if (_organisation is null) { return BadRequest("Organisation not found"); }
            if (_organisation.ArchivedPupils.FirstOrDefault(s => s == pupilId) is null) { return BadRequest("Pupil is not in organisation archive"); }
            bool _authorized = _user.FindFirstValue("UserID") == _organisation?.ActiveUsers.Find(s => s == _user.FindFirstValue("UserID"));

            if (_user.IsInRole("admin") || _authorized)
            {
                _organisation.ArchivedPupils.Remove(pupilId);
                await _organisationsService.UpdateAsync(_organisation.Id, _organisation);
                await _archivedPupilsService.RemoveAsync(pupilId);
                return NoContent();
            }
            else
            {
                return Unauthorized();
            }
        }
        

        [HttpPost]
        [Route("RemoveToArchive/{pupilId:length(24)}")]
        [Authorize(Roles = "admin, localadmin")]
        public async Task<IActionResult> RemoveToArchive(string pupilId, ArchivationOptions options)
        {
            var _user = _httpContextAccessor.HttpContext?.User;
            if (_user is null) { return NotFound("JWT error"); }
            var _pupil = await _pupilsService.GetAsync(pupilId);
            if (_pupil is null) { return BadRequest("Pupil not found"); }
            var _organisation = await _organisationsService.GetAsync(_pupil?.Organisation);
            if (_organisation is null) { return BadRequest("Organisation not found"); }
            if (_organisation.ActivePupils.FirstOrDefault(s => s == pupilId) is null) { return BadRequest("Pupil is not in organisation"); }
            bool _authorized = _user.FindFirstValue("UserID") == _organisation?.ActiveUsers.Find(s => s == _user.FindFirstValue("UserID"));

            if (_user.IsInRole("admin") || _authorized)
            {
                if (options.DeleteFromSystem)
                {
                    _organisation.ActivePupils.Remove(pupilId);
                    await _pupilsService.RemoveAsync(pupilId);
                    await _organisationsService.UpdateAsync(_organisation.Id, _organisation);
                    return Ok("Deleted");
                }

                if (options.FinalArchivation)
                {
                    _pupil.IsActive = false;
                    _organisation.ActivePupils.Remove(pupilId);
                    await _pupilsService.RemoveAsync(pupilId);
                    await _archivedPupilsService.CreateAsync(_pupil);
                    if (!options.DeleteFromOrg)
                    {
                        _organisation.ArchivedPupils.Add(pupilId);
                    }
                    //return Ok();
                }
                else
                {
                    Pupil pupilClone = _pupil.Clone();
                    pupilClone.Id = null;
                    await _archivedPupilsService.CreateAsync(pupilClone);
                    if (options.DeleteFromOrg)
                    {
                        _organisation.ActivePupils.Remove(pupilId);
                    }
                    else
                    {
                        _organisation.ArchivedPupils.Add(pupilClone.Id);        //присваиваем ид после генерации ИД при сохранении в БД }
                    }
                }
                await _organisationsService.UpdateAsync(_organisation.Id, _organisation);
                return Ok();
            }
            else return Forbid();
        }

        [HttpGet]
        [Authorize]
        [Route("RestorePupil/{pupilId:length(24)}/{orgId:length(24)?}")]
        public async Task<ActionResult<Pupil>> RestorePupil(string pupilId, string? orgId)
        {

            var _user = _httpContextAccessor.HttpContext?.User;
            if (_user is null) { return NotFound("JWT error"); }
            orgId ??= _user.FindFirstValue("OrganisationID");

            var _organisation = await _organisationsService.GetAsync(orgId);
            if (_organisation is null) return BadRequest("No organisation found");

            bool _authorized = _user.FindFirstValue("UserID") == _organisation?.ActiveUsers.Find(s => s == _user.FindFirstValue("UserID"));
            if (_authorized || _user.IsInRole("admin"))
            {
                var restoredPupil = await _archivedPupilsService.GetAsync(pupilId);
                if (restoredPupil is not null)
                {
                    restoredPupil.IsActive = true;
                    _organisation.ActivePupils.Add(restoredPupil.Id);
                    _organisation.ArchivedPupils.Remove(pupilId);
                    await _archivedPupilsService.RemoveAsync(restoredPupil.Id);
                    await _pupilsService.CreateAsync(restoredPupil);
                    await _organisationsService.UpdateAsync(_organisation.Id, _organisation);
                    return restoredPupil;
                }
                else
                {
                    return BadRequest("Wrong pupil ID");
                }
            }
            else { return Forbid(); }
        }
    }
}
