using BetaSharp.Blocks;
using BetaSharp.Items;
using BetaSharp.NBT;
using BetaSharp.Util;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Entities;

public sealed class EntityEnderman : EntityMonster
{
    private static readonly bool[] s_canCarryBlocks = BuildCarryTable();

    private readonly SyncedProperty<byte> _carriedId;
    private readonly SyncedProperty<byte> _carriedData;
    private int _teleportDelay;
    private int _stareTicks;
    public bool IsAggressive { get; private set; }

    public override EntityType Type => EntityRegistry.Enderman;

    public EntityEnderman(IWorldContext world) : base(world)
    {
        texture = "/mob/enderman.png";
        movementSpeed = 0.3F;
        attackStrength = 7;
        health = 40;
        setBoundingBoxSpacing(0.6F, 2.9F);
        stepHeight = 1.0F;
        _carriedId = DataSynchronizer.MakeProperty(16, (byte)0);
        _carriedData = DataSynchronizer.MakeProperty(17, (byte)0);
    }

    public override string getTexture() => "/mob/enderman.png";

    public int GetCarried() => _carriedId.Value & 255;

    public int GetCarryingData() => _carriedData.Value & 255;

    public void SetCarried(int blockId) => _carriedId.Value = (byte)blockId;

    public void SetCarryingData(int meta) => _carriedData.Value = (byte)meta;

    protected override Entity? findPlayerToAttack()
    {
        EntityPlayer? player = world.Entities.GetClosestPlayerTarget(x, y, z, 64.0D);
        if (player != null)
        {
            if (ShouldAttackPlayer(player))
            {
                if (++_stareTicks >= 5)
                {
                    _stareTicks = 0;
                    return player;
                }
            }
            else
            {
                _stareTicks = 0;
            }
        }

        return null;
    }

    public override void tickMovement()
    {
        if (isWet())
        {
            damage(null!, 1);
        }

        IsAggressive = playerToAttack != null;
        movementSpeed = playerToAttack != null ? 0.65F : 0.3F;

        if (!world.IsRemote)
        {
            TryPickupOrPlaceBlock();
        }

        for (int i = 0; i < 2; ++i)
        {
            world.Broadcaster.AddParticle("portal", x + (random.NextDouble() - 0.5D) * width, y + random.NextDouble() * height - 0.25D, z + (random.NextDouble() - 0.5D) * width, (random.NextDouble() - 0.5D) * 2.0D, -random.NextDouble(), (random.NextDouble() - 0.5D) * 2.0D);
        }

        if (!world.IsRemote)
        {
            float brightness = getBrightnessAtEyes(1.0F);
            bool isDay = world.GetTime() % 24000L < 12000L;
            int topY = world.Reader.GetTopSolidBlockY(MathHelper.Floor(x), MathHelper.Floor(z));
            if (isDay && brightness > 0.5F && MathHelper.Floor(y) >= topY && random.NextFloat() * 30.0F < (brightness - 0.4F) * 2.0F)
            {
                playerToAttack = null;
                TeleportRandomly();
            }

            if (isWet())
            {
                playerToAttack = null;
                TeleportRandomly();
            }
        }

        base.tickMovement();

        if (!world.IsRemote && isAlive())
        {
            if (playerToAttack is EntityPlayer player && ShouldAttackPlayer(player))
            {
                sidewaysSpeed = 0.0F;
                forwardSpeed = 0.0F;
                if (playerToAttack.getSquaredDistance(this) < 16.0D)
                {
                    TeleportRandomly();
                }

                _teleportDelay = 0;
            }
            else if (playerToAttack != null)
            {
                if (playerToAttack.getSquaredDistance(this) > 256.0D && ++_teleportDelay >= 30 && TeleportToEntity(playerToAttack))
                {
                    _teleportDelay = 0;
                }
            }
            else
            {
                _teleportDelay = 0;
            }
        }
    }

    protected override string getLivingSound() => "mob.endermen.idle";
    protected override string getHurtSound() => "mob.endermen.hit";
    protected override string getDeathSound() => "mob.endermen.death";
    protected override int getDropItemId() => Item.EnderPearl.id;

    public override void writeNbt(NBTTagCompound nbt)
    {
        base.writeNbt(nbt);
        nbt.SetShort("carried", (short)GetCarried());
        nbt.SetShort("carriedData", (short)GetCarryingData());
    }

    public override void readNbt(NBTTagCompound nbt)
    {
        base.readNbt(nbt);
        SetCarried(nbt.GetShort("carried"));
        SetCarryingData(nbt.GetShort("carriedData"));
    }

    private bool ShouldAttackPlayer(EntityPlayer player)
    {
        ItemStack? helmet = player.inventory.armor[3];
        if (helmet != null && helmet.itemId == Block.Pumpkin.id)
        {
            return false;
        }

        Vec3D look = player.getLook(1.0F).normalize();
        Vec3D delta = new(x - player.x, boundingBox.MinY + height / 2.0D - (player.y + player.getEyeHeight()), z - player.z);
        double distance = delta.magnitude();
        delta = delta.normalize();
        double dot = look.x * delta.x + look.y * delta.y + look.z * delta.z;
        return dot > 1.0D - 0.025D / distance && player.canSee(this);
    }

    private void TryPickupOrPlaceBlock()
    {
        if (GetCarried() == 0)
        {
            if (random.NextInt(20) != 0)
            {
                return;
            }

            int pickupX = MathHelper.Floor(x - 2.0D + random.NextDouble() * 4.0D);
            int pickupY = MathHelper.Floor(y + random.NextDouble() * 3.0D);
            int pickupZ = MathHelper.Floor(z - 2.0D + random.NextDouble() * 4.0D);
            int blockId = world.Reader.GetBlockId(pickupX, pickupY, pickupZ);
            if (blockId > 0 && s_canCarryBlocks[blockId])
            {
                SetCarried(blockId);
                SetCarryingData(world.Reader.GetBlockMeta(pickupX, pickupY, pickupZ));
                world.Writer.SetBlock(pickupX, pickupY, pickupZ, 0, 0, false);
            }

            return;
        }

        if (random.NextInt(2000) != 0)
        {
            return;
        }

        int placeX = MathHelper.Floor(x - 1.0D + random.NextDouble() * 2.0D);
        int placeY = MathHelper.Floor(y + random.NextDouble() * 2.0D);
        int placeZ = MathHelper.Floor(z - 1.0D + random.NextDouble() * 2.0D);
        int placeBlock = world.Reader.GetBlockId(placeX, placeY, placeZ);
        int belowBlock = world.Reader.GetBlockId(placeX, placeY - 1, placeZ);
        if (placeBlock == 0 && belowBlock > 0)
        {
            Block below = Block.Blocks[belowBlock];
            if (below.isFullCube() && below.isOpaque())
            {
                world.Writer.SetBlock(placeX, placeY, placeZ, GetCarried(), GetCarryingData(), false);
                SetCarried(0);
                SetCarryingData(0);
            }
        }
    }

    private bool TeleportRandomly()
    {
        return TeleportTo(x + (random.NextDouble() - 0.5D) * 64.0D, y + random.NextInt(64) - 32, z + (random.NextDouble() - 0.5D) * 64.0D);
    }

    private bool TeleportToEntity(Entity target)
    {
        Vec3D delta = new(x - target.x, boundingBox.MinY + height / 2.0D - target.y - target.getEyeHeight(), z - target.z);
        delta = delta.normalize();
        return TeleportTo(x + (random.NextDouble() - 0.5D) * 8.0D - delta.x * 16.0D, y + random.NextInt(16) - 8 - delta.y * 16.0D, z + (random.NextDouble() - 0.5D) * 8.0D - delta.z * 16.0D);
    }

    private bool TeleportTo(double targetX, double targetY, double targetZ)
    {
        double prevX = x;
        double prevY = y;
        double prevZ = z;
        x = targetX;
        y = targetY;
        z = targetZ;

        int blockX = MathHelper.Floor(x);
        int blockY = MathHelper.Floor(y);
        int blockZ = MathHelper.Floor(z);
        bool foundGround = false;

        while (!foundGround && blockY > 0)
        {
            int belowId = world.Reader.GetBlockId(blockX, blockY - 1, blockZ);
            if (belowId > 0 && Block.Blocks[belowId].material.BlocksMovement)
            {
                foundGround = true;
            }
            else
            {
                --y;
                --blockY;
            }
        }

        if (!foundGround)
        {
            setPosition(prevX, prevY, prevZ);
            return false;
        }

        setPosition(x, y, z);
        if (world.Entities.GetEntityCollisionsScratch(this, boundingBox).Count > 0 || world.Reader.IsMaterialInBox(boundingBox, m => m.IsFluid))
        {
            setPosition(prevX, prevY, prevZ);
            return false;
        }

        for (int i = 0; i < 128; ++i)
        {
            double progress = i / 127.0D;
            float velX = (random.NextFloat() - 0.5F) * 0.2F;
            float velY = (random.NextFloat() - 0.5F) * 0.2F;
            float velZ = (random.NextFloat() - 0.5F) * 0.2F;
            double particleX = prevX + (x - prevX) * progress + (random.NextDouble() - 0.5D) * width * 2.0D;
            double particleY = prevY + (y - prevY) * progress + random.NextDouble() * height;
            double particleZ = prevZ + (z - prevZ) * progress + (random.NextDouble() - 0.5D) * width * 2.0D;
            world.Broadcaster.AddParticle("portal", particleX, particleY, particleZ, velX, velY, velZ);
        }

        world.Broadcaster.PlaySoundAtEntity(this, "mob.endermen.portal", 1.0F, 1.0F);
        return true;
    }

    private static bool[] BuildCarryTable()
    {
        bool[] carry = new bool[256];
        carry[Block.GrassBlock.id] = true;
        carry[Block.Dirt.id] = true;
        carry[Block.Sand.id] = true;
        carry[Block.Gravel.id] = true;
        carry[Block.Dandelion.id] = true;
        carry[Block.Rose.id] = true;
        carry[Block.BrownMushroom.id] = true;
        carry[Block.RedMushroom.id] = true;
        carry[Block.TNT.id] = true;
        carry[Block.Cactus.id] = true;
        carry[Block.Clay.id] = true;
        carry[Block.Pumpkin.id] = true;
        carry[Block.Melon.id] = true;
        carry[Block.Mycelium.id] = true;
        return carry;
    }
}
