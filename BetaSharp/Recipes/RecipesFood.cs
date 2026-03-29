using BetaSharp.Blocks;
using BetaSharp.Items;

namespace BetaSharp.Recipes;

internal class RecipesFood
{
    public void AddRecipes(CraftingManager m)
    {
        m.AddShapelessRecipe(new ItemStack(Item.MushroomStew), Block.BrownMushroom, Block.RedMushroom, Item.Bowl);
        m.AddRecipe(new ItemStack(Item.Cookie, 8), "#X#", 'X', new ItemStack(Item.Dye, 1, 3), '#', Item.Wheat);
        m.AddRecipe(new ItemStack(Block.Melon), "MMM", "MMM", "MMM", 'M', Item.Melon);
        m.AddRecipe(new ItemStack(Item.MelonSeeds), "M", 'M', Item.Melon);
        m.AddRecipe(new ItemStack(Item.PumpkinSeeds, 4), "M", 'M', Block.Pumpkin);
        m.AddShapelessRecipe(new ItemStack(Item.FermentedSpiderEye), Item.SpiderEye, Block.BrownMushroom, Item.Sugar);
        m.AddShapelessRecipe(new ItemStack(Item.SpeckledMelon), Item.Melon, Item.GoldNugget);
        m.AddShapelessRecipe(new ItemStack(Item.GoldenCarrot), Item.Carrot, Item.GoldNugget, Item.GoldNugget, Item.GoldNugget, Item.GoldNugget, Item.GoldNugget, Item.GoldNugget, Item.GoldNugget, Item.GoldNugget);
        m.AddShapelessRecipe(new ItemStack(Item.BlazePowder, 2), Item.BlazeRod);
        m.AddShapelessRecipe(new ItemStack(Item.MagmaCream), Item.BlazePowder, Item.Slimeball);
    }
}
