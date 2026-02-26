using PSZK_MarsRoverProject.Controllers;
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
        Image roverKep;
        private BitmapImage groundImage1;
        private BitmapImage groundImage2;
        private BitmapImage groundImage3;
        private BitmapImage groundImage4;
        private BitmapImage groundImage5;
        private BitmapImage groundImage6;
        private BitmapImage groundImage7;
        private BitmapImage obstacleImage;
        private BitmapImage gemimage;
        private bool FollowRover;
        const int tileMeret = 80;
        public MainWindow()
        {
            InitializeComponent();
            groundImage1 = new BitmapImage(new Uri("pack://application:,,,/Images/kep31.png"));
            groundImage2 = new BitmapImage(new Uri("pack://application:,,,/Images/kep32.png"));
            groundImage3 = new BitmapImage(new Uri("pack://application:,,,/Images/kep33.png"));
            groundImage4 = new BitmapImage(new Uri("pack://application:,,,/Images/kep34.png"));
            groundImage5 = new BitmapImage(new Uri("pack://application:,,,/Images/kep35.png"));
            groundImage6 = new BitmapImage(new Uri("pack://application:,,,/Images/kep36.png"));
            groundImage7 = new BitmapImage(new Uri("pack://application:,,,/Images/kep37.png"));
            obstacleImage = new BitmapImage(new Uri("pack://application:,,,/Images/obstacle2.png"));
            gemimage = new BitmapImage(new Uri("pack://application:,,,/Images/gem.png"));
            CsvBeolvaso();
            //terkep[32, 34] = "R";
            JatekterFeltoltes();
        }

        private void JatekterFeltoltes()
        {
            //tile rajzolás
            for (int i = 0; i < terkep.GetLength(0); i++)
            {
                for (int j = 0; j < terkep.GetLength(1); j++)
                {
                    Image talaj = new Image() 
                    { 
                        Width = tileMeret, 
                        Height = tileMeret,
                        SnapsToDevicePixels = true
                    };
                    string jel = terkep[i, j] == "R" ? "." : terkep[i, j];
                    talaj.Source = GetImageSource(jel);
                    Canvas.SetLeft(talaj, j * tileMeret);
                    Canvas.SetTop(talaj, i * tileMeret);
                    jatekter.Children.Add(talaj);
                }
            }
            //rover rajzolás
            roverKep = new Image()
            {
                Width = tileMeret,
                Height = tileMeret,
                Source = new BitmapImage(new Uri("pack://application:,,,/Images/rover.png")),
            };
            //A rover képe mindig a legfelső rétegben legyen
            Panel.SetZIndex(roverKep, 10);
            FrissitRoverPozicio();
            jatekter.Children.Add(roverKep);
        }

        /// <summary>
        /// A rover pozíciójának frissítése a játéktéren, valamint a pozíció szövegének frissítése
        /// </summary>
        private void FrissitRoverPozicio()
        {
            Canvas.SetLeft(roverKep, rover.Xposition * tileMeret);
            Canvas.SetTop(roverKep, rover.Yposition * tileMeret);
            txtPos.Text = $"X: {rover.Xposition}, Y: {rover.Yposition}";
            if (FollowRoverBox.IsChecked == true)
            {
                kamera.ScrollToVerticalOffset(rover.Yposition * tileMeret - (kamera.ActualHeight / 2));
                kamera.ScrollToHorizontalOffset(rover.Xposition * tileMeret - (kamera.ActualWidth / 2));
            }
        }

        /// <summary>
        /// A megadott karakter alapján visszaadja a megfelelő képet a játéktérhez
        /// </summary>
        /// <param name="karakter">A karakter, amelyhez a képet keresünk</param>
        /// <returns>A megfelelő ImageSource</returns>
        private ImageSource GetImageSource(string karakter)
        {
            switch (karakter)
            {
                case ".":
                    Random rnd = new Random();
                    int szam = rnd.Next(1, 8); // 1-től 7-ig
                    if (szam == 1) return groundImage1;
                    if (szam == 2) return groundImage2;
                    if (szam == 3) return groundImage3;
                    if (szam == 4) return groundImage4;
                    if (szam == 5) return groundImage5;
                    if (szam == 6) return groundImage6;
                    return groundImage7;
                case "#":
                    return obstacleImage;
                case "G":
                    return gemimage;
                case "Y":
                    return gemimage;
                case "B":
                    return gemimage;
                default:
                    return groundImage1;
            }
        }



        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            kamera.ScrollToVerticalOffset(rover.Yposition * tileMeret - (kamera.ActualHeight / 2));
            kamera.ScrollToHorizontalOffset(rover.Xposition * tileMeret - (kamera.ActualWidth / 2));
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.W:
                    kamera.ScrollToVerticalOffset(kamera.VerticalOffset - tileMeret);
                    break;
                case Key.S:
                    kamera.ScrollToVerticalOffset(kamera.VerticalOffset + tileMeret);
                    break;
                case Key.A:
                    kamera.ScrollToHorizontalOffset(kamera.HorizontalOffset - tileMeret);
                    break;
                case Key.D:
                    kamera.ScrollToHorizontalOffset(kamera.HorizontalOffset + tileMeret);
                    break;
                case Key.Space:
                    kamera.ScrollToVerticalOffset(rover.Yposition * tileMeret - (kamera.ActualHeight / 2));
                    kamera.ScrollToHorizontalOffset(rover.Xposition * tileMeret - (kamera.ActualWidth / 2));
                    break;
                case Key.R:
                    if (rover.Yposition > 0) rover.Yposition--;
                    break;
                case Key.T:
                    if (rover.Yposition < 49) rover.Yposition++;
                    break;
                case Key.Z:
                    if (rover.Xposition < 49) rover.Xposition--;
                    break;
                case Key.U:
                    if (rover.Xposition > 0) rover.Xposition++;
                    break;
                case Key.Q:
                    terkep[rover.Yposition, rover.Xposition] = ".";
                    jatekter.Children.Clear();
                    JatekterFeltoltes();
                    break;
                case Key.E:
                    int[] celKordinata = RoverAI.LegkozelebbiGemKereses(terkep,rover);
                    celPozicio.Text = $"X: {celKordinata[1]}, Y: {celKordinata[0]}";
                    break;
            }
            FrissitRoverPozicio();
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