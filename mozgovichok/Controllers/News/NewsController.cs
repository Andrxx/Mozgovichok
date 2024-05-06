using Microsoft.AspNetCore.Mvc;
using mozgovichok.Models.News;
using mozgovichok.Models.Users;
using mozgovichok.Services.News;
using mozgovichok.Services.Users;

namespace mozgovichok.Controllers.News
{
    [ApiController]
    [Route("api/v1/News")]
    public class NewsController : ControllerBase
    {
        private readonly NewsService _newsService;

        public NewsController(NewsService newsService) =>
            _newsService = newsService;


        [HttpGet]
        //[Authorize]
        [Route("GetNews")]
        public async Task<List<NewsModel>> Get() =>
           await _newsService.GetAsync();

        [HttpGet]
        [Route("GetNews/{id:length(24)}")]
        public async Task<ActionResult<NewsModel>> Get(string id)
        {
            var newsModel = await _newsService.GetAsync(id);

            if (newsModel is null)
            {
                return NotFound();
            }

            return newsModel;
        }

        [HttpPost]
        [Route("Add")]
        public async Task<IActionResult> Post(NewsModel newModel)
        {

            await _newsService.CreateAsync(newModel);

            return CreatedAtAction(nameof(Get), new { id = newModel.Id }, newModel);
        }

        [HttpPut]
        [Route("Edit/{id:length(24)}")]
        public async Task<IActionResult> Update(string id, NewsModel updatedNews)
        {
            var user = await _newsService.GetAsync(id);

            if (user is null)
            {
                return NotFound();
            }

            updatedNews.Id = user.Id;

            await _newsService.UpdateAsync(id, updatedNews);

            return NoContent();
        }

        [HttpDelete]
        [Route("Delete/{id:length(24)}")]
        public async Task<IActionResult> Delete(string id)
        {
            var admin = await _newsService.GetAsync(id);

            if (admin is null)
            {
                return NotFound();
            }

            await _newsService.RemoveAsync(id);

            return NoContent();
        }
    }
}
