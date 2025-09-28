using First.API.Models;
using First.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace First.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    //[Authorize(Roles = "Admin")] // Only Admin can access all by default
    public class GamesController : ControllerBase
    {
        private readonly IGamesServices _gameServices;
        private readonly ILogger<GamesController> _logger;

        public GamesController(ILogger<GamesController> logger, IGamesServices gameServices)
        {
            _logger = logger;
            _gameServices = gameServices ?? throw new ArgumentNullException(nameof(gameServices));
        }

        [HttpGet]
        [Authorize(Roles = "Admin, Readonly, Moderator")] // Both Admin and Readonly can access GET all
        public ActionResult<List<Games>> Get()
        {
            _logger.LogInformation("Fetching all games from the database.");
            var games = _gameServices.Get();

            if (games == null || !games.Any())
            {
                _logger.LogWarning("No games found in the database.");
                return NotFound("No games available.");
            }

            _logger.LogInformation("Successfully retrieved {Count} games.", games.Count);
            return games;
        }

        [HttpGet("{id:length(24)}", Name = "GetGame")]
        [Authorize(Roles = "Admin, Readonly, Moderator")] // Both Admin and Readonly can access GET by id
        public ActionResult<Games> Get(string id)
        {
            _logger.LogInformation("Fetching game with ID: {GameId}", id);

            var game = _gameServices.Get(id);

            if (game == null)
            {
                _logger.LogWarning("Game with ID {GameId} not found.", id);
                return NotFound();
            }

            _logger.LogInformation("Game with ID {GameId} retrieved successfully.", id);
            return game;
        }

        [HttpPost]
        // Only Admin (inherited from class-level [Authorize])
        [Authorize(Roles = "Admin, Moderator")] 

        public ActionResult<Games> Create(Games game)
        {
            _logger.LogInformation("Creating a new game: {GameName}", game.Name);

            _gameServices.Create(game);

            _logger.LogInformation("Game created successfully with ID: {GameId}", game.Id);
            return CreatedAtRoute("GetGame", new { id = game.Id }, game);
        }

        [HttpPut("{id:length(24)}")]
        // Only Admin (inherited from class-level [Authorize])
        [Authorize(Roles = "Admin")]
        public IActionResult Update(string id, Games gameIn)
        {
            _logger.LogInformation("Updating game with ID: {GameId}", id);

            var game = _gameServices.Get(id);

            if (game == null)
            {
                _logger.LogWarning("Cannot update. Game with ID {GameId} not found.", id);
                return NotFound();
            }

            _gameServices.Update(id, gameIn);

            _logger.LogInformation("Game with ID {GameId} updated successfully.", id);
            return NoContent();
        }

        [HttpDelete("{id:length(24)}")]
        // Only Admin (inherited from class-level [Authorize])
        [Authorize(Roles = "Admin")]

        public IActionResult Delete(string id)
        {
            _logger.LogInformation("Attempting to delete game with ID: {GameId}", id);

            var game = _gameServices.Get(id);

            if (game == null)
            {
                _logger.LogWarning("Cannot delete. Game with ID {GameId} not found.", id);
                return NotFound();
            }

            _gameServices.Remove(id);

            _logger.LogInformation("Game with ID {GameId} deleted successfully.", id);
            return NoContent();
        }
    }
}
