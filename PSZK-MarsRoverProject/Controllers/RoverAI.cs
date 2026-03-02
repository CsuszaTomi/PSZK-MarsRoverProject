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
        /// A* algoritmus alapú útvonaltervező, amely meghatározza a legközelebbi 
        /// gyűjthető ásvány (Kék: "B", Sárga: "Y", Zöld: "G") koordinátáit.
        /// Az algoritmus támogatja a 8 irányú mozgást (átlós is) és prioritási sort 
        /// használ a költségek (távolság/energia) optimalizálása érdekében.
        /// </summary>
        /// <param name="terkep">Az 50x50-es marsi felszínt reprezentáló 2D tömb.</param>
        /// <param name="rover">A rover aktuális pozícióját és állapotát tároló objektum.</param>
        /// <returns>A legközelebbi ásvány [sor, oszlop] koordinátái, vagy null, ha nincs elérhető célpont.</returns>
        static public int[] LegkozelebbiGemKereses(string[,] terkep, Rover rover)
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
                Node curr = q.Dequeue();
                //Cel ellenorzese
                if (terkep[curr.X, curr.Y] == "G" || terkep[curr.X, curr.Y] == "Y" || terkep[curr.X, curr.Y] == "B")
                {
                    return new int[] { curr.X, curr.Y };
                }
                //szomszedok vizsgalata
                for (int i = 0; i < directions.GetLength(0); i++)
                {
                    int szomszedX = curr.X + directions[i, 0];
                    int szomszedY = curr.Y + directions[i, 1];
                    if (szomszedX >= 0 && szomszedX < sor && szomszedY >= 0 && szomszedY < oszlop && terkep[szomszedX, szomszedY] != "#")
                    {
                        // KÖLTSÉG SZÁMÍTÁSA: 
                        // minden blokk 1 lépés.
                        // később energiafogyasztas
                        double koltseg = 1.0;
                        double ujG = curr.G + koltseg;
                        if (ujG < eddigiKoltseg[szomszedX, szomszedY])
                        {
                            eddigiKoltseg[szomszedX, szomszedY] = ujG;
                            // Heurisztika (H): Mivel nem tudjuk melyik a legközelebbi gem, 
                            // a keresésnél a H értéke 0.
                            // Ha lenne fix célpontod, ide a távolságot írnánk.
                            Node szomszed = new Node(szomszedX, szomszedY, ujG, 0);
                            q.Enqueue(szomszed, szomszed.F);
                        }
                    }
                }
            }
            return null;
        }
    }
}
