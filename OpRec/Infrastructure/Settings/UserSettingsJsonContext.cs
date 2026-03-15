using System.Text.Json.Serialization;

using OpRec.Domain.Settings.ValueObjects;

namespace OpRec.Infrastructure.Settings
{
    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(UserSettings))]
    internal partial class UserSettingsJsonContext : JsonSerializerContext
    {
    }
}
