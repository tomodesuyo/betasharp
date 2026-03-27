using BetaSharp.Blocks;
using BetaSharp.Creative;
using BetaSharp.Entities;
using BetaSharp.Inventorys;
using BetaSharp.Items;
using BetaSharp.Screens.Slots;

namespace BetaSharp.Screens;

public class CreativeScreenHandler : ScreenHandler
{
    private const int Columns = 9;
    private const int VisibleRows = 5;
    private const int SelectionSlotCount = Columns * VisibleRows;
    private const int TrashSlotId = 45;

    private readonly InventoryBasic _selectionInventory = new("creative", SelectionSlotCount);
    private readonly InventoryBasic _trashInventory = new("creativeTrash", 1);
    private readonly List<ItemStack> _allStacks = [];
    private readonly List<ItemStack> _visibleStacks = [];
    private readonly InventoryPlayer _playerInventory;

    private string _searchQuery = string.Empty;

    public CreativeScreenHandler(InventoryPlayer inventoryPlayer)
    {
        _playerInventory = inventoryPlayer;
        PopulateAllStacks();

        SetTab(CreativeInventoryTab.BuildingBlocks);
    }

    public CreativeInventoryTab CurrentTab { get; private set; } = CreativeInventoryTab.BuildingBlocks;

    public override bool canUse(EntityPlayer player)
    {
        return true;
    }

    public override ItemStack? quickMove(int index)
    {
        if (CurrentTab == CreativeInventoryTab.Inventory)
        {
            if (index == TrashSlotId)
            {
                return null;
            }

            if (_playerInventory.player.playerScreenHandler != null && index >= 0 && index < TrashSlotId)
            {
                return _playerInventory.player.playerScreenHandler.quickMove(index);
            }
        }

        return null;
    }

    public void SetTab(CreativeInventoryTab tab)
    {
        CurrentTab = tab;
        if (tab != CreativeInventoryTab.Search)
        {
            _searchQuery = string.Empty;
        }

        RebuildSlots();
        RebuildVisibleStacks();
    }

    public void SetSearchQuery(string query)
    {
        _searchQuery = query.Trim();
        RebuildVisibleStacks();
    }

    public void ScrollTo(float position)
    {
        int rowCount = GetScrollableRowCount();
        int startRow = (int)(position * rowCount + 0.5D);
        if (startRow < 0)
        {
            startRow = 0;
        }

        for (int row = 0; row < VisibleRows; ++row)
        {
            for (int column = 0; column < Columns; ++column)
            {
                int itemIndex = column + (row + startRow) * Columns;
                _selectionInventory.setStack(column + row * Columns, itemIndex >= 0 && itemIndex < _visibleStacks.Count ? _visibleStacks[itemIndex].copy() : null);
            }
        }
    }

    public int GetScrollableRowCount()
    {
        return Math.Max(0, (_visibleStacks.Count + Columns - 1) / Columns - VisibleRows);
    }

    public bool NeedsScrollBar()
    {
        return CurrentTab.HasScrollbar && GetScrollableRowCount() > 0;
    }

    private void RebuildVisibleStacks()
    {
        if (CurrentTab == CreativeInventoryTab.Inventory)
        {
            return;
        }

        _visibleStacks.Clear();
        foreach (ItemStack stack in _allStacks)
        {
            if (!MatchesCurrentTab(stack))
            {
                continue;
            }

            if (CurrentTab == CreativeInventoryTab.Search && !MatchesSearch(stack))
            {
                continue;
            }

            _visibleStacks.Add(stack);
        }

        ScrollTo(0.0F);
    }

    private void RebuildSlots()
    {
        Slots.Clear();
        TrackedStacks.Clear();

        if (CurrentTab == CreativeInventoryTab.Inventory)
        {
            AddInventorySlots();
            return;
        }

        for (int row = 0; row < VisibleRows; ++row)
        {
            for (int column = 0; column < Columns; ++column)
            {
                AddSlot(new Slot(_selectionInventory, column + row * Columns, 9 + column * 18, 18 + row * 18));
            }
        }

        for (int hotbarSlot = 0; hotbarSlot < 9; ++hotbarSlot)
        {
            AddSlot(new Slot(_playerInventory, hotbarSlot, 9 + hotbarSlot * 18, 112));
        }
    }

    private void AddInventorySlots()
    {
        PlayerScreenHandler playerScreenHandler = (PlayerScreenHandler)_playerInventory.player.playerScreenHandler;

        AddSlot(new CraftingResultSlot(_playerInventory.player, playerScreenHandler.craftingInput, playerScreenHandler.craftingResult, 0, -2000, -2000));

        for (int row = 0; row < 2; ++row)
        {
            for (int column = 0; column < 2; ++column)
            {
                AddSlot(new Slot(playerScreenHandler.craftingInput, column + row * 2, -2000, -2000));
            }
        }

        for (int armorSlot = 0; armorSlot < 4; ++armorSlot)
        {
            int column = armorSlot / 2;
            int row = armorSlot % 2;
            AddSlot(new SlotArmor(playerScreenHandler, _playerInventory, _playerInventory.size() - 1 - armorSlot, 9 + column * 54, 6 + row * 27, armorSlot));
        }

        for (int slotIndex = 9; slotIndex < 36; ++slotIndex)
        {
            int slot = slotIndex - 9;
            int column = slot % 9;
            int row = slot / 9;
            AddSlot(new Slot(_playerInventory, slotIndex, 9 + column * 18, 54 + row * 18));
        }

        for (int hotbarSlot = 0; hotbarSlot < 9; ++hotbarSlot)
        {
            AddSlot(new Slot(_playerInventory, hotbarSlot, 9 + hotbarSlot * 18, 112));
        }

        AddSlot(new Slot(_trashInventory, 0, 173, 112));
    }

    private bool MatchesCurrentTab(ItemStack stack)
    {
        if (CurrentTab == CreativeInventoryTab.Search)
        {
            return true;
        }

        if (CurrentTab == CreativeInventoryTab.BuildingBlocks)
        {
            return IsBuildingBlock(stack);
        }

        if (CurrentTab == CreativeInventoryTab.Decorations)
        {
            return IsDecoration(stack);
        }

        if (CurrentTab == CreativeInventoryTab.Redstone)
        {
            return IsRedstone(stack);
        }

        if (CurrentTab == CreativeInventoryTab.Transportation)
        {
            return IsTransportation(stack);
        }

        if (CurrentTab == CreativeInventoryTab.Misc)
        {
            return IsMisc(stack);
        }

        Item item = stack.getItem();
        if (CurrentTab == CreativeInventoryTab.Tools)
        {
            return item is ItemTool
                   || item is ItemHoe
                   || item is ItemFishingRod
                   || item is ItemShears
                   || item == Item.FlintAndSteel;
        }

        if (CurrentTab == CreativeInventoryTab.Combat)
        {
            return item is ItemSword
                   || item is ItemArmor
                   || item is ItemBow
                   || item == Item.ARROW;
        }

        if (CurrentTab == CreativeInventoryTab.Food)
        {
            return item is ItemFood;
        }

        if (CurrentTab == CreativeInventoryTab.Brewing)
        {
            return item == Item.GlowstoneDust;
        }

        if (CurrentTab == CreativeInventoryTab.Materials)
        {
            return IsMaterial(stack);
        }

        return false;
    }

    private static bool IsBuildingBlock(ItemStack stack)
    {
        if (stack.itemId >= 256)
        {
            return false;
        }

        return !IsDecoration(stack) && !IsRedstone(stack) && !IsTransportation(stack);
    }

    private static bool IsDecoration(ItemStack stack)
    {
        return stack.itemId switch
        {
            var id when id == Block.Rose.id => true,
            var id when id == Block.Dandelion.id => true,
            var id when id == Block.Sapling.id => true,
            var id when id == Block.Leaves.id => true,
            var id when id == Block.Glass.id => true,
            var id when id == Block.Torch.id => true,
            var id when id == Block.Ladder.id => true,
            var id when id == Block.CraftingTable.id => true,
            var id when id == Block.Furnace.id => true,
            var id when id == Block.Chest.id => true,
            var id when id == Block.Jukebox.id => true,
            var id when id == Block.Snow.id => true,
            var id when id == Block.Cactus.id => true,
            var id when id == Block.Pumpkin.id => true,
            var id when id == Block.JackLantern.id => true,
            _ => stack.itemId == Item.Sign.id
                 || stack.itemId == Item.Painting.id
                 || stack.itemId == Item.Bed.id
        };
    }

    private static bool IsRedstone(ItemStack stack)
    {
        return stack.itemId switch
        {
            var id when id == Block.Dispenser.id => true,
            var id when id == Block.Noteblock.id => true,
            var id when id == Block.PoweredRail.id => true,
            var id when id == Block.DetectorRail.id => true,
            var id when id == Block.StickyPiston.id => true,
            var id when id == Block.Piston.id => true,
            var id when id == Block.TNT.id => true,
            var id when id == Block.Lever.id => true,
            var id when id == Block.StonePressurePlate.id => true,
            var id when id == Block.WoodenPressurePlate.id => true,
            var id when id == Block.RedstoneOre.id => true,
            var id when id == Block.RedstoneTorch.id => true,
            var id when id == Block.Button.id => true,
            var id when id == Block.Repeater.id => true,
            var id when id == Block.Trapdoor.id => true,
            _ => stack.itemId == Item.Redstone.id
                 || stack.itemId == Item.Repeater.id
                 || stack.itemId == Item.WoodenDoor.id
                 || stack.itemId == Item.IronDoor.id
        };
    }

    private static bool IsTransportation(ItemStack stack)
    {
        return stack.itemId switch
        {
            var id when id == Block.Rail.id => true,
            var id when id == Block.PoweredRail.id => true,
            var id when id == Block.DetectorRail.id => true,
            _ => stack.itemId == Item.Minecart.id
                 || stack.itemId == Item.ChestMinecart.id
                 || stack.itemId == Item.FurnaceMinecart.id
                 || stack.itemId == Item.Boat.id
                 || stack.itemId == Item.Saddle.id
        };
    }

    private static bool IsMisc(ItemStack stack)
    {
        if (stack.itemId < 256)
        {
            return false;
        }

        return stack.itemId == Item.Bucket.id
               || stack.itemId == Item.WaterBucket.id
               || stack.itemId == Item.LavaBucket.id
               || stack.itemId == Item.MilkBucket.id
               || stack.itemId == Item.Compass.id
               || stack.itemId == Item.Clock.id
               || stack.itemId == Item.Map.id
               || stack.itemId == Item.RecordThirteen.id
               || stack.itemId == Item.RecordCat.id
               || stack.itemId == Item.Snowball.id;
    }

    private static bool IsMaterial(ItemStack stack)
    {
        if (stack.itemId < 256)
        {
            return false;
        }

        Item item = stack.getItem();
        return item is not ItemFood
               && item is not ItemTool
               && item is not ItemHoe
               && item is not ItemSword
               && item is not ItemArmor
               && item is not ItemBow
               && item is not ItemFishingRod
               && item is not ItemShears
               && item != Item.FlintAndSteel
               && item != Item.GlowstoneDust
               && !IsTransportation(stack)
               && !IsRedstone(stack)
               && !IsDecoration(stack)
               && !IsMisc(stack);
    }

    private bool MatchesSearch(ItemStack stack)
    {
        if (string.IsNullOrWhiteSpace(_searchQuery))
        {
            return true;
        }

        string itemName = stack.getItemName() ?? string.Empty;
        string translatedName = TranslationStorage.Instance.TranslateNamedKey(itemName) ?? string.Empty;
        string fallbackName = itemName;
        return translatedName.Contains(_searchQuery, StringComparison.OrdinalIgnoreCase)
               || fallbackName.Contains(_searchQuery, StringComparison.OrdinalIgnoreCase);
    }

    private void PopulateAllStacks()
    {
        for (int blockId = 1; blockId < Block.Blocks.Length; ++blockId)
        {
            Block? block = Block.Blocks[blockId];
            if (block != null)
            {
                _allStacks.Add(new ItemStack(block));
            }
        }

        for (int itemId = 256; itemId < Item.ITEMS.Length; ++itemId)
        {
            Item? item = Item.ITEMS[itemId];
            if (item != null)
            {
                _allStacks.Add(new ItemStack(item));
            }
        }

        AddMetadataVariants(Block.Wool, 16);
        AddMetadataVariants(Block.Slab, 6);
        AddMetadataVariants(Block.Planks, 3);
        AddMetadataVariants(Block.Log, 3);
        AddMetadataVariants(Block.Sapling, 3);
        AddMetadataVariants(Block.Leaves, 3);
        AddMetadataVariants(Item.Dye, 16, 1);
    }

    private void AddMetadataVariants(Block block, int count, int start = 1)
    {
        for (int meta = start; meta < count; ++meta)
        {
            _allStacks.Add(new ItemStack(block, 1, meta));
        }
    }

    private void AddMetadataVariants(Item item, int count, int start = 1)
    {
        for (int meta = start; meta < count; ++meta)
        {
            _allStacks.Add(new ItemStack(item.id, 1, meta));
        }
    }
}
