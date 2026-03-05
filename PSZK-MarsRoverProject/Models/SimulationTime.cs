using System;

namespace PSZK_MarsRoverProject.Models
{
    public class SimulationTime
    {
        public DateTime CurrentTime { get; set; } = new DateTime(2024, 1, 1, 8, 0, 0);

        public string CurrentDayProgression { get; set; } = "nappal";
        public bool IsDay { get; set; } = true;

        public TimeSpan TimeSpent { get; set; } = TimeSpan.Zero;
        public int TimeRate { get; set; } = 1;

        public void SetTime(int hour, int minute)
        {
            // Beállítjuk az órát és percet a jelenlegi napon belül
            CurrentTime = new DateTime(CurrentTime.Year, CurrentTime.Month, CurrentTime.Day, hour, minute, 0);
            UpdateDayState();
        }

        public void AddTime()
        {
            // 30 perc hozzáadása
            TimeSpan step = TimeSpan.FromMinutes(30);
            CurrentTime = CurrentTime.Add(step);
            // Növeljük az összesen eltöltött időt is
            TimeSpent = TimeSpent.Add(step);
            // Frissítjük a napszakot
            UpdateDayState();
        }

        public void UpdateDayState()
        {
            if (CurrentTime.Hour >= 8 && CurrentTime.Hour < 20)
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