#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

/*
 * Your rights to use code governed by this license http://o-s-a.net/doc/license_simple_engine.pdf
 * Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using OsEngine.Language;
using OsEngine.Market;
using System.Diagnostics;
using System;
using static OsEngine.Language.OsLocalization;

namespace OsEngine.Instructions
{
    public class Instruction
    {
        public InstructionLocalized Ru;

        public InstructionLocalized Eng;

        public InstructionType Type;

        public string Description
        {
            get
            {
                OsLocalType currentLanguage =  OsLocalization.CurLocalization;

                if(currentLanguage == OsLocalType.Ru
                    && Ru != null)
                {
                    return Ru.Description;
                }
                else if (currentLanguage == OsLocalType.Eng
                && Eng != null)
                {
                    return Eng.Description;
                }

                return "";
            }
        }

        public string PostLink
        {
            get
            {
                OsLocalType currentLanguage = OsLocalization.CurLocalization;

                if (currentLanguage == OsLocalType.Ru
                    && Ru != null)
                {
                    return Ru.PostLink;
                }
                else if (currentLanguage == OsLocalType.Eng
                && Eng != null)
                {
                    return Eng.PostLink;
                }

                return "";
            }
        }

        public void ShowLinkInBrowser()
        {
            try
            {
                string link = PostLink;

                if (string.IsNullOrEmpty(link))
                {
                    return;
                }

                Process.Start(new ProcessStartInfo(link) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                ServerMaster.SendNewLogMessage(ex.ToString(), OsEngine.Logging.LogMessageType.Error);
            }
        }
    }

    public enum InstructionType
    {
        Post,
        Video
    }

    public class InstructionLocalized
    {
        public string Description;

        public string PostLink;
    }
}

