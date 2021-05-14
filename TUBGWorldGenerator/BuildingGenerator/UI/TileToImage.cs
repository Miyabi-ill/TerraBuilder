﻿namespace TUBGWorldGenerator.BuildingGenerator.UI
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Windows.Media.Imaging;
    using Microsoft.Win32;
    using Terraria;
    using Terraria.ID;
    using Terraria.Map;
    using TUBGWorldGenerator.Utils;

    public static class TileToImage
    {
        private struct BlockStyle
        {
            public bool top;

            public bool bottom;

            public bool left;

            public bool right;

            public BlockStyle(bool up, bool down, bool left, bool right)
            {
                top = up;
                bottom = down;
                this.left = left;
                this.right = right;
            }

            public void Clear()
            {
                top = bottom = left = right = false;
            }
        }

        private static readonly Point wallFrameSize;

        private static readonly Point frameSize8Way = new Point(18, 18);

        private static TextureLoader textureLoader;

        private static Point[][] wallFrameLookup;

        private static int[][] centerWallFrameLookup;

        private static BlockStyle[] blockStyleLookup;

        private static Point[][] selfFrame8WayLookup;

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

            blockStyleLookup = new BlockStyle[6];
            blockStyleLookup[0] = new BlockStyle(up: true, down: true, left: true, right: true);
            blockStyleLookup[1] = new BlockStyle(up: false, down: true, left: true, right: true);
            blockStyleLookup[2] = new BlockStyle(up: false, down: true, left: true, right: false);
            blockStyleLookup[3] = new BlockStyle(up: false, down: true, left: false, right: true);
            blockStyleLookup[4] = new BlockStyle(up: true, down: false, left: true, right: false);
            blockStyleLookup[5] = new BlockStyle(up: true, down: false, left: false, right: true);

            selfFrame8WayLookup = new Point[256][];
            Add8WayLookup(0, 9, 3, 10, 3, 11, 3);
            Add8WayLookup(1, 6, 3, 7, 3, 8, 3);
            Add8WayLookup(2, 12, 0, 12, 1, 12, 2);
            Add8WayLookup(3, 15, 2);
            Add8WayLookup(4, 9, 0, 9, 1, 9, 2);
            Add8WayLookup(5, 13, 2);
            Add8WayLookup(6, 6, 4, 7, 4, 8, 4);
            Add8WayLookup(7, 14, 2);
            Add8WayLookup(8, 6, 0, 7, 0, 8, 0);
            Add8WayLookup(9, 5, 0, 5, 1, 5, 2);
            Add8WayLookup(10, 15, 0);
            Add8WayLookup(11, 15, 1);
            Add8WayLookup(12, 13, 0);
            Add8WayLookup(13, 13, 1);
            Add8WayLookup(14, 14, 0);
            Add8WayLookup(15, 14, 1);
            Add8WayLookup(19, 1, 4, 3, 4, 5, 4);
            Add8WayLookup(23, 16, 3);
            Add8WayLookup(27, 17, 0);
            Add8WayLookup(31, 13, 4);
            Add8WayLookup(37, 0, 4, 2, 4, 4, 4);
            Add8WayLookup(39, 17, 3);
            Add8WayLookup(45, 16, 0);
            Add8WayLookup(47, 12, 4);
            Add8WayLookup(55, 1, 2, 2, 2, 3, 2);
            Add8WayLookup(63, 6, 2, 7, 2, 8, 2);
            Add8WayLookup(74, 1, 3, 3, 3, 5, 3);
            Add8WayLookup(75, 17, 1);
            Add8WayLookup(78, 16, 2);
            Add8WayLookup(79, 13, 3);
            Add8WayLookup(91, 4, 0, 4, 1, 4, 2);
            Add8WayLookup(95, 11, 0, 11, 1, 11, 2);
            Add8WayLookup(111, 17, 4);
            Add8WayLookup(127, 14, 3);
            Add8WayLookup(140, 0, 3, 2, 3, 4, 3);
            Add8WayLookup(141, 16, 1);
            Add8WayLookup(142, 17, 2);
            Add8WayLookup(143, 12, 3);
            Add8WayLookup(159, 16, 4);
            Add8WayLookup(173, 0, 0, 0, 1, 0, 2);
            Add8WayLookup(175, 10, 0, 10, 1, 10, 2);
            Add8WayLookup(191, 15, 3);
            Add8WayLookup(206, 1, 0, 2, 0, 3, 0);
            Add8WayLookup(207, 6, 1, 7, 1, 8, 1);
            Add8WayLookup(223, 14, 4);
            Add8WayLookup(239, 15, 4);
            Add8WayLookup(255, 1, 1, 2, 1, 3, 1);
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
                                Point frame = TerrariaIsSoFuck(tiles, i, j);
                                //int frameX = tiles[i, j].frameX == -1 ? frame.X : tiles[i, j].frameX;
                                //int frameY = tiles[i, j].frameY == -1 ? frame.Y : tiles[i, j].frameY;
                                //image = image.Clone(new Rectangle(frameX, frameY, 16, 16), image.PixelFormat);
                                try
                                {
                                    image = image.Clone(new Rectangle(frame.X, frame.Y, 16, 16), image.PixelFormat);
                                }
                                catch (OutOfMemoryException)
                                {
                                    image = image.Clone(new Rectangle(0, 0, 16, 16), image.PixelFormat);
                                }

                                g.DrawImage(image, (i * 16) + 8, (j * 16) + 8);
                            }
                        }
                    }
                }
            }

            // 8pxづつ余裕を持たせたので、切り出す
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

        private static BlockStyle FindBlockStyle(Tile blockTile)
        {
            return blockStyleLookup[blockTile.blockType()];
        }

        private static bool WillItBlend(ushort myType, ushort otherType)
        {
            if (myType == otherType)
            {
                return true;
            }

            if (TileID.Sets.ForcedDirtMerging[myType] && otherType == 0)
            {
                return true;
            }

            if (Main.tileBrick[myType] && Main.tileBrick[otherType])
            {
                return true;
            }

            if (TileID.Sets.GemsparkFramingTypes[otherType] != 0)
            {
                return TileID.Sets.GemsparkFramingTypes[otherType] == TileID.Sets.GemsparkFramingTypes[myType];
            }

            return false;
        }

        private static void Add8WayLookup(int lookup, short point1X, short point1Y, short point2X, short point2Y, short point3X, short point3Y)
        {
            Point[] array = new Point[3]
            {
                new Point(point1X * frameSize8Way.X, point1Y * frameSize8Way.Y),
                new Point(point2X * frameSize8Way.X, point2Y * frameSize8Way.Y),
                new Point(point3X * frameSize8Way.X, point3Y * frameSize8Way.Y),
            };
            selfFrame8WayLookup[lookup] = array;
        }

        private static void Add8WayLookup(int lookup, short x, short y)
        {
            Point[] array = new Point[3]
            {
                new Point(x * frameSize8Way.X, y * frameSize8Way.Y),
                new Point(x * frameSize8Way.X, y * frameSize8Way.Y),
                new Point(x * frameSize8Way.X, y * frameSize8Way.Y),
            };
            selfFrame8WayLookup[lookup] = array;
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

        private static Point SimpleTileFrame(Tile[,] tiles, int x, int y)
        {
            Tile centerTile = tiles[x, y];
            if (!centerTile.active())
            {
                return default;
            }

            if (Main.tileFrameImportant[centerTile.type] && centerTile.type != 4)
            {
                return new Point(centerTile.frameX, centerTile.frameY);
            }

            BlockStyle blockStyle = FindBlockStyle(centerTile);
            int frameFlag = 0;

            // 上マージ有無
            BlockStyle blockStyle2 = default(BlockStyle);
            if (blockStyle.top)
            {
                if (y - 1 >= 0)
                {
                    Tile tile = tiles[x, y - 1];
                    if (tile.active() && WillItBlend(centerTile.type, tile.type))
                    {
                        blockStyle2 = FindBlockStyle(tile);
                        if (blockStyle2.bottom)
                        {
                            frameFlag |= 1;
                        }
                        else
                        {
                            blockStyle2.Clear();
                        }
                    }
                }
            }

            BlockStyle blockStyle3 = default(BlockStyle);
            if (blockStyle.left)
            {
                if (x - 1 >= 0)
                {
                    Tile tile = tiles[x - 1, y];
                    if (tile.active() && WillItBlend(centerTile.type, tile.type))
                    {
                        blockStyle3 = FindBlockStyle(tile);
                        if (blockStyle3.right)
                        {
                            frameFlag |= 2;
                        }
                        else
                        {
                            blockStyle3.Clear();
                        }
                    }
                }
            }

            BlockStyle blockStyle4 = default(BlockStyle);
            if (blockStyle.right)
            {
                if (x + 1 < tiles.GetLength(0))
                {
                    Tile tile = tiles[x + 1, y];
                    if (tile.active() && WillItBlend(centerTile.type, tile.type))
                    {
                        blockStyle4 = FindBlockStyle(tile);
                        if (blockStyle4.left)
                        {
                            frameFlag |= 4;
                        }
                        else
                        {
                            blockStyle4.Clear();
                        }
                    }
                }
            }

            BlockStyle blockStyle5 = default(BlockStyle);
            if (blockStyle.bottom)
            {
                if (y + 1 < tiles.GetLength(1))
                {
                    Tile tile = tiles[x, y + 1];
                    if (tile.active() && WillItBlend(centerTile.type, tile.type))
                    {
                        blockStyle5 = FindBlockStyle(tile);
                        if (blockStyle5.top)
                        {
                            frameFlag |= 8;
                        }
                        else
                        {
                            blockStyle5.Clear();
                        }
                    }
                }
            }

            if (blockStyle2.left && blockStyle3.top)
            {
                if (x - 1 >= 0 && y - 1 >= 0)
                {
                    Tile tile = tiles[x - 1, y - 1];
                    if (tile.active() && WillItBlend(centerTile.type, tile.type))
                    {
                        BlockStyle blockStyle6 = FindBlockStyle(tile);
                        if (blockStyle6.right && blockStyle6.bottom)
                        {
                            frameFlag |= 0x10;
                        }
                    }
                }
            }

            if (blockStyle2.right && blockStyle4.top)
            {
                if (x + 1 < tiles.GetLength(0) && y - 1 >= 0)
                {
                    Tile tile = tiles[x + 1, y - 1];
                    if (tile.active() && WillItBlend(centerTile.type, tile.type))
                    {
                        BlockStyle blockStyle7 = FindBlockStyle(tile);
                        if (blockStyle7.left && blockStyle7.bottom)
                        {
                            frameFlag |= 0x20;
                        }
                    }
                }
            }

            if (blockStyle5.left && blockStyle3.bottom)
            {
                if (x - 1 >= 0 && y + 1 < tiles.GetLength(1))
                {
                    Tile tile = tiles[x - 1, y + 1];
                    if (tile.active() && WillItBlend(centerTile.type, tile.type))
                    {
                        BlockStyle blockStyle8 = FindBlockStyle(tile);
                        if (blockStyle8.right && blockStyle8.top)
                        {
                            frameFlag |= 0x40;
                        }
                    }
                }
            }

            if (blockStyle5.right && blockStyle4.bottom)
            {
                if (x + 1 < tiles.GetLength(0) && y + 1 < tiles.GetLength(1))
                {
                    Tile tile = tiles[x + 1, y + 1];
                    if (tile.active() && WillItBlend(centerTile.type, tile.type))
                    {
                        BlockStyle blockStyle9 = FindBlockStyle(tile);
                        if (blockStyle9.left && blockStyle9.top)
                        {
                            frameFlag |= 0x80;
                        }
                    }
                }
            }

            return selfFrame8WayLookup[frameFlag][0];
        }

        private static Point TerrariaIsSoFuck(Tile[,] tiles, int x, int y)
        {
            bool mergeUp;
            bool mergeDown;
            bool mergeLeft;
            bool mergeRight;

            Tile tile = tiles[x, y];
            int tileType = tile.type;
            if (!tile.active())
            {
                return default;
            }

            if (Main.tileFrameImportant[tileType] && tileType != 4)
            {
                return new Point(tiles[x, y].frameX, tiles[x, y].frameY);
            }

            // 石と同じ扱いにする判定を楽にするため、同じIDで判定
            if (Main.tileStone[tileType])
            {
                tileType = 1;
            }

            // GemSpark
            if ((tileType >= 255
                && tileType <= 268)
                || tileType == 385
                || tileType == 446
                || tileType == 447
                || tileType == 448)
            {
                SimpleTileFrame(tiles, x, y);
                return default;
            }

            Rectangle tileFrame = new Rectangle(-1, -1, 0, 0);

            int upLeft = -1;
            int up = -1;
            int upRight = -1;
            int left = -1;
            int right = -1;
            int downLeft = -1;
            int down = -1;
            int downRight = -1;

            Tile tile2 = null;
            if (x - 1 >= 0)
            {
                tile2 = tiles[x - 1, y];
                if (tile2 != null && tile2.active())
                {
                    left = (Main.tileStone[tile2.type] ? 1 : tile2.type);
                    if (tile2.slope() == 1 || tile2.slope() == 3)
                    {
                        left = -1;
                    }
                }
            }

            Tile tile3 = null;
            if (x + 1 < tiles.GetLength(0))
            {
                tile3 = tiles[x + 1, y];
                if (tile3 != null && tile3.active())
                {
                    right = (Main.tileStone[tile3.type] ? 1 : tile3.type);
                    if (tile3.slope() == 2 || tile3.slope() == 4)
                    {
                        right = -1;
                    }
                }
            }

            Tile tile8 = null;
            if (y - 1 >= 0)
            {
                tile8 = tiles[x, y - 1];
                if (tile8 != null && tile8.active())
                {
                    up = (Main.tileStone[tile8.type] ? 1 : tile8.type);
                    if (tile8.slope() == 3 || tile8.slope() == 4)
                    {
                        up = -1;
                    }
                }
            }

            Tile tile9 = null;
            if (y + 1 < tiles.GetLength(1))
            {
                tile9 = tiles[x, y + 1];
                if (tile9 != null && tile9.active())
                {
                    down = (Main.tileStone[tile9.type] ? 1 : tile9.type);
                    if (tile9.slope() == 1 || tile9.slope() == 2)
                    {
                        down = -1;
                    }
                }
            }

            Tile tile6 = null;
            if (x - 1 >= 0 && y - 1 >= 0)
            {
                tile6 = tiles[x - 1, y - 1];
                if (tile6 != null && tile6.active())
                {
                    upLeft = (Main.tileStone[tile6.type] ? 1 : tile6.type);
                }
            }

            Tile tile7 = null;
            if (x + 1 < tiles.GetLength(0) && y - 1 >= 0)
            {
                tile7 = tiles[x + 1, y - 1];
                if (tile7 != null && tile7.active())
                {
                    upRight = (Main.tileStone[tile7.type] ? 1 : tile7.type);
                }
            }

            Tile tile4 = null;
            if (x - 1 >= 0 && y + 1 < tiles.GetLength(1))
            {
                tile4 = tiles[x - 1, y + 1];
                if (tile4 != null && tile4.active())
                {
                    downLeft = (Main.tileStone[tile4.type] ? 1 : tile4.type);
                }
            }

            Tile tile5 = null;
            if (x + 1 < tiles.GetLength(0) && y + 1 < tiles.GetLength(1))
            {
                tile5 = tiles[x + 1, y + 1];
                if (tile5 != null && tile5.active())
                {
                    downRight = (Main.tileStone[tile5.type] ? 1 : tile5.type);
                }
            }

            if (tile.slope() == 2)
            {
                up = -1;
                left = -1;
            }
            if (tile.slope() == 1)
            {
                up = -1;
                right = -1;
            }
            if (tile.slope() == 4)
            {
                down = -1;
                left = -1;
            }
            if (tile.slope() == 3)
            {
                down = -1;
                right = -1;
            }

            switch (tileType)
            {
                case 147:
                    WorldGen.TileMergeAttempt(tileType, Main.tileBrick, TileID.Sets.Ices, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
                    break;
                case 161:
                case 163:
                case 164:
                case 200:
                    WorldGen.TileMergeAttempt(tileType, Main.tileBrick, TileID.Sets.Snow, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
                    break;
                case 162:
                    WorldGen.TileMergeAttempt(tileType, Main.tileBrick, TileID.Sets.IcesSnow, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
                    break;
                default:
                    if (Main.tileBrick[tileType])
                    {
                        if (tileType == 60 || tileType == 70)
                        {
                            WorldGen.TileMergeAttempt(tileType, Main.tileBrick, TileID.Sets.Mud, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
                        }
                        else
                        {
                            WorldGen.TileMergeAttempt(tileType, Main.tileBrick, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
                        }
                    }
                    else if (Main.tilePile[tileType])
                    {
                        WorldGen.TileMergeAttempt(tileType, Main.tilePile, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
                    }
                    break;
            }
            if ((tileType == 1 || Main.tileMoss[tileType] || tileType == 117 || tileType == 25 || tileType == 203) && down == 165 && tile9 != null)
            {
                if (tile9.frameY == 72)
                {
                    down = tileType;
                }
                else if (tile9.frameY == 0)
                {
                    down = tileType;
                }
            }
            if ((tileType == 1 || Main.tileMoss[tileType] || tileType == 117 || tileType == 25 || tileType == 203) && up == 165 && tile8 != null)
            {
                if (tile8.frameY == 90)
                {
                    up = tileType;
                }
                else if (tile8.frameY == 54)
                {
                    up = tileType;
                }
            }
            if (tileType == 225)
            {
                if (down == 165)
                {
                    down = tileType;
                }
                if (up == 165)
                {
                    up = tileType;
                }
            }
            if ((tileType == 200 || tileType == 161 || tileType == 147 || tileType == 163 || tileType == 164) && down == 165)
            {
                down = tileType;
            }
            if ((tile.slope() == 1 || tile.slope() == 2) && down > -1 && !TileID.Sets.Platforms[down])
            {
                down = tileType;
            }
            if (up > -1 && (tile8.slope() == 1 || tile8.slope() == 2) && !TileID.Sets.Platforms[up])
            {
                up = tileType;
            }
            if ((tile.slope() == 3 || tile.slope() == 4) && up > -1 && !TileID.Sets.Platforms[up])
            {
                up = tileType;
            }
            if (down > -1 && (tile9.slope() == 3 || tile9.slope() == 4) && !TileID.Sets.Platforms[down])
            {
                down = tileType;
            }
            if (tileType == 124)
            {
                if (up > -1 && Main.tileSolid[up] && !TileID.Sets.Platforms[up])
                {
                    up = tileType;
                }
                if (down > -1 && Main.tileSolid[down] && !TileID.Sets.Platforms[down])
                {
                    down = tileType;
                }
            }

            if (up > -1 && tile8.halfBrick() && !TileID.Sets.Platforms[up])
            {
                up = tileType;
            }

            if (left > -1 && tile2?.halfBrick() == true)
            {
                if (tile.halfBrick())
                {
                    left = tileType;
                }
                else if (tile2.type != tileType)
                {
                    left = -1;
                }
            }

            if (right > -1 && tile3?.halfBrick() == true)
            {
                if (tile.halfBrick())
                {
                    right = tileType;
                }
                else if (tile3.type != tileType)
                {
                    right = -1;
                }
            }

            if (tile.halfBrick())
            {
                if (left != tileType)
                {
                    left = -1;
                }
                if (right != tileType)
                {
                    right = -1;
                }
                up = -1;
            }

            if (tile9 != null && tile9.halfBrick())
            {
                down = -1;
            }

            mergeUp = false;
            mergeDown = false;
            mergeLeft = false;
            mergeRight = false;

            if (!Main.tileRope[tileType] && TileID.Sets.BlockMergesWithMergeAllBlock[tileType])
            {
                WorldGen.TileMergeAttempt(tileType, Main.tileBlendAll, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
            }
            if (Main.tileBlendAll[tileType])
            {
                WorldGen.TileMergeAttempt(tileType, TileID.Sets.BlockMergesWithMergeAllBlock, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
            }
            if (TileID.Sets.ForcedDirtMerging[tileType])
            {
                if (up == 0)
                {
                    up = tileType;
                }
                if (down == 0)
                {
                    down = tileType;
                }
                if (left == 0)
                {
                    left = tileType;
                }
                if (right == 0)
                {
                    right = tileType;
                }
                if (upLeft == 0)
                {
                    upLeft = tileType;
                }
                if (upRight == 0)
                {
                    upRight = tileType;
                }
                if (downLeft == 0)
                {
                    downLeft = tileType;
                }
                if (downRight == 0)
                {
                    downRight = tileType;
                }
            }

            switch (tileType)
            {
                case 0:
                    {
                        if (up > -1 && Main.tileMergeDirt[up])
                        {
                            //TileFrame(i, j - 1);
                            if (mergeDown)
                            {
                                up = tileType;
                            }
                        }
                        else if (up == 147)
                        {
                            //TileFrame(i, j - 1);
                            if (mergeDown)
                            {
                                up = tileType;
                            }
                        }
                        if (down > -1 && Main.tileMergeDirt[down])
                        {
                            //TileFrame(i, j + 1);
                            if (mergeUp)
                            {
                                down = tileType;
                            }
                        }
                        else if (down == 147)
                        {
                            //TileFrame(i, j + 1);
                            if (mergeUp)
                            {
                                down = tileType;
                            }
                        }
                        if (left > -1 && Main.tileMergeDirt[left])
                        {
                            //TileFrame(i - 1, j);
                            if (mergeRight)
                            {
                                left = tileType;
                            }
                        }
                        else if (left == 147)
                        {
                            //TileFrame(i - 1, j);
                            if (mergeRight)
                            {
                                left = tileType;
                            }
                        }
                        if (right > -1 && Main.tileMergeDirt[right])
                        {
                            //TileFrame(i + 1, j);
                            if (mergeLeft)
                            {
                                right = tileType;
                            }
                        }
                        else if (right == 147)
                        {
                            //TileFrame(i + 1, j);
                            if (mergeLeft)
                            {
                                right = tileType;
                            }
                        }
                        bool[] mergesWithDirtInASpecialWay = TileID.Sets.Conversion.MergesWithDirtInASpecialWay;
                        if (up > -1 && mergesWithDirtInASpecialWay[up])
                        {
                            up = tileType;
                        }
                        if (down > -1 && mergesWithDirtInASpecialWay[down])
                        {
                            down = tileType;
                        }
                        if (left > -1 && mergesWithDirtInASpecialWay[left])
                        {
                            left = tileType;
                        }
                        if (right > -1 && mergesWithDirtInASpecialWay[right])
                        {
                            right = tileType;
                        }
                        if (upLeft > -1 && Main.tileMergeDirt[upLeft])
                        {
                            upLeft = tileType;
                        }
                        else if (upLeft > -1 && mergesWithDirtInASpecialWay[upLeft])
                        {
                            upLeft = tileType;
                        }
                        if (upRight > -1 && Main.tileMergeDirt[upRight])
                        {
                            upRight = tileType;
                        }
                        else if (upRight > -1 && mergesWithDirtInASpecialWay[upRight])
                        {
                            upRight = tileType;
                        }
                        if (downLeft > -1 && Main.tileMergeDirt[downLeft])
                        {
                            downLeft = tileType;
                        }
                        else if (downLeft > -1 && mergesWithDirtInASpecialWay[downLeft])
                        {
                            downLeft = tileType;
                        }
                        if (downRight > -1 && Main.tileMergeDirt[downRight])
                        {
                            downRight = tileType;
                        }
                        else if (downRight > -1 && mergesWithDirtInASpecialWay[downRight])
                        {
                            downRight = tileType;
                        }
                        WorldGen.TileMergeAttempt(-2, 59, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
                        WorldGen.TileMergeAttempt(tileType, 191, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
                        if (up > -1 && TileID.Sets.ForcedDirtMerging[up])
                        {
                            up = tileType;
                        }
                        if (down > -1 && TileID.Sets.ForcedDirtMerging[down])
                        {
                            down = tileType;
                        }
                        if (left > -1 && TileID.Sets.ForcedDirtMerging[left])
                        {
                            left = tileType;
                        }
                        if (right > -1 && TileID.Sets.ForcedDirtMerging[right])
                        {
                            right = tileType;
                        }
                        if (upLeft > -1 && TileID.Sets.ForcedDirtMerging[upLeft])
                        {
                            upLeft = tileType;
                        }
                        if (upRight > -1 && TileID.Sets.ForcedDirtMerging[upRight])
                        {
                            upRight = tileType;
                        }
                        if (downLeft > -1 && TileID.Sets.ForcedDirtMerging[downLeft])
                        {
                            downLeft = tileType;
                        }
                        if (downRight > -1 && TileID.Sets.ForcedDirtMerging[downRight])
                        {
                            downRight = tileType;
                        }
                        break;
                    }
                case 213:
                    if (up > -1 && Main.tileSolid[up] && !Main.tileSolidTop[up])
                    {
                        up = tileType;
                    }
                    if (down > -1 && Main.tileSolid[down])
                    {
                        down = tileType;
                    }
                    if (up != tileType)
                    {
                        if (left > -1 && Main.tileSolid[left])
                        {
                            left = tileType;
                        }
                        if (right > -1 && Main.tileSolid[right])
                        {
                            right = tileType;
                        }
                    }
                    break;
                case 53:
                    WorldGen.TileMergeAttemptFrametest(x, y, tileType, 397, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
                    WorldGen.TileMergeAttemptFrametest(x, y, tileType, 396, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
                    break;
                case 234:
                    WorldGen.TileMergeAttemptFrametest(x, y, tileType, 399, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
                    WorldGen.TileMergeAttemptFrametest(x, y, tileType, 401, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
                    break;
                case 112:
                    WorldGen.TileMergeAttemptFrametest(x, y, tileType, 398, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
                    WorldGen.TileMergeAttemptFrametest(x, y, tileType, 400, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
                    break;
                case 116:
                    WorldGen.TileMergeAttemptFrametest(x, y, tileType, 402, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
                    WorldGen.TileMergeAttemptFrametest(x, y, tileType, 403, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
                    break;
            }

            if (Main.tileMergeDirt[tileType])
            {
                WorldGen.TileMergeAttempt(-2, 0, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
                if (tileType == 1)
                {
                    if ((double)y > Main.rockLayer)
                    {
                        WorldGen.TileMergeAttemptFrametest(x, y, tileType, 59, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
                    }
                    WorldGen.TileMergeAttemptFrametest(x, y, tileType, 57, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
                }
            }
            else
            {
                switch (tileType)
                {
                    case 58:
                    case 75:
                    case 76:
                        WorldGen.TileMergeAttempt(-2, 57, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
                        break;
                    case 57:
                        WorldGen.TileMergeAttempt(-2, 1, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
                        WorldGen.TileMergeAttemptFrametest(x, y, tileType, TileID.Sets.HellSpecial, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
                        break;
                    case 59:
                        if ((double)y > Main.rockLayer)
                        {
                            WorldGen.TileMergeAttempt(-2, 1, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
                        }
                        WorldGen.TileMergeAttempt(tileType, TileID.Sets.GrassSpecial, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
                        WorldGen.TileMergeAttemptFrametest(x, y, tileType, TileID.Sets.JungleSpecial, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
                        if ((double)y < Main.rockLayer)
                        {
                            WorldGen.TileMergeAttemptFrametest(x, y, tileType, 0, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
                        }
                        else
                        {
                            WorldGen.TileMergeAttempt(tileType, 0, ref up, ref down, ref left, ref right);
                        }
                        break;
                    case 211:
                        WorldGen.TileMergeAttempt(59, 60, ref up, ref down, ref left, ref right);
                        WorldGen.TileMergeAttempt(-2, 59, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
                        break;
                    case 225:
                    case 226:
                        WorldGen.TileMergeAttempt(-2, 59, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
                        break;
                    case 60:
                        WorldGen.TileMergeAttempt(59, 211, ref up, ref down, ref left, ref right);
                        break;
                    case 189:
                        WorldGen.TileMergeAttemptFrametest(x, y, tileType, TileID.Sets.MergesWithClouds, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
                        break;
                    case 196:
                        WorldGen.TileMergeAttempt(-2, 189, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
                        WorldGen.TileMergeAttempt(tileType, 460, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
                        break;
                    case 460:
                        WorldGen.TileMergeAttempt(-2, 189, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
                        WorldGen.TileMergeAttempt(tileType, 196, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
                        break;
                    case 147:
                        WorldGen.TileMergeAttemptFrametest(x, y, tileType, TileID.Sets.IcesSlush, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
                        break;
                    case 161:
                    case 163:
                    case 164:
                    case 200:
                    case 224:
                        WorldGen.TileMergeAttempt(-2, 147, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
                        break;
                    case 162:
                        WorldGen.TileMergeAttempt(-2, TileID.Sets.IcesSnow, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
                        break;
                    case 32:
                        if (down == 23)
                        {
                            down = tileType;
                        }
                        break;
                    case 352:
                        if (down == 199)
                        {
                            down = tileType;
                        }
                        break;
                    case 69:
                        if (down == 60)
                        {
                            down = tileType;
                        }
                        break;
                    case 51:
                        WorldGen.TileMergeAttempt(tileType, TileID.Sets.AllTiles, Main.tileNoAttach, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
                        break;
                    case 192:
                        WorldGen.TileMergeAttemptFrametest(x, y, tileType, 191, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
                        break;
                    case 191:
                        WorldGen.TileMergeAttempt(-2, 192, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
                        WorldGen.TileMergeAttempt(tileType, 0, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
                        break;
                    case 384:
                        WorldGen.TileMergeAttemptFrametest(x, y, tileType, 383, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
                        break;
                    case 383:
                        WorldGen.TileMergeAttempt(-2, 384, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
                        WorldGen.TileMergeAttempt(tileType, 59, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
                        break;
                    case 407:
                        WorldGen.TileMergeAttempt(-2, 404, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
                        break;
                    case 404:
                        WorldGen.TileMergeAttempt(-2, 396, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
                        WorldGen.TileMergeAttemptFrametest(x, y, tileType, 407, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
                        break;
                    case 397:
                        WorldGen.TileMergeAttempt(-2, 53, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
                        WorldGen.TileMergeAttemptFrametest(x, y, tileType, 396, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
                        break;
                    case 396:
                        WorldGen.TileMergeAttempt(-2, 397, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
                        WorldGen.TileMergeAttempt(-2, 53, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
                        WorldGen.TileMergeAttemptFrametest(x, y, tileType, 404, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
                        break;
                    case 398:
                        WorldGen.TileMergeAttempt(-2, 112, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
                        WorldGen.TileMergeAttemptFrametest(x, y, tileType, 400, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
                        break;
                    case 400:
                        WorldGen.TileMergeAttempt(-2, 398, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
                        WorldGen.TileMergeAttempt(-2, 112, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
                        break;
                    case 399:
                        WorldGen.TileMergeAttempt(-2, 234, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
                        WorldGen.TileMergeAttemptFrametest(x, y, tileType, 401, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
                        break;
                    case 401:
                        WorldGen.TileMergeAttempt(-2, 399, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
                        WorldGen.TileMergeAttempt(-2, 234, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
                        break;
                    case 402:
                        WorldGen.TileMergeAttempt(-2, 116, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
                        WorldGen.TileMergeAttemptFrametest(x, y, tileType, 403, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
                        break;
                    case 403:
                        WorldGen.TileMergeAttempt(-2, 402, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
                        WorldGen.TileMergeAttempt(-2, 116, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
                        break;
                }
            }
            if (tileType == 0)
            {
                WorldGen.TileMergeAttempt(tileType, Main.tileMoss, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
                WorldGen.TileMergeAttempt(tileType, TileID.Sets.tileMossBrick, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
            }
            else if (Main.tileMoss[tileType] || TileID.Sets.tileMossBrick[tileType])
            {
                WorldGen.TileMergeAttempt(tileType, 0, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
            }
            else if (Main.tileStone[tileType] || tileType == 1)
            {
                WorldGen.TileMergeAttempt(tileType, Main.tileMoss, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
            }
            else if (tileType == 38)
            {
                WorldGen.TileMergeAttempt(tileType, TileID.Sets.tileMossBrick, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
            }
            if (TileID.Sets.Conversion.Grass[tileType])
            {
                WorldGen.TileMergeAttempt(tileType, TileID.Sets.Ore, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
            }
            else if (TileID.Sets.Ore[tileType])
            {
                WorldGen.TileMergeAttempt(tileType, TileID.Sets.Conversion.Grass, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
            }
            if (tileType == 59)
            {
                WorldGen.TileMergeAttempt(tileType, TileID.Sets.OreMergesWithMud, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
            }
            else if (TileID.Sets.OreMergesWithMud[tileType])
            {
                WorldGen.TileMergeAttempt(tileType, 59, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
            }

            bool flag = false;
            if (tileType == 2 || tileType == 23 || tileType == 60 || tileType == 477 || tileType == 492 || tileType == 70 || tileType == 109 || tileType == 199 || Main.tileMoss[tileType] || TileID.Sets.NeedsGrassFraming[tileType] || TileID.Sets.tileMossBrick[tileType])
            {
                flag = true;
                WorldGen.TileMergeAttemptWeird(tileType, -1, Main.tileSolid, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
                int num32 = TileID.Sets.NeedsGrassFramingDirt[tileType];
                if (tileType == 60 || tileType == 70)
                {
                    num32 = 59;
                }
                else if (Main.tileMoss[tileType])
                {
                    num32 = 1;
                }
                else if (TileID.Sets.tileMossBrick[tileType])
                {
                    num32 = 38;
                }
                else
                {
                    switch (tileType)
                    {
                        case 2:
                        case 477:
                            WorldGen.TileMergeAttempt(num32, 23, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
                            break;
                        case 23:
                            WorldGen.TileMergeAttempt(num32, 2, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
                            break;
                    }
                }
                if (up != tileType && up != num32 && (down == tileType || down == num32))
                {
                    if (left == num32 && right == tileType)
                    {
                        tileFrame.X = 0;
                        tileFrame.Y = 198;
                    }
                    else if (left == tileType && right == num32)
                    {
                        tileFrame.X = 54;
                        tileFrame.Y = 198;
                    }
                }
                else if (down != tileType && down != num32 && (up == tileType || up == num32))
                {
                    if (left == num32 && right == tileType)
                    {
                        tileFrame.X = 0;
                        tileFrame.Y = 216;
                    }
                    else if (left == tileType && right == num32)
                    {
                        tileFrame.X = 54;
                        tileFrame.Y = 216;
                    }
                }
                else if (left != tileType && left != num32 && (right == tileType || right == num32))
                {
                    if (up == num32 && down == tileType)
                    {
                        tileFrame.X = 72;
                        tileFrame.Y = 144;
                    }
                    else if (down == tileType && up == num32)
                    {
                        tileFrame.X = 72;
                        tileFrame.Y = 90;
                    }
                }
                else if (right != tileType && right != num32 && (left == tileType || left == num32))
                {
                    if (up == num32 && down == tileType)
                    {
                        tileFrame.X = 90;
                        tileFrame.Y = 144;
                    }
                    else if (down == tileType && right == up)
                    {
                        tileFrame.X = 90;
                        tileFrame.Y = 90;
                    }
                }
                else if (up == tileType && down == tileType && left == tileType && right == tileType)
                {
                    if (upLeft != tileType && upRight != tileType && downLeft != tileType && downRight != tileType)
                    {
                        if (downRight == num32)
                        {
                            tileFrame.X = 108;
                            tileFrame.Y = 324;
                        }
                        else if (upRight == num32)
                        {
                            tileFrame.X = 108;
                            tileFrame.Y = 342;
                        }
                        else if (downLeft == num32)
                        {
                            tileFrame.X = 108;
                            tileFrame.Y = 360;
                        }
                        else if (upLeft == num32)
                        {
                            tileFrame.X = 108;
                            tileFrame.Y = 378;
                        }
                        else
                        {
                            tileFrame.X = 144;
                            tileFrame.Y = 234;
                        }
                    }
                    else if (upLeft != tileType && downRight != tileType)
                    {
                        tileFrame.X = 36;
                        tileFrame.Y = 306;
                    }
                    else if (upRight != tileType && downLeft != tileType)
                    {
                        tileFrame.X = 90;
                        tileFrame.Y = 306;
                    }
                    else if (upLeft != tileType && upRight == tileType && downLeft == tileType && downRight == tileType)
                    {
                        tileFrame.X = 54;
                        tileFrame.Y = 108;
                    }
                    else if (upLeft == tileType && upRight != tileType && downLeft == tileType && downRight == tileType)
                    {
                        tileFrame.X = 36;
                        tileFrame.Y = 108;
                    }
                    else if (upLeft == tileType && upRight == tileType && downLeft != tileType && downRight == tileType)
                    {
                        tileFrame.X = 54;
                        tileFrame.Y = 90;
                    }
                    else if (upLeft == tileType && upRight == tileType && downLeft == tileType && downRight != tileType)
                    {
                        tileFrame.X = 36;
                        tileFrame.Y = 90;
                    }
                }
                else if (up == tileType && down == num32 && left == tileType && right == tileType && upLeft == -1 && upRight == -1)
                {
                    tileFrame.X = 108;
                    tileFrame.Y = 18;
                }
                else if (up == num32 && down == tileType && left == tileType && right == tileType && downLeft == -1 && downRight == -1)
                {
                    tileFrame.X = 108;
                    tileFrame.Y = 36;
                }
                else if (up == tileType && down == tileType && left == num32 && right == tileType && upRight == -1 && downRight == -1)
                {
                    tileFrame.X = 198;
                    tileFrame.Y = 0;
                }
                else if (up == tileType && down == tileType && left == tileType && right == num32 && upLeft == -1 && downLeft == -1)
                {
                    tileFrame.X = 180;
                    tileFrame.Y = 0;
                }
                else if (up == tileType && down == num32 && left == tileType && right == tileType)
                {
                    if (upRight != -1)
                    {
                        tileFrame.X = 54;
                        tileFrame.Y = 108;
                    }
                    else if (upLeft != -1)
                    {
                        tileFrame.X = 36;
                        tileFrame.Y = 108;
                    }
                }
                else if (up == num32 && down == tileType && left == tileType && right == tileType)
                {
                    if (downRight != -1)
                    {
                        tileFrame.X = 54;
                        tileFrame.Y = 90;
                    }
                    else if (downLeft != -1)
                    {
                        tileFrame.X = 36;
                        tileFrame.Y = 90;
                    }
                }
                else if (up == tileType && down == tileType && left == tileType && right == num32)
                {
                    if (upLeft != -1)
                    {
                        tileFrame.X = 54;
                        tileFrame.Y = 90;
                    }
                    else if (downLeft != -1)
                    {
                        tileFrame.X = 54;
                        tileFrame.Y = 108;
                    }
                }
                else if (up == tileType && down == tileType && left == num32 && right == tileType)
                {
                    if (upRight != -1)
                    {
                        tileFrame.X = 36;
                        tileFrame.Y = 90;
                    }
                    else if (downRight != -1)
                    {
                        tileFrame.X = 36;
                        tileFrame.Y = 108;
                    }
                }
                else if ((up == num32 && down == tileType && left == tileType && right == tileType) || (up == tileType && down == num32 && left == tileType && right == tileType) || (up == tileType && down == tileType && left == num32 && right == tileType) || (up == tileType && down == tileType && left == tileType && right == num32))
                {
                    tileFrame.X = 18;
                    tileFrame.Y = 18;
                }
                if ((up == tileType || up == num32) && (down == tileType || down == num32) && (left == tileType || left == num32) && (right == tileType || right == num32))
                {
                    if (upLeft != tileType && upLeft != num32 && (upRight == tileType || upRight == num32) && (downLeft == tileType || downLeft == num32) && (downRight == tileType || downRight == num32))
                    {
                        tileFrame.X = 54;
                        tileFrame.Y = 108;
                    }
                    else if (upRight != tileType && upRight != num32 && (upLeft == tileType || upLeft == num32) && (downLeft == tileType || downLeft == num32) && (downRight == tileType || downRight == num32))
                    {
                        tileFrame.X = 36;
                        tileFrame.Y = 108;
                    }
                    else if (downLeft != tileType && downLeft != num32 && (upLeft == tileType || upLeft == num32) && (upRight == tileType || upRight == num32) && (downRight == tileType || downRight == num32))
                    {
                        tileFrame.X = 54;
                        tileFrame.Y = 90;
                    }
                    else if (downRight != tileType && downRight != num32 && (upLeft == tileType || upLeft == num32) && (downLeft == tileType || downLeft == num32) && (upRight == tileType || upRight == num32))
                    {
                        tileFrame.X = 36;
                        tileFrame.Y = 90;
                    }
                }
                if (up != num32 && up != tileType && down == tileType && left != num32 && left != tileType && right == tileType && downRight != num32 && downRight != tileType)
                {
                    tileFrame.X = 90;
                    tileFrame.Y = 270;
                }
                else if (up != num32 && up != tileType && down == tileType && left == tileType && right != num32 && right != tileType && downLeft != num32 && downLeft != tileType)
                {
                    tileFrame.X = 144;
                    tileFrame.Y = 270;
                }
                else if (down != num32 && down != tileType && up == tileType && left != num32 && left != tileType && right == tileType && upRight != num32 && upRight != tileType)
                {
                    tileFrame.X = 90;
                    tileFrame.Y = 288;
                }
                else if (down != num32 && down != tileType && up == tileType && left == tileType && right != num32 && right != tileType && upLeft != num32 && upLeft != tileType)
                {
                    tileFrame.X = 144;
                    tileFrame.Y = 288;
                }
                else if (up != tileType && up != num32 && down == tileType && left == tileType && right == tileType && downLeft != tileType && downLeft != num32 && downRight != tileType && downRight != num32)
                {
                    tileFrame.X = 144;
                    tileFrame.Y = 216;
                }
                else if (down != tileType && down != num32 && up == tileType && left == tileType && right == tileType && upLeft != tileType && upLeft != num32 && upRight != tileType && upRight != num32)
                {
                    tileFrame.X = 144;
                    tileFrame.Y = 252;
                }
                else if (left != tileType && left != num32 && down == tileType && up == tileType && right == tileType && upRight != tileType && upRight != num32 && downRight != tileType && downRight != num32)
                {
                    tileFrame.X = 126;
                    tileFrame.Y = 234;
                }
                else if (right != tileType && right != num32 && down == tileType && up == tileType && left == tileType && upLeft != tileType && upLeft != num32 && downLeft != tileType && downLeft != num32)
                {
                    tileFrame.X = 162;
                    tileFrame.Y = 234;
                }
                else if (up != num32 && up != tileType && (down == num32 || down == tileType) && left == num32 && right == num32)
                {
                    tileFrame.X = 36;
                    tileFrame.Y = 270;
                }
                else if (down != num32 && down != tileType && (up == num32 || up == tileType) && left == num32 && right == num32)
                {
                    tileFrame.X = 36;
                    tileFrame.Y = 288;
                }
                else if (left != num32 && left != tileType && (right == num32 || right == tileType) && up == num32 && down == num32)
                {
                    tileFrame.X = 0;
                    tileFrame.Y = 270;
                }
                else if (right != num32 && right != tileType && (left == num32 || left == tileType) && up == num32 && down == num32)
                {
                    tileFrame.X = 18;
                    tileFrame.Y = 270;
                }
                else if (up == tileType && down == num32 && left == num32 && right == num32)
                {
                    tileFrame.X = 198;
                    tileFrame.Y = 288;
                }
                else if (up == num32 && down == tileType && left == num32 && right == num32)
                {
                    tileFrame.X = 198;
                    tileFrame.Y = 270;
                }
                else if (up == num32 && down == num32 && left == tileType && right == num32)
                {
                    tileFrame.X = 198;
                    tileFrame.Y = 306;
                }
                else if (up == num32 && down == num32 && left == num32 && right == tileType)
                {
                    tileFrame.X = 144;
                    tileFrame.Y = 306;
                }

                if (up != tileType && up != num32 && down == tileType && left == tileType && right == tileType)
                {
                    if ((downLeft == num32 || downLeft == tileType) && downRight != num32 && downRight != tileType)
                    {
                        tileFrame.X = 0;
                        tileFrame.Y = 324;
                    }
                    else if ((downRight == num32 || downRight == tileType) && downLeft != num32 && downLeft != tileType)
                    {
                        tileFrame.X = 54;
                        tileFrame.Y = 324;
                    }
                }
                else if (down != tileType && down != num32 && up == tileType && left == tileType && right == tileType)
                {
                    if ((upLeft == num32 || upLeft == tileType) && upRight != num32 && upRight != tileType)
                    {
                        tileFrame.X = 0;
                        tileFrame.Y = 342;
                    }
                    else if ((upRight == num32 || upRight == tileType) && upLeft != num32 && upLeft != tileType)
                    {
                        tileFrame.X = 54;
                        tileFrame.Y = 342;
                    }
                }
                else if (left != tileType && left != num32 && up == tileType && down == tileType && right == tileType)
                {
                    if ((upRight == num32 || upRight == tileType) && downRight != num32 && downRight != tileType)
                    {
                        tileFrame.X = 54;
                        tileFrame.Y = 360;
                    }
                    else if ((downRight == num32 || downRight == tileType) && upRight != num32 && upRight != tileType)
                    {
                        tileFrame.X = 0;
                        tileFrame.Y = 360;
                    }
                }
                else if (right != tileType && right != num32 && up == tileType && down == tileType && left == tileType)
                {
                    if ((upLeft == num32 || upLeft == tileType) && downLeft != num32 && downLeft != tileType)
                    {
                        tileFrame.X = 0;
                        tileFrame.Y = 378;
                    }
                    else if ((downLeft == num32 || downLeft == tileType) && upLeft != num32 && upLeft != tileType)
                    {
                        tileFrame.X = 54;
                        tileFrame.Y = 378;
                    }
                }
                if ((up == tileType || up == num32) && (down == tileType || down == num32) && (left == tileType || left == num32) && (right == tileType || right == num32) && upLeft != -1 && upRight != -1 && downLeft != -1 && downRight != -1)
                {
                    if ((x + y) % 2 == 1)
                    {
                        tileFrame.X = 108;
                        tileFrame.Y = 198;
                    }
                    else
                    {
                        tileFrame.X = 18;
                        tileFrame.Y = 18;
                    }
                }

                WorldGen.TileMergeAttempt(-2, num32, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
            }

            WorldGen.TileMergeAttempt(tileType, Main.tileMerge[tileType], ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
            if (tileFrame.X == -1 && tileFrame.Y == -1 && (Main.tileMergeDirt[tileType] || (tileType > -1 && TileID.Sets.ChecksForMerge[tileType])))
            {
                if (!flag)
                {
                    flag = true;
                    WorldGen.TileMergeAttemptWeird(tileType, -1, Main.tileSolid, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
                }
                if (up > -1 && up != tileType)
                {
                    up = -1;
                }
                if (down > -1 && down != tileType)
                {
                    down = -1;
                }
                if (left > -1 && left != tileType)
                {
                    left = -1;
                }
                if (right > -1 && right != tileType)
                {
                    right = -1;
                }
                if (up != -1 && down != -1 && left != -1 && right != -1)
                {
                    if (up == -2 && down == tileType && left == tileType && right == tileType)
                    {
                        tileFrame.X = 144;
                        tileFrame.Y = 108;
                    }
                    else if (up == tileType && down == -2 && left == tileType && right == tileType)
                    {
                        tileFrame.X = 144;
                        tileFrame.Y = 90;
                    }
                    else if (up == tileType && down == tileType && left == -2 && right == tileType)
                    {
                        tileFrame.X = 162;
                        tileFrame.Y = 126;
                    }
                    else if (up == tileType && down == tileType && left == tileType && right == -2)
                    {
                        tileFrame.X = 144;
                        tileFrame.Y = 126;
                    }
                    else if (up == -2 && down == tileType && left == -2 && right == tileType)
                    {
                        tileFrame.X = 36;
                        tileFrame.Y = 90;
                    }
                    else if (up == -2 && down == tileType && left == tileType && right == -2)
                    {
                        tileFrame.X = 54;
                        tileFrame.Y = 90;
                    }
                    else if (up == tileType && down == -2 && left == -2 && right == tileType)
                    {
                        tileFrame.X = 36;
                        tileFrame.Y = 108;
                    }
                    else if (up == tileType && down == -2 && left == tileType && right == -2)
                    {
                        tileFrame.X = 54;
                        tileFrame.Y = 108;
                    }
                    else if (up == tileType && down == tileType && left == -2 && right == -2)
                    {
                        tileFrame.X = 180;
                        tileFrame.Y = 126;
                    }
                    else if (up == -2 && down == -2 && left == tileType && right == tileType)
                    {
                        tileFrame.X = 144;
                        tileFrame.Y = 180;
                    }
                    else if (up == -2 && down == tileType && left == -2 && right == -2)
                    {
                        tileFrame.X = 198;
                        tileFrame.Y = 90;
                    }
                    else if (up == tileType && down == -2 && left == -2 && right == -2)
                    {
                        tileFrame.X = 198;
                        tileFrame.Y = 144;
                    }
                    else if (up == -2 && down == -2 && left == tileType && right == -2)
                    {
                        tileFrame.X = 216;
                        tileFrame.Y = 144;
                    }
                    else if (up == -2 && down == -2 && left == -2 && right == tileType)
                    {
                        tileFrame.X = 216;
                        tileFrame.Y = 90;
                    }
                    else if (up == -2 && down == -2 && left == -2 && right == -2)
                    {
                        tileFrame.X = 108;
                        tileFrame.Y = 198;
                    }
                    else if (up == tileType && down == tileType && left == tileType && right == tileType)
                    {
                        if (upLeft == -2)
                        {
                            tileFrame.X = 18;
                            tileFrame.Y = 108;
                        }
                        if (upRight == -2)
                        {
                            tileFrame.X = 0;
                            tileFrame.Y = 108;
                        }
                        if (downLeft == -2)
                        {
                            tileFrame.X = 18;
                            tileFrame.Y = 90;
                        }
                        if (downRight == -2)
                        {
                            tileFrame.X = 0;
                            tileFrame.Y = 90;
                        }
                    }
                }
                else
                {
                    if (tileType != 2 && tileType != 23 && tileType != 60 && tileType != 70 && tileType != 109 && tileType != 199 && tileType != 477 && tileType != 492)
                    {
                        if (up == -1 && down == -2 && left == tileType && right == tileType)
                        {
                            tileFrame.X = 234;
                            tileFrame.Y = 0;
                        }
                        else if (up == -2 && down == -1 && left == tileType && right == tileType)
                        {
                            tileFrame.X = 234;
                            tileFrame.Y = 18;
                        }
                        else if (up == tileType && down == tileType && left == -1 && right == -2)
                        {
                            tileFrame.X = 234;
                            tileFrame.Y = 36;
                        }
                        else if (up == tileType && down == tileType && left == -2 && right == -1)
                        {
                            tileFrame.X = 234;
                            tileFrame.Y = 54;
                        }
                    }
                    if (up != -1 && down != -1 && left == -1 && right == tileType)
                    {
                        if (up == -2 && down == tileType)
                        {
                            tileFrame.X = 72;
                            tileFrame.Y = 144;
                        }
                        else if (down == -2 && up == tileType)
                        {
                            tileFrame.X = 72;
                            tileFrame.Y = 90;
                        }
                    }
                    else if (up != -1 && down != -1 && left == tileType && right == -1)
                    {
                        if (up == -2 && down == tileType)
                        {
                            tileFrame.X = 90;
                            tileFrame.Y = 144;
                        }
                        else if (down == -2 && up == tileType)
                        {
                            tileFrame.X = 90;
                            tileFrame.Y = 90;
                        }
                    }
                    else if (up == -1 && down == tileType && left != -1 && right != -1)
                    {
                        if (left == -2 && right == tileType)
                        {
                            tileFrame.X = 0;
                            tileFrame.Y = 198;
                        }
                        else if (right == -2 && left == tileType)
                        {
                            tileFrame.X = 54;
                            tileFrame.Y = 198;
                        }
                    }
                    else if (up == tileType && down == -1 && left != -1 && right != -1)
                    {
                        if (left == -2 && right == tileType)
                        {
                            tileFrame.X = 0;
                            tileFrame.Y = 216;
                        }
                        else if (right == -2 && left == tileType)
                        {
                            tileFrame.X = 54;
                            tileFrame.Y = 216;
                        }
                    }
                    else if (up != -1 && down != -1 && left == -1 && right == -1)
                    {
                        if (up == -2 && down == -2)
                        {
                            tileFrame.X = 108;
                            tileFrame.Y = 216;
                        }
                        else if (up == -2)
                        {
                            tileFrame.X = 126;
                            tileFrame.Y = 144;
                        }
                        else if (down == -2)
                        {
                            tileFrame.X = 126;
                            tileFrame.Y = 90;
                        }
                    }
                    else if (up == -1 && down == -1 && left != -1 && right != -1)
                    {
                        if (left == -2 && right == -2)
                        {
                            tileFrame.X = 162;
                            tileFrame.Y = 198;
                        }
                        else if (left == -2)
                        {
                            tileFrame.X = 0;
                            tileFrame.Y = 252;
                        }
                        else if (right == -2)
                        {
                            tileFrame.X = 54;
                            tileFrame.Y = 252;
                        }
                    }
                    else if (up == -2 && down == -1 && left == -1 && right == -1)
                    {
                        tileFrame.X = 108;
                        tileFrame.Y = 144;
                    }
                    else if (up == -1 && down == -2 && left == -1 && right == -1)
                    {
                        tileFrame.X = 108;
                        tileFrame.Y = 90;
                    }
                    else if (up == -1 && down == -1 && left == -2 && right == -1)
                    {
                        tileFrame.X = 0;
                        tileFrame.Y = 234;
                    }
                    else if (up == -1 && down == -1 && left == -1 && right == -2)
                    {
                        tileFrame.X = 54;
                        tileFrame.Y = 234;
                    }
                }
            }

            int num33 = tile.blockType();
            if (TileID.Sets.HasSlopeFrames[tileType])
            {
                if (num33 == 0)
                {
                    bool flag2 = tileType == up && tile8.topSlope();
                    bool flag3 = tileType == left && tile2.leftSlope();
                    bool flag4 = tileType == right && tile3.rightSlope();
                    bool flag5 = tileType == down && tile9.bottomSlope();
                    int num34 = 0;
                    int num35 = 0;
                    if (flag2.ToInt() + flag3.ToInt() + flag4.ToInt() + flag5.ToInt() > 2)
                    {
                        int num36 = (tile8.slope() == 1).ToInt() + (tile3.slope() == 1).ToInt() + (tile9.slope() == 4).ToInt() + (tile2.slope() == 4).ToInt();
                        int num37 = (tile8.slope() == 2).ToInt() + (tile3.slope() == 3).ToInt() + (tile9.slope() == 3).ToInt() + (tile2.slope() == 2).ToInt();
                        if (num36 == num37)
                        {
                            num34 = 2;
                            num35 = 4;
                        }
                        else if (num36 > num37)
                        {
                            bool num38 = tileType == upLeft && tile6.slope() == 0;
                            bool flag6 = tileType == downRight && tile5.slope() == 0;
                            if (num38 && flag6)
                            {
                                num35 = 4;
                            }
                            else if (flag6)
                            {
                                num34 = 6;
                            }
                            else
                            {
                                num34 = 7;
                                num35 = 1;
                            }
                        }
                        else
                        {
                            bool num39 = tileType == upRight && tile7.slope() == 0;
                            bool flag7 = tileType == downLeft && tile4.slope() == 0;
                            if (num39 && flag7)
                            {
                                num35 = 4;
                                num34 = 1;
                            }
                            else if (flag7)
                            {
                                num34 = 7;
                            }
                            else
                            {
                                num34 = 6;
                                num35 = 1;
                            }
                        }
                        tileFrame.X = (18 + num34) * 18;
                        tileFrame.Y = num35 * 18;
                    }
                    else
                    {
                        if (flag2 && flag3 && tileType == down && tileType == right)
                        {
                            num35 = 2;
                        }
                        else if (flag2 && flag4 && tileType == down && tileType == left)
                        {
                            num34 = 1;
                            num35 = 2;
                        }
                        else if (flag4 && flag5 && tileType == up && tileType == left)
                        {
                            num34 = 1;
                            num35 = 3;
                        }
                        else if (flag5 && flag3 && tileType == up && tileType == right)
                        {
                            num35 = 3;
                        }
                        if (num34 != 0 || num35 != 0)
                        {
                            tileFrame.X = (18 + num34) * 18;
                            tileFrame.Y = num35 * 18;
                        }
                    }
                }
                if (num33 >= 2 && (tileFrame.X < 0 || tileFrame.Y < 0))
                {
                    int num40 = -1;
                    int num41 = -1;
                    int num42 = -1;
                    int num43 = 0;
                    int num44 = 0;
                    switch (num33)
                    {
                        case 2:
                            num40 = left;
                            num41 = down;
                            num42 = downLeft;
                            num43++;
                            break;
                        case 3:
                            num40 = right;
                            num41 = down;
                            num42 = downRight;
                            break;
                        case 4:
                            num40 = left;
                            num41 = up;
                            num42 = upLeft;
                            num43++;
                            num44++;
                            break;
                        case 5:
                            num40 = right;
                            num41 = up;
                            num42 = upRight;
                            num44++;
                            break;
                    }
                    if (tileType != num40 || tileType != num41 || tileType != num42)
                    {
                        if (tileType == num40 && tileType == num41)
                        {
                            num43 += 2;
                        }
                        else if (tileType == num40)
                        {
                            num43 += 4;
                        }
                        else if (tileType == num41)
                        {
                            num43 += 4;
                            num44 += 2;
                        }
                        else
                        {
                            num43 += 2;
                            num44 += 2;
                        }
                    }
                    tileFrame.X = (18 + num43) * 18;
                    tileFrame.Y = num44 * 18;
                }
            }

            if (tileFrame.X < 0 || tileFrame.Y < 0)
            {
                if (!flag)
                {
                    WorldGen.TileMergeAttemptWeird(tileType, -1, Main.tileSolid, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
                }
                if (tileType == 2 || tileType == 23 || tileType == 60 || tileType == 70 || tileType == 109 || tileType == 199 || tileType == 477 || tileType == 492 || Main.tileMoss[tileType] || TileID.Sets.tileMossBrick[tileType])
                {
                    WorldGen.TileMergeAttempt(tileType, -2, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
                }
                if (up == tileType && down == tileType && left == tileType && right == tileType)
                {
                    if (upLeft != tileType && upRight != tileType)
                    {
                        tileFrame.X = 108;
                        tileFrame.Y = 18;
                    }
                    else if (downLeft != tileType && downRight != tileType)
                    {
                        tileFrame.X = 108;
                        tileFrame.Y = 36;
                    }
                    else if (upLeft != tileType && downLeft != tileType)
                    {
                        tileFrame.X = 180;
                        tileFrame.Y = 0;
                    }
                    else if (upRight != tileType && downRight != tileType)
                    {
                        tileFrame.X = 198;
                        tileFrame.Y = 0;
                    }
                    else
                    {
                        tileFrame.X = 18;
                        tileFrame.Y = 18;
                    }
                }
                else if (up != tileType && down == tileType && left == tileType && right == tileType)
                {
                    tileFrame.X = 18;
                    tileFrame.Y = 0;
                }
                else if (up == tileType && down != tileType && left == tileType && right == tileType)
                {
                    tileFrame.X = 18;
                    tileFrame.Y = 36;
                }
                else if (up == tileType && down == tileType && left != tileType && right == tileType)
                {
                    tileFrame.X = 0;
                    tileFrame.Y = 0;
                }
                else if (up == tileType && down == tileType && left == tileType && right != tileType)
                {
                    tileFrame.X = 72;
                    tileFrame.Y = 0;
                }
                else if (up != tileType && down == tileType && left != tileType && right == tileType)
                {
                    tileFrame.X = 0;
                    tileFrame.Y = 54;
                }
                else if (up != tileType && down == tileType && left == tileType && right != tileType)
                {
                    tileFrame.X = 18;
                    tileFrame.Y = 54;
                }
                else if (up == tileType && down != tileType && left != tileType && right == tileType)
                {
                    tileFrame.X = 0;
                    tileFrame.Y = 72;
                }
                else if (up == tileType && down != tileType && left == tileType && right != tileType)
                {
                    tileFrame.X = 18;
                    tileFrame.Y = 72;
                }
                else if (up == tileType && down == tileType && left != tileType && right != tileType)
                {
                    tileFrame.X = 90;
                    tileFrame.Y = 0;
                }
                else if (up != tileType && down != tileType && left == tileType && right == tileType)
                {
                    tileFrame.X = 108;
                    tileFrame.Y = 72;
                }
                else if (up != tileType && down == tileType && left != tileType && right != tileType)
                {
                    tileFrame.X = 108;
                    tileFrame.Y = 0;
                }
                else if (up == tileType && down != tileType && left != tileType && right != tileType)
                {
                    tileFrame.X = 108;
                    tileFrame.Y = 54;
                }
                else if (up != tileType && down != tileType && left != tileType && right == tileType)
                {
                    tileFrame.X = 162;
                    tileFrame.Y = 0;
                }
                else if (up != tileType && down != tileType && left == tileType && right != tileType)
                {
                    tileFrame.X = 216;
                    tileFrame.Y = 0;
                }
                else if (up != tileType && down != tileType && left != tileType && right != tileType)
                {
                    tileFrame.X = 162;
                    tileFrame.Y = 54;
                }
            }
            if (tileFrame.X <= -1 || tileFrame.Y <= -1)
            {
                tileFrame.X = 18;
                tileFrame.Y = 18;
            }

            return new Point((short)tileFrame.X, (short)tileFrame.Y);
        }
    }
}