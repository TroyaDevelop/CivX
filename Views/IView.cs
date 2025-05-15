using System;
using CivilizationSimulation.Models;

namespace CivilizationSimulation.Views
{
    public interface IView
    {
        void Display();
        void Clear();
    }
}