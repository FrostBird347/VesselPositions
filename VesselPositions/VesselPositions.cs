using System;
using System.Collections.Generic;
using System.IO;
using DarkMultiPlayerCommon;
using DarkMultiPlayerServer;
using MessageStream2;

namespace VesselPositions
{
    public class VesselPositions : DMPPlugin
    {
        public override void OnServerStart()
        {
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
            Console.WriteLine("Updated vessel data: " + vesselID);
        }

        public void RemoveVessel(ClientObject client, Guid vesselID)
        {
            Console.WriteLine("Removing vessel data: " + vesselID);
        }

        public void PositionVessel(ClientObject client, VesselUpdate update)
        {
            Console.WriteLine("Update vessel position: " + update.vesselID);
            if (update.isSurfaceUpdate)
            {
                //Play with
                //update.position (Lat, long, alt);
                //update.velocity (Surface relative speed, probably just take it's length);
            }
            else
            {
                //Play with, order is https://github.com/godarklight/DarkMultiPlayer/blob/master/Client/VesselUpdate.cs#L90-L96
                //update.orbit;
            }
        }
    }
}
