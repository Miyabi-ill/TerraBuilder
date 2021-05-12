using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TUBGWorldGenerator.BuildingGenerator.UI
{
    public class TextureLoader
    {
        //private static readonly Color ColorKey = Color.FromNonPremultiplied(247, 119, 249, 255);
        //private Bitmap defaultTexture;
        private Bitmap actuator;

        public string ExtractedContentsDir { get; set; }

        public static Microsoft.Xna.Framework.Rectangle ZeroSixteenRectangle { get; } = new Microsoft.Xna.Framework.Rectangle(0, 0, 16, 16);

        private Dictionary<int, Bitmap> Moon { get; } = new Dictionary<int, Bitmap>();

        private Dictionary<int, Bitmap> Tiles { get; } = new Dictionary<int, Bitmap>();

        private Dictionary<int, Bitmap> Underworld { get; } = new Dictionary<int, Bitmap>();

        private Dictionary<int, Bitmap> Backgrounds { get; } = new Dictionary<int, Bitmap>();

        private Dictionary<int, Bitmap> Walls { get; } = new Dictionary<int, Bitmap>();

        private Dictionary<int, Bitmap> Trees { get; } = new Dictionary<int, Bitmap>();

        private Dictionary<int, Bitmap> TreeTops { get; } = new Dictionary<int, Bitmap>();

        private Dictionary<int, Bitmap> TreeBranches { get; } = new Dictionary<int, Bitmap>();

        private Dictionary<int, Bitmap> Shrooms { get; } = new Dictionary<int, Bitmap>();

        private Dictionary<int, Bitmap> Npcs { get; } = new Dictionary<int, Bitmap>();

        private Dictionary<int, Bitmap> Liquids { get; } = new Dictionary<int, Bitmap>();

        private Dictionary<string, Bitmap> Misc { get; } = new Dictionary<string, Bitmap>();

        private Dictionary<int, Bitmap> ArmorHead { get; } = new Dictionary<int, Bitmap>();

        private Dictionary<int, Bitmap> ArmorBody { get; } = new Dictionary<int, Bitmap>();

        private Dictionary<int, Bitmap> ArmorFemale { get; } = new Dictionary<int, Bitmap>();

        private Dictionary<int, Bitmap> ArmorLegs { get; } = new Dictionary<int, Bitmap>();

        private Dictionary<int, Bitmap> Item { get; } = new Dictionary<int, Bitmap>();

        public Bitmap Actuator { get { return actuator ?? (actuator = (Bitmap)GetMisc("Actuator")); } }

        public Bitmap GetTile(int num) => GetTextureById(Tiles, num, "Images\\Tiles_{0}");

        public Bitmap GetUnderworld(int num) => GetTextureById(Underworld, num, "Images\\Backgrounds\\Underworld {0}");

        public Bitmap GetBackground(int num) => GetTextureById(Backgrounds, num, "Images\\Background_{0}");

        public Bitmap GetWall(int num) => GetTextureById(Walls, num, "Images\\Wall_{0}");

        public Bitmap GetTree(int num)
        {
            if (!Trees.ContainsKey(num))
            {
                if (num >= 0)
                {
                    string name = $"Images\\Tiles_5_{num}";
                    Trees[num] = LoadTexture(name);
                }
                else
                {
                    string name = "Images\\Tiles_5";
                    Trees[num] = LoadTexture(name);
                }
            }
            return Trees[num];
        }

        public Bitmap GetTreeTops(int num) => GetTextureById(TreeTops, num, "Images\\Tree_Tops_{0}");

        public Bitmap GetTreeBranches(int num) => GetTextureById(TreeBranches, num, "Images\\Tree_Branches_{0}");

        public Bitmap GetShroomTop(int num) => GetTextureById(Shrooms, num, "Images\\Shroom_Tops");

        public Bitmap GetNPC(int num) => GetTextureById(Npcs, num, "Images\\NPC_{0}");

        public Bitmap GetLiquid(int num) => GetTextureById(Liquids, num, "Images\\Liquid_{0}");

        public Bitmap GetMisc(string name) => GetTextureById(Misc, name, "Images\\{0}");

        public Bitmap GetArmorHead(int num) => GetTextureById(ArmorHead, num, "Images\\Armor_Head_{0}");

        public Bitmap GetArmorBody(int num) => GetTextureById(ArmorBody, num, "Images\\Armor_Body_{0}");

        public Bitmap GetArmorFemale(int num) => GetTextureById(ArmorFemale, num, "Images\\Female_Body_{0}");

        public Bitmap GetArmorLegs(int num) => GetTextureById(ArmorLegs, num, "Images\\Armor_Legs_{0}");

        public Bitmap GetItem(int num) => GetTextureById(Item, num, "Images\\Item_{0}");

        public Bitmap GetMoon(int num) => GetTextureById(Moon, num, "Images\\Moon_{0}");

        private Bitmap GetTextureById<T>(Dictionary<T, Bitmap> collection, T id, string path)
        {
            if (!collection.ContainsKey(id))
            {
                string name = string.Format(path, id);
                collection[id] = LoadTexture(name);
            }

            return collection[id];
        }

        private Bitmap LoadTexture(string path)
        {
            path += ".png";
            path = Path.Combine(ExtractedContentsDir, path);
            if (File.Exists(path))
            {
                return (Bitmap)Image.FromFile(path);
            }

            return null;
        }
    }
}
