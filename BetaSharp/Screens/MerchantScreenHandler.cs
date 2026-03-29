using BetaSharp.Entities;
using BetaSharp.Inventorys;
using BetaSharp.Items;
using BetaSharp.Network.Packets.S2CPlay;
using BetaSharp.Screens.Slots;
using BetaSharp.Trading;

namespace BetaSharp.Screens;

public sealed class MerchantScreenHandler : ScreenHandler
{
    private readonly EntityPlayer _player;
    private readonly IMerchant _merchant;
    private readonly InventoryMerchant _merchantInventory;
    private MerchantRecipeList _offers = new();

    public int SelectedRecipeIndex { get; private set; } = -1;
    public MerchantRecipeList Offers => _offers;
    public InventoryMerchant MerchantInventory => _merchantInventory;

    public MerchantScreenHandler(InventoryPlayer playerInventory) : this(playerInventory, NullMerchant.Instance)
    {
    }

    public MerchantScreenHandler(InventoryPlayer playerInventory, IMerchant merchant)
    {
        _player = playerInventory.player;
        _merchant = merchant;
        _merchantInventory = new InventoryMerchant(this, merchant);

        AddSlot(new Slot(_merchantInventory, 0, 36, 53));
        AddSlot(new Slot(_merchantInventory, 1, 62, 53));
        AddSlot(new MerchantResultSlot(playerInventory.player, merchant, _merchantInventory, 2, 120, 53, this));

        for (int row = 0; row < 3; ++row)
        {
            for (int column = 0; column < 9; ++column)
            {
                AddSlot(new Slot(playerInventory, column + row * 9 + 9, 8 + column * 18, 84 + row * 18));
            }
        }

        for (int hotbarSlot = 0; hotbarSlot < 9; ++hotbarSlot)
        {
            AddSlot(new Slot(playerInventory, hotbarSlot, 8 + hotbarSlot * 18, 142));
        }
    }

    public void SetOffers(MerchantRecipeList offers, int selectedRecipeIndex)
    {
        _offers = offers ?? new MerchantRecipeList();
        if (_offers.Count == 0)
        {
            SelectedRecipeIndex = -1;
        }
        else
        {
            SelectedRecipeIndex = Math.Clamp(selectedRecipeIndex, 0, _offers.Count - 1);
        }

        _merchantInventory.SetOffers(_offers, SelectedRecipeIndex);
        SendContentUpdates();
    }

    public void SelectRecipe(int recipeIndex)
    {
        if (_offers.Count == 0)
        {
            SelectedRecipeIndex = -1;
        }
        else
        {
            SelectedRecipeIndex = Math.Clamp(recipeIndex, 0, _offers.Count - 1);
        }

        _merchantInventory.SetCurrentRecipeIndex(SelectedRecipeIndex);
        SendContentUpdates();
    }

    public void SendOffersTo(ServerPlayerEntity player)
    {
        player.networkHandler.sendPacket(MerchantOffersS2CPacket.Get(SyncId, _offers, SelectedRecipeIndex));
    }

    public void OnRecipeUsed()
    {
        _merchantInventory.UpdateOffers();
        SendContentUpdates();
        if (_player is ServerPlayerEntity serverPlayer)
        {
            SendOffersTo(serverPlayer);
        }
    }

    public override void onSlotUpdate(IInventory inventory)
    {
        _merchantInventory.UpdateOffers();
        base.onSlotUpdate(inventory);
    }

    public override void onClosed(EntityPlayer player)
    {
        base.onClosed(player);
        _merchant.SetCustomer(null);

        if (player.world.IsRemote)
        {
            return;
        }

        DropInputSlot(player, 0);
        DropInputSlot(player, 1);
    }

    public override bool canUse(EntityPlayer player)
    {
        EntityPlayer? customer = _merchant.GetCustomer();
        return customer == null || customer == player;
    }

    public override ItemStack? quickMove(int slotNumber)
    {
        if (slotNumber < 0 || slotNumber >= Slots.Count)
        {
            return null;
        }

        Slot slot = Slots[slotNumber];
        if (slot == null || !slot.hasStack())
        {
            return null;
        }

        ItemStack source = slot.getStack();
        ItemStack result = source.copy();

        if (slotNumber == 2)
        {
            insertItem(source, 3, 39, true);
            slot.onTakeItem(source);
        }
        else if (slotNumber == 0 || slotNumber == 1)
        {
            insertItem(source, 3, 39, false);
        }
        else
        {
            int countBefore = source.count;
            insertItem(source, 0, 2, false);
            if (source.count == countBefore)
            {
                if (slotNumber >= 3 && slotNumber < 30)
                {
                    insertItem(source, 30, 39, false);
                }
                else if (slotNumber >= 30 && slotNumber < 39)
                {
                    insertItem(source, 3, 30, false);
                }
            }
        }

        if (source.count == 0)
        {
            slot.setStack(null);
        }
        else
        {
            slot.markDirty();
        }

        return source.count == result.count ? null : result;
    }

    private void DropInputSlot(EntityPlayer player, int slotIndex)
    {
        ItemStack? stack = _merchantInventory.getStack(slotIndex);
        if (stack == null)
        {
            return;
        }

        _merchantInventory.setStack(slotIndex, null);
        player.dropItem(stack);
    }

    private sealed class NullMerchant : IMerchant
    {
        public static readonly NullMerchant Instance = new();

        public EntityPlayer? GetCustomer() => null;

        public void SetCustomer(EntityPlayer? player)
        {
        }

        public MerchantRecipeList GetRecipes(EntityPlayer player) => new();

        public void SetRecipes(MerchantRecipeList recipes)
        {
        }

        public void UseRecipe(MerchantRecipe recipe)
        {
        }
    }
}
