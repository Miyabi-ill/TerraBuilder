namespace TUBGWorldGenerator.BuildingGenerator
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
    using TUBGWorldGenerator.BuildingGenerator.UI;

    public class BuildingCache
    {
        private string cacheDirectory = "Cache";

        public BuildingCache(BuildingGenerator buildingGenerator)
        {
            BuildingGenerator = buildingGenerator;
            ReloadDirectory();
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

        private BuildingGenerator BuildingGenerator { get; }

        private HashAlgorithm HashAlgorithm { get; set; } = SHA256.Create();

        //private Dictionary<string, BuildRoot> BuildingNameBuildDictionary { get; } = new Dictionary<string, BuildRoot>();

        private Dictionary<string, BitmapImage> BuildingNameBitmapDictionary { get; } = new Dictionary<string, BitmapImage>();

        /// <summary>
        /// キャッシュディレクトリの辞書。ファイル名(の拡張子を除いた名前)に対し、建物名とハッシュを保存する。Jsonに書き込み/読み込みする。
        /// </summary>
        private Dictionary<string, (string name, string hash, ObservableCollection<string> tags)> CacheFileNameDictionary { get; set; } = new Dictionary<string, (string name, string hash, ObservableCollection<string> tags)>();

        private Regex FileNameRegex { get; set; } = new Regex(@"Cache\\(.*)\.png");

        public void ReloadDirectory()
        {
            BuildingNameBitmapDictionary.Clear();
            CacheFileNameDictionary.Clear();

            if (!Directory.Exists(CacheDirectory))
            {
                Directory.CreateDirectory(CacheDirectory);
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

            if (!Directory.Exists(BuildingGenerator.BuildingsRootPath))
            {
                Directory.CreateDirectory(BuildingGenerator.BuildingsRootPath);
            }

            // まず建築物のjsonを確認し、ハッシュを確認。
            // ハッシュ不一致またはファイルが辞書に存在しなければ、新規作成
            foreach (string file in Directory.GetFiles(BuildingGenerator.BuildingsRootPath, "*.json", SearchOption.AllDirectories))
            {
                // 親ディレクトリの名前と拡張子を省く
                string buildingFileName = file.Replace(BuildingGenerator.BuildingsRootPath, string.Empty).Trim('\\');
                buildingFileName = buildingFileName.Substring(0, buildingFileName.Length - 5);

                string text;
                using (StreamReader sr = new StreamReader(file))
                {
                    text = sr.ReadToEnd();
                }

                // ファイル名がある かつ ハッシュ一致で画像取得
                if (CacheFileNameDictionary.ContainsKey(buildingFileName)
                    && VerifyHash(text, CacheFileNameDictionary[buildingFileName].hash))
                {
                    Bitmap image = (Bitmap)Image.FromFile(Path.Combine(CacheDirectory, buildingFileName + ".png"));

                    BuildingNameBitmapDictionary.Add(buildingFileName, Utils.WorldToImage.Convert(image));
                    continue;
                }

                // それ以外は新規作成
                else
                {
                    try
                    {
                        var build = JsonConvert.DeserializeObject<BuildRoot>(text, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto });
                        if (!string.IsNullOrEmpty(build.Name))
                        {
                            BitmapImage image = TileToImage.CreateBitmap(build.Build());
                            BuildingNameBitmapDictionary.Add(buildingFileName, image);

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
                    catch (Exception e)
                    {
                    }
                }
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

                foreach (string name in deletedNames)
                {
                    CacheFileNameDictionary.Remove(name);
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
                MessageBox.Show("キャッシュフォルダの削除に失敗しました。ファイルが開かれている可能性があります。");
            }
        }

        public IEnumerable<SearchResult> Search(string keywords)
        {
            // 検索が空なら
            string search = keywords.Trim().ToLowerInvariant();
            if (string.IsNullOrEmpty(search))
            {
                foreach (string name in CacheFileNameDictionary.Keys)
                {
                    yield return new SearchResult() { Name = CacheFileNameDictionary[name].name, OriginalName = name, Image = BuildingNameBitmapDictionary[name], Tags = CacheFileNameDictionary[name].tags };
                }

                yield break;
            }

            // 検索が空以外なら、ファイル名、名前、タグから検索
            // 複数キーワードの場合、Linqで繰り返しフィルターしていく
            string[] searches = search.Split(' ', '　');
            if (searches.Length < 2)
            {
                foreach (var result in InitialSearch(search))
                {
                    yield return result;
                }

                yield break;
            }

            IEnumerable<SearchResult> results = InitialSearch(searches[0]);
            for (int i = 1; i < searches.Length; i++)
            {
                string keyword = searches[i];
                results = results.Where(x =>
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

            foreach (var result in results)
            {
                yield return result;
            }

            IEnumerable<SearchResult> InitialSearch(string keyword)
            {
                foreach (var cache in CacheFileNameDictionary)
                {
                    if (cache.Key.ToLowerInvariant().Contains(keyword)
                        || cache.Value.name.ToLowerInvariant().Contains(keyword))
                    {
                        yield return new SearchResult() { Name = CacheFileNameDictionary[cache.Key].name, OriginalName = cache.Key, Image = BuildingNameBitmapDictionary[cache.Key], Tags = CacheFileNameDictionary[cache.Key].tags };
                    }
                    else
                    {
                        foreach (string tag in cache.Value.tags)
                        {
                            if (string.Equals(tag, keyword, StringComparison.OrdinalIgnoreCase))
                            {
                                yield return new SearchResult() { Name = CacheFileNameDictionary[cache.Key].name, OriginalName = cache.Key, Image = BuildingNameBitmapDictionary[cache.Key], Tags = CacheFileNameDictionary[cache.Key].tags };
                                break;
                            }
                        }
                    }
                }
            }
        }

        private string CalculateHash(string text)
        {
            byte[] hashed = HashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(text));
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
    }
}
