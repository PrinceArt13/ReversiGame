using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Game;

namespace ReversiGame
{
    class AppViewModel : INotifyPropertyChanged
    {
        public delegate void AccountHandler((int, int) iter, Ellipse disk);

        private (int, int) CanvasIndexes;
        private Ellipse LastAdd;
        public StrategyGame SG;
        private bool SinglePlayer;
        private Computer computer;
        private bool ComputerTurn;
        private readonly GameSetter gameSetter;

        public Canvas canvas;

        public AppViewModel()
        {
            Visibility = "Hidden";
            gameSetter = new GameSetter();
        }

        private SolidColorBrush playerbrush;
        public SolidColorBrush PlayerBrush
        {
            get
            {
                return this.playerbrush;
            }
            set
            {
                playerbrush = value;
                OnPropertyChanged();
            }
        }

        private string playername;
        public string PlayerName
        {
            get
            {
                return this.playername;
            }
            set
            {
                playername = value;
                OnPropertyChanged();
            }
        }

        private string score;
        public string Score
        {
            get
            {
                return this.score;
            }
            set
            {
                score = value;
                OnPropertyChanged();
            }
        }

        private string fieldsize;
        public string FieldSize
        {
            get
            {
                return this.fieldsize;
            }
            set
            {
                fieldsize = value;
                OnPropertyChanged();
            }
        }
        private string visibility;
        public string Visibility
        {
            get
            {
                return this.visibility;
            }
            set
            {
                visibility = value;
                OnPropertyChanged();
            }
        }

        public void CreateStrategyGame(int size, bool singleplayer)
        {
            SG = gameSetter.MakeGame(false, new NewGame(size));
            DisplayGameSet();
            this.SinglePlayer = singleplayer;
            ComputerTurn = false;
            if (singleplayer)
                computer = new Computer(SG.Player2);
            ShowActivePlayer();
        }

        private void ShowActivePlayer()
        {
            Player player = SG.ActivePlayer;
            PlayerName = player.Name;
            PlayerBrush = player.Brush;
            Score = player.Score.ToString();
        }

        private void DisplayGameSet()
        {
            int s = SG.PlacedDisks.GameField.GetLength(0);
            for (int i = 0; i < s; i++)
                for (int j = 0; j < s; j++)
                    if (SG.PlacedDisks.GameField[i, j] != null)
                        AddDisk((i, j), SG.PlacedDisks.GameField[i, j].ellipse);
        }

        private void DrawGameArea(Canvas GameArea, int size, int a = 20)
        {
            bool doneDrawingBackground = false;
            int nextX = 0, nextY = 0;
            int rowCounter = 0;
            int colCounter = 0;
            bool Cell = false;
            while (doneDrawingBackground == false)
            {
                Rectangle rect = new Rectangle
                {
                    Width = a,
                    Height = a,
                    Fill = Cell ? Brushes.Gray : Brushes.DarkGray
                };
                GameArea.Children.Add(rect);
                Canvas.SetTop(rect, nextY);
                Canvas.SetLeft(rect, nextX);
                Canvas.SetZIndex(rect, 0);
                Cell = !Cell;
                nextX += a;
                colCounter++;
                if (nextX > GameArea.ActualWidth || colCounter == size)
                {
                    nextX = 0;
                    nextY += a;
                    rowCounter++;
                    colCounter = 0;
                    Cell = rowCounter % 2 != 0;
                }
                if (nextY > GameArea.ActualHeight || rowCounter == size)
                    doneDrawingBackground = true;
            }
        }
        private void AddDisk((int, int) iter, Ellipse disk)
        {
            if (disk == null)
                MessageBox.Show("Диски можно размещать только рядом с другими дисками!");
            else
            {
                canvas.Children.Add(disk);
                Canvas.SetTop(disk, iter.Item1 * 20);
                Canvas.SetLeft(disk, iter.Item2 * 20);
                Canvas.SetZIndex(disk, 1);
            }
        }

        public void Process_Click(System.Windows.Point p)
        {
            if (!ComputerTurn)
            {
                int col = (int)Math.Floor(p.X / 20);
                int row = (int)Math.Floor(p.Y / 20);
                try
                {
                    SG.Add_Disk(row, col);
                    CanvasIndexes = (row, col);
                    LastAdd = SG.PlacedDisks.GameField[row, col].ellipse;
                    ShowActivePlayer();
                }
                catch
                {
                    CanvasIndexes = (-1, -1);
                    LastAdd = null;
                }
                AddDisk(CanvasIndexes, LastAdd);
                if (!SG.gameFinished)
                {
                    if (SinglePlayer && LastAdd != null)
                        MakeComputerTurn();
                }
                else EndGame();
            }
        }
        private async void MakeComputerTurn()
        {
            ComputerTurn = true;
            await Task.Delay(500);
            (CanvasIndexes, LastAdd) = computer.ComputerTurn(SG);
            if (LastAdd == null || SG.gameFinished)
                EndGame();
            else
            {
                ShowActivePlayer();
                AddDisk(CanvasIndexes, LastAdd);
            }
            ComputerTurn = false;
        }

        

        private void Start(bool SinglePlayer, string fieldsize)
        {
            try
            {
                int size = int.Parse(fieldsize);
                if ((size < 6 || size > 20))
                    MessageBox.Show("Поле может быть не меньше 6x6 и не больше 20x20!");
                else
                {
                    canvas.Children.Clear();
                    DrawGameArea(canvas, size);
                    CreateStrategyGame(int.Parse(fieldsize), SinglePlayer);
                    Visibility = "Visible";
                }
            }
            catch
            {
                MessageBox.Show("Введите числа!");
            }
            FieldSize = "";
        }

        private Command startGame;
        public Command StartGame
        {
            get
            {
                return startGame ?? (startGame = new Command(obj =>
                {
                    if (int.Parse(obj.ToString()) == 0)
                        SinglePlayer = true;
                    else SinglePlayer = false;
                    Start(SinglePlayer, FieldSize);
                    FieldSize = "";
                }));
            }
        }

        private void EndGame()
        {
            int Score1 = SG.Player1.Score;
            int Score2 = SG.Player2.Score;
            string name1 = SG.Player1.Name;
            string name2 = SG.Player2.Name;
            MessageBox.Show
                ($"Конец игры! \n{name1}: {Score1}" +
                $"\n {name2}: {Score2}" +
                $"\n Победитель - {SG.Winner().Name}!");
            canvas.Children.Clear();
            Visibility = "Hidden";
        }
        #region Сохранение, загрузка, горячие клавиши, правила игры
        private void Save()
        {
            int s = SG.PlacedDisks.GameField.GetLength(0);
            SaveAndLoad.SaveGame(s, SG.DisksToInt(), SG.P1Turn, SinglePlayer);
        }

        private Command saveGame;
        public Command SaveGame
        {
            get
            {
                return saveGame ?? (saveGame = new Command(obj =>
                {
                    Save();
                    MessageBox.Show("Игра успешно сохранена!");
                }));
            }
        }

        private Command saveAndQuit;
        public Command SaveAndQuit
        {
            get
            {
                return saveAndQuit ?? (saveAndQuit = new Command(obj =>
                {
                    Save();
                    Environment.Exit(0);
                }));
            }
        }

        private Command loadGame;
        public Command LoadGame
        {
            get
            {
                return loadGame ?? (loadGame = new Command(obj =>
                {
                    SaveAndLoad.GameData gd = SaveAndLoad.LoadGame();
                    if (gd == null)
                    {
                        MessageBox.Show("Сохранённых игр нет!");
                    }
                    else
                    {
                        canvas.Children.Clear();
                        int s = SG.PlacedDisks.GameField.GetLength(0);
                        DrawGameArea(canvas, s);
                        SG = gameSetter.MakeGame(gd.FirstPlayerNext, new LoadGame(gd.Size, gd.Field));
                        this.SinglePlayer = gd.IsSinglePlayer;
                        ComputerTurn = false;
                        if (SinglePlayer)
                            computer = new Computer(SG.Player2);
                        ShowActivePlayer();
                    }
                    if (SG != null)
                    {
                        Visibility = "Visible";
                        canvas.Children.Clear();
                        int s = SG.PlacedDisks.GameField.GetLength(0);
                        DrawGameArea(canvas, s);
                        DisplayGameSet();
                    }
                }));
            }
        }

        private Command startSolo;
        public Command StartSolo
        {
            get
            {
                return startSolo ?? (startSolo = new Command(obj =>
                {
                    Start(true, "8");
                }));
            }
        }

        private Command startDuo;
        public Command StartDuo
        {
            get
            {
                return startDuo ?? (startDuo = new Command(obj =>
                {
                    Start(false, "8");
                }));
            }
        }
        private Command gameRule;
        public Command GameRule
        {
            get
            {
                return gameRule ?? (gameRule = new Command(obj =>
                {
                    MessageBox.Show
                    ("Разрабатываемое приложение представляет собой программную" +
                    " реализацию логической игры “Реверси”. Игра идет на поле" +
                    " произвольного размера. Два игрока поочереди устанавливают" +
                    " фишки своего цвета на поле. Фишку можно ставить только на те" +
                    " клетки, рядом с которыми уже стоят фишки. Если между" +
                    " установленной фишкой и какой либо другой фишкой того же цвета" +
                    " находятся фишки другого цвета, все они меняют свой цвет.\n\n" +
                    "Побеждает игрок, у которого больше всего фишек на поле, иначе - ничья.\n");
                }));
            }
        }

        private Command hotKeys;
        public Command HotKeys
        {
            get
            {
                return hotKeys ?? (hotKeys = new Command(obj =>
                {
                    MessageBox.Show("Горячие клавиши:\n" +
                        "CTRL+L - загрузить игру\n" +
                        "CTRL+S - сохранить игру\n" +
                        "Q - Сохранить и выйти\n" +
                        "A - Запустить одиночную игру 8x8\n" +
                        "B - Запустить игру 8x8 на два игрока");
                }));
            }
        }
        #endregion
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
