using System;
using System.Collections.Generic;

namespace CivilizationSimulation.Models
{
    /// <summary>
    /// Фабрика для создания технологий
    /// </summary>
    public static class TechnologyFactory
    {
        /// <summary>
        /// Создает список всех доступных технологий в игре
        /// </summary>
        /// <returns>Список технологий</returns>
        public static List<Technology> CreateAllTechnologies()
        {
            var technologies = new List<Technology>();
            
            // Базовые технологии
            var agriculture = new Technology("Земледелие", "Позволяет выращивать пищу вместо собирательства");
            agriculture.Cost[ResourceType.Knowledge] = 10;

            var pottery = new Technology("Гончарное дело", "Позволяет строить амбар для хранения большего количества еды");
            pottery.Cost[ResourceType.Knowledge] = 20;
            
            // Технологии второго уровня
            var writing = new Technology("Письменность", "Значительно ускоряет получение знаний");
            writing.Cost[ResourceType.Knowledge] = 120;
            writing.Prerequisites.Add("Гончарное дело");
            
            // Добавление только нужных технологий
            technologies.Add(agriculture);
            technologies.Add(pottery);
            technologies.Add(writing);
            
            return technologies;
        }
    }
}