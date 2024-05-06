using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using mozgovichok.Models.Courses;
using mozgovichok.Models.DTO;
using mozgovichok.Models.Organisations;
using mozgovichok.Services.Courses;
using mozgovichok.Services.Organisations;
using System.Security.Claims;

namespace mozgovichok.Controllers.Organisations
{
    [ApiController]
    [Route("api/v1/pupils")]
    [Authorize]
    //[EnableCors("AllowAll")]
    public class PupilsController : ControllerBase
    {
        private readonly PupilsService _pupilsService;
        private readonly OrganisationsService _organisationsService;
        private readonly CoursesService _coursesService;
        //private readonly ArchivedPupilsService _archivedPupilsService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PupilsController(PupilsService pupilsServices, OrganisationsService organisationsService, IHttpContextAccessor httpContextAccessor, CoursesService coursesService)
        {
            _pupilsService = pupilsServices;
            _organisationsService = organisationsService;
            _coursesService = coursesService;
            _httpContextAccessor = httpContextAccessor;
            //_archivedPupilsService = archivedPupilsService;
        }

        [HttpGet]
        [Route("GetPupils")]
        //[EnableCors("AllowAll")]
        public async Task<IActionResult> GetPupils()
        {


            var _user = _httpContextAccessor.HttpContext?.User;

            if (_user is null) { return NotFound("JWT error"); }
            
            string? orgId = _user.FindFirstValue("OrganisationID");


            if (!string.IsNullOrWhiteSpace(orgId))
            {
                List<Pupil> pupils = await _pupilsService.GetAsyncForOrg(orgId);
                foreach (Pupil p in pupils)
                {
                    p.isExerciseReady = isReady(p);
                }
                pupils.Reverse();
                return new JsonResult(pupils);
            }
            else
            {
                if (_user.IsInRole("admin"))
                {
                    List<Pupil> pupils = await _pupilsService.GetAsync();
                    foreach(Pupil p in pupils)
                    {
                        p.isExerciseReady = isReady(p);
                    }
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
        //[EnableCors("AllowAll")]
        public async Task<ActionResult<Pupil>> Get(string id)
        {
            var _user = _httpContextAccessor.HttpContext?.User;

            if (_user is null) { return NotFound("JWT error"); }
            if (_user.IsInRole("admin"))
            {
                return new JsonResult(await _pupilsService.GetAsync(id));
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
                    Pupil? pupil = await _pupilsService.GetAsync(id);
                    if (pupil is not null) { pupil.isExerciseReady = isReady(pupil); }
                    return new JsonResult(pupil);
                }
                else { return Forbid(); }
            }
            else
            {
                return Forbid();
            }
        }

        /// <summary>
        /// Создаем нового подопечного и прикрепляем к организации, вызывавшего пользователя. Админ может зарегестрировать без организации или в любой организации
        /// </summary>
        /// <param name="newPupil"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Add")]
        //[EnableCors("AllowAll")]
        public async Task<IActionResult> Post(Pupil newPupil)
        {
            var _user = _httpContextAccessor.HttpContext?.User;

            if (_user is null) { return NotFound("JWT error"); }

            if (_user.IsInRole("admin"))
            {
                await _pupilsService.CreateAsync(newPupil);
                return CreatedAtAction(nameof(Get), new { id = newPupil.Id }, newPupil);
            }

            string? orgId = _user.FindFirstValue("OrganisationID");
            if (orgId == null) { return BadRequest("User is not associated with organisation"); }
            var _organisation = await _organisationsService.GetAsync(orgId);
            if(_organisation is null) { return BadRequest("Organisation not found"); }

            bool _authorized = _user.FindFirstValue("UserID") == _organisation?.ActiveUsers.Find(s => s == _user.FindFirstValue("UserID"));
            if(_authorized)
            {
                newPupil.Organisation = orgId;
                await _pupilsService.CreateAsync(newPupil);
                _organisation.ActivePupils.Add(newPupil.Id);
                await _organisationsService.UpdateAsync(_organisation.Id, _organisation);
                return CreatedAtAction(nameof(Get), new { id = newPupil.Id }, newPupil);
            }
            else { return Forbid(); }   
        }

        [HttpPut]
        [Route("Edit/{id:length(24)}")]
        public async Task<IActionResult> Update(string id, Pupil updatedPupil)
        {
            var _user = _httpContextAccessor.HttpContext?.User;

            if (_user is null) { return NotFound("JWT error"); }
            if(id != updatedPupil.Id) { return BadRequest("Wrong Id"); }
            if (_user.IsInRole("admin"))
            {
                await _pupilsService.UpdateAsync(id, updatedPupil);
                return NoContent();
            }

            string? orgId = _user.FindFirstValue("OrganisationID");
            var _organisation = await _organisationsService.GetAsync(orgId);
            if (_organisation is null ) { return BadRequest("Organisation not found"); }

            bool _authorized = _user.FindFirstValue("UserID") == _organisation?.ActiveUsers.Find(s => s == _user.FindFirstValue("UserID"));

            if( _authorized) 
            {
                //if (string.IsNullOrEmpty(updatedPupil.Organisation)) { return BadRequest("Pupil is not assighned to organisation"); }
                bool _isPupil = updatedPupil.Organisation == _organisation?.Id;
                bool _isInOrg = !string.IsNullOrEmpty(_organisation?.ActivePupils.FirstOrDefault(s => s == updatedPupil.Id));
                //if (!_isPupil && !_isInOrg) { return BadRequest("Pupil in not associated with organisation"); }
                await _pupilsService.UpdateAsync(id, updatedPupil);
                return new JsonResult(updatedPupil);
            }
            else return Forbid();  
        }


        [HttpPut]
        [Route("AddToOrganisation/{pupilId:length(24)}/{orgId:length(24)}")]
        [Authorize(Roles = "admin, localadmin")]
        public async Task<IActionResult> AddToOrganisation(string pupilId, string orgId)
        {
            var _user = _httpContextAccessor.HttpContext?.User;

            if (_user is null) { return NotFound("JWT error"); }
            var _organisation = await _organisationsService.GetAsync(orgId);
            if (_organisation is null) { return BadRequest("Organisation not found"); }
            if (_user.IsInRole("admin"))
            {
                var _pupil = await _pupilsService.GetAsync(pupilId);
                if (_pupil is null) { return BadRequest("Pupil not found"); }
                _organisation.ActivePupils.Add(pupilId);
                _pupil.Organisation = orgId;
                await _pupilsService.UpdateAsync(pupilId, _pupil);
                await _organisationsService.UpdateAsync(_organisation.Id, _organisation);
                return NoContent();
            }

            bool _authorized = _user.FindFirstValue("UserID") == _organisation?.ActiveUsers.Find(s => s == _user.FindFirstValue("UserID"));
            if (_authorized)
            {
                var _pupil = await _pupilsService.GetAsync(pupilId);
                if (_pupil is null) { return BadRequest("Pupil not found"); }
                _organisation.ActivePupils.Add(pupilId);
                _pupil.Organisation = orgId;
                await _pupilsService.UpdateAsync(pupilId, _pupil);
                await _organisationsService.UpdateAsync(_organisation.Id, _organisation);
                return NoContent();
            }
            else return Forbid();
        }

        [HttpPut]
        [Route("RemoveFromOrganisation/{pupilId:length(24)}")]
        [Authorize(Roles = "admin, localadmin")]
        public async Task<IActionResult> RemoveFromOrganisation(string pupilId)
        {
            var _user = _httpContextAccessor.HttpContext?.User;

            if (_user is null) { return NotFound("JWT error"); }
            if (_user.IsInRole("admin"))
            {
                var _pupil = await _pupilsService.GetAsync(pupilId);
                if (_pupil is null ) { return BadRequest("Pupil not found"); }
                var organisation = await _organisationsService.GetAsync(_pupil?.Organisation);
                if (organisation is null ) { return BadRequest("Organisation not found"); }
                organisation.ActivePupils.Remove(pupilId);
                _pupil.Organisation = string.Empty;
                await _pupilsService.UpdateAsync(pupilId, _pupil);
                await _organisationsService.UpdateAsync(organisation.Id, organisation);
                return NoContent();
            }

            string? orgId = _user.FindFirstValue("OrganisationID");
            var _organisation = await _organisationsService.GetAsync(orgId);
            if (_organisation is null) { return BadRequest("Organisation not found"); }

            bool _authorized = _user.FindFirstValue("UserID") == _organisation?.ActiveUsers.Find(s => s == _user.FindFirstValue("UserID"));

            if (_authorized)
            {
                var _pupil = await _pupilsService.GetAsync(pupilId);
                if (_pupil is null) { return BadRequest("Pupil not found"); }
                _organisation.ActivePupils.Remove(pupilId);
                _pupil.Organisation = string.Empty;
                await _pupilsService.UpdateAsync(pupilId, _pupil);
                await _organisationsService.UpdateAsync(_organisation.Id, _organisation);
                return NoContent();
            }
            else return Forbid();
        }

        [HttpDelete]
        [Route("Delete/{id:length(24)}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete(string id)
        {
            var pupil = await _pupilsService.GetAsync(id);

            if (pupil is null)
            {
                return NotFound();
            }

            await _pupilsService.RemoveAsync(id);
            return NoContent();
        }

        [HttpPost]
        [Route("AddCourse/{pupilId:length(24)}")]
        [Authorize]
        public async Task<IActionResult> AddCourse(string pupilId, [FromBody]string courseId)
        {
            var _user = _httpContextAccessor.HttpContext?.User;

            if (_user is null) { return NotFound("JWT error"); }
            if (string.IsNullOrEmpty(courseId)) { return NotFound("Course Id absent"); }
            Course? course = await _coursesService.GetAsync(courseId);
            if (course is null) { return BadRequest("Course not found (wrong courseId)"); }
            var _admin = _user.IsInRole("admin");
            string? orgId = _user.FindFirstValue("OrganisationID");
            var _organisation = await _organisationsService.GetAsync(orgId);

            if (_organisation is null)
            {
                if (_admin)
                {
                    return NotFound("No organisation found");
                }
                else
                {
                    return Forbid();
                }
            }

            bool _authorised = _user.FindFirstValue("UserID") == _organisation?.ActiveUsers.Find(s => s == _user.FindFirstValue("UserID"));
            if (_authorised || _admin)
            {
                if (_organisation.ActivePupils.Contains(pupilId))
                {
                    Pupil? _pupil = await _pupilsService.GetAsync(pupilId);
                    if (_pupil is not null)
                    {
                        if(_pupil.ActiveCourse is not null) { return BadRequest("Pupil is passing course. Finish or remove it before adding new."); }
                        _pupil.ActiveCourse = course;
                        await _pupilsService.UpdateAsync(_pupil.Id, _pupil);
                        return Ok();
                    }
                    else { return NotFound("Pupil not found"); }
                }
                else
                {
                    return Forbid();
                }
            }
            else
            {
                return Forbid();
            }
        }

        [HttpPut]
        [Route("DeleteCourse/{pupilId:length(24)}")]
        [Authorize]
        public async Task<IActionResult> DeleteCourse(string pupilId)
        {
            var _user = _httpContextAccessor.HttpContext?.User;

            if (_user is null) { return NotFound("JWT error"); }
            var _admin = _user.IsInRole("admin");
            string? orgId = _user.FindFirstValue("OrganisationID");
            var _organisation = await _organisationsService.GetAsync(orgId);

            if (_organisation is null)
            {
                if (_admin)
                {
                    return NotFound("No organisation found");
                }
                else
                {
                    return Forbid();
                }
            }

            bool _authorised = _user.FindFirstValue("UserID") == _organisation?.ActiveUsers.Find(s => s == _user.FindFirstValue("UserID"));
            if (_authorised || _admin)
            {
                if (_organisation.ActivePupils.Contains(pupilId))
                {
                    Pupil? _pupil = await _pupilsService.GetAsync(pupilId);
                    if (_pupil is not null)
                    {
                        _pupil.ActiveCourse = null;
                        await _pupilsService.UpdateAsync(_pupil.Id, _pupil);
                        return Ok();
                    }
                    else { return NotFound("Pupil not found"); }
                }
                else
                {
                    return Forbid();
                }
            }
            else
            {
                return Forbid();
            }
        }

        [HttpPut]
        [Route("FinishCurrentCourse/{pupilId:length(24)}")]
        [Authorize]
        public async Task<IActionResult> FinishCurrentCourse(string pupilId)
        {
            var _user = _httpContextAccessor.HttpContext?.User;

            if (_user is null) { return NotFound("JWT error"); }
            var _admin = _user.IsInRole("admin");
            string? orgId = _user.FindFirstValue("OrganisationID");
            var _organisation = await _organisationsService.GetAsync(orgId);

            if (_organisation is null)
            {
                if (_admin)
                {
                    return NotFound("No organisation found");
                }
                else
                {
                    return Forbid();
                }
            }

            bool _authorised = _user.FindFirstValue("UserID") == _organisation?.ActiveUsers.Find(s => s == _user.FindFirstValue("UserID"));
            if (_authorised || _admin)
            {
                if (_organisation.ActivePupils.Contains(pupilId))
                {
                    Pupil? _pupil = await _pupilsService.GetAsync(pupilId);
                    if (_pupil is not null)
                    {
                        if( _pupil.PassedCourses is null) { _pupil.PassedCourses = new(); }
                        if(_pupil.ActiveCourse is null) 
                        { 
                            return NotFound("No active course");
                        }
                        _pupil.PassedCourses.Add(_pupil.ActiveCourse);
                        _pupil.ActiveCourse = null;
                        await _pupilsService.UpdateAsync(_pupil.Id, _pupil);
                        return Ok();
                    }
                    else { return NotFound("Pupil not found"); }
                }
                else
                {
                    return Forbid();
                }
            }
            else
            {
                return Forbid();
            }
        }

        private bool isReady (Pupil pupil)
        {
            if (pupil.ActiveCourse is null) return false;
            var exercise =  pupil.ActiveCourse.Exercises.FirstOrDefault(e => e.isFinished == false);
            if (exercise is null) return false;
            var order = exercise.Orders.LastOrDefault(o => o.isFinished == true);
            if(order?.OrderStatistic?.EndTime is null) return true;
            DateTime actualDate = (DateTime)order.OrderStatistic.EndTime;
            
            bool isOrder = order is not null && (exercise.Orders.Count > 0);
            
            if ((actualDate.AddDays(3) < DateTime.Now) && isOrder) return true;
            else return false;
        }
    }
}
