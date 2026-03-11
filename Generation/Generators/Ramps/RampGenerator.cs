using UnityEngine;

public class RampGenerator : MonoBehaviour
{
    public void Generate(MapData map)
    {
        for (int x = 1; x < map.Width - 1; x++)
        {
            for (int y = 1; y < map.Height - 1; y++)
            {
                MapTile tile = map.GetTile(x, y);

                if (!tile.IsLand)
                    continue;

                int h = tile.Height;

                MapTile north = map.GetTile(x, y + 1);
                MapTile south = map.GetTile(x, y - 1);
                MapTile east = map.GetTile(x + 1, y);
                MapTile west = map.GetTile(x - 1, y);

                if (north != null && north.Height == h + 1)
                {
                    SetRamp(tile, RampDirection.North);
                }
                else if (south != null && south.Height == h + 1)
                {
                    SetRamp(tile, RampDirection.South);
                }
                else if (east != null && east.Height == h + 1)
                {
                    SetRamp(tile, RampDirection.East);
                }
                else if (west != null && west.Height == h + 1)
                {
                    SetRamp(tile, RampDirection.West);
                }
            }
        }
    }

    private void SetRamp(MapTile tile, RampDirection dir)
    {
        tile.Type = TileType.Ramp;
        tile.RampDirection = dir;
    }
}