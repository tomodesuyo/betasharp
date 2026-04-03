namespace BetaSharp.NBT;

public static class NbtUtils
{
    public static T DeepCopy<T>(T tag) where T : NBTBase
    {
        using MemoryStream stream = new();
        NBTBase.WriteTag(tag, stream);
        stream.Position = 0;
        return (T)NBTBase.ReadTag(stream);
    }

    public static void MergeInto(NBTTagCompound target, NBTTagCompound source)
    {
        foreach ((string key, NBTBase value) in source.Dictionary)
        {
            target.SetTag(key, DeepCopy(value));
        }
    }
}
