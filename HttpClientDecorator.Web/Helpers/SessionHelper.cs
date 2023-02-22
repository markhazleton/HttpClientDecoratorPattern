using System.Text.Json;
namespace HttpClientDecorator.Web.Helpers;

/// <summary>
/// Provides extension methods for the <see cref="ISession"/> interface to store and retrieve complex objects as JSON strings in the user session.
/// </summary>
public static class SessionHelper
{
    /// <summary>
    /// Serializes an object of type T to JSON and stores it in the session with the specified key.
    /// </summary>
    /// <typeparam name="T">The type of the object to be stored in the session. Must be a class.</typeparam>
    /// <param name="session">The session instance.</param>
    /// <param name="key">The key to use for the stored object in the session.</param>
    /// <param name="value">The object to be stored in the session.</param>
    public static void SetObjectAsJson<T>(this ISession session, string key, T value) where T : class
    {
        if (session == null)
        {
            throw new ArgumentNullException(nameof(session));
        }

        if (key == null)
        {
            throw new ArgumentNullException(nameof(key));
        }

        string json = JsonSerializer.Serialize(value);
        session.SetString(key, json);
    }

    /// <summary>
    /// Retrieves an object of type T from the session with the specified key, deserializing it from a JSON string.
    /// </summary>
    /// <typeparam name="T">The type of the object to be retrieved from the session. Must be a class.</typeparam>
    /// <param name="session">The session instance.</param>
    /// <param name="key">The key of the stored object in the session.</param>
    /// <returns>The object retrieved from the session, or null if the object was not found or could not be deserialized.</returns>
    public static T GetObjectFromJson<T>(this ISession session, string key) where T : class
    {
        if (session == null)
        {
            throw new ArgumentNullException(nameof(session));
        }

        if (key == null)
        {
            throw new ArgumentNullException(nameof(key));
        }

        string json = session.GetString(key);
        if (json == null)
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(json);
    }
}
