namespace TUBGWorldGenerator.Utils
{
    using System;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Windows.Media.Imaging;
    using Terraria;
    using Terraria.Map;
    using TUBGWorldGenerator.WorldGeneration;

    /// <summary>
    /// ワールドを画像に変換する。
    /// </summary>
    public static class WorldToImage
    {
        /// <summary>
        /// タイルを画像にする。
        /// </summary>
        /// <param name="sandbox">ワールドサンドボックス</param>
        /// <returns>画像</returns>
        public static BitmapImage CreateMapImage(WorldSandbox sandbox)
        {
            int arrayIndex = 0;
            var array = new byte[sandbox.TileCountX * sandbox.TileCountY * 3];

            WorldMap worldMap = new WorldMap(sandbox.TileCountX, sandbox.TileCountY)
            {
                _tiles = new MapTile[sandbox.TileCountX, sandbox.TileCountY],
            };

            if (Main.Map == null)
            {
                Main.Map = worldMap;
            }

            for (int y = 0; y < sandbox.TileCountY; y++)
            {
                for (int x = 0; x < sandbox.TileCountX; x++)
                {
                    var mapTile = MapHelper.CreateMapTile(x, y, 255);
                    var color = MapHelper.GetMapTileXnaColor(ref mapTile);

                    array[arrayIndex * 3] = color.B;
                    array[(arrayIndex * 3) + 1] = color.G;
                    array[(arrayIndex * 3) + 2] = color.R;
                    arrayIndex++;

                    worldMap._tiles[x, y] = mapTile;
                }
            }

            Bitmap bitmap = CreateBitmap(sandbox.TileCountX, sandbox.TileCountY, array);
            return Convert(bitmap);
        }

        /// <summary>
        /// タイルを画像にする。
        /// </summary>
        /// <param name="tiles">タイル</param>
        /// <returns>画像</returns>
        public static BitmapImage CreateMapImage(Tile[,] tiles)
        {
            int w = tiles.GetLength(0);
            int h = tiles.GetLength(1);

            int arrayIndex = 0;
            var array = new byte[w * h * 3];

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    var mapTile = CreateMapTileFromTile(tiles[x, y], x, y);
                    var color = MapHelper.GetMapTileXnaColor(ref mapTile);

                    array[arrayIndex * 3] = color.B;
                    array[(arrayIndex * 3) + 1] = color.G;
                    array[(arrayIndex * 3) + 2] = color.R;
                    arrayIndex++;
                }
            }

            Bitmap bitmap = CreateBitmap(w, h, array);
            return Convert(bitmap);
        }

        public static (int stride, Array array) CreateMapArray(Tile[,] tiles)
        {
            int w = tiles.GetLength(0);
            int h = tiles.GetLength(1);

            int stride = (w * 3) + (w % 4);
            var array = new byte[stride * h];

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    var mapTile = CreateMapTileFromTile(tiles[x, y], x, y);
                    var color = MapHelper.GetMapTileXnaColor(ref mapTile);

                    array[(x * 3) + (y * stride)] = color.B;
                    array[(x * 3) + (y * stride) + 1] = color.G;
                    array[(x * 3) + (y * stride) + 2] = color.R;
                }
            }

            return (stride, array);
        }

        public static MapTile CreateMapTileFromTile(Tile tile, int x, int y)
        {
            if (tile == null)
            {
                return default(MapTile);
            }

            int color = 0;
            int mapTileType = 0;
            int mapTileOption = 0;
            if (tile.active())
            {
                int type = tile.type;
                mapTileType = MapHelper.tileLookup[type];
                if (type == 5)
                {
                    color = (int)tile.color();
                }
                else
                {
                    if (type == 51 && (x + y) % 2 == 0)
                    {
                        mapTileType = 0;
                    }

                    if (mapTileType != 0)
                    {
                        if (type == 160)
                        {
                            color = 0;
                        }
                        else
                        {
                            color = (int)tile.color();
                        }

                        MapHelper.GetTileBaseOption(y, tile, ref mapTileOption);
                    }
                }
            }

            if (mapTileType == 0)
            {
                if (tile.liquid > 32)
                {
                    int num5 = (int)tile.liquidType();
                    mapTileType = (int)MapHelper.liquidPosition + num5;
                }
                else if (tile.wall > 0 && tile.wall < 316)
                {
                    int wall = (int)tile.wall;
                    mapTileType = (int)MapHelper.wallLookup[wall];
                    color = (int)tile.wallColor();
                    if (wall <= 27)
                    {
                        if (wall != 21)
                        {
                            if (wall != 27)
                            {
                                mapTileOption = 0;
                            }
                            else
                            {
                                mapTileOption = x % 2;
                            }
                        }
                    }
                    else if (wall - 88 > 5 && wall != 168 && wall != 241)
                    {
                        mapTileOption = 0;
                    }
                    else
                    {
                        color = 0;
                    }
                }
            }

            if (mapTileType == 0)
            {
                mapTileType = (int)MapHelper.skyPosition + 255;
                color = 0;
            }

            return MapTile.Create((ushort)(mapTileType + mapTileOption), 255, (byte)color);
        }

        public static BitmapImage Convert(Bitmap src)
        {
            MemoryStream ms = new MemoryStream();
            src.Save(ms, ImageFormat.Bmp);
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            image.StreamSource = ms;
            image.EndInit();
            image.Freeze();
            return image;
        }

        public static Bitmap CreateBitmap(int width, int height, byte[] datas)
        {
            var b = new Bitmap(width, height, PixelFormat.Format24bppRgb);

            var boundsRect = new Rectangle(0, 0, width, height);
            BitmapData bmpData = b.LockBits(
                boundsRect,
                ImageLockMode.WriteOnly,
                b.PixelFormat);

            IntPtr ptr = bmpData.Scan0;

            int bytes = bmpData.Stride * b.Height;
            Marshal.Copy(datas, 0, ptr, bytes);
            b.UnlockBits(bmpData);
            return b;
        }
    }
}
