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

                var menuOptions = new SelectionPrompt<string>()
                    .Title("Выберите действие:")
                    .PageSize(10)
                    .AddChoices(new[] {
                        "1. Симулировать день",
                        "2. Исследовать технологии", 
                        "3. Управление поселением",
                        "4. Управление населением",
                        "5. Подробная информация о поселении",
                        "6. Выход"
                    });
                string choice;
                choice = AnsiConsole.Prompt(menuOptions);
                switch (choice)
                {
                    case "1. Симулировать день":
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
                        AnsiConsole.Status()
                            .Start("Симуляция дня...", ctx => {
                                _gameController.SimulateDay();
                                System.Threading.Thread.Sleep(1000);
                            });
                        break;
                    case "2. Исследовать технологии":
                        var techView = new TechnologyView(_gameController, _settlement);
                        techView.Display();
                        break;
                    case "3. Управление поселением":
                        var buildingView = new BuildingView(_gameController, _settlement);
                        buildingView.Display();
                        break;
                    case "4. Управление населением":
                        var populationView = new PopulationView(_gameController, _settlement);
                        populationView.Display();
                        break;
                    case "5. Подробная информация о поселении":
                        var settlementView = new SettlementView(_settlement);
                        settlementView.Display();
                        break;
                    case "6. Выход":
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