using System;
namespace VesselPositions
{
    public class Orbit
    {
        private const double tau = Math.PI * 2;
        public double inclination;
        public double eccentricity;
        public double semiMajorAxis;
        public double LAN;
        public double argumentOfPeriapsis;
        public double meanAnomalyAtEpoch;
        public double epoch;
        public int referenceBody;
        //State stuff
        public double period;
        public double eccentricAnomaly;
        public double trueAnomaly;
        public double distanceToCB;
        public double[] position;
        public double[] velocity;
        public double converged;

        public Orbit(double[] orbit, int referenceBody)
        {
            this.inclination = orbit[0];
            this.eccentricity = orbit[1];
            this.semiMajorAxis = orbit[2];
            this.LAN = orbit[3];
            this.argumentOfPeriapsis = orbit[4];
            this.meanAnomalyAtEpoch = orbit[5];
            this.epoch = orbit[6];
            this.referenceBody = referenceBody;
            position = new double[3];
            velocity = new double[3];
            Solve();
        }

        public void UpdateToUT(double universeTime)
        {
            double deltaTime = universeTime - epoch;
            double GM = PlanetInfo.GetGM(referenceBody);
            meanAnomalyAtEpoch = meanAnomalyAtEpoch + deltaTime * Math.Sqrt(GM / Math.Pow(semiMajorAxis, 3));
            meanAnomalyAtEpoch = meanAnomalyAtEpoch % tau;
            if (meanAnomalyAtEpoch < 0)
            {
                meanAnomalyAtEpoch += tau;
            }
            epoch = universeTime;
            Solve();
        }

        public void Solve()
        {
            period = tau * Math.Sqrt(Math.Pow(semiMajorAxis, 3) / PlanetInfo.GetGM(referenceBody));
            SolveKepler();
            double trueX = Math.Sqrt(1d - eccentricity) * Math.Cos(eccentricAnomaly / 2d);
            double trueY = Math.Sqrt(1d + eccentricity) * Math.Sin(eccentricAnomaly / 2d);
            trueAnomaly = 2d * Math.Atan2(trueY, trueX);
            distanceToCB = semiMajorAxis * (1 - eccentricity * Math.Cos(eccentricAnomaly));
            double[] orbitFramePosition = new double[] { Math.Cos(trueAnomaly), Math.Sin(trueAnomaly), 0 };
            double GM = PlanetInfo.GetGM(referenceBody);
            double velocityScale = Math.Sqrt(GM * semiMajorAxis) / distanceToCB;
            double[] velocityUnscaled = new double[] { -Math.Sin(eccentricAnomaly), Math.Sqrt(1 - eccentricity * eccentricity) * Math.Cos(eccentricAnomaly) };
            double[] orbitFrameVelocity = Vector.Multiply(velocityUnscaled, velocityScale);
            double sinW = Math.Sin(argumentOfPeriapsis);
            double cosW = Math.Cos(argumentOfPeriapsis);
            double sinO = Math.Sin(LAN);
            double cosO = Math.Cos(LAN);
            double sinI = Math.Sin(inclination);
            double cosI = Math.Cos(inclination);
            double X1 = cosW * cosO - sinW * cosI * sinO;
            double X2 = sinW * cosO + cosW * cosI * sinO;
            double Y1 = cosW * sinO + sinW * cosI * cosO;
            double Y2 = cosW * cosI * cosO - sinW * sinO;
            double Z1 = sinW * sinI;
            double Z2 = cosW * sinI;
            //Rotation part
            position[0] = orbitFramePosition[0] * X1 - orbitFramePosition[1] * X2;
            position[1] = orbitFramePosition[0] * Y1 + orbitFramePosition[1] * Y2;
            position[2] = orbitFramePosition[0] * Z1 + orbitFramePosition[1] * Z2;
            velocity[0] = orbitFrameVelocity[0] * X1 - orbitFrameVelocity[1] * X2;
            velocity[1] = orbitFrameVelocity[0] * Y1 + orbitFrameVelocity[1] * Y2;
            velocity[2] = orbitFrameVelocity[0] * Z1 + orbitFrameVelocity[1] * Z2;
        }

        private const int ITERATIONS = 10;
        private const double CONVERGE = 1e-12;
        private void SolveKepler()
        {
            //Normalise meanAnomaly
            meanAnomalyAtEpoch = meanAnomalyAtEpoch % tau;
            while (meanAnomalyAtEpoch < -Math.PI)
            {
                meanAnomalyAtEpoch += tau;
            }
            while (meanAnomalyAtEpoch > Math.PI)
            {
                meanAnomalyAtEpoch -= tau;
            }
            eccentricAnomaly = meanAnomalyAtEpoch;
            Kepler(ITERATIONS);
            //Console.WriteLine("ecc: " + eccentricAnomaly + ", converge: " + converged);
        }

        private void Kepler(int iterationsLeft)
        {
            if (iterationsLeft == 0)
            {
                return;
            }
            double topHalf = eccentricAnomaly - eccentricity * Math.Sin(eccentricAnomaly) - meanAnomalyAtEpoch;
            double bottomHalf = 1 - eccentricity * Math.Cos(eccentricAnomaly);
            converged = topHalf / bottomHalf;
            eccentricAnomaly = eccentricAnomaly - converged;
            if (Math.Abs(converged) < CONVERGE)
            {
                return;
            }
            Kepler(iterationsLeft - 1);
        }
    }
}
