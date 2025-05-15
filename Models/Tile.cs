using System.Collections.Generic;

namespace CivilizationSimulation.Models
{
    public class Tile
    {
        public int X { get; set; }
        public int Y { get; set; }
        public TerrainType Terrain { get; set; }
        public List<Resource> Resources { get; set; } = new List<Resource>();

        public Tile(int x, int y, TerrainType terrain)
        {
            X = x;
            Y = y;
            Terrain = terrain;
        }

        public string GetTerrainSymbol() => Terrain switch
        {
            TerrainType.Grass => "G",
            TerrainType.Forest => "F",
            TerrainType.Stone => "S",
            TerrainType.Water => "W",
            _ => "?"
        };

        public string GetColorHex() => Terrain switch
        {
            TerrainType.Grass => "#90EE90",
            TerrainType.Forest => "#228B22",
            TerrainType.Stone => "#CD853F",
            TerrainType.Water => "#1E90FF",
            _ => "#FFFFFF"
        };
    }
}
