
using First.API.Models;
using First.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace First.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GamesController : ControllerBase
    {
        private readonly IGamesServices _gameServices;
        private readonly ILogger<GamesController> _logger;

        public GamesController(ILogger<GamesController> logger, IGamesServices gameServices)
        {
           _logger = logger;
            _gameServices = (IGamesServices?)gameServices;
        }


        [HttpGet]
        public ActionResult<List<Games>> Get()
        {   
            _logger.LogCritical("Fetching all games");
            return _gameServices.Get();
        }

        [HttpGet("{id:length(24)}", Name = "GetGame")]
        public ActionResult<Games> Get(string id)
        {
            var game = _gameServices.Get(id);

            if (game == null)
            {
                return NotFound();
            }

            return game;
        }

        [HttpPost]
        public ActionResult<Games> Create(Games game)
        {
            _gameServices.Create(game);
            return CreatedAtRoute("GetGame", new { id = game.Id }, game);
        }

        [HttpPut("{id:length(24)}")]
        public IActionResult Update(string id, Games gameIn)
        {
            var game = _gameServices.Get(id);

            if (game == null)
            {
                return NotFound();
            }

            _gameServices.Update(id, gameIn);

            return NoContent();
        }

        [HttpDelete("{id:length(24)}")]
        public IActionResult Delete(string id)
        {
            var game = _gameServices.Get(id);

            if (game == null)
            {
                return NotFound();
            }

            _gameServices.Remove(id);

            return NoContent();
        }
    }
}