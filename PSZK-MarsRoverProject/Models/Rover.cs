using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSZK_MarsRoverProject.Models
{
    internal class Rover
    {
        public Rover() { }
        public int Xposition { get; set; }
        public int Yposition { get; set; }
        public float BatteryLevel { get; set; }
        public bool IsCharging { get; set; }

        public int CurrentSpeed { get; set; }

        public bool IsMining { get; set; }

        public void MovementEnergyConsumption()
        {
            float usedEnergy = 2 * (float)Math.Pow(CurrentSpeed, 2);
            BatteryLevel -= usedEnergy;
        }

        public void Mine()
        {
            BatteryLevel -= 2;
        }

        public void ChargeBattery(SimulationTime time)
        {
            if (time.CurrentDayProgression == "nappal")
            {
                if(BatteryLevel <= 100)
                {
                    IsCharging = true;
                    BatteryLevel += 10;
                }
            }
            else
            {
                IsCharging = false;
            }
        }

    }
}
