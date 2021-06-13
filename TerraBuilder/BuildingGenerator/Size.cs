namespace TerraBuilder.BuildingGenerator
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

        public Size(int width, int height)
        {
            Width = width;
            Height = height;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"w: {Width}, h: {Height}";
        }
    }
}
