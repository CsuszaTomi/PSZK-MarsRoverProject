using System.Globalization;
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
using PSZK_MarsRoverProject.Controllers;
using PSZK_MarsRoverProject.Models;

namespace PSZK_MarsRoverProject
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public DispatcherTimer simTimer;
        public string[,] map = new string[50, 50];
        public Image[,] gemImg = new Image[50, 50];
        Rover rover = new Rover() { Xposition = 32, Yposition = 34, BatteryLevel = 100, IsCharging = true };
        Log log = new Log();
        public Image roverImg;
        public BitmapImage groundImage1;
        public BitmapImage groundImage2;
        public BitmapImage groundImage3;
        //public BitmapImage groundImage4;
        public BitmapImage groundImage5;
        public BitmapImage groundImage6;
        public BitmapImage obstacleImage;
        public BitmapImage gemimage;
        public bool FollowRover;
        public const int tileSize = 80;
        public SimulationTime Time = new SimulationTime();
        List<int[]> activePath = null;
        bool gameOn = false;
        bool gameStarted = false;
        public MainWindow()
        {
            InitializeComponent();
            Time.SetTime(8, 0);

            groundImage1 = new BitmapImage(new Uri("pack://application:,,,/Images/ground1.png"));
            groundImage2 = new BitmapImage(new Uri("pack://application:,,,/Images/ground2.png"));
            groundImage3 = new BitmapImage(new Uri("pack://application:,,,/Images/ground3.png"));
            //groundImage4 = new BitmapImage(new Uri("pack://application:,,,/Images/ground4.png"));
            groundImage5 = new BitmapImage(new Uri("pack://application:,,,/Images/ground5.png"));
            groundImage6 = new BitmapImage(new Uri("pack://application:,,,/Images/ground6.png"));
            obstacleImage = new BitmapImage(new Uri("pack://application:,,,/Images/obstacle2.png"));
            gemimage = new BitmapImage(new Uri("pack://application:,,,/Images/bluegem.png"));
            simTimer = new DispatcherTimer();
            simTimer.Interval = TimeSpan.FromSeconds(Time.TimeRate);
            simTimer.Tick += SimTimer_Tick;
            map = MapController.CsvReader();
            //terkep[32, 34] = "R";
            MapController.FillUpGameSpace(this);
        }

        private void SimTimer_Tick(object sender, EventArgs e)
        {
            //idolepes
            Time.AddTime();
            if(Time.CurrentTime.Minute == 30 || Time.CurrentTime.Minute == 0)
            {
                //toltes
                rover.ChargeBattery(Time);
                //akciók végrehajtása
                if (gameOn)
                {
                    // Ha a Rover épp nem bányászik, és nincs is hova mennie (üres az út),
                    // akkor itt az ideje, hogy az AI új célt keressen!
                    if (!rover.IsMining && (activePath == null || activePath.Count == 0))
                    {
                        activePath = RoverAI.LegkozelebbiGemKereses(map, rover);

                        if (activePath != null && activePath.Count > 0)
                        {
                            var cel = activePath.Last();
                            celPozicio.Text = $"Cél -> X: {cel[1]}, Y: {cel[0]}";
                        }
                        else
                        {
                            celPozicio.Text = "Nincs több elérhető ásvány!";
                            gameOn = false; // Le is állíthatjuk a játékot, ha nincs több cél
                        }
                    }
                }
                if (rover.IsMining)
                {
                    // Bányászat végrehajtása
                    rover.Mine(Time);
                    rover.IsMining = false; // Befejezte a bányászatot (fél óra telt el)
                                            // A bányászat helyén a térkép újra üres lesz
                    map[rover.Yposition, rover.Xposition] = ".";
                    if (gemImg[rover.Yposition, rover.Xposition] != null)
                    {
                        jatekter.Children.Remove(gemImg[rover.Yposition, rover.Xposition]);
                        gemImg[rover.Yposition, rover.Xposition] = null;
                    }
                }
                else if (activePath != null && activePath.Count > 0)
                {
                    // Mozgás végrehajtása a meghatározott útvonalon
                    int desiredSpeed = GetOptimalSpeed(activePath.Count);
                    // Ha kevesebb lépés van hátra, mint a sebességünk, akkor csak annyit megyünk
                    rover.CurrentSpeed = Math.Min(desiredSpeed, activePath.Count);
                    // Fogyasztás levonása a sebesség alapján (E = 2 * v^2)
                    rover.MovementEnergyConsumption();
                    // Lépések megtétele a listában
                    for (int i = 0; i < rover.CurrentSpeed; i++)
                    {
                        int[] nextStep = activePath[0];
                        rover.Yposition = nextStep[0]; // Y a sor
                        rover.Xposition = nextStep[1]; // X az oszlop
                        activePath.RemoveAt(0);// Az első elemet eltávolítjuk, mert már odaértünk
                    }

                    //utvege
                    if (activePath.Count == 0)
                    {
                        rover.IsMining = true;
                    }
                }
                else
                {
                    //standby
                    rover.DrainBattery(1);
                    simTimer.Stop();
                }
            }

            egysegnyiuzemanyag.Text = $"{rover.AllBatteryUsage.ToString()} egység";
            UpdateChart();
            EnergyBar.Value = rover.BatteryLevel;
            ido.Text = $"Idő: {Time.GetCurrentTimeString()} ({Time.CurrentDayProgression})";
            RefreshRoverPosition();

            // Ellenőrizzük, hogy a rover lemerült-e
            if (rover.BatteryLevel <= 0)
            {
                rover.BatteryLevel = 0;
                simTimer.Stop();
                MessageBox.Show("A küldetés véget ért: A Rover lemerült!");
            }
        }


        private int GetOptimalSpeed(int hatralevoLepesek)
        {
            int speed = 1;

            if (Time.IsDay)
            {
                // NAPPAL (+10 töltés)
                if (rover.BatteryLevel >= 80)
                {
                    speed = 3; // Fogyasztás: 18, Töltés: 10 -> Nettó: -8
                }
                else if (rover.BatteryLevel >= 20)
                {
                    speed = 2; // Fogyasztás: 8, Töltés: 10 -> Nettó: +2
                }
                else
                {
                    speed = 1; // Fogyasztás: 2, Töltés: 10 -> Nettó: +8
                }
            }
            else
            {
                // ÉJSZAKA (0 töltés)
                if (rover.BatteryLevel >= 40)
                {
                    speed = 2; // Fogyasztás: 8 -> Nettó: -8
                }
                else
                {
                    speed = 1; // Fogyasztás: 2 -> Nettó: -2
                               // A 3-as sebességet éjjel tiltjuk, hogy ne haljon meg gyorsan
                }
            }
            log.DistanceTraveled += Math.Min(speed, hatralevoLepesek);

            return Math.Min(speed, hatralevoLepesek);
        }

        /// <summary>
        /// A rover pozíciójának frissítése a játéktéren, valamint a pozíció szövegének frissítése
        /// </summary>
        public void RefreshRoverPosition()
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            kamera.ScrollToVerticalOffset(rover.Yposition * tileSize - (kamera.ActualHeight / 2));
            kamera.ScrollToHorizontalOffset(rover.Xposition * tileSize - (kamera.ActualWidth / 2));
        }

        private string DrawPieSlice(double radius, double startAngle, double sweepAngle)
        {
            // A kör közepe a 140x140-es méret alapján
            double centerX = 70;
            double centerY = 70;

            if (sweepAngle >= 360) sweepAngle = 359.999;

            // Szögek átváltása radiánba a -90 azért kell, hogy felülről, 12 órától induljon a rajz
            double startRad = (startAngle - 90) * Math.PI / 180.0;
            double endRad = (startAngle + sweepAngle - 90) * Math.PI / 180.0;

            // Kezdő és végpontok kiszámítása a kör ívén
            double x1 = centerX + radius * Math.Cos(startRad);
            double y1 = centerY + radius * Math.Sin(startRad);

            double x2 = centerX + radius * Math.Cos(endRad);
            double y2 = centerY + radius * Math.Sin(endRad);

            // Ha 180 foknál nagyobb a szelet, a "nagy ívet" kell rajzolnia
            int isLargeArc = sweepAngle > 180 ? 1 : 0;

            // Itt rakjuk össze a XAML szöveget (a CultureInfo kell, hogy pontot használjon tizedesjelként)
            return $"M {centerX},{centerY} L {x1.ToString(CultureInfo.InvariantCulture)},{y1.ToString(CultureInfo.InvariantCulture)} " +
                   $"A {radius},{radius} 0 {isLargeArc},1 {x2.ToString(CultureInfo.InvariantCulture)},{y2.ToString(CultureInfo.InvariantCulture)} Z";
        }

        // 2. A frissítő metódus, amit meg tudsz hívni
        public void UpdateChart()
        {
            // 1. Összes energia kiszámítása
            double total = rover.Speed1BatteryUsage +
                           rover.Speed2BatteryUsage +
                           rover.Speed3BatteryUsage +
                           rover.MiningBatteryUsage +
                           rover.StandByBatteryUsage;

            // Ha még nem fogyasztott semmit, nem rajzolunk semmit
            if (total == 0) return;

            // 2. Kiszámoljuk, melyik hány fokot kap a 360-ból
            double a1 = (rover.Speed1BatteryUsage / total) * 360;
            double a2 = (rover.Speed2BatteryUsage / total) * 360;
            double a3 = (rover.Speed3BatteryUsage / total) * 360;
            double a4 = (rover.MiningBatteryUsage / total) * 360;
            double a5 = (rover.StandByBatteryUsage / total) * 360;

            double currentAngle = 0;

            // 3. Szeletek megrajzolása sorban egymás után
            SliceSpeed1.Data = Geometry.Parse(DrawPieSlice(70, currentAngle, a1));
            currentAngle += a1;

            SliceSpeed2.Data = Geometry.Parse(DrawPieSlice(70, currentAngle, a2));
            currentAngle += a2;

            SliceSpeed3.Data = Geometry.Parse(DrawPieSlice(70, currentAngle, a3));
            currentAngle += a3;

            SliceMining.Data = Geometry.Parse(DrawPieSlice(70, currentAngle, a4));
            currentAngle += a4;

            SliceStandby.Data = Geometry.Parse(DrawPieSlice(70, currentAngle, a5));
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
                    if (!gameStarted)
                    {
                        simTimer.Start();
                        gameStarted = true;
                    }
                    gameOn = !gameOn;
                    break;
            }
            RefreshRoverPosition();
        }


        private void BoostButton3_Click(object sender, RoutedEventArgs e)
        {
            Time.TimeRate = 0.0001;
            simTimer.Interval = TimeSpan.FromSeconds(Time.TimeRate);
        }

        private void BoostButton2_Click(object sender, RoutedEventArgs e)
        {
            Time.TimeRate = 0.01;
            simTimer.Interval = TimeSpan.FromSeconds(Time.TimeRate);
        }

        private void BoostButton1_Click(object sender, RoutedEventArgs e)
        {
            Time.TimeRate = 1;
            simTimer.Interval = TimeSpan.FromSeconds(Time.TimeRate);
        }
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Height >= 800)
            {
                StatisztikaExpander.IsExpanded = true;
            }
            else
            {
                StatisztikaExpander.IsExpanded = false;
            }
        }
    }
}