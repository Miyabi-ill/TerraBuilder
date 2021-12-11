﻿namespace TerraBuilder.BuildingGenerator.Parts
{
    using System;
    using Terraria;

    /// <summary>
    /// 既存の建築をインポートする。
    /// </summary>
    public class Import : BuildBase
    {
        private string buildingName;

        /// <summary>
        /// インポートする建物名
        /// </summary>
        public string BuildingName
        {
            get => buildingName;
            set
            {
                buildingName = value;
                RaisePropertyChanged(nameof(BuildingName));
            }
        }

        /// <inheritdoc/>
        public override Tile[,] Build(Random rand)
        {
            try
            {
                BuildBase build = BuildingGenerator.CurrentGenerator.BuildingsDict[BuildingName];
                if (build == null)
                {
                    return new Tile[0, 0];
                }
                else
                {
                    return build.Build(rand);
                }
            }
            catch
            {
                return new Tile[0, 0];
            }
        }
    }
}
