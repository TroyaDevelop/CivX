using System;

namespace CivilizationSimulation.Models
{
    public enum ResourceType
    {
        Food,      // Пища
        Knowledge  // Знания (используются для открытия технологий)
    }

    // Вспомогательный класс для перевода названий ресурсов
    public static class ResourceHelper
    {
        // Получение русского названия ресурса для отображения в интерфейсе
        public static string GetDisplayName(ResourceType resourceType)
        {
            return resourceType switch
            {
                ResourceType.Food => "Пища",
                ResourceType.Knowledge => "Знания",
                _ => resourceType.ToString()
            };
        }
    }

    public class Resource
    {
        public ResourceType Type { get; private set; }
        public int Amount { get; private set; }
        public int MaxAmount { get; set; }

        public Resource(ResourceType type, int initialAmount, int maxAmount)
        {
            Type = type;
            Amount = initialAmount;
            MaxAmount = maxAmount;
        }

        public void Add(int value)
        {
            Amount = Math.Min(Amount + value, MaxAmount);
        }

        public bool Remove(int value)
        {
            if (Amount >= value)
            {
                Amount -= value;
                return true;
            }
            return false;
        }

        public override string ToString()
        {
            return $"{Type}: {Amount}/{MaxAmount}";
        }
    }
}