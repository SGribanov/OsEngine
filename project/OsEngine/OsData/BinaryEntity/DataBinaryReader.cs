#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

using System.IO;

namespace OsEngine.OsData.BinaryEntity
{
    public class DataBinaryReader : BinaryReader
    {
        public DataBinaryReader(Stream stream) : base(stream) { }

        public long ReadGrowing(long lastValue)
        {
            uint offset = ULeb128.Read(BaseStream);

            if (offset == ULeb128.Max4BValue)
                return lastValue + Leb128.Read(BaseStream);
            else
                return lastValue + offset;
        }

        public long ReadLeb128() { return Leb128.Read(BaseStream); }
    }
}

