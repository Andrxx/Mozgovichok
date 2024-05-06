using Microsoft.AspNetCore.Mvc;
using mozgovichok.Infrastructure;
using mozgovichok.Models.Organisations;
using mozgovichok.Services.Organisations;

namespace mozgovichok.Controllers.Organisations
{
    [ApiController]
    [Route("api/v1/org/payments/")]
    public class PaymentsController : ControllerBase
    {
        private readonly OrganisationsService _organisationsService;

        public PaymentsController (OrganisationsService organisationsService)
        {
            _organisationsService = organisationsService;
        }

        //[HttpGet]
        //[Route("GetOrganisations")]
        //public async Task<List<Organisation>> Get() =>
        //    await _organisationsService.GetAsync();

        [HttpGet]
        [Route("getpayments/{id:length(24)}")]
        public async Task<ActionResult<List<Payment>>> Get(string id)
        {
            var payments = await _organisationsService.GetPaymentsAsync(id);

            if (payments is null)
            {
                return NotFound("No organisation found");
            }
            return payments;
        }

        [HttpPost]
        [Route("add/{orgId:length(24)}")]
        public async Task<IActionResult> Post([FromRoute] string orgId, [FromBody] Payment newPayment)
        {
            var payment = await _organisationsService.CreatePaymentAsync(orgId, newPayment);
            if (payment is null)
            {
                return NotFound();
            }

            return CreatedAtAction(nameof(Get), new { id = newPayment.Id }, newPayment);
        }
    }
}
