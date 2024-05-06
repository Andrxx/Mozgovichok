using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using mozgovichok.Models.Organisations;
using mozgovichok.Models.Users;
using mozgovichok.Services.Organisations;

namespace mozgovichok.Controllers.Organisations
{
    [ApiController]
    [Route("api/v1/Tariff/")]
    [Authorize(Roles = "admin")]
    public class TariffsController : ControllerBase
    {
        private readonly TariffsService _tariffsService;
        private readonly OrganisationsService _organisationsService;
        //Organisation? organisation;
        public TariffsController(TariffsService tariffsService, OrganisationsService organisationsService)
        {
            _tariffsService = tariffsService;
            _organisationsService = organisationsService;
        }


        /// <summary>
        /// Получение тарифов, метод работает с тарифом в БД
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetTariffs")]
        [AllowAnonymous]
        public async Task<List<Tariff>> Get() =>
            await _tariffsService.GetAsync();

        /// <summary>
        /// Получение тарифа по ИД, метод работает с тарифом в БД
        /// </summary>
        [HttpGet]
        [Route("GetTariff/{id:length(24)}")]
        [AllowAnonymous]
        public async Task<ActionResult<Tariff>> Get(string id)
        {
            var admin = await _tariffsService.GetAsync(id);

            if (admin is null)
            {
                return NotFound();
            }

            return admin;
        }

        //методы редактирования тарифов, в текущей версии не используем

        [HttpPost]
        [Route("Add")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Post(Tariff newTariff)
        {

            await _tariffsService.CreateAsync(newTariff);

            return CreatedAtAction(nameof(Get), new { id = newTariff.Id }, newTariff);
        }

        [HttpPut]
        [Route("Edit/{id:length(24)}")]
        public async Task<IActionResult> Update(string id, Tariff updatedTariff)
        {
            var tariff = await _tariffsService.GetAsync(id);

            if (tariff is null)
            {
                return NotFound();
            }

            updatedTariff.Id = tariff.Id;

            await _tariffsService.UpdateAsync(id, updatedTariff);

            return NoContent();
        }

        [HttpDelete]
        [Route("Delete/{id:length(24)}")]
        public async Task<IActionResult> Delete(string id)
        {
            var admin = await _tariffsService.GetAsync(id);

            if (admin is null)
            {
                return NotFound();
            }

            await _tariffsService.RemoveAsync(id);

            return NoContent();
        }
    }
}
