using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PSZK_MarsRoverProject.Controllers
{
    internal class MapController
    {
        public static string[,] CsvReader()
        {
            string[,] map = new string[50, 50];
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
            return map;
        }

        public static int[] GetSLocation(string[,] map)
        {
            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    if (map[i, j] == "S")
                    {
                        return new int[] { i, j };
                    }
                }
            }
            return new int[] { -1, -1 };
        }

        public static void FillUpGameSpace(MainWindow mw)
        {
            int rows = mw.map.GetLength(0);
            int cols = mw.map.GetLength(1);
            int pixelWidth = cols * MainWindow.tileSize;
            int pixelHeight = rows * MainWindow.tileSize;

            // Talaj kirajzolása egyetlen háttérképbe (DrawingVisual)
            DrawingVisual drawingVisual = new DrawingVisual();
            using (DrawingContext drawingContext = drawingVisual.RenderOpen())
            {
                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < cols; j++)
                    {
                        ImageSource talajSource = GetGroundImageSource(mw);
                        Rect rect = new Rect(j * MainWindow.tileSize, i * MainWindow.tileSize, MainWindow.tileSize, MainWindow.tileSize);
                        drawingContext.DrawImage(talajSource, rect);
                    }
                }
            }

            // A kirajzolt vizuális elem bitmap-pé alakítása
            RenderTargetBitmap bmp = new RenderTargetBitmap(pixelWidth, pixelHeight, 96, 96, PixelFormats.Pbgra32);
            bmp.Render(drawingVisual);

            Image hattekKep = new Image()
            {
                Width = pixelWidth,
                Height = pixelHeight,
                Source = bmp,
                SnapsToDevicePixels = true
            };
            Canvas.SetLeft(hattekKep, 0);
            Canvas.SetTop(hattekKep, 0);
            Panel.SetZIndex(hattekKep, 0);
            mw.jatekter.Children.Add(hattekKep);

            // Csak az objektumok (ásványok, akadályok) kerülnek külön Image elemként a Canvas-ra
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    string jel = mw.map[i, j];
                    if (jel != "." && jel != "R" && jel != "S")
                    {
                        Image targy = new Image()
                        {
                            Width = MainWindow.tileSize,
                            Height = MainWindow.tileSize,
                            Source = GetOtherImageSource(jel, mw),
                            SnapsToDevicePixels = true
                        };
                        Canvas.SetLeft(targy, j * MainWindow.tileSize);
                        Canvas.SetTop(targy, i * MainWindow.tileSize);
                        Panel.SetZIndex(targy, 1);
                        mw.jatekter.Children.Add(targy);

                        if (jel == "G" || jel == "Y" || jel == "B")
                        {
                            mw.gemImg[i, j] = targy;
                        }
                    }
                }
            }

            // Rover rajzolás
            mw.roverImg = new Image()
            {
                Width = MainWindow.tileSize,
                Height = MainWindow.tileSize,
                Source = new BitmapImage(new Uri("pack://application:,,,/Images/kicsikocsi.png")),
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = new RotateTransform(0)
            };
            Panel.SetZIndex(mw.roverImg, 10);
            mw.RefreshRoverPosition();
            mw.jatekter.Children.Add(mw.roverImg);
        }

        private static readonly Random rnd = new Random();

        public static ImageSource GetOtherImageSource(string karakter, MainWindow mw)
        {
            int szam = rnd.Next(1, 3);
            switch (karakter)
            {
                case "#":
                    switch (szam)
                    {
                        case 1: return mw.obstacleImage;
                        case 2: return mw.obstacleImage2;
                        case 3: return mw.obstacleImage3;
                        default: return mw.obstacleImage;
                    }
                case "G":
                    return mw.gemimage1;
                case "Y":
                    return mw.gemimage2;
                case "B":
                    return mw.gemimage;
                default:
                    return mw.groundImage1;
            }
        }

        public static ImageSource GetGroundImageSource(MainWindow mw)
        {
            int szam = rnd.Next(1, 7);
            switch (szam)
            {
                case 1: return mw.groundImage1;
                case 2: return mw.groundImage2;
                case 3: return mw.groundImage3;
                case 4: return mw.groundImage5;
                case 5: return mw.groundImage6;
                default: return mw.groundImage1;
            }
        }
    }
}