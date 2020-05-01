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
        public static string SharedPluginDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PluginData");
        public static string MapPluginFolder;
        public static string VesselPosFolder;
        public static string MapConfigFolder;
        private Dictionary<Guid, VesselInfo> vessels = new Dictionary<Guid, VesselInfo>();
        private long lastSendTime = 0;
        public static double UpdateFileSpeed = 1;
        private bool inited = false;

        public void ReloadConfig(string input)
        {
            DarkLog.Normal("[ModdedVesselPositions] Reloading config...");
            Config.SetupConfig();
            DarkLog.Normal("[ModdedVesselPositions] Finished.");
        }

        public override void OnServerStart()
        {
            
            DarkLog.Normal("[ModdedVesselPositions] Starting...");
            Config.SetupConfig();
            string[] OutDatedFileList = Directory.GetFiles(VesselPositions.VesselPosFolder);
            foreach (string vesselFile in OutDatedFileList)
            {
                if (File.Exists(vesselFile))
                {
                    File.Delete(vesselFile);
                }

            }
            DarkLog.Debug("[ModdedVesselPositions] Reset 'PluginData/DMPServerMap-FrostBird347/VesselPos'");
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
            DarkLog.Normal("[ModdedVesselPositions] Started!");
            CommandHandler.RegisterCommand("reloadpos", ReloadConfig, "Reload the ModdedVesselPositions plugin config.");
        }

        public override void OnUpdate()
        {
            if (!inited)
            {
                return;
            }
            long currentTime = DateTime.UtcNow.Ticks;
            if (currentTime > lastSendTime + (TimeSpan.TicksPerSecond * UpdateFileSpeed))
            {
                //Console.WriteLine("vessel update!");
                lastSendTime = currentTime;
                lock (vessels)
                {
                    double currentSubspaceTime = ServerTime.GetTime();
                    foreach (KeyValuePair<Guid, VesselInfo> kvp in vessels)
                    {
                        VesselInfo vi = kvp.Value;
						vi.Update(currentSubspaceTime + 100);
						double[] NextposLLH = new double[] { vi.latitude, vi.longitude, vi.altitude };
						vi.Update(currentSubspaceTime + 250);
						double[] NextposLLH2 = new double[] { vi.latitude, vi.longitude, vi.altitude };
						vi.Update(currentSubspaceTime + 500);
						double[] NextposLLH3 = new double[] { vi.latitude, vi.longitude, vi.altitude };
						vi.Update(currentSubspaceTime + 750);
						double[] NextposLLH4 = new double[] { vi.latitude, vi.longitude, vi.altitude };
						vi.Update(currentSubspaceTime + 1000);
						double[] NextposLLH5 = new double[] { vi.latitude, vi.longitude, vi.altitude };
						vi.Update(currentSubspaceTime);
                        double[] posLLH = new double[] { vi.latitude, vi.longitude, vi.altitude };
                        //Console.WriteLine(kvp.Key + " Pos: " + Vector.GetString(posLLH, 2) + " velocity: " + vi.velocity.ToString("F1") + " time: " + currentSubspaceTime.ToString("F1"));
                        string currentvesselDataString = "pid = " + kvp.Key + "\npos = " + Vector.GetString(posLLH, 2) + "\nnextloc = " + Vector.GetString(NextposLLH, 2) + "\nnextloc2 = " + Vector.GetString(NextposLLH2, 2) + "\nnextloc3 = " + Vector.GetString(NextposLLH3, 2) + "\nnextloc4 = " + Vector.GetString(NextposLLH4, 2) + "\nnextloc5 = " + Vector.GetString(NextposLLH5, 2) + "\nvel = " + vi.velocity.ToString("F1") + "\ntime = " + currentSubspaceTime.ToString("F1");
                        byte[] currentvesselData = Encoding.Default.GetBytes(currentvesselDataString);
                        File.WriteAllBytes(Path.Combine(VesselPosFolder, kvp.Key + ".txt"), currentvesselData);

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
            string vesselFile = VesselPosFolder + "/" + vesselID + ".txt";
            if (File.Exists(vesselFile))
            {
                File.Delete(vesselFile);
            }
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