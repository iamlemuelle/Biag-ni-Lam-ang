using System;

[Serializable]
public class UserData
{
    public string userName;
    public string userAuthId;
    public GameInfo userGamePreferences = new GameInfo();
}