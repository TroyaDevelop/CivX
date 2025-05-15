using System;
using System.Collections.Generic;
using System.Linq;
using Spectre.Console;
using CivilizationSimulation.Models;
using CivilizationSimulation.Controllers;
using CivilizationSimulation.Utilities;

namespace CivilizationSimulation.Views
{
    public class PopulationView : IView
    {
        private readonly GameController _gameController;
        private readonly Settlement _settlement;

        public PopulationView(GameController gameController, Settlement settlement)
        {
            _gameController = gameController;
            _settlement = settlement;
        }

        public void Display()
        {
            bool exitView = false;
            int selected = 0; // 0 - рабочие, 1 - учёные
            int workers = _settlement.Workers;
            int scientists = _settlement.Scientists;
            int total = _settlement.TotalPopulation;
            // Подсчёт назначенных на тайлы рабочих
            int assignedToTiles = 0;
            if (_settlement.Map != null)
            {
                for (int x = 0; x < _settlement.Map.GetLength(0); x++)
                for (int y = 0; y < _settlement.Map.GetLength(1); y++)
                    assignedToTiles += _settlement.Map[x, y].AssignedWorkers;
            }
            // + рабочие в зданиях
            foreach (var b in _settlement.Buildings)
                assignedToTiles += b.AssignedWorkers;

            while (!exitView)
            {
                Clear();
                AnsiConsole.Write(new Rule("[yellow]Управление населением[/]").Centered());
                AnsiConsole.MarkupLine($"[bold]Всего жителей:[/] {total}");
                AnsiConsole.MarkupLine($"[grey]Назначено рабочих на тайлы/здания: {assignedToTiles}[/]");
                AnsiConsole.MarkupLine("[grey]Минимум рабочих: нельзя уменьшить ниже числа назначенных[/]");
                AnsiConsole.MarkupLine("");

                // Пересчёт свободных жителей
                int free = total - workers - scientists;
                if (free < 0)
                {
                    if (selected == 0) workers += free;
                    else if (selected == 1) scientists += free;
                    free = 0;
                }
                // Не даём уменьшить число рабочих ниже назначенных на тайлы
                if (workers < assignedToTiles) workers = assignedToTiles;

                // Отрисовка спиннера
                var table = new Table();
                table.Border(TableBorder.None);
                table.AddColumn("");
                table.AddColumn("");
                table.AddColumn("");
                // Рабочие
                string workerSelector = selected == 0 ? "[yellow]>[/]" : " ";
                string workerLabel = selected == 0 ? "[bold yellow]Рабочие[/]" : "Рабочие";
                string workerValue = workers == assignedToTiles
                    ? $"[grey]{workers} (мин)[/]" 
                    : $"[white]{workers}[/]";
                table.AddRow(workerSelector, workerLabel, workerValue);
                // Учёные
                string scientistSelector = selected == 1 ? "[yellow]>[/]" : " ";
                string scientistLabel = selected == 1 ? "[bold yellow]Учёные[/]" : "Учёные";
                table.AddRow(scientistSelector, scientistLabel, $"[white]{scientists}[/]");
                // Свободные жители (неинтерактивно)
                table.AddRow(" ", "[grey]Свободные жители[/]", $"[white]{free}[/]");
                AnsiConsole.Write(table);
                AnsiConsole.MarkupLine("[grey]Вверх/вниз — выбрать, Влево/вправо — изменить, Enter/Esc — сохранить и выйти[/]");

                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.UpArrow && selected > 0) selected--;
                else if (key.Key == ConsoleKey.DownArrow && selected < 1) selected++;
                else if (key.Key == ConsoleKey.LeftArrow)
                {
                    if (selected == 0 && workers > assignedToTiles)
                    {
                        workers--;
                    }
                    else if (selected == 1 && scientists > 0)
                    {
                        scientists--;
                    }
                }
                else if (key.Key == ConsoleKey.RightArrow)
                {
                    if (selected == 0 && free > 0)
                    {
                        workers++;
                    }
                    else if (selected == 1 && free > 0)
                    {
                        scientists++;
                    }
                }
                else if (key.Key == ConsoleKey.Enter || key.Key == ConsoleKey.Escape)
                {
                    // Применить изменения
                    _settlement.Workers = workers;
                    _settlement.Scientists = scientists;
                    AnsiConsole.MarkupLine($"[green]Распределение обновлено: рабочих — {workers}, учёных — {scientists}, свободных — {free}[/]");
                    AnsiConsole.MarkupLine("\n[grey]Нажмите любую клавишу для возврата...[/]");
                    Console.ReadKey();
                    exitView = true;
                }
            }
        }

        public void Clear()
        {
            AnsiConsole.Clear();
        }
    }
}