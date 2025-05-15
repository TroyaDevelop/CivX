using System;
using System.Collections.Generic;
using System.Linq;
using Spectre.Console;
using CivilizationSimulation.Models;
using CivilizationSimulation.Services;
using CivilizationSimulation.Utilities;

namespace CivilizationSimulation.Views
{
    public class SettlementView : IView
    {
        private readonly Settlement _settlement;
        private readonly ResourceService _resourceService;

        public SettlementView(Settlement settlement)
        {
            _settlement = settlement;
            _resourceService = new ResourceService(settlement);
        }

        public void Display()
        {
            Clear();
            
            // Ресурсы
            var resourceTable = new Table();
            resourceTable.Title = new TableTitle("Ресурсы поселения");
            resourceTable.AddColumn("Тип ресурса");
            resourceTable.AddColumn("Количество");
            // resourceTable.AddColumn("Максимально"); // Удалено
            resourceTable.AddColumn("Доходы");
            resourceTable.AddColumn("Расходы");
            resourceTable.AddColumn("Баланс");
            
            // Получаем доходы и расходы
            var (income, expense, balance) = CalculateResourceIncomeExpenseBalance();
            
            foreach (var resource in _settlement.Resources)
            {
                if (resource.Key == ResourceType.Food || resource.Key == ResourceType.Knowledge)
                {
                    string color = "white";
                    if (resource.Key == ResourceType.Food && resource.Value < _settlement.TotalPopulation * 5) 
                        color = "red";
                    if (resource.Key == ResourceType.Knowledge && resource.Value > 40)
                        color = "blue";
                    
                    int inc = income.ContainsKey(resource.Key) ? income[resource.Key] : 0;
                    int exp = expense.ContainsKey(resource.Key) ? expense[resource.Key] : 0;
                    int bal = balance.ContainsKey(resource.Key) ? balance[resource.Key] : 0;
                    string incColor = inc > 0 ? "green" : "grey";
                    string expColor = exp > 0 ? "red" : "grey";
                    string balColor = bal > 0 ? "green" : (bal < 0 ? "red" : "grey");
                    
                    resourceTable.AddRow(
                        ResourceHelper.GetDisplayName(resource.Key),
                        $"[{color}]{resource.Value}[/]",
                        // _settlement.MaxResources[resource.Key].ToString(), // Удалено
                        $"[{incColor}]+{inc}[/]",
                        $"[{expColor}]-{exp}[/]",
                        $"[{balColor}]{(bal >= 0 ? "+" : "")}{bal}[/]"
                    );
                }
            }
            AnsiConsole.Write(resourceTable);
            
            // Здания
            // if (_settlement.Buildings.Any())
            // {
            //     ...
            // }
            // Информация о профессиях
            // DisplayProfessionsDistribution();
            // Удалён вызов DisplayMap();
            AnsiConsole.MarkupLine("\n[grey]Нажмите любую клавишу для возврата...[/]");
            Console.ReadKey();
        }

        // Новый метод для подсчёта доходов, расходов и баланса
        private (Dictionary<ResourceType, int> income, Dictionary<ResourceType, int> expense, Dictionary<ResourceType, int> balance) CalculateResourceIncomeExpenseBalance()
        {
            var income = new Dictionary<ResourceType, int> { { ResourceType.Food, 0 }, { ResourceType.Knowledge, 0 } };
            var expense = new Dictionary<ResourceType, int> { { ResourceType.Food, 0 }, { ResourceType.Knowledge, 0 } };
            var balance = new Dictionary<ResourceType, int> { { ResourceType.Food, 0 }, { ResourceType.Knowledge, 0 } };

            // Доходы: поля (Field)
            var completedFields = _settlement.Buildings.Where(b => b.IsCompleted && b.Type == BuildingType.Field).ToList();
            foreach (var field in completedFields)
            {
                if (field.AssignedWorkers > 0)
                {
                    income[ResourceType.Food] += field.GetEffect("FoodProduction") * field.AssignedWorkers;
                }
            }
            // Базовый доход пищи
            income[ResourceType.Food] += 2; // базовый доход
            // Доходы: рабочие на карте
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
                                income[ResourceType.Food] += cell.AssignedWorkers;
                                break;
                        }
                    }
                }
            }
            // Доходы: учёные
            income[ResourceType.Knowledge] += _settlement.Scientists;
            // Доходы: здания
            foreach (var building in _settlement.Buildings.Where(b => b.IsCompleted))
            {
                switch (building.Type)
                {
                    case BuildingType.School:
                        income[ResourceType.Knowledge] += building.GetEffect("KnowledgeProduction");
                        break;
                }
            }
            // Расходы: еда на каждого жителя
            int foodPerPerson = 2;
            expense[ResourceType.Food] = _settlement.TotalPopulation * foodPerPerson;
            // Баланс
            balance[ResourceType.Food] = income[ResourceType.Food] - expense[ResourceType.Food];
            balance[ResourceType.Knowledge] = income[ResourceType.Knowledge] - (expense.ContainsKey(ResourceType.Knowledge) ? expense[ResourceType.Knowledge] : 0);
            return (income, expense, balance);
        }
        
        private Dictionary<ResourceType, int> CalculateResourceProduction()
        {
            var production = new Dictionary<ResourceType, int>
            {
                { ResourceType.Food, 0 },
                { ResourceType.Knowledge, 0 }
            };
            
            // Сначала учитываем работников на полях (фермах)
            var completedFields = _settlement.Buildings.Where(b => b.IsCompleted && b.Type == BuildingType.Field).ToList();
            foreach (var field in completedFields)
            {
                if (field.AssignedWorkers > 0)
                {
                    production[ResourceType.Food] += field.GetEffect("FoodProduction") * field.AssignedWorkers;
                }
            }
            
            // Доходы: учёные
            production[ResourceType.Knowledge] += _settlement.Scientists;
            
            // Добавляем производство от зданий
            foreach (var building in _settlement.Buildings.Where(b => b.IsCompleted))
            {
                switch (building.Type)
                {
                    case BuildingType.School:
                        production[ResourceType.Knowledge] += building.GetEffect("KnowledgeProduction");
                        break;
                }
            }
            
            // --- Производство с рабочих на карте ---
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
                                production[ResourceType.Food] += cell.AssignedWorkers;
                                break;
                            // Лес теперь не даёт материалов, а даёт productionPoints (см. ResourceService)
                        }
                    }
                }
            }
            
            // Вычитаем потребление еды
            int foodPerPerson = 4; // Среднее потребление на человека
            production[ResourceType.Food] -= _settlement.TotalPopulation * foodPerPerson;
            
            return production;
        }

        private void DisplayMap()
        {
            if (_settlement.Map == null)
            {
                AnsiConsole.MarkupLine("[red]Карта не сгенерирована![/]");
                return;
            }
            AnsiConsole.Write(new Rule("[yellow]Карта местности[/]").Centered());
            for (int y = 0; y < _settlement.MapSize; y++)
            {
                string row = "";
                for (int x = 0; x < _settlement.MapSize; x++)
                {
                    var cell = _settlement.Map[x, y];
                    string symbol = cell.Terrain switch
                    {
                        TerrainType.Grass => "[green].[/]",
                        TerrainType.Forest => "[olive]♣[/]",
                        TerrainType.Stone => "[grey]■[/]",
                        _ => "."
                    };
                    if (cell.AssignedWorkers > 0)
                        symbol = "[blue]@[/]";
                    row += symbol + " ";
                }
                AnsiConsole.MarkupLine(row);
            }
        }

        public void Clear()
        {
            AnsiConsole.Clear();
        }
    }
}