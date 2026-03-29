using BetaSharp.Blocks;
using BetaSharp.Entities;
using BetaSharp.Items;
using BetaSharp.PathFinding;
using BetaSharp.Rules;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds;
using BetaSharp.Worlds.Biomes.Source;
using BetaSharp.Worlds.Chunks;
using BetaSharp.Worlds.Chunks.Storage;
using BetaSharp.Worlds.Core.Systems;
using BetaSharp.Worlds.Dimensions;
using BetaSharp.Worlds.Gen.Chunks;
using BetaSharp.Worlds.Mechanics;
using BetaSharp.Worlds.Storage;
using BetaSharp.Worlds.Generation.Biomes;
using BetaSharp.Worlds.Generation.Structures;

namespace BetaSharp.Tests;

public class LegacyBiomeLayersTests
{
    [Fact]
    public void AddIslandPreservesNeighborBiomeIdsWhenGrowingLand()
    {
        TestParentLayer parent = new([
            LegacyBiomeIds.Desert, LegacyBiomeIds.Ocean, LegacyBiomeIds.Desert,
            LegacyBiomeIds.Ocean, LegacyBiomeIds.Ocean, LegacyBiomeIds.Ocean,
            LegacyBiomeIds.Desert, LegacyBiomeIds.Ocean, LegacyBiomeIds.Desert,
        ]);

        LegacyGenLayerIsland layer = new(3L, parent);
        bool producedDesert = false;

        for (int worldSeed = 0; worldSeed < 256; ++worldSeed)
        {
            layer.InitWorldGenSeed(worldSeed);
            int value = layer.GetValues(0, 0, 1, 1)[0];

            Assert.True(
                value is LegacyBiomeIds.Ocean or LegacyBiomeIds.Desert,
                $"Unexpected biome id {value} for seed {worldSeed}.");

            producedDesert |= value == LegacyBiomeIds.Desert;
        }

        Assert.True(producedDesert);
    }

    [Fact]
    public void BiomeLayerChangesWhenWorldSeedChanges()
    {
        LegacyGenLayer[] firstLayers = LegacyGenLayer.Build(12345L);
        LegacyGenLayer[] secondLayers = LegacyGenLayer.Build(67890L);

        int[] first = firstLayers[0].GetValues(-32, -32, 64, 64);
        int[] second = secondLayers[0].GetValues(-32, -32, 64, 64);

        bool differs = false;
        for (int i = 0; i < first.Length; ++i)
        {
            if (first[i] != second[i])
            {
                differs = true;
                break;
            }
        }

        Assert.True(differs);
    }

    [Fact]
    public void BiomeNearOriginChangesWhenWorldSeedChanges()
    {
        BiomeSource first = new(new StubWorldContext(12345L));
        BiomeSource second = new(new StubWorldContext(67890L));

        Biome[] firstBiomes = first.GetBiomesInArea(null, -32, -32, 64, 64);
        Biome[] secondBiomes = second.GetBiomesInArea(null, -32, -32, 64, 64);

        bool differs = false;
        for (int i = 0; i < firstBiomes.Length; ++i)
        {
            if (firstBiomes[i] != secondBiomes[i])
            {
                differs = true;
                break;
            }
        }

        Assert.True(differs);
    }

    [Fact]
    public void BiomeSourceDoesNotCollapseIntoOnlyIcePlains()
    {
        BiomeSource source = new(new StubWorldContext(12345L));
        Biome[] biomes = source.GetBiomesForGeneration(null, -64, -64, 128, 128);

        bool hasNonIceBiome = false;
        for (int i = 0; i < biomes.Length; ++i)
        {
            if (biomes[i] != Biome.IcePlains && biomes[i] != Biome.IceMountains)
            {
                hasNonIceBiome = true;
                break;
            }
        }

        Assert.True(hasNonIceBiome);
    }

    [Fact]
    public void BiomeSourceDoesNotCollapseIntoOnlyOcean()
    {
        BiomeSource source = new(new StubWorldContext(12345L));
        Biome[] biomes = source.GetBiomesForGeneration(null, -64, -64, 128, 128);

        bool hasLandBiome = false;
        for (int i = 0; i < biomes.Length; ++i)
        {
            if (biomes[i] != Biome.Ocean && biomes[i] != Biome.FrozenOcean)
            {
                hasLandBiome = true;
                break;
            }
        }

        Assert.True(hasLandBiome);
    }

    [Fact]
    public void BiomeFingerprintsVaryAcrossManySeeds()
    {
        HashSet<int> fingerprints = [];

        for (int seed = 1; seed <= 12; ++seed)
        {
            BiomeSource source = new(new StubWorldContext(seed));
            Biome[] biomes = source.GetBiomesInArea(null, -128, -128, 128, 128);
            HashCode hash = new();

            for (int i = 0; i < biomes.Length; ++i)
            {
                hash.Add(biomes[i].Id);
            }

            fingerprints.Add(hash.ToHashCode());
        }

        Assert.True(fingerprints.Count >= 5, $"Expected broad biome variation across seeds, but only found {fingerprints.Count} unique fingerprints.");
    }

    [Fact]
    public void DimensionChunkStorageUsesSeparateDirectories()
    {
        string tempRoot = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"betasharp-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempRoot);

        try
        {
            RegionWorldStorage storage = new(tempRoot, "world", createPlayersDir: false);
            RegionChunkStorage overworld = (RegionChunkStorage)storage.GetChunkStorage(new OverworldDimension());
            RegionChunkStorage nether = (RegionChunkStorage)storage.GetChunkStorage(new NetherDimension());
            RegionChunkStorage end = (RegionChunkStorage)storage.GetChunkStorage(new EndDimension());

            Assert.EndsWith(System.IO.Path.Combine("world"), GetStoragePath(overworld));
            Assert.EndsWith(System.IO.Path.Combine("world", "DIM-1"), GetStoragePath(nether));
            Assert.EndsWith(System.IO.Path.Combine("world", "DIM1"), GetStoragePath(end));
        }
        finally
        {
            Directory.Delete(tempRoot, recursive: true);
        }
    }

    [Fact]
    public void EndDimensionUsesDedicatedEndBiome()
    {
        EndDimension dimension = new();
        dimension.SetWorld(new StubWorldContext(12345L, dimension));

        Assert.Same(Biome.End, dimension.BiomeSource.GetBiome(0, 0));
        Assert.NotSame(Biome.Sky, dimension.BiomeSource.GetBiome(0, 0));
    }

    [Fact]
    public void EndChunkGeneratorProducesEndStoneAroundSpawnIsland()
    {
        EndDimension dimension = new();
        StubWorldContext world = new(12345L, dimension);
        dimension.SetWorld(world);

        EndChunkGenerator generator = new(world, world.Seed);
        byte[] blocks = new byte[-short.MinValue];
        typeof(EndChunkGenerator)
            .GetMethod("BuildTerrain", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
            .Invoke(generator, [0, 0, blocks]);

        Assert.Contains((byte)Block.EndStone.id, blocks);
    }

    [Fact]
    public void StrongholdStartBuildsPortalRoom()
    {
        OverworldDimension dimension = new();
        StubWorldContext world = new(12345L, dimension);
        dimension.SetWorld(world);

        StructureStrongholdStart start = new(world, new JavaRandom(12345L), 0, 0);

        Assert.True(start.IsSizeableStructure());
        Assert.Contains(start.GetComponents(), component => component is ComponentStrongholdPortalRoom);
        Assert.True(start.GetComponents().Count >= 2);
    }

    [Fact]
    public void VillageStartBuildsMultipleComponents()
    {
        OverworldDimension dimension = new();
        StubWorldContext world = new(12345L, dimension);
        dimension.SetWorld(world);

        StructureVillageStart start = new(world, new JavaRandom(12345L), 0, 0, 0);

        Assert.NotEmpty(start.GetComponents());
        Assert.Contains(start.GetComponents(), component => component is ComponentVillageStartPiece);
    }

    [Fact]
    public void OverworldSpawnPointRequiresGrassBlock()
    {
        OverworldDimension dimension = new();
        dimension.SetWorld(new StubWorldContext(12345L, dimension, spawnBlockId: Block.Snow.id));
        Assert.False(dimension.IsValidSpawnPoint(0, 0));

        dimension = new OverworldDimension();
        dimension.SetWorld(new StubWorldContext(12345L, dimension, spawnBlockId: Block.GrassBlock.id));
        Assert.True(dimension.IsValidSpawnPoint(0, 0));
    }

    private sealed class TestParentLayer(int[] values) : LegacyGenLayer(1L)
    {
        public override int[] GetValues(int x, int z, int width, int depth)
        {
            Assert.Equal(3, width);
            Assert.Equal(3, depth);
            return values;
        }
    }

    private sealed class StubWorldContext(long seed, Dimension? dimension = null, int spawnBlockId = 0) : IWorldContext
    {
        private readonly WorldProperties _properties = new(seed, "test") { TerrainType = WorldType.Default };

        public IBlockReader Reader => throw new NotSupportedException();
        public IBlockWriter Writer => throw new NotSupportedException();
        public ChunkHost ChunkHost => throw new NotSupportedException();
        public WorldEventBroadcaster Broadcaster => throw new NotSupportedException();
        public RedstoneEngine Redstone => throw new NotSupportedException();
        public EntityManager Entities => throw new NotSupportedException();
        public LightingEngine Lighting => throw new NotSupportedException();
        public EnvironmentManager Environment => throw new NotSupportedException();
        public Dimension Dimension => dimension ?? throw new NotSupportedException();
        public WorldTickScheduler TickScheduler => throw new NotSupportedException();
        public long Seed => seed;
        public bool IsRemote => false;
        public RuleSet Rules => throw new NotSupportedException();
        public PersistentStateManager StateManager => throw new NotSupportedException();
        public int Difficulty => 0;
        public WorldProperties Properties => _properties;
        public JavaRandom Random => new(seed);
        PathFinder IWorldContext.Pathing => throw new NotSupportedException();
        public void SetDifficulty(int difficulty) => throw new NotSupportedException();
        public long GetTime() => 0L;
        public int GetSpawnBlockId(int x, int z) => spawnBlockId;
        public float GetTemperature(int x, int y, int z) => 0.0F;
        public bool CanBlockFreeze(int x, int y, int z) => false;
        public bool CanBlockFreeze(int x, int y, int z, bool requireSurroundedWater) => false;
        public bool CanSnowAt(int x, int y, int z) => false;
        public bool SpawnEntity(Entity entity) => throw new NotSupportedException();
        public bool SpawnItemDrop(double x, double y, double z, ItemStack itemStack) => throw new NotSupportedException();
        public bool CanInteract(EntityPlayer player, int x, int y, int z) => false;
        public Explosion CreateExplosion(Entity? source, double x, double y, double z, float power, bool fire) => throw new NotSupportedException();
        public Explosion CreateExplosion(Entity? source, double x, double y, double z, float power) => throw new NotSupportedException();
    }

    private static string GetStoragePath(RegionChunkStorage storage)
    {
        return (string)typeof(RegionChunkStorage)
            .GetField("_dir", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
            .GetValue(storage)!;
    }
}
