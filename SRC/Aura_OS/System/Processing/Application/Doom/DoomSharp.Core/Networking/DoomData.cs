namespace DoomSharp.Core.Networking;

public class DoomData
{
    public uint CheckSum { get; set; }
    public byte RetransmitFrom { get; set; }
    public byte StartTic { get; set; }
    public byte Player { get; set; }
    public byte NumTics { get; set; }

    public TicCommand[] Commands { get; } =
    {
        new(),new(),new(),new(),
        new(),new(),new(),new(),
        new(),new(),new(),new()
    };
}