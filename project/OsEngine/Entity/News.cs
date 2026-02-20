/*
 * Your rights to use code governed by this license http://o-s-a.net/doc/license_simple_engine.pdf
 * Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System;

#nullable enable

namespace OsEngine.Entity
{
    public class News
    {
        public DateTime TimeMessage { get; set; }

        public string Source { get; set; } = string.Empty;

        public string Value { get; set; } = string.Empty;

    }
}
