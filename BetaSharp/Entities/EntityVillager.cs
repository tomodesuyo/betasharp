using BetaSharp.NBT;
using BetaSharp.Util;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Entities;

public sealed class EntityVillager : EntityAnimal
{
    public override EntityType Type => EntityRegistry.Villager;
    public readonly SyncedProperty<int> Profession;

    public EntityVillager(IWorldContext world) : this(world, 0)
    {
    }

    public EntityVillager(IWorldContext world, int profession) : base(world)
    {
        texture = "/mob/villager/villager.png";
        setBoundingBoxSpacing(0.6F, 1.8F);
        movementSpeed = 0.5F;
        health = 20;
        Profession = DataSynchronizer.MakeProperty(16, profession);
    }

    public override void tickLiving()
    {
        base.tickLiving();
    }

    public override string getTexture()
    {
        return Profession.Value switch
        {
            0 => "/mob/villager/farmer.png",
            1 => "/mob/villager/librarian.png",
            2 => "/mob/villager/priest.png",
            3 => "/mob/villager/smith.png",
            4 => "/mob/villager/butcher.png",
            _ => "/mob/villager/villager.png",
        };
    }

    protected override string getLivingSound() => "mob.villager.default";
    protected override string getHurtSound() => "mob.villager.defaulthurt";
    protected override string getDeathSound() => "mob.villager.defaultdeath";

    public override void writeNbt(NBTTagCompound nbt)
    {
        base.writeNbt(nbt);
        nbt.SetInteger("Profession", Profession.Value);
    }

    public override void readNbt(NBTTagCompound nbt)
    {
        base.readNbt(nbt);
        Profession.Value = nbt.GetInteger("Profession");
    }
}
