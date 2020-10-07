using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace TaustaProjekti.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Route("api/player")]
    [Route("api/players")]

    public class PlayersController : ControllerBase
    {
        private readonly ILogger<PlayersController> _log;
        private readonly IRepository _irepository;

        public PlayersController(ILogger<PlayersController> log, IRepository repository)
        {
            _log = log;
            _irepository = repository;
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<Player> Get(Guid id)
        {
            await _irepository.Get(id);
            return null;
        }
        [HttpGet]
        [Route("Getall")]
        public Task<Player[]> GetAll()
        {
            return _irepository.GetAll();
        }

        [HttpPost] //{"Name": "Matti"}
        [Route("Create")]

        public async Task<Player> Create([FromBody] NewPlayer pl)
        {
            Player newPl = new Player() { Id = Guid.NewGuid(), Name = pl.Name };

            await _irepository.Create(newPl);
            return null;
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<Player> Delete([FromBody] Guid id)
        {
            await _irepository.Delete(id);
            return null;
        }

        [HttpGet] //"M"
        [Route("name:string")]
        public async Task<Player> GetPlayerWithName(string name)
        {
            await _irepository.GetPlayerWithName(name);
            return null;
        }


        [HttpGet] //"M"
        [Route("GetFlaggedPlayers")]
        public async Task<Player> GetFlaggedPlayers()
        {
            await _irepository.GetFlaggedPlayers();
            return null;
        }

        [HttpGet] //"M"
        [Route("UpdatePlayerName/new_name:string")]
        public async Task<Player> ChangePlayerName(Guid id, string name)
        {
            await _irepository.ChangePlayerName(id, name);
            return null;
        }

        [HttpGet] //"M"
        [Route("GetBestPlayers")]
        public async Task<Player[]> GetBestPlayers(int levelIndex)
        {
            await _irepository.GetBestPlayers(levelIndex);
            return null;
        }

        [HttpPost]
        [Route("AddFlagToPlayer/flag:string")]
        public async Task<UpdateResult> AddFlagToPlayer(Guid id, string flag)
        {
            await _irepository.AddFlagToPlayer(id, flag);
            return null;

        }
        [HttpPost]
        [Route("UpdateScores")]
        public async Task<Boolean> UpdateScores(Guid id, ScoreSubmission sub)
        {
            bool result = await _irepository.UpdateScores(id, sub);
            return result;
        }
    }
}
