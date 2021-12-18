namespace TerraBuilder.BuildingGenerator.Parts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class PartsConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(BuildBase).IsAssignableFrom(objectType);
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override object ReadJson(
            JsonReader reader,
            Type objectType,
            object existingValue,
            JsonSerializer serializer)
        {
            // JObjectをJsonからロード
            JObject jObject = JObject.Load(reader);

            string typeName = (string)jObject["TypeName"];
            BuildBase target;
            switch (typeName)
            {
                case nameof(Box):
                    target = new Box();
                    break;
                case nameof(BuildParent):
                    target = new BuildParent();
                    break;
                case nameof(BuildRoot):
                    target = new BuildRoot();
                    break;
                case nameof(Import):
                    target = new Import();
                    break;
                case nameof(Line):
                    target = new Line();
                    break;
                case nameof(Rectangle):
                    target = new Rectangle();
                    break;
                case nameof(Repeat):
                    target = new Repeat();
                    break;
                case nameof(TileObject):
                    target = new TileObject();
                    break;
                default:
                    throw new JsonReaderException($"TypeName `{typeName}` does not implemented.");
            }

            // Populate the object properties
            serializer.Populate(jObject.CreateReader(), target);

            return target;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
