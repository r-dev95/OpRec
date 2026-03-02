namespace ScreenOpRecorder.Domain.ValueObjects
{
    public readonly record struct ScreenRect(double X, double Y, double Width, double Height)
    {
        public static readonly ScreenRect Empty = new(0, 0, 0, 0);

        public bool HasArea => Width > 0 && Height > 0;
    }
}
