namespace TerraBuilder.Chests
{
    using System.ComponentModel;
    using Newtonsoft.Json;

    public class ItemContext : ActionContext, IEditableObject
    {
        private string previousName;
        private int previousPrefixID;
        private int previousItemID;

        [JsonIgnore]
        public string Name { get; set; }

        public int PrefixID { get; set; }

        public int ItemID { get; set; }

        public void BeginEdit()
        {
            previousName = Name;
            previousPrefixID = PrefixID;
            previousItemID = ItemID;
        }

        public void CancelEdit()
        {
            Name = previousName;
            PrefixID = previousPrefixID;
            ItemID = previousItemID;

            previousName = null;
            previousPrefixID = 0;
            previousItemID = 0;
        }

        public void EndEdit()
        {
            if (string.IsNullOrEmpty(Name) || (Configs.Items.ContainsKey(Name) && Configs.Items[Name] != this))
            {
                CancelEdit();
                return;
            }

            if (!string.IsNullOrEmpty(previousName) && Configs.Items.ContainsKey(previousName))
            {
                Configs.Items.Remove(previousName);
                Configs.Items.Add(Name, this);
            }

            previousName = null;
            previousPrefixID = 0;
            previousItemID = 0;
        }
    }
}
