namespace TUBGWorldGenerator.BuildingGenerator.UI
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Windows.Media.Imaging;
    using Microsoft.Win32;
    using Terraria;
    using Terraria.Map;
    using TUBGWorldGenerator.Utils;

    public static class TileToImage
    {
        private static readonly Point wallFrameSize;

        private static TextureLoader textureLoader;

        private static Point[][] wallFrameLookup;

        private static int[][] centerWallFrameLookup;

        static TileToImage()
        {
            wallFrameSize = new Point(36, 36);
            wallFrameLookup = new Point[20][];
            AddWallFrameLookup(0, 9, 3, 10, 3, 11, 3, 6, 6);
            AddWallFrameLookup(1, 6, 3, 7, 3, 8, 3, 4, 6);
            AddWallFrameLookup(2, 12, 0, 12, 1, 12, 2, 12, 5);
            AddWallFrameLookup(3, 1, 4, 3, 4, 5, 4, 3, 6);
            AddWallFrameLookup(4, 9, 0, 9, 1, 9, 2, 9, 5);
            AddWallFrameLookup(5, 0, 4, 2, 4, 4, 4, 2, 6);
            AddWallFrameLookup(6, 6, 4, 7, 4, 8, 4, 5, 6);
            AddWallFrameLookup(7, 1, 2, 2, 2, 3, 2, 3, 5);
            AddWallFrameLookup(8, 6, 0, 7, 0, 8, 0, 6, 5);
            AddWallFrameLookup(9, 5, 0, 5, 1, 5, 2, 5, 5);
            AddWallFrameLookup(10, 1, 3, 3, 3, 5, 3, 1, 6);
            AddWallFrameLookup(11, 4, 0, 4, 1, 4, 2, 4, 5);
            AddWallFrameLookup(12, 0, 3, 2, 3, 4, 3, 0, 6);
            AddWallFrameLookup(13, 0, 0, 0, 1, 0, 2, 0, 5);
            AddWallFrameLookup(14, 1, 0, 2, 0, 3, 0, 1, 5);
            AddWallFrameLookup(15, 1, 1, 2, 1, 3, 1, 2, 5);
            AddWallFrameLookup(16, 6, 1, 7, 1, 8, 1, 7, 5);
            AddWallFrameLookup(17, 6, 2, 7, 2, 8, 2, 8, 5);
            AddWallFrameLookup(18, 10, 0, 10, 1, 10, 2, 10, 5);
            AddWallFrameLookup(19, 11, 0, 11, 1, 11, 2, 11, 5);

            centerWallFrameLookup = new int[3][]
            {
                new int[3] { 2, 0, 0 },
                new int[3] { 0, 1, 4 },
                new int[3] { 0, 3, 0 },
            };
        }

        public static BitmapImage CreateBitmap(Tile[,] tiles)
        {
            if (textureLoader == null)
            {
                string path = null;
                if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
                {
                    using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\GOG.com\Games\1207665503\"))
                    {
                        if (key != null)
                        {
                            path = Path.Combine((string)key.GetValue("PATH"), "Content");
                        }
                    }
                }

                // find steam
                if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
                {
                    // try with dionadar's fix
                    using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 105600"))
                    {
                        if (key != null)
                        {
                            path = Path.Combine((string)key.GetValue("InstallLocation"), "Content");
                        }
                    }
                }

                // if that fails, try steam path
                if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
                {
                    using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\\Valve\\Steam"))
                    {
                        if (key != null)
                        {
                            path = key.GetValue("SteamPath") as string;
                        }
                    }

                    // no steam key, let's try steam in program files
                    if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
                    {
                        path = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
                        path = Path.Combine(path, "Steam");
                    }

                    path = Path.Combine(path, "steamapps", "common", "terraria", "Content");
                }

                // if that fails, try steam path - the long way
                if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
                {
                    using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\\Valve\\Steam"))
                    {
                        if (key != null)
                        {
                            path = key.GetValue("InstallPath") as string;
                        }
                        else
                        {
                            using (RegistryKey key2 = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\\WOW6432Node\\Valve\\Steam"))
                            {
                                if (key2 != null)
                                {
                                    path = key2.GetValue("InstallPath") as string;
                                }
                            }
                        }

                        // no steam key, let's try steam in program files
                        if (!string.IsNullOrWhiteSpace(path) && Directory.Exists(path))
                        {
                            var vdfFile = Path.Combine(path, "steamapps", "libraryfolders.vdf");

                            using (var file = File.Open(vdfFile, FileMode.Open, FileAccess.Read, FileShare.Read))
                            using (TextReader tr = new StreamReader(file))
                            {
                                var libraryPaths = new List<string>();
                                string line = null;
                                bool foundPath = false;
                                while ((line = tr.ReadLine()) != null && !foundPath)
                                {
                                    if (!string.IsNullOrWhiteSpace(line))
                                    {
                                        var split = line.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
                                        foreach (var item in split)
                                        {
                                            var trimmed = item.Trim('\"').Replace("\\\\", "\\");
                                            if (Directory.Exists(trimmed))
                                            {

                                                var testpath = Path.Combine(trimmed, "steamapps", "common", "terraria", "Content");
                                                if (Directory.Exists(testpath))
                                                {
                                                    path = testpath;
                                                    foundPath = true;
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                textureLoader = new TextureLoader() { ExtractedContentsDir = path };
            }

            // 上下左右に8pxづつ余裕を持たせる
            Bitmap result = new Bitmap((tiles.GetLength(0) * 16) + 16, (tiles.GetLength(1) * 16) + 16);
            using (Graphics g = Graphics.FromImage(result))
            {
                for (int i = 0; i < tiles.GetLength(0); i++)
                {
                    for (int j = 0; j < tiles.GetLength(1); j++)
                    {
                        if (tiles[i, j].wall != 0)
                        {
                            Bitmap image = textureLoader.GetWall(tiles[i, j].wall);
                            if (image == null)
                            {
                                var array = new byte[16 * 16 * 3];
                                var mapTile = WorldToImage.CreateMapTileFromTile(tiles[i, j], i, j);
                                var color = MapHelper.GetMapTileXnaColor(ref mapTile);
                                for (int ind = 0; ind < array.Length; ind += 3)
                                {
                                    array[ind] = color.B;
                                    array[ind + 1] = color.G;
                                    array[ind + 2] = color.R;
                                }

                                image = WorldToImage.CreateBitmap(16, 16, array);
                                g.DrawImageUnscaled(image, (i * 16) + 8, (j * 16) + 8);
                            }
                            else
                            {
                                var point = GetWallFrame(tiles, i, j);
                                image = image.Clone(new Rectangle(point.X, point.Y, 32, 32), image.PixelFormat);
                                g.DrawImageUnscaled(image, i * 16, j * 16);
                            }
                        }
                    }
                }

                for (int i = 0; i < tiles.GetLength(0); i++)
                {
                    for (int j = 0; j < tiles.GetLength(1); j++)
                    {
                        if (tiles[i, j].active())
                        {
                            Bitmap image = textureLoader.GetTile(tiles[i, j].type);
                            if (image != null)
                            {
                                int frameX = tiles[i, j].frameX == -1 ? 0 : tiles[i, j].frameX;
                                int frameY = tiles[i, j].frameY == -1 ? 0 : tiles[i, j].frameY;
                                image = image.Clone(new Rectangle(frameX, frameY, 16, 16), image.PixelFormat);
                                g.DrawImage(image, (i * 16) + 8, (j * 16) + 8);
                            }
                        }
                    }
                }
            }

            result = result.Clone(new Rectangle(8, 8, tiles.GetLength(0) * 16, tiles.GetLength(1) * 16), result.PixelFormat);
            return WorldToImage.Convert(result);
        }

        public static Point GetWallFrame(Tile[,] tiles, int x, int y)
        {
            Tile tile = tiles[x, y];
            if (tile == null)
            {
                return default;
            }

            if (tile.wall >= 316)
            {
                tile.wall = 0;
            }

            if (tile.wall == 0)
            {
                tile.wallColor(0);
                return default;
            }

            int wallFlag = 0;
            if (y - 1 >= 0)
            {
                Tile tile2 = tiles[x, y - 1];
                if (tile2 != null && (tile2.wall > 0 || (tile2.active() && tile2.type == 54)))
                {
                    wallFlag = 1;
                }
            }

            if (x - 1 >= 0)
            {
                Tile tile2 = tiles[x - 1, y];
                if (tile2 != null && (tile2.wall > 0 || (tile2.active() && tile2.type == 54)))
                {
                    wallFlag |= 2;
                }
            }

            if (x + 1 < tiles.GetLength(0))
            {
                Tile tile2 = tiles[x + 1, y];
                if (tile2 != null && (tile2.wall > 0 || (tile2.active() && tile2.type == 54)))
                {
                    wallFlag |= 4;
                }
            }

            if (x + 1 < tiles.GetLength(1))
            {
                Tile tile2 = tiles[x, y + 1];
                if (tile2 != null && (tile2.wall > 0 || (tile2.active() && tile2.type == 54)))
                {
                    wallFlag |= 8;
                }
            }

            //int flameNumber = 0;
            //if (Main.wallLargeFrames[tile.wall] == 1)
            //{
            //    flameNumber = phlebasTileFrameNumberLookup[y % 4][x % 3] - 1;
            //    tile.wallFrameNumber((byte)flameNumber);
            //}
            //else if (Main.wallLargeFrames[tile.wall] == 2)
            //{
            //    flameNumber = lazureTileFrameNumberLookup[x % 2][y % 2] - 1;
            //    tile.wallFrameNumber((byte)flameNumber);
            //}
            //else
            //{
            //    flameNumber = tile.wallFrameNumber();
            //}

            if (wallFlag == 15)
            {
                wallFlag += centerWallFrameLookup[x % 3][y % 3];
            }

            return wallFrameLookup[wallFlag][0];
        }

        private static void AddWallFrameLookup(int lookup, short point1X, short point1Y, short point2X, short point2Y, short point3X, short point3Y, short point4X, short point4Y)
        {
            Point[] array = new Point[4]
            {
                new Point(point1X * wallFrameSize.X, point1Y * wallFrameSize.Y),
                new Point(point2X * wallFrameSize.X, point2Y * wallFrameSize.Y),
                new Point(point3X * wallFrameSize.X, point3Y * wallFrameSize.Y),
                new Point(point4X * wallFrameSize.X, point4Y * wallFrameSize.Y),
            };
            wallFrameLookup[lookup] = array;
        }
    }
}
