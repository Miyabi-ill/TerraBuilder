namespace TUBGWorldGenerator.Utils
{
    using System;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Windows.Media.Imaging;
    using Terraria.Map;
    using TUBGWorldGenerator.WorldGeneration;

    public class WorldToImage
    {
        public static BitmapImage CreateMapImage(WorldSandbox sandbox)
        {
            int arrayIndex = 0;
            var array = new byte[sandbox.TileCountX * sandbox.TileCountY * 3];

            WorldMap worldMap = new WorldMap(sandbox.TileCountX, sandbox.TileCountY)
            {
                _tiles = new MapTile[sandbox.TileCountX, sandbox.TileCountY],
            };

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

        private static BitmapImage Convert(Bitmap src)
        {
            MemoryStream ms = new MemoryStream();
            src.Save(ms, ImageFormat.Bmp);
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            image.StreamSource = ms;
            image.EndInit();
            return image;
        }

        private static Bitmap CreateBitmap(int width, int height, byte[] datas)
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
