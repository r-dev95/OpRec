using Windows.Foundation;

namespace ScreenOpRecorder.Shared.Messages;

public record SelectionCompletedMessage(Rect captureRect);
