using System;
using System.Collections.Generic;
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

namespace ReversiGame
{
    public partial class GamePage : Page
    {
        public GamePage()
        {
            InitializeComponent();
            ((AppViewModel)DataContext).canvas = GameArea;
            //void MouseClick(object sender, MouseButtonEventArgs e)
            //{
            //    Point p = Mouse.GetPosition(GameArea);
            //    ((AppViewModel)DataContext).Process_Click(p);
            //}
        }
    }
}
