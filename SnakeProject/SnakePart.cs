using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SnakeProject
{
    /// <summary>
    /// Class used to define snake part in game
    /// </summary>
    public class SnakePart
    {
        /// <summary>
        /// Rectangles for snake part
        /// </summary>
        public UIElement UiElement { get; set; }

        /// <summary>
        /// Position of snake
        /// </summary>
        public Point Position { get; set; } 

        /// <summary>
        /// Defines which recatngle is the head of snake 
        /// </summary>
        public bool IsHead { get; set; }
    }
}
