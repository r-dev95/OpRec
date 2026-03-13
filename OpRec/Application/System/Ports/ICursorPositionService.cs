namespace OpRec.Application.System.Ports
{
    public interface ICursorPositionService
    {
        bool TryGetPosition(out int x, out int y);
    }
}
