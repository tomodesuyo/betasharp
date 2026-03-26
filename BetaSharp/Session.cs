using BetaSharp.Blocks;
namespace BetaSharp;

public class Session(string username, string sessionId)
{
    public static List<object> RegisteredBlocksList = new List<object>();
    public string username = username;
    public string sessionId = sessionId;

	static Session()
	{
		RegisteredBlocksList.Add(Block.Stone);
		RegisteredBlocksList.Add(Block.Cobblestone);
		RegisteredBlocksList.Add(Block.Bricks);
		RegisteredBlocksList.Add(Block.Dirt);
		RegisteredBlocksList.Add(Block.Planks);
		RegisteredBlocksList.Add(Block.Planks);
		RegisteredBlocksList.Add(Block.Leaves);
		RegisteredBlocksList.Add(Block.Torch);
		RegisteredBlocksList.Add(Block.Slab);
		RegisteredBlocksList.Add(Block.Glass);
		RegisteredBlocksList.Add(Block.MossyCobblestone);
		RegisteredBlocksList.Add(Block.Sapling);
		RegisteredBlocksList.Add(Block.Dandelion);
		RegisteredBlocksList.Add(Block.Rose);
		// RegisteredBlocksList.Add(Block.MushroomBrown);
		// RegisteredBlocksList.Add(Block.MushroomRed);
		RegisteredBlocksList.Add(Block.Sand);
		RegisteredBlocksList.Add(Block.Gravel);
		RegisteredBlocksList.Add(Block.Sponge);
		RegisteredBlocksList.Add(Block.Wool);
		RegisteredBlocksList.Add(Block.CoalOre);
		RegisteredBlocksList.Add(Block.IronOre);
		RegisteredBlocksList.Add(Block.GoldOre);
		RegisteredBlocksList.Add(Block.IronBlock);
		RegisteredBlocksList.Add(Block.GoldBlock);
		// RegisteredBlocksList.Add(Block.BookShelf);
		RegisteredBlocksList.Add(Block.TNT);
		RegisteredBlocksList.Add(Block.Obsidian);
	}
}
