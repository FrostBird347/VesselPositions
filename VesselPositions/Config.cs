using System;
using System.IO;
using System.Text;
using DarkMultiPlayerServer;
using VesselPositions;

namespace VesselPositions
{
    public static class Config
    {

        public static void SetupConfig()
        {
            if (!Directory.Exists(VesselPositions.SharedPluginDirectory))
            {
                Directory.CreateDirectory(VesselPositions.SharedPluginDirectory);
            }
            VesselPositions.MapPluginFolder = VesselPositions.SharedPluginDirectory + "/DMPServerMap-FrostBird347";
            if (!Directory.Exists(VesselPositions.MapPluginFolder))
            {
                Directory.CreateDirectory(VesselPositions.MapPluginFolder);
            }
            VesselPositions.VesselPosFolder = VesselPositions.MapPluginFolder + "/VesselPos";
            if (!Directory.Exists(VesselPositions.VesselPosFolder))
            {
                Directory.CreateDirectory(VesselPositions.VesselPosFolder);
            }
            VesselPositions.MapConfigFolder = VesselPositions.MapPluginFolder + "/Config";
            if (!Directory.Exists(VesselPositions.MapConfigFolder))
            {
                Directory.CreateDirectory(VesselPositions.MapConfigFolder);
            }
            if (File.Exists(VesselPositions.MapConfigFolder + "/VesselPos.txt"))
            {
                string UpdateSpeedReturn = GetConfigValue("UpdateSpeed");
                if (UpdateSpeedReturn != "nil")
                {
                    VesselPositions.UpdateFileSpeed = double.Parse(UpdateSpeedReturn, System.Globalization.CultureInfo.InvariantCulture);
                }
                else
                {
                    VesselPositions.UpdateFileSpeed = 1;
                    string OldConfigString = File.ReadAllText(VesselPositions.MapConfigFolder + "/VesselPos.txt");
                    string NewConfigString = OldConfigString + "\nUpdateSpeed = 1";
                    byte[] NewConfigData = Encoding.Default.GetBytes(NewConfigString);
                    File.WriteAllBytes(Path.Combine(VesselPositions.MapConfigFolder + "/VesselPos.txt"), NewConfigData);
                }
            }
            else
            {
                VesselPositions.UpdateFileSpeed = 1;
                string NewConfigString = "UpdateSpeed = 1";
                byte[] NewConfigData = Encoding.Default.GetBytes(NewConfigString);
                File.WriteAllBytes(Path.Combine(VesselPositions.MapConfigFolder + "/VesselPos.txt"), NewConfigData);
                DarkLog.Debug("[ModdedVesselPositions] Created Config File");
            }

        }

        public static string GetConfigValue(string Value)
        {
            string findvalue = Value + " =";
            string FinalVesselValue = "nil";
            bool foundvar = false;
            string ConfigFile = VesselPositions.MapConfigFolder + "/VesselPos.txt";
            using (StreamReader sr = new StreamReader(ConfigFile))
            {
                string currentLine = sr.ReadLine();
                while (currentLine != null && !foundvar)
                {
                    string trimmedLine = currentLine.Trim();
                    if (trimmedLine.Trim().StartsWith(findvalue, StringComparison.Ordinal))
                    {
                        FinalVesselValue = trimmedLine.Substring(trimmedLine.IndexOf("=", StringComparison.Ordinal) + 2);
                        foundvar = true;
                    }
                    currentLine = sr.ReadLine();
                }
            }
            return FinalVesselValue;
        }


    }
}
