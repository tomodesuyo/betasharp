using System.Collections.Concurrent;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Core.Systems;
using BetaSharp.Worlds.Generation.Biomes;

namespace BetaSharp.Worlds.Biomes.Source;

public class BiomeSource
{
    public static readonly ConcurrentBag<BiomeSource> Pool = [];
    private static readonly IReadOnlyList<Biome> s_biomesToSpawnIn =
    [
        Biome.Forest,
        Biome.Plains,
        Biome.Taiga,
        Biome.TaigaHills,
        Biome.ForestHills,
    ];

    private LegacyGenLayer _biomeLayer = null!;
    private LegacyGenLayer _voronoiLayer = null!;
    private LegacyGenLayer _temperatureLayer = null!;
    private LegacyGenLayer _downfallLayer = null!;
    private LegacyBiomeCache _cache = null!;
    public double[] TemperatureMap;
    public double[] DownfallMap;
    public Biome[] Biomes;

    protected BiomeSource()
    {
    }

    public BiomeSource(IWorldContext world) => Restore(world);

    public void Restore(IWorldContext world)
    {
        LegacyGenLayer[] layers = LegacyGenLayer.Build(world.Seed);
        _biomeLayer = layers[0];
        _voronoiLayer = layers[1];
        _temperatureLayer = layers[2];
        _downfallLayer = layers[3];
        _cache = new LegacyBiomeCache(this);
    }

    public virtual Biome GetBiome(ChunkPos chunkPos)
    {
        return GetBiome(chunkPos.X << 4, chunkPos.Z << 4);
    }

    public virtual Biome GetBiome(int x, int z) => _cache.GetBiome(x, z);

    public virtual double GetTemperature(int x, int z) => _cache.GetTemperature(x, z);
    public virtual double GetTemperature(int x, int y, int z) => GetTemperature(x, z);
    public virtual double GetDownfall(int x, int z) => _cache.GetDownfall(x, z);

    public virtual IReadOnlyList<Biome> GetBiomesToSpawnIn() => s_biomesToSpawnIn;

    public virtual Biome[] GetBiomesInArea(int x, int z, int width, int depth)
    {
        Biomes = GetBiomesInArea(Biomes, x, z, width, depth, true);
        return Biomes;
    }

    public virtual double[] GetTemperatures(double[] map, int x, int z, int width, int depth)
    {
        LegacyIntCache.Reset();
        int size = width * depth;
        if (map == null || map.Length < size)
        {
            map = new double[size];
        }

        int[] raw = _temperatureLayer.GetValues(x, z, width, depth);
        for (int i = 0; i < size; ++i)
        {
            map[i] = Math.Min(raw[i] / 65536.0D, 1.0D);
        }

        return map;
    }

    internal virtual double[] GetDownfall(double[]? map, int x, int z, int width, int depth)
    {
        int size = width * depth;
        if (map == null || map.Length < size)
        {
            map = new double[size];
        }

        LegacyIntCache.Reset();
        int[] raw = _downfallLayer.GetValues(x, z, width, depth);
        for (int i = 0; i < size; ++i)
        {
            map[i] = Math.Min(raw[i] / 65536.0D, 1.0D);
        }

        return map;
    }

    public virtual Biome[] GetBiomesForGeneration(Biome[]? biomes, int x, int z, int width, int depth)
    {
        LegacyIntCache.Reset();
        int size = width * depth;
        if (biomes == null || biomes.Length < size)
        {
            biomes = new Biome[size];
        }

        int[] raw = _biomeLayer.GetValues(x, z, width, depth);
        for (int i = 0; i < size; ++i)
        {
            biomes[i] = ResolveLegacyBiome(raw[i]);
        }

        return biomes;
    }

    public virtual Biome[] GetBiomesInArea(Biome[]? biomes, int x, int z, int width, int depth)
    {
        return GetBiomesInArea(biomes, x, z, width, depth, true);
    }

    internal virtual Biome[] GetBiomesInArea(Biome[]? biomes, int x, int z, int width, int depth, bool useCache)
    {
        LegacyIntCache.Reset();
        int size = width * depth;
        if (biomes == null || biomes.Length < size)
        {
            biomes = new Biome[size];
        }

        if (useCache && width == 16 && depth == 16 && (x & 15) == 0 && (z & 15) == 0)
        {
            Biome[] cached = _cache.GetBiomes(x, z);
            Array.Copy(cached, 0, biomes, 0, size);
            TemperatureMap = GetTemperatures(TemperatureMap, x, z, width, depth);
            DownfallMap = GetDownfall(DownfallMap, x, z, width, depth);
            return biomes;
        }

        int[] raw = _voronoiLayer.GetValues(x, z, width, depth);
        for (int i = 0; i < size; ++i)
        {
            biomes[i] = ResolveLegacyBiome(raw[i]);
        }

        TemperatureMap = GetTemperatures(TemperatureMap, x, z, width, depth);
        DownfallMap = GetDownfall(DownfallMap, x, z, width, depth);
        return biomes;
    }

    public void CleanupCache() => _cache.Cleanup();

    public virtual bool AreBiomesViable(int x, int z, int radius, ICollection<Biome> allowed)
    {
        int minX = x - radius >> 2;
        int minZ = z - radius >> 2;
        int maxX = x + radius >> 2;
        int maxZ = z + radius >> 2;
        int width = maxX - minX + 1;
        int depth = maxZ - minZ + 1;
        LegacyIntCache.Reset();
        int[] raw = _biomeLayer.GetValues(minX, minZ, width, depth);

        for (int i = 0; i < width * depth; ++i)
        {
            if (!allowed.Contains(ResolveLegacyBiome(raw[i])))
            {
                return false;
            }
        }

        return true;
    }

    public virtual Vec3i? FindBiomePosition(int x, int z, int radius, ICollection<Biome> allowed, JavaRandom random)
    {
        int minX = x - radius >> 2;
        int minZ = z - radius >> 2;
        int maxX = x + radius >> 2;
        int maxZ = z + radius >> 2;
        int width = maxX - minX + 1;
        int depth = maxZ - minZ + 1;
        LegacyIntCache.Reset();
        int[] raw = _biomeLayer.GetValues(minX, minZ, width, depth);
        Vec3i? result = null;
        int found = 0;

        for (int i = 0; i < raw.Length; ++i)
        {
            int biomeX = (minX + i % width) << 2;
            int biomeZ = (minZ + i / width) << 2;
            Biome biome = ResolveLegacyBiome(raw[i]);
            if (allowed.Contains(biome) && (result == null || random.NextInt(found + 1) == 0))
            {
                result = new Vec3i(biomeX, 0, biomeZ);
                ++found;
            }
        }

        return result;
    }

    internal static Biome ResolveLegacyBiome(int id)
    {
        return id switch
        {
            LegacyBiomeIds.Ocean => Biome.Ocean,
            LegacyBiomeIds.Plains => Biome.Plains,
            LegacyBiomeIds.Desert => Biome.Desert,
            LegacyBiomeIds.ExtremeHills => Biome.ExtremeHills,
            LegacyBiomeIds.Forest => Biome.Forest,
            LegacyBiomeIds.Taiga => Biome.Taiga,
            LegacyBiomeIds.Swampland => Biome.Swampland,
            LegacyBiomeIds.River => Biome.River,
            LegacyBiomeIds.Hell => Biome.Hell,
            LegacyBiomeIds.Sky => Biome.Sky,
            LegacyBiomeIds.FrozenOcean => Biome.FrozenOcean,
            LegacyBiomeIds.FrozenRiver => Biome.FrozenRiver,
            LegacyBiomeIds.IcePlains => Biome.IcePlains,
            LegacyBiomeIds.IceMountains => Biome.IceMountains,
            LegacyBiomeIds.MushroomIsland => Biome.MushroomIsland,
            LegacyBiomeIds.MushroomIslandShore => Biome.MushroomIslandShore,
            LegacyBiomeIds.Beach => Biome.Beach,
            LegacyBiomeIds.DesertHills => Biome.DesertHills,
            LegacyBiomeIds.ForestHills => Biome.ForestHills,
            LegacyBiomeIds.TaigaHills => Biome.TaigaHills,
            LegacyBiomeIds.ExtremeHillsEdge => Biome.ExtremeHillsEdge,
            _ => Biome.Plains,
        };
    }
}
