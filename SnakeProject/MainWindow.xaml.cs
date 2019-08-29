using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml.Serialization;

namespace SnakeProject
{
    /// <summary>
    /// Main Window for snake game
    /// </summary>
    public partial class MainWindow : Window
    {
        //variable used to score the game
        private int currentScore = 0;

        //Snake food variables
        private UIElement snakeFood = null;
        private SolidColorBrush foodBrush = Brushes.Red;

        //Random variable for random location food generation
        private Random rnd = new Random();

        //Start game constants
        const int SnakeStartLength = 2;
        const int SnakeStartSpeed = 400;
        const int SnakeSpeedThreshold = 100;
        const int SnakeSquareSize = 25; // keeps snake objects, squares, at a fixed size 

        //variables used to draw snake
        private SolidColorBrush snakeBodyBrush = Brushes.Green; //Snake body is green
        private SolidColorBrush snakeHeadBrush = Brushes.YellowGreen; //Snake head is yellow green
        private List<SnakePart> snakeParts = new List<SnakePart>(); //list used used to keep reference to all snake parts

        //varibales used to move snake
        public enum SnakeDirection { Left, Right, Up, Down }; //holds snake direction options
        private SnakeDirection snakeDirection = SnakeDirection.Right; //holds current snake direction
        private int snakeLength; //holds current length of snake

        //Timer mechanism used to execute code in variables
        DispatcherTimer gameTicker = new DispatcherTimer();

        //Constant represent maximum number highscore entries can be on the list
        const int MaxHighscoreListEntryCount = 10;

        const string HighscoreFile = "C:\\C# Project\\SnakeGame\\snake_highscorelist.xml";

        public MainWindow()
        {
            InitializeComponent();
            gameTicker.Tick += gameTickTimer_Tick;
            LoadHighscoreList(); //calls the load highscore method on start up
        }

        //Action from gameTicker object is carried out in intervals to move snake
        private void gameTickTimer_Tick(object sender, EventArgs e)
        {
            MoveSnake();
        }


        private void Window_ContentRendered(object sender, EventArgs e)
        {
            //Calls this method after the controls in the window are initialised/rendered.
            DrawGameArea();
        }

        //Method call when a new game is started
        private void StartNewGame()
        {
            //All boards to be hidden at the start of a new game.
            bdrWelcomeMessage.Visibility = Visibility.Collapsed;
            bdrHighscoreList.Visibility = Visibility.Collapsed;
            bdrEndOfGame.Visibility = Visibility.Collapsed;

            // Remove potential dead snake parts and leftover food...
            foreach (SnakePart snakeBodyPart in snakeParts)
            {
                if (snakeBodyPart.UiElement != null)
                    GameArea.Children.Remove(snakeBodyPart.UiElement);
            }
            snakeParts.Clear();
            if (snakeFood != null)
                GameArea.Children.Remove(snakeFood);

            //Reset everything when new game is started
            currentScore = 0;
            snakeLength = SnakeStartLength;
            snakeDirection = SnakeDirection.Right;
            snakeParts.Add(new SnakePart() { Position = new Point(SnakeSquareSize * 5, SnakeSquareSize * 5) });
            gameTicker.Interval = TimeSpan.FromMilliseconds(SnakeStartSpeed);



            // Draw the snake  and generate food
            DrawSnake();
            DrawSnakeFood();

            //Update status at start
            UpdateGameStatus();

            // Start the game          
            gameTicker.IsEnabled = true;
        }

        private void DrawGameArea()
        {
            bool doneDrawingBackground = false; //defines when background is finished drawn.
            int nextX = 0, nextY = 0; 
            int rowCounter = 0;
            bool nextIsOdd = false;

            while (doneDrawingBackground == false)
            {
                Rectangle rect = new Rectangle // rectangle shape used to fill canvas
                {
                    Width = SnakeSquareSize, // Width and height are set to match the defined fixed square size 
                    Height = SnakeSquareSize,
                    Fill = nextIsOdd ? Brushes.White : Brushes.Black // if the next square to be filled is an odd square, 
                                                                    //fills the square with white, otherwise fill is in black.
                };
                GameArea.Children.Add(rect);
                Canvas.SetTop(rect, nextY);
                Canvas.SetLeft(rect, nextX);

                nextIsOdd = !nextIsOdd; 
                nextX += SnakeSquareSize;
                if (nextX >= GameArea.ActualWidth) //if reactangles spawn to the end of the row then goes to new line 
                {
                    nextX = 0;
                    nextY += SnakeSquareSize;
                    rowCounter++;
                    nextIsOdd = (rowCounter % 2 != 0);
                }

                if (nextY >= GameArea.ActualHeight)
                    doneDrawingBackground = true;
            }
        }

        private void DrawSnake()
        {
            foreach (SnakePart snakePart in snakeParts)
            {
                if (snakePart.UiElement == null)
                {
                    snakePart.UiElement = new Rectangle()
                    {
                        Width = SnakeSquareSize,
                        Height = SnakeSquareSize,
                        Fill = (snakePart.IsHead ? snakeHeadBrush : snakeBodyBrush)
                    };
                    GameArea.Children.Add(snakePart.UiElement);
                    Canvas.SetTop(snakePart.UiElement, snakePart.Position.Y);
                    Canvas.SetLeft(snakePart.UiElement, snakePart.Position.X);
                }
            }
        }

        private void MoveSnake()
        {
            // Removes the last part of the snake, in preparation of the new part added below  
            while (snakeParts.Count >= snakeLength)
            {
                GameArea.Children.Remove(snakeParts[0].UiElement);
                snakeParts.RemoveAt(0);
            }
            // add a new element to the snake, which will be the (new) head  
            // then mark all existing parts as non-head (body) elements and then  
            // make sure that they use the body brush  
            foreach (SnakePart snakePart in snakeParts)
            {
                (snakePart.UiElement as Rectangle).Fill = snakeBodyBrush;
                snakePart.IsHead = false;
            }

            // Determines in which direction to expand the snake, based on the current direction  
            SnakePart snakeHead = snakeParts[snakeParts.Count - 1];
            double nextX = snakeHead.Position.X;
            double nextY = snakeHead.Position.Y;
            switch (snakeDirection)
            {
                case SnakeDirection.Left:
                    nextX -= SnakeSquareSize;
                    break;
                case SnakeDirection.Right:
                    nextX += SnakeSquareSize;
                    break;
                case SnakeDirection.Up:
                    nextY -= SnakeSquareSize;
                    break;
                case SnakeDirection.Down:
                    nextY += SnakeSquareSize;
                    break;
            }

            // Adds the new head part to our list of snake parts 
            snakeParts.Add(new SnakePart()
            {
                Position = new Point(nextX, nextY),
                IsHead = true
            });
            //snake is then drawn again
            DrawSnake();
            // Collision check method is then called
            DoCollisionCheck();          
        }

        private Point GetNextFoodPosition() //Used to generate a random position for the food
        {
            //ensures that random location generation does not exceed the size of the game area
            int maxX = (int)(GameArea.ActualWidth / SnakeSquareSize);
            int maxY = (int)(GameArea.ActualHeight / SnakeSquareSize);
            int foodX = rnd.Next(0, maxX) * SnakeSquareSize;
            int foodY = rnd.Next(0, maxY) * SnakeSquareSize;

            foreach (SnakePart snakePart in snakeParts)
            {
                if ((snakePart.Position.X == foodX) && (snakePart.Position.Y == foodY))
                    //recursive function calls itself again if food location clashes with snake location
                    return GetNextFoodPosition(); 
            }
            //returns new food location once food location can be used
            return new Point(foodX, foodY);
        }

        //Draws the snake food using the random position generated from the GetNextFoodPosition method
        private void DrawSnakeFood() 
        {
            Point foodPosition = GetNextFoodPosition();
            snakeFood = new Ellipse() //red dot used as food
            {
                Width = SnakeSquareSize,
                Height = SnakeSquareSize,
                Fill = foodBrush
            };
            GameArea.Children.Add(snakeFood);
            Canvas.SetTop(snakeFood, foodPosition.Y);
            Canvas.SetLeft(snakeFood, foodPosition.X);
        }

        //Checks for user input to change snake direction
        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            SnakeDirection originalSnakeDirection = snakeDirection;
            switch (e.Key)
            {
                //Checks first that current snake direction allows user to change snake direction to the new direction
                case Key.Up:
                    if (snakeDirection != SnakeDirection.Down)
                        snakeDirection = SnakeDirection.Up;
                    break;
                case Key.Down:
                    if (snakeDirection != SnakeDirection.Up)
                        snakeDirection = SnakeDirection.Down;
                    break;
                case Key.Left:
                    if (snakeDirection != SnakeDirection.Right)
                        snakeDirection = SnakeDirection.Left;
                    break;
                case Key.Right:
                    if (snakeDirection != SnakeDirection.Left)
                        snakeDirection = SnakeDirection.Right;
                    break;
                case Key.Space: // Spacebar key used to start a new game
                    StartNewGame();
                    break;
            }
            if (snakeDirection != originalSnakeDirection)
                MoveSnake(); //Moves snake as long as its going in a new direction 
        }

        //Method checks whether snake has collided with something in game and calls the appropriate method 
        //after determining what it collided with
        private void DoCollisionCheck()
        {
            //Snake head is set again at the end of the list
            SnakePart snakeHead = snakeParts[snakeParts.Count - 1]; 

            //Checks if snake head's new position is the same as the position of the food 
            //which then calls the eatsnakefood method
            if ((snakeHead.Position.X == Canvas.GetLeft(snakeFood)) && (snakeHead.Position.Y == Canvas.GetTop(snakeFood)))
            {
                EatSnakeFood();
                return;
            }

            //Checks if snake head's new position goes outside the game area's borders 
            //which then calls the endgame method
            if ((snakeHead.Position.Y < 0) || (snakeHead.Position.Y >= GameArea.ActualHeight) ||
            (snakeHead.Position.X < 0) || (snakeHead.Position.X >= GameArea.ActualWidth))
            {
                EndGame();
            }

            //Checks id the snake head's new position is the same as another snake part's position 
            //which then calls the engame method
            foreach (SnakePart snakeBodyPart in snakeParts.Take(snakeParts.Count - 1))
            {
                if ((snakeHead.Position.X == snakeBodyPart.Position.X) && (snakeHead.Position.Y == snakeBodyPart.Position.Y))
                    EndGame();
            }
        }

        //Method carries out appropriate actions when player eats snake food
        private void EatSnakeFood()
        {
            snakeLength++; //snake length and score increases 
            currentScore++;

            //Improved score decreases the time interval between ticks making the snake faster
            int timerInterval = Math.Max(SnakeSpeedThreshold, (int)gameTicker.Interval.TotalMilliseconds - (currentScore * 2));
            gameTicker.Interval = TimeSpan.FromMilliseconds(timerInterval);

            //Removes consumed snake food and generates a new one in another random location
            GameArea.Children.Remove(snakeFood);
            DrawSnakeFood();

            //update game status is then called
            UpdateGameStatus();
        }

        //update game status method then changes score and game speed shown in the title of the window
        private void UpdateGameStatus()
        {
            this.tbStatusScore.Text = currentScore.ToString();
            this.tbStatusSpeed.Text = gameTicker.Interval.TotalMilliseconds.ToString();
        }

        //Method executed when user loses and game must end
        private void EndGame()
        {
            bool isNewHighscore = false;
            if (currentScore > 0)
            {
                //checks for the lowest highscore and stores it in the lowestHighscore variable to be compared
                //with user's score at the end of the current game
                int lowestHighscore = (this.HighscoreList.Count > 0 ? this.HighscoreList.Min(x => x.Score) : 0);
                if ((currentScore > lowestHighscore) || (this.HighscoreList.Count < MaxHighscoreListEntryCount))
                {
                    //brings up the new highscore board if user made a new highscore
                    bdrNewHighscore.Visibility = Visibility.Visible;
                    txtPlayerName.Focus();
                    isNewHighscore = true;
                }
            }
            //If no new highscore is set, user is simply shown the game over screen with their score
            if (!isNewHighscore)
            {
                tbFinalScore.Text = currentScore.ToString();
                bdrEndOfGame.Visibility = Visibility.Visible;
            }

            //gameTicker is disabled to deactivate the game mechanics
            gameTicker.IsEnabled = false;
        }

        //Action registired when user clicks on window
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //Alows user to drag and move window when clicking the window
            this.DragMove();
        }

        //New custom close button for the window
        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        //Button hides the welcome message board and shows the highscore list
        private void BtnShowHighscoreList_Click(object sender, RoutedEventArgs e)
        {
            bdrWelcomeMessage.Visibility = Visibility.Collapsed; 
            bdrHighscoreList.Visibility = Visibility.Visible;
        }

        //Button adds user's name input to the observable collection 
        private void BtnAddToHighscoreList_Click(object sender, RoutedEventArgs e)
        {
            int newIndex = 0;
            // Looks fo where the new entry to the highscore list should be made
            if ((this.HighscoreList.Count > 0) && (currentScore < this.HighscoreList.Max(x => x.Score)))
            {
                SnakeHighscore justAbove = this.HighscoreList.OrderByDescending(x => x.Score).First(x => x.Score >= currentScore);
                if (justAbove != null)
                    newIndex = this.HighscoreList.IndexOf(justAbove) + 1;
            }
            // Create & insert the new entry
            this.HighscoreList.Insert(newIndex, new SnakeHighscore()
            {
                PlayerName = txtPlayerName.Text,
                Score = currentScore
            });
            // Make sure that the amount of entries does not exceed the maximum
            while (this.HighscoreList.Count > MaxHighscoreListEntryCount)
                this.HighscoreList.RemoveAt(MaxHighscoreListEntryCount);

            //Calls the save highscore list method after creating the new entry
            SaveHighscoreList();

            //Once new highscore entry is added new highscore board is hidden and highscore board list is shown
            bdrNewHighscore.Visibility = Visibility.Collapsed;
            bdrHighscoreList.Visibility = Visibility.Visible;
        }

        //Collection of smake highscore objects
        public ObservableCollection<SnakeHighscore> HighscoreList
        {
            get; set;
        } = new ObservableCollection<SnakeHighscore>();

        //method used to load an xml file with the list of highscores
        private void LoadHighscoreList()
        {
            if (File.Exists(HighscoreFile))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(List<SnakeHighscore>));
                using (Stream reader = new FileStream(HighscoreFile, FileMode.Open))
                {
                    List<SnakeHighscore> tempList = (List<SnakeHighscore>)serializer.Deserialize(reader);
                    this.HighscoreList.Clear();
                    foreach (var item in tempList.OrderByDescending(x => x.Score))
                        this.HighscoreList.Add(item);
                }
            }
        }

        //Method writes highscore list to an xml file to save it
        private void SaveHighscoreList()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<SnakeHighscore>));
            using (Stream writer = new FileStream(HighscoreFile, FileMode.Create))
            {
                serializer.Serialize(writer, this.HighscoreList);
            }
        }
    }
    }

