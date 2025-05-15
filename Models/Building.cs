using System;
using System.Collections.Generic;

namespace CivilizationSimulation.Models
{
    public enum BuildingType
    {
        Field,      // Поле - увеличивает производство пищи
        School,     // Школа - увеличивает производство знаний
        Barn        // Амбар - увеличивает только макс. хранилище еды
    }

    public class Building
    {
        public string Name { get; set; }
        public BuildingType Type { get; private set; }
        public int Level { get; private set; } = 1;
        public bool IsCompleted { get; private set; } = false;
        public int BuildProgress { get; private set; } = 0;
        public int RequiredProductionPoints { get; private set; } // Сколько очков нужно для завершения
        public Dictionary<ResourceType, int> MaintenanceCost { get; private set; }
        public Dictionary<ResourceType, int> BuildCost { get; private set; }
        public Dictionary<ResourceType, int> UpgradeCost { get; private set; }
        public Dictionary<string, int> Effects { get; private set; }
        // --- Новый подход: здание хранит только число назначенных рабочих ---
        public int AssignedWorkers { get; set; } = 0; // Число рабочих на здании
        public virtual int MaxWorkers => Type == BuildingType.Field ? 1 : 0;
        public virtual bool CanAssignWorkers => MaxWorkers > 0;
        public int ProductionPoints { get; private set; } = 0; // Очки производства (скрытые)

        public Building(string name, BuildingType type)
        {
            Name = name;
            Type = type;
            MaintenanceCost = new Dictionary<ResourceType, int>();
            BuildCost = new Dictionary<ResourceType, int>();
            UpgradeCost = new Dictionary<ResourceType, int>();
            Effects = new Dictionary<string, int>();
            switch (Type)
            {
                case BuildingType.Field:
                    RequiredProductionPoints = 5;
                    break;
                case BuildingType.School:
                    RequiredProductionPoints = 40;
                    break;
                case BuildingType.Barn:
                    RequiredProductionPoints = 15;
                    break;
                default:
                    RequiredProductionPoints = 20;
                    break;
            }
            InitializeBuilding();
        }

        private void InitializeBuilding()
        {
            switch (Type)
            {
                case BuildingType.Field:
                    Effects["FoodProduction"] = 2; // +2 к производству пищи
                    break;
                case BuildingType.School:
                    BuildCost[ResourceType.Knowledge] = 20;
                    UpgradeCost[ResourceType.Knowledge] = 40;
                    Effects["KnowledgeProduction"] = 2; // +2 к производству знаний
                    break;
                case BuildingType.Barn:
                    BuildCost[ResourceType.Knowledge] = 30;
                    Effects["MaxPopulation"] = 2; // +2 к максимуму жителей
                    break;
            }
        }

        public void AddProgress(int progress)
        {
            if (IsCompleted) return;
            AddProductionPoints(progress);
        }

        public bool Upgrade()
        {
            if (!IsCompleted) return false;
            
            Level++;
            
            // Увеличиваем эффекты здания в зависимости от уровня
            foreach (var effect in Effects.Keys.ToList())
            {
                Effects[effect] = (int)(Effects[effect] * 1.5);
            }
            
            return true;
        }
        
        public int GetEffect(string effectName)
        {
            if (Effects.ContainsKey(effectName) && IsCompleted)
                return Effects[effectName];
            return 0;
        }

        // Назначить рабочего (теперь просто увеличивает счетчик)
        public bool AssignWorker()
        {
            if (!CanAssignWorkers || !IsCompleted) return false;
            if (AssignedWorkers >= MaxWorkers) return false;
            AssignedWorkers++;
            return true;
        }

        // Снять рабочего
        public bool RemoveWorker()
        {
            if (AssignedWorkers > 0) { AssignedWorkers--; return true; }
            return false;
        }

        // Получить количество назначенных работников
        public int GetAssignedWorkerCount() => AssignedWorkers;

        public void AddProductionPoints(int points)
        {
            if (IsCompleted) return;
            ProductionPoints += points;
            // Пересчитываем BuildProgress в проценты
            BuildProgress = (int)(100.0 * ProductionPoints / RequiredProductionPoints);
            if (ProductionPoints >= RequiredProductionPoints)
            {
                IsCompleted = true;
                BuildProgress = 100;
            }
        }
    }
}