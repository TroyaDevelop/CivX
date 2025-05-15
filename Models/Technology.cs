using System;
using System.Collections.Generic;

namespace CivilizationSimulation.Models
{
    public class Technology
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsDiscovered { get; private set; }
        public Dictionary<ResourceType, int> Cost { get; set; }
        public List<string> Prerequisites { get; set; }

        public Technology(string name, string description)
        {
            Name = name;
            Description = description;
            IsDiscovered = false;
            Cost = new Dictionary<ResourceType, int>();
            Prerequisites = new List<string>();
        }

        public void Discover()
        {
            IsDiscovered = true;
        }

        public bool ArePrerequisitesMet(IEnumerable<Technology> discoveredTechnologies)
        {
            if (Prerequisites.Count == 0)
                return true;

            foreach (var prerequisite in Prerequisites)
            {
                bool found = false;
                foreach (var tech in discoveredTechnologies)
                {
                    if (tech.Name == prerequisite && tech.IsDiscovered)
                    {
                        found = true;
                        break;
                    }
                }
                
                if (!found)
                    return false;
            }
            
            return true;
        }
    }
}