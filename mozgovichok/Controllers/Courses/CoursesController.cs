using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using mozgovichok.Models.Courses;
using mozgovichok.Services.Courses;

namespace mozgovichok.Controllers.Courses
{
    [ApiController]
    [Route("api/v1/courses/courses")]
    [Authorize]
    public class CoursesController : ControllerBase
    {
        private readonly CoursesService _coursesServices;
        private readonly ExercisesService _exercisesServices;

        public CoursesController(CoursesService coursesServices, ExercisesService exercisesService)
        {
            _coursesServices = coursesServices;
            _exercisesServices = exercisesService;
        }
        
        [HttpGet]
        [Authorize]
        [Route("GetCourses")]
        public async Task<List<Course>> Get() =>
           await _coursesServices.GetAsync();


        [HttpGet]
        [Route("GetCourse/{id:length(24)}")]
        [Authorize]
        public async Task<ActionResult<Course>> Get(string id)
        {
            var course = await _coursesServices.GetAsync(id);

            if (course is null)
            {
                return NotFound();
            }

            return course;
        }

        [HttpPost]
        [Route("Add")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Post(Course newCourse)
        {
            await _coursesServices.CreateAsync(newCourse);

            return CreatedAtAction(nameof(Get), new { id = newCourse.Id }, newCourse);
        }

        [HttpPut]
        [Route("Edit/{id:length(24)}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Update(string id, Course updatedCourse)
        {
            var course = await _coursesServices.GetAsync(id);

            if (course is null)
            {
                return NotFound();
            }

            updatedCourse.Id = course.Id;

            await _coursesServices.UpdateAsync(id, updatedCourse);

            return NoContent();
        }

        [HttpDelete]
        [Route("Delete/{id:length(24)}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete(string id)
        {
            var course = await _coursesServices.GetAsync(id);

            if (course is null)
            {
                return NotFound();
            }
            await _coursesServices.RemoveAsync(id);

            return NoContent();
        }

        [HttpPut]
        [Route("AddExercise/{courseId:length(24)}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> AddExercise(string courseId, [FromBody]string exerciseId)
        {
            var course = await _coursesServices.GetAsync(courseId);
            Exercise? exercise = await _exercisesServices.GetAsync(exerciseId);
            if (course is null || exercise is null)
            {
                return NotFound();
            }

            course.Exercises.Add(exercise);

            await _coursesServices.UpdateAsync(courseId, course);

            return new JsonResult(course);
        }
    }
}
