namespace TerraBuilder.BuildingGenerator
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Media.Imaging;
    using Newtonsoft.Json;
    using TerraBuilder.BuildingGenerator.Parts;
    using TerraBuilder.BuildingGenerator.UI;
    using Terraria;

    public class BuildingCache
    {
        private string cacheDirectory = "Cache";

        public BuildingCache(BuildingGenerator buildingGenerator)
        {
            BuildingGenerator = buildingGenerator;
            ReloadDirectory();

            LoadFavorites();
        }

        public string CacheDirectory
        {
            get => cacheDirectory;
            set
            {
                cacheDirectory = value;
                FileNameRegex = new Regex(value.Trim('\\') + @"\\(.*)\.png");
                ReloadDirectory();
            }
        }

        public string BuildingDirectory
        {
            get => BuildingGenerator.BuildingsRootPath;
            set => BuildingGenerator.BuildingsRootPath = value;
        }

        public BuildingFavorites Favorites { get; set; }

        private BuildingGenerator BuildingGenerator { get; }

        private HashAlgorithm HashAlgorithm { get; set; } = SHA256.Create();

        private Dictionary<string, BuildRoot> BuildingNameBuildDictionary { get; } = new Dictionary<string, BuildRoot>();

        private Dictionary<string, Tile[,]> BuildingNameTilesDictionary { get; } = new Dictionary<string, Tile[,]>();

        private Dictionary<string, BitmapImage> BuildingNameBitmapDictionary { get; } = new Dictionary<string, BitmapImage>();

        private Dictionary<string, BitmapImage> ItemNameBitmapDictionary { get; } = new Dictionary<string, BitmapImage>();

        private Dictionary<string, (string name, ObservableCollection<string> tags)> TileTags { get; set; } = new Dictionary<string, (string name, ObservableCollection<string> tags)>();

        /// <summary>
        /// キャッシュディレクトリの辞書.ファイル名(の拡張子を除いた名前)に対し、建物名とハッシュを保存する.Jsonに書き込み/読み込みする.
        /// </summary>
        private Dictionary<string, (string name, string hash, ObservableCollection<string> tags)> CacheFileNameDictionary { get; set; } = new Dictionary<string, (string name, string hash, ObservableCollection<string> tags)>();

        private Regex FileNameRegex { get; set; } = new Regex(@"Cache\\(.*)\.png");

        public void ReloadFile(string fileName, bool saveCacheDict = false)
        {
            if (string.Equals(Path.GetExtension(fileName), ".json", StringComparison.InvariantCultureIgnoreCase))
            {
                // 親ディレクトリの名前と拡張子を省く
                string buildingFileName = string.IsNullOrEmpty(BuildingDirectory) ? fileName : fileName.Replace(BuildingDirectory, string.Empty).Trim('\\');
                buildingFileName = buildingFileName.Substring(0, buildingFileName.Length - 5);

                string text;
                using (StreamReader sr = new StreamReader(fileName))
                {
                    text = sr.ReadToEnd();
                }

                // ファイル名がある かつ ハッシュ一致
                if (CacheFileNameDictionary.ContainsKey(buildingFileName)
                    && VerifyHash(text, CacheFileNameDictionary[buildingFileName].hash))
                {
                    // 画像は必要なときに読み込む
                    return;
                }

                // それ以外は新規作成
                else
                {
                    try
                    {
                        var build = JsonConvert.DeserializeObject<BuildRoot>(text, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto });
                        if (!string.IsNullOrEmpty(build.Name))
                        {
                            BitmapImage image = TileToImage.CreateBitmap(build.Build(WorldGeneration.WorldGenerationRunner.CurrentRunner.GlobalContext.Random));
                            BuildingNameBitmapDictionary.Add(buildingFileName, image);
                            BuildingNameBuildDictionary.Add(buildingFileName, build);

                            BitmapEncoder encoder = new PngBitmapEncoder();
                            encoder.Frames.Add(BitmapFrame.Create(image));

                            string savePath = Path.Combine(CacheDirectory, buildingFileName + ".png");
                            if (!Directory.Exists(Path.GetDirectoryName(savePath)))
                            {
                                Directory.CreateDirectory(Path.GetDirectoryName(savePath));
                            }

                            using (var fileStream = new FileStream(savePath, FileMode.Create))
                            {
                                encoder.Save(fileStream);
                            }

                            if (CacheFileNameDictionary.ContainsKey(buildingFileName))
                            {
                                CacheFileNameDictionary[buildingFileName] = (build.Name, CalculateHash(text), build.Tags);
                            }
                            else
                            {
                                CacheFileNameDictionary.Add(buildingFileName, (build.Name, CalculateHash(text), build.Tags));
                            }
                        }
                    }
                    catch
                    {
                    }
                }
            }
            else if (string.Equals(Path.GetExtension(fileName), ".TEditSch", StringComparison.InvariantCultureIgnoreCase))
            {
                string buildingFileName = string.IsNullOrEmpty(BuildingDirectory) ? fileName : fileName.Replace(BuildingDirectory, string.Empty).Trim('\\');
                buildingFileName = buildingFileName.Substring(0, buildingFileName.Length - 9);

                byte[] data = File.ReadAllBytes(fileName);

                if (CacheFileNameDictionary.ContainsKey(buildingFileName)
                   && VerifyHash(data, CacheFileNameDictionary[buildingFileName].hash))
                {
                    // 画像は必要なときに読み込む
                    return;
                }
                else
                {
                    try
                    {
                        var scheme = TEditScheme.Read(data);
                        if (!string.IsNullOrEmpty(scheme.name))
                        {
                            BitmapImage image;
                            if (scheme.tiles.GetLength(0) * scheme.tiles.GetLength(1) > 10000)
                            {
                                image = TerraBuilder.Utils.WorldToImage.CreateMapImage(scheme.tiles);
                            }
                            else
                            {
                                image = TileToImage.CreateBitmap(scheme.tiles);
                            }

                            BuildingNameBitmapDictionary.Add(buildingFileName, image);
                            BuildingNameTilesDictionary.Add(buildingFileName, scheme.tiles);

                            BitmapEncoder encoder = new PngBitmapEncoder();
                            encoder.Frames.Add(BitmapFrame.Create(image));

                            string savePath = Path.Combine(CacheDirectory, buildingFileName + ".png");
                            if (!Directory.Exists(Path.GetDirectoryName(savePath)))
                            {
                                Directory.CreateDirectory(Path.GetDirectoryName(savePath));
                            }

                            using (var fileStream = new FileStream(savePath, FileMode.Create))
                            {
                                encoder.Save(fileStream);
                            }

                            // タグなしで登録とする
                            if (CacheFileNameDictionary.ContainsKey(buildingFileName))
                            {
                                CacheFileNameDictionary[buildingFileName] = (scheme.name, CalculateHash(data), new ObservableCollection<string>());
                            }
                            else
                            {
                                CacheFileNameDictionary.Add(buildingFileName, (scheme.name, CalculateHash(data), new ObservableCollection<string>()));
                            }
                        }
                    }
                    catch
                    {
                    }
                }
            }

            if (saveCacheDict)
            {
                // Json書き込み
                using (var sw = new StreamWriter(Path.Combine(CacheDirectory, "cache.json")))
                {
                    sw.Write(JsonConvert.SerializeObject(CacheFileNameDictionary));
                }
            }
        }

        public void ReloadDirectory()
        {
            BuildingNameBitmapDictionary.Clear();
            CacheFileNameDictionary.Clear();
            BuildingNameBuildDictionary.Clear();
            BuildingNameTilesDictionary.Clear();

            if (!Directory.Exists(CacheDirectory))
            {
                Directory.CreateDirectory(CacheDirectory);
            }

            if (TileTags.Count == 0)
            {
                LoadTileTags();
            }

            string cacheFilePath = Path.Combine(CacheDirectory, "cache.json");
            if (File.Exists(cacheFilePath))
            {
                using (var sr = new StreamReader(cacheFilePath))
                {
                    CacheFileNameDictionary = JsonConvert.DeserializeObject<Dictionary<string, (string name, string hash, ObservableCollection<string> tags)>>(sr.ReadToEnd());
                }

                if (CacheFileNameDictionary == null)
                {
                    CacheFileNameDictionary = new Dictionary<string, (string name, string hash, ObservableCollection<string> tags)>();
                }
            }

            // キャッシュファイルが辞書に存在しなければ削除
            foreach (string file in Directory.GetFiles(CacheDirectory, "*.png", SearchOption.AllDirectories))
            {
                var match = FileNameRegex.Match(file);
                if (!match.Success
                    || !CacheFileNameDictionary.ContainsKey(match.Groups[1].Value))
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch
                    {
                    }
                }
            }

            if (string.IsNullOrEmpty(BuildingDirectory))
            {
                // パス未設定、スキップ
                return;
            }

            if (!Directory.Exists(BuildingDirectory))
            {
                Directory.CreateDirectory(BuildingDirectory);
            }

            // json, TEditSchファイルを読みこみ
            foreach (string file in Directory.EnumerateFiles(BuildingDirectory, "*.*", SearchOption.AllDirectories)
                .Where(s => s.EndsWith(".json") || s.EndsWith(".TEditSch")))
            {
                ReloadFile(file);
            }

            if (CacheFileNameDictionary.Count != BuildingNameBitmapDictionary.Count)
            {
                // キャッシュ辞書に存在していたエントリが消えているため、削除
                var deletedNames = CacheFileNameDictionary.Keys.ToList();
                foreach (string name in BuildingNameBitmapDictionary.Keys)
                {
                    if (CacheFileNameDictionary.ContainsKey(name))
                    {
                        deletedNames.Remove(name);
                        continue;
                    }
                }

                // ファイルが存在しないか確認後、削除
                foreach (string name in deletedNames)
                {
                    string path = Path.Combine(CacheDirectory, name + ".png");
                    if (!File.Exists(path))
                    {
                        CacheFileNameDictionary.Remove(name);
                    }
                }
            }

            // Json書き込み
            using (var sw = new StreamWriter(cacheFilePath))
            {
                sw.Write(JsonConvert.SerializeObject(CacheFileNameDictionary));
            }
        }

        public void ClearCache()
        {
            BuildingNameBitmapDictionary.Clear();
            CacheFileNameDictionary.Clear();

            try
            {
                Directory.Delete(CacheDirectory);
            }
            catch
            {
                MessageBox.Show("キャッシュフォルダの削除に失敗しました.ファイルが開かれている可能性があります.");
            }
        }

        public Tile[,] GetTilesFromSearchResult(SearchResult from)
        {
            if (from == null)
            {
                return new Tile[0, 0];
            }

            string name = from.OriginalName;
            if (CacheFileNameDictionary.ContainsKey(name))
            {
                var build = GetBuildingFromBuildingName(name);
                if (build != null)
                {
                    BuildingGenerator.Root = build;
                    BuildingGenerator.Build();
                    return BuildingGenerator.Result == null ? new Tile[0, 0] : BuildingGenerator.Result;
                }
                else
                {
                    return GetSchemeTilesFromBuildingName(name);
                }
            }
            else if (TileTags.ContainsKey(name))
            {
                Random rand = WorldGeneration.WorldGenerationRunner.CurrentRunner.GlobalContext.Random;
                Tile[,] tiles = new Parts.TileObject() { ItemName = new ConstantValue(name.Substring(5)) }.Build(rand);
                if (tiles.GetLength(0) > 0)
                {
                    return tiles;
                }
                else
                {
                    if (name.StartsWith("Tile:"))
                    {
                        int tileId = TerrariaNameDict.ItemNameToItem[name.Substring(5)].createTile;
                        string tileTypeName = TerrariaNameDict.TileNameToID.First(p => p.Value == tileId).Key;
                        return new Parts.Rectangle() { FillTile = new ConstantValue(tileTypeName), Size = new Size() { Width = new ConstantValue(1), Height = new ConstantValue(1) } }.Build(rand);
                    }
                    else if (name.StartsWith("Wall:"))
                    {
                        int wallId = TerrariaNameDict.ItemNameToItem[name.Substring(5)].createWall;
                        string wallTypeName = TerrariaNameDict.WallNameToID.First(p => p.Value == wallId).Key;
                        return new Parts.Rectangle() { FillWall = new ConstantValue(wallTypeName), Size = new Size() { Width = new ConstantValue(1), Height = new ConstantValue(1) } }.Build(rand);
                    }

                    throw new ArgumentException("This could never happen.");
                }
            }
            else
            {
                throw new ArgumentException("This could never happen.");
            }
        }

        public async Task<List<SearchResult>> Search(string keywords)
        {
            List<SearchResult> results = new List<SearchResult>();

            // 検索が空なら
            string search = keywords.Trim().ToLowerInvariant();
            if (string.IsNullOrEmpty(search))
            {
                foreach (string name in CacheFileNameDictionary.Keys)
                {
                    // TEditSch形式なら編集可能
                    bool editable = !BuildingNameBuildDictionary.ContainsKey(name);
                    results.Add(new SearchResult()
                    {
                        Name = CacheFileNameDictionary[name].name,
                        OriginalName = name,
                        ImageGetFunction = () => GetBitmapFromBuildingName(name),
                        Tags = CacheFileNameDictionary[name].tags,
                        IsFavorite = Favorites.IsFavorite(name),
                        IsEditable = editable,
                    });
                }

                foreach (var tileTag in TileTags)
                {
                    results.Add(new SearchResult()
                    {
                        Name = tileTag.Value.name,
                        OriginalName = tileTag.Key,
                        ImageGetFunction = () => GetItemBitmapFromTile(tileTag.Key),
                        Tags = tileTag.Value.tags,
                        IsFavorite = Favorites.IsFavorite(tileTag.Key),
                    });
                }

                return new List<SearchResult>(results.OrderByDescending(x => x.IsFavorite));
            }

            // 検索が空以外なら、ファイル名、名前、タグから検索
            // 複数キーワードの場合、Linqで繰り返しフィルターしていく
            string[] searches = search.Split(' ', '　');
            if (searches.Length < 2)
            {
                return new List<SearchResult>(InitialSearch(keywords).OrderByDescending(x => x.IsFavorite));
            }

            IEnumerable<SearchResult> searchResults = InitialSearch(searches[0]);
            for (int i = 1; i < searches.Length; i++)
            {
                string keyword = searches[i];
                searchResults = searchResults.Where(x =>
                {
                    if (x.Name.ToLowerInvariant().Contains(keyword)
                        || x.OriginalName.ToLowerInvariant().Contains(keyword))
                    {
                        return true;
                    }

                    foreach (string tag in x.Tags)
                    {
                        if (string.Equals(tag, keyword, StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                    }

                    return false;
                });
            }

            return new List<SearchResult>(searchResults.OrderByDescending(x => x.IsFavorite));

            IEnumerable<SearchResult> InitialSearch(string keyword)
            {
                foreach (var cache in CacheFileNameDictionary)
                {
                    if (cache.Key.ToLowerInvariant().Contains(keyword)
                        || cache.Value.name.ToLowerInvariant().Contains(keyword))
                    {
                        // TEditSch形式なら編集可能
                        bool editable = !BuildingNameBuildDictionary.ContainsKey(cache.Key);
                        yield return new SearchResult()
                        {
                            Name = CacheFileNameDictionary[cache.Key].name,
                            OriginalName = cache.Key,
                            ImageGetFunction = () => GetBitmapFromBuildingName(cache.Key),
                            Tags = CacheFileNameDictionary[cache.Key].tags,
                            IsFavorite = Favorites.IsFavorite(cache.Key),
                            IsEditable = editable,
                        };
                    }
                    else
                    {
                        foreach (string tag in cache.Value.tags)
                        {
                            if (string.Equals(tag, keyword, StringComparison.OrdinalIgnoreCase))
                            {
                                // TEditSch形式なら編集可能
                                bool editable = !BuildingNameBuildDictionary.ContainsKey(cache.Key);
                                yield return new SearchResult()
                                {
                                    Name = CacheFileNameDictionary[cache.Key].name,
                                    OriginalName = cache.Key,
                                    ImageGetFunction = () => GetBitmapFromBuildingName(cache.Key),
                                    Tags = CacheFileNameDictionary[cache.Key].tags,
                                    IsFavorite = Favorites.IsFavorite(cache.Key),
                                    IsEditable = editable,
                                };
                                break;
                            }
                        }
                    }
                }

                foreach (var tileTag in TileTags)
                {
                    if (tileTag.Key.ToLowerInvariant().Contains(keyword)
                        || tileTag.Value.name.ToLowerInvariant().Contains(keyword))
                    {
                        yield return new SearchResult()
                        {
                            Name = tileTag.Value.name,
                            OriginalName = tileTag.Key,
                            ImageGetFunction = () => GetItemBitmapFromTile(tileTag.Key),
                            Tags = tileTag.Value.tags,
                            IsFavorite = Favorites.IsFavorite(tileTag.Key),
                        };
                    }
                    else
                    {
                        foreach (string tag in tileTag.Value.tags)
                        {
                            if (string.Equals(tag, keyword, StringComparison.OrdinalIgnoreCase))
                            {
                                yield return new SearchResult()
                                {
                                    Name = tileTag.Value.name,
                                    OriginalName = tileTag.Key,
                                    ImageGetFunction = () => GetItemBitmapFromTile(tileTag.Key),
                                    Tags = tileTag.Value.tags,
                                    IsFavorite = Favorites.IsFavorite(tileTag.Key),
                                };
                                break;
                            }
                        }
                    }
                }
            }
        }

        public void SaveFavorites()
        {
            using (var sw = new StreamWriter("favorites.json"))
            {
                sw.WriteLine(JsonConvert.SerializeObject(Favorites));
            }
        }

        private void LoadFavorites()
        {
            if (File.Exists("favorites.json"))
            {
                using (var sr = new StreamReader("favorites.json"))
                {
                    Favorites = JsonConvert.DeserializeObject<BuildingFavorites>(sr.ReadToEnd());
                }
            }
            else
            {
                Favorites = new BuildingFavorites();
            }
        }

        private string CalculateHash(string text)
        {
            return CalculateHash(Encoding.UTF8.GetBytes(text));
        }

        private string CalculateHash(byte[] value)
        {
            byte[] hashed = HashAlgorithm.ComputeHash(value);
            StringBuilder builder = new StringBuilder();

            for (int i = 0; i < hashed.Length; i++)
            {
                builder.Append(hashed[i].ToString("x2"));
            }

            return builder.ToString();
        }

        private bool VerifyHash(string text, string hash)
        {
            string hashedText = CalculateHash(text);
            StringComparer comparer = StringComparer.OrdinalIgnoreCase;

            return comparer.Compare(hashedText, hash) == 0;
        }

        private bool VerifyHash(byte[] value, string hash)
        {
            string hashedValue = CalculateHash(value);
            StringComparer comparer = StringComparer.OrdinalIgnoreCase;

            return comparer.Compare(hashedValue, hash) == 0;
        }

        private void LoadTileTags()
        {
            string tileTagCacheFile = Path.Combine(CacheDirectory, "tileTags.json");
            bool regenerateCacheFile = true;

            // タイルに対するタグ名をファイルから読み込み
            if (File.Exists(tileTagCacheFile))
            {
                using (var sr = new StreamReader(tileTagCacheFile))
                {
                    try
                    {
                        TileTags = JsonConvert.DeserializeObject<Dictionary<string, (string name, ObservableCollection<string> tags)>>(sr.ReadToEnd());
                        regenerateCacheFile = false;
                    }
                    catch
                    {
                        TileTags = null;
                    }
                }
            }

            // ファイルからの読み込みに失敗
            if (TileTags == null)
            {
                if (MessageBox.Show("タイルタグファイル(tileTag.json)の読み込みに失敗しました.初期状態に戻しますか？", "読み込みに失敗", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                {
                    regenerateCacheFile = false;
                }
            }

            // 再生成する
            if (regenerateCacheFile)
            {
                TileTags = new Dictionary<string, (string name, ObservableCollection<string> tags)>();
                Dictionary<int, object> tileIdsAlreadyAdd = new Dictionary<int, object>();
                Dictionary<int, object> wallIdsAlreadyAdd = new Dictionary<int, object>();

                // まずアイテム名からタイル、壁を取得.
                foreach (var item in TerrariaNameDict.ItemNameToItem)
                {
                    if (item.Value.createTile != -1)
                    {
                        string key = "Tile:" + item.Key;
                        if (!TileTags.ContainsKey(key))
                        {
                            TileTags.Add(key, (item.Value.Name, new ObservableCollection<string>() { "タイル", "たいる", "tile" }));
                        }

                        if (!tileIdsAlreadyAdd.ContainsKey(item.Value.createTile))
                        {
                            tileIdsAlreadyAdd.Add(item.Value.createTile, null);
                        }
                    }

                    if (item.Value.createWall != -1)
                    {
                        string key = "Wall:" + item.Key;
                        if (!TileTags.ContainsKey(key))
                        {
                            TileTags.Add(key, (item.Value.Name, new ObservableCollection<string>() { "wall", "壁", "かべ", "カベ" }));
                        }

                        if (!wallIdsAlreadyAdd.ContainsKey(item.Value.createWall))
                        {
                            wallIdsAlreadyAdd.Add(item.Value.createWall, null);
                        }
                    }
                }

                //for (int i = 0; i < TileID.Count; i++)
                //{
                //    if (tileIdsAlreadyAdd.ContainsKey(i))
                //    {
                //        continue;
                //    }

                //    string tileName = "Tile:" + TerrariaNameDict.TileNameToID.First(x => x.Value == i).Key;
                //    TileTags.Add(tileName, (tileName, new ObservableCollection<string>() { "タイル", "たいる", "tile" }));
                //}

                //for (int i = 0; i < WallID.Count; i++)
                //{
                //    if (wallIdsAlreadyAdd.ContainsKey(i))
                //    {
                //        continue;
                //    }

                //    string wallName = "Wall:" + TerrariaNameDict.WallNameToID.First(x => x.Value == i).Key;
                //    TileTags.Add(wallName, (wallName, new ObservableCollection<string>() { "wall", "壁", "かべ", "カベ" }));
                //}

                using (var sw = new StreamWriter(tileTagCacheFile))
                {
                    sw.WriteLine(JsonConvert.SerializeObject(TileTags, Formatting.Indented));
                }
            }
        }

        private BitmapImage GetBitmapFromBuildingName(string buildingName)
        {
            if (BuildingNameBitmapDictionary.ContainsKey(buildingName))
            {
                return BuildingNameBitmapDictionary[buildingName];
            }

            try
            {
                Bitmap image = (Bitmap)Image.FromFile(Path.Combine(CacheDirectory, buildingName + ".png"));
                BitmapImage bitmapImage = TerraBuilder.Utils.WorldToImage.Convert(image);
                BuildingNameBitmapDictionary.Add(buildingName, bitmapImage);
                return bitmapImage;
            }
            catch { }
            return null;
        }

        private BuildRoot GetBuildingFromBuildingName(string buildingName)
        {
            if (BuildingNameBuildDictionary.ContainsKey(buildingName))
            {
                return BuildingNameBuildDictionary[buildingName];
            }

            string filePath = string.IsNullOrEmpty(BuildingDirectory) ? buildingName + ".json" : Path.Combine(BuildingDirectory, buildingName + ".json");
            if (File.Exists(filePath))
            {
                try
                {
                    using (var sr = new StreamReader(filePath))
                    {
                        var build = JsonConvert.DeserializeObject<BuildRoot>(sr.ReadToEnd(), new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto });
                        if (!string.IsNullOrEmpty(build.Name))
                        {
                            BuildingNameBuildDictionary.Add(buildingName, build);
                            return build;
                        }
                    }
                }
                catch
                {
                    return null;
                }

                return null;
            }
            else
            {
                return null;
            }
        }

        private Tile[,] GetSchemeTilesFromBuildingName(string buildingName)
        {
            if (BuildingNameTilesDictionary.ContainsKey(buildingName))
            {
                return BuildingNameTilesDictionary[buildingName];
            }

            string filePath = string.IsNullOrEmpty(BuildingDirectory) ? buildingName + ".TEditSch" : Path.Combine(BuildingDirectory, buildingName + ".TEditSch");
            if (File.Exists(filePath))
            {
                var scheme = TEditScheme.Read(filePath);
                BuildingNameTilesDictionary.Add(buildingName, scheme.tiles);

                return new Tile[0, 0];
            }
            else
            {
                return new Tile[0, 0];
            }
        }

        private BitmapImage GetItemBitmapFromTile(string itemName)
        {
            lock (ItemNameBitmapDictionary)
            {
                if (ItemNameBitmapDictionary.ContainsKey(itemName))
                {
                    return ItemNameBitmapDictionary[itemName];
                }

                string key = itemName;
                if (key.StartsWith("Tile:"))
                {
                    key = key.Substring(5);
                }
                else if (key.StartsWith("Wall:"))
                {
                    key = key.Substring(5);
                }

                try
                {
                    BitmapImage image = TerraBuilder.Utils.WorldToImage.Convert(TextureLoader.Instance.GetItem(TerrariaNameDict.ItemNameToItem[key].type));
                    ItemNameBitmapDictionary.Add(itemName, image);
                    return image;
                }
                catch { }
                return null;
            }
        }
    }
}
