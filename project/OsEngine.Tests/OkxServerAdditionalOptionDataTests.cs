#nullable enable

using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.CompilerServices;
using OsEngine.Market.Servers.OKX;

namespace OsEngine.Tests;

public sealed class OkxServerAdditionalOptionDataTests
{
    [Fact]
    public void GetOrAddAdditionalOptionData_WithMissingKey_ShouldCreateAndStoreValue()
    {
        OkxServerRealization realization = CreateRealization();
        object dictionary = CreateAdditionalOptionDataDictionary();
        SetPrivateField(realization, "_additionalOptionData", dictionary);

        object value = InvokeGetOrAddAdditionalOptionData(realization, "BTC-USD-240329-50000-C");

        Assert.Same(value, GetDictionaryValue(dictionary, "BTC-USD-240329-50000-C"));
    }

    [Fact]
    public void GetOrAddAdditionalOptionData_WithExistingKey_ShouldReuseStoredValue()
    {
        OkxServerRealization realization = CreateRealization();
        object dictionary = CreateAdditionalOptionDataDictionary();
        object existing = CreateAdditionalOptionDataInstance();
        SetDictionaryValue(dictionary, "BTC-USD-240329-50000-C", existing);
        SetPrivateField(realization, "_additionalOptionData", dictionary);

        object value = InvokeGetOrAddAdditionalOptionData(realization, "BTC-USD-240329-50000-C");

        Assert.Same(existing, value);
    }

    private static OkxServerRealization CreateRealization()
    {
        return (OkxServerRealization)RuntimeHelpers.GetUninitializedObject(typeof(OkxServerRealization));
    }

    private static object InvokeGetOrAddAdditionalOptionData(OkxServerRealization realization, string instId)
    {
        MethodInfo method = typeof(OkxServerRealization).GetMethod(
            "GetOrAddAdditionalOptionData",
            BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException("GetOrAddAdditionalOptionData method not found.");

        return method.Invoke(realization, [instId])
            ?? throw new InvalidOperationException("GetOrAddAdditionalOptionData returned null.");
    }

    private static object CreateAdditionalOptionDataDictionary()
    {
        Type valueType = GetAdditionalOptionDataType();
        Type dictionaryType = typeof(ConcurrentDictionary<,>).MakeGenericType(typeof(string), valueType);
        return Activator.CreateInstance(dictionaryType)!;
    }

    private static object CreateAdditionalOptionDataInstance()
    {
        return Activator.CreateInstance(GetAdditionalOptionDataType())!;
    }

    private static Type GetAdditionalOptionDataType()
    {
        return typeof(OkxServerRealization).GetNestedType("AdditionalOptionData", BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("AdditionalOptionData type not found.");
    }

    private static object GetDictionaryValue(object dictionary, string key)
    {
        PropertyInfo indexer = dictionary.GetType().GetProperty("Item")
            ?? throw new InvalidOperationException("Dictionary indexer not found.");

        return indexer.GetValue(dictionary, [key])
            ?? throw new InvalidOperationException("Dictionary value was null.");
    }

    private static void SetDictionaryValue(object dictionary, string key, object value)
    {
        PropertyInfo indexer = dictionary.GetType().GetProperty("Item")
            ?? throw new InvalidOperationException("Dictionary indexer not found.");

        indexer.SetValue(dictionary, value, [key]);
    }

    private static void SetPrivateField(OkxServerRealization realization, string fieldName, object value)
    {
        FieldInfo field = typeof(OkxServerRealization).GetField(
            fieldName,
            BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException($"{fieldName} field not found.");

        field.SetValue(realization, value);
    }
}
