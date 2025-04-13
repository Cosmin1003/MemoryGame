using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Shapes;
using MemoryGame.Models;
using MemoryGame.ViewModels;

namespace MemoryGame.Views
{
    /// <summary>
    /// Interaction logic for GameChooser.xaml
    /// </summary>
    public partial class GameChooserWindow : Window
    {
        public User _user;
        public GameChooserWindow(User user)
        {
            InitializeComponent();
            DataContext = new GameChooserVM(user);
        }

        private void Help_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Nume: Bardas Cosmin Flavius\nGrupa: 10LF231", "About", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
