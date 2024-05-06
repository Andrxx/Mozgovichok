using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using mozgovichok.Models.Courses;
using mozgovichok.Services.Courses;

namespace mozgovichok.Controllers.Courses
{
    [ApiController]
    [Route("api/v1/courses/exercises")]
    [Authorize]
    public class ExercisesController : ControllerBase
    {
        private readonly ExercisesService _exercisesServices;
        private readonly OrdersService _ordersService;

        public ExercisesController(ExercisesService exercisesServices, OrdersService ordersService)
        {
            _exercisesServices = exercisesServices;
            _ordersService = ordersService;
        }

        [HttpGet]
        [Route("GetExercises")]
        public async Task<List<Exercise>> Get() =>
           await _exercisesServices.GetAsync();

        [HttpGet]
        [Route("GetExercise/{id:length(24)}")]
        public async Task<ActionResult<Exercise>> Get(string id)
        {
            var exercise = await _exercisesServices.GetAsync(id);

            if (exercise is null)
            {
                return NotFound();
            }

            return exercise;
        }

        [HttpPost]
        [Route("Add")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Post(Exercise newExercise)
        {

            await _exercisesServices.CreateAsync(newExercise);

            return CreatedAtAction(nameof(Get), new { id = newExercise.Id }, newExercise);
        }

        [HttpPut]
        [Route("Edit/{id:length(24)}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Update(string id, Exercise updatedExercise)
        {
            var exercise = await _exercisesServices.GetAsync(id);

            if (exercise is null)
            {
                return NotFound();
            }

            updatedExercise.Id = exercise.Id;

            await _exercisesServices.UpdateAsync(id, updatedExercise);

            return NoContent();
        }

        [HttpDelete]
        [Route("Delete/{id:length(24)}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete(string id)
        {
            var admin = await _exercisesServices.GetAsync(id);

            if (admin is null)
            {
                return NotFound();
            }
            await _exercisesServices.RemoveAsync(id);

            return NoContent();
        }

        [HttpPut]
        [Route("AddOrder/{exerciseId:length(24)}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> AddOrder(string exerciseId, [FromBody] string orderId)
        {
            var exercise = await _exercisesServices.GetAsync(exerciseId);
            Order? order = await _ordersService.GetAsync(orderId);
            if (exercise is null || order is null)
            {
                return NotFound("Wrong exercise or order.");
            }

            exercise.Orders.Add(order);

            await _exercisesServices.UpdateAsync(exerciseId, exercise);

            return new JsonResult(exercise);
        }
    }
}
