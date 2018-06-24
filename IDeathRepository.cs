namespace TerrariaDeathCounter
{
    interface IDeathRepository
    {
        int RecordDeath(string playerName, string killerName);
        int GetNumberOfDeaths(string playerName, string killerName);
    }
}
