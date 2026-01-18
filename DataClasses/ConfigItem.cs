using System.Collections.Generic;

namespace barcode_gen
{
    public class ConfigItem
    {
        public string Name { get; set; }
        public List<BlockItem> Blocks { get; set; } = new List<BlockItem>();

    }
}
