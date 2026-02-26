#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8767

using System;
using System.Collections.Generic;

namespace OsEngine.Market.Servers.FixProtocolEntities
{
    public class FixEntity
    {
        public List<Field> Fields;

        public FixEntity()
        {
            Fields = new List<Field>();
        }

        public string EntityType
        {
            get { return Fields[2].Value; }
        }


        public void AddField(Field field)
        {
            Fields.Add(field);
        }

        public string GetFieldByTag(int tag)
        {
            string value;

            try
            {
                value = Fields.Find(field => field.Tag == tag).Value;
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Запрошено отсутствующее поле", ex);
            }

            return value;
        }

    }
}

