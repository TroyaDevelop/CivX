using System;
using System.Collections.Generic;
using CivilizationSimulation.Models;

namespace CivilizationSimulation.Services
{
    public class PopulationService
    {
        private readonly Settlement _settlement;
        private readonly Random _random = new Random();

        public PopulationService(Settlement settlement)
        {
            _settlement = settlement;
        }

        // Вся логика индивидуальных жителей и профессий удалена
        // Оставлены только методы для роста населения и потребления еды

        public void ConsumeFoodAndUpdateStatus()
        {
            int foodPerPerson = 2;
            int foodNeeded = _settlement.TotalPopulation * foodPerPerson;
            if (_settlement.Resources[ResourceType.Food] >= foodNeeded)
            {
                _settlement.Resources[ResourceType.Food] -= foodNeeded;
            }
            else
            {
                _settlement.Resources[ResourceType.Food] = 0;
            }
            _settlement.CheckPopulationGrowth();
        }
    }
}