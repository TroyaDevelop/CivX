using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using Spectre.Console;
using CivilizationSimulation.Models;
using CivilizationSimulation.Controllers;
using CivilizationSimulation.Services;
using CivilizationSimulation.Views;

namespace CivilizationSimulation
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            
            // Приветственный экран
            DisplayWelcomeScreen();
            
            // Создание нового поселения
            string settlementName = AnsiConsole.Ask<string>("Введите название для вашего нового поселения:");
            var settlement = new Settlement(settlementName);
            
            // Создание начальной популяции
            CreateInitialPopulation(settlement);
            
            // Создание контроллера игры
            var gameController = new GameController(settlement);
            
            // Создание главного меню
            var menuView = new MenuView(gameController, settlement);
            
            // Главный игровой цикл
            while (true)
            {
                menuView.Clear();
                
                // Удалён вывод таблицы ресурсов
                // (Было: создание resourceTable, заполнение и AnsiConsole.Write(resourceTable);)
                
                // Отображение меню и обработка выбора пользователя
                menuView.Display();
            }
        }
        
        static void DisplayWelcomeScreen()
        {
            AnsiConsole.Clear();
            
            AnsiConsole.Write(
                new FigletText("Симуляция цивилизации")
                    .Centered()
                    .Color(Color.Gold1)
            );
            
            AnsiConsole.Write(new Rule("[yellow]Добро пожаловать в мир Симуляции Цивилизации![/]").Centered());
            
            AnsiConsole.MarkupLine("\nВы начинаете с небольшого поселения и нескольких жителей.");
            AnsiConsole.MarkupLine("Ваша задача - развивать поселение, исследуя новые технологии и строя здания.");
            AnsiConsole.MarkupLine("Следите за запасами еды и материалов, чтобы ваше поселение могло процветать.");
            AnsiConsole.MarkupLine("\n[grey]Нажмите любую клавишу, чтобы начать...[/]");
            
            Console.ReadKey();
            AnsiConsole.Clear();
        }
        
        static void CreateInitialPopulation(Settlement settlement)
        {
            // Удалить вызов settlement.AddPerson(new Person());
        }
    }
}
