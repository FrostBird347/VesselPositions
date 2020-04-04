using System;
using System.Collections.Generic;
namespace VesselPositions
{
    public static class PlanetInfo
    {
        private static Dictionary<int, Planet> planets = new Dictionary<int, Planet>();
        private static Dictionary<string, int> planetNameToInt = new Dictionary<string, int>();

        private class Planet
        {
            public string name;
            public double timeToRotate;
            public double mass;
        }

        public static void Init()
        {
            //Kerbol
            planets[0] = new Planet();
            planets[0].name = "Kerbol";
            planets[0].timeToRotate = 432000;
            planets[0].mass = 1.757e28;
            //Kerbin
            planets[1] = new Planet();
            planets[1].name = "Kerbin";
            planets[1].timeToRotate = 21549;
            planets[1].mass = 5.292e22;
            //Mun
            planets[2] = new Planet();
            planets[2].name = "Mun";
            planets[2].timeToRotate = 138984;
            planets[2].mass = 9.760e20;
            //Minmus
            planets[3] = new Planet();
            planets[3].name = "Minmus";
            planets[3].timeToRotate = 40400;
            planets[3].mass = 2.646e19;
            //Moho
            planets[4] = new Planet();
            planets[4].name = "Moho";
            planets[4].timeToRotate = 1210000;
            planets[4].mass = 2.526e21;
            //Eve
            planets[5] = new Planet();
            planets[5].name = "Eve";
            planets[5].timeToRotate = 80500;
            planets[5].mass = 1.224e23;
            //Duna
            planets[6] = new Planet();
            planets[6].name = "Duna";
            planets[6].timeToRotate = 65518;
            planets[6].mass = 4.515e21;
            //Ike
            planets[7] = new Planet();
            planets[7].name = "Ike";
            planets[7].timeToRotate = 65518;
            planets[7].mass = 2.782e20;
            //Jool
            planets[8] = new Planet();
            planets[8].name = "Jool";
            planets[8].timeToRotate = 36000;
            planets[8].mass = 4.233e24;
            //Laythe
            planets[9] = new Planet();
            planets[9].name = "Laythe";
            planets[9].timeToRotate = 52981;
            planets[9].mass = 2.940e22;
            //Vall
            planets[10] = new Planet();
            planets[10].name = "Vall";
            planets[10].timeToRotate = 105962;
            planets[10].mass = 3.109e21;
            //Bop
            planets[11] = new Planet();
            planets[11].name = "Bop";
            planets[11].timeToRotate = 544507;
            planets[11].mass = 3.726e19;
            //Tylo
            planets[12] = new Planet();
            planets[12].name = "Tylo";
            planets[12].timeToRotate = 211926;
            planets[12].mass = 4.233e22;
            //Gilly
            planets[13] = new Planet();
            planets[13].name = "Gilly";
            planets[13].timeToRotate = 28255;
            planets[13].mass = 1.242e17;
            //Pol
            planets[14] = new Planet();
            planets[14].name = "Pol";
            planets[14].timeToRotate = 901903;
            planets[14].mass = 1.081e19;
            //Dres
            planets[15] = new Planet();
            planets[15].name = "Dres";
            planets[15].timeToRotate = 34800;
            planets[15].mass = 3.219e20;
            //Eeloo
            planets[16] = new Planet();
            planets[16].name = "Eeloo";
            planets[16].timeToRotate = 19460;
            planets[16].mass = 1.115e21;

            BuildPlanetList();
        }

        private static void BuildPlanetList()
        {
            foreach (KeyValuePair<int, Planet> kvp in planets)
            {
                planetNameToInt.Add(kvp.Value.name, kvp.Key);
            }
        }

        /// <summary>
        /// Returns in radians
        /// </summary>
        public static double GetRotation(int index, double time)
        {
            if (!planets.ContainsKey(index))
            {
                return 0;
            }
            return 2 * Math.PI * ((time / planets[index].timeToRotate) % 1d);
        }

        public static double GetGM(int index)
        {
            if (!planets.ContainsKey(index))
            {
                return 0;
            }
            return planets[index].mass * 6.67408e-11;
        }

        public static int GetReference(string name)
        {
            if (!planetNameToInt.ContainsKey(name))
            {
                return -1;
            }
            return planetNameToInt[name];
        }

        public static string GetName(int id)
        {
            if (!planets.ContainsKey(id))
            {
                return null;
            }
            return planets[id].name;
        }
    }
}
