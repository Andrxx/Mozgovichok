using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using mozgovichok.Infrastructure;
using mozgovichok.Models.Courses;
using mozgovichok.Models.Courses.Statistics;
using mozgovichok.Models.DTO;
using mozgovichok.Models.Organisations;
using mozgovichok.Services.Organisations;
using mozgovichok.Services.Users;
using System.Security.Claims;
using System.Security.Cryptography;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace mozgovichok.Controllers.Organisations
{
    [ApiController]
    [Route("api/v1/sessions/")]
    //[Route("VRapi/v1/Sessions/")]
    [Authorize]
    public class SessionsController : ControllerBase
    {
        private readonly OrganisationsService _organisationsService;
        private readonly PupilsService _pupilsService;
        private readonly UsersService _usersService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SessionsController(OrganisationsService organisationService, PupilsService pupilsService,  IHttpContextAccessor httpContextAccessor, UsersService usersService)
        {
            _organisationsService = organisationService;
            _pupilsService = pupilsService;
            _httpContextAccessor = httpContextAccessor;
            _usersService = usersService;
        }

        [HttpGet]
        [Route("GetSessions/{orgId:length(24)?}")]
        //[Route("VRapi/v1/sessions/GetSessions/{orgId:length(24)}")]
        public async Task<ActionResult<List<Session>>> GetSessions(string? orgId)
        {

            var _user = _httpContextAccessor.HttpContext?.User;
            if (_user is null) { return NotFound("JWT error"); }

            bool _admin = _user.IsInRole("admin");
            if (string.IsNullOrWhiteSpace(orgId))
            {
                orgId = _user.FindFirstValue("OrganisationID");
            }

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

            bool _authorized = _user.FindFirstValue("UserID") == _organisation?.ActiveUsers.Find(s => s == _user.FindFirstValue("UserID"));
            if (_authorized || _admin)
            {
                List<Session>? _sessions = _organisation?.Sessions;
                return new JsonResult(_sessions);
            }
            else { return Forbid(); }
        }

        [HttpGet]
        [Route("GetSession/{sessionId:length(24)}/{orgId:length(24)?}")]
        //[Route("VRapi/v1/sessions/GetSession/{orgId:length(24)}&{sessionId:length(24)}")]
        public async Task<ActionResult<Session>> GetSessionAsync( string sessionId, string? orgId)
        {
            var _user = _httpContextAccessor.HttpContext?.User;
            if (_user is null) { return NotFound("JWT error"); }

            bool _admin = _user.IsInRole("admin");

            if (string.IsNullOrWhiteSpace(orgId))
            {
                orgId = _user.FindFirstValue("OrganisationID");
            }
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

            bool _authorized = _user.FindFirstValue("UserID") == _organisation?.ActiveUsers.Find(s => s == _user.FindFirstValue("UserID"));
            if (_authorized || _admin)
            {
                _organisation.Sessions ??= new();
                Session? _session = _organisation?.Sessions.Find(s => s.Id == sessionId);
                if (_session is null) { return NotFound("Wrong session Id"); }
                return _session;
            }
            else { return Forbid(); }
        }

        [HttpGet]
        [Route("~/VRapi/v1/sessions/GetCourse/{PupilId:length(24)}")]
        public async Task<ActionResult<Session>> GetCourse(string PupilId)
        {
            Course? _course;
            var _user = _httpContextAccessor.HttpContext?.User;
            if (_user is null) { return NotFound("JWT error"); }
            string? orgId = _user.FindFirstValue("OrganisationID");

            var _organisation = await _organisationsService.GetAsync(orgId);
            if (_organisation is null)
            {
                return Forbid();
            }

            Session? _session = _organisation.Sessions?.FirstOrDefault(s => s.ExaminedPupil?.Id == PupilId);
            if(_session is not null)
            {
                _course = _session.ExaminedPupil?.ActiveCourse;
                return new JsonResult(_course);
            }
            //_course = _organisation.Sessions?.FirstOrDefault(s => s?.ExaminedPupil?.Id == PupilId).ExaminedPupil?.ActiveCourse;  // _pupilsService.GetAsync(PupilId).Result?.ActiveCourse; ;
            else
            {
                return NotFound("No Active courses finded");
            }
        }



        /// <summary>
        /// Создание сессии. Если админ - ИД организации явно передаются в модели сесии, если пользователь - из токена
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("Create")]
        public async Task<IActionResult> Create([FromBody] SessionCreateData data)
        {
            Session newSession = new();
            string? orgId;
            Pupil? _pupil = null;
            var _user = _httpContextAccessor.HttpContext?.User;
            if (_user is null) { return NotFound("JWT error"); }
            var _admin = _user.IsInRole("admin");
            if (!_admin) { orgId = _user.FindFirstValue("OrganisationID"); }
            else orgId = data.OrgId;

            Organisation? _organisation = await _organisationsService.GetAsync(orgId);
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
            if (data.AddPupil) 
            { 
                bool _isPupil = !string.IsNullOrEmpty(_organisation.ActivePupils.FirstOrDefault(s => s == data.PupilId));
                if (!_isPupil) { return BadRequest("Pupil in not associated with organisation"); }
                _pupil = await _pupilsService.GetAsync(data.PupilId);
                if (_pupil is null) { return BadRequest("Pupil not found"); }
            }

            bool _authorised = _user.FindFirstValue("UserID") == _organisation?.ActiveUsers.Find(s => s == _user.FindFirstValue("UserID"));
            if (_authorised || _admin)
            {
                newSession.StartTime = DateTime.Now;
                newSession.IsReady = true;
                newSession.IsActive = false;
                newSession.EndTime = null;
                newSession.OrganisationStarted =_organisation.Id;
                newSession.ExaminedPupil = _pupil; 
                newSession.Id = ObjectId.GenerateNewId().ToString();
                _organisation.Sessions ??= new();
                _organisation.Sessions.Add(newSession);
                await _organisationsService.UpdateAsync(_organisation.Id, _organisation);
                return new JsonResult(newSession);
            }
            else {
                return Forbid();
            }
        }

        [HttpPut]
        [Route("StatusToggle/{sessionId:length(24)}/{orgId:length(24)?}")]
        public async Task<IActionResult> StatusToggle(string sessionId, string? orgId, [FromBody]bool Started)
        {
            var _user = _httpContextAccessor.HttpContext?.User;
            if (_user is null) { return NotFound("JWT error"); }
            var _admin = _user.IsInRole("admin");
            if (!_admin) { orgId = _user.FindFirstValue("OrganisationID"); }
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
            var _session = _organisation.Sessions.FirstOrDefault(s => s.Id == sessionId);
            if (_session is null) { return NotFound("No session found"); }


            bool _authorised = _user.FindFirstValue("UserID") == _organisation?.ActiveUsers.Find(s => s == _user.FindFirstValue("UserID"));
            if (_authorised || _admin)
            {
                
                _session.IsActive = Started;
                await _organisationsService.UpdateAsync(_organisation.Id, _organisation);
                return new JsonResult(_session);
            }
            else
            {
                return Forbid();
            }
        }

        [HttpPut]
        [Route("Delete/{sessionId:length(24)}/{orgId:length(24)?}")]
        public async Task<IActionResult> Delete(string sessionId, string? orgId)
        {
            var _user = _httpContextAccessor.HttpContext?.User;

            if (_user is null) { return NotFound("JWT error"); }
            var _admin = _user.IsInRole("admin");
            if (string.IsNullOrWhiteSpace(orgId))
            {
                orgId = _user.FindFirstValue("OrganisationID");
            }
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
                if (_organisation.Sessions != null)
                {
                    _organisation.Sessions.RemoveAll(s => s.Id == sessionId);
                    await _organisationsService.UpdateAsync(_organisation.Id, _organisation);
                    return Ok();
                }
                else
                {
                    return BadRequest();
                }
            }
            else
            {
                return Forbid();
            }
        }

        [HttpPut]
        [Route("AddPupil/{sessionId:length(24)}/{orgId:length(24)?}")]
        public async Task<IActionResult> AddPupil(string sessionId, string? orgId, [FromBody]string pupilId)
        {
            var _user = _httpContextAccessor.HttpContext?.User;

            if (_user is null) { return NotFound("JWT error"); }
            var _admin = _user.IsInRole("admin");
            if (!_admin)
            {
                orgId = _user.FindFirstValue("OrganisationID");
            }
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
            Pupil? _pupil = await _pupilsService.GetAsync(pupilId); 
            if(_pupil is null) { return BadRequest("Pupil not Found"); }
            bool _authorised = _user.FindFirstValue("UserID") == _organisation?.ActiveUsers.Find(s => s == _user.FindFirstValue("UserID"));
            if (_authorised || _admin)
            {
                _organisation.Sessions ??= new List<Session>();
                Session _session = _organisation.Sessions.FirstOrDefault(s => s.Id == sessionId);
                if (_session is not null)
                {
                    _organisation.Sessions.Remove(_session);
                    _session.ExaminedPupil = _pupil;
                    _session.Id = sessionId;
                    _organisation.Sessions.Add(_session);
                    await _organisationsService.UpdateAsync(_organisation.Id, _organisation);
                    //pupil.Organisation = _organisation.Id;
                    //await _pupilsService.UpdateAsync(pupil.Id, pupil);
                    return new JsonResult(_session);
                }
                else
                {
                    return NotFound("Error in session data");
                }
            }
            else
            {
                return Forbid();
            }
        }

        [HttpPut]
        [Route("DeletePupil/{sessionId:length(24)}/{orgId:length(24)?}")]
        public async Task<IActionResult> DeletePupil(string sessionId, string? orgId)
        {
            var _user = _httpContextAccessor.HttpContext?.User;

            if (_user is null) { return NotFound("JWT error"); }
            var _admin = _user.IsInRole("admin");
            if (string.IsNullOrWhiteSpace(orgId))
            {
                orgId = _user.FindFirstValue("OrganisationID");
            }
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
                _organisation.Sessions ??= new List<Session>();
                Session _session = _organisation.Sessions.FirstOrDefault(s => s.Id == sessionId);
                if (_session is not null)
                {
                    _organisation.Sessions.Remove(_session);
                    _session.ExaminedPupil = null;
                    _session.Id = sessionId;
                    _organisation.Sessions.Add(_session);
                    await _organisationsService.UpdateAsync(_organisation.Id, _organisation);
                    return new JsonResult(_session);
                }
                else
                {
                    return NotFound("Error in session data");
                }
            }
            else
            {
                return Forbid();
            }
        }

        [HttpPost]
        [Route("~/VRapi/v1/sessions/SendStats/{sessionId:length(24)}")]
        public async Task<ActionResult<Session>> SendStats(string sessionId, VROrderStatistic stats)
        {

            var _user = _httpContextAccessor.HttpContext?.User;
            if (_user is null) { return NotFound("JWT error"); }

            string? orgId = _user.FindFirstValue("OrganisationID");

            var _organisation = await _organisationsService.GetAsync(orgId);
            if (_organisation is null)
            {
                return BadRequest("No organisation found");
            }

            Session? _session = _organisation.Sessions?.FirstOrDefault(s => s.Id == sessionId);
            if (_session is not null)
            {
                Exercise? _exercise =  _session?.ExaminedPupil?.ActiveCourse?.Exercises.FirstOrDefault(e => e.Id == stats.ExerciseId);
                if (_exercise is null) { return BadRequest("Wrong exercise Id"); }
                Order? _order = _exercise.Orders.FirstOrDefault(o => o.Id == stats.OrderId);
                if (_order is null) { return BadRequest("Wrong odred Id"); }
                _order.OrderStatistic = StatsHandler.CreateOrderStats(stats);
                if (stats.isFinished) {
                    _order.isFinished = true;
                    _order.OrderStatistic.EndTime = DateTime.UtcNow;
                    _exercise.ExerciseStatistic = StatsHandler.CountExerciseStats(_exercise);
                }
                await _organisationsService.UpdateAsync(_organisation.Id, _organisation);
                await _pupilsService.UpdateAsync(_session.ExaminedPupil.Id, _session.ExaminedPupil);
                return new OkResult();
            }
   
            else
            {
                return NotFound("No Active courses finded");
            }
        }

    }
}
