using System;
using System.Collections.Generic;

namespace Shared
{
    [Serializable]
    public class Grid
    {
        public string Name;
        public List<District> Districts = new List<District>();

        public Grid(string name)
        {
            Name = name;
        }
    }
}
