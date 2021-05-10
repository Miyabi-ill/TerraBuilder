namespace TUBGWorldGenerator.BuildingGenerator
{
    /// <summary>
    /// サイズ
    /// </summary>
    public struct Size
    {
        /// <summary>
        /// 幅
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// 高さ
        /// </summary>
        public int Height { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"w: {Width}, h: {Height}";
        }
    }
}
