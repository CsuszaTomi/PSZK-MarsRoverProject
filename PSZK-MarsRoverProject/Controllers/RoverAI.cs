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
        public int[] LegkozelebbiGemKereses(string[,] terkep, Rover rover)
        {
            int[] celKordinata;
            List<int> bfs(List<List<int>> adj)
            {
                int V = adj.Count;
                bool[] visited = new bool[V];
                List<int> res = new List<int>();

                int src = 0;
                Queue<int> q = new Queue<int>();
                visited[src] = true;
                q.Enqueue(src);

                while (q.Count > 0)
                {
                    int curr = q.Dequeue();
                    res.Add(curr);

                    // visit all the unvisited
                    // neighbours of current node
                    foreach (int x in adj[curr])
                    {
                        if (!visited[x])
                        {
                            visited[x] = true;
                            q.Enqueue(x);
                        }
                    }
                }

                return res;
            }
            celKordinata = new int[2];
            return celKordinata;
        }
    }
}
