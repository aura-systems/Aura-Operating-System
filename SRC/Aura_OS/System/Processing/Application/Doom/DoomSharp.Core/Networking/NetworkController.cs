namespace DoomSharp.Core.Networking;

public class NetworkController
{
    public DoomCommunication DoomCom { get; } = new();

    public void Initialize()
    {
        DoomCom.TicDup = 1;
        DoomCom.ExtraTics = 0;
        
        // Single player support only
        DoomGame.Instance.Game.NetGame = false;
        DoomCom.Id = Constants.DoomComId;
        DoomCom.NumPlayers = DoomCom.NumNodes = 1;
        DoomCom.DeathMatch = false;
        DoomCom.ConsolePlayer = 0;

        // when multiplayer: setup netsend/netget functions
    }

    public void NetworkCommand()
    {
        if (DoomCom.Command == Command.Send)
        {
            NetSend();
            return;
        }

        if (DoomCom.Command == Command.Get)
        {
            NetGet();
            return;
        }

        DoomGame.Error($"Bad net cmd: {DoomCom.Command}");
    }

    private void NetSend()
    {

    }

    private void NetGet()
    {

    }
}