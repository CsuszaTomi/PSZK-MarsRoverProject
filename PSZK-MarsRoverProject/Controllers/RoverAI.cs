using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PSZK_MarsRoverProject.Models;

namespace PSZK_MarsRoverProject.Controllers
{
    internal class RoverAI
    {
        /// <summary>
        /// A* algoritmus alapú útvonaltervező, amely megkeresi a legközelebbi 
        /// gyűjthető ásványt (Kék: "B", Sárga: "Y", Zöld: "G") a térképen, és 
        /// kiszámítja az odavezető legoptimálisabb útvonalat.
        /// Az algoritmus támogatja a 8 irányú mozgást (átlós is) és prioritási sort 
        /// használ a költségek (távolság/energia) optimalizálása érdekében.
        /// </summary>
        /// <param name="terkep">Az 50x50-es marsi felszínt reprezentáló 2D tömb.</param>
        /// <param name="rover">A rover aktuális pozícióját és állapotát tároló objektum.</param>
        /// <returns>A startponttól a célig vezető koordináták listája (ahol a 0. elem a kiindulópont), vagy null, ha nincs elérhető célpont.</returns>
        static public List<int[]> LegkozelebbiGemKereses(string[,] terkep, Rover rover)
        {
            int sor = terkep.GetLength(0);
            int oszlop = terkep.GetLength(1);
            //Összegyűjtjük az összes gem pozícióját a heurisztikához
            List<(int x, int y)> gemek = new List<(int, int)>();
            for (int i = 0; i < sor; i++)
                for (int j = 0; j < oszlop; j++)
                    if (terkep[i, j] == "G" || terkep[i, j] == "Y" || terkep[i, j] == "B")
                        gemek.Add((i, j));

            //A prioritási sor létrehozása az A* algoritmushoz, ahol a csomópontokat a becsült teljes költség (F = G + H) alapján rendezzük
            PriorityQueue<Node, double> q = new PriorityQueue<Node, double>();
            double[,] eddigiKoltseg = new double[sor, oszlop];
            for (int i = 0; i < sor; i++)
                for (int j = 0; j < oszlop; j++)
                    eddigiKoltseg[i, j] = double.MaxValue;
            int startX = (int)rover.Yposition;
            int startY = (int)rover.Xposition;
            // Heurisztika (H): légvonalbeli távolság a legközelebbi gemhez
            double startH = Heurisztika(startX, startY, gemek);
            Node startNode = new Node(startX, startY, 0, startH);
            q.Enqueue(startNode, startNode.F);
            eddigiKoltseg[startX, startY] = 0;

            // Lehetséges mozgási irányok
            int[,] directions = new int[,] {
                { -1, 0 }, { 1, 0 }, { 0, -1 }, { 0, 1 },
                { -1, -1 }, { -1, 1 }, { 1, -1 }, { 1, 1 }
            };
            // A* algoritmus fő ciklusa, addig fut, amíg van még vizsgálandó csomópont a prioritási sorban
            while (q.Count > 0)
            {
                Node currentTile = q.Dequeue();
                if (terkep[currentTile.X, currentTile.Y] == "G" || terkep[currentTile.X, currentTile.Y] == "Y" || terkep[currentTile.X, currentTile.Y] == "B")
                {
                    return UtvonalOsszeallitasa(currentTile);
                }
                for (int i = 0; i < directions.GetLength(0); i++)
                {
                    int szomszedX = currentTile.X + directions[i, 0];
                    int szomszedY = currentTile.Y + directions[i, 1];

                    if (szomszedX >= 0 && szomszedX < sor && szomszedY >= 0 && szomszedY < oszlop && terkep[szomszedX, szomszedY] != "#")
                    {
                        double koltseg = 1.0;
                        double ujG = currentTile.G + koltseg;
                        // Csak akkor frissítjük a szomszéd költségét és helyét, ha jobb útvonalat találtunk
                        if (ujG < eddigiKoltseg[szomszedX, szomszedY])
                        {
                            eddigiKoltseg[szomszedX, szomszedY] = ujG;
                            double h = Heurisztika(szomszedX, szomszedY, gemek);
                            Node szomszed = new Node(szomszedX, szomszedY, ujG, h, currentTile);
                            q.Enqueue(szomszed, szomszed.F);
                        }
                    }
                }
            }
            return null;
        }

        public static double Heurisztika(int x, int y, List<(int x, int y)> gemek)
        {
            if (gemek.Count == 0) return 0;
            double minTav = double.MaxValue;
            foreach (var g in gemek)
            {
                double tav = Math.Sqrt(Math.Pow(g.x - x, 2) + Math.Pow(g.y - y, 2));
                if (tav < minTav) minTav = tav;
            }
            return minTav;
        }

        /// <summary>
        /// Visszafejti a megtalált útvonalat a célponttól a kiindulási pontig 
        /// a Node-ok szülő (Parent) referenciái alapján.
        /// </summary>
        /// <param name="celpont">Az A* algoritmus által megtalált célállomás csomópontja.</param>
        /// <returns>A lépések sorrendjét tartalmazó lista, ahol az első elem a startpont, az utolsó pedig a cél.</returns>
        static List<int[]> UtvonalOsszeallitasa(Node celpont)
        {
            List<int[]> utvonal = new List<int[]>();
            Node aktualis = celpont;
            // Visszafejtjük az útvonalat a célponttól a kiindulási pontig
            while (aktualis != null && aktualis.Parent != null)
            {
                // Az aktuális csomópont koordinátáit hozzáadjuk az útvonalhoz
                utvonal.Add(new int[] { aktualis.X, aktualis.Y });
                aktualis = aktualis.Parent;
            }
            utvonal.Reverse(); //start lessz elso
            return utvonal;
        }

        /// <summary>
        /// Kiszámítja az utat a rover aktuális pozíciójától a kezdőpontig (S).
        /// </summary>
        /// <param name="terkep">Az 50x50-es marsi felszín.</param>
        /// <param name="rover">A rover aktuális pozíciója.</param>
        /// <returns>A kezdőpontig vezető koordináták listája.</returns>
        static public List<int[]> BackToSpawn(string[,] terkep, Rover rover)
        {
            // S helyének meghatározása a térképen
            int[] spawnPos = MapController.GetSLocation(terkep);
            if (spawnPos[0] == -1) 
                return null; // Ha nincs S a pályán
            int celX = spawnPos[0];
            int celY = spawnPos[1];
            int sor = terkep.GetLength(0);
            int oszlop = terkep.GetLength(1);
            // A* algoritmus használata a visszavezető útvonal megtalálásához
            PriorityQueue<Node, double> q = new PriorityQueue<Node, double>();
            double[,] eddigiKoltseg = new double[sor, oszlop];
            for (int i = 0; i < sor; i++)
            {
                for (int j = 0; j < oszlop; j++)
                {
                    eddigiKoltseg[i, j] = double.MaxValue;
                }
            }
            int startX = (int)rover.Yposition;
            int startY = (int)rover.Xposition;
            Node startNode = new Node(startX, startY, 0, 0);
            q.Enqueue(startNode, startNode.F);
            eddigiKoltseg[startX, startY] = 0;

            int[,] directions = new int[,] {
                { -1, 0 }, { 1, 0 }, { 0, -1 }, { 0, 1 },
                { -1, -1 }, { -1, 1 }, { 1, -1 }, { 1, 1 }
            };

            while (q.Count > 0)
            {
                Node currentTile = q.Dequeue();

                //megérkezés a célhoz ellenőrzése
                if (currentTile.X == celX && currentTile.Y == celY)
                {
                    return UtvonalOsszeallitasa(currentTile);
                }

                for (int i = 0; i < directions.GetLength(0); i++)
                {
                    int szomszedX = currentTile.X + directions[i, 0];
                    int szomszedY = currentTile.Y + directions[i, 1];

                    if (szomszedX >= 0 && szomszedX < sor && szomszedY >= 0 && szomszedY < oszlop && terkep[szomszedX, szomszedY] != "#")
                    {
                        // Egyszerű távolság alapú költség (1 lépés = 1 egység)
                        double ujG = currentTile.G + 1.0;

                        if (ujG < eddigiKoltseg[szomszedX, szomszedY])
                        {
                            eddigiKoltseg[szomszedX, szomszedY] = ujG;

                            // Heurisztika (H): légvonalbeli távolság a célig (S-ig)
                            double h = Math.Sqrt(Math.Pow(celX - szomszedX, 2) + Math.Pow(celY - szomszedY, 2));

                            Node szomszed = new Node(szomszedX, szomszedY, ujG, h, currentTile);
                            q.Enqueue(szomszed, szomszed.F);
                        }
                    }
                }
            }

            return null; // Nincs útvonal
        }

        //public static int HazautIdoIgenyPercben(string[,] terkep, Rover rover)
        //{
        //    var utvonal = BackToSpawn(terkep, rover);
        //    if (utvonal == null) 
        //        return 0;
        //    int lepesek = utvonal.Count;
        //    int percPerLepes = 30;
        //    return (lepesek * percPerLepes);
        //}

        public static int HazautIdoIgenyPercben(string[,] terkep, Rover rover)
        {
            var utvonal = BackToSpawn(terkep, rover);
            if (utvonal == null)
                return 0;

            int lepesek = utvonal.Count;
            int alapSzuksegesPerc = lepesek * 17;
            return alapSzuksegesPerc + 60;
        }
    }
}
