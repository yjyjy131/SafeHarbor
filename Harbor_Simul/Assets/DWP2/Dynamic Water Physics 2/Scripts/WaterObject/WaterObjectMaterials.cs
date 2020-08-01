using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DWP2
{
    public static class WaterObjectMaterials
    {
        public struct WaterObjectMaterial
        {
            public WaterObjectMaterial(string name, float density)
            {
                this.name = name;
                this.density = density;
            }
        
            public string name;
            public float density;
        }

        public static WaterObjectMaterial[] Materials =
        {
            new WaterObjectMaterial("Custom", -1),
            new WaterObjectMaterial("Bottle (Empty)", 80),
            new WaterObjectMaterial("Wooden Crate (Empty)", 90),
            new WaterObjectMaterial("Cork", 120),
            new WaterObjectMaterial("Steel Barrel (Empty)", 140),
            new WaterObjectMaterial("Wooden Barrel (Empty)", 230),
            new WaterObjectMaterial("Default Material", 400),
            new WaterObjectMaterial("Wood (Pine)", 440),
            new WaterObjectMaterial("Snow", 560),
            new WaterObjectMaterial("Wood (Oak)", 600),
            new WaterObjectMaterial("Ice", 920),
            new WaterObjectMaterial("Oil", 920),
            new WaterObjectMaterial("Rubber (Solid)", 1200),
            new WaterObjectMaterial("Plastic (Solid)", 1200),
            new WaterObjectMaterial("Sand", 1500),
            new WaterObjectMaterial("Brick", 1800),
            new WaterObjectMaterial("Concrete", 2300),
            new WaterObjectMaterial("Glass", 2700),
            new WaterObjectMaterial("Aluminum", 2700),
            new WaterObjectMaterial("Titanium", 4500),
            new WaterObjectMaterial("Steel", 7800),
            new WaterObjectMaterial("Copper", 8300),
            new WaterObjectMaterial("Lead", 11300),
        };

        public static string[] MaterialNames
        {
            get { return Materials.Select(m => m.name).ToArray(); }
        }
    }
}

