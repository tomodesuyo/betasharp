using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Generation.Biomes;

namespace BetaSharp.Worlds.Biomes.Source;

internal static class LegacyBiomeIds
{
    public const int Ocean = 0;
    public const int Plains = 1;
    public const int Desert = 2;
    public const int ExtremeHills = 3;
    public const int Forest = 4;
    public const int Taiga = 5;
    public const int Swampland = 6;
    public const int River = 7;
    public const int Hell = 8;
    public const int Sky = 9;
    public const int FrozenOcean = 10;
    public const int FrozenRiver = 11;
    public const int IcePlains = 12;
    public const int IceMountains = 13;
    public const int MushroomIsland = 14;
    public const int MushroomIslandShore = 15;
    public const int Beach = 16;
    public const int DesertHills = 17;
    public const int ForestHills = 18;
    public const int TaigaHills = 19;
    public const int ExtremeHillsEdge = 20;
    public const int Jungle = 21;
    public const int JungleHills = 22;
}

internal static class LegacyIntCache
{
    [ThreadStatic] private static LegacyIntCacheState? s_state;

    private sealed class LegacyIntCacheState
    {
        public int LargeSize = 256;
        public readonly List<int[]> FreeSmall = [];
        public readonly List<int[]> InUseSmall = [];
        public readonly List<int[]> FreeLarge = [];
        public readonly List<int[]> InUseLarge = [];
    }

    private static LegacyIntCacheState State => s_state ??= new LegacyIntCacheState();

    public static int[] Get(int size)
    {
        LegacyIntCacheState state = State;
        if (size <= 256)
        {
            if (state.FreeSmall.Count == 0)
            {
                int[] created = new int[256];
                state.InUseSmall.Add(created);
                return created;
            }

            int[] reused = state.FreeSmall[^1];
            state.FreeSmall.RemoveAt(state.FreeSmall.Count - 1);
            state.InUseSmall.Add(reused);
            return reused;
        }

        if (size > state.LargeSize)
        {
            state.LargeSize = size;
            state.FreeLarge.Clear();
            state.InUseLarge.Clear();
            int[] created = new int[state.LargeSize];
            state.InUseLarge.Add(created);
            return created;
        }

        if (state.FreeLarge.Count == 0)
        {
            int[] created = new int[state.LargeSize];
            state.InUseLarge.Add(created);
            return created;
        }

        int[] reusedLarge = state.FreeLarge[^1];
        state.FreeLarge.RemoveAt(state.FreeLarge.Count - 1);
        state.InUseLarge.Add(reusedLarge);
        return reusedLarge;
    }

    public static void Reset()
    {
        LegacyIntCacheState state = State;
        if (state.FreeLarge.Count > 0)
        {
            state.FreeLarge.RemoveAt(state.FreeLarge.Count - 1);
        }

        if (state.FreeSmall.Count > 0)
        {
            state.FreeSmall.RemoveAt(state.FreeSmall.Count - 1);
        }

        state.FreeLarge.AddRange(state.InUseLarge);
        state.FreeSmall.AddRange(state.InUseSmall);
        state.InUseLarge.Clear();
        state.InUseSmall.Clear();
    }
}

internal abstract class LegacyGenLayer
{
    private long _worldGenSeed;
    private long _chunkSeed;
    private readonly long _baseSeed;

    protected LegacyGenLayer? Parent;

    protected LegacyGenLayer(long seed)
    {
        _baseSeed = seed;
        _baseSeed *= _baseSeed * 6364136223846793005L + 1442695040888963407L;
        _baseSeed += seed;
        _baseSeed *= _baseSeed * 6364136223846793005L + 1442695040888963407L;
        _baseSeed += seed;
        _baseSeed *= _baseSeed * 6364136223846793005L + 1442695040888963407L;
        _baseSeed += seed;
    }

    public static LegacyGenLayer[] Build(long seed)
    {
        LegacyGenLayer layer = new LegacyLayerIsland(1L);
        layer = new LegacyGenLayerZoomFuzzy(2000L, layer);
        layer = new LegacyGenLayerIsland(1L, layer);
        layer = new LegacyGenLayerZoom(2001L, layer);
        layer = new LegacyGenLayerIsland(2L, layer);
        layer = new LegacyGenLayerSnow(2L, layer);
        layer = new LegacyGenLayerZoom(2002L, layer);
        layer = new LegacyGenLayerIsland(3L, layer);
        layer = new LegacyGenLayerZoom(2003L, layer);
        layer = new LegacyGenLayerIsland(4L, layer);
        layer = new LegacyGenLayerMushroomIsland(5L, layer);

        const int biomeSize = 4;
        LegacyGenLayer riverBase = LegacyGenLayerZoom.Apply(1000L, layer, 0);
        LegacyGenLayer riverInit = new LegacyGenLayerRiverInit(100L, riverBase);
        riverBase = LegacyGenLayerZoom.Apply(1000L, riverInit, biomeSize + 2);
        LegacyGenLayer river = new LegacyGenLayerRiver(1L, riverBase);
        LegacyGenLayer smoothRiver = new LegacyGenLayerSmooth(1000L, river);

        LegacyGenLayer biomeBase = LegacyGenLayerZoom.Apply(1000L, layer, 0);
        LegacyGenLayer villageLandscape = new LegacyGenLayerVillageLandscape(200L, biomeBase);
        LegacyGenLayer biomeLayer = LegacyGenLayerZoom.Apply(1000L, villageLandscape, 2);
        biomeLayer = new LegacyGenLayerHills(1000L, biomeLayer);
        LegacyGenLayer temperature = new LegacyGenLayerTemperature(biomeLayer);
        LegacyGenLayer downfall = new LegacyGenLayerDownfall(biomeLayer);

        for (int i = 0; i < biomeSize; ++i)
        {
            biomeLayer = new LegacyGenLayerZoom(1000L + i, biomeLayer);
            if (i == 0)
            {
                biomeLayer = new LegacyGenLayerIsland(3L, biomeLayer);
            }

            if (i == 1)
            {
                biomeLayer = new LegacyGenLayerShore(1000L, biomeLayer);
                biomeLayer = new LegacyGenLayerSwampRivers(1000L, biomeLayer);
            }

            LegacyGenLayer smoothTemperature = new LegacyGenLayerSmoothZoom(1000L + i, temperature);
            temperature = new LegacyGenLayerTemperatureMix(smoothTemperature, biomeLayer, i);
            LegacyGenLayer smoothDownfall = new LegacyGenLayerSmoothZoom(1000L + i, downfall);
            downfall = new LegacyGenLayerDownfallMix(smoothDownfall, biomeLayer, i);
        }

        LegacyGenLayer smoothBiome = new LegacyGenLayerSmooth(1000L, biomeLayer);
        LegacyGenLayer riverMix = new LegacyGenLayerRiverMix(100L, smoothBiome, smoothRiver);
        LegacyGenLayer smoothTemperatureFinal = LegacyGenLayerSmoothZoom.Apply(1000L, temperature, 2);
        LegacyGenLayer smoothDownfallFinal = LegacyGenLayerSmoothZoom.Apply(1000L, downfall, 2);
        LegacyGenLayer voronoi = new LegacyGenLayerZoomVoronoi(10L, riverMix);

        riverMix.InitWorldGenSeed(seed);
        smoothTemperatureFinal.InitWorldGenSeed(seed);
        smoothDownfallFinal.InitWorldGenSeed(seed);
        voronoi.InitWorldGenSeed(seed);
        return [riverMix, voronoi, smoothTemperatureFinal, smoothDownfallFinal];
    }

    public virtual void InitWorldGenSeed(long seed)
    {
        _worldGenSeed = seed;
        Parent?.InitWorldGenSeed(seed);
        _worldGenSeed *= _worldGenSeed * 6364136223846793005L + 1442695040888963407L;
        _worldGenSeed += _baseSeed;
        _worldGenSeed *= _worldGenSeed * 6364136223846793005L + 1442695040888963407L;
        _worldGenSeed += _baseSeed;
        _worldGenSeed *= _worldGenSeed * 6364136223846793005L + 1442695040888963407L;
        _worldGenSeed += _baseSeed;
    }

    protected void InitChunkSeed(long x, long z)
    {
        _chunkSeed = _worldGenSeed;
        _chunkSeed *= _chunkSeed * 6364136223846793005L + 1442695040888963407L;
        _chunkSeed += x;
        _chunkSeed *= _chunkSeed * 6364136223846793005L + 1442695040888963407L;
        _chunkSeed += z;
        _chunkSeed *= _chunkSeed * 6364136223846793005L + 1442695040888963407L;
        _chunkSeed += x;
        _chunkSeed *= _chunkSeed * 6364136223846793005L + 1442695040888963407L;
        _chunkSeed += z;
    }

    protected int NextInt(int bound)
    {
        int value = (int)((_chunkSeed >> 24) % bound);
        if (value < 0)
        {
            value += bound;
        }

        _chunkSeed *= _chunkSeed * 6364136223846793005L + 1442695040888963407L;
        _chunkSeed += _worldGenSeed;
        return value;
    }

    public abstract int[] GetValues(int x, int z, int width, int depth);
}

internal sealed class LegacyLayerIsland(long seed) : LegacyGenLayer(seed)
{
    public override int[] GetValues(int x, int z, int width, int depth)
    {
        int[] values = LegacyIntCache.Get(width * depth);
        for (int dz = 0; dz < depth; ++dz)
        {
            for (int dx = 0; dx < width; ++dx)
            {
                InitChunkSeed(x + dx, z + dz);
                values[dx + dz * width] = NextInt(10) == 0 ? 1 : 0;
            }
        }

        if (x > -width && x <= 0 && z > -depth && z <= 0)
        {
            values[-x + -z * width] = 1;
        }

        return values;
    }
}

internal sealed class LegacyGenLayerIsland(long seed, LegacyGenLayer parent) : LegacyGenLayer(seed)
{
    public override int[] GetValues(int x, int z, int width, int depth)
    {
        Parent ??= parent;
        int parentX = x - 1;
        int parentZ = z - 1;
        int parentWidth = width + 2;
        int parentDepth = depth + 2;
        int[] parentValues = Parent.GetValues(parentX, parentZ, parentWidth, parentDepth);
        int[] values = LegacyIntCache.Get(width * depth);

        for (int dz = 0; dz < depth; ++dz)
        {
            for (int dx = 0; dx < width; ++dx)
            {
                int northwest = parentValues[dx + (dz * parentWidth)];
                int northeast = parentValues[dx + 2 + dz * parentWidth];
                int southwest = parentValues[dx + (dz + 2) * parentWidth];
                int southeast = parentValues[dx + 2 + (dz + 2) * parentWidth];
                int center = parentValues[dx + 1 + (dz + 1) * parentWidth];
                InitChunkSeed(dx + x, dz + z);

                if (center != 0 || northwest == 0 && northeast == 0 && southwest == 0 && southeast == 0)
                {
                    values[dx + dz * width] = center != 1 || northwest == 1 && northeast == 1 && southwest == 1 && southeast == 1
                        ? center
                        : 1 - NextInt(5) / 4;
                }
                else
                {
                    values[dx + dz * width] = NextInt(3) / 2;
                }
            }
        }

        return values;
    }
}

internal class LegacyGenLayerZoom(long seed, LegacyGenLayer parent) : LegacyGenLayer(seed)
{
    public static LegacyGenLayer Apply(long seed, LegacyGenLayer layer, int zooms)
    {
        LegacyGenLayer result = layer;
        for (int i = 0; i < zooms; ++i)
        {
            result = new LegacyGenLayerZoom(seed + i, result);
        }

        return result;
    }

    public override int[] GetValues(int x, int z, int width, int depth)
    {
        Parent ??= parent;
        int parentX = x >> 1;
        int parentZ = z >> 1;
        int parentWidth = (width >> 1) + 3;
        int parentDepth = (depth >> 1) + 3;
        int[] parentValues = Parent.GetValues(parentX, parentZ, parentWidth, parentDepth);
        int[] zoomed = LegacyIntCache.Get(parentWidth * 2 * parentDepth * 2);
        int zoomWidth = parentWidth << 1;

        for (int dz = 0; dz < parentDepth - 1; ++dz)
        {
            int row = dz << 1;
            int writeIndex = row * zoomWidth;
            int northwest = parentValues[dz * parentWidth];
            int southwest = parentValues[(dz + 1) * parentWidth];

            for (int dx = 0; dx < parentWidth - 1; ++dx)
            {
                InitChunkSeed((dx + parentX) << 1, (dz + parentZ) << 1);
                int northeast = parentValues[dx + 1 + dz * parentWidth];
                int southeast = parentValues[dx + 1 + (dz + 1) * parentWidth];
                zoomed[writeIndex] = northwest;
                zoomed[writeIndex++ + zoomWidth] = Choose(northwest, southwest);
                zoomed[writeIndex] = Choose(northwest, northeast);
                zoomed[writeIndex++ + zoomWidth] = Choose4(northwest, northeast, southwest, southeast);
                northwest = northeast;
                southwest = southeast;
            }
        }

        int[] values = LegacyIntCache.Get(width * depth);
        for (int dz = 0; dz < depth; ++dz)
        {
            Array.Copy(zoomed, (dz + (z & 1)) * (parentWidth << 1) + (x & 1), values, dz * width, width);
        }

        return values;
    }

    protected virtual int Choose(int first, int second) => NextInt(2) == 0 ? first : second;

    protected virtual int Choose4(int a, int b, int c, int d)
    {
        if (b == c && c == d) return b;
        if (a == b && a == c) return a;
        if (a == b && a == d) return a;
        if (a == c && a == d) return a;
        if (a == b && c != d) return a;
        if (a == c && b != d) return a;
        if (a == d && b != c) return a;
        if (b == a && c != d) return b;
        if (b == c && a != d) return b;
        if (b == d && a != c) return b;
        if (c == a && b != d) return c;
        if (c == b && a != d) return c;
        if (c == d && a != b) return c;
        if (d == a && b != c) return c;
        if (d == b && a != c) return c;
        if (d == c && a != b) return c;
        return NextInt(4) switch
        {
            0 => a,
            1 => b,
            2 => c,
            _ => d,
        };
    }
}

internal sealed class LegacyGenLayerZoomFuzzy(long seed, LegacyGenLayer parent) : LegacyGenLayerZoom(seed, parent)
{
    protected override int Choose4(int a, int b, int c, int d)
    {
        return NextInt(4) switch
        {
            0 => a,
            1 => b,
            2 => c,
            _ => d,
        };
    }
}

internal sealed class LegacyGenLayerRiverInit(long seed, LegacyGenLayer parent) : LegacyGenLayer(seed)
{
    public override int[] GetValues(int x, int z, int width, int depth)
    {
        Parent ??= parent;
        int[] parentValues = Parent.GetValues(x, z, width, depth);
        int[] values = LegacyIntCache.Get(width * depth);

        for (int dz = 0; dz < depth; ++dz)
        {
            for (int dx = 0; dx < width; ++dx)
            {
                InitChunkSeed(dx + x, dz + z);
                values[dx + dz * width] = parentValues[dx + dz * width] > 0 ? NextInt(2) + 2 : 0;
            }
        }

        return values;
    }
}

internal sealed class LegacyGenLayerRiver(long seed, LegacyGenLayer parent) : LegacyGenLayer(seed)
{
    public override int[] GetValues(int x, int z, int width, int depth)
    {
        Parent ??= parent;
        int parentX = x - 1;
        int parentZ = z - 1;
        int parentWidth = width + 2;
        int parentDepth = depth + 2;
        int[] parentValues = Parent.GetValues(parentX, parentZ, parentWidth, parentDepth);
        int[] values = LegacyIntCache.Get(width * depth);

        for (int dz = 0; dz < depth; ++dz)
        {
            for (int dx = 0; dx < width; ++dx)
            {
                int west = parentValues[dx + (dz + 1) * parentWidth];
                int east = parentValues[dx + 2 + (dz + 1) * parentWidth];
                int north = parentValues[dx + 1 + dz * parentWidth];
                int south = parentValues[dx + 1 + (dz + 2) * parentWidth];
                int center = parentValues[dx + 1 + (dz + 1) * parentWidth];
                values[dx + dz * width] = center != 0 && west != 0 && east != 0 && north != 0 && south != 0
                    ? center == west && center == east && center == north && center == south ? -1 : LegacyBiomeIds.River
                    : LegacyBiomeIds.River;
            }
        }

        return values;
    }
}

internal sealed class LegacyGenLayerSmooth(long seed, LegacyGenLayer parent) : LegacyGenLayer(seed)
{
    public override int[] GetValues(int x, int z, int width, int depth)
    {
        Parent ??= parent;
        int parentX = x - 1;
        int parentZ = z - 1;
        int parentWidth = width + 2;
        int parentDepth = depth + 2;
        int[] parentValues = Parent.GetValues(parentX, parentZ, parentWidth, parentDepth);
        int[] values = LegacyIntCache.Get(width * depth);

        for (int dz = 0; dz < depth; ++dz)
        {
            for (int dx = 0; dx < width; ++dx)
            {
                int west = parentValues[dx + (dz + 1) * parentWidth];
                int east = parentValues[dx + 2 + (dz + 1) * parentWidth];
                int north = parentValues[dx + 1 + dz * parentWidth];
                int south = parentValues[dx + 1 + (dz + 2) * parentWidth];
                int center = parentValues[dx + 1 + (dz + 1) * parentWidth];

                if (west == east && north == south)
                {
                    InitChunkSeed(dx + x, dz + z);
                    center = NextInt(2) == 0 ? west : north;
                }
                else
                {
                    if (west == east) center = west;
                    if (north == south) center = north;
                }

                values[dx + dz * width] = center;
            }
        }

        return values;
    }
}

internal sealed class LegacyGenLayerVillageLandscape(long seed, LegacyGenLayer parent) : LegacyGenLayer(seed)
{
    private static readonly int[] s_allowedBiomes =
    [
        LegacyBiomeIds.Desert,
        LegacyBiomeIds.Forest,
        LegacyBiomeIds.ExtremeHills,
        LegacyBiomeIds.Swampland,
        LegacyBiomeIds.Plains,
        LegacyBiomeIds.Taiga,
        LegacyBiomeIds.Jungle,
    ];

    public override int[] GetValues(int x, int z, int width, int depth)
    {
        Parent ??= parent;
        int[] parentValues = Parent.GetValues(x, z, width, depth);
        int[] values = LegacyIntCache.Get(width * depth);

        for (int dz = 0; dz < depth; ++dz)
        {
            for (int dx = 0; dx < width; ++dx)
            {
                InitChunkSeed(dx + x, dz + z);
                int parentValue = parentValues[dx + dz * width];
                values[dx + dz * width] = parentValue switch
                {
                    LegacyBiomeIds.Ocean => LegacyBiomeIds.Ocean,
                    LegacyBiomeIds.MushroomIsland => LegacyBiomeIds.MushroomIsland,
                    1 => s_allowedBiomes[NextInt(s_allowedBiomes.Length)],
                    _ => LegacyBiomeIds.IcePlains,
                };
            }
        }

        return values;
    }
}

internal sealed class LegacyGenLayerSnow(long seed, LegacyGenLayer parent) : LegacyGenLayer(seed)
{
    public override int[] GetValues(int x, int z, int width, int depth)
    {
        Parent ??= parent;
        int[] parentValues = Parent.GetValues(x - 1, z - 1, width + 2, depth + 2);
        int[] values = LegacyIntCache.Get(width * depth);

        for (int dz = 0; dz < depth; ++dz)
        {
            for (int dx = 0; dx < width; ++dx)
            {
                int center = parentValues[dx + 1 + (dz + 1) * (width + 2)];
                InitChunkSeed(dx + x, dz + z);
                values[dx + dz * width] = center == 0 ? 0 : NextInt(5) == 0 ? LegacyBiomeIds.IcePlains : 1;
            }
        }

        return values;
    }
}

internal sealed class LegacyGenLayerMushroomIsland(long seed, LegacyGenLayer parent) : LegacyGenLayer(seed)
{
    public override int[] GetValues(int x, int z, int width, int depth)
    {
        Parent ??= parent;
        int[] parentValues = Parent.GetValues(x - 1, z - 1, width + 2, depth + 2);
        int[] values = LegacyIntCache.Get(width * depth);

        for (int dz = 0; dz < depth; ++dz)
        {
            for (int dx = 0; dx < width; ++dx)
            {
                int northwest = parentValues[dx + (dz * (width + 2))];
                int northeast = parentValues[dx + 2 + dz * (width + 2)];
                int southwest = parentValues[dx + (dz + 2) * (width + 2)];
                int southeast = parentValues[dx + 2 + (dz + 2) * (width + 2)];
                int center = parentValues[dx + 1 + (dz + 1) * (width + 2)];
                InitChunkSeed(dx + x, dz + z);
                values[dx + dz * width] = center == 0 && northwest == 0 && northeast == 0 && southwest == 0 && southeast == 0 && NextInt(100) == 0
                    ? LegacyBiomeIds.MushroomIsland
                    : center;
            }
        }

        return values;
    }
}

internal sealed class LegacyGenLayerHills(long seed, LegacyGenLayer parent) : LegacyGenLayer(seed)
{
    public override int[] GetValues(int x, int z, int width, int depth)
    {
        Parent ??= parent;
        int[] parentValues = Parent.GetValues(x - 1, z - 1, width + 2, depth + 2);
        int[] values = LegacyIntCache.Get(width * depth);

        for (int dz = 0; dz < depth; ++dz)
        {
            for (int dx = 0; dx < width; ++dx)
            {
                InitChunkSeed(dx + x, dz + z);
                int center = parentValues[dx + 1 + (dz + 1) * (width + 2)];
                if (NextInt(3) != 0)
                {
                    values[dx + dz * width] = center;
                    continue;
                }

                int replacement = center switch
                {
                    LegacyBiomeIds.Desert => LegacyBiomeIds.DesertHills,
                    LegacyBiomeIds.Forest => LegacyBiomeIds.ForestHills,
                    LegacyBiomeIds.Taiga => LegacyBiomeIds.TaigaHills,
                    LegacyBiomeIds.Plains => LegacyBiomeIds.Forest,
                    LegacyBiomeIds.IcePlains => LegacyBiomeIds.IceMountains,
                    LegacyBiomeIds.Jungle => LegacyBiomeIds.JungleHills,
                    _ => center,
                };

                if (replacement == center)
                {
                    values[dx + dz * width] = center;
                    continue;
                }

                int north = parentValues[dx + 1 + dz * (width + 2)];
                int east = parentValues[dx + 2 + (dz + 1) * (width + 2)];
                int west = parentValues[dx + (dz + 1) * (width + 2)];
                int south = parentValues[dx + 1 + (dz + 2) * (width + 2)];
                values[dx + dz * width] = north == center && east == center && west == center && south == center ? replacement : center;
            }
        }

        return values;
    }
}

internal sealed class LegacyGenLayerShore(long seed, LegacyGenLayer parent) : LegacyGenLayer(seed)
{
    public override int[] GetValues(int x, int z, int width, int depth)
    {
        Parent ??= parent;
        int[] parentValues = Parent.GetValues(x - 1, z - 1, width + 2, depth + 2);
        int[] values = LegacyIntCache.Get(width * depth);

        for (int dz = 0; dz < depth; ++dz)
        {
            for (int dx = 0; dx < width; ++dx)
            {
                int center = parentValues[dx + 1 + (dz + 1) * (width + 2)];
                int north = parentValues[dx + 1 + dz * (width + 2)];
                int east = parentValues[dx + 2 + (dz + 1) * (width + 2)];
                int west = parentValues[dx + (dz + 1) * (width + 2)];
                int south = parentValues[dx + 1 + (dz + 2) * (width + 2)];

                if (center == LegacyBiomeIds.MushroomIsland)
                {
                    values[dx + dz * width] = north != LegacyBiomeIds.Ocean && east != LegacyBiomeIds.Ocean && west != LegacyBiomeIds.Ocean && south != LegacyBiomeIds.Ocean
                        ? center
                        : LegacyBiomeIds.MushroomIslandShore;
                }
                else if (center == LegacyBiomeIds.ExtremeHills)
                {
                    values[dx + dz * width] = north == LegacyBiomeIds.ExtremeHills && east == LegacyBiomeIds.ExtremeHills && west == LegacyBiomeIds.ExtremeHills && south == LegacyBiomeIds.ExtremeHills
                        ? center
                        : LegacyBiomeIds.ExtremeHillsEdge;
                }
                else if (center != LegacyBiomeIds.Ocean && center != LegacyBiomeIds.River && center != LegacyBiomeIds.Swampland)
                {
                    values[dx + dz * width] = north != LegacyBiomeIds.Ocean && east != LegacyBiomeIds.Ocean && west != LegacyBiomeIds.Ocean && south != LegacyBiomeIds.Ocean
                        ? center
                        : LegacyBiomeIds.Beach;
                }
                else
                {
                    values[dx + dz * width] = center;
                }
            }
        }

        return values;
    }
}

internal sealed class LegacyGenLayerSwampRivers(long seed, LegacyGenLayer parent) : LegacyGenLayer(seed)
{
    public override int[] GetValues(int x, int z, int width, int depth)
    {
        Parent ??= parent;
        int[] parentValues = Parent.GetValues(x - 1, z - 1, width + 2, depth + 2);
        int[] values = LegacyIntCache.Get(width * depth);

        for (int dz = 0; dz < depth; ++dz)
        {
            for (int dx = 0; dx < width; ++dx)
            {
                InitChunkSeed(dx + x, dz + z);
                int center = parentValues[dx + 1 + (dz + 1) * (width + 2)];
                values[dx + dz * width] =
                    center == LegacyBiomeIds.Swampland && NextInt(6) == 0
                        ? LegacyBiomeIds.River
                        : (center == LegacyBiomeIds.Jungle || center == LegacyBiomeIds.JungleHills) && NextInt(8) == 0
                            ? LegacyBiomeIds.River
                            : center;
            }
        }

        return values;
    }
}

internal sealed class LegacyGenLayerTemperature(LegacyGenLayer parent) : LegacyGenLayer(0L)
{
    public override int[] GetValues(int x, int z, int width, int depth)
    {
        Parent ??= parent;
        int[] biomes = Parent.GetValues(x, z, width, depth);
        int[] values = LegacyIntCache.Get(width * depth);
        for (int i = 0; i < width * depth; ++i)
        {
            values[i] = BiomeSource.ResolveLegacyBiome(biomes[i]).GetTemperatureBits();
        }

        return values;
    }
}

internal sealed class LegacyGenLayerDownfall(LegacyGenLayer parent) : LegacyGenLayer(0L)
{
    public override int[] GetValues(int x, int z, int width, int depth)
    {
        Parent ??= parent;
        int[] biomes = Parent.GetValues(x, z, width, depth);
        int[] values = LegacyIntCache.Get(width * depth);
        for (int i = 0; i < width * depth; ++i)
        {
            values[i] = BiomeSource.ResolveLegacyBiome(biomes[i]).GetDownfallBits();
        }

        return values;
    }
}

internal sealed class LegacyGenLayerTemperatureMix(LegacyGenLayer temperatureLayer, LegacyGenLayer biomeLayer, int pass) : LegacyGenLayer(0L)
{
    public override int[] GetValues(int x, int z, int width, int depth)
    {
        Parent ??= biomeLayer;
        int[] biomes = Parent.GetValues(x, z, width, depth);
        int[] temperatures = temperatureLayer.GetValues(x, z, width, depth);
        int[] values = LegacyIntCache.Get(width * depth);

        for (int i = 0; i < width * depth; ++i)
        {
            values[i] = temperatures[i] + (BiomeSource.ResolveLegacyBiome(biomes[i]).GetTemperatureBits() - temperatures[i]) / (pass * 2 + 1);
        }

        return values;
    }
}

internal sealed class LegacyGenLayerDownfallMix(LegacyGenLayer downfallLayer, LegacyGenLayer biomeLayer, int pass) : LegacyGenLayer(0L)
{
    public override int[] GetValues(int x, int z, int width, int depth)
    {
        Parent ??= biomeLayer;
        int[] biomes = Parent.GetValues(x, z, width, depth);
        int[] downfall = downfallLayer.GetValues(x, z, width, depth);
        int[] values = LegacyIntCache.Get(width * depth);

        for (int i = 0; i < width * depth; ++i)
        {
            values[i] = downfall[i] + (BiomeSource.ResolveLegacyBiome(biomes[i]).GetDownfallBits() - downfall[i]) / (pass + 1);
        }

        return values;
    }
}

internal sealed class LegacyGenLayerRiverMix(long seed, LegacyGenLayer biomeLayer, LegacyGenLayer riverLayer) : LegacyGenLayer(seed)
{
    public override void InitWorldGenSeed(long seed)
    {
        biomeLayer.InitWorldGenSeed(seed);
        riverLayer.InitWorldGenSeed(seed);
        base.InitWorldGenSeed(seed);
    }

    public override int[] GetValues(int x, int z, int width, int depth)
    {
        int[] biomes = biomeLayer.GetValues(x, z, width, depth);
        int[] rivers = riverLayer.GetValues(x, z, width, depth);
        int[] values = LegacyIntCache.Get(width * depth);

        for (int i = 0; i < width * depth; ++i)
        {
            values[i] = biomes[i] == LegacyBiomeIds.Ocean
                ? biomes[i]
                : rivers[i] >= 0
                    ? biomes[i] == LegacyBiomeIds.IcePlains
                        ? LegacyBiomeIds.FrozenRiver
                        : biomes[i] == LegacyBiomeIds.MushroomIsland || biomes[i] == LegacyBiomeIds.MushroomIslandShore
                            ? LegacyBiomeIds.MushroomIslandShore
                            : rivers[i]
                    : biomes[i];
        }

        return values;
    }
}

internal sealed class LegacyGenLayerSmoothZoom(long seed, LegacyGenLayer parent) : LegacyGenLayer(seed)
{
    public static LegacyGenLayer Apply(long seed, LegacyGenLayer layer, int zooms)
    {
        LegacyGenLayer result = layer;
        for (int i = 0; i < zooms; ++i)
        {
            result = new LegacyGenLayerSmoothZoom(seed + i, result);
        }

        return result;
    }

    public override int[] GetValues(int x, int z, int width, int depth)
    {
        Parent ??= parent;
        int parentX = x >> 1;
        int parentZ = z >> 1;
        int parentWidth = (width >> 1) + 3;
        int parentDepth = (depth >> 1) + 3;
        int[] parentValues = Parent.GetValues(parentX, parentZ, parentWidth, parentDepth);
        int[] zoomed = LegacyIntCache.Get(parentWidth * 2 * parentDepth * 2);
        int zoomWidth = parentWidth << 1;

        for (int dz = 0; dz < parentDepth - 1; ++dz)
        {
            int row = dz << 1;
            int writeIndex = row * zoomWidth;
            int northwest = parentValues[dz * parentWidth];
            int southwest = parentValues[(dz + 1) * parentWidth];

            for (int dx = 0; dx < parentWidth - 1; ++dx)
            {
                InitChunkSeed((dx + parentX) << 1, (dz + parentZ) << 1);
                int northeast = parentValues[dx + 1 + dz * parentWidth];
                int southeast = parentValues[dx + 1 + (dz + 1) * parentWidth];
                zoomed[writeIndex] = northwest;
                zoomed[writeIndex++ + zoomWidth] = northwest + (southwest - northwest) * NextInt(256) / 256;
                zoomed[writeIndex] = northwest + (northeast - northwest) * NextInt(256) / 256;
                int northBlend = northwest + (northeast - northwest) * NextInt(256) / 256;
                int southBlend = southwest + (southeast - southwest) * NextInt(256) / 256;
                zoomed[writeIndex++ + zoomWidth] = northBlend + (southBlend - northBlend) * NextInt(256) / 256;
                northwest = northeast;
                southwest = southeast;
            }
        }

        int[] values = LegacyIntCache.Get(width * depth);
        for (int dz = 0; dz < depth; ++dz)
        {
            Array.Copy(zoomed, (dz + (z & 1)) * (parentWidth << 1) + (x & 1), values, dz * width, width);
        }

        return values;
    }
}

internal sealed class LegacyGenLayerZoomVoronoi(long seed, LegacyGenLayer parent) : LegacyGenLayer(seed)
{
    public override int[] GetValues(int x, int z, int width, int depth)
    {
        x -= 2;
        z -= 2;
        const int shift = 2;
        int scale = 1 << shift;
        int parentX = x >> shift;
        int parentZ = z >> shift;
        int parentWidth = (width >> shift) + 3;
        int parentDepth = (depth >> shift) + 3;
        Parent ??= parent;
        int[] parentValues = Parent.GetValues(parentX, parentZ, parentWidth, parentDepth);
        int zoomWidth = parentWidth << shift;
        int zoomDepth = parentDepth << shift;
        int[] zoomed = LegacyIntCache.Get(zoomWidth * zoomDepth);

        for (int dz = 0; dz < parentDepth - 1; ++dz)
        {
            int northwest = parentValues[dz * parentWidth];
            int southwest = parentValues[(dz + 1) * parentWidth];

            for (int dx = 0; dx < parentWidth - 1; ++dx)
            {
                double jitter = scale * 0.9D;
                InitChunkSeed((dx + parentX) << shift, (dz + parentZ) << shift);
                double nwX = (NextInt(1024) / 1024.0D - 0.5D) * jitter;
                double nwZ = (NextInt(1024) / 1024.0D - 0.5D) * jitter;
                InitChunkSeed((dx + parentX + 1) << shift, (dz + parentZ) << shift);
                double neX = (NextInt(1024) / 1024.0D - 0.5D) * jitter + scale;
                double neZ = (NextInt(1024) / 1024.0D - 0.5D) * jitter;
                InitChunkSeed((dx + parentX) << shift, (dz + parentZ + 1) << shift);
                double swX = (NextInt(1024) / 1024.0D - 0.5D) * jitter;
                double swZ = (NextInt(1024) / 1024.0D - 0.5D) * jitter + scale;
                InitChunkSeed((dx + parentX + 1) << shift, (dz + parentZ + 1) << shift);
                double seX = (NextInt(1024) / 1024.0D - 0.5D) * jitter + scale;
                double seZ = (NextInt(1024) / 1024.0D - 0.5D) * jitter + scale;
                int northeast = parentValues[dx + 1 + dz * parentWidth];
                int southeast = parentValues[dx + 1 + (dz + 1) * parentWidth];

                for (int localZ = 0; localZ < scale; ++localZ)
                {
                    int writeIndex = ((dz << shift) + localZ) * zoomWidth + (dx << shift);
                    for (int localX = 0; localX < scale; ++localX)
                    {
                        double distNorthwest = (localZ - nwZ) * (localZ - nwZ) + (localX - nwX) * (localX - nwX);
                        double distNortheast = (localZ - neZ) * (localZ - neZ) + (localX - neX) * (localX - neX);
                        double distSouthwest = (localZ - swZ) * (localZ - swZ) + (localX - swX) * (localX - swX);
                        double distSoutheast = (localZ - seZ) * (localZ - seZ) + (localX - seX) * (localX - seX);
                        zoomed[writeIndex++] = distNorthwest < distNortheast && distNorthwest < distSouthwest && distNorthwest < distSoutheast
                            ? northwest
                            : distNortheast < distNorthwest && distNortheast < distSouthwest && distNortheast < distSoutheast
                                ? northeast
                                : distSouthwest < distNorthwest && distSouthwest < distNortheast && distSouthwest < distSoutheast
                                    ? southwest
                                    : southeast;
                    }
                }

                northwest = northeast;
                southwest = southeast;
            }
        }

        int[] values = LegacyIntCache.Get(width * depth);
        for (int dz = 0; dz < depth; ++dz)
        {
            Array.Copy(zoomed, (dz + (z & (scale - 1))) * (parentWidth << shift) + (x & (scale - 1)), values, dz * width, width);
        }

        return values;
    }
}

internal sealed class LegacyBiomeCacheBlock
{
    public readonly double[] Temperature = new double[256];
    public readonly double[] Downfall = new double[256];
    public readonly Biome[] Biomes = new Biome[256];
    public readonly int ChunkX;
    public readonly int ChunkZ;
    public long LastAccessTime;

    public LegacyBiomeCacheBlock(BiomeSource biomeSource, int chunkX, int chunkZ)
    {
        ChunkX = chunkX;
        ChunkZ = chunkZ;
        biomeSource.GetTemperatures(Temperature, chunkX << 4, chunkZ << 4, 16, 16);
        biomeSource.GetDownfall(Downfall, chunkX << 4, chunkZ << 4, 16, 16);
        biomeSource.GetBiomesInArea(Biomes, chunkX << 4, chunkZ << 4, 16, 16, false);
    }
}

internal sealed class LegacyBiomeCache(BiomeSource biomeSource)
{
    private readonly Dictionary<long, LegacyBiomeCacheBlock> _entries = [];
    private readonly List<LegacyBiomeCacheBlock> _cache = [];
    private long _lastCleanupTime;

    private LegacyBiomeCacheBlock GetBlock(int x, int z)
    {
        int chunkX = x >> 4;
        int chunkZ = z >> 4;
        long key = (uint)chunkX | ((long)(uint)chunkZ << 32);
        if (!_entries.TryGetValue(key, out LegacyBiomeCacheBlock? block))
        {
            block = new LegacyBiomeCacheBlock(biomeSource, chunkX, chunkZ);
            _entries[key] = block;
            _cache.Add(block);
        }

        block.LastAccessTime = Environment.TickCount64;
        return block;
    }

    public Biome GetBiome(int x, int z) => GetBlock(x, z).Biomes[(x & 15) + ((z & 15) << 4)];
    public double GetTemperature(int x, int z) => GetBlock(x, z).Temperature[(x & 15) + ((z & 15) << 4)];
    public double GetDownfall(int x, int z) => GetBlock(x, z).Downfall[(x & 15) + ((z & 15) << 4)];
    public Biome[] GetBiomes(int x, int z) => GetBlock(x, z).Biomes;

    public void Cleanup()
    {
        long now = Environment.TickCount64;
        long elapsed = now - _lastCleanupTime;
        if (elapsed <= 7500L && elapsed >= 0L)
        {
            return;
        }

        _lastCleanupTime = now;
        for (int i = 0; i < _cache.Count; ++i)
        {
            LegacyBiomeCacheBlock block = _cache[i];
            long age = now - block.LastAccessTime;
            if (age > 30000L || age < 0L)
            {
                _cache.RemoveAt(i--);
                long key = (uint)block.ChunkX | ((long)(uint)block.ChunkZ << 32);
                _entries.Remove(key);
            }
        }
    }
}
