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
        public float AllBatteryUsage { get; set; }
        public float BatteryLevel { get; set; }
        public bool IsCharging { get; set; }

        public int CurrentSpeed { get; set; }

        public bool IsMining { get; set; }

        public void MovementEnergyConsumption()
        {
            float usedEnergy = 2 * (CurrentSpeed*CurrentSpeed);
            AllBatteryUsage += usedEnergy;
            BatteryLevel -= usedEnergy;
        }

        public void Mine(SimulationTime time)
        {
            if (time.IsDay)
            {
                BatteryLevel += 8;
                AllBatteryUsage += 2;
            }
            else
            {
                BatteryLevel -= 2;
                AllBatteryUsage += 2;
            }
        }

        public void DrainBattery(float amount)
        {
            BatteryLevel -= amount;
        }

        public void ChargeBattery(SimulationTime time)
        {
            if (time.IsDay && CurrentSpeed != 3)
            {
                IsCharging = true;
                BatteryLevel += 10;

                // Ne menjen 100 fölé (elég egyszer a végére írni!)
                if (BatteryLevel > 100)
                {
                    BatteryLevel = 100;
                }
            }
            else if (time.IsDay && CurrentSpeed == 3)
            {
                IsCharging = false;
                BatteryLevel += 10;

                if (BatteryLevel > 100)
                {
                    BatteryLevel = 100;
                }
            }
            else
            {
                // Éjszaka nincs töltés
                IsCharging = false;
            }
        }

    }
}
