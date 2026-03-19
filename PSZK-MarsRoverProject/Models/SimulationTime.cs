using System;

namespace PSZK_MarsRoverProject.Models
{
    public class SimulationTime
    {
        public DateTime CurrentTime { get; set; } = new DateTime(2024, 1, 1, 8, 0, 0);

        public string CurrentDayProgression { get; set; } = "nappal";
        public bool IsDay { get; set; } = true;

        public TimeSpan TimeSpent { get; set; } = TimeSpan.Zero;
        public double TimeRate { get; set; } = 1;

        public DateTime MissionEndTime { get; set; } = new DateTime(2026, 1, 1, 0, 0, 0);


        public void SetTime(int hour, int minute)
        {
            // Beállítjuk az órát és percet a jelenlegi napon belül
            CurrentTime = new DateTime(CurrentTime.Year, CurrentTime.Month, CurrentTime.Day, hour, minute, 0);
            UpdateDayState();
        }

        public void RemainingMissionTimeChange(MainWindow mw)
        {
            // Kiszámoljuk az eddig eltelt időt percben
            double passedMinutes = TimeSpent.TotalMinutes;
            double remainingMinutes = mw.maxMinutes - passedMinutes;
            if (remainingMinutes < 0) 
                remainingMinutes = 0;
            // Kiszámoljuk a hátralévő időt és beállítjuk a MissionEndTime-ot
            TimeSpan remaining = TimeSpan.FromMinutes(remainingMinutes);
            MissionEndTime = CurrentTime.Add(remaining);
            // Frissítjük a megjelenített hátralévő időt
            mw.hatralevoido.Text = $"{(int)remaining.TotalHours:00}:{remaining.Minutes:00}";
        }

        public void AddTime()
        {
            // 30 perc hozzáadása
            TimeSpan step = TimeSpan.FromMinutes(1);
            CurrentTime = CurrentTime.Add(step);
            // Növeljük az összesen eltöltött időt is
            TimeSpent = TimeSpent.Add(step);
            // Frissítjük a napszakot
            UpdateDayState();
        }


        public void UpdateDayState()
        {
            if (CurrentTime.Hour >= 6 && CurrentTime.Hour < 22)
            {
                IsDay = true;
                CurrentDayProgression = "nappal";
            }
            else
            {
                IsDay = false;
                CurrentDayProgression = "ejszaka";
            }
        }

        public string GetCurrentTimeString()
        {
            return CurrentTime.ToString("HH:mm");
        }
    }
}