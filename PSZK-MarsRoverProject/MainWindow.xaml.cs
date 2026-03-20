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
        Rover rover = new Rover();
        Log log = new Log();
        public Image roverImg;
        public BitmapImage groundImage1;
        public BitmapImage groundImage2;
        public BitmapImage groundImage3;
        //public BitmapImage groundImage4;
        public BitmapImage groundImage5;
        public BitmapImage groundImage6;
        public BitmapImage obstacleImage;
        public BitmapImage obstacleImage2;
        public BitmapImage obstacleImage3;
        public BitmapImage gemimage;
        public BitmapImage gemimage1;
        public BitmapImage gemimage2;
        public bool FollowRover;
        public const int tileSize = 80;
        public SimulationTime Time = new SimulationTime();
        List<int[]> activePath = null;
        int pathIndex = 0; 
        bool gameOn = false;
        bool BackToSpawn = false;
        bool gameStarted = false;
        int[] sPosition = new int[2];
        public int missionLength = 48;
        public int maxMinutes = 2880;
        double visualX;
        double visualY;
        double stepX = 0;
        double stepY = 0;
        double lastVisualX = -1;
        double lastVisualY = -1;
        bool isCalculatingPath = false;
        public MainWindow()
        {
            InitializeComponent();
            Time.SetTime(8, 0);
            map = MapController.CsvReader();
            sPosition = MapController.GetSLocation(map);
            rover = new Rover() { Xposition = sPosition[1], Yposition = sPosition[0], BatteryLevel = 100, IsCharging = true, Direction = "Down" };
            groundImage1 = new BitmapImage(new Uri("pack://application:,,,/Images/ground1.png"));
            groundImage2 = new BitmapImage(new Uri("pack://application:,,,/Images/ground2.png"));
            groundImage3 = new BitmapImage(new Uri("pack://application:,,,/Images/ground3.png"));
            //groundImage4 = new BitmapImage(new Uri("pack://application:,,,/Images/ground4.png"));
            groundImage5 = new BitmapImage(new Uri("pack://application:,,,/Images/ground5.png"));
            groundImage6 = new BitmapImage(new Uri("pack://application:,,,/Images/ground6.png"));
            obstacleImage = new BitmapImage(new Uri("pack://application:,,,/Images/obst1.png"));
            obstacleImage2 = new BitmapImage(new Uri("pack://application:,,,/Images/obst2.png"));
            obstacleImage3 = new BitmapImage(new Uri("pack://application:,,,/Images/obst3.png"));
            gemimage = new BitmapImage(new Uri("pack://application:,,,/Images/bluegem.png"));
            gemimage1 = new BitmapImage(new Uri("pack://application:,,,/Images/zoldasvany.png"));
            gemimage2 = new BitmapImage(new Uri("pack://application:,,,/Images/sargaasvany.png"));
            simTimer = new DispatcherTimer();
            visualX = sPosition[1];
            visualY = sPosition[0];
            simTimer.Interval = TimeSpan.FromSeconds(Time.TimeRate);
            simTimer.Tick += SimTimer_Tick;
            //terkep[32, 34] = "R";
            MapController.FillUpGameSpace(this);
        }

        private void StartBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!gameStarted)
                {
                    int hours = int.Parse(DurationInput.Text);
                    if (hours < 25)
                    {
                        MessageBox.Show("Kérem, adjon meg egy 24-nél számot a küldetés hosszára órában!");
                        return;
                    }
                    gameStarted = true;
                    gameOn = true;
                    simTimer.Start();
                    missionLength = hours;
                    maxMinutes = missionLength * 60;
                    WriteToLog("A küldetés elindult...", 0);
                }
            }
            catch (FormatException)
            {
                MessageBox.Show("Kérem, érvényes számot adjon meg a küldetés hosszára percben!");
            }
        }
        private async void SimTimer_Tick(object sender, EventArgs e)
        {
            //if(Time.MissionEndTime.Minute != 0)
            //{
            //    if(rover.Yposition == sPosition[0] && rover.Xposition == sPosition[1])
            //    {
            //        MessageBox.Show($"Siker");
            //    }
            //    else
            //    {
            //        MessageBox.Show($"A rover pozija x{rover.Xposition} és y{rover.Yposition}, {missionLength}");
            //    }
            //}
            //idolepes
            Time.AddTime();
            Time.RemainingMissionTimeChange(this);
            //Rover vizuális mozgatása a logikai pozíció felé, sima animációval
            if (stepX != 0)
            {
                visualX += stepX;
                if ((stepX > 0 && visualX >= rover.Xposition) || (stepX < 0 && visualX <= rover.Xposition))
                {
                    visualX = rover.Xposition;
                    stepX = 0; // Megállítjuk az X tengelyen
                }
            }
            if (stepY != 0)
            {
                visualY += stepY;
                // Ez a feltétel biztosítja, hogy ne lépjünk túl a célpozíción, és pontosan odaérjünk, ahol a logikai rover van
                if ((stepY > 0 && visualY >= rover.Yposition) || (stepY < 0 && visualY <= rover.Yposition))
                {
                    visualY = rover.Yposition;
                    stepY = 0; // Megállítjuk az Y tengelyen
                }
            }
            //Visszaszámlálás a küldetés végéig
            double passedMinutes = Time.TimeSpent.TotalMinutes;
            double remainingMinutes = maxMinutes - passedMinutes;
            int requiredMinutes = RoverAI.HazautIdoIgenyPercben(map, rover);
            if (remainingMinutes <= requiredMinutes && !BackToSpawn)
            {
                BackToSpawn = true;
                activePath = null;
                pathIndex = 0;
                WriteToLog("Visszatérés a bázisra.", 0);
            }
            if (Time.IsDay)
            {
                SunImage.Opacity = 1;
                MoonImage.Opacity = 0;
                idoszak.Text = "Nappal";
            }
            else
            {
                SunImage.Opacity = 0;
                MoonImage.Opacity = 1;
                idoszak.Text = "Éjszaka";
            }
            if (Time.CurrentTime.Minute == 30 || Time.CurrentTime.Minute == 0)
            {
                visualX = rover.Xposition;
                visualY = rover.Yposition;
                stepX = 0;
                stepY = 0;

                if (gameOn)
                {
                    // Ha épp számolunk utat, ne csináljunk mást
                    if (isCalculatingPath) return;

                    if (!rover.IsMining && (activePath == null || activePath.Count == 0))
                    {
                        if (BackToSpawn)
                        {
                            if (rover.Yposition == sPosition[0] && rover.Xposition == sPosition[1])
                            {
                                gameOn = false;
                                simTimer.Stop();
                                WriteToLog("KÜLDETÉS SIKERES! A rover visszaért.", 0);
                                return;
                            }
                            isCalculatingPath = true;
                            activePath = await Task.Run(() => RoverAI.BackToSpawn(map, rover));
                            pathIndex = 0;
                            isCalculatingPath = false;
                        }
                        else
                        {
                            isCalculatingPath = true;
                            activePath = await Task.Run(() => RoverAI.LegkozelebbiGemKereses(map, rover));
                            pathIndex = 0;
                            isCalculatingPath = false;

                            if (activePath == null)
                            {
                                BackToSpawn = true;
                            }
                        }
                        if (activePath != null && activePath.Count > 0)
                        {
                            var cel = activePath.Last();
                            celPozicio.Text = $"Cél -> X: {cel[1]}, Y: {cel[0]}";
                        }
                        else
                        {
                            celPozicio.Text = "Nincs több elérhető ásvány!";
                            BackToSpawn = true;
                        }
                    }
                }
                if (rover.IsMining)
                {
                    // Bányászat végrehajtása
                    rover.Mine(Time);
                    asvanycounter.Text = $"Kibányászot ásványok: {rover.CollectedMinerals}";
                    // Bányászat után a cella kiürül, a rover pedig készen áll a következő célpont keresésére
                    rover.IsMining = false;
                    map[(int)rover.Yposition, (int)rover.Xposition] = ".";
                    if (gemImg[(int)rover.Yposition, (int)rover.Xposition] != null)
                    {
                        jatekter.Children.Remove(gemImg[(int)rover.Yposition, (int)rover.Xposition]);
                        gemImg[(int)rover.Yposition, (int)rover.Xposition] = null;
                    }
                    WriteToLog($"Kibányásztam egy ásványt a {rover.Xposition};{rover.Yposition} koordinátán!", 0);
                }
                else if (activePath != null && pathIndex < activePath.Count)
                {
                    // Mozgás végrehajtása a meghatározott útvonalon
                    int remainingSteps = activePath.Count - pathIndex;
                    int desiredSpeed = GetOptimalSpeed(remainingSteps);
                    // Ha kevesebb lépés van hátra, mint a sebességünk, akkor csak annyit megyünk
                    rover.CurrentSpeed = Math.Min(desiredSpeed, remainingSteps);
                    log.DistanceTraveled += rover.CurrentSpeed;
                    // Fogyasztás levonása a sebesség alapján (E = 2 * v^2)
                    rover.MovementEnergyConsumption();
                    // Lépések megtétele index-szel (RemoveAt(0) helyett)
                    double startX = rover.Xposition;
                    double startY = rover.Yposition;
                    for (int i = 0; i < rover.CurrentSpeed; i++)
                    {
                        int[] nextStep = activePath[pathIndex];
                        rover.Yposition = nextStep[0]; // Y a sor
                        rover.Xposition = nextStep[1]; // X az oszlop
                        pathIndex++;
                    }
                    stepX = (rover.Xposition - startX) / 30;
                    stepY = (rover.Yposition - startY) / 30;

                    //utvege
                    if (pathIndex >= activePath.Count)
                    {
                        rover.IsMining = true;
                        activePath = null;
                        pathIndex = 0;
                    }
                }
                else if (BackToSpawn)
                {
                    if (rover.Yposition == sPosition[0] && rover.Xposition == sPosition[1])
                    {
                        gameOn = false;
                        simTimer.Stop();
                        WriteToLog("Visszaértem a kiindulási pontra és a küldetés véget ért!", 0);
                        return;
                    }
                    else if (activePath == null || pathIndex >= activePath.Count)
                    {
                        isCalculatingPath = true;
                        activePath = await Task.Run(() => RoverAI.BackToSpawn(map, rover));
                        pathIndex = 0;
                        isCalculatingPath = false;

                        if (activePath != null && activePath.Count > 0)
                        {
                            int remainingSteps = activePath.Count - pathIndex;
                            int desiredSpeed = GetOptimalSpeed(remainingSteps);
                            rover.CurrentSpeed = Math.Min(desiredSpeed, remainingSteps);
                            log.DistanceTraveled += rover.CurrentSpeed;
                            rover.MovementEnergyConsumption();

                            for (int i = 0; i < rover.CurrentSpeed; i++)
                            {
                                if (pathIndex < activePath.Count)
                                {
                                    int[] nextStep = activePath[pathIndex];
                                    rover.Yposition = nextStep[0]; // Sor
                                    rover.Xposition = nextStep[1]; // Oszlop
                                    pathIndex++;
                                }
                            }
                            WriteToLog($"Hazafelé tartok... Pozíció: {rover.Xposition};{rover.Yposition}", rover.CurrentSpeed);
                        }
                    }
                }
                else
                {
                    //standby
                    rover.DrainBattery(1);
                }
                rover.ChargeBattery(Time);
                IsChargingindicator.Visibility = rover.IsCharging ? Visibility.Visible : Visibility.Collapsed;
                IsntChargingindicator.Visibility = rover.IsCharging ? Visibility.Collapsed : Visibility.Visible;
            }

            if (Time.CurrentTime.Minute == 30 || Time.CurrentTime.Minute == 0)
            {
                egysegnyiuzemanyag.Text = $"{rover.AllBatteryUsage} egység";
                osszlepes.Text = $"Megtett összlépés: {log.DistanceTraveled}";
                UpdateChart(); // Most már csak logikai lépésenként számolja újra a kört!
                egyblokk.Text = $"Lassú (2 egység/fél óra) {rover.Speed1BatteryUsage} energia elhasználva.";
                kettőblokk.Text = $"Normál (8 egység/fél óra) {rover.Speed2BatteryUsage} energia elhasználva.";
                háromblokk.Text = $"Gyors (18 egység/fél óra) {rover.Speed3BatteryUsage} energia elhasználva.";
                miningenergy.Text = $"Bányászás (2 egység/fél óra) {rover.MiningBatteryUsage} energia elhasználva.";
                standybyenergy.Text = $"StandBy (1 egység/fél óra) {rover.StandByBatteryUsage} energia elhasználva.";
                EnergyBar.Value = rover.BatteryLevel;
            }


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
                }
            }

            return Math.Min(speed, hatralevoLepesek);
        }

        private void WriteToLog(string message, int speed)
        {
            string logText =
            $"[{Time.GetCurrentTimeString()}] {message}\n" +
            $"  • Akku: {rover.BatteryLevel}\n" +
            $"  • Sebesség: {speed} | Távolság: {log.DistanceTraveled}\n" +
            $"  • Begyűjtött ásványok: {rover.CollectedMinerals}";

            TextBlock newLog = new TextBlock
            {
                Text = logText,
                Foreground = Brushes.White,
                FontSize = 12,
                Margin = new Thickness(0, 0, 0, 5)
            };

            LogPanel.Children.Insert(0, newLog);

            if (LogPanel.Children.Count > 50)
            {
                LogPanel.Children.RemoveAt(LogPanel.Children.Count - 1);
            }
        }


        /// <summary>
        /// A rover pozíciójának frissítése a játéktéren a VIZUÁLIS koordináták alapján
        /// </summary>
        public void RefreshRoverPosition()
        {
            // Itt a logikai rover.Xposition helyett a folyamatosan változó visualX-et használjuk!
            Canvas.SetLeft(roverImg, visualX * tileSize);
            Canvas.SetTop(roverImg, visualY * tileSize);
            txtPos.Text = $"X: {rover.Xposition}, Y: {rover.Yposition}";
            if (FollowRoverBox.IsChecked == true)
            {
                kamera.ScrollToVerticalOffset(visualY * tileSize - (kamera.ActualHeight / 2));
                kamera.ScrollToHorizontalOffset(visualX * tileSize - (kamera.ActualWidth / 2));
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

            // Nagyív jelzése (1 ha a sweepAngle nagyobb, mint 180 fok)
            int isLargeArc = sweepAngle > 180 ? 1 : 0;
            return $"M {centerX},{centerY} L {x1.ToString(CultureInfo.InvariantCulture)},{y1.ToString(CultureInfo.InvariantCulture)} " +
                   $"A {radius},{radius} 0 {isLargeArc},1 {x2.ToString(CultureInfo.InvariantCulture)},{y2.ToString(CultureInfo.InvariantCulture)} Z";
        }

        public void UpdateChart()
        {
            // Az egyes tevékenységekhez tartozó fogyasztások összegzése
            double total = rover.Speed1BatteryUsage +
                           rover.Speed2BatteryUsage +
                           rover.Speed3BatteryUsage +
                           rover.MiningBatteryUsage +
                           rover.StandByBatteryUsage;

            // Ha még nem fogyasztott semmit, nem rajzolunk semmit
            if (total == 0) return;

            // Szeletek szögeinek kiszámítása a teljes fogyasztáshoz viszonyítva
            double a1 = (rover.Speed1BatteryUsage / total) * 360;
            double a2 = (rover.Speed2BatteryUsage / total) * 360;
            double a3 = (rover.Speed3BatteryUsage / total) * 360;
            double a4 = (rover.MiningBatteryUsage / total) * 360;
            double a5 = (rover.StandByBatteryUsage / total) * 360;

            double currentAngle = 0;
            // A szeletek rajzolása a körön
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
            }
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