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
        public RandomValue Width { get; set; }

        /// <summary>
        /// 高さ
        /// </summary>
        public RandomValue Height { get; set; }

        public Size(int width, int height)
        {
            Width = new ConstantValue(width);
            Height = new ConstantValue(height);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"w: {Width}, h: {Height}";
        }
    }
}
