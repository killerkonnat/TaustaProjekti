using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaustaProjekti
{
    public class Player
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        public bool IsBanned { get; set; }
        public DateTime CreationTime { get; set; }


        public string[] reviewFlags = { };
        public int[] levelScores { get; set; }

    }
}
