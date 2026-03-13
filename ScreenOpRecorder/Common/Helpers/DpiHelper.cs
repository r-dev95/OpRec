using Windows.Foundation;

namespace ScreenOpRecorder.Common.Helpers
{
    internal static class DpiHelper
    {
        public static double ToPhysical(double logical, double scaleFactor)
        {
            return logical * scaleFactor;
        }

        public static double ToLogical(double physical, double scaleFactor)
        {
            return physical / scaleFactor;
        }

        public static int ToPhysicalInt(double logical, double scaleFactor)
        {
            return (int)ToPhysical(logical, scaleFactor);
        }

        public static Point ToPhysical(Point logicalPoint, double scaleFactor)
        {
            return new(ToPhysical(logicalPoint.X, scaleFactor), ToPhysical(logicalPoint.Y, scaleFactor));
        }

        public static Point ToLogical(Point physicalPoint, double scaleFactor)
        {
            return new(ToLogical(physicalPoint.X, scaleFactor), ToLogical(physicalPoint.Y, scaleFactor));
        }

        public static Size ToPhysical(Size logicalSize, double scaleFactor)
        {
            return new(ToPhysical(logicalSize.Width, scaleFactor), ToPhysical(logicalSize.Height, scaleFactor));
        }

        public static Size ToLogical(Size physicalSize, double scaleFactor)
        {
            return new(ToLogical(physicalSize.Width, scaleFactor), ToLogical(physicalSize.Height, scaleFactor));
        }

        public static Rect ToPhysical(Rect logicalRect, double scaleFactor)
        {
            var point = ToPhysical(new Point(logicalRect.X, logicalRect.Y), scaleFactor);
            var size = ToPhysical(new Size(logicalRect.Width, logicalRect.Height), scaleFactor);
            return new Rect(point, size);
        }

        public static Rect ToLogical(Rect physicalRect, double scaleFactor)
        {
            var point = ToLogical(new Point(physicalRect.X, physicalRect.Y), scaleFactor);
            var size = ToLogical(new Size(physicalRect.Width, physicalRect.Height), scaleFactor);
            return new Rect(point, size);
        }
    }
}
