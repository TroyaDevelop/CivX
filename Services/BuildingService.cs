using System;
using System.Collections.Generic;
using System.Linq;
using CivilizationSimulation.Models;

namespace CivilizationSimulation.Services
{
    public class BuildingService
    {
        private readonly Settlement _settlement;

        public BuildingService(Settlement settlement)
        {
            _settlement = settlement;
        }

        public bool StartConstruction(string buildingName, BuildingType type)
        {
            // Удалена проверка на наличие технологии "Строительство"
            
            // Создаем новое здание
            Building building = new Building(buildingName, type);

            // Проверяем наличие необходимых ресурсов для строительства
            foreach (var resourceCost in building.BuildCost)
            {
                if (!_settlement.HasResource(resourceCost.Key, resourceCost.Value))
                    return false;
            }

            // Списание ресурсов
            foreach (var resourceCost in building.BuildCost)
            {
                _settlement.ConsumeResource(resourceCost.Key, resourceCost.Value);
            }

            // Добавляем здание в список строений поселения
            _settlement.Buildings.Add(building);
            return true;
        }

        public void ProgressBuilding(int productionPoints)
        {
            // Получаем список всех незавершённых зданий
            var unfinishedBuildings = _settlement.Buildings.Where(b => !b.IsCompleted).ToList();
            if (unfinishedBuildings.Count == 0 || productionPoints <= 0)
                return;
            int buildingIndex = 0;
            for (int i = 0; i < productionPoints; i++)
            {
                unfinishedBuildings[buildingIndex].AddProductionPoints(1);
                buildingIndex = (buildingIndex + 1) % unfinishedBuildings.Count;
            }
        }

        public bool UpgradeBuilding(string buildingName)
        {
            var building = _settlement.GetBuilding(buildingName);
            
            if (building == null || !building.IsCompleted)
                return false;

            // Проверяем наличие необходимых ресурсов для улучшения
            foreach (var resourceCost in building.UpgradeCost)
            {
                if (!_settlement.HasResource(resourceCost.Key, resourceCost.Value))
                    return false;
            }

            // Списание ресурсов
            foreach (var resourceCost in building.UpgradeCost)
            {
                _settlement.ConsumeResource(resourceCost.Key, resourceCost.Value);
            }

            // Улучшаем здание
            return building.Upgrade();
        }

        public void MaintainBuildings()
        {
            // В будущем можно реализовать систему обслуживания зданий,
            // которая будет требовать ресурсы для поддержания зданий в рабочем состоянии
            
            foreach (var building in _settlement.Buildings.Where(b => b.IsCompleted))
            {
                foreach (var cost in building.MaintenanceCost)
                {
                    _settlement.ConsumeResource(cost.Key, cost.Value);
                }
            }
        }
    }
}