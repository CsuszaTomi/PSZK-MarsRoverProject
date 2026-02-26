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
        /// Szélességi keresés (BFS) algoritmus segítségével meghatározza a legközelebbi 
        /// gyűjthető érc (Gem: "G", "Y", "B") koordinátáit a megadott térképen.
        /// Az algoritmus figyelembe veszi az akadályokat ("#"), és garantáltan 
        /// a legkevesebb lépésben elérhető célpontot adja vissza.
        /// </summary>
        /// <param name="terkep">A 2D-s rács, amely a pálya elemeit tartalmazza.</param>
        /// <param name="rover">A Rover objektum, amely tartalmazza az aktuális pozíciót.</param>
        /// <returns>A legközelebbi érc [x, y] koordinátái, vagy null, ha nincs elérhető érc a pályán.</returns>
        static public int[] LegkozelebbiGemKereses(string[,] terkep, Rover rover)
        {
            int[] celKordinata;
            // Breadth-First Search (BFS) algoritmus a legközelebbi gem keresésére
            Queue<int[]> q = new Queue<int[]>();
            bool[,] visited = new bool[terkep.GetLength(0), terkep.GetLength(1)];
            int startX = rover.Yposition;
            int startY = rover.Xposition;
            visited[startX, startY] = true;
            // kezdő pozíció hozzáadása a sorhoz
            q.Enqueue(new int[] { startX, startY });
            while (q.Count > 0)
            {
                // aktuális pozíció lekérése a sorból
                int[] curr = q.Dequeue();
                int x = curr[0];
                int y = curr[1];
                // ha gemet találunk, visszatérünk a koordinátákkal
                if (terkep[x, y] == "G" || terkep[x,y] == "Y" || terkep[x,y] == "B")
                {
                    return new int[] { x, y };
                }
                // szomszédok bejárása
                int[,] directions = new int[,] { { -1, 0 }, { 1, 0 }, { 0, -1 }, { 0, 1 } };
                for (int i = 0; i < directions.GetLength(0); i++)
                {
                    // új pozíció kiszámítása
                    int newX = x + directions[i, 0];
                    int newY = y + directions[i, 1];
                    // új pozíció érvényességének ellenőrzése és hozzáadása a sorhoz, ha még nem látogattuk meg és nem akadály
                    if (newX >= 0 && newX < terkep.GetLength(0) && newY >= 0 && newY < terkep.GetLength(1) && !visited[newX, newY] && terkep[newX, newY] != "#")
                    {
                        // új pozíció megjelölése látogatottként és hozzáadása a sorhoz
                        visited[newX, newY] = true;
                        q.Enqueue(new int[] { newX, newY });
                    }
                }
            }
            return null; // ha nem találunk gemet, visszatérünk null-lal
        }
    }
}
