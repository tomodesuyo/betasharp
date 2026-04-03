using BetaSharp.Blocks;
using BetaSharp.Blocks.Entities;
using BetaSharp;
using BetaSharp.Creative;
using BetaSharp.Client.Rendering.Entities.Models;
using BetaSharp.Entities;
using BetaSharp.Items;
using BetaSharp.Inventorys;
using BetaSharp.NBT;
using BetaSharp.Potions;
using BetaSharp.Screens;
using BetaSharp.Screens.Slots;
using BetaSharp.Trading;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds;
using BetaSharp.Worlds.Chunks;
using BetaSharp.Worlds.Core;
using BetaSharp.Worlds.Core.Systems;
using BetaSharp.Worlds.Dimensions;
using BetaSharp.Worlds.Biomes.Source;
using BetaSharp.Worlds.Generation.Biomes;
using BetaSharp.Worlds.Generation.Generators.Features;
using BetaSharp.Worlds.Generation.Structures;
using BetaSharp.Server.Commands;
using BetaSharp.Worlds.Storage;
using System.Reflection;
using System.Collections.Concurrent;

namespace BetaSharp.Tests;

public class EndAndVillageIntegrationTests
{
    [Fact]
    public void WorldBiomeSourceChangesWhenWorldSeedChanges()
    {
        TestWorld first = new(new OverworldDimension(), 12345L, WorldType.Default);
        TestWorld second = new(new OverworldDimension(), 67890L, WorldType.Default);

        Biome[] firstBiomes = first.Dimension.BiomeSource.GetBiomesInArea(null, -64, -64, 64, 64);
        Biome[] secondBiomes = second.Dimension.BiomeSource.GetBiomesInArea(null, -64, -64, 64, 64);

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
    public void EndBiomeDecorationSpawnsDragonAtOriginChunk()
    {
        TestWorld world = new(new EndDimension(), 12345L);

        Biome.End.Decorate(world, new JavaRandom(12345L), 0, 0);

        List<EntityDragon> dragons = world.Entities.CollectEntitiesOfType<EntityDragon>(new Box(-64.0D, 0.0D, -64.0D, 64.0D, 256.0D, 64.0D));
        Assert.Single(dragons);
    }

    [Fact]
    public void EndBiomeDecorationDoesNotRespawnDragonAfterFirstSpawn()
    {
        TestWorld world = new(new EndDimension(), 12345L);

        Biome.End.Decorate(world, new JavaRandom(12345L), 0, 0);
        Biome.End.Decorate(world, new JavaRandom(54321L), 0, 0);

        List<EntityDragon> dragons = world.Entities.CollectEntitiesOfType<EntityDragon>(new Box(-64.0D, 0.0D, -64.0D, 64.0D, 256.0D, 64.0D));
        Assert.Single(dragons);
        Assert.True(world.Properties.HasSpawnedEnderDragon);
    }

    [Fact]
    public void DragonDeathCreatesExitPortalAndEgg()
    {
        TestWorld world = new(new EndDimension(), 12345L);
        EntityDragon dragon = new(world);
        dragon.setPositionAndAngles(0.0D, 80.0D, 0.0D, 0.0F, 0.0F);
        world.SpawnEntity(dragon);

        Assert.True(dragon.damage(null!, 200));

        for (int i = 0; i < 200; ++i)
        {
            dragon.tickMovement();
        }

        Assert.True(dragon.dead);
        Assert.Equal(Block.EndPortal.id, world.Reader.GetBlockId(1, 64, 0));
        Assert.Equal(Block.DragonEgg.id, world.Reader.GetBlockId(0, 68, 0));
    }

    [Fact]
    public void DragonEggTeleportsWhenUsed()
    {
        TestWorld world = new(new EndDimension(), 12345L);
        DummyPlayer player = new(world);
        world.Writer.SetBlock(0, 70, 0, Block.DragonEgg.id);

        Block.DragonEgg.onUse(new OnUseEvent(world, player, 0, 70, 0));

        Assert.Equal(0, world.Reader.GetBlockId(0, 70, 0));

        bool foundEgg = false;
        for (int x = -16; x <= 16 && !foundEgg; ++x)
        {
            for (int y = 62; y <= 78 && !foundEgg; ++y)
            {
                for (int z = -16; z <= 16; ++z)
                {
                    if (world.Reader.GetBlockId(x, y, z) == Block.DragonEgg.id)
                    {
                        foundEgg = true;
                        break;
                    }
                }
            }
        }

        Assert.True(foundEgg);
    }

    [Fact]
    public void EndSpikeFeatureCreatesCrystalToppedObsidianPillar()
    {
        TestWorld world = new(new EndDimension(), 12345L);
        EndSpikeFeature feature = new();

        Assert.True(feature.Generate(world, new JavaRandom(12345L), 0, 64, 0));

        List<EntityEnderCrystal> crystals = world.Entities.CollectEntitiesOfType<EntityEnderCrystal>(new Box(-4.0D, 60.0D, -4.0D, 4.0D, 128.0D, 4.0D));
        Assert.Single(crystals);
        int topY = MathHelper.Floor(crystals[0].y);
        Assert.Equal(Block.Bedrock.id, world.Reader.GetBlockId(0, topY, 0));
    }

    [Fact]
    public void EndDimensionKeepsBrightnessFloorAboveCaveDarkness()
    {
        TestWorld world = new(new EndDimension(), 12345L);

        Assert.Equal(0.1F, world.Dimension.LightLevelToLuminance[0], 3);
    }

    [Fact]
    public void PumpkinCreatesPlayerBuiltIronGolem()
    {
        TestWorld world = new(new OverworldDimension(), 12345L);

        world.Writer.SetBlockWithoutCallingOnPlaced(0, 5, 0, Block.IronBlock.id, 0);
        world.Writer.SetBlockWithoutCallingOnPlaced(0, 6, 0, Block.IronBlock.id, 0);
        world.Writer.SetBlockWithoutCallingOnPlaced(-1, 6, 0, Block.IronBlock.id, 0);
        world.Writer.SetBlockWithoutCallingOnPlaced(1, 6, 0, Block.IronBlock.id, 0);

        world.Writer.SetBlock(0, 7, 0, Block.Pumpkin.id);

        List<EntityIronGolem> golems = world.Entities.CollectEntitiesOfType<EntityIronGolem>(new Box(-8.0D, 0.0D, -8.0D, 8.0D, 128.0D, 8.0D));
        Assert.Single(golems);
        Assert.True(golems[0].IsPlayerCreated);
        Assert.Equal(0, world.Reader.GetBlockId(0, 7, 0));
        Assert.Equal(0, world.Reader.GetBlockId(0, 6, 0));
    }

    [Fact]
    public void VillageCollectionCreatesVillageFromShelteredDoor()
    {
        TestWorld world = new(new OverworldDimension(), 12345L);

        world.Writer.SetBlockWithoutCallingOnPlaced(0, 4, 0, Block.Door.id, 0);
        world.Writer.SetBlockWithoutCallingOnPlaced(0, 5, 0, Block.Door.id, 8);
        for (int offset = -5; offset < 0; ++offset)
        {
            world.Writer.SetBlockWithoutCallingOnPlaced(offset, 5, 0, Block.Cobblestone.id, 0);
        }

        world.Villages.AddVillagerPosition(0, 4, 0);
        world.Villages.Tick();

        Assert.Single(world.Villages.Villages);
        Assert.Equal(1, world.Villages.Villages[0].GetNumVillageDoors());
    }

    [Fact]
    public void VillageLocatorFindsNearestVillageWithoutPregeneratingChunks()
    {
        TestWorld world = new(new OverworldDimension(), 12345L, WorldType.Default);
        MapGenVillage villageGenerator = new(0);

        Vec3i? result = villageGenerator.FindNearestStructure(world, 0, 64, 0);

        Assert.NotNull(result);
    }

    [Fact]
    public void DoorPlacementKeepsMirrorBitInUpperHalf()
    {
        TestWorld world = new(new OverworldDimension(), 12345L);
        world.Writer.SetBlockWithoutCallingOnPlaced(0, 4, 0, Block.Cobblestone.id, 0);
        world.Writer.SetBlockWithoutCallingOnPlaced(-1, 5, 0, Block.Cobblestone.id, 0);
        world.Writer.SetBlockWithoutCallingOnPlaced(-1, 6, 0, Block.Cobblestone.id, 0);

        Type itemDoorType = typeof(Item).Assembly.GetType("BetaSharp.Items.ItemDoor")!;
        MethodInfo placeDoorBlock = itemDoorType.GetMethod("PlaceDoorBlock", BindingFlags.Public | BindingFlags.Static)!;
        placeDoorBlock.Invoke(null, [world, 0, 5, 0, 1, Block.Door]);

        Assert.Equal(1, world.Reader.GetBlockMeta(0, 5, 0));
        Assert.Equal(9, world.Reader.GetBlockMeta(0, 6, 0));
    }

    [Fact]
    public void DoorTogglePreservesUpperHalfMetadata()
    {
        TestWorld world = new(new OverworldDimension(), 12345L);
        DummyPlayer player = new(world);
        world.Writer.SetBlockWithoutCallingOnPlaced(0, 4, 0, Block.Cobblestone.id, 0);
        world.Writer.SetBlockWithoutCallingOnPlaced(-1, 5, 0, Block.Cobblestone.id, 0);
        world.Writer.SetBlockWithoutCallingOnPlaced(-1, 6, 0, Block.Cobblestone.id, 0);

        Type itemDoorType = typeof(Item).Assembly.GetType("BetaSharp.Items.ItemDoor")!;
        MethodInfo placeDoorBlock = itemDoorType.GetMethod("PlaceDoorBlock", BindingFlags.Public | BindingFlags.Static)!;
        placeDoorBlock.Invoke(null, [world, 0, 5, 0, 1, Block.Door]);

        int upperMetaBefore = world.Reader.GetBlockMeta(0, 6, 0);
        Block.Door.onUse(new OnUseEvent(world, player, 0, 5, 0));

        Assert.Equal(upperMetaBefore, world.Reader.GetBlockMeta(0, 6, 0));
        Assert.Equal(5, world.Reader.GetBlockMeta(0, 5, 0));
    }

    [Fact]
    public void RedstoneBlockPowersAdjacentDustContinuously()
    {
        TestWorld world = new(new OverworldDimension(), 12345L);
        IWorldContext context = world;

        world.Writer.SetBlockWithoutCallingOnPlaced(0, 4, 0, Block.RedstoneBlock.id, 0);
        world.Writer.SetBlockWithoutCallingOnPlaced(1, 4, 0, Block.RedstoneWire.id, 0);
        Block.RedstoneWire.onPlaced(new OnPlacedEvent(world, null, 1, 1, 1, 4, 0));

        Assert.True(context.Redstone.IsPoweringSide(0, 4, 0, 5));
        Assert.True(context.Redstone.IsPowered(1, 4, 0));
        Assert.True(world.Reader.GetBlockMeta(1, 4, 0) > 0);
    }

    [Fact]
    public void SeedFoodsStackToSixtyFour()
    {
        Assert.Equal(64, Item.Carrot.getMaxCount());
        Assert.Equal(64, Item.Potato.getMaxCount());
    }

    [Fact]
    public void BoneMealAppliesToCarrotsAndPotatoes()
    {
        TestWorld world = new(new OverworldDimension(), 12345L);
        DummyPlayer player = new(world);
        ItemStack boneMeal = new(Item.Dye, 2, 15);

        world.Writer.SetBlockWithoutCallingOnPlaced(0, 4, 0, Block.Farmland.id, 0);
        world.Writer.SetBlockWithoutCallingOnPlaced(0, 5, 0, Block.Carrot.id, 0);
        world.Writer.SetBlockWithoutCallingOnPlaced(1, 4, 0, Block.Farmland.id, 0);
        world.Writer.SetBlockWithoutCallingOnPlaced(1, 5, 0, Block.Potato.id, 0);

        Assert.True(Item.Dye.useOnBlock(boneMeal, player, world, 0, 5, 0, 1));
        Assert.True(Item.Dye.useOnBlock(boneMeal, player, world, 1, 5, 0, 1));

        Assert.Equal(7, world.Reader.GetBlockMeta(0, 5, 0));
        Assert.Equal(7, world.Reader.GetBlockMeta(1, 5, 0));
        Assert.Equal(0, boneMeal.count);
    }

    [Fact]
    public void MerchantTradeConsumesInputsAndProducesOutput()
    {
        TestWorld world = new(new OverworldDimension(), 12345L);
        DummyPlayer player = new(world);
        MerchantRecipeList recipes = new();
        recipes.Add(new MerchantRecipe(new ItemStack(Item.Wheat, 18), new ItemStack(Item.Emerald, 1)));

        DummyMerchant merchant = new(recipes);
        merchant.SetCustomer(player);

        MerchantScreenHandler handler = new(player.inventory, merchant);
        handler.SetOffers(recipes, 0);
        handler.MerchantInventory.setStack(0, new ItemStack(Item.Wheat, 18));

        Assert.NotNull(handler.MerchantInventory.getStack(2));
        Assert.Equal(Item.Emerald.id, handler.MerchantInventory.getStack(2).itemId);

        handler.onSlotClick(2, 0, false, player);

        Assert.NotNull(player.inventory.getCursorStack());
        Assert.Equal(Item.Emerald.id, player.inventory.getCursorStack().itemId);
        Assert.Null(handler.MerchantInventory.getStack(0));
        Assert.Null(handler.MerchantInventory.getStack(2));
        Assert.Equal(1, recipes[0].ToolUses);
    }

    [Fact]
    public void SupportedCarpetSurvivesNeighborUpdates()
    {
        TestWorld world = new(new OverworldDimension(), 12345L);
        world.Writer.SetBlockWithoutCallingOnPlaced(0, 4, 0, Block.Cobblestone.id, 0);
        world.Writer.SetBlockWithoutCallingOnPlaced(0, 5, 0, Block.Carpet.id, 0);

        Block.Carpet.neighborUpdate(new OnTickEvent(world, 0, 5, 0, 0, Block.Carpet.id));

        Assert.Equal(Block.Carpet.id, world.Reader.GetBlockId(0, 5, 0));
    }

    [Fact]
    public void StructureTorchPlacementResolvesSupportDirection()
    {
        TestWorld world = new(new OverworldDimension(), 12345L);
        world.Writer.SetBlockWithoutCallingOnPlaced(-1, 5, 0, Block.Cobblestone.id, 0);

        TestStructurePiece piece = new();
        StructureBoundingBox bounds = new(0, 5, 0, 0, 5, 0);
        piece.PlaceTorch(world, bounds, 0, 5, 0);

        Assert.Equal(Block.Torch.id, world.Reader.GetBlockId(0, 5, 0));
        Assert.Equal(1, world.Reader.GetBlockMeta(0, 5, 0));
    }

    [Fact]
    public void StructurePlacementDoesNotDoubleRotatePrecomputedMetadata()
    {
        TestWorld world = new(new OverworldDimension(), 12345L);
        MetadataPlacementProbe piece = new(1, 0, 5, 0);
        StructureBoundingBox bounds = new(0, 5, 0, 0, 5, 0);

        int rotatedMeta = piece.MapMeta(Block.WoodenStairs.id, 3);
        piece.Place(world, Block.WoodenStairs.id, rotatedMeta, 0, 0, 0, bounds);

        Assert.Equal(rotatedMeta, world.Reader.GetBlockMeta(0, 5, 0));
    }

    [Fact]
    public void StrongholdGenerationIsThreadSafe()
    {
        ConcurrentQueue<Exception> exceptions = [];

        Parallel.For(0, 24, i =>
        {
            try
            {
                TestWorld world = new(new OverworldDimension(), 12345L + i, WorldType.Default);
                _ = new StructureStrongholdStart(world, new JavaRandom(98765L + i), i, -i);
            }
            catch (Exception ex)
            {
                exceptions.Enqueue(ex);
            }
        });

        Assert.Empty(exceptions);
    }

    [Fact]
    public void BlazeIsRegisteredAndCreatable()
    {
        TestWorld world = new(new OverworldDimension(), 12345L);

        Entity? entity = EntityRegistry.Create("Blaze", world);

        Assert.NotNull(entity);
        Assert.IsType<EntityBlaze>(entity);
        Assert.Equal(61, EntityRegistry.GetRawId(entity!));
    }

    [Fact]
    public void CraftingManagerIncludesBrewingStandRecipeAndItemRepair()
    {
        object manager = GetCraftingManager();
        MethodInfo findMatchingRecipe = manager.GetType().GetMethod("FindMatchingRecipe", BindingFlags.Instance | BindingFlags.Public)!;

        InventoryCrafting brewingInventory = new(new DummyScreenHandler(), 3, 3);
        brewingInventory.setStack(1, new ItemStack(Item.BlazeRod));
        brewingInventory.setStack(3, new ItemStack(Block.Cobblestone));
        brewingInventory.setStack(4, new ItemStack(Block.Cobblestone));
        brewingInventory.setStack(5, new ItemStack(Block.Cobblestone));

        ItemStack? brewingStand = (ItemStack?)findMatchingRecipe.Invoke(manager, [brewingInventory]);

        Assert.NotNull(brewingStand);
        Assert.Equal(Item.BrewingStand.id, brewingStand!.itemId);

        InventoryCrafting repairInventory = new(new DummyScreenHandler(), 2, 2);
        repairInventory.setStack(0, new ItemStack(Item.IronPickaxe, 1, 200));
        repairInventory.setStack(1, new ItemStack(Item.IronPickaxe, 1, 180));

        ItemStack? repaired = (ItemStack?)findMatchingRecipe.Invoke(manager, [repairInventory]);

        Assert.NotNull(repaired);
        Assert.Equal(Item.IronPickaxe.id, repaired!.itemId);
        Assert.True(repaired.getDamage() < 180);
    }

    [Fact]
    public void SmeltingManagerIncludesModernFoodAndOreRecipes()
    {
        Type smeltingType = typeof(Item).Assembly.GetType("BetaSharp.Recipes.SmeltingRecipeManager")!;
        object manager = smeltingType.GetMethod("getInstance", BindingFlags.Static | BindingFlags.Public)!.Invoke(null, null)!;
        MethodInfo craft = smeltingType.GetMethod("Craft", BindingFlags.Instance | BindingFlags.Public)!;

        ItemStack? cookedBeef = (ItemStack?)craft.Invoke(manager, [Item.RawBeef.id]);
        ItemStack? lapisDye = (ItemStack?)craft.Invoke(manager, [Block.LapisOre.id]);
        ItemStack? netherBrick = (ItemStack?)craft.Invoke(manager, [Block.Netherrack.id]);

        Assert.NotNull(cookedBeef);
        Assert.Equal(Item.CookedBeef.id, cookedBeef!.itemId);
        Assert.NotNull(lapisDye);
        Assert.Equal(Item.Dye.id, lapisDye!.itemId);
        Assert.Equal(4, lapisDye.getDamage());
        Assert.NotNull(netherBrick);
        Assert.Equal(Item.NetherBrickItem.id, netherBrick!.itemId);
    }

    [Fact]
    public void CraftingManagerIncludesPumpkinPieRecipe()
    {
        object manager = GetCraftingManager();
        MethodInfo findMatchingRecipe = manager.GetType().GetMethod("FindMatchingRecipe", BindingFlags.Instance | BindingFlags.Public)!;

        InventoryCrafting inventory = new(new DummyScreenHandler(), 3, 3);
        inventory.setStack(0, new ItemStack(Block.Pumpkin));
        inventory.setStack(1, new ItemStack(Item.Sugar));
        inventory.setStack(2, new ItemStack(Item.Egg));

        ItemStack? pumpkinPie = (ItemStack?)findMatchingRecipe.Invoke(manager, [inventory]);

        Assert.NotNull(pumpkinPie);
        Assert.Equal(Item.PumpkinPie.id, pumpkinPie!.itemId);
    }

    [Fact]
    public void DrinkingPotionAppliesEffectAndReturnsGlassBottle()
    {
        TestWorld world = new(new OverworldDimension(), 12345L);
        DummyPlayer player = new(world);
        int potionDamage = BrewPotionDamage(0, Item.NetherWart, Item.Sugar);
        ItemStack potion = new(Item.Potion, 1, potionDamage);

        ItemStack result = potion.use(world, player);

        Assert.Equal(Item.GlassBottle.id, result.itemId);
        Assert.True(player.hasPotionEffect(Potion.MoveSpeed));
    }

    [Fact]
    public void BrewingStandCanTurnPotionIntoSplashPotion()
    {
        TestWorld world = new(new OverworldDimension(), 12345L);
        world.Writer.SetBlockWithoutCallingOnPlaced(0, 4, 0, Block.BrewingStand.id, 0);

        BlockEntityBrewingStand brewingStand = new()
        {
            World = world,
            X = 0,
            Y = 4,
            Z = 0
        };

        int potionDamage = BrewPotionDamage(0, Item.NetherWart, Item.Sugar);
        brewingStand.setStack(0, new ItemStack(Item.Potion, 1, potionDamage));
        brewingStand.setStack(3, new ItemStack(Item.Gunpowder));

        RunBrewingCycle(brewingStand, world);

        ItemStack? brewed = brewingStand.getStack(0);
        Assert.True(Item.Gunpowder.isPotionIngredient());
        Assert.NotNull(brewed);
        Assert.True(ItemPotion.IsSplash(brewed!.getDamage()));
        Assert.Contains(((ItemPotion)Item.Potion).GetEffects(brewed.getDamage())!, effect => effect.PotionId == Potion.MoveSpeed.Id);
    }

    [Fact]
    public void FireResistanceEffectClearsFireTicks()
    {
        TestWorld world = new(new OverworldDimension(), 12345L);
        DummyPlayer player = new(world)
        {
            fireTicks = 80
        };

        player.addPotionEffect(new PotionEffect(Potion.FireResistance.Id, 200, 0));
        player.baseTick();

        Assert.Equal(0, player.fireTicks);
    }

    [Fact]
    public void CauldronAcceptsWaterBucketAndFillsGlassBottle()
    {
        TestWorld world = new(new OverworldDimension(), 12345L);
        DummyPlayer player = new(world);
        player.inventory.selectedSlot = 0;

        world.Writer.SetBlockWithoutCallingOnPlaced(0, 4, 0, Block.Cauldron.id, 0);
        player.inventory.setStack(0, new ItemStack(Item.WaterBucket));

        Assert.True(Block.Cauldron.onUse(new OnUseEvent(world, player, 0, 4, 0)));
        Assert.Equal(3, world.Reader.GetBlockMeta(0, 4, 0));
        Assert.Equal(Item.Bucket.id, player.getHand()!.itemId);

        player.inventory.setStack(0, new ItemStack(Item.GlassBottle));

        Assert.True(Block.Cauldron.onUse(new OnUseEvent(world, player, 0, 4, 0)));
        Assert.Equal(2, world.Reader.GetBlockMeta(0, 4, 0));
        Assert.Equal(Item.Potion.id, player.getHand()!.itemId);
        Assert.Equal(0, player.getHand()!.getDamage());
    }

    [Fact]
    public void GlassBottleCanFillFromWaterSource()
    {
        TestWorld world = new(new OverworldDimension(), 12345L);
        DummyPlayer player = new(world);
        player.setPositionAndAngles(0.5D, 5.0D, 0.5D, 0.0F, 15.0F);
        world.Writer.SetBlockWithoutCallingOnPlaced(0, 4, 1, Block.Water.id, 0);

        ItemStack result = new ItemStack(Item.GlassBottle).use(world, player);

        Assert.Equal(Item.Potion.id, result.itemId);
        Assert.Equal(0, result.getDamage());
    }

    [Fact]
    public void PressurePlateCanBePlacedOnFence()
    {
        TestWorld world = new(new OverworldDimension(), 12345L);
        world.Writer.SetBlockWithoutCallingOnPlaced(0, 4, 0, Block.Fence.id, 0);

        Assert.True(Block.StonePressurePlate.canPlaceAt(new CanPlaceAtContext(world, 0, 0, 5, 0)));
    }

    [Fact]
    public void CreativeInventoryIncludesAllPotionEffectFamilies()
    {
        TestWorld world = new(new OverworldDimension(), 12345L);
        DummyPlayer player = new(world);
        CreativeScreenHandler handler = new(player.inventory);
        FieldInfo allStacksField = typeof(CreativeScreenHandler).GetField("_allStacks", BindingFlags.Instance | BindingFlags.NonPublic)!;
        List<ItemStack> allStacks = (List<ItemStack>)allStacksField.GetValue(handler)!;

        HashSet<int> potionIds = [];
        foreach (ItemStack stack in allStacks)
        {
            if (stack.itemId != Item.Potion.id)
            {
                continue;
            }

            List<PotionEffect>? effects = ((ItemPotion)Item.Potion).GetEffects(stack.getDamage());
            if (effects == null)
            {
                continue;
            }

            for (int i = 0; i < effects.Count; ++i)
            {
                potionIds.Add(effects[i].PotionId);
            }
        }

        Assert.Contains(Potion.MoveSpeed.Id, potionIds);
        Assert.Contains(Potion.Jump.Id, potionIds);
        Assert.Contains(Potion.FireResistance.Id, potionIds);
        Assert.Contains(Potion.WaterBreathing.Id, potionIds);
        Assert.Contains(Potion.NightVision.Id, potionIds);
        Assert.Contains(Potion.Invisibility.Id, potionIds);
        Assert.Contains(Potion.Poison.Id, potionIds);
        Assert.Contains(Potion.Weakness.Id, potionIds);
    }

    [Fact]
    public void CreativeInventoryIncludesAllCarpetColors()
    {
        TestWorld world = new(new OverworldDimension(), 12345L);
        DummyPlayer player = new(world);
        CreativeScreenHandler handler = new(player.inventory);
        FieldInfo allStacksField = typeof(CreativeScreenHandler).GetField("_allStacks", BindingFlags.Instance | BindingFlags.NonPublic)!;
        List<ItemStack> allStacks = (List<ItemStack>)allStacksField.GetValue(handler)!;

        List<ItemStack> carpetStacks = allStacks.Where(stack => stack.itemId == Block.Carpet.id).ToList();

        Assert.Equal(16, carpetStacks.Select(stack => stack.getDamage()).Distinct().Count());
    }

    [Fact]
    public void CreativeInventoryIncludesAllMonsterEggVariants()
    {
        TestWorld world = new(new OverworldDimension(), 12345L);
        DummyPlayer player = new(world);
        CreativeScreenHandler handler = new(player.inventory);
        FieldInfo allStacksField = typeof(CreativeScreenHandler).GetField("_allStacks", BindingFlags.Instance | BindingFlags.NonPublic)!;
        List<ItemStack> allStacks = (List<ItemStack>)allStacksField.GetValue(handler)!;

        List<ItemStack> monsterEggStacks = allStacks.Where(stack => stack.itemId == Block.Silverfish.id).ToList();

        Assert.Equal(6, monsterEggStacks.Select(stack => stack.getDamage()).Distinct().Count());
    }

    [Fact]
    public void CreativeInventoryUsesDedicatedSkullItem()
    {
        TestWorld world = new(new OverworldDimension(), 12345L);
        DummyPlayer player = new(world);
        CreativeScreenHandler handler = new(player.inventory);

        List<ItemStack> allStacks = GetCreativeStacks(handler, "_allStacks");

        Assert.Contains(allStacks, stack => stack.itemId == Item.Skull.id);
        Assert.DoesNotContain(allStacks, stack => stack.itemId == Block.Skull.id);

        handler.SetTab(CreativeInventoryTab.Decorations);
        List<ItemStack> visibleStacks = GetCreativeStacks(handler, "_visibleStacks");

        Assert.Contains(visibleStacks, stack => stack.itemId == Item.Skull.id);
    }

    [Fact]
    public void CreativeInventoryMatchesModernFoodBrewingAndTransportationTabs()
    {
        TestWorld world = new(new OverworldDimension(), 12345L);
        DummyPlayer player = new(world);
        CreativeScreenHandler handler = new(player.inventory);

        handler.SetTab(CreativeInventoryTab.Food);
        List<ItemStack> foodStacks = GetCreativeStacks(handler, "_visibleStacks");
        Assert.Contains(foodStacks, stack => stack.itemId == Item.PumpkinPie.id);
        Assert.Contains(foodStacks, stack => stack.itemId == Item.GoldenApple.id);
        Assert.DoesNotContain(foodStacks, stack => stack.itemId == Item.GoldenCarrot.id);
        Assert.DoesNotContain(foodStacks, stack => stack.itemId == Block.Cake.id);

        handler.SetTab(CreativeInventoryTab.Brewing);
        List<ItemStack> brewingStacks = GetCreativeStacks(handler, "_visibleStacks");
        Assert.Contains(brewingStacks, stack => stack.itemId == Item.GoldenCarrot.id);
        Assert.Contains(brewingStacks, stack => stack.itemId == Item.RabbitFoot.id);
        Assert.DoesNotContain(brewingStacks, stack => stack.itemId == Item.NetherWart.id);
        Assert.DoesNotContain(brewingStacks, stack => stack.itemId == Item.SpiderEye.id);

        handler.SetTab(CreativeInventoryTab.Redstone);
        List<ItemStack> redstoneStacks = GetCreativeStacks(handler, "_visibleStacks");
        Assert.DoesNotContain(redstoneStacks, stack => stack.itemId == Block.PoweredRail.id);

        handler.SetTab(CreativeInventoryTab.Transportation);
        List<ItemStack> transportationStacks = GetCreativeStacks(handler, "_visibleStacks");
        Assert.Contains(transportationStacks, stack => stack.itemId == Block.PoweredRail.id);

        handler.SetTab(CreativeInventoryTab.Tools);
        List<ItemStack> toolStacks = GetCreativeStacks(handler, "_visibleStacks");
        Assert.Contains(toolStacks, stack => stack.itemId == Item.Lead.id);
        Assert.Contains(toolStacks, stack => stack.itemId == Item.NameTag.id);

        handler.SetTab(CreativeInventoryTab.Misc);
        List<ItemStack> miscStacks = GetCreativeStacks(handler, "_visibleStacks");
        Assert.Contains(miscStacks, stack => stack.itemId == Item.WritableBook.id);
    }

    [Fact]
    public void TranslationStorageContainsSurvivalCraftingLabel()
    {
        Assert.Equal("Crafting", TranslationStorage.Instance.TranslateKey("container.crafting"));
    }

    [Fact]
    public void VillageWeightedPieceListContainsAllOriginalHouseTypes()
    {
        HashSet<Type> seenPieceTypes = [];
        for (int seed = 1; seed <= 64; ++seed)
        {
            List<StructureVillagePieceWeight> pieces = StructureVillagePieces.GetStructureVillageWeightedPieceList(new JavaRandom(seed), 0);
            for (int i = 0; i < pieces.Count; ++i)
            {
                seenPieceTypes.Add(pieces[i].PieceType);
            }
        }

        Assert.Contains(typeof(ComponentVillageHouse4Garden), seenPieceTypes);
        Assert.Contains(typeof(ComponentVillageChurch), seenPieceTypes);
        Assert.Contains(typeof(ComponentVillageHouse1), seenPieceTypes);
        Assert.Contains(typeof(ComponentVillageWoodHut), seenPieceTypes);
        Assert.Contains(typeof(ComponentVillageHall), seenPieceTypes);
        Assert.Contains(typeof(ComponentVillageField), seenPieceTypes);
        Assert.Contains(typeof(ComponentVillageField2), seenPieceTypes);
        Assert.Contains(typeof(ComponentVillageHouse2), seenPieceTypes);
        Assert.Contains(typeof(ComponentVillageHouse3), seenPieceTypes);
    }

    [Fact]
    public void DesertVillageStartUsesSandstoneReplacements()
    {
        TestWorld world = new(new DesertVillageDimension(), 12345L);
        StructureVillageStart village = new(world, new JavaRandom(12345L), 0, 0, 0);

        village.GenerateStructure(world, new JavaRandom(12345L), new StructureBoundingBox(-64, 0, -64, 64, 128, 64));

        Assert.Equal(Block.Sandstone.id, world.Reader.GetBlockId(3, 64, 3));
        Assert.Equal(Block.Sandstone.id, world.Reader.GetBlockId(2, 63, 2));
    }

    [Fact]
    public void InvalidInventoryStackDoesNotCrashPlayerInventoryTick()
    {
        TestWorld world = new(new OverworldDimension(), 12345L);
        DummyPlayer player = new(world);

        player.inventory.setStack(0, new ItemStack(9999, 1, 0));
        player.inventory.inventoryTick();

        Assert.Null(player.inventory.getStack(0));
    }

    [Fact]
    public void SheepDropsMuttonOnDeath()
    {
        TestWorld world = new(new OverworldDimension(), 12345L);
        EntitySheep sheep = new(world);
        sheep.setPositionAndAngles(0.5D, 4.0D, 0.5D, 0.0F, 0.0F);
        Assert.True(world.SpawnEntity(sheep));

        Assert.True(sheep.damage(null!, sheep.health));

        List<EntityItem> drops = world.Entities.CollectEntitiesOfType<EntityItem>(new Box(-4.0D, 0.0D, -4.0D, 4.0D, 16.0D, 4.0D));
        Assert.Contains(drops, drop => drop.stack.itemId == Item.RawMutton.id);
    }

    [Fact]
    public void IronGolemAttackAnimationReturnsToIdleAfterAttackTicksExpire()
    {
        TestWorld world = new(new OverworldDimension(), 12345L);
        EntityIronGolem golem = new(world);
        ModelIronGolem model = new();

        golem.processServerEntityStatus(4);
        Assert.Equal(10, golem.AttackTicks);

        model.setLivingAnimations(golem, 0.0F, 0.0F, 0.0F);
        Assert.NotEqual(0.0F, model.RightArm.rotateAngleX);
        Assert.Equal(model.RightArm.rotateAngleX, model.LeftArm.rotateAngleX);

        for (int i = 0; i < 10; ++i)
        {
            golem.tick();
        }

        Assert.Equal(0, golem.AttackTicks);
        model.setLivingAnimations(golem, 0.0F, 0.0F, 0.0F);
        Assert.Equal(0.0F, model.RightArm.rotateAngleX);
        Assert.Equal(0.0F, model.LeftArm.rotateAngleX);
    }

    [Fact]
    public void IronGolemAttackAnimationExpiresForInterpolatedClientEntities()
    {
        TestWorld world = new(new OverworldDimension(), 12345L) { IsRemote = true };
        EntityIronGolem golem = new(world) { interpolateOnly = true };
        ModelIronGolem model = new();

        golem.processServerEntityStatus(4);
        Assert.Equal(10, golem.AttackTicks);

        for (int i = 0; i < 10; ++i)
        {
            golem.tick();
        }

        Assert.Equal(0, golem.AttackTicks);
        model.setLivingAnimations(golem, 0.0F, 0.0F, 0.0F);
        Assert.Equal(0.0F, model.RightArm.rotateAngleX);
        Assert.Equal(0.0F, model.LeftArm.rotateAngleX);
    }

    [Fact]
    public void RabbitRaidsFullyGrownCarrots()
    {
        TestWorld world = new(new OverworldDimension(), 12345L);
        world.Writer.SetBlock(4, 3, 2, Block.Farmland.id, 7);
        world.Writer.SetBlock(4, 4, 2, Block.Carrot.id, 7);

        EntityRabbit rabbit = new(world);
        rabbit.setPositionAndAngles(2.5D, 4.0D, 2.5D, 0.0F, 0.0F);
        Assert.True(world.SpawnEntity(rabbit));

        for (int i = 0; i < 200 && world.Reader.GetBlockId(4, 4, 2) == Block.Carrot.id; ++i)
        {
            rabbit.tick();
        }

        Assert.Equal(0, world.Reader.GetBlockId(4, 4, 2));
    }

    [Fact]
    public void PickupAllCollectsMatchingStacksFromSameInventory()
    {
        TestWorld world = new(new OverworldDimension(), 12345L);
        DummyPlayer player = new(world);
        InventoryBasic inventory = new("pickupAll", 3);
        inventory.setStack(0, new ItemStack(Item.Stick, 1));
        inventory.setStack(1, new ItemStack(Item.Stick, 2));
        inventory.setStack(2, new ItemStack(Item.Bread, 1));

        DummyScreenHandler handler = new();
        handler.AddTestSlot(new Slot(inventory, 0, 0, 0));
        handler.AddTestSlot(new Slot(inventory, 1, 18, 0));
        handler.AddTestSlot(new Slot(inventory, 2, 36, 0));

        player.inventory.setItemStack(new ItemStack(Item.Stick, 1));

        handler.onSlotClick(0, 0, ScreenHandlerClickMode.PickupAll, player);

        Assert.NotNull(player.inventory.getCursorStack());
        Assert.Equal(4, player.inventory.getCursorStack()!.count);
        Assert.Null(inventory.getStack(0));
        Assert.Null(inventory.getStack(1));
        Assert.NotNull(inventory.getStack(2));
    }

    [Fact]
    public void DragSplitDistributesCursorStackAcrossSelectedSlots()
    {
        TestWorld world = new(new OverworldDimension(), 12345L);
        DummyPlayer player = new(world);
        InventoryBasic inventory = new("dragSplit", 3);

        DummyScreenHandler handler = new();
        handler.AddTestSlot(new Slot(inventory, 0, 0, 0));
        handler.AddTestSlot(new Slot(inventory, 1, 18, 0));
        handler.AddTestSlot(new Slot(inventory, 2, 36, 0));

        player.inventory.setItemStack(new ItemStack(Item.Stick, 6));

        handler.onSlotClick(-999, ScreenHandler.PackDragData(0, 0), ScreenHandlerClickMode.Drag, player);
        handler.onSlotClick(0, ScreenHandler.PackDragData(1, 0), ScreenHandlerClickMode.Drag, player);
        handler.onSlotClick(1, ScreenHandler.PackDragData(1, 0), ScreenHandlerClickMode.Drag, player);
        handler.onSlotClick(2, ScreenHandler.PackDragData(1, 0), ScreenHandlerClickMode.Drag, player);
        handler.onSlotClick(-999, ScreenHandler.PackDragData(2, 0), ScreenHandlerClickMode.Drag, player);

        Assert.Equal(2, inventory.getStack(0)!.count);
        Assert.Equal(2, inventory.getStack(1)!.count);
        Assert.Equal(2, inventory.getStack(2)!.count);
        Assert.Null(player.inventory.getCursorStack());
    }

    [Fact]
    public void CreativeBowFiresWithoutArrows()
    {
        TestWorld world = new(new OverworldDimension(), 12345L);
        DummyPlayer player = new(world);
        player.capabilities.SetGameMode(BetaSharp.Worlds.Core.Systems.GameMode.Creative);

        ItemStack bow = new(Item.BOW);

        Item.BOW.use(bow, world, player);

        List<EntityArrow> arrows = world.Entities.CollectEntitiesOfType<EntityArrow>(new Box(-8.0D, -8.0D, -8.0D, 8.0D, 16.0D, 8.0D));
        Assert.Single(arrows);
        Assert.False(player.inventory.consumeInventoryItem(Item.ARROW.id));
    }

    [Fact]
    public void NetherWartSeedsPlaceOnlyOnSoulSand()
    {
        TestWorld world = new(new OverworldDimension(), 12345L);
        DummyPlayer player = new(world);
        ItemStack netherWart = new(Item.NetherWart);

        world.Writer.SetBlock(0, 4, 0, Block.Soulsand.id);
        world.Writer.SetBlock(1, 4, 0, Block.Farmland.id);

        Assert.True(Item.NetherWart.useOnBlock(netherWart, player, world, 0, 4, 0, 1));
        Assert.Equal(Block.NetherWart.id, world.Reader.GetBlockId(0, 5, 0));

        ItemStack secondNetherWart = new(Item.NetherWart);
        Assert.False(Item.NetherWart.useOnBlock(secondNetherWart, player, world, 1, 4, 0, 1));
        Assert.Equal(0, world.Reader.GetBlockId(1, 5, 0));
    }

    [Fact]
    public void FullyGrownNetherWartDropsMultipleItems()
    {
        TestWorld world = new(new OverworldDimension(), 12345L);
        world.Writer.SetBlock(0, 4, 0, Block.Soulsand.id);
        world.Writer.SetBlockWithoutCallingOnPlaced(0, 5, 0, Block.NetherWart.id, 3);

        Block.NetherWart.dropStacks(new OnDropEvent(world, 0, 5, 0, 3));

        List<EntityItem> drops = world.Entities.CollectEntitiesOfType<EntityItem>(new Box(-4.0D, 0.0D, -4.0D, 4.0D, 16.0D, 4.0D));
        List<EntityItem> wartDrops = drops.Where(drop => drop.stack.itemId == Item.NetherWart.id).ToList();

        Assert.InRange(wartDrops.Count, 2, 4);
    }

    [Fact]
    public void SkullBlockUsesDedicatedHeadTextures()
    {
        Assert.Equal(225, Block.Skull.getTexture(2, 0));
        Assert.Equal(240, Block.Skull.getTexture(1, 0));
    }

    [Fact]
    public void TrapdoorRemainsPlacedAfterSupportBlockIsRemoved()
    {
        TestWorld world = new(new OverworldDimension(), 12345L);
        DummyPlayer player = new(world);
        ItemStack trapdoor = new(Block.Trapdoor);

        world.Writer.SetBlock(0, 4, 0, Block.Cobblestone.id);
        Assert.True(Item.ITEMS[Block.Trapdoor.id].useOnBlock(trapdoor, player, world, 0, 4, 0, 1));
        Assert.Equal(Block.Trapdoor.id, world.Reader.GetBlockId(0, 5, 0));

        world.Writer.SetBlock(0, 4, 0, 0);

        Assert.Equal(Block.Trapdoor.id, world.Reader.GetBlockId(0, 5, 0));
    }

    [Fact]
    public void EnchantedGoldenAppleGrantsAbsorptionAndFoil()
    {
        TestWorld world = new(new OverworldDimension(), 12345L);
        DummyPlayer player = new(world);
        ItemStack apple = new(Item.GoldenApple.id, 1, 1);

        Assert.True(Item.GoldenApple.hasEffect(apple));

        Item.GoldenApple.use(apple, world, player);

        Assert.True(player.hasPotionEffect(Potion.Absorption));
        Assert.True(player.hasPotionEffect(Potion.Regeneration));
        Assert.True(player.hasPotionEffect(Potion.Resistance));
        Assert.True(player.hasPotionEffect(Potion.FireResistance));
    }

    [Fact]
    public void SilverfishConvertsNearbyMonsterEggsBackIntoHostBlocksWhenDamaged()
    {
        TestWorld world = new(new OverworldDimension(), 12345L);
        EntitySilverfish silverfish = new(world);
        silverfish.setPositionAndAngles(0.5D, 4.0D, 0.5D, 0.0F, 0.0F);
        Assert.True(world.SpawnEntity(silverfish));
        world.Writer.SetBlockWithoutCallingOnPlaced(1, 4, 0, Block.Silverfish.id, 5);

        Assert.True(silverfish.damage(null!, 1));

        for (int i = 0; i < 20; ++i)
        {
            silverfish.tickLiving();
        }

        Assert.Equal(Block.StoneBrick.id, world.Reader.GetBlockId(1, 4, 0));
        Assert.Equal(3, world.Reader.GetBlockMeta(1, 4, 0));

        List<EntitySilverfish> silverfishEntities = world.Entities.CollectEntitiesOfType<EntitySilverfish>(new Box(-8.0D, 0.0D, -8.0D, 8.0D, 16.0D, 8.0D));
        Assert.True(silverfishEntities.Count >= 2);
    }

    [Fact]
    public void NetherFortressGeneratorPlacesNetherBrickWartAndBlazeSpawner()
    {
        TestWorld world = new(new NetherDimension(), 12345L);
        MapGenNetherFortress fortress = new();

        Vec3i? fortressPos = fortress.FindNearestStructure(world, 0, 64, 0);

        Assert.NotNull(fortressPos);

        int chunkX = fortressPos!.Value.X >> 4;
        int chunkZ = fortressPos.Value.Z >> 4;
        for (int x = chunkX - 8; x <= chunkX + 8; ++x)
        {
            for (int z = chunkZ - 8; z <= chunkZ + 8; ++z)
            {
                fortress.GenerateStructuresInChunk(world, new JavaRandom(world.Seed + x * 31L + z * 17L), x, z);
            }
        }

        bool foundNetherBrick = false;
        bool foundNetherWart = false;
        bool foundBlazeSpawner = false;
        bool foundChest = false;
        for (int x = fortressPos.Value.X - 96; x <= fortressPos.Value.X + 96; ++x)
        {
            for (int y = fortressPos.Value.Y - 16; y <= fortressPos.Value.Y + 32; ++y)
            {
                for (int z = fortressPos.Value.Z - 96; z <= fortressPos.Value.Z + 96; ++z)
                {
                    int blockId = world.Reader.GetBlockId(x, y, z);
                    foundNetherBrick |= blockId == Block.NetherBrick.id;
                    foundNetherWart |= blockId == Block.NetherWart.id;
                    foundBlazeSpawner |= blockId == Block.Spawner.id;
                    foundChest |= blockId == Block.Chest.id;
                }
            }
        }

        Assert.True(foundNetherBrick);
        Assert.True(foundNetherWart);
        Assert.True(foundBlazeSpawner);
        Assert.True(foundChest);
    }

    [Fact]
    public void MonsterPlacerChangesSpawnerMobWithoutConsumingCreativeStack()
    {
        TestWorld world = new(new OverworldDimension(), 12345L);
        DummyPlayer player = new(world);
        player.SetGameMode(1);
        world.Writer.SetBlock(0, 4, 0, Block.Spawner.id);
        ItemStack spawnEgg = new(Item.MonsterPlacer, 1, 61);

        bool used = spawnEgg.useOnBlock(player, world, 0, 4, 0, 1);

        Assert.True(used);
        Assert.Equal(1, spawnEgg.count);
        Assert.Equal("Blaze", world.Entities.GetBlockEntity<BlockEntityMobSpawner>(0, 4, 0)!.GetSpawnedEntityId());
    }

    [Fact]
    public void MobSpawnerPersistsOnePointEightSettings()
    {
        BlockEntityMobSpawner spawner = new();
        spawner.SetSpawnedEntityId("WitherSkeleton");
        spawner.SpawnDelay = 42;
        spawner.MinSpawnDelay = 120;
        spawner.MaxSpawnDelay = 480;
        spawner.SpawnCount = 3;
        spawner.MaxNearbyEntities = 9;
        spawner.RequiredPlayerRange = 20;
        spawner.SpawnRange = 6;

        NBTTagCompound nbt = new();
        spawner.writeNbt(nbt);

        BlockEntityMobSpawner copy = new();
        copy.readNbt(nbt);

        Assert.Equal("WitherSkeleton", copy.GetSpawnedEntityId());
        Assert.Equal(42, copy.SpawnDelay);
        Assert.Equal(120, copy.MinSpawnDelay);
        Assert.Equal(480, copy.MaxSpawnDelay);
        Assert.Equal(3, copy.SpawnCount);
        Assert.Equal(9, copy.MaxNearbyEntities);
        Assert.Equal(20, copy.RequiredPlayerRange);
        Assert.Equal(6, copy.SpawnRange);
    }

    [Fact]
    public void LocateCommandNormalizesFortressAliases()
    {
        Assert.Equal("Fortress", LocateCommand.NormalizeStructureId("fortress"));
        Assert.Equal("Fortress", LocateCommand.NormalizeStructureId("netherfortress"));
        Assert.Equal("Fortress", LocateCommand.NormalizeStructureId("nether_fortress"));
    }

    [Fact]
    public void SkullItemPlacesSkullBlock()
    {
        TestWorld world = new(new OverworldDimension(), 12345L);
        DummyPlayer player = new(world);
        world.Writer.SetBlockWithoutCallingOnPlaced(0, 4, 0, Block.Stone.id, 0);
        ItemStack skull = new(Item.Skull);

        bool used = skull.useOnBlock(player, world, 0, 4, 0, 1);

        Assert.True(used);
        Assert.Equal(Block.Skull.id, world.Reader.GetBlockId(0, 5, 0));
    }

    private static object GetCraftingManager()
    {
        Type craftingManagerType = typeof(Item).Assembly.GetType("BetaSharp.Recipes.CraftingManager")!;
        return craftingManagerType.GetMethod("getInstance", BindingFlags.Static | BindingFlags.Public)!.Invoke(null, null)!;
    }

    private static List<ItemStack> GetCreativeStacks(CreativeScreenHandler handler, string fieldName)
    {
        FieldInfo field = typeof(CreativeScreenHandler).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)!;
        return (List<ItemStack>)field.GetValue(handler)!;
    }

    private static int BrewPotionDamage(int baseDamage, params Item[] ingredients)
    {
        int damage = baseDamage;
        for (int i = 0; i < ingredients.Length; ++i)
        {
            damage = PotionHelper.ApplyIngredient(damage, ingredients[i].getPotionEffect()!);
        }

        return damage;
    }

    private static void RunBrewingCycle(BlockEntityBrewingStand brewingStand, TestWorld world)
    {
        for (int i = 0; i <= 400; ++i)
        {
            brewingStand.tick(world.Entities);
        }
    }

    private sealed class TestWorld : World
    {
        public TestWorld(Dimension dimension, long seed, WorldType terrainType)
            : base(new EmptyWorldStorage(), "test", new WorldSettings(seed, terrainType), dimension)
        {
            SetDifficulty(1);
            EventProcessingEnabled = true;
        }

        public TestWorld(Dimension dimension, long seed)
            : this(dimension, seed, WorldType.Flat)
        {
        }

        protected override IChunkSource CreateChunkCache() => new TestChunkSource(this);
    }

    private sealed class DesertVillageDimension : Dimension
    {
        public override void InitBiomeSource() => BiomeSource = new DesertVillageBiomeSource();
    }

    private sealed class DesertVillageBiomeSource : BiomeSource
    {
        public override Biome GetBiome(ChunkPos chunkPos) => Biome.Desert;

        public override Biome GetBiome(int x, int z) => Biome.Desert;

        public override double GetTemperature(int x, int z) => 2.0D;

        public override double GetTemperature(int x, int y, int z) => 2.0D;

        public override double GetDownfall(int x, int z) => 0.0D;

        public override Biome[] GetBiomesInArea(int x, int z, int width, int depth)
        {
            return GetBiomesInArea(null, x, z, width, depth);
        }

        public override Biome[] GetBiomesInArea(Biome[]? biomes, int x, int z, int width, int depth)
        {
            int size = width * depth;
            if (biomes == null || biomes.Length < size)
            {
                biomes = new Biome[size];
            }

            Array.Fill(biomes, Biome.Desert);
            return biomes;
        }

        public override Biome[] GetBiomesForGeneration(Biome[]? biomes, int x, int z, int width, int depth)
        {
            return GetBiomesInArea(biomes, x, z, width, depth);
        }

        public override bool AreBiomesViable(int x, int z, int radius, ICollection<Biome> allowed)
        {
            return allowed.Contains(Biome.Desert);
        }
    }

    private sealed class DummyPlayer(IWorldContext world) : EntityPlayer(world)
    {
        public override EntityType Type => EntityRegistry.Player;

        public override void spawn()
        {
        }
    }

    private sealed class TestStructurePiece : StructureComponent
    {
        public TestStructurePiece() : base(0)
        {
            BoundingBox = new StructureBoundingBox();
        }

        public override bool AddComponentParts(IWorldContext world, JavaRandom random, StructureBoundingBox bounds) => true;

        public void PlaceTorch(IWorldContext world, StructureBoundingBox bounds, int x, int y, int z)
        {
            PlaceBlockAtCurrentPosition(world, Block.Torch.id, 0, x, y, z, bounds);
        }
    }

    private sealed class MetadataPlacementProbe(int facing, int x, int y, int z) : StructureComponent(0)
    {
        public override bool AddComponentParts(IWorldContext world, JavaRandom random, StructureBoundingBox bounds) => true;

        public int MapMeta(int blockId, int meta) => GetMetadataWithOffset(blockId, meta);

        public void Place(IWorldContext world, int blockId, int meta, int localX, int localY, int localZ, StructureBoundingBox bounds)
        {
            Facing = facing;
            BoundingBox = new StructureBoundingBox(x, y, z, x, y, z);
            PlaceBlockAtCurrentPosition(world, blockId, meta, localX, localY, localZ, bounds);
        }
    }

    private sealed class DummyScreenHandler : ScreenHandler
    {
        public void AddTestSlot(Slot slot)
        {
            AddSlot(slot);
        }

        public override bool canUse(EntityPlayer player) => true;
    }

    private sealed class DummyMerchant(MerchantRecipeList recipes) : IMerchant
    {
        private EntityPlayer? _customer;

        public EntityPlayer? GetCustomer() => _customer;

        public void SetCustomer(EntityPlayer? player)
        {
            _customer = player;
        }

        public MerchantRecipeList GetRecipes(EntityPlayer player) => recipes;

        public void SetRecipes(MerchantRecipeList newRecipes)
        {
        }

        public void UseRecipe(MerchantRecipe recipe)
        {
            recipe.IncrementToolUses();
        }
    }

    private sealed class TestChunkSource(TestWorld world) : IChunkSource
    {
        private readonly Dictionary<ChunkPos, Chunk> _chunks = [];

        public bool IsChunkLoaded(int x, int z) => true;

        public Chunk GetChunk(int x, int z)
        {
            ChunkPos pos = new(x, z);
            if (_chunks.TryGetValue(pos, out Chunk? chunk))
            {
                return chunk;
            }

            chunk = new Chunk(world, new byte[-short.MinValue], x, z);
            _chunks[pos] = chunk;
            InitializeTerrain(chunk);
            chunk.PopulateHeightMapOnly();
            return chunk;
        }

        public Chunk LoadChunk(int x, int z) => GetChunk(x, z);

        public void DecorateTerrain(IChunkSource source, int x, int z)
        {
        }

        public bool Save(bool saveEntities, LoadingDisplay display) => true;

        public bool Tick() => false;

        public bool CanSave() => true;

        public string GetDebugInfo() => "TestChunkSource";

        private void InitializeTerrain(Chunk chunk)
        {
            for (int localX = 0; localX < 16; ++localX)
            {
                for (int localZ = 0; localZ < 16; ++localZ)
                {
                    if (world.Dimension.Id == 1)
                    {
                        for (int y = 0; y <= 63; ++y)
                        {
                            SetBlock(chunk, localX, y, localZ, Block.EndStone.id);
                        }
                    }
                    else if (world.Dimension.Id == -1)
                    {
                        for (int y = 0; y <= 31; ++y)
                        {
                            SetBlock(chunk, localX, y, localZ, Block.Netherrack.id);
                        }

                        for (int y = 124; y <= 127; ++y)
                        {
                            SetBlock(chunk, localX, y, localZ, Block.Bedrock.id);
                        }
                    }
                    else
                    {
                        SetBlock(chunk, localX, 0, localZ, Block.Bedrock.id);
                        SetBlock(chunk, localX, 1, localZ, Block.Dirt.id);
                        SetBlock(chunk, localX, 2, localZ, Block.Dirt.id);
                        SetBlock(chunk, localX, 3, localZ, Block.GrassBlock.id);
                    }
                }
            }
        }

        private static void SetBlock(Chunk chunk, int localX, int y, int localZ, int blockId)
        {
            chunk.Blocks[localX << 11 | localZ << 7 | y] = (byte)blockId;
        }
    }
}
