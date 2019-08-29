using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnakeProject
{
    /// <summary>
    /// Class of highscores in snake game
    /// </summary>
    public class SnakeHighscore
    {
        /// <summary>
        /// Name of the player with highscore
        /// </summary>
        public string PlayerName { get; set; }

        /// <summary>
        /// Player's new highscore
        /// </summary>
        public int Score { get; set; }
    }
}
