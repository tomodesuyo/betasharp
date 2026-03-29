using BetaSharp;
using BetaSharp.Blocks;
using BetaSharp.Creative;
using BetaSharp.Entities;
using BetaSharp.Inventorys;
using BetaSharp.Items;
using BetaSharp.Potions;
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
    private static List<ItemStack>? s_cachedPotionVariants;

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

        if (CurrentTab == CreativeInventoryTab.Brewing || CurrentTab == CreativeInventoryTab.Search)
        {
            _visibleStacks.Sort(CompareVisibleStacks);
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
                AddSlot(new CreativeSelectionSlot(_selectionInventory, column + row * Columns, 9 + column * 18, 18 + row * 18));
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
                   || item == Item.FlintAndSteel
                   || item == Item.Compass
                   || item == Item.Clock;
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
            return IsFood(stack);
        }

        if (CurrentTab == CreativeInventoryTab.Brewing)
        {
            return IsBrewing(stack);
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
            var id when id == Block.Vine.id => true,
            var id when id == Block.Grass.id => true,
            var id when id == Block.DeadBush.id => true,
            var id when id == Block.Bookshelf.id => true,
            var id when id == Block.GlassPane.id => true,
            var id when id == Block.Torch.id => true,
            var id when id == Block.Ladder.id => true,
            var id when id == Block.Fence.id => true,
            var id when id == Block.CraftingTable.id => true,
            var id when id == Block.Furnace.id => true,
            var id when id == Block.Chest.id => true,
            var id when id == Block.Jukebox.id => true,
            var id when id == Block.Snow.id => true,
            var id when id == Block.Carpet.id => true,
            var id when id == Block.Cactus.id => true,
            var id when id == Block.LilyPad.id => true,
            var id when id == Block.Cocoa.id => true,
            var id when id == Block.EnchantmentTable.id => true,
            var id when id == Block.EndPortalFrame.id => true,
            var id when id == Block.Silverfish.id => true,
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
            var id when id == Block.FenceGate.id => true,
            var id when id == Block.RedstoneLamp.id => true,
            var id when id == Block.RedstoneBlock.id => true,
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

        return stack.itemId == Item.MonsterPlacer.id
               || stack.itemId == Item.Bucket.id
               || stack.itemId == Item.WaterBucket.id
               || stack.itemId == Item.LavaBucket.id
               || stack.itemId == Item.MilkBucket.id
               || stack.itemId == Item.Map.id
               || stack.itemId == Item.Book.id
               || stack.itemId == Item.Paper.id
               || stack.itemId == Item.Slimeball.id
               || stack.itemId == Item.Bone.id
               || stack.itemId == Item.EnderPearl.id
               || stack.itemId == Item.EyeOfEnder.id
               || stack.itemId == Item.FireCharge.id
               || stack.itemId == Item.ExpBottle.id
               || stack.itemId == Item.RecordThirteen.id
               || stack.itemId == Item.RecordCat.id
               || stack.itemId == Item.RecordBlocks.id
               || stack.itemId == Item.RecordChirp.id
               || stack.itemId == Item.RecordFar.id
               || stack.itemId == Item.RecordMall.id
               || stack.itemId == Item.RecordMellohi.id
               || stack.itemId == Item.RecordStal.id
               || stack.itemId == Item.RecordStrad.id
               || stack.itemId == Item.RecordWard.id
               || stack.itemId == Item.RecordEleven.id
               || stack.itemId == Item.Snowball.id;
    }

    private static bool IsFood(ItemStack stack)
    {
        return stack.itemId == Block.Cake.id || stack.getItem() is ItemFood;
    }

    private static bool IsBrewing(ItemStack stack)
    {
        return stack.itemId switch
        {
            var id when id == Block.BrewingStand.id => true,
            var id when id == Block.Cauldron.id => true,
            _ => stack.itemId == Item.Potion.id
                 || stack.itemId == Item.GlassBottle.id
                 || stack.itemId == Item.NetherWart.id
                 || stack.itemId == Item.SpiderEye.id
                 || stack.itemId == Item.GhastTear.id
                 || stack.itemId == Item.Redstone.id
                 || stack.itemId == Item.GlowstoneDust.id
                 || stack.itemId == Item.Sugar.id
                 || stack.itemId == Item.Gunpowder.id
                 || stack.itemId == Item.BlazePowder.id
                 || stack.itemId == Item.MagmaCream.id
                 || stack.itemId == Item.FermentedSpiderEye.id
                 || stack.itemId == Item.SpeckledMelon.id
                 || stack.itemId == Item.BrewingStand.id
                 || stack.itemId == Item.Cauldron.id
        };
    }

    private static bool IsMaterial(ItemStack stack)
    {
        if (stack.itemId < 256)
        {
            return false;
        }

        Item item = stack.getItem();
        if (stack.itemId == Item.Compass.id || stack.itemId == Item.Clock.id)
        {
            return false;
        }

        return item is not ItemFood
               && item is not ItemTool
               && item is not ItemHoe
               && item is not ItemSword
               && item is not ItemArmor
               && item is not ItemBow
               && item is not ItemFishingRod
               && item is not ItemShears
               && item != Item.FlintAndSteel
               && !IsBrewing(stack)
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
                if (item is ItemMonsterPlacer monsterPlacer)
                {
                    foreach (int entityId in monsterPlacer.GetSupportedEntityIds())
                {
                    _allStacks.Add(new ItemStack(item.id, 1, entityId));
                }
            }
                else if (item is ItemPotion potionItem)
                {
                    AddPotionVariants(potionItem);
                }
                else
                {
                    _allStacks.Add(new ItemStack(item));
                }
            }
        }

        AddMetadataVariants(Block.Wool, 16);
        AddMetadataVariants(Block.Slab, 6);
        AddMetadataVariants(Block.Planks, 4);
        AddMetadataVariants(Block.Log, 4);
        AddMetadataVariants(Block.Sapling, 4);
        AddMetadataVariants(Block.Leaves, 4);
        AddMetadataVariants(Block.StoneBrick, 4);
        AddMetadataVariants(Block.Silverfish, 3);
        AddMetadataVariants(Block.Carpet, 16);
        AddMetadataVariants(Block.Sponge, 2);
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

    private void AddPotionVariants(ItemPotion potionItem)
    {
        if (s_cachedPotionVariants == null)
        {
            Dictionary<string, int> representativePotionData = new(StringComparer.Ordinal);
            for (int potionDamage = 0; potionDamage <= short.MaxValue; ++potionDamage)
            {
                List<PotionEffect>? effects = potionItem.GetEffects(potionDamage);
                if (effects == null || effects.Count == 0)
                {
                    continue;
                }

                string signature = GetPotionVariantSignature(effects, ItemPotion.IsSplash(potionDamage));
                representativePotionData.TryAdd(signature, potionDamage);
            }

            s_cachedPotionVariants = [new ItemStack(Item.Potion, 1, 0)];
            foreach (int potionDamage in representativePotionData.Values.OrderBy(value => value))
            {
                s_cachedPotionVariants.Add(new ItemStack(Item.Potion, 1, potionDamage));
            }
        }

        for (int i = 0; i < s_cachedPotionVariants.Count; ++i)
        {
            _allStacks.Add(s_cachedPotionVariants[i].copy());
        }
    }

    private static string GetPotionVariantSignature(List<PotionEffect> effects, bool splash)
    {
        IEnumerable<PotionEffect> orderedEffects = effects
            .OrderBy(effect => effect.PotionId)
            .ThenBy(effect => effect.Amplifier)
            .ThenBy(effect => effect.Duration);
        return (splash ? "splash|" : "drink|") + string.Join(";", orderedEffects.Select(effect => $"{effect.PotionId}:{effect.Amplifier}:{effect.Duration}"));
    }

    private static int CompareVisibleStacks(ItemStack a, ItemStack b)
    {
        int compare = string.Compare(GetLocalizedStackName(a), GetLocalizedStackName(b), StringComparison.OrdinalIgnoreCase);
        if (compare != 0)
        {
            return compare;
        }

        compare = a.itemId.CompareTo(b.itemId);
        return compare != 0 ? compare : a.getDamage().CompareTo(b.getDamage());
    }

    private static string GetLocalizedStackName(ItemStack stack)
    {
        string translated = TranslationStorage.Instance.TranslateNamedKey(stack.getItemName()).Trim();
        return translated.Length > 0 ? translated : stack.getItemName();
    }
}
