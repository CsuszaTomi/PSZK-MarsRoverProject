using PSZK_MarsRoverProject.Controllers;
using PSZK_MarsRoverProject.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PSZK_MarsRoverProject.View
{
    internal class Dashboard
    {
        private static string DrawPieSlice(double radius, double startAngle, double sweepAngle)
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

        public static void UpdateChart(Rover rover, MainWindow mw)
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
            mw.SliceSpeed1.Data = Geometry.Parse(DrawPieSlice(70, currentAngle, a1));
            currentAngle += a1;

            mw.SliceSpeed2.Data = Geometry.Parse(DrawPieSlice(70, currentAngle, a2));
            currentAngle += a2;

            mw.SliceSpeed3.Data = Geometry.Parse(DrawPieSlice(70, currentAngle, a3));
            currentAngle += a3;

            mw.SliceMining.Data = Geometry.Parse(DrawPieSlice(70, currentAngle, a4));
            currentAngle += a4;

            mw.SliceStandby.Data = Geometry.Parse(DrawPieSlice(70, currentAngle, a5));
        }

        public static void WriteToLog(string message, int speed, MainWindow mw, Rover rover, Log log)
        {
            string logText =
            $"[{mw.Time.GetCurrentTimeString()}] {message}\n" +
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

            mw.LogPanel.Children.Insert(0, newLog);

            if (mw.LogPanel.Children.Count > 50)
            {
                mw.LogPanel.Children.RemoveAt(mw.LogPanel.Children.Count - 1);
            }
        }
    }
}
