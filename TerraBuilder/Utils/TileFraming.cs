namespace TerraBuilder.Utils
{
    using System.Drawing;
    using Terraria;
    using Terraria.ID;

    public static class TileFraming
    {
        public static void FrameAroundTile(Tile[,] tiles, int centerX, int centerY)
        {
            Tile tile = tiles[centerX, centerY];
            for (int x = centerX - 1; x <= centerX + 1; x++)
            {
                for (int y = centerY - 1; y <= centerY + 1; y++)
                {
                    if (x == 0 && y == 0)
                    {
                        FrameTile(tiles, x, y);
                        continue;
                    }

                    if (ShouldFrame(tile, tiles, x, y))
                    {
                        FrameTile(tiles, x, y);
                    }
                }
            }
        }

        public static void FrameTile(Tile[,] tiles, int x, int y)
        {
            Tile tile = tiles[x, y];
            int tileId = tile.type;

            if (!tile.active())
            {
                tile.halfBrick(false);
                tile.color(0);
                tile.slope(0);
                return;
            }

            if (Main.tileStone[tileId])
            {
                tileId = TileID.Stone;
            }

            int frameX = tile.frameX;
            int frameY = tile.frameY;
            Rectangle frameRect = new Rectangle(-1, -1, 0, 0);
            if (Main.tileFrameImportant[tile.type])
            {
                if (tileId == TileID.SandDrip)
                {
                    return;
                }

                if (TileID.Sets.Platforms[tileId])
                {
                    Tile leftTile = tiles[x - 1, y];
                    Tile rightTile = tiles[x + 1, y];
                    Tile leftBottomTile = tiles[x - 1, y + 1];
                    Tile rightBottomTile = tiles[x + 1, y + 1];
                    Tile leftTopTile = tiles[x - 1, y - 1];
                    Tile rightTopTile = tiles[x + 1, y - 1];

                    int rightId = -1;
                    int leftId = -1;
                    if (leftTile != null && leftTile.active())
                    {
                        leftId = Main.tileStone[leftTile.type] ? 1 : ((!TileID.Sets.Platforms[leftTile.type]) ? leftTile.type : tileId);
                    }

                    if (rightTile != null && rightTile.active())
                    {
                        rightId = Main.tileStone[rightTile.type] ? 1 : ((!TileID.Sets.Platforms[rightTile.type]) ? rightTile.type : tileId);
                    }

                    if (rightId >= 0 && !Main.tileSolid[rightId])
                    {
                        rightId = -1;
                    }

                    if (leftId >= 0 && !Main.tileSolid[leftId])
                    {
                        leftId = -1;
                    }

                    if (leftId == tileId && leftTile.halfBrick() != tile.halfBrick())
                    {
                        leftId = -1;
                    }

                    if (rightId == tileId && rightTile.halfBrick() != tile.halfBrick())
                    {
                        rightId = -1;
                    }

                    if (leftId != -1 && leftId != tileId && tile.halfBrick())
                    {
                        leftId = -1;
                    }

                    if (rightId != -1 && rightId != tileId && tile.halfBrick())
                    {
                        rightId = -1;
                    }

                    if (leftId == -1 && leftTopTile.active() && leftTopTile.type == tileId && leftTopTile.slope() == 1)
                    {
                        leftId = tileId;
                    }

                    if (rightId == -1 && rightTopTile.active() && rightTopTile.type == tileId && rightTopTile.slope() == 2)
                    {
                        rightId = tileId;
                    }

                    if (leftId == tileId && leftTile.slope() == 2 && rightId != tileId)
                    {
                        rightId = -1;
                    }

                    if (rightId == tileId && rightTile.slope() == 1 && leftId != tileId)
                    {
                        leftId = -1;
                    }

                    if (tile.slope() == 1)
                    {
                        if (TileID.Sets.Platforms[rightTile.type] && rightTile.slope() == 0 && !rightTile.halfBrick())
                        {
                            frameRect.X = 468;
                        }
                        else if (!rightBottomTile.active() && (!TileID.Sets.Platforms[rightBottomTile.type] || rightBottomTile.slope() == 2))
                        {
                            if (!leftTile.active() && (!TileID.Sets.Platforms[leftTopTile.type] || leftTopTile.slope() != 1))
                            {
                                frameRect.X = 432;
                            }
                            else
                            {
                                frameRect.X = 360;
                            }
                        }
                        else if (!leftTile.active() && (!TileID.Sets.Platforms[leftTopTile.type] || leftTopTile.slope() != 1))
                        {
                            frameRect.X = 396;
                        }
                        else
                        {
                            frameRect.X = 180;
                        }
                    }
                    else if (tile.slope() == 2)
                    {
                        if (TileID.Sets.Platforms[leftTile.type] && leftTile.slope() == 0 && !leftTile.halfBrick())
                        {
                            frameRect.X = 450;
                        }
                        else if (!leftBottomTile.active() && (!TileID.Sets.Platforms[leftBottomTile.type] || leftBottomTile.slope() == 1))
                        {
                            if (!rightTile.active() && (!TileID.Sets.Platforms[rightTopTile.type] || rightTopTile.slope() != 2))
                            {
                                frameRect.X = 414;
                            }
                            else
                            {
                                frameRect.X = 342;
                            }
                        }
                        else if (!rightTile.active() && (!TileID.Sets.Platforms[rightTopTile.type] || rightTopTile.slope() != 2))
                        {
                            frameRect.X = 378;
                        }
                        else
                        {
                            frameRect.X = 144;
                        }
                    }
                    else if (leftId == tileId && rightId == tileId)
                    {
                        if (leftTile.slope() == 2 && rightTile.slope() == 1)
                        {
                            frameRect.X = 252;
                        }
                        else if (leftTile.slope() == 2)
                        {
                            frameRect.X = 216;
                        }
                        else if (rightTile.slope() == 1)
                        {
                            frameRect.X = 234;
                        }
                        else
                        {
                            frameRect.X = 0;
                        }
                    }
                    else if (leftId == tileId && rightId == -1)
                    {
                        if (leftTile.slope() == 2)
                        {
                            frameRect.X = 270;
                        }
                        else
                        {
                            frameRect.X = 18;
                        }
                    }
                    else if (leftId == -1 && rightId == tileId)
                    {
                        if (rightTile.slope() == 1)
                        {
                            frameRect.X = 288;
                        }
                        else
                        {
                            frameRect.X = 36;
                        }
                    }
                    else if (leftId != tileId && rightId == tileId)
                    {
                        frameRect.X = 54;
                    }
                    else if (leftId == tileId && rightId != tileId)
                    {
                        frameRect.X = 72;
                    }
                    else if (leftId != tileId && leftId != -1 && rightId == -1)
                    {
                        frameRect.X = 108;
                    }
                    else if (leftId == -1 && rightId != tileId && rightId != -1)
                    {
                        frameRect.X = 126;
                    }
                    else
                    {
                        frameRect.X = 90;
                    }

                    tile.frameX = (short)frameRect.X;

                    return;
                }

                switch (tileId)
                {
                    case TileID.Plants:
                    case TileID.CorruptPlants:
                    case TileID.JunglePlants:
                    case TileID.MushroomPlants:
                    case TileID.Plants2:
                    case TileID.JunglePlants2:
                    case TileID.HallowedPlants:
                    case TileID.HallowedPlants2:
                    case TileID.CrimsonPlants:
                        break;
                    case TileID.Torches:
                        FrameTorch(tiles, x, y);
                        break;
                    case TileID.Heart:
                    case TileID.ShadowOrbs:
                        break;
                    case TileID.Switches:
                        break;
                    case TileID.Crystals:
                    case TileID.HolidayLights:
                        break;
                    case TileID.Stalactite:
                        break;
                    case TileID.ExposedGems:
                        break;
                    case TileID.LongMoss:
                        break;
                    case TileID.SmallPiles:
                        break;
                    case TileID.DyePlants:
                        break;
                    case TileID.Teleporter:
                        break;
                    case TileID.ProjectilePressurePad:
                        break;
                    case TileID.LilyPad:
                        break;
                    case TileID.Cattail:
                        break;
                    case TileID.Seaweed:
                        break;
                    case TileID.Bamboo:
                        break;
                    case TileID.SeaOats:
                        break;
                    case TileID.RockGolemHead:
                        break;
                    default:
                        break;
                }

                return;
            }
        }

        private static void FrameTorch(Tile[,] tiles, int x, int y)
        {
            Tile tile = tiles[x, y];
            Tile topTile = tiles[x, y - 1];
            Tile bottomTile = tiles[x, y + 1];
            Tile leftTile = tiles[x - 1, y];
            Tile rightTile = tiles[x + 1, y];
            Tile leftBottomTile = tiles[x - 1, y + 1];
            Tile rightBottomTile = tiles[x + 1, y + 1];
            Tile leftTopTile = tiles[x - 1, y - 1];
            Tile rightTopTile = tiles[x + 1, y - 1];
            short num = 0;
            if (tile.frameX >= 66)
            {
                num = 66;
            }

            int bottomType = -1;
            int leftType = -1;
            int rightType = -1;
            int leftBottomType = -1;
            int rightBottomType = -1;
            int leftTopType = -1;
            int rightTopType = -1;

            if (bottomTile?.active() == true && !bottomTile.halfBrick() && !bottomTile.topSlope() && !tile.inActive())
            {
                bottomType = bottomTile.type;
            }

            if (leftTile?.active() == true && (leftTile.slope() == 0 || (int)leftTile.slope() % 2 != 1))
            {
                leftType = leftTile.type;
            }

            if (rightTile?.active() == true && (rightTile.slope() == 0 || (int)rightTile.slope() % 2 != 0))
            {
                rightType = rightTile.type;
            }

            if (leftBottomTile?.active() == true)
            {
                leftBottomType = leftBottomTile.type;
            }

            if (rightBottomTile?.active() == true)
            {
                rightBottomType = rightBottomTile.type;
            }

            if (leftTopTile?.active() == true)
            {
                leftTopType = leftTopTile.type;
            }

            if (rightTopTile?.active() == true)
            {
                rightTopType = rightTopTile.type;
            }

            if (bottomType >= 0 && Main.tileSolid[bottomType] && (!Main.tileNoAttach[bottomType] || TileID.Sets.Platforms[bottomType]))
            {
                tile.frameX = num;
            }
            else if ((leftType >= 0 && Main.tileSolid[leftType] && !Main.tileNoAttach[leftType]) || (leftType >= 0 && TileID.Sets.IsBeam[leftType]) || (IsTreeType(leftType) && IsTreeType(leftTopType) && IsTreeType(leftBottomType)))
            {
                tile.frameX = (short)(22 + num);
            }
            else if ((rightType >= 0 && Main.tileSolid[rightType] && !Main.tileNoAttach[rightType]) || (rightType >= 0 && TileID.Sets.IsBeam[rightType]) || (IsTreeType(rightType) && IsTreeType(rightTopType) && IsTreeType(rightBottomType)))
            {
                tile.frameX = (short)(44 + num);
            }
            else if (tile.wall > 0)
            {
                tile.frameX = num;
            }
            else
            {
                tile.frameX = 0;
            }
        }

        private static bool ShouldFrame(Tile original, Tile[,] tiles, int x, int y)
        {
            if (x < 0 || x >= tiles.GetLength(0) || y < 0 || y >= tiles.GetLength(1))
            {
                return false;
            }

            if (tiles[x, y] == null)
            {
                return false;
            }

            if (original.active() && tiles[x, y].active())
            {
                if (original.type == tiles[x, y].type)
                {
                    return true;
                }
                else if (TileID.Sets.Platforms[original.type] && TileID.Sets.Platforms[tiles[x, y].type])
                {
                    return true;
                }
                else if (Main.tileStone[original.type] && Main.tileStone[tiles[x, y].type])
                {
                    return true;
                }

                return false;
            }
            else
            {
                return false;
            }
        }

        private static bool IsTreeType(int tree)
        {
            if (tree >= 0)
            {
                return TileID.Sets.IsATreeTrunk[tree];
            }

            return false;
        }
    }
}
