//using Newtonsoft.Json;
//using System.Text;

//namespace PlainlyIpc.Converter;

///// <summary>
///// Newtonsoft.Json based IObjectConverter implentation.
///// </summary>
//public sealed class NewtonsoftJsonObjectConverter : IObjectConverter
//{
//    /// <inheritdoc/>
//    public byte[] Serialize<T>(T? data)
//    {
//        return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data, Formatting.Indented));
//    }

//    /// <inheritdoc/>
//    public T? Deserialize<T>(byte[] data)
//    {
//        if (data is null) { throw new ArgumentNullException(nameof(data)); }
//        return JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(data));
//    }

//    /// <inheritdoc/>
//    public object? Deserialize(byte[] data, Type type)
//    {
//        if (data is null) { throw new ArgumentNullException(nameof(data)); }
//        if (type is null) { throw new ArgumentNullException(nameof(type)); }
//        return JsonConvert.DeserializeObject(Encoding.UTF8.GetString(data), type);
//    }

//}
