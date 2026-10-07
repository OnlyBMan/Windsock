namespace Windsock
{
    public static class RadarImages
    {
        public static List<byte[]> BuildPackets(string framePath, string installName)
        {
            if (installName != "domestic" && installName != "wxscan")
            {
                throw new ArgumentException("Unknown i1 installation: " + installName);
            }

            string destination = "/usr/twc/" + installName + "/data/volatile/images/radar/us/" + Path.GetFileName(framePath);
            List<string> commands = new List<string>
            {
                "import twccommon\ntwccommon.Log.info('Windsock radar image')\n",
                "if not isInterested(imageData=['radar.us']):\n    abortMsg()\n"
            };
            string storeCommand = "wxdata.setImageData('radar.us', " + PythonStringHelper.PyValue(destination) + ")\n";
            return PacketEncoding.BuildFileMessage(commands, File.ReadAllBytes(framePath), destination, storeCommand);
        }
    }
}
