using BetaSharp.Blocks;
using BetaSharp.Util.Maths;
using BetaSharp.Util.Maths.Noise;
using BetaSharp.Worlds.Chunks;
using BetaSharp.Worlds.Core.Systems;
using BetaSharp.Worlds.Generation.Biomes;

namespace BetaSharp.Worlds.Gen.Chunks;

internal sealed class EndChunkGenerator : IChunkSource
{
    private readonly JavaRandom _random;
    private readonly OctavePerlinNoiseSampler _noiseGen1;
    private readonly OctavePerlinNoiseSampler _noiseGen2;
    private readonly OctavePerlinNoiseSampler _noiseGen3;
    private readonly OctavePerlinNoiseSampler _noiseGen4;
    private readonly OctavePerlinNoiseSampler _noiseGen5;
    private readonly IWorldContext _world;
    private readonly long _seed;
    private double[]? _densities;
    private double[]? _noiseData1;
    private double[]? _noiseData2;
    private double[]? _noiseData3;
    private double[]? _noiseData4;
    private double[]? _noiseData5;

    public EndChunkGenerator(IWorldContext world, long seed)
    {
        _world = world;
        _seed = seed;
        _random = new JavaRandom(seed);
        _noiseGen1 = new OctavePerlinNoiseSampler(_random, 16);
        _noiseGen2 = new OctavePerlinNoiseSampler(_random, 16);
        _noiseGen3 = new OctavePerlinNoiseSampler(_random, 8);
        _noiseGen4 = new OctavePerlinNoiseSampler(_random, 10);
        _noiseGen5 = new OctavePerlinNoiseSampler(_random, 16);
    }

    public IChunkSource CreateParallelInstance() => new EndChunkGenerator(_world, _seed);

    public bool IsChunkLoaded(int x, int z) => true;

    public Chunk LoadChunk(int x, int z) => GetChunk(x, z);

    public Chunk GetChunk(int chunkX, int chunkZ)
    {
        _random.SetSeed(chunkX * 341873128712L + chunkZ * 132897987541L);
        byte[] blocks = new byte[-short.MinValue];
        BuildTerrain(chunkX, chunkZ, blocks);
        BuildSurfaces(blocks);
        Chunk chunk = new(_world, blocks, chunkX, chunkZ);
        chunk.PopulateHeightMap();
        return chunk;
    }

    public void DecorateTerrain(IChunkSource source, int chunkX, int chunkZ)
    {
        BlockSand.fallInstantly = true;
        int blockX = chunkX * 16;
        int blockZ = chunkZ * 16;
        Biome biome = _world.Dimension.BiomeSource.GetBiome(blockX + 16, blockZ + 16);
        _random.SetSeed(_world.Seed);
        long seedX = _random.NextLong() / 2L * 2L + 1L;
        long seedZ = _random.NextLong() / 2L * 2L + 1L;
        _random.SetSeed((chunkX * seedX + chunkZ * seedZ) ^ _world.Seed);
        biome.Decorate(_world, _random, blockX, blockZ);
        BlockSand.fallInstantly = false;
    }

    public bool Save(bool saveEntities, LoadingDisplay display) => true;

    public bool Tick() => false;

    public bool CanSave() => true;

    public string GetDebugInfo() => "RandomLevelSource";

    private void BuildTerrain(int chunkX, int chunkZ, byte[] blocks)
    {
        const int horizontalScale = 2;
        const int xSize = horizontalScale + 1;
        const int ySize = 33;
        const int zSize = horizontalScale + 1;
        _densities = GenerateDensityMap(_densities, chunkX * horizontalScale, 0, chunkZ * horizontalScale, xSize, ySize, zSize);

        for (int sampleX = 0; sampleX < horizontalScale; ++sampleX)
        {
            for (int sampleZ = 0; sampleZ < horizontalScale; ++sampleZ)
            {
                for (int sampleY = 0; sampleY < 32; ++sampleY)
                {
                    const double verticalStep = 0.25D;
                    double density000 = _densities[((sampleX + 0) * zSize + sampleZ + 0) * ySize + sampleY + 0];
                    double density010 = _densities[((sampleX + 0) * zSize + sampleZ + 1) * ySize + sampleY + 0];
                    double density100 = _densities[((sampleX + 1) * zSize + sampleZ + 0) * ySize + sampleY + 0];
                    double density110 = _densities[((sampleX + 1) * zSize + sampleZ + 1) * ySize + sampleY + 0];
                    double step000 = (_densities[((sampleX + 0) * zSize + sampleZ + 0) * ySize + sampleY + 1] - density000) * verticalStep;
                    double step010 = (_densities[((sampleX + 0) * zSize + sampleZ + 1) * ySize + sampleY + 1] - density010) * verticalStep;
                    double step100 = (_densities[((sampleX + 1) * zSize + sampleZ + 0) * ySize + sampleY + 1] - density100) * verticalStep;
                    double step110 = (_densities[((sampleX + 1) * zSize + sampleZ + 1) * ySize + sampleY + 1] - density110) * verticalStep;

                    for (int subY = 0; subY < 4; ++subY)
                    {
                        const double horizontalStep = 0.125D;
                        double densityX0 = density000;
                        double densityX1 = density010;
                        double densityX0Step = (density100 - density000) * horizontalStep;
                        double densityX1Step = (density110 - density010) * horizontalStep;

                        for (int subX = 0; subX < 8; ++subX)
                        {
                            int blockIndex = ((subX + sampleX * 8) << 11) | (sampleZ * 8 << 7) | (sampleY * 4 + subY);
                            const int chunkHeight = 128;
                            const double zStep = 0.125D;
                            double density = densityX0;
                            double densityStep = (densityX1 - densityX0) * zStep;

                            for (int subZ = 0; subZ < 8; ++subZ)
                            {
                                blocks[blockIndex] = density > 0.0D ? (byte)Block.EndStone.id : (byte)0;
                                blockIndex += chunkHeight;
                                density += densityStep;
                            }

                            densityX0 += densityX0Step;
                            densityX1 += densityX1Step;
                        }

                        density000 += step000;
                        density010 += step010;
                        density100 += step100;
                        density110 += step110;
                    }
                }
            }
        }
    }

    private static void BuildSurfaces(byte[] blocks)
    {
        for (int localX = 0; localX < 16; ++localX)
        {
            for (int localZ = 0; localZ < 16; ++localZ)
            {
                int topDepth = -1;
                const int fillerDepth = 1;

                for (int y = 127; y >= 0; --y)
                {
                    int index = (localZ * 16 + localX) * 128 + y;
                    byte block = blocks[index];

                    if (block == 0)
                    {
                        topDepth = -1;
                    }
                    else if (block == Block.EndStone.id)
                    {
                        if (topDepth == -1)
                        {
                            topDepth = fillerDepth;
                            blocks[index] = (byte)Block.EndStone.id;
                        }
                        else if (topDepth > 0)
                        {
                            --topDepth;
                            blocks[index] = (byte)Block.EndStone.id;
                        }
                    }
                }
            }
        }
    }

    private double[] GenerateDensityMap(double[]? densityMap, int x, int y, int z, int sizeX, int sizeY, int sizeZ)
    {
        densityMap ??= new double[sizeX * sizeY * sizeZ];

        double horizontalScale = 684.412D;
        const double verticalScale = 684.412D;
        _noiseData4 = _noiseGen4.create(_noiseData4, x, z, sizeX, sizeZ, 1.121D, 1.121D, 0.5D);
        _noiseData5 = _noiseGen5.create(_noiseData5, x, z, sizeX, sizeZ, 200.0D, 200.0D, 0.5D);
        horizontalScale *= 2.0D;
        _noiseData1 = _noiseGen3.create(_noiseData1, x, y, z, sizeX, sizeY, sizeZ, horizontalScale / 80.0D, verticalScale / 160.0D, horizontalScale / 80.0D);
        _noiseData2 = _noiseGen1.create(_noiseData2, x, y, z, sizeX, sizeY, sizeZ, horizontalScale, verticalScale, horizontalScale);
        _noiseData3 = _noiseGen2.create(_noiseData3, x, y, z, sizeX, sizeY, sizeZ, horizontalScale, verticalScale, horizontalScale);

        int xyzIndex = 0;
        int xzIndex = 0;

        for (int localX = 0; localX < sizeX; ++localX)
        {
            for (int localZ = 0; localZ < sizeZ; ++localZ)
            {
                double scale = (_noiseData4[xzIndex] + 256.0D) / 512.0D;
                scale = Math.Min(scale, 1.0D);

                double depth = _noiseData5[xzIndex] / 8000.0D;
                if (depth < 0.0D)
                {
                    depth = -depth * 0.3D;
                }

                depth = depth * 3.0D - 2.0D;

                float offsetX = localX + x;
                float offsetZ = localZ + z;
                float islandFalloff = 100.0F - MathHelper.Sqrt(offsetX * offsetX + offsetZ * offsetZ) * 8.0F;
                islandFalloff = Math.Clamp(islandFalloff, -100.0F, 80.0F);

                depth = Math.Min(depth, 1.0D);
                depth /= 8.0D;
                depth = 0.0D;
                if (scale < 0.0D)
                {
                    scale = 0.0D;
                }

                scale += 0.5D;
                ++xzIndex;
                double halfHeight = sizeY / 2.0D;

                for (int localY = 0; localY < sizeY; ++localY)
                {
                    double lowNoise = _noiseData2[xyzIndex] / 512.0D;
                    double highNoise = _noiseData3[xyzIndex] / 512.0D;
                    double blend = (_noiseData1[xyzIndex] / 10.0D + 1.0D) / 2.0D;
                    double density = blend < 0.0D
                        ? lowNoise
                        : blend > 1.0D
                            ? highNoise
                            : lowNoise + (highNoise - lowNoise) * blend;

                    density -= 8.0D;
                    density += islandFalloff;

                    const int topFadeSize = 2;
                    if (localY > sizeY / 2 - topFadeSize)
                    {
                        double fade = (localY - (sizeY / 2.0D - topFadeSize)) / 64.0D;
                        fade = Math.Clamp(fade, 0.0D, 1.0D);
                        density = density * (1.0D - fade) + -3000.0D * fade;
                    }

                    const int bottomFadeSize = 8;
                    if (localY < bottomFadeSize)
                    {
                        double fade = (bottomFadeSize - localY) / (bottomFadeSize - 1.0D);
                        density = density * (1.0D - fade) + -30.0D * fade;
                    }

                    densityMap[xyzIndex++] = density;
                }
            }
        }

        return densityMap;
    }
}
