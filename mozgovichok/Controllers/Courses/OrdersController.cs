using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using mozgovichok.Models.Courses;
using mozgovichok.Models.Users;
using mozgovichok.Services.Courses;
using mozgovichok.Services.Users;

namespace mozgovichok.Controllers.Courses
{
    [ApiController]
    [Route("api/v1/courses/ordes")]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly OrdersService _ordersServices;

        public OrdersController(OrdersService ordersServices) =>
            _ordersServices = ordersServices;


        [HttpGet]
        //[Authorize(Roles = "admin")]
        [Route("GetOrders")]
        public async Task<List<Order>> Get() =>
           await _ordersServices.GetAsync();

        [HttpGet]
        [Route("GetOrder/{id:length(24)}")]
        [Authorize]
        public async Task<ActionResult<Order>> Get(string id)
        {
            var order = await _ordersServices.GetAsync(id);

            if (order is null)
            {
                return NotFound();
            }

            return order;
        }

        [HttpPost]
        [Route("Add")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Post(Order newOrder)
        {

            await _ordersServices.CreateAsync(newOrder);

            return CreatedAtAction(nameof(Get), new { id = newOrder.Id }, newOrder);
        }

        [HttpPut]
        [Route("Edit/{id:length(24)}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Update(string id, Order updatedOrder)
        {
            var admin = await _ordersServices.GetAsync(id);

            if (admin is null)
            {
                return NotFound();
            }

            updatedOrder.Id = admin.Id;

            await _ordersServices.UpdateAsync(id, updatedOrder);

            return NoContent();
        }

        [HttpDelete]
        [Route("Delete/{id:length(24)}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete(string id)
        {
            var admin = await _ordersServices.GetAsync(id);

            if (admin is null)
            {
                return NotFound();
            }
            await _ordersServices.RemoveAsync(id);

            return NoContent();
        }
    }
}
