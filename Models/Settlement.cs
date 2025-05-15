using System;
using System.Collections.Generic;
using System.Linq;

namespace CivilizationSimulation.Models
{
    public enum TerrainType
    {
        Grass,
        Forest,
        Stone,
        Water
    }

    public class MapCell
    {
        public TerrainType Terrain { get; set; }
        public int AssignedWorkers { get; set; } // Число назначенных рабочих
        public int MaxWorkers { get; set; } = 1; // Можно расширить по типу тайла
    }

    public class Settlement
    {
        public string Name { get; set; }
        public int Day { get; set; } = 1;
        public int MaxPopulation { get; private set; } = 3;
        public int MaxBuildingSlots { get; set; } = 5;
        public int FoodGrowthThreshold { get; set; } = 10; // Порог еды для роста населения
        public int TotalPopulation { get; set; }
        public int Workers { get; set; }
        public int Scientists { get; set; }
        public List<Building> Buildings { get; private set; }
        public List<Technology> Technologies { get; private set; }
        public Dictionary<ResourceType, int> Resources { get; private set; }
        public MapCell[,]? Map { get; private set; }
        public int MapSize { get; } = 7;

        public Settlement(string name)
        {
            Name = name;
            TotalPopulation = 1;
            Workers = 1;
            Scientists = 0;
            Buildings = new List<Building>();
            Technologies = TechnologyFactory.CreateAllTechnologies();
            Resources = new Dictionary<ResourceType, int>();
            foreach (ResourceType resourceType in Enum.GetValues(typeof(ResourceType)))
            {
                Resources[resourceType] = 0;
            }
            var foodType = ResourceType.Food;
            var knowledgeType = ResourceType.Knowledge;
            Resources[foodType] = 0;
            Resources[knowledgeType] = 0;
            FoodGrowthThreshold = 10;
            GenerateMap();
        }

        public void AddResource(ResourceType type, int amount)
        {
            if (Resources.ContainsKey(type))
            {
                Resources[type] += amount;
            }
        }

        public bool HasResource(ResourceType type, int amount)
        {
            return Resources.ContainsKey(type) && Resources[type] >= amount;
        }

        public bool ConsumeResource(ResourceType type, int amount)
        {
            if (HasResource(type, amount))
            {
                Resources[type] -= amount;
                return true;
            }
            return false;
        }

        public Technology? GetTechnology(string name)
        {
            return Technologies.FirstOrDefault(t => t.Name == name);
        }

        public bool HasTechnology(string name)
        {
            var tech = GetTechnology(name);
            return tech != null && tech.IsDiscovered;
        }

        public Building? GetBuilding(string name)
        {
            return Buildings.FirstOrDefault(b => b.Name == name);
        }

        public void UpdateMaxPopulation()
        {
            MaxPopulation = 3;
            foreach (var building in Buildings.Where(b => b.IsCompleted))
            {
                if (building.Type == BuildingType.Barn)
                {
                    MaxPopulation += building.GetEffect("MaxPopulation");
                }
            }
        }

        public int GetActiveBuildingSlots() => Buildings.Count(b => !b.IsCompleted);

        public void GenerateMap()
        {
            var rnd = new Random();
            Map = new MapCell[MapSize, MapSize];
            for (int x = 0; x < MapSize; x++)
            for (int y = 0; y < MapSize; y++)
            {
                TerrainType t = (TerrainType)rnd.Next(0, 3); // Grass, Forest, Stone
                Map[x, y] = new MapCell { Terrain = t, AssignedWorkers = 0, MaxWorkers = 1 };
            }
        }

        public bool AssignWorkerToCell(int x, int y, int workers)
        {
            if (Map != null && Map[x, y].AssignedWorkers + workers <= Map[x, y].MaxWorkers)
            {
                Map[x, y].AssignedWorkers += workers;
                return true;
            }
            return false;
        }

        public void RemoveWorkerFromCell(int x, int y, int workers)
        {
            if (Map != null && Map[x, y].AssignedWorkers >= workers)
                Map[x, y].AssignedWorkers -= workers;
        }

        public void CheckPopulationGrowth()
        {
            while (Resources[ResourceType.Food] >= FoodGrowthThreshold)
            {
                if (TotalPopulation < MaxPopulation)
                {
                    Resources[ResourceType.Food] -= FoodGrowthThreshold;
                    TotalPopulation++;
                    Workers++;
                    FoodGrowthThreshold = (TotalPopulation) * 10;
                }
                else
                {
                    break;
                }
            }
        }
    }
}