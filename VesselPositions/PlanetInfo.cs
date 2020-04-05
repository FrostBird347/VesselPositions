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
            public double initialRotation;
            public double timeToRotate;
            public double mass;
            public double radius;
        }

        public static void Init()
        {
            //Kerbol
            planets[0] = new Planet();
            planets[0].name = "Sun";
            planets[0].initialRotation = 0;
            planets[0].timeToRotate = 432000;
            planets[0].mass = 1.75654591319326E+28;
            planets[0].radius = 261600000;
            //Kerbin
            planets[1] = new Planet();
            planets[1].name = "Kerbin";
            planets[1].initialRotation = 90;
            planets[1].timeToRotate = 21549;
            planets[1].mass = 5.29151583439215E+22;
            planets[1].radius = 600000;
            //Mun
            planets[2] = new Planet();
            planets[2].name = "Mun";
            planets[2].initialRotation = 230;
            planets[2].timeToRotate = 138984;
            planets[2].mass = 9.7599066119646E+20;
            planets[2].radius = 200000;
            //Minmus
            planets[3] = new Planet();
            planets[3].name = "Minmus";
            planets[3].initialRotation = 230;
            planets[3].timeToRotate = 40400;
            planets[3].mass = 2.64575795662095E+19;
            planets[3].radius = 60000;
            //Moho
            planets[4] = new Planet();
            planets[4].name = "Moho";
            planets[4].initialRotation = 190;
            planets[4].timeToRotate = 1210000;
            planets[4].mass = 2.52633139930162E+21;
            planets[4].radius = 250000;
            //Eve
            planets[5] = new Planet();
            planets[5].name = "Eve";
            planets[5].initialRotation = 0;
            planets[5].timeToRotate = 80500;
            planets[5].mass = 1.2243980038014E+23;
            planets[5].radius = 700000;
            //Duna
            planets[6] = new Planet();
            planets[6].name = "Duna";
            planets[6].initialRotation = 90;
            planets[6].timeToRotate = 65518;
            planets[6].mass = 4.51542702477492E+21;
            planets[6].radius = 320000;
            //Ike
            planets[7] = new Planet();
            planets[7].name = "Ike";
            planets[7].initialRotation = 0;
            planets[7].timeToRotate = 65518;
            planets[7].mass = 2.78216152235874E+20;
            planets[7].radius = 130000;
            //Jool
            planets[8] = new Planet();
            planets[8].name = "Jool";
            planets[8].initialRotation = 0;
            planets[8].timeToRotate = 36000;
            planets[8].mass = 4.23321273059351E+24;
            planets[8].radius = 6000000;
            //Laythe
            planets[9] = new Planet();
            planets[9].name = "Laythe";
            planets[9].initialRotation = 90;
            planets[9].timeToRotate = 52981;
            planets[9].mass = 2.93973106291216E+22;
            planets[9].radius = 500000;
            //Vall
            planets[10] = new Planet();
            planets[10].name = "Vall";
            planets[10].initialRotation = 0;
            planets[10].timeToRotate = 105962;
            planets[10].mass = 3.10876554482042E+21;
            planets[10].radius = 300000;
            //Bop
            planets[11] = new Planet();
            planets[11].name = "Bop";
            planets[11].initialRotation = 230;
            planets[11].timeToRotate = 544507;
            planets[11].mass = 3.72610898343278E+19;
            planets[11].radius = 65000;
            //Tylo
            planets[12] = new Planet();
            planets[12].name = "Tylo";
            planets[12].initialRotation = 0;
            planets[12].timeToRotate = 211926;
            planets[12].mass = 4.23321273059351E+22;
            planets[12].radius = 600000;
            //Gilly
            planets[13] = new Planet();
            planets[13].name = "Gilly";
            planets[13].initialRotation = 5;
            planets[13].timeToRotate = 28255;
            planets[13].mass = 1.24203632781093E+17;
            planets[13].radius = 13000;
            //Pol
            planets[14] = new Planet();
            planets[14].name = "Pol";
            planets[14].initialRotation = 25;
            planets[14].timeToRotate = 901903;
            planets[14].mass = 1.08135065806823E+19;
            planets[14].radius = 44000;
            //Dres
            planets[15] = new Planet();
            planets[15].name = "Dres";
            planets[15].initialRotation = 25;
            planets[15].timeToRotate = 34800;
            planets[15].mass = 3.21909365785247E+20;
            planets[15].radius = 138000;
            //Eeloo
            planets[16] = new Planet();
            planets[16].name = "Eeloo";
            planets[16].initialRotation = 25;
            planets[16].timeToRotate = 19460;
            planets[16].mass = 1.11492242417007E+21;
            planets[16].radius = 210000;

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
            double timePart = Constants.TAU * ((time / planets[index].timeToRotate) % 1d);
            return ((planets[index].initialRotation / Constants.DEGREES_IN_RADIANS) + timePart) % Constants.TAU;
        }

        public static double GetGM(int index)
        {
            if (!planets.ContainsKey(index))
            {
                return 0;
            }
            return planets[index].mass * 6.67408e-11;
        }

        public static double GetRadius(int index)
        {
            if (!planets.ContainsKey(index))
            {
                return 0;
            }
            return planets[index].radius;
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
