using BetaSharp.Blocks;
using BetaSharp.Items;

namespace BetaSharp.Creative;

public sealed class CreativeInventoryTab
{
    public static readonly CreativeInventoryTab BuildingBlocks = new("Building Blocks", new ItemStack(Block.Bricks), 0, true, "/gui/creative_inv/list_items.png");
    public static readonly CreativeInventoryTab Decorations = new("Decorations", new ItemStack(Block.Rose), 1, true, "/gui/creative_inv/list_items.png");
    public static readonly CreativeInventoryTab Redstone = new("Redstone", new ItemStack(Item.Redstone), 2, true, "/gui/creative_inv/list_items.png");
    public static readonly CreativeInventoryTab Transportation = new("Transportation", new ItemStack(Block.PoweredRail), 3, true, "/gui/creative_inv/list_items.png");
    public static readonly CreativeInventoryTab Misc = new("Misc", new ItemStack(Item.LavaBucket), 4, true, "/gui/creative_inv/list_items.png");
    public static readonly CreativeInventoryTab Search = new("Search", new ItemStack(Item.Compass), 5, true, "/gui/creative_inv/search.png");
    public static readonly CreativeInventoryTab Food = new("Food", new ItemStack(Item.Apple), 0, false, "/gui/creative_inv/list_items.png");
    public static readonly CreativeInventoryTab Tools = new("Tools", new ItemStack(Item.IronAxe), 1, false, "/gui/creative_inv/list_items.png");
    public static readonly CreativeInventoryTab Combat = new("Combat", new ItemStack(Item.GoldenSword), 2, false, "/gui/creative_inv/list_items.png");
    public static readonly CreativeInventoryTab Brewing = new("Brewing", new ItemStack(Item.GlowstoneDust), 3, false, "/gui/creative_inv/list_items.png");
    public static readonly CreativeInventoryTab Materials = new("Materials", new ItemStack(Item.Stick), 4, false, "/gui/creative_inv/list_items.png");
    public static readonly CreativeInventoryTab Inventory = new("Inventory", new ItemStack(Block.Chest), 5, false, "/gui/creative_inv/survival_inv.png", hasScrollbar: false, drawTitle: false);

    public static readonly IReadOnlyList<CreativeInventoryTab> Tabs =
    [
        BuildingBlocks,
        Decorations,
        Redstone,
        Transportation,
        Misc,
        Search,
        Food,
        Tools,
        Combat,
        Brewing,
        Materials,
        Inventory
    ];

    private CreativeInventoryTab(string label, ItemStack icon, int column, bool isTopRow, string backgroundTexture, bool hasScrollbar = true, bool drawTitle = true)
    {
        Label = label;
        Icon = icon;
        Column = column;
        IsTopRow = isTopRow;
        BackgroundTexture = backgroundTexture;
        HasScrollbar = hasScrollbar;
        DrawTitle = drawTitle;
    }

    public string Label { get; }
    public ItemStack Icon { get; }
    public int Column { get; }
    public bool IsTopRow { get; }
    public string BackgroundTexture { get; }
    public bool HasScrollbar { get; }
    public bool DrawTitle { get; }
}
