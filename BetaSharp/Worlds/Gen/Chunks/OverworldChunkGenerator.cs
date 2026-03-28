using BetaSharp.Blocks;
using BetaSharp.Blocks.Materials;
using BetaSharp.Util.Maths;
using BetaSharp.Util.Maths.Noise;
using BetaSharp.Worlds.Biomes.Source;
using BetaSharp.Worlds.Chunks;
using BetaSharp.Worlds.Core.Systems;
using BetaSharp.Worlds.Generation.Biomes;
using BetaSharp.Worlds.Generation.Generators.Carvers;
using BetaSharp.Worlds.Generation.Generators.Features;
using BetaSharp.Worlds.Generation.Structures;

namespace BetaSharp.Worlds.Gen.Chunks;

internal class OverworldChunkGenerator : IChunkSource
{
    private readonly BiomeSource _biomeSource;
    private readonly Carver _caveCarver = new CaveCarver();
    private readonly Carver _ravineCarver = new RavineCarver();
    private readonly OctavePerlinNoiseSampler _depthNoise;
    private readonly OctavePerlinNoiseSampler _floatingIslandNoise;
    private readonly OctavePerlinNoiseSampler _floatingIslandScale;
    private readonly OctavePerlinNoiseSampler _forestNoise;
    private readonly IWorldContext _level;
    private readonly OctavePerlinNoiseSampler _maxLimitPerlinNoise;
    private readonly OctavePerlinNoiseSampler _minLimitPerlinNoise;
    private readonly JavaRandom _random;
    private readonly float[] _biomeWeights = new float[25];

    // Seed and per-instance biome source (allows thread-safe parallel generation)
    private readonly long _seed;
    private readonly OctavePerlinNoiseSampler _selectorNoise;
    private readonly MapGenMineshaft _mineshaftGenerator = new();
    private readonly MapGenStronghold _strongholdGenerator = new();
    private readonly MapGenVillage _villageGenerator = new(0);
    private Biome[] _biomes;
    private Biome[] _generationBiomes;
    private double[] _depthBuffer = new double[256];
    private double[] _depthNoiseBuffer;
    private PlantPatchFeature _featureBrownMushroom;
    private CactusPatchFeature _featureCactus;
    private ClayOreFeature _featureClay;
    private OreFeature _featureCoal;
    private PlantPatchFeature _featureDandelion;
    private DeadBushPatchFeature _featureDeadBush;
    private OreFeature _featureDiamond;
    private OreFeature _featureDirt;
    private DungeonFeature _featureDungeon;
    private OreFeature _featureGold;
    private GrassPatchFeature _featureGrass1;
    private GrassPatchFeature _featureGrass2;
    private OreFeature _featureGravel;
    private OreFeature _featureIron;
    private OreFeature _featureLapis;
    private LakeFeature _featureLavaLake;
    private SpringFeature _featureLavaSpring;
    private PumpkinPatchFeature _featurePumpkin;
    private PlantPatchFeature _featureRedMushroom;
    private OreFeature _featureRedstone;
    private PlantPatchFeature _featureRose;
    private SugarCanePatchFeature _featureSugarcane;

    // Pre-allocated feature instances reused across every decorated chunk
    private LakeFeature _featureWaterLake;
    private SpringFeature _featureWaterSpring;
    private double[] _heightMap;
    private double[] _maxLimitPerlinNoiseBuffer;
    private double[] _minLimitPerlinNoiseBuffer;
    private double[] _scaleNoiseBuffer;
    private double[] _selectorNoiseBuffer;
    private double[] _temperatures;

    public OverworldChunkGenerator(IWorldContext world, long seed)
    {
        _level = world;
        _random = new JavaRandom(seed);
        _minLimitPerlinNoise = new OctavePerlinNoiseSampler(_random, 16);
        _maxLimitPerlinNoise = new OctavePerlinNoiseSampler(_random, 16);
        _selectorNoise = new OctavePerlinNoiseSampler(_random, 8);
        _depthNoise = new OctavePerlinNoiseSampler(_random, 4);
        _floatingIslandScale = new OctavePerlinNoiseSampler(_random, 10);
        _floatingIslandNoise = new OctavePerlinNoiseSampler(_random, 16);
        _forestNoise = new OctavePerlinNoiseSampler(_random, 8);
        _seed = seed;
        _biomeSource = world.Dimension.BiomeSource;
        InitBiomeWeights();
        InitFeatures();
    }

    private OverworldChunkGenerator(IWorldContext level, long seed, BiomeSource biomeSource)
    {
        _level = level;
        _seed = seed;
        _biomeSource = biomeSource;
        _random = new JavaRandom(seed);
        _minLimitPerlinNoise = new OctavePerlinNoiseSampler(_random, 16);
        _maxLimitPerlinNoise = new OctavePerlinNoiseSampler(_random, 16);
        _selectorNoise = new OctavePerlinNoiseSampler(_random, 8);
        _depthNoise = new OctavePerlinNoiseSampler(_random, 4);
        _floatingIslandScale = new OctavePerlinNoiseSampler(_random, 10);
        _floatingIslandNoise = new OctavePerlinNoiseSampler(_random, 16);
        _forestNoise = new OctavePerlinNoiseSampler(_random, 8);
        InitBiomeWeights();
        InitFeatures();
    }

    // Creates a thread-safe parallel generator with its own BiomeSource and _random state.
    // All noise samplers are deterministically equivalent (same seed), so chunk output is identical.
    public IChunkSource CreateParallelInstance()
        => new OverworldChunkGenerator(_level, _seed, new BiomeSource(_level));

    public Chunk LoadChunk(int chunkX, int chunkZ) => GetChunk(chunkX, chunkZ);

    /// <summary>
    ///     Generates a chunk at the given coordinates. The chunk is generated by first creating a low-resolution height map,
    ///     then interpolating it to determine the base terrain, and finally carving caves and adding features to it.
    /// </summary>
    /// <param name="chunkX">The x-coordinate of the chunk</param>
    /// <param name="chunkZ">The z-coordinate of the chunk</param>
    /// <returns>The generated chunk</returns>
    public Chunk GetChunk(int chunkX, int chunkZ)
    {
        _random.SetSeed(chunkX * 341873128712L + chunkZ * 132897987541L);
        byte[] blocks = new byte[-short.MinValue];
        Chunk chunk = new(_level, blocks, chunkX, chunkZ);
        _generationBiomes = _biomeSource.GetBiomesForGeneration(_generationBiomes, chunkX * 4 - 2, chunkZ * 4 - 2, 10, 10);
        BuildTerrain(chunkX, chunkZ, blocks, _generationBiomes);
        _biomes = _biomeSource.GetBiomesInArea(_biomes, chunkX * 16, chunkZ * 16, 16, 16);
        BuildSurfaces(chunkX, chunkZ, blocks, _biomes);
        _mineshaftGenerator.Generate(this, _level, chunkX, chunkZ, blocks);
        _villageGenerator.Generate(this, _level, chunkX, chunkZ, blocks);
        _strongholdGenerator.Generate(this, _level, chunkX, chunkZ, blocks);
        _caveCarver.carve(this, _level, chunkX, chunkZ, blocks);
        _ravineCarver.carve(this, _level, chunkX, chunkZ, blocks);
        chunk.PopulateHeightMap();
        return chunk;
    }

    public bool IsChunkLoaded(int x, int z) => true;

    /// <summary>
    ///     Generates the features of the chunk, such as ores, trees, lakes, etc. The features that are generated depend on the
    ///     biome of the chunk and some _random factors.
    /// </summary>
    /// <param name="source">The chunk source that is generating the chunk</param>
    /// <param name="chunkX">The x-coordinate of the chunk</param>
    /// <param name="chunkZ">The z-coordinate of the chunk</param>
    public void DecorateTerrain(IChunkSource source, int chunkX, int chunkZ)
    {
        BlockSand.fallInstantly = true;
        int blockX = chunkX * 16;
        int blockZ = chunkZ * 16;
        Biome chunkBiome = _biomeSource.GetBiome(blockX + 16, blockZ + 16);
        _random.SetSeed(_level.Seed);
        long xOffset = _random.NextLong() / 2L * 2L + 1L;
        long zOffset = _random.NextLong() / 2L * 2L + 1L;
        _random.SetSeed((chunkX * xOffset + chunkZ * zOffset) ^ _level.Seed);
        int featureX;
        int featureY;
        int featureZ;
        bool hasGeneratedVillage;
        _mineshaftGenerator.GenerateStructuresInChunk(_level, _random, chunkX, chunkZ);
        hasGeneratedVillage = _villageGenerator.GenerateStructuresInChunk(_level, _random, chunkX, chunkZ);
        _strongholdGenerator.GenerateStructuresInChunk(_level, _random, chunkX, chunkZ);

        if (!hasGeneratedVillage && _random.NextInt(4) == 0)
        {
            featureX = blockX + _random.NextInt(16) + 8;
            featureY = _random.NextInt(128);
            featureZ = blockZ + _random.NextInt(16) + 8;
            _featureWaterLake.Generate(_level, _random, featureX, featureY, featureZ);
        }

        if (!hasGeneratedVillage && _random.NextInt(8) == 0)
        {
            featureX = blockX + _random.NextInt(16) + 8;
            featureY = _random.NextInt(_random.NextInt(120) + 8);
            featureZ = blockZ + _random.NextInt(16) + 8;
            if (featureY < 64 || _random.NextInt(10) == 0)
            {
                _featureLavaLake.Generate(_level, _random, featureX, featureY, featureZ);
            }
        }

        chunkBiome.Decorate(_level, _random, blockX, blockZ);

        NaturalSpawner.SpawnChunkAnimals(_level, chunkBiome, blockX + 8, blockZ + 8, 16, 16, _random);

        blockX += 8;
        blockZ += 8;
        _temperatures = _biomeSource.GetTemperatures(_temperatures, blockX, blockZ, 16, 16);

        for (int x = blockX; x < blockX + 16; ++x)
        {
            for (int z = blockZ; z < blockZ + 16; ++z)
            {
                int offsetX = x - blockX;
                int offsetZ = z - blockZ;
                int precipitationY = _level.Reader.GetTopSolidBlockY(x, z);
                if (_level.CanBlockFreeze(x, precipitationY - 1, z))
                {
                    _level.Writer.SetBlock(x, precipitationY - 1, z, Block.Ice.id, 0, doUpdate: false);
                }

                if (_level.CanSnowAt(x, precipitationY, z))
                {
                    _level.Writer.SetBlock(x, precipitationY, z, Block.Snow.id, 0, doUpdate: false);
                }
            }
        }

        BlockSand.fallInstantly = false;
    }

    public bool Save(bool saveEntities, LoadingDisplay display) => true;

    public bool Tick() => false;

    public bool CanSave() => true;

    public string GetDebugInfo() => "RandomLevelSource";

    public Vec3i? FindNearestStructure(string structureId, int x, int y, int z)
    {
        return structureId switch
        {
            "Stronghold" => _strongholdGenerator.FindNearestStructure(_level, x, y, z),
            "Village" => _villageGenerator.FindNearestStructure(_level, x, y, z),
            "Mineshaft" => _mineshaftGenerator.FindNearestStructure(_level, x, y, z),
            _ => null,
        };
    }

    private void InitFeatures()
    {
        _featureWaterLake = new LakeFeature(Block.Water.id);
        _featureLavaLake = new LakeFeature(Block.Lava.id);
        _featureDungeon = new DungeonFeature();
        _featureClay = new ClayOreFeature(32);
        _featureDirt = new OreFeature(Block.Dirt.id, 32);
        _featureGravel = new OreFeature(Block.Gravel.id, 32);
        _featureCoal = new OreFeature(Block.CoalOre.id, 16);
        _featureIron = new OreFeature(Block.IronOre.id, 8);
        _featureGold = new OreFeature(Block.GoldOre.id, 8);
        _featureRedstone = new OreFeature(Block.RedstoneOre.id, 7);
        _featureDiamond = new OreFeature(Block.DiamondOre.id, 7);
        _featureLapis = new OreFeature(Block.LapisOre.id, 6);
        _featureDandelion = new PlantPatchFeature(Block.Dandelion.id);
        _featureGrass1 = new GrassPatchFeature(Block.Grass.id, 1);
        _featureGrass2 = new GrassPatchFeature(Block.Grass.id, 2);
        _featureDeadBush = new DeadBushPatchFeature(Block.DeadBush.id);
        _featureRose = new PlantPatchFeature(Block.Rose.id);
        _featureBrownMushroom = new PlantPatchFeature(Block.BrownMushroom.id);
        _featureRedMushroom = new PlantPatchFeature(Block.RedMushroom.id);
        _featureSugarcane = new SugarCanePatchFeature();
        _featurePumpkin = new PumpkinPatchFeature();
        _featureCactus = new CactusPatchFeature();
        _featureWaterSpring = new SpringFeature(Block.FlowingWater.id);
        _featureLavaSpring = new SpringFeature(Block.FlowingLava.id);
    }

    private void InitBiomeWeights()
    {
        for (int x = -2; x <= 2; ++x)
        {
            for (int z = -2; z <= 2; ++z)
            {
                _biomeWeights[x + 2 + (z + 2) * 5] = 10.0F / MathHelper.Sqrt(x * x + z * z + 0.2F);
            }
        }
    }

    /// <summary>
    ///     Generate the base terrain
    /// </summary>
    /// <param name="chunkX">X-Coordinate of this chunk</param>
    /// <param name="chunkZ">Z-Coordinate of this chunk</param>
    /// <param name="blocks">1D Array of Blocks within this chunk</param>
    /// <param name="biomes">1D Array of Biome values within this chunk</param>
    /// <param name="temperatures">1D Array of Temperature values within this chunk</param>
    /// <returns>The interpolated result.</returns>
    public void BuildTerrain(int chunkX, int chunkZ, byte[] blocks, Biome[] biomes)
    {
        const byte horiScale = 4; // ChunkWidth / 4 = 4
        const byte seaLevel = 63;
        const int xMax = horiScale + 1; // ChunkWidth / 4 + 1
        const byte yMax = 17; // ChunkHeight / 8 + 1
        const int zMax = horiScale + 1; // ChunkWidth / 4 + 1

        _heightMap = GenerateHeightMap(_heightMap, chunkX * horiScale, 0, chunkZ * horiScale, xMax, yMax, zMax);

        for (int sampleX = 0; sampleX < horiScale; ++sampleX)
        {
            for (int sampleZ = 0; sampleZ < horiScale; ++sampleZ)
            {
                for (int sampleY = 0; sampleY < 16; ++sampleY)
                {
                    const double verticalLerpStep = 0.125D;
                    double corner000 = _heightMap[((sampleX + 0) * zMax + sampleZ + 0) * yMax + sampleY + 0];
                    double corner010 = _heightMap[((sampleX + 0) * zMax + sampleZ + 1) * yMax + sampleY + 0];
                    double corner100 = _heightMap[((sampleX + 1) * zMax + sampleZ + 0) * yMax + sampleY + 0];
                    double corner110 = _heightMap[((sampleX + 1) * zMax + sampleZ + 1) * yMax + sampleY + 0];
                    double corner001 = (_heightMap[((sampleX + 0) * zMax + sampleZ + 0) * yMax + sampleY + 1] - corner000) * verticalLerpStep;
                    double corner011 = (_heightMap[((sampleX + 0) * zMax + sampleZ + 1) * yMax + sampleY + 1] - corner010) * verticalLerpStep;
                    double corner101 = (_heightMap[((sampleX + 1) * zMax + sampleZ + 0) * yMax + sampleY + 1] - corner100) * verticalLerpStep;
                    double corner111 = (_heightMap[((sampleX + 1) * zMax + sampleZ + 1) * yMax + sampleY + 1] - corner110) * verticalLerpStep;

                    for (int subY = 0; subY < 8; ++subY)
                    {
                        const double horizontalLerpStep = 0.25D;
                        double terrainX0 = corner000;
                        double terrainX1 = corner010;
                        double terrainStepX0 = (corner100 - corner000) * horizontalLerpStep;
                        double terrainStepX1 = (corner110 - corner010) * horizontalLerpStep;

                        for (int subX = 0; subX < 4; ++subX)
                        {
                            int blockIndex = ((subX + sampleX * 4) << 11) | ((sampleZ * 4) << 7) | (sampleY * 8 + subY);
                            const short chunkHeight = 128; // Chunk Height
                            double terrainDensity = terrainX0;
                            double densityStepZ = (terrainX1 - terrainX0) * horizontalLerpStep;

                            for (int subZ = 0; subZ < 4; ++subZ)
                            {
                                int blockType = 0;
                                if (sampleY * 8 + subY < seaLevel)
                                {
                                    blockType = Block.Water.id;
                                }

                                if (terrainDensity > 0.0D)
                                {
                                    blockType = Block.Stone.id;
                                }

                                blocks[blockIndex] = (byte)blockType;
                                blockIndex += chunkHeight;
                                terrainDensity += densityStepZ;
                            }

                            terrainX0 += terrainStepX0;
                            terrainX1 += terrainStepX1;
                        }

                        corner000 += corner001;
                        corner010 += corner011;
                        corner100 += corner101;
                        corner110 += corner111;
                    }
                }
            }
        }
    }

    /// <summary>
    ///     Generate the base terrain
    /// </summary>
    /// <param name="chunkX">X-Coordinate of this chunk</param>
    /// <param name="chunkZ">Z-Coordinate of this chunk</param>
    /// <param name="blocks">1D Array of Blocks within this chunk</param>
    /// <param name="biomes">1D Array of Biome values within this chunk</param>
    /// <returns>The interpolated result.</returns>
    public void BuildSurfaces(int chunkX, int chunkZ, byte[] blocks, Biome[] biomes)
    {
        const byte seaLevel = 63;
        double chunkBiome = 1.0D / 32.0D;
        _depthBuffer = _depthNoise.create(_depthBuffer, chunkX * 16, chunkZ * 16, 0.0D, 16, 16, 1, chunkBiome * 2.0D, chunkBiome * 2.0D, chunkBiome * 2.0D);

        for (int horizontalScale = 0; horizontalScale < 16; ++horizontalScale)
        {
            for (int zOffset = 0; zOffset < 16; ++zOffset)
            {
                Biome verticalScale = biomes[horizontalScale + zOffset * 16];
                float temperature = (float)_biomeSource.GetTemperature(chunkX * 16 + horizontalScale, chunkZ * 16 + zOffset);
                int featureX = (int)(_depthBuffer[horizontalScale + zOffset * 16] / 3.0D + 3.0D + _random.NextDouble() * 0.25D);
                int featureY = -1;
                byte featureZ = verticalScale.TopBlockId;
                byte scaleFraction = verticalScale.SoilBlockId;

                for (int iX = 127; iX >= 0; --iX)
                {
                    int treeFeature = (zOffset * 16 + horizontalScale) * 128 + iX;
                    if (iX <= 0 + _random.NextInt(5))
                    {
                        blocks[treeFeature] = (byte)Block.Bedrock.id;
                    }
                    else
                    {
                        byte z = blocks[treeFeature];
                        if (z == 0)
                        {
                            featureY = -1;
                        }
                        else if (z == Block.Stone.id)
                        {
                            if (featureY == -1)
                            {
                                if (featureX <= 0)
                                {
                                    featureZ = 0;
                                    scaleFraction = (byte)Block.Stone.id;
                                }
                                else if (iX >= seaLevel - 4 && iX <= seaLevel + 1)
                                {
                                    featureZ = verticalScale.TopBlockId;
                                    scaleFraction = verticalScale.SoilBlockId;
                                }

                                if (iX < seaLevel && featureZ == 0)
                                {
                                    featureZ = temperature < 0.15F ? (byte)Block.Ice.id : (byte)Block.Water.id;
                                }

                                featureY = featureX;
                                if (iX >= seaLevel - 1)
                                {
                                    blocks[treeFeature] = featureZ;
                                }
                                else
                                {
                                    blocks[treeFeature] = scaleFraction;
                                }
                            }
                            else if (featureY > 0)
                            {
                                --featureY;
                                blocks[treeFeature] = scaleFraction;
                                if (featureY == 0 && scaleFraction == Block.Sand.id)
                                {
                                    featureY = _random.NextInt(4);
                                    scaleFraction = (byte)Block.Sandstone.id;
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    /// @brief Generates the low-resolution height map that is used to generate the terrain of the overworld. The height map is generated by sampling 5 different noise maps and applying biome-dependent modifications to them.
    ///
    /// @param terrainMap The terrain map that the scaled-down terrain values will be written to
    /// @param chunkPos The x,y,z coordinate of the sub-chunk
    /// @param max Defines the area of the terrainMap
    /// <summary>
    ///     Generates the low-resolution height map that is used to generate the terrain of the overworld. The height map is
    ///     generated by sampling 5 different noise maps and applying biome-dependent modifications to them.
    /// </summary>
    /// <param name="heightMap">The terrain map that the scaled-down terrain values will be written to</param>
    /// <param name="x">The x-coordinate of the sub-chunk</param>
    /// <param name="y">The y-coordinate of the sub-chunk</param>
    /// <param name="z">The z-coordinate of the sub-chunk</param>
    /// <param name="sizeX">The x-size of the terrainMap</param>
    /// <param name="sizeY">The y-size of the terrainMap</param>
    /// <param name="sizeZ">The z-size of the terrainMap</param>
    /// <returns>The generated height map</returns>
    private double[] GenerateHeightMap(double[]? heightMap, int x, int y, int z, int sizeX, int sizeY, int sizeZ)
    {
        if (heightMap == null)
        {
            heightMap = new double[sizeX * sizeY * sizeZ];
        }

        if (_heightMap == null)
        {
            _heightMap = new double[sizeX * sizeY * sizeZ];
        }

        double horizontalScale = 684.412D;
        double verticalScale = 684.412D;
        _scaleNoiseBuffer = _floatingIslandScale.create(_scaleNoiseBuffer, x, z, sizeX, sizeZ, 1.121D, 1.121D, 0.5D);
        _depthNoiseBuffer = _floatingIslandNoise.create(_depthNoiseBuffer, x, z, sizeX, sizeZ, 200.0D, 200.0D, 0.5D);
        _selectorNoiseBuffer = _selectorNoise.create(_selectorNoiseBuffer, x, y, z, sizeX, sizeY, sizeZ, horizontalScale / 80.0D, verticalScale / 160.0D, horizontalScale / 80.0D);
        _minLimitPerlinNoiseBuffer = _minLimitPerlinNoise.create(_minLimitPerlinNoiseBuffer, x, y, z, sizeX, sizeY, sizeZ, horizontalScale, verticalScale, horizontalScale);
        _maxLimitPerlinNoiseBuffer = _maxLimitPerlinNoise.create(_maxLimitPerlinNoiseBuffer, x, y, z, sizeX, sizeY, sizeZ, horizontalScale, verticalScale, horizontalScale);
        int noiseIndex = 0;
        int biomeIndex = 0;

        for (int sampleX = 0; sampleX < sizeX; ++sampleX)
        {
            for (int sampleZ = 0; sampleZ < sizeZ; ++sampleZ)
            {
                float rootHeight = 0.0F;
                float heightVariation = 0.0F;
                float totalWeight = 0.0F;
                Biome centerBiome = _generationBiomes[sampleX + 2 + (sampleZ + 2) * (sizeX + 5)];

                for (int offsetX = -2; offsetX <= 2; ++offsetX)
                {
                    for (int offsetZ = -2; offsetZ <= 2; ++offsetZ)
                    {
                        Biome biome = _generationBiomes[sampleX + offsetX + 2 + (sampleZ + offsetZ + 2) * (sizeX + 5)];
                        float weight = _biomeWeights[offsetX + 2 + (offsetZ + 2) * 5] / (biome.RootHeight + 2.0F);
                        if (biome.RootHeight > centerBiome.RootHeight)
                        {
                            weight /= 2.0F;
                        }

                        rootHeight += biome.HeightVariation * weight;
                        heightVariation += biome.RootHeight * weight;
                        totalWeight += weight;
                    }
                }

                rootHeight /= totalWeight;
                heightVariation /= totalWeight;
                rootHeight = rootHeight * 0.9F + 0.1F;
                heightVariation = (heightVariation * 4.0F - 1.0F) / 8.0F;
                double depthNoise = _depthNoiseBuffer[biomeIndex] / 8000.0D;
                if (depthNoise < 0.0D)
                {
                    depthNoise = -depthNoise * 0.3D;
                }

                depthNoise = depthNoise * 3.0D - 2.0D;
                if (depthNoise < 0.0D)
                {
                    depthNoise /= 2.0D;
                    if (depthNoise < -1.0D)
                    {
                        depthNoise = -1.0D;
                    }

                    depthNoise /= 1.4D;
                    depthNoise /= 2.0D;
                }
                else
                {
                    if (depthNoise > 1.0D)
                    {
                        depthNoise = 1.0D;
                    }

                    depthNoise /= 8.0D;
                }

                ++biomeIndex;

                for (int sampleY = 0; sampleY < sizeY; ++sampleY)
                {
                    double localHeightVariation = heightVariation;
                    double localRootHeight = rootHeight;
                    localHeightVariation += depthNoise * 0.2D;
                    localHeightVariation = localHeightVariation * sizeY / 16.0D;
                    double surface = sizeY / 2.0D + localHeightVariation * 4.0D;
                    double densityOffset = ((sampleY - surface) * 12.0D * 128.0D) / 128.0D / localRootHeight;
                    if (densityOffset < 0.0D)
                    {
                        densityOffset *= 4.0D;
                    }

                    double lower = _minLimitPerlinNoiseBuffer[noiseIndex] / 512.0D;
                    double upper = _maxLimitPerlinNoiseBuffer[noiseIndex] / 512.0D;
                    double alpha = (_selectorNoiseBuffer[noiseIndex] / 10.0D + 1.0D) / 2.0D;
                    double density = alpha < 0.0D ? lower : alpha > 1.0D ? upper : lower + (upper - lower) * alpha;
                    density -= densityOffset;
                    if (sampleY > sizeY - 4)
                    {
                        double fade = (sampleY - (sizeY - 4)) / 3.0F;
                        density = density * (1.0D - fade) + -10.0D * fade;
                    }

                    heightMap[noiseIndex] = density;
                    ++noiseIndex;
                }
            }
        }

        return heightMap;
    }
}
