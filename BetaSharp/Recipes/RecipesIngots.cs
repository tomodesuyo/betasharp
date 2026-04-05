using BetaSharp.Blocks;
using BetaSharp.Items;

namespace BetaSharp.Recipes;

internal class RecipesIngots
{
    private object[][] recipeItems = [[Block.GoldBlock, new ItemStack(Item.GoldIngot, 9)], [Block.IronBlock, new ItemStack(Item.IronIngot, 9)], [Block.DiamondBlock, new ItemStack(Item.Diamond, 9)], [Block.LapisBlock, new ItemStack(Item.Dye, 9, 4)], [Block.EmeraldBlock, new ItemStack(Item.Emerald, 9)], [Block.RedstoneBlock, new ItemStack(Item.Redstone, 9)], [Block.Hay, new ItemStack(Item.Wheat, 9)]];

    public void AddRecipes(CraftingManager m)
    {
        for (int i = 0; i < recipeItems.Length; ++i)
        {
            Block block = (Block)recipeItems[i][0];
            ItemStack ingot = (ItemStack)recipeItems[i][1];
            m.AddRecipe(new ItemStack(block), "###", "###", "###", '#', ingot);
            m.AddRecipe(ingot, "#", '#', block);
        }

        m.AddRecipe(new ItemStack(Item.IronNugget, 9), "#", '#', Item.IronIngot);
        m.AddRecipe(new ItemStack(Item.IronIngot, 1), ["###", "###", "###", '#', Item.IronNugget]);
    }
}
