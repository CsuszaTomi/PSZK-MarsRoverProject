using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PSZK_MarsRoverProject.Models;

namespace PSZK_MarsRoverProject.Models
{
    internal class Rover
    {
        public Rover() { }
        public int Xposition { get; set; }
        public int Yposition { get; set; }
        public float AllBatteryUsage { get; set; }
        public float Speed3BatteryUsage { get; set; }
        public float Speed2BatteryUsage { get; set; }
        public float Speed1BatteryUsage { get; set; }
        public float CollectedMinerals { get; set; }
        public float StandByBatteryUsage { get; set; }
        public float MiningBatteryUsage { get; set; }
        public float BatteryLevel { get; set; }
        public bool IsCharging { get; set; }
        public string Direction { get; set; }
        public int CurrentSpeed { get; set; }

        public bool IsMining { get; set; }



        public void MovementEnergyConsumption()
        {
            float usedEnergy = 2 * (CurrentSpeed*CurrentSpeed);
            if (CurrentSpeed == 3)
            {   
                Speed3BatteryUsage += usedEnergy;
            }
            else if (CurrentSpeed == 2)
            {
                Speed2BatteryUsage += usedEnergy;
            }
            else if (CurrentSpeed == 1)
            {
                Speed1BatteryUsage += usedEnergy;
            }
            AllBatteryUsage += usedEnergy;
            BatteryLevel -= usedEnergy;
        }

        public void Mine(SimulationTime time)
        {
            BatteryLevel -= 2;
            AllBatteryUsage += 2;
            MiningBatteryUsage += 2;
            CollectedMinerals += 1;
        }

        public void DrainBattery(float amount)
        {
            BatteryLevel -= amount;
            AllBatteryUsage += amount;
            StandByBatteryUsage += amount;
        }

        public void Addbattery(float amount)
        {
            BatteryLevel += amount;
            if (BatteryLevel > 100) 
            {
                BatteryLevel = 100;
            }
        }

        public void ChargeBattery(SimulationTime time)
        {
            if (time.IsDay)
            {
                IsCharging = true;
                Addbattery(10);
            }
            else
            {
                IsCharging = false;
            }
        }

    }
}
