using BetaSharp.Blocks;
using BetaSharp.Items;

namespace BetaSharp.Recipes;

internal class RecipesCrafting
{
    public void AddRecipes(CraftingManager manager)
    {
        manager.AddRecipe(new ItemStack(Block.Chest), "###", "# #", "###", '#', Block.Planks);
        manager.AddRecipe(new ItemStack(Block.Furnace), "###", "# #", "###", '#', Block.Cobblestone);
        manager.AddRecipe(new ItemStack(Block.CraftingTable), "##", "##", '#', Block.Planks);
        manager.AddRecipe(new ItemStack(Block.Sandstone), "##", "##", '#', Block.Sand);
        manager.AddRecipe(new ItemStack(Block.Sandstone, 4, 2), "##", "##", '#', Block.Sandstone);
        manager.AddRecipe(new ItemStack(Block.Sandstone, 1, 1), "#", "#", '#', new ItemStack(Block.Slab, 1, 1));
        manager.AddRecipe(new ItemStack(Block.StoneBrick, 4), "##", "##", '#', Block.Stone);
        manager.AddRecipe(new ItemStack(Block.IronBars, 16), "###", "###", '#', Item.IronIngot);
        manager.AddRecipe(new ItemStack(Block.GlassPane, 16), "###", "###", '#', Block.Glass);
        manager.AddRecipe(new ItemStack(Block.RedstoneLamp), " R ", "RGR", " R ", 'R', Item.Redstone, 'G', Block.Glowstone);
        manager.AddRecipe(new ItemStack(Block.SlimeBlock), "###", "###", "###", '#', Item.Slimeball);

        for (int color = 0; color < 16; ++color)
        {
            manager.AddRecipe(new ItemStack(Block.Carpet, 3, color), "##", '#', new ItemStack(Block.Wool, 1, color));
        }
    }
}
