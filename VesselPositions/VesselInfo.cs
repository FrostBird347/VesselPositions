using System;
using DarkMultiPlayerServer;
namespace VesselPositions
{
    public class VesselInfo
    {
        public Guid vesselID;
        public string planet;
        public double latitude;
        public double longitude;
        public double altitude;
        public double velocity;
        public bool landed;
        public Orbit orbit;

        public VesselInfo(Guid vesselID)
        {
            this.vesselID = vesselID;
        }

        public void Update(double universeUT)
        {
            if (landed)
            {
                //Landed vessels don't move.
                return;
            }
            orbit.UpdateToUT(universeUT);
            UpdateOrbit();
        }

        private void UpdateOrbit()
        {
            double x = orbit.position[0];
            double y = orbit.position[1];
            double z = orbit.position[2];
            double x2 = x * x;
            double y2 = y * y;
            double z2 = z * z;
            this.altitude = Math.Sqrt(x2 + y2 + z2);
            this.longitude = Math.Atan2(y, x);
            this.latitude = Math.Acos(z / this.altitude);
            this.velocity = Vector.Length(orbit.velocity);
        }

        public void UpdateOrbit(Orbit orbit)
        {
            landed = false;
            this.orbit = orbit;
            UpdateOrbit();
        }

        public void UpdateLanded(double latitude, double longitude, double altitude, double velocity)
        {
            landed = true;
            this.latitude = latitude;
            this.longitude = longitude;
            this.altitude = altitude;
            this.velocity = velocity;
        }

        public void UpdatePlanet(int id)
        {
            planet = PlanetInfo.GetName(id);
        }
    }
}
