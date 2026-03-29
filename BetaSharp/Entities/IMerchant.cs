using BetaSharp.Trading;

namespace BetaSharp.Entities;

public interface IMerchant
{
    EntityPlayer? GetCustomer();

    void SetCustomer(EntityPlayer? player);

    MerchantRecipeList GetRecipes(EntityPlayer player);

    void SetRecipes(MerchantRecipeList recipes);

    void UseRecipe(MerchantRecipe recipe);
}
