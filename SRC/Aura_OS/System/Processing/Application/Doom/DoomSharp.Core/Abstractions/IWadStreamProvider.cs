using DoomSharp.Core.Data;
using System.Threading.Tasks;

namespace DoomSharp.Core.Abstractions
{
    public interface IWadStreamProvider
    {
        WadFile LoadFromFile(string file);
    }
}
