/*
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace TerraBuilder.WorldGeneration.Layers.Vanilla
{
    public class Marble : IWorldGenerationAction<Marble.MarbleContext>
    {
        /// <inheritdoc/>
        public string Name => "Vanilla:" + nameof(Marble);

        /// <inheritdoc/>
        public string Description => "バニラ：マーブル環境";

        /// <inheritdoc/>
        public MarbleContext Context { get; private set; } = new MarbleContext();

        /// <summary>
        /// マーブル環境を生成する
        /// </summary>
        /// <param name="sandbox"><inheritdoc/></param>
        /// <returns>生成に成功したらtrue</returns>
        public bool Run(WorldSandbox sandbox)
        {
            Random random = WorldGenerationRunner.CurrentRunner.GlobalContext.Random;
            int count = Context.Count;
            int maxRetry = Context.MaxRetry;
            for (int i = 0; i < count; i++)
            {
                for (int j = 0; j < maxRetry; j++)
                {
                    if (PlaceMarble(sandbox))
                    {
                        break;
                    }
                }
            }
        }

        private bool PlaceMarble(WorldSandbox sandbox, int originX, int originY)
        {
            Random random = WorldGenerationRunner.CurrentRunner.GlobalContext.Random;
            int width = random.Next(80, 150) / 3;
            int height = random.Next(40, 60) / 3;

            var slabs = new (SlabType slabType, bool hasWall)[width + 1, height + 1];

            // 3から12の範囲
            int factor = ((height * 3) - random.Next(20, 30)) / 3;
            originX -= width * 3 / 2;
            originY -= height * 3 / 2;
            for (int x = -1; x < width + 1; x++)
            {
                // 0から1の範囲、xが小さいほど小さい
                double percentileX = ((x - (width / 2)) / width) + 0.5;

                // -2から0の範囲、端にいくほど0に近く、中央に近いほど-2に近い
                int heightLevel = (int)((0.5 - Math.Abs(percentileX - 0.5)) * 5.0) - 2;
                for (int y = -1; y < height + 1; y++)
                {
                    bool hasWall = true;
                    bool shouldPlaceSolid = false;
                    bool isSolid = IsGroupSolid((x * 3) + originX, (y * 3) + originY, 3);
                    int num5 = Math.Abs(y - (height / 2)) - (factor / 4) + heightLevel;
                    if (num5 > 3)
                    {
                        shouldPlaceSolid = isSolid;
                        hasWall = false;
                    }
                    else if (num5 > 0)
                    {
                        shouldPlaceSolid = y - (height / 2) > 0 || isSolid;
                        hasWall = y - (height / 2) < 0 || num5 <= 2;
                    }
                    else if (num5 == 0)
                    {
                        shouldPlaceSolid = random.Next(2) == 0 && (y - (height / 2) > 0 || isSolid);
                    }

                    if (Math.Abs(percentileX - 0.5) > (double)(0.35f + (random.NextDouble() * 0.1f)) && !isSolid)
                    {
                        hasWall = false;
                        shouldPlaceSolid = false;
                    }

                    slabs[x + 1, y + 1] = (shouldPlaceSolid ? SlabType.Solid : SlabType.Empty, hasWall);
                }
            }

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    SmoothSlope(x + 1, y + 1);
                }
            }

            int halfWidth = width / 2;
            int halfHeight = height / 2;
            int i = (halfHeight + 1) * (halfHeight + 1);
            double startHeight = (random.NextDouble() * 2) - 1;
            double middleHeight = (random.NextDouble() * 2) - 1;
            double endHeight = (random.NextDouble() * 2) - 1;
            float offsetY = 0f;
            for (int x = 0; x <= width; x++)
            {
                float distance = halfHeight / (float)halfWidth * (x - halfWidth);
                int rangeY = Math.Min(halfHeight, (int)Math.Sqrt(Math.Max(0f, i - (distance * distance))));
                offsetY = (x >= width / 2) ? (offsetY + MathHelper.Lerp((float)middleHeight, (float)endHeight, (x / (width / 2)) - 1f)) : (offsetY + MathHelper.Lerp((float)startHeight, (float)middleHeight, x / (width / 2)));
                for (int y = halfHeight - rangeY; y <= halfHeight + rangeY; y++)
                {
                    PlaceSlab(slabs[x + 1, y + 1], (x * 3) + originX, (y * 3) + originY + (int)offsetY, 3);
                }
            }

            return true;
        }

        public class MarbleContext : ActionContext
        {
            public int Count { get; set; }

            public int MaxRetry { get; set; }
        }

        private enum SlabType
        {
            Solid,
            Empty,
        }
    }
}
*/