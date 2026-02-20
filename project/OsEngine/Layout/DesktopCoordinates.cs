#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

/*
 * Your rights to use code governed by this license https://github.com/AlexWan/OsEngine/blob/master/LICENSE
 * Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System.Windows;

namespace OsEngine.Layout
{
    public class DesktopCoordinates
    {

        public static double YmousePos(System.Windows.Window ui)
        {
            var point = System.Windows.Forms.Control.MousePosition;
            Point newPoint = new Point(point.X, point.Y);

            var transform = PresentationSource.FromVisual(ui).CompositionTarget.TransformFromDevice;
            var mouse = transform.Transform(newPoint);

            return mouse.Y;
        }

        public static double XmousePos(System.Windows.Window ui)
        {
            var point = System.Windows.Forms.Control.MousePosition;
            Point newPoint = new Point(point.X, point.Y);

            var transform = PresentationSource.FromVisual(ui).CompositionTarget.TransformFromDevice;
            var mouse = transform.Transform(newPoint);

            return mouse.X;
        }

        public static double YmousePosOldVersion()
        {
            var point = System.Windows.Forms.Control.MousePosition;
            return point.Y;
        }

        public static double XmousePosOldVersion()
        {
            var point = System.Windows.Forms.Control.MousePosition;
            return point.X;
        }

    }
    public struct POINT
    {
        public int X;
        public int Y;
    }
}

