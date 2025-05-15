using System;
using System.Collections.Generic;
using System.Linq;
using CivilizationSimulation.Models;

namespace CivilizationSimulation.Services
{
    public class ResourceService
    {
        private readonly Settlement _settlement;
        private readonly Random _random = new Random();

        public ResourceService(Settlement settlement)
        {
            _settlement = settlement;
        }

        public int ProduceResources()
        {
            int foodProduction = 0;
            int productionPoints = 0;
            int knowledgeProduction = 0;

            // Производство пищи с полей
            var completedFields = _settlement.Buildings.Where(b => b.IsCompleted && b.Type == BuildingType.Field).ToList();
            foreach (var field in completedFields)
            {
                if (field.AssignedWorkers > 0)
                {
                    foodProduction += field.GetEffect("FoodProduction") * field.AssignedWorkers;
                }
            }

            // Базовый доход пищи
            foodProduction += 2; // базовый доход

            // Производство с рабочих на карте
            if (_settlement.Map != null)
            {
                for (int x = 0; x < _settlement.Map.GetLength(0); x++)
                for (int y = 0; y < _settlement.Map.GetLength(1); y++)
                {
                    var cell = _settlement.Map[x, y];
                    if (cell.AssignedWorkers > 0)
                    {
                        switch (cell.Terrain)
                        {
                            case TerrainType.Grass:
                                foodProduction += cell.AssignedWorkers;
                                break;
                            case TerrainType.Forest:
                                productionPoints += cell.AssignedWorkers;
                                break;
                        }
                    }
                }
            }

            // Производство знаний учёными
            knowledgeProduction += _settlement.Scientists;

            // Производство от зданий
            foreach (var building in _settlement.Buildings.Where(b => b.IsCompleted))
            {
                switch (building.Type)
                {
                    case BuildingType.School:
                        knowledgeProduction += building.GetEffect("KnowledgeProduction");
                        break;
                }
            }

            _settlement.AddResource(ResourceType.Food, foodProduction);
            _settlement.AddResource(ResourceType.Knowledge, knowledgeProduction);
            return productionPoints;
        }
        
        public void AddRandomResources()
        {
            // Положительное событие: добавление случайных ресурсов
            ResourceType resourceType = (ResourceType)_random.Next(0, Enum.GetValues(typeof(ResourceType)).Length);
            int amount = _random.Next(10, 31); // От 10 до 30 ресурсов
            
            _settlement.AddResource(resourceType, amount);
        }
        
        public void ReduceRandomResources()
        {
            // Отрицательное событие: уменьшение случайных ресурсов
            ResourceType resourceType = (ResourceType)_random.Next(0, Enum.GetValues(typeof(ResourceType)).Length);
            int amount = _random.Next(5, 21); // От 5 до 20 ресурсов
            
            // Убеждаемся, что не отнимаем больше, чем есть
            int currentAmount = _settlement.Resources[resourceType];
            int reduceAmount = Math.Min(currentAmount, amount);
            
            if (reduceAmount > 0)
            {
                _settlement.ConsumeResource(resourceType, reduceAmount);
            }
        }
    }
}