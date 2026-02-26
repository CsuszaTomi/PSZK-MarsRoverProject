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
        public double G; // Eddigi költség (mennyi energiát használtunk el idáig)
        public double H; // Heurisztika (becsült távolság a célig)
        public double F => G + H; // Összesített érték

        public Node(int x, int y, double g, double h)
        {
            X = x; Y = y; G = g; H = h;
        }
    }
}
