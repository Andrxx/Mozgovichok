using Microsoft.AspNetCore.Mvc;
using mozgovichok.Models.Users;
using mozgovichok.Services.Users;
using NSwag.Annotations;

namespace mozgovichok.Controllers.Users
{
    [ApiController]
    [Route("api/v1/Users/[controller]")]
    public class SpecialistsController : ControllerBase
    {
        private readonly SpecialistsService _specialistsService;

        public SpecialistsController(SpecialistsService specialistsService)
        {
            _specialistsService = specialistsService;
        }

        [HttpGet]
        public async Task<List<Specialist>> Get() =>
           await _specialistsService.GetAsync();

        [HttpGet("{id:length(24)}")]
        public async Task<ActionResult<Specialist>> Get(string id)
        {
            var specialist = await _specialistsService.GetAsync(id);

            if (specialist is null)
            {
                return NotFound();
            }

            return specialist;
        }

        [HttpPost]
        public async Task<IActionResult> Post(Specialist newSpecialist)
        {

            await _specialistsService.CreateAsync(newSpecialist);

            return CreatedAtAction(nameof(Get), new { id = newSpecialist.Id }, newSpecialist);
        }

        [HttpPut("{id:length(24)}")]
        public async Task<IActionResult> Update(string id, Specialist updatedSpecialist)
        {
            var admin = await _specialistsService.GetAsync(id);

            if (admin is null)
            {
                return NotFound();
            }

            updatedSpecialist.Id = admin.Id;

            await _specialistsService.UpdateAsync(id, updatedSpecialist);

            return NoContent();
        }

        [HttpDelete("{id:length(24)}")]
        public async Task<IActionResult> Delete(string id)
        {
            var specialist = await _specialistsService.GetAsync(id);

            if (specialist is null)
            {
                return NotFound();
            }

            await _specialistsService.RemoveAsync(id);

            return NoContent();
        }
    }
}
