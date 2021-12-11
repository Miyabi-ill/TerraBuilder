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
        public RandomValue<int> Width { get; set; }

        /// <summary>
        /// 高さ
        /// </summary>
        public RandomValue<int> Height { get; set; }

        public Size(int width, int height)
        {
            Width = new ConstantValue<int> { Value = width };
            Height = new ConstantValue<int> { Value = height };
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"w: {Width}, h: {Height}";
        }
    }
}
