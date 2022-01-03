namespace TerraBuilder.WorldEdit.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TerraBuilder.WorldEdit;
    using Terraria;
    using Terraria.ID;

    [TestClass]
    public class TileProtectionMapTests
    {
        [TestMethod]
        public void TileProtectionMap_IndexerTest()
        {
            // TODO: 他のテストを考える
            // 大きなCoordinateでArgumentOutOfRange?
            Coordinate coordinate = new(0, 0);

            WorldSandbox sandbox = new();
            // 最初は保護が何もないことを期待
            Assert.AreEqual(TileProtectionMap.TileProtectionType.None, sandbox.TileProtectionMap[coordinate]);
            sandbox.TileProtectionMap[coordinate] = TileProtectionMap.TileProtectionType.Solid;
            Assert.AreEqual(TileProtectionMap.TileProtectionType.Solid, sandbox.TileProtectionMap[coordinate]);
        }

        [TestMethod]
        public void ClearTest()
        {
            Coordinate coordinate = new(1, 1);

            WorldSandbox sandbox = new();
            // 一つ書き換えてからクリアする.
            sandbox.TileProtectionMap[coordinate] = TileProtectionMap.TileProtectionType.Solid;
            sandbox.TileProtectionMap.Clear();
            Assert.AreEqual(TileProtectionMap.TileProtectionType.None, sandbox.TileProtectionMap[coordinate]);
        }

        [TestMethod("PlaceTileTest_保護なし")]
        public void PlaceTileTest_NoneProtection()
        {
            Tile tile = new();
            tile.type = 15;
            tile.active(false);
            tile.wall = 24;
            tile.liquid = 200;
            tile.wire2(true);
            tile.color(12);
            tile.wallColor(13);

            Tile other = new();
            other.type = 16;
            other.active(true);
            other.wall = 25;
            other.liquid = 201;
            other.wire3(true);
            other.color(11);
            other.wallColor(12);
            Coordinate coordinate = new(1, 1);

            WorldSandbox sandbox = new();
            _ = sandbox.TileProtectionMap.PlaceTile(coordinate, tile);
            Tile result = sandbox.TileProtectionMap.PlaceTile(coordinate, other);
            Assert.IsFalse(ReferenceEquals(other, result));
            Assert.AreEqual(other, result);
        }

        [TestMethod("PlaceTileTest_タイルタイプ保護")]
        public void PlaceTileTest_TileTypeProtection()
        {
            Tile tile = new();
            tile.type = 15;
            tile.active(false);
            tile.wall = 24;
            tile.liquid = 200;
            tile.wire2(true);
            tile.color(12);
            tile.wallColor(13);

            Tile other = new();
            other.type = 16;
            other.active(true);
            other.wall = 25;
            other.liquid = 201;
            other.wire3(true);
            other.color(11);
            other.wallColor(12);
            Coordinate coordinate = new(1, 1);

            WorldSandbox sandbox = new();
            _ = sandbox.TileProtectionMap.PlaceTile(coordinate, tile);
            sandbox.TileProtectionMap[coordinate] = TileProtectionMap.TileProtectionType.TileType;
            Tile result = sandbox.TileProtectionMap.PlaceTile(coordinate, other);
            Assert.AreEqual(result.type, 15);
            other.type = 15;
            Assert.IsFalse(ReferenceEquals(other, result));
            Assert.AreEqual(other, result);
        }

        [TestMethod("PlaceTileTest_タイルフレーム保護")]
        public void PlaceTileTest_TileFrameProtection()
        {
            Tile tile = new();
            tile.type = 15;
            tile.frameX = 18;
            tile.frameY = 19;
            tile.active(false);
            tile.wall = 24;
            tile.liquid = 200;
            tile.wire2(true);
            tile.color(12);
            tile.wallColor(13);

            Tile other = new();
            other.type = 16;
            other.frameX = 20;
            other.frameY = 21;
            other.active(true);
            other.wall = 25;
            other.liquid = 201;
            other.wire3(true);
            other.color(11);
            other.wallColor(12);
            Coordinate coordinate = new(1, 1);

            WorldSandbox sandbox = new();
            _ = sandbox.TileProtectionMap.PlaceTile(coordinate, tile);
            sandbox.TileProtectionMap[coordinate] = TileProtectionMap.TileProtectionType.TileFrame;
            Tile result = sandbox.TileProtectionMap.PlaceTile(coordinate, other);
            Assert.AreEqual(result.frameX, 18);
            Assert.AreEqual(result.frameY, 19);
            Assert.IsFalse(ReferenceEquals(other, result));
            other.frameX = 18;
            other.frameY = 19;
            Assert.AreEqual(other, result);
        }
    }
}