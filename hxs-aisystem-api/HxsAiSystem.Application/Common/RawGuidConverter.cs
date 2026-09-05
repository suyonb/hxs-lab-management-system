namespace HxsAiSystem.Application.Common;

public static class RawGuidConverter
{
    public static Guid ToGuid(byte[]? value)
    {
        return value is { Length: 16 } ? new Guid(value) : Guid.Empty;
    }

    public static byte[] ToRaw(Guid value)
    {
        return value.ToByteArray();
    }

    public static byte[]? ToNullableRaw(Guid? value)
    {
        return value.HasValue && value.Value != Guid.Empty ? value.Value.ToByteArray() : null;
    }
}
