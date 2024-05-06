using Microsoft.AspNetCore.Mvc;
using mozgovichok.Models.Users;
using mozgovichok.Services.Users;
using NSwag.Annotations;

namespace mozgovichok.Controllers.Users
{
    [ApiController]
    [Route("api/v1/Users/[controller]")]
    public class CuratorsController : ControllerBase
    {
        private readonly CuratorsService _curatorsService;

        public CuratorsController(CuratorsService curatorsService)
        {
            _curatorsService = curatorsService;
        }
        [HttpGet]
        public async Task<List<Curator>> Get() =>
           await _curatorsService.GetAsync();

        [HttpGet("{id:length(24)}")]
        public async Task<ActionResult<Curator>> Get(string id)
        {
            var curator = await _curatorsService.GetAsync(id);

            if (curator is null)
            {
                return NotFound();
            }

            return curator;
        }

        [HttpPost]
        public async Task<IActionResult> Post(Curator newCurator)
        {
            await _curatorsService.CreateAsync(newCurator);

            return CreatedAtAction(nameof(Get), new { id = newCurator.Id }, newCurator);
        }

        [HttpPut("{id:length(24)}")]
        public async Task<IActionResult> Update(string id, Curator updatedCurator)
        {
            var curator = await _curatorsService.GetAsync(id);

            if (curator is null)
            {
                return NotFound();
            }

            updatedCurator.Id = curator.Id;

            await _curatorsService.UpdateAsync(id, updatedCurator);

            return NoContent();
        }

        [HttpDelete("{id:length(24)}")]
        public async Task<IActionResult> Delete(string id)
        {
            var curator = await _curatorsService.GetAsync(id);

            if (curator is null)
            {
                return NotFound();
            }

            await _curatorsService.RemoveAsync(id);

            return NoContent();
        }
    }
}
