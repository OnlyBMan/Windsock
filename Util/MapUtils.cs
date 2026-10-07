namespace Windsock
{
    public class MapUtils
    {
        public static MapCutRange ConvertMercatorRange(List<I1Map> maps)
        {
            MapCutRange mapCutRange = new MapCutRange();
            int? minXTile = null;
            int? minYTile = null;
            int? maxXTile = null;
            int? maxYTile = null;

            foreach (I1Map map in maps)
            {
                if (map.MapcutCoordinate == null || map.MapcutCoordinate.Length < 2
                    || map.MapcutSize == null || map.MapcutSize.Length < 2)
                {
                    continue;
                }

                int left = map.MapcutCoordinate[0];
                int top = map.MapcutCoordinate[1];
                int right = map.MapcutCoordinate[0] + map.MapcutSize[0];
                int bottom = map.MapcutCoordinate[1] + map.MapcutSize[1];


                double longitudeRadians = (left - 72046.041456) / 31073.895649;
                double mercatorNorthing = (top + 7521.072516) / 25893.432399;

                double longitudeRadiansRight = (right - 72046.041456) / 31073.895649;
                double mercatorNorthingBottom = (bottom + 7521.072516) / 25893.432399;

                int tileX = (int)Math.Floor(128 * (longitudeRadians + Math.PI) / (2 * Math.PI));
                int tileY = (int)Math.Floor(128 * (1 - mercatorNorthing / Math.PI) / 2);
                int tileXRight = (int)Math.Floor(128 * (longitudeRadiansRight + Math.PI) / (2 * Math.PI));
                int tileYBottom = (int)Math.Floor(128 * (1 - mercatorNorthingBottom / Math.PI) / 2);

                if (!minXTile.HasValue || Math.Min(tileX, tileXRight) < minXTile.Value)
                {
                    minXTile = Math.Min(tileX, tileXRight);
                }
                if (!minYTile.HasValue || Math.Min(tileY, tileYBottom) < minYTile.Value)
                {
                    minYTile = Math.Min(tileY, tileYBottom);
                }

                if (!maxXTile.HasValue || Math.Max(tileX, tileXRight) > maxXTile.Value)
                {
                    maxXTile = Math.Max(tileX, tileXRight);
                }
                if (!maxYTile.HasValue || Math.Max(tileY, tileYBottom) > maxYTile.Value)
                {
                    maxYTile = Math.Max(tileY, tileYBottom);
                }
            }

            mapCutRange.minX = minXTile ?? 0;
            mapCutRange.minY = minYTile ?? 0;
            mapCutRange.maxX = maxXTile ?? 0;
            mapCutRange.maxY = maxYTile ?? 0;
            return mapCutRange;
        }
    }
}
