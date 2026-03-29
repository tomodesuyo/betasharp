using BetaSharp.Items;

namespace BetaSharp.Trading;

public sealed class MerchantRecipe
{
    public ItemStack BuyItem1 { get; }
    public ItemStack? BuyItem2 { get; }
    public ItemStack SellItem { get; }
    public int ToolUses { get; private set; }
    public int MaxToolUses { get; }

    public bool HasSecondBuyItem => BuyItem2 != null;
    public bool IsDisabled => ToolUses >= MaxToolUses;

    public MerchantRecipe(ItemStack buyItem1, ItemStack sellItem) : this(buyItem1, null, sellItem)
    {
    }

    public MerchantRecipe(ItemStack buyItem1, ItemStack? buyItem2, ItemStack sellItem, int toolUses = 0, int maxToolUses = 7)
    {
        BuyItem1 = buyItem1.copy();
        BuyItem2 = buyItem2?.copy();
        SellItem = sellItem.copy();
        ToolUses = toolUses;
        MaxToolUses = maxToolUses;
    }

    public bool Matches(ItemStack? first, ItemStack? second)
    {
        if (!MatchesStack(first, BuyItem1))
        {
            return false;
        }

        if (BuyItem2 == null)
        {
            return second == null;
        }

        return MatchesStack(second, BuyItem2);
    }

    public void IncrementToolUses()
    {
        ++ToolUses;
    }

    private static bool MatchesStack(ItemStack? input, ItemStack required)
    {
        if (input == null || input.itemId != required.itemId)
        {
            return false;
        }

        if (required.getHasSubtypes() && input.getDamage() != required.getDamage())
        {
            return false;
        }

        return input.count >= required.count;
    }
}
