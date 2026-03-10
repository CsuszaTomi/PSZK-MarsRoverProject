using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSZK_MarsRoverProject.Models
{
    internal class Node
    {
        public int X;
        public int Y;
        public double G; //koltseg
        public double H; //heurisztika
        public double F => G + H; //teljes koltseg (G + H)
        public Node Parent; // elozo node a pathban

        public Node(int x, int y, double g, double h, Node parent = null)
        {
            X = x; Y = y; G = g; H = h;
            Parent = parent;
        }
    }
}
