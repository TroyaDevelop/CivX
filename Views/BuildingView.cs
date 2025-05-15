using System;
using System.Collections.Generic;
using System.Linq;
using Spectre.Console;
using CivilizationSimulation.Models;
using CivilizationSimulation.Controllers;
using CivilizationSimulation.Utilities;

namespace CivilizationSimulation.Views
{
    public class BuildingView : IView
    {
        private readonly GameController _gameController;
        private readonly Settlement _settlement;

        public BuildingView(GameController gameController, Settlement settlement)
        {
            _gameController = gameController;
            _settlement = settlement;
        }

        public void Display()
        {
            Clear();
            AnsiConsole.Write(new Rule("[yellow]Управление поселением[/]").Centered());

            // Показываем ресурсы
            var resourceTable = new Table();
            resourceTable.Title = new TableTitle("Доступные ресурсы");
            resourceTable.AddColumn("Тип ресурса");
            resourceTable.AddColumn("Количество");
            foreach (var resource in _settlement.Resources)
            {
                resourceTable.AddRow(ResourceHelper.GetDisplayName(resource.Key), resource.Value.ToString());
            }
            AnsiConsole.Write(resourceTable);

            // Карта теперь основной интерфейс управления
            var mapView = new MapView(_settlement);
            mapView.Display();
        }

        public void Clear()
        {
            AnsiConsole.Clear();
        }
    }
}