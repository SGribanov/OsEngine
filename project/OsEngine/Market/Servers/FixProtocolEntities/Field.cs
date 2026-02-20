#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8767

namespace OsEngine.Market.Servers.FixProtocolEntities
{
    public class Field
    {
        public int Tag;
        public string Value;

        public Field(int tag, string value)
        {
            Tag = tag;
            Value = value;
        }
    }
}

