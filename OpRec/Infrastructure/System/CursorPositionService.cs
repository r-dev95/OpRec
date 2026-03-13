using OpRec.Application.System.Ports;
using OpRec.Infrastructure.Input;

namespace OpRec.Infrastructure.System
{
    public sealed class CursorPositionService : ICursorPositionService
    {
        public bool TryGetPosition(out int x, out int y)
        {
            if (InputHelper.GetCursorPos(out var point))
            {
                x = point.x;
                y = point.y;
                return true;
            }

            x = 0;
            y = 0;
            return false;
        }
    }
}
