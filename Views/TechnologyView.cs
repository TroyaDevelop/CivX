using System;
using System.Collections.Generic;
using System.Linq;
using Spectre.Console;
using CivilizationSimulation.Models;
using CivilizationSimulation.Controllers;

namespace CivilizationSimulation.Views
{
    public class TechnologyView : IView
    {
        private readonly GameController _gameController;
        private readonly Settlement _settlement;

        public TechnologyView(GameController gameController, Settlement settlement)
        {
            _gameController = gameController;
            _settlement = settlement;
        }

        public void Display()
        {
            while (true)
            {
                Clear();
                AnsiConsole.Write(new Rule("[yellow]Исследование технологий[/]").Centered());
                // Ресурсы
                var resourceTable = new Table();
                resourceTable.Title = new TableTitle("Доступные ресурсы");
                resourceTable.AddColumn("Тип ресурса");
                resourceTable.AddColumn("Количество");
                foreach (var resource in _settlement.Resources)
                {
                    resourceTable.AddRow(ResourceHelper.GetDisplayName(resource.Key), resource.Value.ToString());
                }
                AnsiConsole.Write(resourceTable);
                // Список технологий
                var techs = _settlement.Technologies
                    .Where(t => t.IsDiscovered || t.Prerequisites.All(pr => _settlement.HasTechnology(pr)))
                    .ToList();
                int selected = 0;
                bool exit = false;
                while (!exit)
                {
                    Console.Clear();
                    AnsiConsole.Write(new Rule("[yellow]Технологии[/]").Centered());
                    var table = new Table();
                    table.AddColumn("");
                    table.AddColumn("Название");
                    table.AddColumn("Статус");
                    table.AddColumn("Описание");
                    table.AddColumn("Требуемые ресурсы");
                    table.AddColumn("Требуемые технологии");
                    for (int i = 0; i < techs.Count; i++)
                    {
                        var tech = techs[i];
                        string selector = i == selected ? "[yellow]>[/]" : " ";
                        string status = tech.IsDiscovered ? "[green]Открыта[/]" : "[yellow]Не открыта[/]";
                        string cost = string.Join(", ", tech.Cost.Select(item => $"{ResourceHelper.GetDisplayName(item.Key)}: {item.Value}"));
                        string prerequisites = tech.Prerequisites.Count > 0 ? string.Join(", ", tech.Prerequisites) : "Нет";
                        table.AddRow(selector, tech.Name, status, tech.Description, cost, prerequisites);
                    }
                    AnsiConsole.Write(table);
                    AnsiConsole.MarkupLine("[grey]Стрелки — выбор, Enter — изучить, Esc — назад[/]");
                    var key = Console.ReadKey(true);
                    if (key.Key == ConsoleKey.UpArrow && selected > 0) selected--;
                    else if (key.Key == ConsoleKey.DownArrow && selected < techs.Count - 1) selected++;
                    else if (key.Key == ConsoleKey.Escape) { exit = true; break; }
                    else if (key.Key == ConsoleKey.Enter)
                    {
                        var tech = techs[selected];
                        if (tech.IsDiscovered)
                        {
                            AnsiConsole.MarkupLine("[grey]Технология уже изучена.[/]");
                            Thread.Sleep(700);
                            continue;
                        }
                        if (_gameController.ResearchTechnology(tech.Name))
                        {
                            AnsiConsole.MarkupLine($"[green]Технология {tech.Name} успешно исследована![/]");
                        }
                        else
                        {
                            AnsiConsole.MarkupLine($"[red]Не удалось исследовать технологию {tech.Name}.[/]");
                            AnsiConsole.MarkupLine("[red]Проверьте, достаточно ли ресурсов и выполнены ли все предварительные условия.[/]");
                        }
                        AnsiConsole.MarkupLine("[grey]Нажмите любую клавишу для продолжения...[/]");
                        Console.ReadKey();
                        break;
                    }
                }
                break;
            }
        }

        public void Clear()
        {
            AnsiConsole.Clear();
        }
    }
}