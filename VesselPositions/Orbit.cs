//Reference: https://downloads.rene-schwarz.com/download/M001-Keplerian_Orbit_Elements_to_Cartesian_State_Vectors.pdf

using System;
namespace VesselPositions
{
    public class Orbit
    {

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
            double deltaTime = (universeTime - epoch);
            if (eccentricity < 1)
            {
                period = Constants.TAU * Math.Sqrt(Math.Pow(semiMajorAxis, 3) / PlanetInfo.GetGM(referenceBody));
                deltaTime = deltaTime % period;
                while (deltaTime < 0)
                {
                    deltaTime += period;
                }
            }
            else
            {
                period = Double.PositiveInfinity;
            }
            double GM = PlanetInfo.GetGM(referenceBody);
            double positiveSMA = Math.Abs(semiMajorAxis);
            double meanMotion = Math.Sqrt(GM / (positiveSMA * positiveSMA * positiveSMA));
            meanAnomalyAtEpoch = meanAnomalyAtEpoch + deltaTime * meanMotion;
            //Normalise meanAnomaly
            meanAnomalyAtEpoch = meanAnomalyAtEpoch % Constants.TAU;
            while (meanAnomalyAtEpoch < -Math.PI)
            {
                meanAnomalyAtEpoch += Constants.TAU;
            }
            while (meanAnomalyAtEpoch > Math.PI)
            {
                meanAnomalyAtEpoch -= Constants.TAU;
            }
            epoch = universeTime;
            Solve();
        }

        public void Solve()
        {
            SolveKepler();
            if (eccentricity < 1)
            {
                SolveNormal();
            }
            else
            {
                SolveHyperbolic();
            }
            //Convert TrueAnomaly to state vectors
            double GM = PlanetInfo.GetGM(referenceBody);
            double num = semiMajorAxis * (1.0 - eccentricity * eccentricity);
            double cosTA = Math.Cos(trueAnomaly);
            double sinTA = Math.Sin(trueAnomaly);
            distanceToCB = num / (1.0 + eccentricity * cosTA);
            double[] orbitFramePosition = new double[] { cosTA * distanceToCB, sinTA * distanceToCB, 0 };
            double velocityScale = Math.Sqrt(GM / num);
            double[] orbitFrameVelocity = new double[] { -sinTA * velocityScale, (cosTA + eccentricity) * velocityScale, 0 };
            //Rotation from orbital plane to body non-rotating
            double sinW = Math.Sin(argumentOfPeriapsis / Constants.DEGREES_IN_RADIANS);
            double cosW = Math.Cos(argumentOfPeriapsis / Constants.DEGREES_IN_RADIANS);
            double sinO = Math.Sin(LAN / Constants.DEGREES_IN_RADIANS);
            double cosO = Math.Cos(LAN / Constants.DEGREES_IN_RADIANS);
            double sinI = Math.Sin(inclination / Constants.DEGREES_IN_RADIANS);
            double cosI = Math.Cos(inclination / Constants.DEGREES_IN_RADIANS);
            double X1 = cosW * cosO - sinW * cosI * sinO;
            double X2 = sinW * cosO + cosW * cosI * sinO;
            double Y1 = cosW * sinO + sinW * cosI * cosO;
            double Y2 = cosW * cosI * cosO - sinW * sinO;
            double Z1 = sinW * sinI;
            double Z2 = cosW * sinI;
            position[0] = orbitFramePosition[0] * X1 - orbitFramePosition[1] * X2;
            position[1] = orbitFramePosition[0] * Y1 + orbitFramePosition[1] * Y2;
            position[2] = orbitFramePosition[0] * Z1 + orbitFramePosition[1] * Z2;
            velocity[0] = orbitFrameVelocity[0] * X1 - orbitFrameVelocity[1] * X2;
            velocity[1] = orbitFrameVelocity[0] * Y1 + orbitFrameVelocity[1] * Y2;
            velocity[2] = orbitFrameVelocity[0] * Z1 + orbitFrameVelocity[1] * Z2;
        }

        private void SolveNormal()
        {
            double trueX = Math.Sqrt(1d - eccentricity) * Math.Cos(eccentricAnomaly / 2d);
            double trueY = Math.Sqrt(1d + eccentricity) * Math.Sin(eccentricAnomaly / 2d);
            trueAnomaly = 2d * Math.Atan2(trueY, trueX);
        }

        private void SolveHyperbolic()
        {
            double trueX = Math.Sqrt(eccentricity - 1d) * Math.Cosh(eccentricAnomaly / 2d);
            double trueY = Math.Sqrt(eccentricity + 1d) * Math.Sinh(eccentricAnomaly / 2d);
            trueAnomaly = 2d * Math.Atan2(trueY, trueX);
        }

        private const int ITERATIONS = 50;
        private const double CONVERGE = 1e-7;
        private void SolveKepler()
        {
            //From KSP
            if (eccentricity < 1)
            {
                eccentricAnomaly = meanAnomalyAtEpoch + eccentricity * Math.Sin(meanAnomalyAtEpoch) + 0.5 * eccentricity * eccentricity * Math.Sin(2.0 * meanAnomalyAtEpoch);
            }
            //From KSP
            if (eccentricity > 1)
            {
                double num2 = 2.0 * meanAnomalyAtEpoch / eccentricity;
                eccentricAnomaly = Math.Log(Math.Sqrt(num2 * num2 + 1.0) + num2);
            }
            //Standard Kepler
            if (eccentricity < 1)
            {
                KeplerNormal(ITERATIONS);
            }
            //Hypberbolic Kepler
            if (eccentricity >= 1)
            {
                KeplerHyperbolic(ITERATIONS);
            }
        }

        private void KeplerNormal(int iterationsLeft)
        {
            if (iterationsLeft == 0)
            {
                return;
            }
            double topHalf = eccentricAnomaly - eccentricity * Math.Sin(eccentricAnomaly);
            converged = (meanAnomalyAtEpoch - topHalf) / (1.0 - eccentricity * Math.Cos(eccentricAnomaly));
            eccentricAnomaly += converged;
            if (Math.Abs(converged) < CONVERGE)
            {
                return;
            }
            KeplerNormal(iterationsLeft - 1);
        }

        private void KeplerHyperbolic(int iterationsLeft)
        {
            if (iterationsLeft == 0)
            {
                return;
            }
            converged = (eccentricity * Math.Sinh(eccentricAnomaly) - eccentricAnomaly - meanAnomalyAtEpoch) / (eccentricity * Math.Cosh(eccentricAnomaly) - 1.0);
            eccentricAnomaly -= converged;
            if (Math.Abs(converged) < CONVERGE)
            {
                return;
            }
            KeplerHyperbolic(iterationsLeft - 1);
        }
    }
}
