#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

using System.IO;

namespace OsEngine.OsData.BinaryEntity
{
    public class DataBinaryWriter : BinaryWriter
    {
        public DataBinaryWriter(Stream stream) : base(stream) { }

        public void WriteGrowing(long value)
        {
            if (value >= 0 && value <= 268435454)
            {
                ULeb128.WriteULeb128(this, ((ulong)value));
            }
            else
            {
                ULeb128.WriteULeb128(this, 268435455);
                Leb128.WriteLeb128(this, value);
            }
        }

        public void WriteLeb128(long value) { Leb128.WriteLeb128(this, value); }
    }
}

