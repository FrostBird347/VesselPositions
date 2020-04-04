using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DarkMultiPlayerCommon;
using DarkMultiPlayerServer;
using MessageStream2;
using ConfigNodeParser;

namespace VesselPositions
{
    public class VesselPositions : DMPPlugin
    {
        private Dictionary<Guid, VesselInfo> vessels = new Dictionary<Guid, VesselInfo>();
        private long lastSendTime = 0;

        public override void OnServerStart()
        {
            PlanetInfo.Init();
            foreach (string file in Directory.GetFiles(Path.Combine(Server.universeDirectory, "Vessels")))
            {
                string croppedName = Path.GetFileNameWithoutExtension(file);
                if (Guid.TryParse(croppedName, out Guid vesselID))
                {
                    byte[] vesselData = File.ReadAllBytes(file);
                    UpdateVessel(null, vesselID, vesselData);
                }
            }
        }

        public override void OnUpdate()
        {
            long currentTime = DateTime.UtcNow.Ticks;
            if (currentTime > lastSendTime + TimeSpan.TicksPerSecond)
            {
                lastSendTime = currentTime;
                lock (vessels)
                {
                    foreach (KeyValuePair<Guid, VesselInfo> kvp in vessels)
                    {
                        Console.WriteLine(kvp.Key + " velocity: " + kvp.Value.velocity);
                    }
                }
            }
        }

        public override void OnMessageReceived(ClientObject client, ClientMessage messageData)
        {
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
            ConfigNode cn = ConfigNodeReader.StringToConfigNode(vesselDataString).GetNode("ORBIT");
            double[] orbit = new double[7];
            orbit[0] = double.Parse(cn.GetValue("INC"));
            orbit[1] = double.Parse(cn.GetValue("ECC"));
            orbit[2] = double.Parse(cn.GetValue("SMA"));
            orbit[3] = double.Parse(cn.GetValue("LAN"));
            orbit[4] = double.Parse(cn.GetValue("LPE"));
            orbit[5] = double.Parse(cn.GetValue("MNA"));
            orbit[6] = double.Parse(cn.GetValue("EPH"));
            int referenceBody = Int32.Parse(cn.GetValue("REF"));
            Orbit o = new Orbit(orbit, referenceBody);
            VesselInfo vi = GetVesselInfo(vesselID);
            vi.UpdateOrbit(o);
        }

        public void RemoveVessel(ClientObject client, Guid vesselID)
        {
            RemoveVesselInfo(vesselID);
        }

        public void PositionVessel(ClientObject client, VesselUpdate update)
        {
            Console.WriteLine("Update vessel position: " + update.vesselID);
            if (update.isSurfaceUpdate)
            {
                VesselInfo vi = GetVesselInfo(update.vesselID);
                vi.longitude = update.position[1];
                vi.altitude = update.position[2];
                vi.velocity = Vector.Length(update.velocity);
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
