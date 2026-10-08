namespace Windsock
{
    public static class SatRadImages
    {
        public static List<byte[]> BuildPackets(string framePath, string installName)
        {
            if (installName != "domestic" && installName != "wxscan")
            {
                throw new ArgumentException("Unknown i1 installation: " + installName);
            }

            string destination = "/usr/twc/" + installName + "/data/volatile/images/radarSatellite/us/" + Path.GetFileName(framePath);
            List<string> commands = new List<string>
            {
                "import twccommon\ntwccommon.Log.info('Windsock satrad image')\n",
                "if not isInterested(imageData=['radarSatellite.us']):\n    abortMsg()\n"
            };
            string storeCommand = "wxdata.setImageData('radarSatellite.us', " + PythonStringHelper.PyValue(destination) + ")\n";
            return PacketEncoding.BuildFileMessage(commands, File.ReadAllBytes(framePath), destination, storeCommand);
        }
    }
}
