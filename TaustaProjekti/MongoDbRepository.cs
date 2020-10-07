using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TaustaProjekti
{
    public class MongoDbRepository :IRepository
    {

        private readonly IMongoCollection<Player> _playerCollection;
        private readonly IMongoCollection<BsonDocument> _bsonDocumentCollection;

        public MongoDbRepository()
        {
            var mongoClient = new MongoClient("mongodb://localhost:27017");
            var database = mongoClient.GetDatabase("game");
            _playerCollection = database.GetCollection<Player>("players");

            _bsonDocumentCollection = database.GetCollection<BsonDocument>("players");
        }

        public async Task<Player> Create(Player player)
        {
            await _playerCollection.InsertOneAsync(player);
            return player;
        }

        public async Task<Player> Delete(Guid id)
        {
            FilterDefinition<Player> filter = Builders<Player>.Filter.Eq(p => p.Id, id);
            return await _playerCollection.FindOneAndDeleteAsync(filter);
        }

        public async Task<Player> Get(Guid id)
        {
            var filter = Builders<Player>.Filter.Eq(player => player.Id, id);
            return await _playerCollection.Find(filter).FirstAsync();
        }

        public async Task<Player[]> GetAll()
        {
            var players = await _playerCollection.Find(new BsonDocument()).ToListAsync();
            return players.ToArray();
        }

        public async Task<Player> Modify(Guid id, ModifiedPlayer player)
        {
            FilterDefinition<Player> filter = Builders<Player>.Filter.Eq(p => p.Id, id);
            Player returnPlayer = await _playerCollection.Find(filter).FirstAsync();
            returnPlayer.levelScores = player.levelScores;
            await _playerCollection.ReplaceOneAsync(filter, returnPlayer);
            return returnPlayer;
        }

        public async Task<Player> GetPlayerWithName(string name)
        {
            var filter = Builders<Player>.Filter.Eq("Name", name);
            return await _playerCollection.Find(filter).FirstAsync();
        }

        public async Task<UpdateResult> ChangePlayerName(Guid id, string name)
        {
            var filter = Builders<Player>.Filter.Eq("Id", id);
            var update = Builders<Player>.Update.Set("Name", name);
            return await _playerCollection.UpdateOneAsync(filter, update);
        }

        public async Task<Boolean> UpdateScores(Guid id, ScoreSubmission sub)
        {
            //Muuttaa scores-arrayn stringiksi, hashaa stringin ja tarkistaa if (hash == checksum)
            //yrittää vaikeuttaa huijareiden pääsemistä pistelistoille. Peli tallentaa pisteet kahteen eri muuttujaan, joista
            //toinen on hashatty versio. Jos huijaava pelaaja muuttaa muistissa olevia piste-arvoja, tai webapille lähetettyä
            //pistearvoa pakettieditorilla ilman että muuttaa myös checksumin sitä vastaavaan arvoon, niin listalle lähetetyt pisteet hylätään

 

            var filter = Builders<Player>.Filter.Eq(player => player.Id, id);
            Player player = await _playerCollection.Find(filter).FirstAsync();
            if (player.IsBanned) //älä anna bannittyjen pelaajien tallentaa arvoja
            {
                return false;
            }

            string hash;
            string scoresString = JsonConvert.SerializeObject(sub.scores);
            hash = hashFunction(scoresString);


            if (hash == sub.checksum)
            {
                int[] scores = sub.scores;
                ModifiedPlayer modP = new ModifiedPlayer();
               
                modP.levelScores = scores;

                //Tarkista onko pelaajan lähettämissä arvoissa epäilyttävän pieniä aikoja läpäistä jokin taso,
                //(alle 25 sekuntia) ei tarkoita varmaa huijaria. Hyväksy arvot, mutta merkitse pelaaja lipulla myöhempää manuaalista tarkistusta varten
                //Tästä lipusta tulee olemaan enemmän hyötyä, kun "replay"-tiedostojen lähettäminen on implementoitu.
                //TODO: Myöhemmin aseta jokaiselle tasolla erillinen "epäilyttävä" aikaraja riippuen parhaiden pelaajien lähettämistä arvoista. Manuaalinen/automaattinen?
                for(int i = 0; i< modP.levelScores.Length; i++)
                {
                    if (modP.levelScores[i] < 25000)
                    {
                        if (modP.levelScores[i] < 1000)
                        {
                            //Jos jonkin tason aika on alle sekunti, hylkää kaikki arvot
                            await AddFlagToPlayer(id, "zeroTimer");
                            return false;
                        }
                        await AddFlagToPlayer(id, "lowScore");
                        break;
                    }
                }

                await Modify(id, modP);
                return true;
            }
            else
            {
                //Lisää pelaajalle lippu epäilyttävästä toiminnasta, virheellinen checksum
                await AddFlagToPlayer(id, "invalidChecksum");
                return false;
            }
        }

        private string hashFunction(string theString)
        {
            string hash;
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                hash = BitConverter.ToString(
                  md5.ComputeHash(Encoding.UTF8.GetBytes(theString))
                ).Replace("-", String.Empty);
            }
            return hash;
        }

        public async Task<Player[]> GetBestPlayers(int levelIndex)
        {

            var filter = Builders<Player>.Filter.Empty;
            var searchString = "levelScores." + levelIndex;
            SortDefinition<Player> sortDef = Builders<Player>.Sort.Ascending(searchString);
            List<Player> players = await _playerCollection.Find(filter).Sort(sortDef).Limit(20).ToListAsync();
            return players.ToArray();
        }

        public async Task<Player[]> GetFlaggedPlayers()
        {
            var builder = Builders<Player>.Filter;

            var filter = builder.Exists("reviewFlags") & builder.SizeGt("reviewFlags", 0);
            List<Player> players = await _playerCollection.Find(filter).ToListAsync();
            return players.ToArray();

        }

        public async Task<UpdateResult> AddFlagToPlayer(Guid id, string tagName)
        {
            var filter = Builders<Player>.Filter.Eq("Id", id);
            var update = Builders<Player>.Update.Push("reviewFlags", tagName);
            return await _playerCollection.UpdateOneAsync(filter, update);
        }
    }
}
