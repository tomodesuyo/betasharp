using BetaSharp.Blocks;
using BetaSharp.Entities;
using BetaSharp.Registries;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds.ClientData.Colors;
using BetaSharp.Worlds.Core.Systems;
using BetaSharp.Worlds.Colors;
using BetaSharp.Worlds.Generation.Generators.Features;

namespace BetaSharp.Worlds.Generation.Biomes;

public class Biome
{
    private static readonly IRegistry<Biome> s_registry = DefaultRegistries.Biomes;

    public static readonly Biome Ocean = Register(0, "ocean", new BiomeGenOcean().SetColor(112).SetName("Ocean").SetHeight(-1.0F, 0.4F));
    public static readonly Biome Plains = Register(1, "plains", new BiomeGenPlains().SetColor(9286496).SetName("Plains").SetClimate(0.8F, 0.4F));
    public static readonly Biome Desert = Register(2, "desert", new BiomeGenDesert().SetColor(16421912).SetName("Desert").DisableRain().SetClimate(2.0F, 0.0F).SetHeight(0.1F, 0.2F));
    public static readonly Biome ExtremeHills = Register(3, "extreme_hills", new Biome().SetColor(6316128).SetName("Extreme Hills").SetHeight(0.2F, 1.3F).SetClimate(0.2F, 0.3F));
    public static readonly Biome Forest = Register(4, "forest", new BiomeGenForest().SetColor(353825).SetName("Forest").SetFoliageColor(5159473).SetClimate(0.7F, 0.8F));
    public static readonly Biome Taiga = Register(5, "taiga", new BiomeGenTaiga().SetColor(747097).SetName("Taiga").SetFoliageColor(5159473).SetClimate(0.05F, 0.8F).SetHeight(0.1F, 0.4F));
    public static readonly Biome Swampland = Register(6, "swampland", new BiomeGenSwamp().SetColor(522674).SetName("Swampland").SetFoliageColor(9154376).SetHeight(-0.2F, 0.1F).SetClimate(0.8F, 0.9F));
    public static readonly Biome River = Register(7, "river", new BiomeGenRiver().SetColor(255).SetName("River").SetHeight(-0.5F, 0.0F));
    public static readonly Biome Hell = Register(8, "hell", new BiomeGenHell().SetColor(0xFF0000).SetName("Hell").DisableRain().SetClimate(2.0F, 0.0F));
    public static readonly Biome Sky = Register(9, "sky", new BiomeGenSky().SetColor(0x8080FF).SetName("Sky").DisableRain());
    public static readonly Biome FrozenOcean = Register(10, "frozen_ocean", new BiomeGenOcean().SetColor(9474208).SetName("FrozenOcean").SetHeight(-1.0F, 0.5F).SetClimate(0.0F, 0.5F));
    public static readonly Biome FrozenRiver = Register(11, "frozen_river", new BiomeGenRiver().SetColor(10526975).SetName("FrozenRiver").SetHeight(-0.5F, 0.0F).SetClimate(0.0F, 0.5F));
    public static readonly Biome IcePlains = Register(12, "ice_plains", new Biome().SetColor(0xFFFFFF).SetName("Ice Plains").EnableSnow().SetClimate(0.0F, 0.5F));
    public static readonly Biome IceMountains = Register(13, "ice_mountains", new Biome().SetColor(10526880).SetName("Ice Mountains").EnableSnow().SetHeight(0.2F, 1.2F).SetClimate(0.0F, 0.5F));
    public static readonly Biome MushroomIsland = Register(14, "mushroom_island", new Biome().SetColor(0xFF00FF).SetName("MushroomIsland").SetClimate(0.9F, 1.0F).SetHeight(0.2F, 1.0F));
    public static readonly Biome MushroomIslandShore = Register(15, "mushroom_island_shore", new Biome().SetColor(10486015).SetName("MushroomIslandShore").SetClimate(0.9F, 1.0F).SetHeight(-1.0F, 0.1F));
    public static readonly Biome Beach = Register(16, "beach", new Biome().SetColor(16440917).SetName("Beach").SetClimate(0.8F, 0.4F).SetHeight(0.0F, 0.1F));
    public static readonly Biome DesertHills = Register(17, "desert_hills", new BiomeGenDesert().SetColor(13786898).SetName("DesertHills").DisableRain().SetClimate(2.0F, 0.0F).SetHeight(0.2F, 0.7F));
    public static readonly Biome ForestHills = Register(18, "forest_hills", new BiomeGenForest().SetColor(2250012).SetName("ForestHills").SetFoliageColor(5159473).SetClimate(0.7F, 0.8F).SetHeight(0.2F, 0.6F));
    public static readonly Biome TaigaHills = Register(19, "taiga_hills", new BiomeGenTaiga().SetColor(1456435).SetName("TaigaHills").SetFoliageColor(5159473).SetClimate(0.05F, 0.8F).SetHeight(0.2F, 0.7F));
    public static readonly Biome ExtremeHillsEdge = Register(20, "extreme_hills_edge", new Biome().SetColor(7501978).SetName("Extreme Hills Edge").SetHeight(0.2F, 0.8F).SetClimate(0.2F, 0.3F));
    public static readonly Biome Jungle = Register(21, "jungle", new BiomeGenJungle().SetColor(5470985).SetName("Jungle").SetFoliageColor(5470985).SetClimate(1.2F, 0.9F).SetHeight(0.2F, 0.4F));
    public static readonly Biome JungleHills = Register(22, "jungle_hills", new BiomeGenJungle().SetColor(2900485).SetName("JungleHills").SetFoliageColor(5470985).SetClimate(1.2F, 0.9F).SetHeight(1.8F, 0.2F));
    public static readonly Biome End = Register(23, "end", new BiomeGenEnd().SetColor(0x8080FF).SetName("The End").DisableRain().SetClimate(0.5F, 0.0F).SetHeight(0.1F, 0.2F));
    public static readonly Biome Rainforest = Register(30, "rainforest", new BiomeGenRainforest().SetColor(9419456).SetName("Rainforest").SetFoliageColor(2092120));
    public static readonly Biome SeasonalForest = Register(31, "seasonal_forest", new Biome().SetColor(10215459).SetName("Seasonal Forest"));
    public static readonly Biome Savanna = Register(32, "savanna", new BiomeGenDesert().SetColor(14278691).SetName("Savanna"));
    public static readonly Biome Shrubland = Register(33, "shrubland", new Biome().SetColor(10595616).SetName("Shrubland"));
    public static readonly Biome IceDesert = Register(34, "ice_desert", new BiomeGenDesert().SetColor(0xFFED93).SetName("Ice Desert").EnableSnow().DisableRain().SetFoliageColor(0xC4D339));
    public static readonly Biome Tundra = Register(35, "tundra", new Biome().SetColor(0x57EBF9).SetName("Tundra").EnableSnow().SetFoliageColor(0xC4D339));

    private static readonly Biome[] s_biomes = new Biome[4096];

    public string Name { get; private set; } = "";
    public int Id { get; private set; }
    public int GrassColor { get; private set; }
    public byte TopBlockId = (byte)Block.GrassBlock.id;
    public byte SoilBlockId = (byte)Block.Dirt.id;
    public int FoliageColor { get; private set; } = 0x4EE031;
    public int WaterColorMultiplier { get; private set; } = 0xFFFFFF;
    public float RootHeight { get; private set; } = 0.1F;
    public float HeightVariation { get; private set; } = 0.3F;
    public float Temperature { get; private set; } = 0.5F;
    public float Downfall { get; private set; } = 0.5F;
    public int TreesPerChunk { get; private set; }
    public int FlowersPerChunk { get; private set; } = 2;
    public int GrassPerChunk { get; private set; } = 1;
    public int DeadBushPerChunk { get; private set; }
    public int MushroomsPerChunk { get; private set; }
    public int ReedsPerChunk { get; private set; }
    public int CactiPerChunk { get; private set; }
    public int WaterLiliesPerChunk { get; private set; }
    public int SandPerChunk { get; private set; } = 3;
    public int GravelPerChunk { get; private set; } = 1;
    public int ClayPerChunk { get; private set; } = 1;
    internal BiomeDecorator Decorator { get; private set; } = new();
    protected WeightedRandomSelector<SpawnListEntry> MonsterList { get; } = new();
    protected WeightedRandomSelector<SpawnListEntry> CreatureList { get; } = new();
    protected WeightedRandomSelector<SpawnListEntry> WaterCreatureList { get; } = new();

    public bool HasSnow { get; private set; }
    public bool HasRain { get; private set; } = true;

    protected Biome()
    {
        MonsterList.Add(new SpawnListEntry(w => new EntitySpider(w)), 10);
        MonsterList.Add(new SpawnListEntry(w => new EntityZombie(w)), 10);
        MonsterList.Add(new SpawnListEntry(w => new EntitySkeleton(w)), 10);
        MonsterList.Add(new SpawnListEntry(w => new EntityCreeper(w)), 10);
        MonsterList.Add(new SpawnListEntry(w => new EntitySlime(w)), 10);
        MonsterList.Add(new SpawnListEntry(w => new EntityEnderman(w), 1, 4), 1);

        CreatureList.Add(new SpawnListEntry(w => new EntitySheep(w)), 12);
        CreatureList.Add(new SpawnListEntry(w => new EntityPig(w)), 10);
        CreatureList.Add(new SpawnListEntry(w => new EntityChicken(w)), 10);
        CreatureList.Add(new SpawnListEntry(w => new EntityCow(w)), 8);

        WaterCreatureList.Add(new SpawnListEntry(w => new EntitySquid(w)), 10);
    }

    private static Biome Register(int id, string name, Biome biome)
    {
        biome.Id = id;
        s_registry.Register(id, ResourceLocation.Parse(name), biome);
        return biome;
    }

    protected Biome DisableRain() { HasRain = false; return this; }
    protected Biome EnableSnow() { HasSnow = true; return this; }
    protected Biome SetName(string name) { Name = name; return this; }
    protected Biome SetFoliageColor(int color) { FoliageColor = color; return this; }
    protected Biome SetWaterColor(int color) { WaterColorMultiplier = color; return this; }
    protected Biome SetColor(int color) { GrassColor = color; return this; }
    protected Biome SetClimate(float temperature, float downfall)
    {
        if (temperature > 0.1F && temperature < 0.2F)
        {
            throw new ArgumentException("Please avoid temperatures in the range 0.1 - 0.2 because of snow");
        }

        Temperature = temperature;
        Downfall = downfall;
        return this;
    }
    protected Biome SetHeight(float rootHeight, float heightVariation) { RootHeight = rootHeight; HeightVariation = heightVariation; return this; }
    protected Biome SetDecorator(int treesPerChunk = 0, int flowersPerChunk = 2, int grassPerChunk = 1, int deadBushPerChunk = 0, int mushroomsPerChunk = 0, int reedsPerChunk = 0, int cactiPerChunk = 0, int sandPerChunk = 3, int gravelPerChunk = 1, int clayPerChunk = 1, int waterLiliesPerChunk = 0)
    {
        TreesPerChunk = treesPerChunk;
        FlowersPerChunk = flowersPerChunk;
        GrassPerChunk = grassPerChunk;
        DeadBushPerChunk = deadBushPerChunk;
        MushroomsPerChunk = mushroomsPerChunk;
        ReedsPerChunk = reedsPerChunk;
        CactiPerChunk = cactiPerChunk;
        SandPerChunk = sandPerChunk;
        GravelPerChunk = gravelPerChunk;
        ClayPerChunk = clayPerChunk;
        WaterLiliesPerChunk = waterLiliesPerChunk;
        Decorator.TreesPerChunk = treesPerChunk;
        Decorator.FlowersPerChunk = flowersPerChunk;
        Decorator.GrassPerChunk = grassPerChunk;
        Decorator.DeadBushPerChunk = deadBushPerChunk;
        Decorator.MushroomsPerChunk = mushroomsPerChunk;
        Decorator.ReedsPerChunk = reedsPerChunk;
        Decorator.CactiPerChunk = cactiPerChunk;
        Decorator.SandPerChunk = sandPerChunk;
        Decorator.GravelPerChunk = gravelPerChunk;
        Decorator.ClayPerChunk = clayPerChunk;
        Decorator.WaterLiliesPerChunk = waterLiliesPerChunk;
        return this;
    }

    private protected Biome SetBiomeDecorator(BiomeDecorator decorator)
    {
        Decorator = decorator;
        return this;
    }

    public virtual void Decorate(IWorldContext world, JavaRandom rand, int blockX, int blockZ) => Decorator.Decorate(world, rand, this, blockX, blockZ);

    public static void Init()
    {
        for (int i = 0; i < 64; ++i)
        {
            for (int j = 0; j < 64; ++j)
            {
                s_biomes[i + j * 64] = LocateBiome(i / 63.0F, j / 63.0F);
            }
        }

        Desert.TopBlockId = Desert.SoilBlockId = (byte)Block.Sand.id;
        DesertHills.TopBlockId = DesertHills.SoilBlockId = (byte)Block.Sand.id;
        IceDesert.TopBlockId = IceDesert.SoilBlockId = (byte)Block.Sand.id;
        Ocean.TopBlockId = Ocean.SoilBlockId = (byte)Block.Sand.id;
        River.TopBlockId = River.SoilBlockId = (byte)Block.Sand.id;
        MushroomIsland.TopBlockId = (byte)Block.Mycelium.id;
        MushroomIsland.SoilBlockId = (byte)Block.Dirt.id;
        MushroomIslandShore.TopBlockId = (byte)Block.Mycelium.id;
        MushroomIslandShore.SoilBlockId = (byte)Block.Dirt.id;
        MushroomIsland.CreatureList.Clear();
        MushroomIsland.CreatureList.Add(new SpawnListEntry(w => new EntityMooshroom(w), 4, 8), 8);
        MushroomIslandShore.CreatureList.Clear();
        MushroomIslandShore.CreatureList.Add(new SpawnListEntry(w => new EntityMooshroom(w), 4, 8), 8);
    }

    public virtual Feature GetRandomWorldGenForTrees(JavaRandom rand)
    {
        return rand.NextInt(10) == 0 ? new LargeOakTreeFeature() : new OakTreeFeature();
    }

    public virtual Feature GetRandomWorldGenForGrass(JavaRandom rand)
    {
        return new GrassPatchFeature(Block.Grass.id, 1);
    }


    public static Biome GetBiome(double temp, double downfall)
    {
        int x = (int)(temp * 63.0D);
        int y = (int)(downfall * 63.0D);
        return s_biomes[x + y * 64];
    }

    public static Biome LocateBiome(float temperature, float downfall)
    {
        downfall *= temperature;
        if (temperature < 0.1f) return Tundra;
        if (downfall < 0.2f)
        {
            if (temperature < 0.5f) return Tundra;
            return temperature < 0.95f ? Savanna : Desert;
        }
        if (downfall > 0.5f && temperature < 0.7f) return Swampland;
        if (temperature < 0.5f) return Taiga;
        if (temperature < 0.97f) return downfall < 0.35f ? Shrubland : Forest;
        if (downfall < 0.45f) return Plains;
        return downfall < 0.9f ? SeasonalForest : Rainforest;
    }

    public virtual int GetSkyColorByTemp(float var1)
    {
        var1 /= 3.0F;
        if (var1 < -1.0F)
        {
            var1 = -1.0F;
        }

        if (var1 > 1.0F)
        {
            var1 = 1.0F;
        }

        return ToRgb(224.0f / 360.0f - var1 * 0.05f, 0.5f + var1 * 0.1f, 1.0f);
    }

    public static (int R, int G, int B) FromHsbColor(float hue, float saturation, float brightness)
    {
        if (saturation == 0f)
        {
            int gray = (int)(brightness * 255f + 0.5f);
            return (gray, gray, gray);
        }

        float h = (hue - MathF.Floor(hue)) * 6f;
        float f = h - MathF.Floor(h);
        float p = brightness * (1f - saturation);
        float q = brightness * (1f - saturation * f);
        float t = brightness * (1f - saturation * (1f - f));

        return (int)h switch
        {
            0 => ToRgb(brightness, t, p),
            1 => ToRgb(q, brightness, p),
            2 => ToRgb(p, brightness, t),
            3 => ToRgb(p, q, brightness),
            4 => ToRgb(t, p, brightness),
            _ => ToRgb(brightness, p, q),
        };

        static (int, int, int) ToRgb(float r, float g, float b) =>
            ((int)(r * 255f + 0.5f), (int)(g * 255f + 0.5f), (int)(b * 255f + 0.5f));
    }

    public static int ToRgb(float hue, float saturation, float brightness)
    {
        (int r, int g, int b) = FromHsbColor(hue, saturation, brightness);
        return (255 << 24) | (r << 16) | (g << 8) | b;
    }

    public WeightedRandomSelector<SpawnListEntry> GetSpawnableList(CreatureKind kind)
    {
        if (kind == CreatureKind.Monster) return MonsterList;
        if (kind == CreatureKind.Creature) return CreatureList;
        if (kind == CreatureKind.WaterCreature) return WaterCreatureList;
        throw new ArgumentException("Invalid creature kind: " + kind);
    }

    public bool GetEnableSnow()
    {
        return HasSnow;
    }

    public virtual float GetBiomeSpawnChance() => 0.1F;

    public bool CanSpawnLightningBolt() => !HasSnow && HasRain;

    public int GetTemperatureBits() => (int)(Temperature * 65536.0F);
    public int GetDownfallBits() => (int)(Downfall * 65536.0F);

    public virtual int GetGrassColorAtCoords(IBlockReader reader, int x, int y, int z)
    {
        var biomeSource = reader.GetBiomeSource();
        double temperature = biomeSource.GetTemperature(x, z);
        double downfall = biomeSource.GetDownfall(x, z);
        return GrassColors.getColor(temperature, downfall);
    }

    public virtual int GetFoliageColorAtCoords(IBlockReader reader, int x, int y, int z)
    {
        var biomeSource = reader.GetBiomeSource();
        double temperature = biomeSource.GetTemperature(x, z);
        double downfall = biomeSource.GetDownfall(x, z);
        return FoliageColors.getFoliageColor(temperature, downfall);
    }

    static Biome() => Init();
}
