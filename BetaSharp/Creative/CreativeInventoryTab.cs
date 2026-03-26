using BetaSharp.Blocks;
using BetaSharp.Items;

namespace BetaSharp.Creative;

public sealed class CreativeInventoryTab
{
    public static readonly CreativeInventoryTab All = new("All", new ItemStack(Block.Bricks), 0, true, "/gui/creative_inv/list_items.png");
    public static readonly CreativeInventoryTab Blocks = new("Blocks", new ItemStack(Block.Stone), 1, true, "/gui/creative_inv/list_items.png");
    public static readonly CreativeInventoryTab Tools = new("Tools", new ItemStack(Item.IronPickaxe), 2, true, "/gui/creative_inv/list_items.png");
    public static readonly CreativeInventoryTab Combat = new("Combat", new ItemStack(Item.IronSword), 3, true, "/gui/creative_inv/list_items.png");
    public static readonly CreativeInventoryTab Food = new("Food", new ItemStack(Item.Apple), 4, true, "/gui/creative_inv/list_items.png");
    public static readonly CreativeInventoryTab Materials = new("Materials", new ItemStack(Item.Redstone), 0, false, "/gui/creative_inv/list_items.png");
    public static readonly CreativeInventoryTab Search = new("Search", new ItemStack(Item.Compass), 5, true, "/gui/creative_inv/search.png");
    public static readonly CreativeInventoryTab Inventory = new("Inventory", new ItemStack(Block.CraftingTable), 5, false, "/gui/creative_inv/survival_inv.png");

    public static readonly IReadOnlyList<CreativeInventoryTab> Tabs =
    [
        All,
        Blocks,
        Tools,
        Combat,
        Food,
        Materials,
        Search,
        Inventory
    ];

    private CreativeInventoryTab(string label, ItemStack icon, int column, bool isTopRow, string backgroundTexture)
    {
        Label = label;
        Icon = icon;
        Column = column;
        IsTopRow = isTopRow;
        BackgroundTexture = backgroundTexture;
    }

    public string Label { get; }
    public ItemStack Icon { get; }
    public int Column { get; }
    public bool IsTopRow { get; }
    public string BackgroundTexture { get; }
}
