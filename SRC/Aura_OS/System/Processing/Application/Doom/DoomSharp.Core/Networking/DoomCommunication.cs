namespace DoomSharp.Core.Networking;

public class DoomCommunication
{
    // Supposed to be DOOMCOM_ID?
    public long Id { get; set; }

    // DOOM executes an int to execute commands.
    public short IntNum { get; set; }

    // Communication between DOOM and the driver.
    // Is CMD_SEND or CMD_GET.
    public Command Command { get; set; }

    // Is dest for send, set by get (-1 = no packet).
    public int RemoteNode { get; set; }

    // Number of bytes in doomdata to be sent
    public int DataLength { get; set; }

    // Info common to all nodes.
    // Console is allways node 0.
    public short NumNodes { get; set; }
    // Flag: 1 = no duplication, 2-5 = dup for slow nets.
    public short TicDup { get; set; }
    // Flag: 1 = send a backup tic in every packet.
    public short ExtraTics { get; set; }
    // Flag: 1 = deathmatch.
    public bool DeathMatch { get; set; }
    // Flag: -1 = new game, 0-5 = load savegame
    public short SaveGame { get; set; }
    public short Episode { get; set; }	// 1-3
    public short Map { get; set; }		// 1-9
    public short Skill { get; set; }		// 1-5

    // Info specific to this node.
    public short ConsolePlayer { get; set; }
    public short NumPlayers { get; set; }

    // These are related to the 3-display mode,
    //  in which two drones looking left and right
    //  were used to render two additional views
    //  on two additional computers.
    // Probably not operational anymore.
    // 1 = left, 0 = center, -1 = right
    public short AngleOffset { get; set; }
    // 1 = drone
    public short Drone { get; set; }

    // The packet data to be sent.
    public DoomData Data { get; set; } = new();
}