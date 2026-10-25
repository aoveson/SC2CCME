using System.Xml;
using System.Xml.Serialization;
using ModManager.StarCraft.Services.Settings.Interface;

namespace ModManager.StarCraft.Services.Settings;

/// <summary>
/// Persists settings as XML at the application's configured settings path.
/// </summary>
public sealed class XmlSettingsStore : ISettingsStore
{
    /// <inheritdoc />
    public ApplicationSettings Read()
    {
        ApplicationSettings retVal = new();

        if(!File.Exists(ApplicationSettings.SettingsFilePath))
        {
            Write(retVal);
            return retVal;
        }

        XmlSerializer serializer = new(typeof(ApplicationSettings));
        using FileStream stream = File.OpenRead(ApplicationSettings.SettingsFilePath);
        using XmlReader reader = XmlReader.Create(stream);

        if(serializer.CanDeserialize(reader) && serializer.Deserialize(reader) is ApplicationSettings settings)
        {
            return settings;
        }

        Write(retVal);
        return retVal;
    }

    /// <inheritdoc />
    public void Write(ApplicationSettings settings)
    {
        XmlSerializer serializer = new(typeof(ApplicationSettings));
        using FileStream stream = File.Create(ApplicationSettings.SettingsFilePath);
        using XmlWriter writer = XmlWriter.Create(stream);
        serializer.Serialize(writer, settings);
    }
}