using BetaSharp.Blocks;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Core.Systems;
using BetaSharp.Worlds.Generation.Generators.Features;

namespace BetaSharp.Worlds.Generation.Biomes;

internal class BiomeDecorator
{
    private readonly ClayOreFeature _clay = new(4);
    private readonly SandPatchFeature _sand = new(7, Block.Sand.id);
    private readonly SandPatchFeature _gravelPatch = new(6, Block.Gravel.id);
    private readonly OreFeature _dirt = new(Block.Dirt.id, 32);
    private readonly OreFeature _gravel = new(Block.Gravel.id, 32);
    private readonly OreFeature _coal = new(Block.CoalOre.id, 16);
    private readonly OreFeature _iron = new(Block.IronOre.id, 8);
    private readonly OreFeature _gold = new(Block.GoldOre.id, 8);
    private readonly OreFeature _redstone = new(Block.RedstoneOre.id, 7);
    private readonly OreFeature _diamond = new(Block.DiamondOre.id, 7);
    private readonly OreFeature _lapis = new(Block.LapisOre.id, 6);
    private readonly PlantPatchFeature _dandelion = new(Block.Dandelion.id);
    private readonly PlantPatchFeature _rose = new(Block.Rose.id);
    private readonly PlantPatchFeature _brownMushroom = new(Block.BrownMushroom.id);
    private readonly PlantPatchFeature _redMushroom = new(Block.RedMushroom.id);
    private readonly SugarCanePatchFeature _reeds = new();
    private readonly CactusPatchFeature _cactus = new();
    private readonly WaterLilyFeature _waterLily = new();
    private readonly DungeonFeature _dungeon = new();
    private readonly DeadBushPatchFeature _deadBush = new(Block.DeadBush.id);
    private readonly PumpkinPatchFeature _pumpkin = new();
    private readonly SpringFeature _waterSpring = new(Block.FlowingWater.id);
    private readonly SpringFeature _lavaSpring = new(Block.FlowingLava.id);

    public int TreesPerChunk { get; set; }
    public int FlowersPerChunk { get; set; } = 2;
    public int GrassPerChunk { get; set; } = 1;
    public int DeadBushPerChunk { get; set; }
    public int MushroomsPerChunk { get; set; }
    public int ReedsPerChunk { get; set; }
    public int CactiPerChunk { get; set; }
    public int WaterLiliesPerChunk { get; set; }
    public int SandPerChunk { get; set; } = 3;
    public int GravelPerChunk { get; set; } = 1;
    public int ClayPerChunk { get; set; } = 1;

    public virtual void Decorate(IWorldContext world, JavaRandom random, Biome biome, int blockX, int blockZ)
    {
        GenerateOres(world, random, blockX, blockZ);

        int featureX;
        int featureY;
        int featureZ;

        for (int i = 0; i < SandPerChunk; ++i)
        {
            featureX = blockX + random.NextInt(16) + 8;
            featureZ = blockZ + random.NextInt(16) + 8;
            _sand.Generate(world, random, featureX, world.Reader.GetTopSolidBlockY(featureX, featureZ), featureZ);
        }

        for (int i = 0; i < ClayPerChunk; ++i)
        {
            featureX = blockX + random.NextInt(16) + 8;
            featureZ = blockZ + random.NextInt(16) + 8;
            _clay.Generate(world, random, featureX, world.Reader.GetTopSolidBlockY(featureX, featureZ), featureZ);
        }

        for (int i = 0; i < GravelPerChunk; ++i)
        {
            featureX = blockX + random.NextInt(16) + 8;
            featureZ = blockZ + random.NextInt(16) + 8;
            _gravelPatch.Generate(world, random, featureX, world.Reader.GetTopSolidBlockY(featureX, featureZ), featureZ);
        }

        int treeCount = TreesPerChunk;
        if (random.NextInt(10) == 0)
        {
            ++treeCount;
        }

        for (int i = 0; i < treeCount; ++i)
        {
            featureX = blockX + random.NextInt(16) + 8;
            featureZ = blockZ + random.NextInt(16) + 8;
            Feature tree = biome.GetRandomWorldGenForTrees(random);
            tree.prepare(1.0D, 1.0D, 1.0D);
            tree.Generate(world, random, featureX, world.Reader.GetTopY(featureX, featureZ), featureZ);
        }

        for (int i = 0; i < FlowersPerChunk; ++i)
        {
            featureX = blockX + random.NextInt(16) + 8;
            featureY = random.NextInt(128);
            featureZ = blockZ + random.NextInt(16) + 8;
            _dandelion.Generate(world, random, featureX, featureY, featureZ);
            if (random.NextInt(4) == 0)
            {
                featureX = blockX + random.NextInt(16) + 8;
                featureY = random.NextInt(128);
                featureZ = blockZ + random.NextInt(16) + 8;
                _rose.Generate(world, random, featureX, featureY, featureZ);
            }
        }

        for (int i = 0; i < GrassPerChunk; ++i)
        {
            featureX = blockX + random.NextInt(16) + 8;
            featureY = random.NextInt(128);
            featureZ = blockZ + random.NextInt(16) + 8;
            biome.GetRandomWorldGenForGrass(random).Generate(world, random, featureX, featureY, featureZ);
        }

        for (int i = 0; i < DeadBushPerChunk; ++i)
        {
            featureX = blockX + random.NextInt(16) + 8;
            featureY = random.NextInt(128);
            featureZ = blockZ + random.NextInt(16) + 8;
            _deadBush.Generate(world, random, featureX, featureY, featureZ);
        }

        for (int i = 0; i < WaterLiliesPerChunk; ++i)
        {
            featureX = blockX + random.NextInt(16) + 8;
            featureZ = blockZ + random.NextInt(16) + 8;

            for (featureY = random.NextInt(128); featureY > 0 && world.Reader.GetBlockId(featureX, featureY - 1, featureZ) == 0; --featureY)
            {
            }

            _waterLily.Generate(world, random, featureX, featureY, featureZ);
        }

        for (int i = 0; i < MushroomsPerChunk; ++i)
        {
            if (random.NextInt(4) == 0)
            {
                featureX = blockX + random.NextInt(16) + 8;
                featureZ = blockZ + random.NextInt(16) + 8;
                featureY = world.Reader.GetTopY(featureX, featureZ);
                _brownMushroom.Generate(world, random, featureX, featureY, featureZ);
            }

            if (random.NextInt(8) == 0)
            {
                featureX = blockX + random.NextInt(16) + 8;
                featureY = random.NextInt(128);
                featureZ = blockZ + random.NextInt(16) + 8;
                _redMushroom.Generate(world, random, featureX, featureY, featureZ);
            }
        }

        if (random.NextInt(4) == 0)
        {
            featureX = blockX + random.NextInt(16) + 8;
            featureY = random.NextInt(128);
            featureZ = blockZ + random.NextInt(16) + 8;
            _brownMushroom.Generate(world, random, featureX, featureY, featureZ);
        }

        if (random.NextInt(8) == 0)
        {
            featureX = blockX + random.NextInt(16) + 8;
            featureY = random.NextInt(128);
            featureZ = blockZ + random.NextInt(16) + 8;
            _redMushroom.Generate(world, random, featureX, featureY, featureZ);
        }

        for (int i = 0; i < ReedsPerChunk; ++i)
        {
            featureX = blockX + random.NextInt(16) + 8;
            featureY = random.NextInt(128);
            featureZ = blockZ + random.NextInt(16) + 8;
            _reeds.Generate(world, random, featureX, featureY, featureZ);
        }

        for (int i = 0; i < 10; ++i)
        {
            featureX = blockX + random.NextInt(16) + 8;
            featureY = random.NextInt(128);
            featureZ = blockZ + random.NextInt(16) + 8;
            _reeds.Generate(world, random, featureX, featureY, featureZ);
        }

        if (random.NextInt(32) == 0)
        {
            featureX = blockX + random.NextInt(16) + 8;
            featureY = random.NextInt(128);
            featureZ = blockZ + random.NextInt(16) + 8;
            _pumpkin.Generate(world, random, featureX, featureY, featureZ);
        }

        for (int i = 0; i < CactiPerChunk; ++i)
        {
            featureX = blockX + random.NextInt(16) + 8;
            featureY = random.NextInt(128);
            featureZ = blockZ + random.NextInt(16) + 8;
            _cactus.Generate(world, random, featureX, featureY, featureZ);
        }

        for (int i = 0; i < 50; ++i)
        {
            featureX = blockX + random.NextInt(16) + 8;
            featureY = random.NextInt(random.NextInt(120) + 8);
            featureZ = blockZ + random.NextInt(16) + 8;
            _waterSpring.Generate(world, random, featureX, featureY, featureZ);
        }

        for (int i = 0; i < 20; ++i)
        {
            featureX = blockX + random.NextInt(16) + 8;
            featureY = random.NextInt(random.NextInt(random.NextInt(112) + 8) + 8);
            featureZ = blockZ + random.NextInt(16) + 8;
            _lavaSpring.Generate(world, random, featureX, featureY, featureZ);
        }
    }

    protected virtual void GenerateOres(IWorldContext world, JavaRandom random, int blockX, int blockZ)
    {
        for (int i = 0; i < 8; ++i)
        {
            int featureX = blockX + random.NextInt(16) + 8;
            int featureY = random.NextInt(128);
            int featureZ = blockZ + random.NextInt(16) + 8;
            _dungeon.Generate(world, random, featureX, featureY, featureZ);
        }

        GenerateStandardOre1(world, random, blockX, blockZ, 20, _dirt, 0, 128);
        GenerateStandardOre1(world, random, blockX, blockZ, 10, _gravel, 0, 128);
        GenerateStandardOre1(world, random, blockX, blockZ, 20, _coal, 0, 128);
        GenerateStandardOre1(world, random, blockX, blockZ, 20, _iron, 0, 64);
        GenerateStandardOre1(world, random, blockX, blockZ, 2, _gold, 0, 32);
        GenerateStandardOre1(world, random, blockX, blockZ, 8, _redstone, 0, 16);
        GenerateStandardOre1(world, random, blockX, blockZ, 1, _diamond, 0, 16);
        GenerateStandardOre2(world, random, blockX, blockZ, 1, _lapis, 16, 16);
    }

    private static void GenerateStandardOre1(IWorldContext world, JavaRandom random, int blockX, int blockZ, int count, Feature feature, int minY, int maxY)
    {
        for (int i = 0; i < count; ++i)
        {
            int featureX = blockX + random.NextInt(16);
            int featureY = random.NextInt(maxY - minY) + minY;
            int featureZ = blockZ + random.NextInt(16);
            feature.Generate(world, random, featureX, featureY, featureZ);
        }
    }

    private static void GenerateStandardOre2(IWorldContext world, JavaRandom random, int blockX, int blockZ, int count, Feature feature, int centerY, int spreadY)
    {
        for (int i = 0; i < count; ++i)
        {
            int featureX = blockX + random.NextInt(16);
            int featureY = random.NextInt(spreadY) + random.NextInt(spreadY) + (centerY - spreadY);
            int featureZ = blockZ + random.NextInt(16);
            feature.Generate(world, random, featureX, featureY, featureZ);
        }
    }
}
