﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace MusicGrid
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
