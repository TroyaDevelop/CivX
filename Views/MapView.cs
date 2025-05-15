using System;
using Spectre.Console;
using CivilizationSimulation.Models;
using CivilizationSimulation.Controllers;

namespace CivilizationSimulation.Views
{
    public class MapView : IView
    {
        private readonly Settlement _settlement;
        private readonly int _mapSize;
        private readonly GameController _gameController;
        private int _selectedX = 0;
        private int _selectedY = 0;

        public MapView(GameController gameController, Settlement settlement)
        {
            _gameController = gameController;
            _settlement = settlement;
            _mapSize = settlement.MapSize;
        }

        public void Display()
        {
            bool exit = false;
            DateTime lastSimulationTime = DateTime.MinValue;
            const int SimulationCooldownSeconds = 2;
            while (!exit)
            {
                Clear();
                var grid = new Grid();
                grid.AddColumn(new GridColumn().Width(30));
                grid.AddColumn(new GridColumn().Width(40));
                grid.AddRow(RenderMapBlock(), DrawTileInfo());
                AnsiConsole.Write(grid);
                AnsiConsole.WriteLine();
                // Подсказки управления внизу
                AnsiConsole.MarkupLine("[grey]Стрелки — навигация, Enter — действия, T — симулировать день, Esc — выход[/]");
                var key = Console.ReadKey(true).Key;
                switch (key)
                {
                    case ConsoleKey.UpArrow: if (_selectedY > 0) _selectedY--; break;
                    case ConsoleKey.DownArrow: if (_selectedY < _mapSize - 1) _selectedY++; break;
                    case ConsoleKey.LeftArrow: if (_selectedX > 0) _selectedX--; break;
                    case ConsoleKey.RightArrow: if (_selectedX < _mapSize - 1) _selectedX++; break;
                    case ConsoleKey.Enter:
                        ShowTileActionMenu();
                        break;
                    case ConsoleKey.T:
                        TimeSpan timeSinceLastSimulation = DateTime.Now - lastSimulationTime;
                        if (timeSinceLastSimulation.TotalSeconds < SimulationCooldownSeconds)
                        {
                            int waitSeconds = SimulationCooldownSeconds - (int)timeSinceLastSimulation.TotalSeconds;
                            AnsiConsole.MarkupLine($"[yellow]Симуляция дня возможна через {waitSeconds} сек.[/]");
                            AnsiConsole.MarkupLine("[grey]Нажмите любую клавишу для продолжения...[/]");
                            Console.ReadKey(true);
                            break;
                        }
                        lastSimulationTime = DateTime.Now;
                        AnsiConsole.Status().Start("Симуляция дня...", ctx => {
                            _gameController.SimulateDay();
                            System.Threading.Thread.Sleep(1000);
                        });
                        break;
                    case ConsoleKey.Escape: exit = true; break;
                }
            }
        }

        private void ShowTileActionMenu()
        {
            var cell = _settlement.Map![_selectedX, _selectedY];
            // Проверяем, есть ли уже здание на этом тайле
            var building = _settlement.Buildings.FirstOrDefault(b => b.Name == $"Поле ({_selectedX},{_selectedY})" || b.Name == $"Амбар ({_selectedX},{_selectedY})");
            if (building != null)
            {
                var actions = new List<string>();
                if (building.CanAssignWorkers && building.IsCompleted)
                {
                    string actionLabel = building.GetAssignedWorkerCount() > 0 ? "Снять рабочего" : "Назначить рабочего";
                    actions.Add(actionLabel);
                }
                var action = AnsiConsole.Prompt(new SelectionPrompt<string>()
                    .Title($"Действие для тайла {_selectedX},{_selectedY} ({building.Name})")
                    .AddChoices(actions.Count > 0 ? actions : new[] { "Назад" }));
                if (action == "Назначить рабочего")
                {
                    if (_settlement.Workers > 0 && building.GetAssignedWorkerCount() < building.MaxWorkers)
                    {
                        building.AssignWorker();
                        AnsiConsole.MarkupLine("[green]Рабочий назначен![/]");
                    }
                    else
                    {
                        AnsiConsole.MarkupLine("[red]Нет доступных рабочих или достигнут лимит![/]");
                    }
                    System.Threading.Thread.Sleep(900);
                }
                else if (action == "Снять рабочего")
                {
                    if (building.GetAssignedWorkerCount() > 0)
                    {
                        building.RemoveWorker();
                        AnsiConsole.MarkupLine("[yellow]Рабочий снят![/]");
                    }
                    else
                    {
                        AnsiConsole.MarkupLine("[red]Нет назначенных рабочих![/]");
                    }
                    System.Threading.Thread.Sleep(900);
                }
                return;
            }
            bool isFieldTile = (int)cell.Terrain == 1001;
            bool isBarnTile = (int)cell.Terrain == 1002;
            bool hasBuilding = _settlement.Buildings.Any(b => b.Name == $"Поле ({_selectedX},{_selectedY})" || b.Name == $"Амбар ({_selectedX},{_selectedY})");
            if (isFieldTile)
            {
                var buildingField = _settlement.GetBuilding($"Поле ({_selectedX},{_selectedY})");
                string actionLabel = (buildingField != null && buildingField.GetAssignedWorkerCount() > 0) ? "Снять рабочего с поля" : "Назначить рабочего на поле";
                var actions = new List<string> { actionLabel };
                var action = AnsiConsole.Prompt(new SelectionPrompt<string>()
                    .Title($"Действие для тайла {_selectedX},{_selectedY} (поле)")
                    .AddChoices(actions));
                if (action == "Назначить рабочего на поле")
                {
                    if (buildingField != null && _settlement.Workers > 0 && buildingField.GetAssignedWorkerCount() < buildingField.MaxWorkers)
                    {
                        buildingField.AssignWorker();
                        AnsiConsole.MarkupLine("[green]Рабочий назначен на поле![/]");
                    }
                    else
                    {
                        AnsiConsole.MarkupLine("[red]Нет доступных рабочих или достигнут лимит![/]");
                    }
                    System.Threading.Thread.Sleep(900);
                }
                else if (action == "Снять рабочего с поля")
                {
                    if (buildingField != null && buildingField.GetAssignedWorkerCount() > 0)
                    {
                        buildingField.RemoveWorker();
                        AnsiConsole.MarkupLine("[yellow]Рабочий снят с поля![/]");
                    }
                    else
                    {
                        AnsiConsole.MarkupLine("[red]Нет назначенных рабочих на поле![/]");
                    }
                    System.Threading.Thread.Sleep(900);
                }
            }
            else if (cell.Terrain == TerrainType.Grass)
            {
                string actionLabel = cell.AssignedWorkers > 0 ? "Снять рабочего" : "Назначить рабочего";
                var actions = new List<string> { actionLabel };
                if (!hasBuilding && (_settlement.HasTechnology("Земледелие") || _settlement.HasTechnology("Гончарное дело")))
                    actions.Add("Строительство");
                var action = AnsiConsole.Prompt(new SelectionPrompt<string>()
                    .Title($"Действие для тайла {_selectedX},{_selectedY} (луг)")
                    .AddChoices(actions));
                if (action == "Назначить рабочего")
                {
                    if (_settlement.Workers > 0 && cell.AssignedWorkers < cell.MaxWorkers)
                    {
                        cell.AssignedWorkers++;
                        AnsiConsole.MarkupLine("[green]Рабочий назначен на луг![/]");
                    }
                    else
                    {
                        AnsiConsole.MarkupLine("[red]Нет доступных рабочих или достигнут лимит![/]");
                    }
                    System.Threading.Thread.Sleep(900);
                }
                else if (action == "Снять рабочего")
                {
                    if (cell.AssignedWorkers > 0)
                    {
                        cell.AssignedWorkers--;
                        AnsiConsole.MarkupLine("[yellow]Рабочий снят с луга![/]");
                    }
                    else
                    {
                        AnsiConsole.MarkupLine("[red]Нет назначенных рабочих на лугу![/]");
                    }
                    System.Threading.Thread.Sleep(900);
                }
                else if (action == "Строительство")
                {
                    ShowBuildMenuOnTile();
                }
            }
            else if (cell.Terrain == TerrainType.Forest)
            {
                string actionLabel = cell.AssignedWorkers > 0 ? "Снять рабочего" : "Назначить рабочего";
                var actions = new List<string> { actionLabel };
                var action = AnsiConsole.Prompt(new SelectionPrompt<string>()
                    .Title($"Действие для тайла {_selectedX},{_selectedY} (лес)")
                    .AddChoices(actions));
                if (action == "Назначить рабочего")
                {
                    if (_settlement.Workers > 0 && cell.AssignedWorkers < cell.MaxWorkers)
                    {
                        cell.AssignedWorkers++;
                        AnsiConsole.MarkupLine("[green]Рабочий назначен в лес![/]");
                    }
                    else
                    {
                        AnsiConsole.MarkupLine("[red]Нет доступных рабочих или достигнут лимит![/]");
                    }
                    System.Threading.Thread.Sleep(900);
                }
                else if (action == "Снять рабочего")
                {
                    if (cell.AssignedWorkers > 0)
                    {
                        cell.AssignedWorkers--;
                        AnsiConsole.MarkupLine("[yellow]Рабочий снят из леса![/]");
                    }
                    else
                    {
                        AnsiConsole.MarkupLine("[red]Нет назначенных рабочих в лесу![/]");
                    }
                    System.Threading.Thread.Sleep(900);
                }
            }
            else
            {
                AnsiConsole.MarkupLine("[red]На этом тайле нельзя выполнять действия![/]");
                System.Threading.Thread.Sleep(900);
            }
        }

        private void ShowBuildMenuOnTile()
        {
            var available = new List<(string, BuildingType, Dictionary<ResourceType, int>, int)>();
            if (_settlement.HasTechnology("Земледелие"))
            {
                var b = new Building($"Поле ({_selectedX},{_selectedY})", BuildingType.Field);
                available.Add(("Поле", BuildingType.Field, b.BuildCost, b.RequiredProductionPoints));
            }
            if (_settlement.HasTechnology("Гончарное дело"))
            {
                var b = new Building($"Амбар ({_selectedX},{_selectedY})", BuildingType.Barn);
                available.Add(("Амбар", BuildingType.Barn, b.BuildCost, b.RequiredProductionPoints));
            }
            if (available.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]Нет доступных зданий для строительства![/]");
                System.Threading.Thread.Sleep(900);
                return;
            }
            int selected = 0;
            bool exit = false;
            while (!exit)
            {
                Console.Clear();
                var table = new Table();
                table.Title = new TableTitle("Доступные здания");
                table.AddColumn("");
                table.AddColumn("Здание");
                table.AddColumn("Цена");
                table.AddColumn("Очки производства");
                for (int i = 0; i < available.Count; i++)
                {
                    var (name, _, cost, prod) = available[i];
                    string selector = i == selected ? "[yellow]>[/]" : " ";
                    string costStr = string.Join(", ", cost.Select(c => $"{ResourceHelper.GetDisplayName(c.Key)}: {c.Value}"));
                    var row = new List<string> { selector, name, costStr, prod.ToString() };
                    if (i == selected)
                        table.AddRow(row.Select(s => $"[yellow]{s}[/]").ToArray());
                    else
                        table.AddRow(row.ToArray());
                }
                AnsiConsole.Write(table);
                AnsiConsole.MarkupLine("[grey]Стрелки — выбор, Enter — построить, Esc — отмена[/]");
                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.UpArrow && selected > 0) selected--;
                else if (key.Key == ConsoleKey.DownArrow && selected < available.Count - 1) selected++;
                else if (key.Key == ConsoleKey.Escape) return;
                else if (key.Key == ConsoleKey.Enter)
                {
                    var chosen = available[selected];
                    BuildOnTile(chosen.Item2);
                    return;
                }
            }
        }

        private void BuildOnTile(BuildingType type)
        {
            // Проверяем, нет ли уже здания на этом тайле
            bool hasBuilding = _settlement.Buildings.Any(b => b.Name == $"{(type == BuildingType.Field ? "Поле" : "Амбар")} ({_selectedX},{_selectedY})");
            if (hasBuilding)
            {
                AnsiConsole.MarkupLine("[red]На этом тайле уже есть здание![/]");
                System.Threading.Thread.Sleep(900);
                return;
            }
            string name = type == BuildingType.Field ? $"Поле ({_selectedX},{_selectedY})" : $"Амбар ({_selectedX},{_selectedY})";
            var building = new Building(name, type);
            _settlement.Buildings.Add(building);
            // Меняем тип тайла на специальный для здания
            var cell = _settlement.Map![_selectedX, _selectedY];
            if (type == BuildingType.Field)
                cell.Terrain = (TerrainType)1001; // Спец. FieldTile
            else if (type == BuildingType.Barn)
                cell.Terrain = (TerrainType)1002; // Спец. BarnTile
            AnsiConsole.MarkupLine($"[green]{name} добавлено в очередь строительства![/]");
            System.Threading.Thread.Sleep(900);
        }

        private Panel RenderMapBlock()
        {
            // Каждый тайл — 1 символ, между тайлами пробел для квадратности
            var mapLines = new string[_mapSize];
            for (int y = 0; y < _mapSize; y++)
            {
                string line = "";
                for (int x = 0; x < _mapSize; x++)
                {
                    var cell = _settlement.Map![x, y];
                    string symbol;
                    string color;
                    if ((int)cell.Terrain == 1001) // FieldTile
                    {
                        symbol = "F";
                        color = "#FFD700";
                    }
                    else if ((int)cell.Terrain == 1002) // BarnTile
                    {
                        symbol = "B";
                        color = "#8B4513";
                    }
                    else
                    {
                        symbol = cell.Terrain switch
                        {
                            TerrainType.Grass => ".",
                            TerrainType.Forest => "T",
                            TerrainType.Stone => "^",
                            TerrainType.Water => "~",
                            _ => "?"
                        };
                        color = cell.Terrain switch
                        {
                            TerrainType.Grass => "#90EE90",
                            TerrainType.Forest => "#228B22",
                            TerrainType.Stone => "#CD853F",
                            TerrainType.Water => "#1E90FF",
                            _ => "#FFFFFF"
                        };
                    }
                    bool selected = (x == _selectedX && y == _selectedY);
                    string wrap(string s) => selected ? $"[bold reverse {color}]{s}[/]" : $"[{color}]{s}[/]";
                    line += wrap(symbol) + " ";
                }
                mapLines[y] = line;
            }
            var mapText = string.Join("\n", mapLines);
            var panel = new Panel(new Markup(mapText))
                .Header("Карта")
                .Border(BoxBorder.Double)
                .Padding(2, 1, 2, 1);
            return panel;
        }

        private string TranslateTerrainType(TerrainType type)
        {
            return type switch
            {
                TerrainType.Grass => "Луг",
                TerrainType.Forest => "Лес",
                TerrainType.Stone => "Камни",
                TerrainType.Water => "Вода",
                _ => type.ToString()
            };
        }

        private Panel DrawTileInfo()
        {
            var cell = _settlement.Map![_selectedX, _selectedY];
            // Проверка: если это тайл здания (FieldTile или BarnTile)
            string infoText = "";
            if ((int)cell.Terrain == 1001 || (int)cell.Terrain == 1002)
            {
                // Найти здание по координатам
                string name = (int)cell.Terrain == 1001 ? $"Поле ({_selectedX},{_selectedY})" : $"Амбар ({_selectedX},{_selectedY})";
                var building = _settlement.GetBuilding(name);
                if (building != null)
                {
                    infoText += $"[bold]Координаты:[/] {_selectedX},{_selectedY}\n";
                    infoText += $"[bold]Здание:[/] {building.Name}\n";
                    infoText += $"[bold]Статус:[/] ";
                    if (building.IsCompleted)
                        infoText += "[green]Построено[/]\n";
                    else
                        infoText += $"[yellow]Строится ({building.BuildProgress}%)[/]\n";
                    if (building.Type == BuildingType.Field)
                        infoText += $"[bold]Работники:[/] {building.GetAssignedWorkerCount()}/{building.MaxWorkers}\n";
                    // Эффекты здания
                    if (building.IsCompleted)
                    {
                        if (building.Type == BuildingType.Field)
                            infoText += "[bold]Эффект:[/] +2 пищи в день\n";
                        if (building.Type == BuildingType.Barn)
                            infoText += "[bold]Эффект:[/] +200 к макс. запасу еды\n";
                    }
                }
                else
                {
                    infoText = $"[bold]Координаты:[/] {_selectedX},{_selectedY}\n[red]Здание не найдено![/]";
                }
            }
            else
            {
                infoText = $"[bold]Координаты:[/] {_selectedX},{_selectedY}\n[bold]Тип:[/] {TranslateTerrainType(cell.Terrain)}\n[bold]Рабочие:[/] {cell.AssignedWorkers}/{cell.MaxWorkers}";
            }
            return new Panel(new Markup(infoText)).Header("О клетке");
        }

        public void Clear() => AnsiConsole.Clear();
    }
}
