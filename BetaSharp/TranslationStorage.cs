using Microsoft.Extensions.Logging;

namespace BetaSharp;

public class TranslationStorage
{
    private ILogger _logger = Log.Instance.For<TranslationStorage>();
    private static readonly TranslationStorage _instance = new();
    public static TranslationStorage Instance => _instance;
    private readonly Dictionary<string, string> _translateTable = new();

    private TranslationStorage()
    {
        LoadLanguageFile("lang/en_US.lang");
        LoadLanguageFile("lang/stats_US.lang");

        AddTranslation("disconnect.genericReason", "%1$s");
        AddTranslation("key.zoom", "Zoom");
        AddTranslation("container.crafting", "Crafting");
        AddTranslation("item.emptyPotion.name", "Water Bottle");
        AddTranslation("potion.moveSpeed.name", "Swiftness");
        AddTranslation("potion.moveSlowdown.name", "Slowness");
        AddTranslation("potion.digSpeed.name", "Haste");
        AddTranslation("potion.digSlowDown.name", "Mining Fatigue");
        AddTranslation("potion.damageBoost.name", "Strength");
        AddTranslation("potion.heal.name", "Instant Health");
        AddTranslation("potion.harm.name", "Instant Damage");
        AddTranslation("potion.jump.name", "Jump Boost");
        AddTranslation("potion.confusion.name", "Nausea");
        AddTranslation("potion.regeneration.name", "Regeneration");
        AddTranslation("potion.resistance.name", "Resistance");
        AddTranslation("potion.fireResistance.name", "Fire Resistance");
        AddTranslation("potion.waterBreathing.name", "Water Breathing");
        AddTranslation("potion.invisibility.name", "Invisibility");
        AddTranslation("potion.blindness.name", "Blindness");
        AddTranslation("potion.nightVision.name", "Night Vision");
        AddTranslation("potion.hunger.name", "Hunger");
        AddTranslation("potion.weakness.name", "Weakness");
        AddTranslation("potion.poison.name", "Poison");
        AddTranslation("potion.wither.name", "Wither");
        AddTranslation("potion.absorption.name", "Absorption");
        AddTranslation("item.emerald.name", "Emerald");
        AddTranslation("item.netherStar.name", "Nether Star");
        AddTranslation("item.netherStalkSeeds.name", "Nether Wart");
        AddTranslation("item.netherbrick.name", "Nether Brick");
        AddTranslation("item.netherquartz.name", "Nether Quartz");
        AddTranslation("item.writingBook.name", "Book and Quill");
        AddTranslation("item.leash.name", "Lead");
        AddTranslation("item.nameTag.name", "Name Tag");
        AddTranslation("item.ironNugget.name", "Iron Nugget");
        AddTranslation("item.elytra.name", "Elytra");
        AddTranslation("item.fireworks.name", "Firework Rocket");
        AddTranslation("item.armorStand.name", "Armor Stand");
        AddTranslation("item.endCrystal.name", "End Crystal");
        AddTranslation("item.totem.name", "Totem of Undying");
        AddTranslation("item.minecartTnt.name", "TNT Minecart");
        AddTranslation("item.pumpkinPie.name", "Pumpkin Pie");
        AddTranslation("item.rabbitStew.name", "Rabbit Stew");
        AddTranslation("item.fishSalmon.name", "Raw Salmon");
        AddTranslation("item.fishTropical.name", "Tropical Fish");
        AddTranslation("item.pufferfish.name", "Pufferfish");
        AddTranslation("item.beetroot.name", "Beetroot");
        AddTranslation("item.chorusFruit.name", "Chorus Fruit");
        AddTranslation("item.beetrootSoup.name", "Beetroot Soup");
        AddTranslation("item.horsearmormetal.name", "Iron Horse Armor");
        AddTranslation("item.horsearmorgold.name", "Gold Horse Armor");
        AddTranslation("item.horsearmordiamond.name", "Diamond Horse Armor");
        AddTranslation("item.skull.name", "Skeleton Skull");
        AddTranslation("item.skull.skeleton.name", "Skeleton Skull");
        AddTranslation("item.skull.wither.name", "Wither Skeleton Skull");
        AddTranslation("item.skull.zombie.name", "Zombie Head");
        AddTranslation("item.skull.char.name", "Player Head");
        AddTranslation("item.skull.creeper.name", "Creeper Head");
        AddTranslation("item.skull.dragon.name", "Dragon Head");
        AddTranslation("item.appleGold.enchanted.name", "Enchanted Golden Apple");
        AddTranslation("tile.oreEmerald.name", "Emerald Ore");
        AddTranslation("tile.blockEmerald.name", "Emerald Block");
        AddTranslation("tile.blockRedstone.name", "Redstone Block");
        AddTranslation("tile.monsterStoneEgg.name", "Monster Egg");
        AddTranslation("tile.cobbleWall.normal.name", "Cobblestone Wall");
        AddTranslation("tile.cobbleWall.mossy.name", "Mossy Cobblestone Wall");
        AddTranslation("tile.netherStalk.name", "Nether Wart");
        AddTranslation("tile.netherquartz.name", "Nether Quartz Ore");
        AddTranslation("tile.quartzBlock.name", "Block of Quartz");
        AddTranslation("tile.stairsQuartz.name", "Quartz Stairs");
        AddTranslation("tile.barrier.name", "Barrier");
        AddTranslation("tile.activatorRail.name", "Activator Rail");
        AddTranslation("tile.skull.name", "Skeleton Skull");
        AddTranslation("tile.skull.skeleton.name", "Skeleton Skull");
        AddTranslation("tile.skull.wither.name", "Wither Skeleton Skull");
        AddTranslation("tile.skull.zombie.name", "Zombie Head");
        AddTranslation("tile.skull.char.name", "Player Head");
        AddTranslation("tile.skull.creeper.name", "Creeper Head");
        AddTranslation("tile.skull.dragon.name", "Dragon Head");
        AddTranslation("item.carrots.name", "Carrot");
        AddTranslation("item.potato.name", "Potato");
        AddTranslation("item.potatoBaked.name", "Baked Potato");
        AddTranslation("item.potatoPoisonous.name", "Poisonous Potato");
        AddTranslation("item.carrotGolden.name", "Golden Carrot");
        AddTranslation("tile.carrots.name", "Carrots");
        AddTranslation("tile.potatoes.name", "Potatoes");
        AddTranslation("tile.sponge.name", "Sponge");
        AddTranslation("tile.sponge.dry.name", "Sponge");
        AddTranslation("tile.sponge.wet.name", "Wet Sponge");
        AddTranslation("tile.cocoa.name", "Cocoa");
        AddTranslation("tile.hayBlock.name", "Hay Bale");
        AddTranslation("tile.slime.name", "Slime Block");
        AddTranslation("item.muttonRaw.name", "Raw Mutton");
        AddTranslation("item.muttonCooked.name", "Cooked Mutton");
        AddTranslation("item.rabbitRaw.name", "Raw Rabbit");
        AddTranslation("item.rabbitCooked.name", "Cooked Rabbit");
        AddTranslation("item.rabbitFoot.name", "Rabbit's Foot");
        AddTranslation("item.rabbitHide.name", "Rabbit Hide");
        AddTranslation("tile.carpet.white.name", "White Carpet");
        AddTranslation("tile.carpet.orange.name", "Orange Carpet");
        AddTranslation("tile.carpet.magenta.name", "Magenta Carpet");
        AddTranslation("tile.carpet.lightBlue.name", "Light Blue Carpet");
        AddTranslation("tile.carpet.yellow.name", "Yellow Carpet");
        AddTranslation("tile.carpet.lime.name", "Lime Carpet");
        AddTranslation("tile.carpet.pink.name", "Pink Carpet");
        AddTranslation("tile.carpet.gray.name", "Gray Carpet");
        AddTranslation("tile.carpet.silver.name", "Light Gray Carpet");
        AddTranslation("tile.carpet.cyan.name", "Cyan Carpet");
        AddTranslation("tile.carpet.purple.name", "Purple Carpet");
        AddTranslation("tile.carpet.blue.name", "Blue Carpet");
        AddTranslation("tile.carpet.brown.name", "Brown Carpet");
        AddTranslation("tile.carpet.green.name", "Green Carpet");
        AddTranslation("tile.carpet.red.name", "Red Carpet");
        AddTranslation("tile.carpet.black.name", "Black Carpet");
        AddTranslation("item.monsterPlacer.50.name", "Spawn Creeper");
        AddTranslation("item.monsterPlacer.51.name", "Spawn Skeleton");
        AddTranslation("item.monsterPlacer.52.name", "Spawn Spider");
        AddTranslation("item.monsterPlacer.53.name", "Spawn Giant");
        AddTranslation("item.monsterPlacer.54.name", "Spawn Zombie");
        AddTranslation("item.monsterPlacer.55.name", "Spawn Slime");
        AddTranslation("item.monsterPlacer.56.name", "Spawn Ghast");
        AddTranslation("item.monsterPlacer.57.name", "Spawn Zombie Pigman");
        AddTranslation("item.monsterPlacer.58.name", "Spawn Enderman");
        AddTranslation("item.monsterPlacer.59.name", "Spawn Cave Spider");
        AddTranslation("item.monsterPlacer.60.name", "Spawn Silverfish");
        AddTranslation("item.monsterPlacer.61.name", "Spawn Blaze");
        AddTranslation("item.monsterPlacer.64.name", "Spawn Wither");
        AddTranslation("item.monsterPlacer.66.name", "Spawn Wither Skeleton");
        AddTranslation("item.monsterPlacer.67.name", "Spawn Magma Cube");
        AddTranslation("item.monsterPlacer.69.name", "Spawn Shulker");
        AddTranslation("item.monsterPlacer.90.name", "Spawn Pig");
        AddTranslation("item.monsterPlacer.91.name", "Spawn Sheep");
        AddTranslation("item.monsterPlacer.92.name", "Spawn Cow");
        AddTranslation("item.monsterPlacer.93.name", "Spawn Chicken");
        AddTranslation("item.monsterPlacer.94.name", "Spawn Squid");
        AddTranslation("item.monsterPlacer.95.name", "Spawn Wolf");
        AddTranslation("item.monsterPlacer.96.name", "Spawn Mooshroom");
        AddTranslation("item.monsterPlacer.97.name", "Spawn Snow Golem");
        AddTranslation("item.monsterPlacer.99.name", "Spawn Iron Golem");
        AddTranslation("item.monsterPlacer.100.name", "Spawn Horse");
        AddTranslation("item.monsterPlacer.101.name", "Spawn Rabbit");
        AddTranslation("item.monsterPlacer.120.name", "Spawn Villager");
        AddTranslation("item.monsterPlacer.121.name", "Spawn Ender Dragon");
    }

    public void AddTranslation(string key, string translation)
    {
        _translateTable[key] = translation;
    }

    private void LoadLanguageFile(string assetPath)
    {
        try
        {
            var asset = AssetManager.Instance.getAsset(assetPath);
            if (asset == null) return;

            using StringReader reader = new(asset.GetTextContent());
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                line = line.Trim();
                if (string.IsNullOrEmpty(line) || line.StartsWith('#')) continue;

                int separatorIndex = line.IndexOf('=');
                if (separatorIndex != -1)
                {
                    string key = line[..separatorIndex].Trim();
                    string value = line[(separatorIndex + 1)..].Trim();
                    _translateTable[key] = value;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to load language file {assetPath}", ex);
        }
    }

    public string TranslateKey(string key)
    {
        return _translateTable.TryGetValue(key, out string value) ? value : key;
    }

    public string TranslateKeyFormat(string key, params object[] values)
    {
        string str = _translateTable.TryGetValue(key, out string value) ? value : key;
        for (int i = 0; i < values.Length; i++)
        {
            str = str.Replace($"%{i + 1}$s", values[i].ToString() ?? string.Empty);
        }
        if (str == "%s")
            str = key + " (Failed to translate key!)";
        return str;
    }

    public string TranslateNamedKey(string key)
    {
        return _translateTable.TryGetValue($"{key}.name", out string value) ? value : "";
    }
}
