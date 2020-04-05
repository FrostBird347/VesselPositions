using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ConfigNodeParser;
using DarkMultiPlayerCommon;
using DarkMultiPlayerServer;
using MessageStream2;

namespace VesselPositions
{
    public class VesselPositions : DMPPlugin
    {
        private Dictionary<Guid, VesselInfo> vessels = new Dictionary<Guid, VesselInfo>();
        private long lastSendTime = 0;
        private bool inited = false;

        public override void OnServerStart()
        {
            if (inited)
            {
                return;
            }
            PlanetInfo.Init();
            ServerTime.Init();
            foreach (string file in Directory.GetFiles(Path.Combine(Server.universeDirectory, "Vessels")))
            {
                string croppedName = Path.GetFileNameWithoutExtension(file);
                if (Guid.TryParse(croppedName, out Guid vesselID))
                {
                    byte[] vesselData = File.ReadAllBytes(file);
                    UpdateVessel(null, vesselID, vesselData);
                }
            }
            inited = true;
        }

        public override void OnUpdate()
        {
            if (!inited)
            {
                return;
            }
            long currentTime = DateTime.UtcNow.Ticks;
            if (currentTime > lastSendTime + (TimeSpan.TicksPerSecond / 1))
            {
                lastSendTime = currentTime;
                lock (vessels)
                {
                    double currentSubspaceTime = ServerTime.GetTime();
                    foreach (KeyValuePair<Guid, VesselInfo> kvp in vessels)
                    {
                        VesselInfo vi = kvp.Value;
                        vi.Update(currentSubspaceTime);
                        double[] posLLH = new double[] { vi.latitude, vi.longitude, vi.altitude };
                        Console.WriteLine(kvp.Key + " Pos: " + Vector.GetString(posLLH, 2) + " velocity: " + vi.velocity.ToString("F1") + " time: " + currentSubspaceTime.ToString("F1"));
                    }
                }
            }
        }

        public override void OnMessageReceived(ClientObject client, ClientMessage messageData)
        {
            if (!inited)
            {
                return;
            }
            if (messageData.type == ClientMessageType.VESSEL_PROTO)
            {
                using (MessageReader mr = new MessageReader(messageData.data))
                {
                    double planetTime = mr.Read<double>();
                    bool vesselIdOK = Guid.TryParse(mr.Read<string>(), out Guid vesselID);
                    bool isDockingUpdate = mr.Read<bool>();
                    bool isFlyingUpdate = mr.Read<bool>();
                    byte[] possibleCompressedBytes = mr.Read<byte[]>();
                    byte[] vesselData = Compression.DecompressIfNeeded(possibleCompressedBytes);
                    if (vesselIdOK)
                    {
                        UpdateVessel(client, vesselID, vesselData);
                    }
                }
            }
            if (messageData.type == ClientMessageType.VESSEL_UPDATE)
            {
                VesselUpdate vu = VesselUpdate.VeselUpdateFromBytes(messageData.data);
                PositionVessel(client, vu);
                Recycler<VesselUpdate>.ReleaseObject(vu);
            }
            if (messageData.type == ClientMessageType.VESSEL_REMOVE)
            {
                using (MessageReader mr = new MessageReader(messageData.data))
                {
                    double planetTime = mr.Read<double>();
                    if (Guid.TryParse(mr.Read<string>(), out Guid vesselID))
                    {
                        RemoveVessel(client, vesselID);
                    }
                }
            }
        }

        public void UpdateVessel(ClientObject client, Guid vesselID, byte[] vesselData)
        {
            string vesselDataString = Encoding.UTF8.GetString(vesselData);
            ConfigNode cn = ConfigNodeReader.StringToConfigNode(vesselDataString);
            string landed = cn.GetValue("landed");
            VesselInfo vi = GetVesselInfo(vesselID);
            if (landed == "True")
            {
                double newLat = Double.Parse(cn.GetValue("lat"));
                double newLong = Double.Parse(cn.GetValue("lon"));
                double newAlt = Double.Parse(cn.GetValue("alt"));
                vi.UpdateLanded(newLat, newLong, newAlt, 0);
            }
            else
            { 
                ConfigNode cnOrbit = cn.GetNode("ORBIT");
                double[] orbit = new double[7];
                orbit[0] = double.Parse(cnOrbit.GetValue("INC"));
                orbit[1] = double.Parse(cnOrbit.GetValue("ECC"));
                orbit[2] = double.Parse(cnOrbit.GetValue("SMA"));
                orbit[3] = double.Parse(cnOrbit.GetValue("LAN"));
                orbit[4] = double.Parse(cnOrbit.GetValue("LPE"));
                orbit[5] = double.Parse(cnOrbit.GetValue("MNA"));
                orbit[6] = double.Parse(cnOrbit.GetValue("EPH"));
                int referenceBody = Int32.Parse(cnOrbit.GetValue("REF"));
                Orbit o = new Orbit(orbit, referenceBody);
                vi.UpdateOrbit(o);
            }
        }

        public void RemoveVessel(ClientObject client, Guid vesselID)
        {
            RemoveVesselInfo(vesselID);
        }

        public void PositionVessel(ClientObject client, VesselUpdate update)
        {
            if (update.isSurfaceUpdate)
            {
                VesselInfo vi = GetVesselInfo(update.vesselID);
                vi.UpdateLanded(update.position[0], update.position[1], update.position[2], Vector.Length(update.velocity));
            }
            else
            {
                VesselInfo vi = GetVesselInfo(update.vesselID);
                int planetReference = PlanetInfo.GetReference(update.bodyName);
                if (planetReference != -1)
                {
                    Orbit o = new Orbit(update.orbit, planetReference);
                    vi.UpdateOrbit(o);
                }
            }
        }

        private VesselInfo GetVesselInfo(Guid vesselID)
        {
            lock (vessels)
            {
                if (vessels.ContainsKey(vesselID))
                {
                    return vessels[vesselID];
                }
                VesselInfo vi = new VesselInfo(vesselID);
                vessels[vesselID] = vi;
                return vi;
            }
        }

        private void RemoveVesselInfo(Guid vesselID)
        {
            lock (vessels)
            {
                if (vessels.ContainsKey(vesselID))
                {
                    vessels.Remove(vesselID);
                }
            }
        }
    }
}
