using LibDotNetParser.PE;

namespace LibDotNetParser.DotNet.Tabels
{
    public interface IMetadataTableRow
    {
        void Read(MetadataReader reader);
    }
}
