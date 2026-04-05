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
        CreativeInventoryTab? tab = GetCreativeTab(stack);
        if (CurrentTab == CreativeInventoryTab.Search)
        {
            return tab != null;
        }

        return tab == CurrentTab;
    }

    private static CreativeInventoryTab? GetCreativeTab(ItemStack stack)
    {
        return stack.itemId < 256 ? GetBlockCreativeTab(stack.itemId) : GetItemCreativeTab(stack);
    }

    private static CreativeInventoryTab? GetBlockCreativeTab(int itemId)
    {
        if (IsHiddenBlock(itemId))
        {
            return null;
        }

        if (IsTransportationBlock(itemId))
        {
            return CreativeInventoryTab.Transportation;
        }

        if (IsRedstoneBlock(itemId))
        {
            return CreativeInventoryTab.Redstone;
        }

        if (IsDecorationBlock(itemId))
        {
            return CreativeInventoryTab.Decorations;
        }

        return CreativeInventoryTab.BuildingBlocks;
    }

    private static CreativeInventoryTab GetItemCreativeTab(ItemStack stack)
    {
        Item item = stack.getItem();

        if (IsDecorationItem(stack.itemId))
        {
            return CreativeInventoryTab.Decorations;
        }

        if (IsRedstoneItem(stack.itemId))
        {
            return CreativeInventoryTab.Redstone;
        }

        if (IsTransportationItem(stack.itemId))
        {
            return CreativeInventoryTab.Transportation;
        }

        if (IsMiscItem(stack.itemId))
        {
            return CreativeInventoryTab.Misc;
        }

        if (IsBrewingItem(stack.itemId))
        {
            return CreativeInventoryTab.Brewing;
        }

        if (item == Item.Cake || item is ItemFood)
        {
            return CreativeInventoryTab.Food;
        }

        if (item is ItemTool
            || item is ItemHoe
            || item is ItemFishingRod
            || item is ItemShears
            || item == Item.FlintAndSteel
            || item == Item.Lead
            || item == Item.NameTag
            || item == Item.Compass
            || item == Item.Clock)
        {
            return CreativeInventoryTab.Tools;
        }

        if (item is ItemSword || item is ItemArmor || item is ItemBow || item == Item.ARROW)
        {
            return CreativeInventoryTab.Combat;
        }

        if (item == Item.Elytra || item == Item.TotemOfUndying)
        {
            return CreativeInventoryTab.Combat;
        }

        return CreativeInventoryTab.Materials;
    }

    private static bool IsHiddenBlock(int itemId)
    {
        return itemId == Block.FlowingWater.id
               || itemId == Block.Water.id
               || itemId == Block.FlowingLava.id
               || itemId == Block.Lava.id
               || itemId == Block.PistonHead.id
               || itemId == Block.MovingPiston.id
               || itemId == Block.Fire.id
               || itemId == Block.Spawner.id
               || itemId == Block.Wheat.id
               || itemId == Block.Farmland.id
               || itemId == Block.LitFurnace.id
               || itemId == Block.Sign.id
               || itemId == Block.Door.id
               || itemId == Block.WallSign.id
               || itemId == Block.IronDoor.id
               || itemId == Block.RedstoneWire.id
               || itemId == Block.LitRedstoneOre.id
               || itemId == Block.RedstoneTorch.id
               || itemId == Block.Repeater.id
               || itemId == Block.PoweredRepeater.id
               || itemId == Block.LockedChest.id
               || itemId == Block.SugarCane.id
               || itemId == Block.NetherPortal.id
               || itemId == Block.Cake.id
               || itemId == Block.BrewingStand.id
               || itemId == Block.Cauldron.id
               || itemId == Block.EndPortal.id
               || itemId == Block.PumpkinStem.id
               || itemId == Block.MelonStem.id
               || itemId == Block.Cocoa.id
               || itemId == Block.Carrot.id
               || itemId == Block.Potato.id
               || itemId == Block.Skull.id
               || itemId == Block.LitRedstoneLamp.id
               || itemId == Block.NetherWart.id;
    }

    private static bool IsDecorationBlock(int itemId)
    {
        return itemId == Block.Sapling.id
               || itemId == Block.Dandelion.id
               || itemId == Block.Rose.id
               || itemId == Block.BrownMushroom.id
               || itemId == Block.RedMushroom.id
               || itemId == Block.Torch.id
               || itemId == Block.Ladder.id
               || itemId == Block.Chest.id
               || itemId == Block.CraftingTable.id
               || itemId == Block.Furnace.id
               || itemId == Block.Snow.id
               || itemId == Block.Cactus.id
               || itemId == Block.Jukebox.id
               || itemId == Block.Fence.id
               || itemId == Block.Leaves.id
               || itemId == Block.Vine.id
               || itemId == Block.GlassPane.id
               || itemId == Block.IronBars.id
               || itemId == Block.LilyPad.id
               || itemId == Block.NetherFence.id
               || itemId == Block.EnchantmentTable.id
               || itemId == Block.EndPortalFrame.id
               || itemId == Block.Silverfish.id
               || itemId == Block.Wall.id
               || itemId == Block.Barrier.id
               || itemId == Block.SlimeBlock.id
               || itemId == Block.Carpet.id
               || itemId == Block.DeadBush.id;
    }

    private static bool IsRedstoneBlock(int itemId)
    {
        return itemId == Block.Dispenser.id
               || itemId == Block.Noteblock.id
               || itemId == Block.StickyPiston.id
               || itemId == Block.Piston.id
               || itemId == Block.TNT.id
               || itemId == Block.Lever.id
               || itemId == Block.StonePressurePlate.id
               || itemId == Block.WoodenPressurePlate.id
               || itemId == Block.LitRedstoneTorch.id
               || itemId == Block.Button.id
               || itemId == Block.Trapdoor.id
               || itemId == Block.FenceGate.id
               || itemId == Block.RedstoneLamp.id
               || itemId == Block.RedstoneBlock.id;
    }

    private static bool IsTransportationBlock(int itemId)
    {
        return itemId == Block.Rail.id
               || itemId == Block.PoweredRail.id
               || itemId == Block.DetectorRail.id
               || itemId == Block.ActivatorRail.id;
    }

    private static bool IsDecorationItem(int itemId)
    {
        return itemId == Item.Painting.id
               || itemId == Item.Sign.id
               || itemId == Item.Bed.id
               || itemId == Item.ArmorStand.id
               || itemId == Item.Skull.id;
    }

    private static bool IsRedstoneItem(int itemId)
    {
        return itemId == Item.Redstone.id
               || itemId == Item.Repeater.id
               || itemId == Item.WoodenDoor.id
               || itemId == Item.IronDoor.id;
    }

    private static bool IsTransportationItem(int itemId)
    {
        return itemId == Item.Minecart.id
               || itemId == Item.ChestMinecart.id
               || itemId == Item.FurnaceMinecart.id
               || itemId == Item.TntMinecart.id
               || itemId == Item.Boat.id
               || itemId == Item.Saddle.id
               || itemId == Item.IronHorseArmor.id
               || itemId == Item.GoldenHorseArmor.id
               || itemId == Item.DiamondHorseArmor.id;
    }

    private static bool IsMiscItem(int itemId)
    {
        return itemId == Item.MonsterPlacer.id
               || itemId == Item.Bucket.id
               || itemId == Item.WaterBucket.id
               || itemId == Item.LavaBucket.id
               || itemId == Item.MilkBucket.id
               || itemId == Item.Map.id
               || itemId == Item.Book.id
               || itemId == Item.WritableBook.id
               || itemId == Item.Paper.id
               || itemId == Item.Slimeball.id
               || itemId == Item.Bone.id
               || itemId == Item.EnderPearl.id
               || itemId == Item.EyeOfEnder.id
               || itemId == Item.FireCharge.id
               || itemId == Item.ExpBottle.id
               || itemId == Item.RecordThirteen.id
               || itemId == Item.RecordCat.id
               || itemId == Item.RecordBlocks.id
               || itemId == Item.RecordChirp.id
               || itemId == Item.RecordFar.id
               || itemId == Item.RecordMall.id
               || itemId == Item.RecordMellohi.id
               || itemId == Item.RecordStal.id
               || itemId == Item.RecordStrad.id
               || itemId == Item.RecordWard.id
               || itemId == Item.RecordEleven.id
               || itemId == Item.RecordWait.id
               || itemId == Item.Snowball.id
               || itemId == Item.FireworkRocket.id
               || itemId == Item.EndCrystal.id;
    }

    private static bool IsBrewingItem(int itemId)
    {
        return itemId == Item.Potion.id
               || itemId == Item.GlassBottle.id
               || itemId == Item.GhastTear.id
               || itemId == Item.FermentedSpiderEye.id
               || itemId == Item.BlazePowder.id
               || itemId == Item.MagmaCream.id
               || itemId == Item.BrewingStand.id
               || itemId == Item.Cauldron.id
               || itemId == Item.SpeckledMelon.id
               || itemId == Item.GoldenCarrot.id
               || itemId == Item.RabbitFoot.id;
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
        _allStacks.Clear();

        for (int blockId = 1; blockId < Block.Blocks.Length; ++blockId)
        {
            Block? block = Block.Blocks[blockId];
            if (block != null)
            {
                AddCreativeStack(new ItemStack(block));
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
                        AddCreativeStack(new ItemStack(item.id, 1, entityId));
                    }
                }
                else if (item is ItemPotion potionItem)
                {
                    AddPotionVariants(potionItem);
                }
                else
                {
                    AddCreativeStack(new ItemStack(item));
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
        AddMetadataVariants(Block.NetherQuartzBlock, 3);
        AddMetadataVariants(Block.Silverfish, 6);
        AddMetadataVariants(Block.Wall, 2);
        AddMetadataVariants(Block.Carpet, 16);
        AddMetadataVariants(Block.Sponge, 2);
        AddMetadataVariants(Item.Dye, 16, 1);
        AddMetadataVariants(Item.GoldenApple, 2);
        AddMetadataVariants(Item.Skull, 6);
    }

    private void AddMetadataVariants(Block block, int count, int start = 1)
    {
        for (int meta = start; meta < count; ++meta)
        {
            AddCreativeStack(new ItemStack(block, 1, meta));
        }
    }

    private void AddMetadataVariants(Item item, int count, int start = 1)
    {
        for (int meta = start; meta < count; ++meta)
        {
            AddCreativeStack(new ItemStack(item.id, 1, meta));
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
            AddCreativeStack(s_cachedPotionVariants[i].copy());
        }
    }

    private void AddCreativeStack(ItemStack stack)
    {
        if (GetCreativeTab(stack) == null)
        {
            return;
        }

        for (int i = 0; i < _allStacks.Count; ++i)
        {
            ItemStack current = _allStacks[i];
            if (current.itemId == stack.itemId && current.getDamage() == stack.getDamage())
            {
                return;
            }
        }

        _allStacks.Add(stack);
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
