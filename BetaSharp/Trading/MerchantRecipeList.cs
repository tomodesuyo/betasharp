using BetaSharp.Items;

namespace BetaSharp.Trading;

public sealed class MerchantRecipeList : List<MerchantRecipe>
{
    public MerchantRecipe? FindMatch(ItemStack? first, ItemStack? second, int preferredIndex = -1)
    {
        if (preferredIndex >= 0 && preferredIndex < Count)
        {
            MerchantRecipe preferred = this[preferredIndex];
            if (!preferred.IsDisabled && preferred.Matches(first, second))
            {
                return preferred;
            }
        }

        for (int i = 0; i < Count; ++i)
        {
            MerchantRecipe recipe = this[i];
            if (!recipe.IsDisabled && recipe.Matches(first, second))
            {
                return recipe;
            }
        }

        return null;
    }
}
