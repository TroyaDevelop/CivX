using System;
using System.Collections.Generic;
using Spectre.Console;
using CivilizationSimulation.Models;
using CivilizationSimulation.Controllers;
using System.Threading;

namespace CivilizationSimulation.Views
{
    public class MenuView : IView
    {
        private readonly GameController _gameController;
        private readonly Settlement _settlement;
        private DateTime _lastSimulationTime = DateTime.MinValue;
        private const int SimulationCooldownSeconds = 2;

        public MenuView(GameController gameController, Settlement settlement)
        {
            _gameController = gameController;
            _settlement = settlement;
        }

        public void Display()
        {
            while (true)
            {
                Clear();
                // Красивая шапка с иконками, рамкой и псевдо-выравниванием по центру
                var headerGrid = new Grid();
                headerGrid.AddColumn(new GridColumn().NoWrap());
                headerGrid.AddColumn(new GridColumn().NoWrap());
                headerGrid.AddColumn(new GridColumn().NoWrap());
                headerGrid.AddColumn(new GridColumn().NoWrap());
                headerGrid.AddColumn(new GridColumn().NoWrap());
                headerGrid.AddRow(
                    new Markup(":house: [bold]" + _settlement.Name + "[/]"),
                    new Markup(":busts_in_silhouette: [yellow]Население:[/] [white]" + _settlement.TotalPopulation + "[/]"),
                    new Markup(":carrot: [green]Пища:[/] [white]" + _settlement.Resources[ResourceType.Food] + "[/]"),
                    new Markup(":books: [blue]Наука:[/] [white]" + _settlement.Resources[ResourceType.Knowledge] + "[/]"),
                    new Markup("[magenta]День:[/] [white]" + _settlement.Day + "[/]")
                );
                var headerPanel = new Panel(headerGrid)
                    .Border(BoxBorder.Double)
                    .Header("[bold]Информация о поселении[/]")
                    .Expand();            // Псевдо-центрирование: добавим пустые строки сверху
                AnsiConsole.WriteLine("\n\n");
                AnsiConsole.Write(headerPanel);
                AnsiConsole.WriteLine("\n");
                // Удалена старая панель с днем и населением

                var menuOptions = new List<string> {
                    "1. Исследовать технологии",
                    "2. Управление поселением",
                    "3. Управление населением",
                    "4. Подробная информация о поселении",
                    "5. Выход"
                };
                for (int i = 0; i < menuOptions.Count; i++)
                {
                    AnsiConsole.MarkupLine($"[white]{menuOptions[i]}[/]");
                }
                AnsiConsole.WriteLine();
                // Подсказки управления внизу
                AnsiConsole.MarkupLine("[grey]Навигация: 1-5 — выбрать действие, T — симулировать день, Esc — выход[/]");
                AnsiConsole.MarkupLine("[grey]Подсказки управления всегда внизу экрана[/]");
                // Ожидание ввода
                var key = Console.ReadKey(true);
                string? choice = null;
                switch (key.Key)
                {
                    case ConsoleKey.D1:
                    case ConsoleKey.NumPad1:
                        choice = menuOptions[0];
                        break;
                    case ConsoleKey.D2:
                    case ConsoleKey.NumPad2:
                        choice = menuOptions[1];
                        break;
                    case ConsoleKey.D3:
                    case ConsoleKey.NumPad3:
                        choice = menuOptions[2];
                        break;
                    case ConsoleKey.D4:
                    case ConsoleKey.NumPad4:
                        choice = menuOptions[3];
                        break;
                    case ConsoleKey.D5:
                    case ConsoleKey.NumPad5:
                        choice = menuOptions[4];
                        break;
                    case ConsoleKey.T:
                        TimeSpan timeSinceLastSimulation = DateTime.Now - _lastSimulationTime;
                        if (timeSinceLastSimulation.TotalSeconds < SimulationCooldownSeconds)
                        {
                            int waitSeconds = SimulationCooldownSeconds - (int)timeSinceLastSimulation.TotalSeconds;
                            AnsiConsole.MarkupLine($"[yellow]Симуляция дня возможна через {waitSeconds} сек.[/]");
                            AnsiConsole.MarkupLine("[grey]Нажмите любую клавишу для продолжения...[/]");
                            Console.ReadKey(true);
                            break;
                        }
                        _lastSimulationTime = DateTime.Now;
                        AnsiConsole.Status().Start("Симуляция дня...", ctx => {
                            _gameController.SimulateDay();
                            Thread.Sleep(1000);
                        });
                        continue;
                    case ConsoleKey.Escape:
                        Environment.Exit(0);
                        break;
                }
                if (choice == null) continue;
                switch (choice)
                {
                    case "1. Исследовать технологии":
                        var techView = new TechnologyView(_gameController, _settlement);
                        techView.Display();
                        break;
                    case "2. Управление поселением":
                        var buildingView = new BuildingView(_gameController, _settlement);
                        buildingView.Display();
                        break;
                    case "3. Управление населением":
                        var populationView = new PopulationView(_gameController, _settlement);
                        populationView.Display();
                        break;
                    case "4. Подробная информация о поселении":
                        var settlementView = new SettlementView(_settlement);
                        settlementView.Display();
                        break;
                    case "5. Выход":
                        Environment.Exit(0);
                        break;
                }
            }
        }

        public void Clear()
        {
            AnsiConsole.Clear();
        }
    }
}