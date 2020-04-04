using System;
using DarkMultiPlayerCommon;
using MessageStream2;

namespace VesselPositions
{
    public class VesselUpdate
    {
        public Guid vesselID;
        public double planetTime;
        public string bodyName;
        public float[] rotation = new float[4];
        public float[] angularVelocity = new float[3];
        public FlightCtrlState flightState = new FlightCtrlState();
        public bool[] actiongroupControls = new bool[5];
        public bool isSurfaceUpdate;
        //Orbital parameters
        public double[] orbit = new double[7];
        //Surface parameters
        //Position = lat,long,alt,ground height.
        public double[] position = new double[4];
        public double[] velocity = new double[3];
        public double[] acceleration = new double[3];
        public float[] terrainNormal = new float[3];
        //SAS
        public bool sasEnabled;
        public int autopilotMode;
        public float[] lockedRotation= new float[4];

        public static VesselUpdate VeselUpdateFromBytes(byte[] messageData)
        {
            VesselUpdate update = Recycler<VesselUpdate>.GetObject();
            using (MessageReader mr = new MessageReader(messageData))
            {
                update.planetTime = mr.Read<double>();
                update.vesselID = new Guid(mr.Read<string>());
                update.bodyName = mr.Read<string>();
                update.rotation = mr.Read<float[]>();
                update.angularVelocity = mr.Read<float[]>();
                //FlightState variables
                update.flightState = new FlightCtrlState();
                update.flightState.mainThrottle = mr.Read<float>();
                update.flightState.wheelThrottleTrim = mr.Read<float>();
                update.flightState.X = mr.Read<float>();
                update.flightState.Y = mr.Read<float>();
                update.flightState.Z = mr.Read<float>();
                update.flightState.killRot = mr.Read<bool>();
                update.flightState.gearUp = mr.Read<bool>();
                update.flightState.gearDown = mr.Read<bool>();
                update.flightState.headlight = mr.Read<bool>();
                update.flightState.wheelThrottle = mr.Read<float>();
                update.flightState.roll = mr.Read<float>();
                update.flightState.yaw = mr.Read<float>();
                update.flightState.pitch = mr.Read<float>();
                update.flightState.rollTrim = mr.Read<float>();
                update.flightState.yawTrim = mr.Read<float>();
                update.flightState.pitchTrim = mr.Read<float>();
                update.flightState.wheelSteer = mr.Read<float>();
                update.flightState.wheelSteerTrim = mr.Read<float>();
                //Action group controls
                update.actiongroupControls = mr.Read<bool[]>();
                //Position/velocity
                update.isSurfaceUpdate = mr.Read<bool>();
                if (update.isSurfaceUpdate)
                {
                    update.position = mr.Read<double[]>();
                    update.velocity = mr.Read<double[]>();
                    update.acceleration = mr.Read<double[]>();
                    update.terrainNormal = mr.Read<float[]>();
                }
                else
                {
                    update.orbit = mr.Read<double[]>();
                }
                update.sasEnabled = mr.Read<bool>();
                if (update.sasEnabled)
                {
                    update.autopilotMode = mr.Read<int>();
                    update.lockedRotation = mr.Read<float[]>();
                }
                return update;
            }
        }

    }
}
