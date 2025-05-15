using CivilizationSimulation.Models;

namespace CivilizationSimulation.Utilities
{
    public static class TranslationHelper
    {
        public static string TranslateProfession(Profession profession)
        {
            return profession switch
            {
                Profession.Scientist => "Ученый",
                Profession.Worker => "Рабочий",
                _ => profession.ToString()
            };
        }

        public static string TranslateBuildingType(BuildingType type)
        {
            return type switch
            {
                BuildingType.Field => "Поле",
                BuildingType.School => "Школа",
                BuildingType.Barn => "Амбар",
                _ => type.ToString()
            };
        }
    }
}