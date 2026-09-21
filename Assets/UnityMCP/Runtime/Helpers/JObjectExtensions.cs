namespace Newtonsoft.Json.Linq
{
    /// <summary>
    /// Extension method polyfill for Newtonsoft.Json versions prior to JObject having native ContainsKey.
    /// In older Json.NET versions, JObject lacks a public ContainsKey(string) method, causing compilation
    /// or runtime MissingMethodException. This polyfill ensures universal compatibility by delegating to Property(prop) != null.
    /// </summary>
    public static class JObjectExtensions
    {
        public static bool ContainsKey(this JObject obj, string propertyName)
        {
            if (obj == null || string.IsNullOrEmpty(propertyName))
                return false;
            return obj.Property(propertyName) != null;
        }
    }
}
