using Microsoft.AspNetCore.Mvc;
using mozgovichok.Models.Users;
using mozgovichok.Services.Users;
using NSwag.Annotations;

namespace mozgovichok.Controllers.Users
{
    [ApiController]
    [Route("api/v1/Users/[controller]")]
    public class AdminsController : ControllerBase
    {
        private readonly AdminsService _adminsService;

        public AdminsController(AdminsService adminsService)
        {
            _adminsService = adminsService;
        }


        [HttpGet]
        public async Task<List<Admin>> Get() =>
            await _adminsService.GetAsync();

        [HttpGet("{id:length(24)}")]
        public async Task<ActionResult<Admin>> Get(string id)
        {
            var admin = await _adminsService.GetAsync(id);

            if (admin is null)
            {
                return NotFound();
            }

            return admin;
        }

        [HttpPost]
        public async Task<IActionResult> Post(Admin newAdmin)
        {

            await _adminsService.CreateAsync(newAdmin);

            return CreatedAtAction(nameof(Get), new { id = newAdmin.Id }, newAdmin);
        }

        [HttpPut("{id:length(24)}")]
        public async Task<IActionResult> Update(string id, Admin updatedAdmin)
        {
            var admin = await _adminsService.GetAsync(id);

            if (admin is null)
            {
                return NotFound();
            }

            updatedAdmin.Id = admin.Id;

            await _adminsService.UpdateAsync(id, updatedAdmin);

            return NoContent();
        }

        [HttpDelete("{id:length(24)}")]
        public async Task<IActionResult> Delete(string id)
        {
            var admin = await _adminsService.GetAsync(id);

            if (admin is null)
            {
                return NotFound();
            }

            await _adminsService.RemoveAsync(id);

            return NoContent();
        }
    }
}
