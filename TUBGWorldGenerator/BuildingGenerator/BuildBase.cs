namespace TUBGWorldGenerator.BuildingGenerator
{
    using System.ComponentModel;
    using System.Xml.Serialization;
    using Newtonsoft.Json;
    using Terraria;

    /// <summary>
    /// 建築生成の基底クラス
    /// </summary>
    public class BuildBase : INotifyPropertyChanged
    {
        /// <inheritdoc/>
        public event PropertyChangedEventHandler PropertyChanged;

        private int x = 1;
        private int y = 1;
        private string name;

        [JsonProperty]
        [DefaultValue(1)]
        /// <summary>
        /// 建築の基準点X。左下を0とする。
        /// </summary>
        public virtual int X
        {
            get => x;
            set
            {
                x = value;
                RaisePropertyChanged(nameof(X));
            }
        }

        [JsonProperty]
        [DefaultValue(1)]
        /// <summary>
        /// 建築の基準点Y。左下を0とする。
        /// </summary>
        public virtual int Y
        {
            get => y;
            set
            {
                y = value;
                RaisePropertyChanged(nameof(Y));
            }
        }

        [JsonProperty]
        /// <summary>
        /// 建築名。<see cref="Import"/>に使われる。
        /// </summary>
        public virtual string Name
        {
            get => name;
            set
            {
                name = value;
                RaisePropertyChanged(nameof(Name));
            }
        }

        /// <summary>
        /// PropertyChangedイベントをInvokeする
        /// </summary>
        /// <param name="propertyName">プロパティ名</param>
        public void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// タイル配列を生成する
        /// </summary>
        /// <returns>生成したタイル配列</returns>
        public virtual Tile[,] Build()
        {
            return new Tile[0, 0];
        }
    }
}
