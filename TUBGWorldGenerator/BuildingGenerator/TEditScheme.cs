namespace TUBGWorldGenerator.BuildingGenerator
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Terraria;
    using Terraria.DataStructures;
    using Terraria.GameContent.Tile_Entities;
    using Terraria.ID;

    public static class TEditScheme
    {
        private enum TileEntityType : byte
        {
            TrainingDummy = 0,
            ItemFrame = 1,
            LogicSensor = 2,
            DisplayDoll = 3,
            WeaponRack = 4,
            HatRack = 5,
            FoodPlatter = 6,
            TeleportationPylon = 7,
        }

        public static void WriteTiles(Tile[,] tiles, string path, string name, List<Chest> chests = null, List<Sign> signs = null, List<TileEntity> tileEntities = null)
        {
            int sizeX = tiles.GetLength(0);
            int sizeY = tiles.GetLength(1);
            using (var stream = new FileStream(path, FileMode.Open))
            {
                using (var b = new BinaryWriter(stream))
                {
                    const int worldVersion = 238;

                    b.Write(name);
                    b.Write(worldVersion);
                    b.Write(sizeX);
                    b.Write(sizeY);

                    for (int x = 0; x < sizeX; x++)
                    {
                        for (int y = 0; y < sizeY; y++)
                        {
                            Tile tile = tiles[x, y];

                            int dataIndex;
                            int headerIndex;

                            byte[] tileData = SerializeTileData(tile, out dataIndex, out headerIndex);

                            // rle compression
                            byte header1 = tileData[headerIndex];

                            short rle = 0;
                            int nextY = y + 1;
                            int remainingY = sizeY - y - 1;
                            while (remainingY > 0 && tile.Equals(tiles[x, nextY]))
                            {
                                rle = (short)(rle + 1);
                                remainingY--;
                                nextY++;
                            }

                            y += rle;

                            if (rle > 0)
                            {
                                tileData[dataIndex++] = (byte)(rle & 255);

                                if (rle <= 255)
                                {
                                    // set bit[6] of header1 for byte size rle
                                    header1 = (byte)(header1 | 64);
                                }
                                else
                                {
                                    // set bit[7] of header1 for int16 size rle
                                    header1 = (byte)(header1 | 128);

                                    // grab the upper half of the int16 and stick it in tiledata
                                    tileData[dataIndex++] = (byte)((rle & 65280) >> 8);
                                }
                            }

                            tileData[headerIndex] = header1;

                            // end rle compression
                            b.Write(tileData, headerIndex, dataIndex - headerIndex);
                        }
                    }

                    if (chests == null)
                    {
                        b.Write((short)0);
                        b.Write((short)Chest.maxItems);
                    }
                    else
                    {
                        b.Write((short)chests.Count);
                        b.Write((short)Chest.maxItems);

                        foreach (Chest chest in chests)
                        {
                            b.Write(chest.x);
                            b.Write(chest.y);
                            b.Write(chest.name ?? string.Empty);

                            for (int slot = 0; slot < Chest.maxItems; slot++)
                            {
                                Item item = chest.item[slot];
                                if (item != null)
                                {
                                    b.Write((short)item.stack);
                                    if (item.stack > 0)
                                    {
                                        b.Write(item.netID);
                                        b.Write(item.prefix);
                                    }
                                }
                                else
                                {
                                    b.Write((short)0);
                                }
                            }
                        }
                    }

                    if (signs == null)
                    {
                        b.Write((short)0);
                    }
                    else
                    {
                        b.Write((short)signs.Count);

                        foreach (Sign sign in signs)
                        {
                            if (sign.text != null)
                            {
                                b.Write(sign.text);
                                b.Write(sign.x);
                                b.Write(sign.y);
                            }
                        }
                    }

                    if (tileEntities == null)
                    {
                        b.Write(0);
                    }
                    else
                    {
                        b.Write(tileEntities.Count);

                        foreach (TileEntity entity in tileEntities)
                        {
                            b.Write(entity.type);
                            b.Write(entity.ID);
                            b.Write(entity.Position.X);
                            b.Write(entity.Position.Y);
                            switch ((TileEntityType)entity.type)
                            {
                                case TileEntityType.TrainingDummy: //it is a dummy
                                    if (entity is TETrainingDummy dummy)
                                    {
                                        b.Write(dummy.npc);
                                    }

                                    break;
                                case TileEntityType.ItemFrame: //it is a item frame
                                    if (entity is TEItemFrame itemFrame)
                                    {
                                        b.Write((short)itemFrame.item.netID);
                                        b.Write((byte)itemFrame.item.prefix);
                                        b.Write((short)itemFrame.item.stack);
                                    }

                                    break;
                                case TileEntityType.LogicSensor: //it is a logic sensor
                                    if (entity is TELogicSensor sensor)
                                    {
                                        b.Write((byte)sensor.logicCheck);
                                        b.Write(sensor.On);
                                    }

                                    break;
                                case TileEntityType.DisplayDoll: // display doll
                                    if (entity is TEDisplayDoll displayDoll)
                                    {
                                        byte numSlots = 8;
                                        BitsByte itemSlotData = default;
                                        BitsByte dyeSlotData = default;
                                        for (int i = 0; i < numSlots; i++)
                                        {
                                            itemSlotData[i] = displayDoll._items[i] != null && displayDoll._items[i].type != ItemID.None;
                                        }

                                        for (int i = 0; i < numSlots; i++)
                                        {
                                            dyeSlotData[i] = displayDoll._dyes[i] != null && displayDoll._dyes[i].type != ItemID.None;
                                        }

                                        b.Write((byte)itemSlotData);
                                        b.Write((byte)dyeSlotData);

                                        for (int i = 0; i < numSlots; i++)
                                        {
                                            if (itemSlotData[i])
                                            {
                                                b.Write(displayDoll._items[i].netID);
                                                b.Write(displayDoll._items[i].prefix);
                                                b.Write(displayDoll._items[i].stack);
                                            }
                                        }

                                        for (int i = 0; i < numSlots; i++)
                                        {
                                            if (dyeSlotData[i])
                                            {
                                                b.Write(displayDoll._dyes[i].netID);
                                                b.Write(displayDoll._dyes[i].prefix);
                                                b.Write(displayDoll._dyes[i].stack);
                                            }
                                        }
                                    }

                                    break;
                                case TileEntityType.WeaponRack: // weapons rack
                                    if (entity is TEWeaponsRack rack)
                                    {
                                        b.Write((short)rack.item.netID);
                                        b.Write((byte)rack.item.prefix);
                                        b.Write((short)rack.item.stack);
                                    }

                                    break;
                                case TileEntityType.HatRack: // hat rack 
                                    if (entity is TEHatRack hatRack)
                                    {
                                        byte numSlots = 2;
                                        BitsByte itemSlotData = default;
                                        BitsByte dyeSlotData = default;
                                        for (int i = 0; i < numSlots; i++)
                                        {
                                            itemSlotData[i] = hatRack._items[i] != null && hatRack._items[i].type != ItemID.None;
                                        }

                                        for (int i = 0; i < numSlots; i++)
                                        {
                                            dyeSlotData[i] = hatRack._dyes[i] != null && hatRack._dyes[i].type != ItemID.None;
                                        }

                                        b.Write((byte)itemSlotData);
                                        b.Write((byte)dyeSlotData);

                                        for (int i = 0; i < numSlots; i++)
                                        {
                                            if (itemSlotData[i])
                                            {
                                                b.Write(hatRack._items[i].netID);
                                                b.Write(hatRack._items[i].prefix);
                                                b.Write(hatRack._items[i].stack);
                                            }
                                        }

                                        for (int i = 0; i < numSlots; i++)
                                        {
                                            if (dyeSlotData[i])
                                            {
                                                b.Write(hatRack._dyes[i].netID);
                                                b.Write(hatRack._dyes[i].prefix);
                                                b.Write(hatRack._dyes[i].stack);
                                            }
                                        }
                                    }

                                    break;
                                case TileEntityType.FoodPlatter: // food platter
                                    if (entity is TEFoodPlatter platter)
                                    {
                                        b.Write((short)platter.item.netID);
                                        b.Write((byte)platter.item.prefix);
                                        b.Write((short)platter.item.stack);
                                    }

                                    break;
                                case TileEntityType.TeleportationPylon: // teleportation pylon
                                    break;
                            }
                        }
                    }

                    b.Write(name);
                    b.Write(worldVersion);
                    b.Write(sizeX);
                    b.Write(sizeY);
                }
            }
        }

        public static (Tile[,] tiles, string name, List<Chest> chests, List<Sign> signs, List<TileEntity> tileEntities) Read(string path)
        {
            using (var stream = new FileStream(path, FileMode.Open))
            {
                using (var b = new BinaryReader(stream))
                {
                    string name = b.ReadString();
                    int version = b.ReadInt32();
                    uint tVersion = (uint)version;

                    // 最新バージョンしかサポートしない
                    if (version >= 222)
                    {
                        int sizeX = b.ReadInt32();
                        int sizeY = b.ReadInt32();
                        Tile[,] tiles = new Tile[sizeX, sizeY];

                        for (int x = 0; x < sizeX; x++)
                        {
                            for (int y = 0; y < sizeY; y++)
                            {
                                Tile tile = DeserializeTileData(b, version, out int rle);

                                tiles[x, y] = tile;
                                while (rle > 0)
                                {
                                    y++;

                                    if (y >= sizeY)
                                    {
                                        break;
                                        throw new IOException($"Invalid Tile Data: RLE Compression outside of bounds [{x},{y}]");
                                    }

                                    tiles[x, y] = (Tile)tile.Clone();
                                    rle--;
                                }
                            }
                        }

                        // チェスト読み込み
                        List<Chest> chests = new List<Chest>();
                        int totalChests = b.ReadInt16();
                        int maxItems = b.ReadInt16();

                        // overflow item check?
                        int itemsPerChest;
                        int overflowItems;
                        if (maxItems > Chest.maxItems)
                        {
                            itemsPerChest = Chest.maxItems;
                            overflowItems = maxItems - Chest.maxItems;
                        }
                        else
                        {
                            itemsPerChest = maxItems;
                            overflowItems = 0;
                        }

                        // read chests
                        for (int i = 0; i < totalChests; i++)
                        {
                            var chest = new Chest
                            {
                                x = b.ReadInt32(),
                                y = b.ReadInt32(),
                                name = b.ReadString(),
                            };

                            // read items in chest
                            for (int slot = 0; slot < itemsPerChest; slot++)
                            {
                                var stackSize = b.ReadInt16();
                                chest.item[slot].stack = stackSize;

                                if (stackSize > 0)
                                {
                                    int id = b.ReadInt32();
                                    byte prefix = b.ReadByte();

                                    chest.item[slot].netID = id;
                                    chest.item[slot].stack = stackSize;
                                    chest.item[slot].prefix = prefix;
                                }
                            }

                            // dump overflow items
                            for (int overflow = 0; overflow < overflowItems; overflow++)
                            {
                                var stackSize = b.ReadInt16();
                                if (stackSize > 0)
                                {
                                    b.ReadInt32();
                                    b.ReadByte();
                                }
                            }

                            chests.Add(chest);
                        }

                        // 看板読み込み
                        List<Sign> signs = new List<Sign>();
                        short totalSigns = b.ReadInt16();

                        for (int i = 0; i < totalSigns; i++)
                        {
                            string text = b.ReadString();
                            int x = b.ReadInt32();
                            int y = b.ReadInt32();
                            signs.Add(new Sign() { x = x, y = y, text = text });
                        }

                        int numEntities = b.ReadInt32();
                        var entities = new List<TileEntity>();
                        for (int i = 0; i < numEntities; i++)
                        {
                            TileEntity entity;
                            int type = b.ReadByte();
                            int id = b.ReadInt32();
                            int posX = b.ReadInt16();
                            int posY = b.ReadInt16();
                            switch ((TileEntityType)type)
                            {
                                case TileEntityType.TrainingDummy: //it is a dummys
                                    int npc = b.ReadInt16();
                                    entity = new TETrainingDummy()
                                    {
                                        ID = id,
                                        type = (byte)type,
                                        Position = new Point16(posX, posY),
                                        npc = npc,
                                    };
                                    break;
                                case TileEntityType.ItemFrame: //it is a item frame
                                    {
                                        int netId = (int)b.ReadInt16();
                                        byte prefix = b.ReadByte();
                                        int stackSize = b.ReadInt16();
                                        Item item = new Item();
                                        item.SetDefaults(netId);
                                        item.prefix = prefix;
                                        item.stack = stackSize;
                                        entity = new TEItemFrame()
                                        {
                                            ID = id,
                                            type = (byte)type,
                                            Position = new Point16(posX, posY),
                                            item = item,
                                        };
                                    }

                                    break;
                                case TileEntityType.LogicSensor: //it is a logic sensor
                                    byte logicCheck = b.ReadByte();
                                    bool on = b.ReadBoolean();
                                    entity = new TELogicSensor()
                                    {
                                        ID = id,
                                        type = (byte)type,
                                        Position = new Point16(posX, posY),
                                        logicCheck = (TELogicSensor.LogicCheckType)logicCheck,
                                        On = on,
                                    };
                                    break;
                                case TileEntityType.DisplayDoll: // display doll
                                    {
                                        byte numSlots = 8;
                                        var itemSlots = (BitsByte)b.ReadByte();
                                        var dyeSlots = (BitsByte)b.ReadByte();
                                        Item[] items = new Item[numSlots];
                                        Item[] dyes = new Item[numSlots];
                                        entity = new TEDisplayDoll()
                                        {
                                            ID = id,
                                            type = (byte)type,
                                            Position = new Point16(posX, posY),
                                            _items = items,
                                            _dyes = dyes,
                                        };

                                        for (int index = 0; index < numSlots; index++)
                                        {
                                            if (itemSlots[index])
                                            {
                                                items[index] = new Item();
                                                items[index].SetDefaults(b.ReadInt16());
                                                items[index].prefix = b.ReadByte();
                                                items[index].stack = b.ReadInt16();
                                            }
                                        }

                                        for (int index = 0; index < numSlots; index++)
                                        {
                                            if (dyeSlots[index])
                                            {
                                                dyes[index] = new Item();
                                                dyes[index].SetDefaults(b.ReadInt16());
                                                dyes[index].prefix = b.ReadByte();
                                                dyes[index].stack = b.ReadInt16();
                                            }
                                        }
                                    }

                                    break;
                                case TileEntityType.WeaponRack: // weapons rack
                                    {
                                        int netId = (int)b.ReadInt16();
                                        byte prefix = b.ReadByte();
                                        int stackSize = b.ReadInt16();
                                        Item item = new Item();
                                        item.SetDefaults(netId);
                                        item.prefix = prefix;
                                        item.stack = stackSize;
                                        entity = new TEWeaponsRack()
                                        {
                                            ID = id,
                                            type = (byte)type,
                                            Position = new Point16(posX, posY),
                                            item = item,
                                        };
                                    }

                                    break;
                                case TileEntityType.HatRack: // hat rack 
                                    {
                                        byte numSlots = 2;
                                        var itemSlots = (BitsByte)b.ReadByte();
                                        var dyeSlots = (BitsByte)b.ReadByte();
                                        Item[] items = new Item[numSlots];
                                        Item[] dyes = new Item[numSlots];
                                        entity = new TEDisplayDoll()
                                        {
                                            ID = id,
                                            type = (byte)type,
                                            Position = new Point16(posX, posY),
                                            _items = items,
                                            _dyes = dyes,
                                        };

                                        for (int index = 0; index < numSlots; index++)
                                        {
                                            if (itemSlots[index])
                                            {
                                                items[index] = new Item();
                                                items[index].SetDefaults(b.ReadInt16());
                                                items[index].prefix = b.ReadByte();
                                                items[index].stack = b.ReadInt16();
                                            }
                                        }

                                        for (int index = 0; index < numSlots; index++)
                                        {
                                            if (dyeSlots[index])
                                            {
                                                dyes[index] = new Item();
                                                dyes[index].SetDefaults(b.ReadInt16());
                                                dyes[index].prefix = b.ReadByte();
                                                dyes[index].stack = b.ReadInt16();
                                            }
                                        }
                                    }

                                    break;
                                case TileEntityType.FoodPlatter: // food platter
                                    {
                                        int netId = (int)b.ReadInt16();
                                        byte prefix = b.ReadByte();
                                        int stackSize = b.ReadInt16();
                                        Item item = new Item();
                                        item.SetDefaults(netId);
                                        item.prefix = prefix;
                                        item.stack = stackSize;
                                        entity = new TEFoodPlatter()
                                        {
                                            ID = id,
                                            type = (byte)type,
                                            Position = new Point16(posX, posY),
                                            item = item,
                                        };
                                    }

                                    break;
                                case TileEntityType.TeleportationPylon: // teleportation pylon
                                    entity = new TETeleportationPylon()
                                    {
                                        ID = id,
                                        type = (byte)type,
                                        Position = new Point16(posX, posY),
                                    };
                                    break;
                                default:
                                    throw new ArgumentException($"Invalid entity type: {type}");
                            }

                            entities.Add(entity);
                        }

                        string verifyName = b.ReadString();
                        int verifyVersion = b.ReadInt32();
                        int verifyX = b.ReadInt32();
                        int verifyY = b.ReadInt32();
                        if (name == verifyName &&
                            version == verifyVersion &&
                            sizeX == verifyX &&
                            sizeY == verifyY)
                        {
                            // valid;
                            return (tiles, name, chests, signs, entities);
                        }

                        b.Close();

                        throw new ArgumentException("File is not valid.");
                    }
                }
            }

            throw new ArgumentException("File could not read");
        }

        public static (Tile[,] tiles, string name, List<Chest> chests, List<Sign> signs, List<TileEntity> tileEntities) Read(byte[] data)
        {
            using (var stream = new MemoryStream(data))
            using (var b = new BinaryReader(stream))
            {
                string name = b.ReadString();
                int version = b.ReadInt32();
                uint tVersion = (uint)version;

                // 最新バージョンしかサポートしない
                if (version >= 222)
                {
                    int sizeX = b.ReadInt32();
                    int sizeY = b.ReadInt32();
                    Tile[,] tiles = new Tile[sizeX, sizeY];

                    for (int x = 0; x < sizeX; x++)
                    {
                        for (int y = 0; y < sizeY; y++)
                        {
                            Tile tile = DeserializeTileData(b, version, out int rle);

                            tiles[x, y] = tile;
                            while (rle > 0)
                            {
                                y++;

                                if (y >= sizeY)
                                {
                                    break;
                                    throw new IOException($"Invalid Tile Data: RLE Compression outside of bounds [{x},{y}]");
                                }

                                tiles[x, y] = (Tile)tile.Clone();
                                rle--;
                            }
                        }
                    }

                    // チェスト読み込み
                    List<Chest> chests = new List<Chest>();
                    int totalChests = b.ReadInt16();
                    int maxItems = b.ReadInt16();

                    // overflow item check?
                    int itemsPerChest;
                    int overflowItems;
                    if (maxItems > Chest.maxItems)
                    {
                        itemsPerChest = Chest.maxItems;
                        overflowItems = maxItems - Chest.maxItems;
                    }
                    else
                    {
                        itemsPerChest = maxItems;
                        overflowItems = 0;
                    }

                    // read chests
                    for (int i = 0; i < totalChests; i++)
                    {
                        var chest = new Chest
                        {
                            x = b.ReadInt32(),
                            y = b.ReadInt32(),
                            name = b.ReadString(),
                        };

                        // read items in chest
                        for (int slot = 0; slot < itemsPerChest; slot++)
                        {
                            var stackSize = b.ReadInt16();
                            chest.item[slot].stack = stackSize;

                            if (stackSize > 0)
                            {
                                int id = b.ReadInt32();
                                byte prefix = b.ReadByte();

                                chest.item[slot].netID = id;
                                chest.item[slot].stack = stackSize;
                                chest.item[slot].prefix = prefix;
                            }
                        }

                        // dump overflow items
                        for (int overflow = 0; overflow < overflowItems; overflow++)
                        {
                            var stackSize = b.ReadInt16();
                            if (stackSize > 0)
                            {
                                b.ReadInt32();
                                b.ReadByte();
                            }
                        }

                        chests.Add(chest);
                    }

                    // 看板読み込み
                    List<Sign> signs = new List<Sign>();
                    short totalSigns = b.ReadInt16();

                    for (int i = 0; i < totalSigns; i++)
                    {
                        string text = b.ReadString();
                        int x = b.ReadInt32();
                        int y = b.ReadInt32();
                        signs.Add(new Sign() { x = x, y = y, text = text });
                    }

                    int numEntities = b.ReadInt32();
                    var entities = new List<TileEntity>();
                    for (int i = 0; i < numEntities; i++)
                    {
                        TileEntity entity;
                        int type = b.ReadByte();
                        int id = b.ReadInt32();
                        int posX = b.ReadInt16();
                        int posY = b.ReadInt16();
                        switch ((TileEntityType)type)
                        {
                            case TileEntityType.TrainingDummy: //it is a dummys
                                int npc = b.ReadInt16();
                                entity = new TETrainingDummy()
                                {
                                    ID = id,
                                    type = (byte)type,
                                    Position = new Point16(posX, posY),
                                    npc = npc,
                                };
                                break;
                            case TileEntityType.ItemFrame: //it is a item frame
                                {
                                    int netId = (int)b.ReadInt16();
                                    byte prefix = b.ReadByte();
                                    int stackSize = b.ReadInt16();
                                    Item item = new Item();
                                    item.SetDefaults(netId);
                                    item.prefix = prefix;
                                    item.stack = stackSize;
                                    entity = new TEItemFrame()
                                    {
                                        ID = id,
                                        type = (byte)type,
                                        Position = new Point16(posX, posY),
                                        item = item,
                                    };
                                }

                                break;
                            case TileEntityType.LogicSensor: //it is a logic sensor
                                byte logicCheck = b.ReadByte();
                                bool on = b.ReadBoolean();
                                entity = new TELogicSensor()
                                {
                                    ID = id,
                                    type = (byte)type,
                                    Position = new Point16(posX, posY),
                                    logicCheck = (TELogicSensor.LogicCheckType)logicCheck,
                                    On = on,
                                };
                                break;
                            case TileEntityType.DisplayDoll: // display doll
                                {
                                    byte numSlots = 8;
                                    var itemSlots = (BitsByte)b.ReadByte();
                                    var dyeSlots = (BitsByte)b.ReadByte();
                                    Item[] items = new Item[numSlots];
                                    Item[] dyes = new Item[numSlots];
                                    entity = new TEDisplayDoll()
                                    {
                                        ID = id,
                                        type = (byte)type,
                                        Position = new Point16(posX, posY),
                                        _items = items,
                                        _dyes = dyes,
                                    };

                                    for (int index = 0; index < numSlots; index++)
                                    {
                                        if (itemSlots[index])
                                        {
                                            items[index] = new Item();
                                            items[index].SetDefaults(b.ReadInt16());
                                            items[index].prefix = b.ReadByte();
                                            items[index].stack = b.ReadInt16();
                                        }
                                    }

                                    for (int index = 0; index < numSlots; index++)
                                    {
                                        if (dyeSlots[index])
                                        {
                                            dyes[index] = new Item();
                                            dyes[index].SetDefaults(b.ReadInt16());
                                            dyes[index].prefix = b.ReadByte();
                                            dyes[index].stack = b.ReadInt16();
                                        }
                                    }
                                }

                                break;
                            case TileEntityType.WeaponRack: // weapons rack
                                {
                                    int netId = (int)b.ReadInt16();
                                    byte prefix = b.ReadByte();
                                    int stackSize = b.ReadInt16();
                                    Item item = new Item();
                                    item.SetDefaults(netId);
                                    item.prefix = prefix;
                                    item.stack = stackSize;
                                    entity = new TEWeaponsRack()
                                    {
                                        ID = id,
                                        type = (byte)type,
                                        Position = new Point16(posX, posY),
                                        item = item,
                                    };
                                }

                                break;
                            case TileEntityType.HatRack: // hat rack 
                                {
                                    byte numSlots = 2;
                                    var itemSlots = (BitsByte)b.ReadByte();
                                    var dyeSlots = (BitsByte)b.ReadByte();
                                    Item[] items = new Item[numSlots];
                                    Item[] dyes = new Item[numSlots];
                                    entity = new TEDisplayDoll()
                                    {
                                        ID = id,
                                        type = (byte)type,
                                        Position = new Point16(posX, posY),
                                        _items = items,
                                        _dyes = dyes,
                                    };

                                    for (int index = 0; index < numSlots; index++)
                                    {
                                        if (itemSlots[index])
                                        {
                                            items[index] = new Item();
                                            items[index].SetDefaults(b.ReadInt16());
                                            items[index].prefix = b.ReadByte();
                                            items[index].stack = b.ReadInt16();
                                        }
                                    }

                                    for (int index = 0; index < numSlots; index++)
                                    {
                                        if (dyeSlots[index])
                                        {
                                            dyes[index] = new Item();
                                            dyes[index].SetDefaults(b.ReadInt16());
                                            dyes[index].prefix = b.ReadByte();
                                            dyes[index].stack = b.ReadInt16();
                                        }
                                    }
                                }

                                break;
                            case TileEntityType.FoodPlatter: // food platter
                                {
                                    int netId = (int)b.ReadInt16();
                                    byte prefix = b.ReadByte();
                                    int stackSize = b.ReadInt16();
                                    Item item = new Item();
                                    item.SetDefaults(netId);
                                    item.prefix = prefix;
                                    item.stack = stackSize;
                                    entity = new TEFoodPlatter()
                                    {
                                        ID = id,
                                        type = (byte)type,
                                        Position = new Point16(posX, posY),
                                        item = item,
                                    };
                                }

                                break;
                            case TileEntityType.TeleportationPylon: // teleportation pylon
                                entity = new TETeleportationPylon()
                                {
                                    ID = id,
                                    type = (byte)type,
                                    Position = new Point16(posX, posY),
                                };
                                break;
                            default:
                                throw new ArgumentException($"Invalid entity type: {type}");
                        }

                        entities.Add(entity);
                    }

                    string verifyName = b.ReadString();
                    int verifyVersion = b.ReadInt32();
                    int verifyX = b.ReadInt32();
                    int verifyY = b.ReadInt32();
                    if (name == verifyName &&
                        version == verifyVersion &&
                        sizeX == verifyX &&
                        sizeY == verifyY)
                    {
                        // valid;
                        return (tiles, name, chests, signs, entities);
                    }

                    b.Close();
                }
            }

            throw new ArgumentException("File is not valid.");
        }

        private static Tile DeserializeTileData(BinaryReader r, int version, out int rle)
        {
            Tile tile = new Tile();

            rle = 0;
            int tileType = -1;
            // byte header4 = 0; // unused, future proofing
            byte header3 = 0;
            byte header2 = 0;
            byte header1 = r.ReadByte();

            // check bit[0] to see if header2 has data
            if ((header1 & 1) == 1)
            {
                header2 = r.ReadByte();

                // check bit[0] to see if header3 has data
                if ((header2 & 1) == 1)
                {
                    header3 = r.ReadByte();

                    // this doesn't exist yet
                    // if ((header3 & 1) == 1)
                    // {
                    //     header4 = r.ReadByte();
                    // }
                }
            }

            // check bit[1] for active tile
            if ((header1 & 2) == 2)
            {
                tile.active(true);

                // read tile type
                if ((header1 & 32) != 32) // check bit[5] to see if tile is byte or little endian int16
                {
                    // tile is byte
                    tileType = r.ReadByte();
                }
                else
                {
                    // tile is little endian int16
                    byte lowerByte = r.ReadByte();
                    tileType = r.ReadByte();
                    tileType = tileType << 8 | lowerByte;
                }

                tile.type = (ushort)tileType; // convert type to ushort after bit operations

                // read frame UV coords
                if (!Main.tileFrameImportant[tileType])
                {
                    tile.frameX = -1;
                    tile.frameY = -1;
                }
                else
                {
                    // read UV coords
                    tile.frameX = r.ReadInt16();
                    tile.frameY = r.ReadInt16();

                    // reset timers
                    if (tile.type == TileID.Timers)
                    {
                        tile.frameY = 0;
                    }
                }

                // check header3 bit[3] for tile color
                if ((header3 & 8) == 8)
                {
                    tile.color(r.ReadByte());
                }
            }

            // Read Walls
            if ((header1 & 4) == 4) // check bit[3] bit for active wall
            {
                tile.wall = r.ReadByte();

                // check bit[4] of header3 to see if there is a wall color
                if ((header3 & 16) == 16)
                {
                    tile.wallColor(r.ReadByte());
                }
            }

            // check for liquids, grab the bit[3] and bit[4], shift them to the 0 and 1 bits
            byte liquidType = (byte)((header1 & 24) >> 3);
            if (liquidType != 0)
            {
                tile.liquid = r.ReadByte();
                tile.liquidType(liquidType);
            }

            // check if we have data in header2 other than just telling us we have header3
            if (header2 > 1)
            {
                // check bit[1] for red wire
                if ((header2 & 2) == 2)
                {
                    tile.wire(true);
                }

                // check bit[2] for blue wire
                if ((header2 & 4) == 4)
                {
                    tile.wire2(true);
                }

                // check bit[3] for green wire
                if ((header2 & 8) == 8)
                {
                    tile.wire3(true);
                }

                // grab bits[4, 5, 6] and shift 4 places to 0,1,2. This byte is our brick style
                byte brickStyle = (byte)((header2 & 112) >> 4);
                if (brickStyle != 0 && tile.type < TileID.Count && TileID.Sets.HasSlopeFrames[tile.type])
                {
                    if (brickStyle == 1)
                    {
                        tile.halfBrick(true);
                    }
                    else
                    {
                        tile.slope((byte)(brickStyle - 1));
                    }
                }
            }

            // check if we have data in header3 to process
            if (header3 > 0)
            {
                // check bit[1] for actuator
                if ((header3 & 2) == 2)
                {
                    tile.actuator(true);
                }

                // check bit[2] for inactive due to actuator
                if ((header3 & 4) == 4)
                {
                    tile.inActive(true);
                }

                if ((header3 & 32) == 32)
                {
                    tile.wire4(true);
                }

                if (version >= 222)
                {
                    if ((header3 & 64) == 64)
                    {
                        tile.wall = (ushort)(r.ReadByte() << 8 | tile.wall);
                    }
                }
            }

            // get bit[6,7] shift to 0,1 for RLE encoding type
            // 0 = no RLE compression
            // 1 = byte RLE counter
            // 2 = int16 RLE counter
            // 3 = ERROR
            byte rleStorageType = (byte)((header1 & 192) >> 6);
            switch (rleStorageType)
            {
                case 0:
                    rle = 0;
                    break;
                case 1:
                    rle = r.ReadByte();
                    break;
                default:
                    rle = r.ReadInt16();
                    break;
            }

            return tile;
        }

        private static byte[] SerializeTileData(Tile tile, out int dataIndex, out int headerIndex)
        {
            byte[] tileData = new byte[15];
            dataIndex = 3;

            byte header3 = (byte)0;
            byte header2 = (byte)0;
            byte header1 = (byte)0;

            // tile data
            if (tile.active())
            {
                // activate bit[1]
                header1 = (byte)(header1 | 2);

                if (tile.type == TileID.MagicalIceBlock && tile.active())
                {
                    tile.active(false);
                }

                // save tile type as byte or int16
                tileData[dataIndex++] = (byte)tile.type;
                if (tile.type > 255)
                {
                    // write high byte
                    tileData[dataIndex++] = (byte)(tile.type >> 8);

                    // set header1 bit[5] for int16 tile type
                    header1 = (byte)(header1 | 32);
                }

                if (Main.tileFrameImportant[tile.type])
                {
                    // pack UV coords
                    tileData[dataIndex++] = (byte)(tile.frameX & 255);
                    tileData[dataIndex++] = (byte)((tile.frameX & 65280) >> 8);
                    tileData[dataIndex++] = (byte)(tile.frameY & 255);
                    tileData[dataIndex++] = (byte)((tile.frameY & 65280) >> 8);
                }

                if (tile.color() != 0)
                {
                    // set header3 bit[3] for tile color active
                    header3 = (byte)(header3 | 8);
                    tileData[dataIndex++] = tile.color();
                }
            }

            // wall data
            if (tile.wall != 0)
            {
                // set header1 bit[2] for wall active
                header1 = (byte)(header1 | 4);
                tileData[dataIndex++] = (byte)tile.wall;

                // save tile wall color
                if (tile.wallColor() != 0)
                {
                    // set header3 bit[4] for wall color active
                    header3 = (byte)(header3 | 16);
                    tileData[dataIndex++] = tile.wallColor();
                }
            }

            // liquid data
            if (tile.liquid != 0 && tile.liquidType() != 0)
            {
                // set bits[3,4] using left shift
                header1 = (byte)(header1 | (byte)((byte)tile.liquidType() << 3));
                tileData[dataIndex++] = tile.liquid;
            }

            // wire data
            if (tile.wire())
            {
                // red wire = header2 bit[1]
                header2 = (byte)(header2 | 2);
            }

            if (tile.wire2())
            {
                // blue wire = header2 bit[2]
                header2 = (byte)(header2 | 4);
            }

            if (tile.wire3())
            {
                // green wire = header2 bit[3]
                header2 = (byte)(header2 | 8);
            }

            // brick style
            byte brickStyle = tile.halfBrick() ? (byte)(1 << 4) : (byte)((byte)(tile.slope() + 1) << 4);
            // set bits[4,5,6] of header2
            header2 = (byte)(header2 | brickStyle);

            // actuator data
            if (tile.actuator())
            {
                // set bit[1] of header3
                header3 = (byte)(header3 | 2);
            }

            if (tile.inActive())
            {
                // set bit[2] of header3
                header3 = (byte)(header3 | 4);
            }

            if (tile.wire4())
            {
                header3 = (byte)(header3 | 32);
            }

            if (tile.wall > 255)
            {
                tileData[dataIndex++] = (byte)(tile.wall >> 8);
                header3 = (byte)(header3 | 64);
            }

            headerIndex = 2;
            if (header3 != 0)
            {
                // set header3 active flag bit[0] of header2
                header2 = (byte)(header2 | 1);
                tileData[headerIndex--] = header3;
            }
            if (header2 != 0)
            {
                // set header2 active flag bit[0] of header1
                header1 = (byte)(header1 | 1);
                tileData[headerIndex--] = header2;
            }

            tileData[headerIndex] = header1;
            return tileData;
        }
    }
}
