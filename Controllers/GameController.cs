using System;
using System.Collections.Generic;
using CivilizationSimulation.Models;
using CivilizationSimulation.Services;

namespace CivilizationSimulation.Controllers
{
    public class GameController
    {
        private readonly Settlement _settlement;
        private readonly ResourceService _resourceService;
        private readonly PopulationService _populationService;
        private readonly TechnologyService _technologyService;
        private readonly BuildingService _buildingService;

        public GameController(Settlement settlement)
        {
            _settlement = settlement;
            _resourceService = new ResourceService(settlement);
            _populationService = new PopulationService(settlement);
            _technologyService = new TechnologyService(settlement);
            _buildingService = new BuildingService(settlement);
        }

        public void SimulateDay()
        {
            // Сначала генерация ресурсов и productionPoints
            int productionPoints = _resourceService.ProduceResources();

            // Затем потребление пищи населением
            _populationService.ConsumeFoodAndUpdateStatus();
            
            // Строительство зданий с учётом productionPoints
            _buildingService.ProgressBuilding(productionPoints);

            // Обслуживание зданий
            _buildingService.MaintainBuildings();

            // Обновление статусов поселения
            _settlement.UpdateMaxPopulation();
            // _settlement.UpdateMaxResources(); // Удалено
            
            // Удалён вызов RemoveDeadPeople
            // _populationService.RemoveDeadPeople();

            // Увеличение счётчика дней
            _settlement.Day++;
            
            // Случайные события (с некоторым шансом)
            if (new Random().NextDouble() < 0.1) // 10% шанс события в день
            {
                GenerateRandomEvent();
            }
        }

        public bool ResearchTechnology(string technologyName)
        {
            return _technologyService.Research(technologyName);
        }

        public bool StartBuilding(string buildingName, BuildingType type)
        {
            return _buildingService.StartConstruction(buildingName, type);
        }

        public bool UpgradeBuilding(string buildingName)
        {
            return _buildingService.UpgradeBuilding(buildingName);
        }

        // Удалить метод AssignProfession и все обращения к нему

        public void GenerateRandomEvent()
        {
            Random rnd = new Random();
            int eventType = rnd.Next(1, 4);

            switch (eventType)
            {
                case 1:
                    // Положительное событие
                    _resourceService.AddRandomResources();
                    break;
                case 2:
                    // Отрицательное событие
                    _resourceService.ReduceRandomResources();
                    break;
                case 3:
                    // Нейтральное событие (пока ничего не делаем)
                    break;
            }
        }

        public bool IsProfessionAvailable(Profession profession)
        {
            switch (profession)
            {
                case Profession.Scientist:
                case Profession.Worker:
                    return true;
                default:
                    return false;
            }
        }
    }
}