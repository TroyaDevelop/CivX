using System;

namespace CivilizationSimulation.Models
{
    public enum Gender
    {
        Male,
        Female
    }

    public enum Profession
    {
        Worker,     // Рабочий — может добывать, строить, работать в зданиях
        Scientist   // Ученый — изучает технологии
    }

    public class Person
    {
        public Profession Profession { get; set; }
        public int WorkEfficiency { get; set; } = 10; // Сколько работы может выполнить за день

        public Person()
        {
            Profession = Profession.Worker; // По умолчанию все начинают рабочими
        }

        public void Work(Settlement settlement)
        {
            // Эта логика теперь перенесена в ResourceService,
            // но оставим метод, чтобы сохранить интерфейс класса для совместимости
        }

        public void Update()
        {
            // Каждый день всё население требует еды
            // Эта логика теперь управляется через PopulationService
        }
    }
}