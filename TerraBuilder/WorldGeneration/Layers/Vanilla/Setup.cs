// Copyright (c) Miyabi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace TerraBuilder.WorldGeneration.Layers.Vanilla
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using TerraBuilder.WorldEdit;
    using Terraria.ID;

    /// <summary>
    /// ワールド全体の設定を行う.
    /// バニラの"Reset"に相当.
    /// </summary>
    public class Setup : IWorldGenerationLayer<Setup.SetupConfig>
    {
        /// <inheritdoc/>
        public string Name => throw new NotImplementedException();

        /// <inheritdoc/>
        public string Description => throw new NotImplementedException();

        /// <summary>
        /// <see cref="Setup"/>のコンフィグ.
        /// </summary>
        public SetupConfig Config { get; } = new SetupConfig();

        /// <inheritdoc/>
        public bool Apply(WorldGenerationRunner runner, WorldSandbox sandbox, out Dictionary<string, object> generatedValueDict)
        {
            //Liquid.ReInit();
            //SetupStatueList();
            //RandomizeWeather();
            //Main.checkXMas();
            //Main.checkHalloween();
            UndergroundDesertLocation = Rectangle.Empty;
            UndergroundDesertHiveLocation = Rectangle.Empty;
            int num935 = 86400;
            Main.slimeRainTime = -genRand.Next(num935 * 2, num935 * 3);
            Main.cloudBGActive = -genRand.Next(8640, 86400);

            // TODO: DRYにするか判断
            switch (this.Config.CopperTierOreSelectMode)
            {
                case SetupConfig.CopperTierOreSelectionMode.Random:
                    if (runner.Random.Next(2) == 0)
                    {
                        sandbox.WorldSetting.OreTiers.CopperTierTileId = TileID.Copper;
                        sandbox.WorldSetting.OreTiers.CopperTierBarItemId = ItemID.CopperBar;
                    }
                    else
                    {
                        sandbox.WorldSetting.OreTiers.CopperTierTileId = TileID.Tin;
                        sandbox.WorldSetting.OreTiers.CopperTierBarItemId = ItemID.TinBar;
                    }

                    break;
                case SetupConfig.CopperTierOreSelectionMode.Copper:
                    sandbox.WorldSetting.OreTiers.CopperTierTileId = TileID.Copper;
                    sandbox.WorldSetting.OreTiers.CopperTierBarItemId = ItemID.CopperBar;
                    break;
                case SetupConfig.CopperTierOreSelectionMode.Tin:
                    sandbox.WorldSetting.OreTiers.CopperTierTileId = TileID.Tin;
                    sandbox.WorldSetting.OreTiers.CopperTierBarItemId = ItemID.TinBar;
                    break;
            }

            switch (this.Config.IronTierOreSelectMode)
            {
                case SetupConfig.IronTierOreSelectionMode.Random:
                    if (runner.Random.Next(2) == 0)
                    {
                        sandbox.WorldSetting.OreTiers.IronTierTileId = TileID.Iron;
                        sandbox.WorldSetting.OreTiers.IronTierBarItemId = ItemID.IronBar;
                    }
                    else
                    {
                        sandbox.WorldSetting.OreTiers.IronTierTileId = TileID.Lead;
                        sandbox.WorldSetting.OreTiers.IronTierBarItemId = ItemID.LeadBar;
                    }

                    break;
                case SetupConfig.IronTierOreSelectionMode.Iron:
                    sandbox.WorldSetting.OreTiers.IronTierTileId = TileID.Iron;
                    sandbox.WorldSetting.OreTiers.IronTierBarItemId = ItemID.IronBar;
                    break;
                case SetupConfig.IronTierOreSelectionMode.Lead:
                    sandbox.WorldSetting.OreTiers.IronTierTileId = TileID.Lead;
                    sandbox.WorldSetting.OreTiers.IronTierBarItemId = ItemID.LeadBar;
                    break;
            }

            switch (this.Config.SilverTierOreSelectMode)
            {
                case SetupConfig.SilverTierOreSelectionMode.Random:
                    if (runner.Random.Next(2) == 0)
                    {
                        sandbox.WorldSetting.OreTiers.SilverTierTileId = TileID.Silver;
                        sandbox.WorldSetting.OreTiers.SilverTierBarItemId = ItemID.SilverBar;
                    }
                    else
                    {
                        sandbox.WorldSetting.OreTiers.SilverTierTileId = TileID.Tungsten;
                        sandbox.WorldSetting.OreTiers.SilverTierBarItemId = ItemID.TungstenBar;
                    }

                    break;
                case SetupConfig.SilverTierOreSelectionMode.Silver:
                    sandbox.WorldSetting.OreTiers.SilverTierTileId = TileID.Silver;
                    sandbox.WorldSetting.OreTiers.SilverTierBarItemId = ItemID.SilverBar;
                    break;
                case SetupConfig.SilverTierOreSelectionMode.Tungsten:
                    sandbox.WorldSetting.OreTiers.SilverTierTileId = TileID.Tungsten;
                    sandbox.WorldSetting.OreTiers.SilverTierBarItemId = ItemID.TungstenBar;
                    break;
            }

            switch (this.Config.GoldTierOreSelectMode)
            {
                case SetupConfig.GoldTierOreSelectionMode.Random:
                    if (runner.Random.Next(2) == 0)
                    {
                        sandbox.WorldSetting.OreTiers.GoldTierTileId = TileID.Gold;
                        sandbox.WorldSetting.OreTiers.GoldTierBarItemId = ItemID.GoldBar;
                    }
                    else
                    {
                        sandbox.WorldSetting.OreTiers.GoldTierTileId = TileID.Platinum;
                        sandbox.WorldSetting.OreTiers.GoldTierBarItemId = ItemID.PlatinumBar;
                    }

                    break;
                case SetupConfig.GoldTierOreSelectionMode.Gold:
                    sandbox.WorldSetting.OreTiers.GoldTierTileId = TileID.Gold;
                    sandbox.WorldSetting.OreTiers.GoldTierBarItemId = ItemID.GoldBar;
                    break;
                case SetupConfig.GoldTierOreSelectionMode.Platinum:
                    sandbox.WorldSetting.OreTiers.GoldTierTileId = TileID.Platinum;
                    sandbox.WorldSetting.OreTiers.GoldTierBarItemId = ItemID.PlatinumBar;
                    break;
            }

            switch (this.Config.EvilSelectMode)
            {
                case SetupConfig.EvilSelectionMode.Random:
                    sandbox.WorldSetting.Evil = runner.Random.Next(2) == 0 ? WorldSetting.WorldEvil.Corruption : WorldSetting.WorldEvil.Crimson;
                    break;
                case SetupConfig.EvilSelectionMode.Corruption:
                    sandbox.WorldSetting.Evil = WorldSetting.WorldEvil.Corruption;
                    break;
                case SetupConfig.EvilSelectionMode.Crimson:
                    sandbox.WorldSetting.Evil = WorldSetting.WorldEvil.Crimson;
                    break;
            }

            sandbox.WorldSetting.WorldID = runner.Random.Next(int.MaxValue);
            //RandomizeTreeStyle();
            //RandomizeCaveBackgrounds();
            //RandomizeBackgrounds(genRand);
            //RandomizeMoonState();
            //TreeTops.CopyExistingWorldInfoForWorldGeneration();

            switch (this.Config.DungeonSideSelectMode)
            {
                case SetupConfig.DungeonSideSelectionMode.Random:
                    sandbox.WorldSetting.DungeonSide = runner.Random.Next(2) == 0 ? WorldEdit.Settings.DungeonSide.Right : WorldEdit.Settings.DungeonSide.Left;
                    break;
                case SetupConfig.DungeonSideSelectionMode.Right:
                    sandbox.WorldSetting.DungeonSide = WorldEdit.Settings.DungeonSide.Right;
                    break;
                case SetupConfig.DungeonSideSelectionMode.Left:
                    sandbox.WorldSetting.DungeonSide = WorldEdit.Settings.DungeonSide.Left;
                    break;
            }

            // TODO: 割合の設定をコンフィグ化？
            const float snowBiomeSelectionRangePercentileOppositeDungeon = 0.6f;
            const float snowBiomeSelectionRangePercentileDungeon = 0.25f;
            int snowCenter = runner.Random.Next(sandbox.TileCountX);
            if (sandbox.WorldSetting.DungeonSide == WorldEdit.Settings.DungeonSide.Right)
            {
                while (snowCenter < sandbox.TileCountX * snowBiomeSelectionRangePercentileOppositeDungeon
                    || snowCenter > sandbox.TileCountX * (1 - snowBiomeSelectionRangePercentileDungeon))
                {
                    snowCenter = runner.Random.Next(sandbox.TileCountX);
                }
            }
            else if (sandbox.WorldSetting.DungeonSide == WorldEdit.Settings.DungeonSide.Left)
            {
                while (snowCenter < sandbox.TileCountX * snowBiomeSelectionRangePercentileDungeon
                    || snowCenter > sandbox.TileCountX * (1 - snowBiomeSelectionRangePercentileOppositeDungeon))
                {
                    snowCenter = runner.Random.Next(sandbox.TileCountX);
                }
            }

            // TODO この辺の設定を全てコンフィグ化
            const int beachBordersWidth = 275;
            const int beachSandRandomCenter = beachBordersWidth + 5 + 40;
            const int beachSandRandomWidthRange = 20;
            const int beachSandDungeonExtraWidth = 40;
            const int beachSandJungleExtraWidth = 20;

            int leftBeachEnd = 0;
            int rightBeachStart = 0;

            int snowWidth = runner.Random.Next(50, 90);
            float scale = sandbox.TileCountX / 4200f;
            snowWidth += (int)(runner.Random.Next(20, 40) * scale);
            snowWidth += (int)(runner.Random.Next(20, 40) * scale);
            int snowOriginLeft = Math.Max(0, snowCenter - snowWidth);
            snowWidth = runner.Random.Next(50, 90);
            snowWidth += (int)(runner.Random.Next(20, 40) * scale);
            snowWidth += (int)(runner.Random.Next(20, 40) * scale);
            int snowOriginRight = Math.Min(sandbox.TileCountX, snowCenter + snowWidth);

            leftBeachEnd = runner.Random.Next(beachSandRandomCenter - beachSandRandomWidthRange, beachSandRandomCenter + beachSandRandomWidthRange);
            if (sandbox.WorldSetting.DungeonSide == WorldEdit.Settings.DungeonSide.Right)
            {
                leftBeachEnd += beachSandDungeonExtraWidth;
            }
            else
            {
                leftBeachEnd += beachSandJungleExtraWidth;
            }

            rightBeachStart = sandbox.TileCountX - runner.Random.Next(beachSandRandomCenter - beachSandRandomWidthRange, beachSandRandomCenter + beachSandRandomWidthRange);
            if (sandbox.WorldSetting.DungeonSide == WorldEdit.Settings.DungeonSide.Left)
            {
                rightBeachStart -= beachSandDungeonExtraWidth;
            }
            else
            {
                rightBeachStart -= beachSandJungleExtraWidth;
            }

            const int beachPadding = 50;
            int dungeonLocation;
            if (sandbox.WorldSetting.DungeonSide == WorldEdit.Settings.DungeonSide.Left)
            {
                dungeonLocation = runner.Random.Next(leftBeachEnd + beachPadding, (int)(sandbox.TileCountX * 0.2));
            }
            else
            {
                dungeonLocation = runner.Random.Next((int)(sandbox.TileCountX * 0.8), rightBeachStart - beachPadding);
            }

            generatedValueDict = new Dictionary<string, object>();
            return true;
        }

        /* バニラ生成名とレイヤー名対応表（仮置き）
* Reset -> Setup
* Terrain -> Terrain
* Dunes -> Dunes
* Ocean Sand -> OceanSand
* Sand Patches -> SandPatches
* Tunnels -> Tunnels
* Dirt Wall Backgrounds -> DirtWallBackgrounds
* Clay -> Clay
* Small Holes -> SmallHoles
* Dirt Layer Caves -> DirtLayerCaves
* Rock Layer Caves -> RockLayerCaves
* Surface Caves -> SurfaceCaves
* Wavy Caves -> WavyCaves
* Generate Ice Biome -> IceBiome
* Grass -> Grass
* Jungle -> Jungle
* Mud Caves To Grass -> MudCavesToGrass
* Full Desert -> FullDesert
* Floating Islands -> FloatingIslands
* Mushroom Patches -> MushroomPatches
* Marble -> Marble
* Granite -> Granite
* Dirt To Mud -> DirtToMud
* Silt -> Silt
* Shinies -> Shinies
* Webs -> Webs
* Underworld -> Underworld
* Corruption -> Corruption
* Lakes -> Lakes
* Dungeon -> Dungeon
* Slush -> Slush
* Mountain Caves -> MountainCaves
* Beaches -> Beaches
* Gems -> Gems
* Gravitating Sand -> GravitateSand
* Create Ocean Caves -> OceanCaves
* Clean Up Dirt -> CleanUpDirt
* Pyramids -> Pyramids
* Dirt Rock Wall Runner -> DirtRockWalls
* Living Trees -> LivingTrees
* Wood Tree Walls -> WoodTreeWalls
* Altars -> Altars
* Wet Jungle -> WetJungle
* Jungle Temple -> JungleTemple
* Hives -> Hives
* Jungle Chests -> JungleChests
* Settle Liquids -> SettleLiquids
* Remove Water From Sand -> RemoveWaterFromSand
* Oasis -> Oasis
* Shell Piles -> ShellPiles
* Smooth World -> SmoothWorld
* Waterfalls -> Waterfalls
* Ice -> Ice
* Wall Variety -> WallVariety
* Life Crystals -> LifeCrystals
* Statues -> Statues
* Buried Chests -> BuriedChests
* Surface Chests -> SurfaceChests
* Jungle Chests Placement -> JungleChestsPlacement
* Water Chests -> WaterChests
* Spider Caves -> SpiderCaves
* Gem Caves -> GemCaves
* Moss -> Moss
* Temple -> Temple
* Cave Walls -> CaveWalls
* Jungle Trees -> JungleTrees
* Floating Island Houses -> FloatingIslandHouses
* Quick Cleanup -> QuickCleanup
* Pots -> Pots
* Hellforge -> Hellforge
* Spreading Grass -> SpreadGrass
* Surface Ore and Stone -> SurfaceOreAndStone
* Place Fallen Log -> FallenLog
* Traps -> Traps
* Piles -> Piles
* Spawn Point -> SpawnPoint
* Grass Wall -> GrassWall
* Guide -> Guide
* Sunflowers -> Sunflowers
* Planting Trees -> PlantTrees
* Herbs -> Herbs
* Dye Plants -> DyePlants
* Webs and Honey -> WebsAndHoney
* Weeds -> Weeds
* Glowing Mushrooms and Jungle Plants -> GlowingMushroomsAndJunglePlants
* Jungle Plants -> JunglePlants
* Vines -> Vines
* Flowers -> Flowers
* Mushrooms -> Mushrooms
* Gems In Ice Biome -> GemsInIceBiome
* Random Gems -> RandomGems
* Moss Grass -> MossGrass
* Muds Walls In Jungle -> MudsWallsInJungle
* Larva -> Larva
* Settle Liquids Again -> SettleLiquidsAgain
* Cactus, Palm Trees & Coral -> SandPlants
* Tile Cleanup -> TileCleanup
* Lihzahrd Altars -> LihzahrdAltars
* Micro Biomes -> MicroBiomes
* Water Plants -> WaterPlants
* Stalac -> Stalac
* Remove Broken Traps -> RemoveBrokenTraps
* Final Cleanup -> FinalCleanup
*/

        /// <summary>
        /// <see cref="Setup"/>のコンフィグ.
        /// </summary>
        public class SetupConfig : LayerConfig
        {
            /// <summary>
            /// ワールドの不浄の選択モード.
            /// </summary>
            public enum EvilSelectionMode
            {
                /// <summary>
                /// ランダムに選択する.
                /// </summary>
                Random = 0,

                /// <summary>
                /// 紫不浄を選択する.
                /// </summary>
                Corruption = 1,

                /// <summary>
                /// クリムゾンを選択する.
                /// </summary>
                Crimson = 2,
            }

            /// <summary>
            /// ワールドの銅ティア鉱石選択モード.
            /// </summary>
            public enum CopperTierOreSelectionMode
            {
                /// <summary>
                /// ランダムに選択.
                /// </summary>
                Random = 0,

                /// <summary>
                /// 銅を選択.
                /// </summary>
                Copper = 1,

                /// <summary>
                /// 錫を選択.
                /// </summary>
                Tin = 2,
            }

            /// <summary>
            /// ワールドの鉄ティア鉱石選択モード.
            /// </summary>
            public enum IronTierOreSelectionMode
            {
                /// <summary>
                /// ランダムに選択.
                /// </summary>
                Random = 0,

                /// <summary>
                /// 鉄を選択.
                /// </summary>
                Iron = 1,

                /// <summary>
                /// 鉛を選択.
                /// </summary>
                Lead = 2,
            }

            /// <summary>
            /// ワールドの銀ティア鉱石選択モード.
            /// </summary>
            public enum SilverTierOreSelectionMode
            {
                /// <summary>
                /// ランダムに選択.
                /// </summary>
                Random = 0,

                /// <summary>
                /// 銀を選択.
                /// </summary>
                Silver = 1,

                /// <summary>
                /// タングステンを選択.
                /// </summary>
                Tungsten = 2,
            }

            /// <summary>
            /// ワールドの金ティア鉱石選択モード.
            /// </summary>
            public enum GoldTierOreSelectionMode
            {
                /// <summary>
                /// ランダムに選択.
                /// </summary>
                Random = 0,

                /// <summary>
                /// 金を選択.
                /// </summary>
                Gold = 1,

                /// <summary>
                /// プラチナを選択.
                /// </summary>
                Platinum = 2,
            }

            /// <summary>
            /// ダンジョンの設置サイド選択モード.
            /// </summary>
            public enum DungeonSideSelectionMode
            {
                /// <summary>
                /// ランダムに選択.
                /// </summary>
                Random = 0,

                /// <summary>
                /// 右側.
                /// </summary>
                Right = 1,

                /// <summary>
                /// 左側.
                /// </summary>
                Left = 2,
            }

            /// <summary>
            /// 不浄の選択モード.
            /// </summary>
            public EvilSelectionMode EvilSelectMode { get; set; }

            /// <summary>
            /// 銅ティア鉱石の選択モード.
            /// </summary>
            public CopperTierOreSelectionMode CopperTierOreSelectMode { get; set; }

            /// <summary>
            /// 鉄ティア鉱石の選択モード.
            /// </summary>
            public IronTierOreSelectionMode IronTierOreSelectMode { get; set; }

            /// <summary>
            /// 銀ティア鉱石の選択モード.
            /// </summary>
            public SilverTierOreSelectionMode SilverTierOreSelectMode { get; set; }

            /// <summary>
            /// 金ティア鉱石の選択モード.
            /// </summary>
            public GoldTierOreSelectionMode GoldTierOreSelectMode { get; set; }

            /// <summary>
            /// ダンジョンの設置サイド選択モード.
            /// </summary>
            public DungeonSideSelectionMode DungeonSideSelectMode { get; set; }
        }
    }
}
