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
using System.Windows.Threading;

namespace PSZK_MarsRoverProject
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DispatcherTimer simTimer;
        string[,] map = new string[50, 50];
        Image[,] gemImg = new Image[50, 50];
        Rover rover = new Rover() { Xposition = 32, Yposition = 34, BatteryLevel = 100, IsCharging = true };
        Image roverImg;
        private BitmapImage groundImage1;
        private BitmapImage groundImage2;
        private BitmapImage groundImage3;
        private BitmapImage groundImage4;
        private BitmapImage groundImage5;
        private BitmapImage groundImage6;
        private BitmapImage groundImage7;
        private BitmapImage groundImage8;
        private BitmapImage groundImage9;
        private BitmapImage obstacleImage;
        private BitmapImage gemimage;
        private bool FollowRover;
        const int tileSize = 80;
        SimulationTime Time = new SimulationTime();
        public MainWindow()
        {
            InitializeComponent();
            Time.SetTime(8, 0);
            groundImage1 = new BitmapImage(new Uri("pack://application:,,,/Images/kep51.png"));
            groundImage2 = new BitmapImage(new Uri("pack://application:,,,/Images/kep52.png"));
            groundImage3 = new BitmapImage(new Uri("pack://application:,,,/Images/kep53.png"));
            groundImage4 = new BitmapImage(new Uri("pack://application:,,,/Images/kep54.png"));
            groundImage5 = new BitmapImage(new Uri("pack://application:,,,/Images/kep55.png"));
            groundImage6 = new BitmapImage(new Uri("pack://application:,,,/Images/kep56.png"));
            groundImage7 = new BitmapImage(new Uri("pack://application:,,,/Images/kep57.png"));
            groundImage8 = new BitmapImage(new Uri("pack://application:,,,/Images/kep58.png"));
            groundImage9 = new BitmapImage(new Uri("pack://application:,,,/Images/kep59.png"));
            obstacleImage = new BitmapImage(new Uri("pack://application:,,,/Images/obstacle2.png"));
            gemimage = new BitmapImage(new Uri("pack://application:,,,/Images/gem.png"));
            simTimer = new DispatcherTimer();
            simTimer.Interval = TimeSpan.FromSeconds(Time.TimeRate);
            simTimer.Tick += SimTimer_Tick;
            simTimer.Start();
            CsvReader();
            //terkep[32, 34] = "R";
            FillUpGameSpace();
        }

        private void SimTimer_Tick(object sender, EventArgs e)
        {
            Time.AddTime(); 
            ido.Text = $"Idő: {Time.GetCurrentTimeString()} ({Time.CurrentDayProgression})";
            //fogyasztás meg stb itt meghivhato
        }

        private void FillUpGameSpace()
        {
            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    //tile rajzolas
                    Image talaj = new Image()
                    {
                        Width = tileSize,
                        Height = tileSize,
                        Source = GetGroundImageSource(),
                        SnapsToDevicePixels = true
                    };
                    Canvas.SetLeft(talaj, j * tileSize);
                    Canvas.SetTop(talaj, i * tileSize);
                    Panel.SetZIndex(talaj, 0); //legalso reteg
                    jatekter.Children.Add(talaj);
                    string jel = map[i, j];
                    if (jel != "." && jel != "R")
                    {
                        Image targy = new Image()
                        {
                            Width = tileSize,
                            Height = tileSize,
                            Source = GetOtherImageSource(jel),
                            SnapsToDevicePixels = true
                        };
                        Canvas.SetLeft(targy, j * tileSize);
                        Canvas.SetTop(targy, i * tileSize);
                        Panel.SetZIndex(targy, 1); // A talaj felett legyen
                        jatekter.Children.Add(targy);
                        // Ha ez egy gem (G, Y, B), elmentjük a referenciáját
                        if (jel == "G" || jel == "Y" || jel == "B")
                        {
                            gemImg[i, j] = targy;
                        }
                    }
                }
            }
            //rover rajzolás
            roverImg = new Image()
            {
                Width = tileSize,
                Height = tileSize,
                Source = new BitmapImage(new Uri("pack://application:,,,/Images/kicsikocsi.png")),
            };
            //A rover képe mindig a legfelső rétegben legyen
            Panel.SetZIndex(roverImg, 10);
            RefreshRoverPosition();
            jatekter.Children.Add(roverImg);
        }

        /// <summary>
        /// A rover pozíciójának frissítése a játéktéren, valamint a pozíció szövegének frissítése
        /// </summary>
        private void RefreshRoverPosition()
        {
            Canvas.SetLeft(roverImg, rover.Xposition * tileSize);
            Canvas.SetTop(roverImg, rover.Yposition * tileSize);
            txtPos.Text = $"X: {rover.Xposition}, Y: {rover.Yposition}";
            if (FollowRoverBox.IsChecked == true)
            {
                kamera.ScrollToVerticalOffset(rover.Yposition * tileSize - (kamera.ActualHeight / 2));
                kamera.ScrollToHorizontalOffset(rover.Xposition * tileSize - (kamera.ActualWidth / 2));
            }
        }

        /// <summary>
        /// A megadott karakter alapján visszaadja a megfelelő képet a játéktérhez
        /// </summary>
        /// <param name="karakter">A karakter, amelyhez a képet keresünk</param>
        /// <returns>A megfelelő ImageSource</returns>
        private ImageSource GetOtherImageSource(string karakter)
        {
            switch (karakter)
            {
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

        /// <summary>
        /// Random módon választ egy talajképet a rendelkezésre álló 7 közül, hogy változatosabbá tegye a játéktér megjelenését
        /// </summary>
        /// <returns></returns>
        private ImageSource GetGroundImageSource()
        {
                Random rnd = new Random();
                int szam = rnd.Next(1, 10); // 1-től 7-ig
                if (szam == 1) return groundImage1;
                if (szam == 2) return groundImage2;
                if (szam == 3) return groundImage3;
                if (szam == 4) return groundImage4;
                if (szam == 5) return groundImage5;
                if (szam == 6) return groundImage6;
                if (szam == 7) return groundImage7;
                if (szam == 8) return groundImage8;
            return groundImage9;
            }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            kamera.ScrollToVerticalOffset(rover.Yposition * tileSize - (kamera.ActualHeight / 2));
            kamera.ScrollToHorizontalOffset(rover.Xposition * tileSize - (kamera.ActualWidth / 2));
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.W:
                    kamera.ScrollToVerticalOffset(kamera.VerticalOffset - tileSize);
                    break;
                case Key.S:
                    kamera.ScrollToVerticalOffset(kamera.VerticalOffset + tileSize);
                    break;
                case Key.A:
                    kamera.ScrollToHorizontalOffset(kamera.HorizontalOffset - tileSize);
                    break;
                case Key.D:
                    kamera.ScrollToHorizontalOffset(kamera.HorizontalOffset + tileSize);
                    break;
                case Key.Space:
                    kamera.ScrollToVerticalOffset(rover.Yposition * tileSize - (kamera.ActualHeight / 2));
                    kamera.ScrollToHorizontalOffset(rover.Xposition * tileSize - (kamera.ActualWidth / 2));
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
                    string aktualisCella = map[rover.Yposition, rover.Xposition];
                    if (aktualisCella == "G" || aktualisCella == "Y" || aktualisCella == "B")
                    {
                        map[rover.Yposition, rover.Xposition] = ".";
                        if (gemImg[rover.Yposition, rover.Xposition] != null)
                        {
                            jatekter.Children.Remove(gemImg[rover.Yposition, rover.Xposition]);
                            gemImg[rover.Yposition, rover.Xposition] = null;
                        }
                    }
                    break;
                case Key.F:
                    ido.Text = $"Idő: {Time.GetCurrentTimeString()}";
                    break;
                case Key.E:
                    int[] celKordinata = RoverAI.LegkozelebbiGemKereses(map,rover);
                    celPozicio.Text = $"X: {celKordinata[1]}, Y: {celKordinata[0]}";
                    break;
            }
            RefreshRoverPosition();
        }
        private void CsvReader()
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
                    map[i, j] = elemek[j];
                }
            }
        }

        private void BoostButton3_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BoostButton2_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BoostButton1_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}