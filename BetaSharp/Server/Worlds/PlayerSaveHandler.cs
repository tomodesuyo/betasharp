using BetaSharp.Entities;

namespace BetaSharp.Server.Worlds;

public interface IPlayerStorage
{
    void SavePlayerData(EntityPlayer player);

    bool LoadPlayerData(EntityPlayer player);
}
