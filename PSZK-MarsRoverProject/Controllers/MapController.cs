using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                return new int[] { -1, -1 }; // Ha nem találjuk meg az S-t, visszatérünk egy érvénytelen helyzettel
        }

        public static void FillUpGameSpace(MainWindow mw)
        {
            for (int i = 0; i < mw.map.GetLength(0); i++)
            {
                for (int j = 0; j < mw.map.GetLength(1); j++)
                {
                    //tile rajzolas
                    Image talaj = new Image()
                    {
                        Width = MainWindow.tileSize,
                        Height = MainWindow.tileSize,
                        Source = GetGroundImageSource(mw),
                        SnapsToDevicePixels = true
                    };
                    Canvas.SetLeft(talaj, j * MainWindow.tileSize);
                    Canvas.SetTop(talaj, i * MainWindow.tileSize);
                    Panel.SetZIndex(talaj, 0); //legalso reteg
                    mw.jatekter.Children.Add(talaj);

                    string jel = mw.map[i, j];
                    if (jel != "." && jel != "R")
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
                        Panel.SetZIndex(targy, 1); // A talaj felett legyen
                        mw.jatekter.Children.Add(targy);

                        // Ha ez egy gem (G, Y, B), elmentjük a referenciáját
                        if (jel == "G" || jel == "Y" || jel == "B")
                        {
                            mw.gemImg[i, j] = targy;
                        }
                    }
                }
            }
            //rover rajzolás
            mw.roverImg = new Image()
            {
                Width = MainWindow.tileSize,
                Height = MainWindow.tileSize,
                Source = new BitmapImage(new Uri("pack://application:,,,/Images/kicsikocsi.png")),
            };
            //A rover képe mindig a legfelső rétegben legyen
            Panel.SetZIndex(mw.roverImg, 10);
            mw.RefreshRoverPosition();
            mw.jatekter.Children.Add(mw.roverImg);
        }


        private static readonly Random rnd = new Random();
        /// <summary>
        /// A megadott karakter alapján visszaadja a megfelelő képet a játéktérhez
        /// </summary>
        /// <param name="karakter">A karakter, amelyhez a képet keresünk</param>
        /// <returns>A megfelelő ImageSource</returns>
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

        /// <summary>
        /// Random módon választ egy talajképet a rendelkezésre álló 7 közül, hogy változatosabbá tegye a játéktér megjelenését
        /// </summary>
        /// <returns></returns>
        public static ImageSource GetGroundImageSource(MainWindow mw)
        {
            // Ha 6-féle talajképed van (groundImage1-6), akkor Next(1, 7) kell
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