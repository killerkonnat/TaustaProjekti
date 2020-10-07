using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaustaProjekti
{
    public interface IRepository
    {

        Task<Player> Get(Guid id);
        Task<Player[]> GetAll();
        Task<Player> Create(Player player);
        Task<Player> Modify(Guid id, ModifiedPlayer player);
        Task<Player> Delete(Guid id);
        Task<UpdateResult> AddFlagToPlayer(Guid id, string tagName);
        Task<Player[]> GetFlaggedPlayers();

        Task<Player[]> GetBestPlayers(int levelIndex);

        Task<Boolean> UpdateScores(Guid id, ScoreSubmission sub);

        Task<UpdateResult> ChangePlayerName(Guid id, string name);

        Task<Player> GetPlayerWithName(string name);

    }
}
