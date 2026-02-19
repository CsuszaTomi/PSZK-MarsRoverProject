using PSZK_MarsRoverProject.Models;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PSZK_MarsRoverProject
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string[,] terkep = new string[50, 50];
        Rover rover = new Rover() { Xposition = 32, Yposition = 34, BatteryLevel = 100, IsCharging = true };
        public MainWindow()
        {
            InitializeComponent();
            CsvBeolvaso();
            terkep[32, 34] = "R";
            JatekterFeltoltes();
        }

        private void JatekterFeltoltes()
        {
            for (int i = 0; i < terkep.GetLength(0); i++)
            {
                for (int j = 0; j < terkep.GetLength(1); j++)
                {
                    TextBlock textBlock = new TextBlock()
                    {
                        Height = 40,
                        Width = 40,
                        Text = terkep[i, j],
                        TextAlignment = TextAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Top,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Foreground = terkep[i, j] == "#" ? Brushes.Red : terkep[i, j] == "R" ? Brushes.Green : Brushes.White,
                        Margin = new Thickness(10 + j * 40, 10 + i * 40, 0, 0)
                    };
                    jatekter.Children.Add(textBlock);
                }
            }
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            double roverX = 34 * 40;
            double roverY = 32 * 40;
            double centerX = roverX - (kamera.ActualWidth / 2);
            double centerY = roverY - (kamera.ActualHeight / 2);
            kamera.ScrollToHorizontalOffset(centerX);
            kamera.ScrollToVerticalOffset(centerY);
        }
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.W:
                    kamera.ScrollToVerticalOffset(kamera.VerticalOffset - 40);
                    break;
                case Key.S:
                    kamera.ScrollToVerticalOffset(kamera.VerticalOffset + 40);
                    break;
                case Key.A:
                    kamera.ScrollToHorizontalOffset(kamera.HorizontalOffset - 40);
                    break;
                case Key.D:
                    kamera.ScrollToHorizontalOffset(kamera.HorizontalOffset + 40);
                    break;
                case Key.Space:
                    kamera.ScrollToVerticalOffset(rover.Yposition * 40 - (kamera.ActualHeight / 2));
                    kamera.ScrollToHorizontalOffset(rover.Xposition * 40 - (kamera.ActualWidth / 2));
                    break;
            }
        }
        private void CsvBeolvaso()
        {
            if (!File.Exists("mars_map_50x50.csv"))
            {
                MessageBox.Show("A térkép fájl nem található!");
                Environment.Exit(1);
            }
            string[] sorok = File.ReadAllLines("mars_map_50x50.csv");
            for (int i = 0; i < sorok.Length; i++)
            {
                string[] elemek = sorok[i].Split(',');
                for (int j = 0; j < elemek.Length && j < 50; j++)
                {
                    terkep[i, j] = elemek[j];
                }
            }
        }
    }
}