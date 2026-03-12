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
            PriorityQueue<Node, double> q = new PriorityQueue<Node, double>();
            //megjegyzes, koltseg megyjegyzes
            double[,] eddigiKoltseg = new double[sor, oszlop];
            for (int i = 0; i < sor; i++)
            {
                for (int j = 0; j < oszlop; j++)
                {
                    eddigiKoltseg[i, j] = double.MaxValue;
                }
            }
            int startX = rover.Yposition;
            int startY = rover.Xposition;
            //Kiinduló pont hozzáadása a prioritási sorhoz
            Node startNode = new Node(startX, startY, 0, 0);
            q.Enqueue(startNode, startNode.F);
            eddigiKoltseg[startX, startY] = 0;
            //8 irany: fel, le, balra, jobbra + 4 diagonal
            int[,] directions = new int[,] {
                { -1, 0 }, { 1, 0 }, { 0, -1 }, { 0, 1 },   // Merőleges
                { -1, -1 }, { -1, 1 }, { 1, -1 }, { 1, 1 }  // Átlós
                };
            while (q.Count > 0)
            {
                Node currentTile = q.Dequeue();
                //Cel ellenorzese
                if (terkep[currentTile.X, currentTile.Y] == "G" || terkep[currentTile.X, currentTile.Y] == "Y" || terkep[currentTile.X, currentTile.Y] == "B")
                {
                    return UtvonalOsszeallitasa(currentTile);
                }
                //szomszedok vizsgalata
                for (int i = 0; i < directions.GetLength(0); i++)
                {
                    int szomszedX = currentTile.X + directions[i, 0];
                    int szomszedY = currentTile.Y + directions[i, 1];
                    if (szomszedX >= 0 && szomszedX < sor && szomszedY >= 0 && szomszedY < oszlop && terkep[szomszedX, szomszedY] != "#")
                    {
                        // KÖLTSÉG SZÁMÍTÁSA: 
                        // minden blokk 1 lépés.
                        // később energiafogyasztas
                        double koltseg = 1.0;
                        double ujG = currentTile.G + koltseg;//curr G + koltseg
                        if (ujG < eddigiKoltseg[szomszedX, szomszedY])
                        {
                            eddigiKoltseg[szomszedX, szomszedY] = ujG;
                            //curr lessz a szomszed szülője
                            Node szomszed = new Node(szomszedX, szomszedY, ujG, 0, currentTile);
                            q.Enqueue(szomszed, szomszed.F);
                        }
                    }
                }
            }
            return null;
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
    }
}
