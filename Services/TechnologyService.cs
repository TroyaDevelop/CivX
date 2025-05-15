using System;
using System.Collections.Generic;
using System.Linq;
using CivilizationSimulation.Models;

namespace CivilizationSimulation.Services
{
    public class TechnologyService
    {
        private readonly Settlement _settlement;

        public TechnologyService(Settlement settlement)
        {
            _settlement = settlement;
        }

        public bool Research(string technologyName)
        {
            var technology = _settlement.GetTechnology(technologyName);
            if (technology == null || technology.IsDiscovered)
                return false;

            // Проверка предпосылок для исследования
            if (!technology.ArePrerequisitesMet(_settlement.Technologies.Where(t => t.IsDiscovered)))
                return false;

            // Проверка наличия необходимых ресурсов
            foreach (var resourceCost in technology.Cost)
            {
                if (!_settlement.HasResource(resourceCost.Key, resourceCost.Value))
                    return false;
            }

            // Списание ресурсов
            foreach (var resourceCost in technology.Cost)
            {
                _settlement.ConsumeResource(resourceCost.Key, resourceCost.Value);
            }

            // Открытие технологии
            technology.Discover();
            return true;
        }

        public IEnumerable<Technology> GetAvailableTechnologies()
        {
            return _settlement.Technologies.Where(t => 
                !t.IsDiscovered && 
                t.ArePrerequisitesMet(_settlement.Technologies.Where(tech => tech.IsDiscovered)));
        }

        public bool CanResearch(string technologyName)
        {
            var technology = _settlement.GetTechnology(technologyName);
            if (technology == null || technology.IsDiscovered)
                return false;

            // Проверка предпосылок для исследования
            if (!technology.ArePrerequisitesMet(_settlement.Technologies.Where(t => t.IsDiscovered)))
                return false;

            // Проверка наличия необходимых ресурсов
            foreach (var resourceCost in technology.Cost)
            {
                if (!_settlement.HasResource(resourceCost.Key, resourceCost.Value))
                    return false;
            }

            return true;
        }
    }
}