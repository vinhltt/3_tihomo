namespace Shared.Contracts.EntityFrameworkUtilities;

public static class TypeList
{
    /// <summary>
    /// The type of character. (EN)<br/>
    /// Kiểu dữ liệu ký tự. (VI)
    /// </summary>
    public static readonly Type TypeOfChar = typeof(char);

    /// <summary>
    /// The type of string. (EN)<br/>
    /// Kiểu dữ liệu chuỗi. (VI)
    /// </summary>
    public static readonly Type TypeOfString = typeof(string);

    /// <summary>
    /// The type of boolean. (EN)<br/>
    /// Kiểu dữ liệu boolean. (VI)
    /// </summary>
    public static readonly Type TypeOfBoolean = typeof(bool);

    /// <summary>
    /// The type of byte. (EN)<br/>
    /// Kiểu dữ liệu byte. (VI)
    /// </summary>
    public static readonly Type TypeOfByte = typeof(byte);

    /// <summary>
    /// The type of short. (EN)<br/>
    /// Kiểu dữ liệu short. (VI)
    /// </summary>
    public static readonly Type TypeOfShort = typeof(short);

    /// <summary>
    /// The type of unsigned short. (EN)<br/>
    /// Kiểu dữ liệu unsigned short. (VI)
    /// </summary>
    public static readonly Type TypeOfUnsignedShort = typeof(short);

    /// <summary>
    /// The type of int. (EN)<br/>
    /// Kiểu dữ liệu int. (VI)
    /// </summary>
    public static readonly Type TypeOfInt = typeof(int);

    /// <summary>
    /// The type of unsigned int. (EN)<br/>
    /// Kiểu dữ liệu unsigned int. (VI)
    /// </summary>
    public static readonly Type TypeOfUnsignedInt = typeof(uint);

    /// <summary>
    /// The type of long. (EN)<br/>
    /// Kiểu dữ liệu long. (VI)
    /// </summary>
    public static readonly Type TypeOfLong = typeof(long);

    /// <summary>
    /// The type of unsigned long. (EN)<br/>
    /// Kiểu dữ liệu unsigned long. (VI)
    /// </summary>
    public static readonly Type TypeOfUnsignedLong = typeof(ulong);

    /// <summary>
    /// The type of float. (EN)<br/>
    /// Kiểu dữ liệu float. (VI)
    /// </summary>
    public static readonly Type TypeOfFloat = typeof(float);

    /// <summary>
    /// The type of double. (EN)<br/>
    /// Kiểu dữ liệu double. (VI)
    /// </summary>
    public static readonly Type TypeOfDouble = typeof(double);

    /// <summary>
    /// The type of decimal. (EN)<br/>
    /// Kiểu dữ liệu decimal. (VI)
    /// </summary>
    public static readonly Type TypeOfDecimal = typeof(decimal);

    /// <summary>
    /// The type of time span. (EN)<br/>
    /// Kiểu dữ liệu time span. (VI)
    /// </summary>
    public static readonly Type TypeOfTimeSpan = typeof(TimeSpan);

    /// <summary>
    /// The type of date time. (EN)<br/>
    /// Kiểu dữ liệu date time. (VI)
    /// </summary>
    public static readonly Type TypeOfDateTime = typeof(DateTime);

    /// <summary>
    /// The type of date time offset. (EN)<br/>
    /// Kiểu dữ liệu date time offset. (VI)
    /// </summary>
    public static readonly Type TypeOfDateTimeOffset = typeof(DateTimeOffset);

    /// <summary>
    /// The type of unique identifier. (EN)<br/>
    /// Kiểu dữ liệu unique identifier. (VI)
    /// </summary>
    public static readonly Type TypeOfGuid = typeof(Guid);

    /// <summary>
    /// The simple types. (EN)<br/>
    /// Các kiểu dữ liệu đơn giản. (VI)
    /// </summary>
    public static readonly Type[] TypeSimpleTypes =
    [
        TypeOfChar,
        TypeOfString,
        TypeOfBoolean,
        TypeOfByte,
        TypeOfShort,
        TypeOfUnsignedShort,
        TypeOfInt,
        TypeOfUnsignedInt,
        TypeOfLong,
        TypeOfUnsignedLong,
        TypeOfDecimal,
        TypeOfDouble,
        TypeOfFloat,
        TypeOfTimeSpan,
        TypeOfDateTime,
        TypeOfDateTimeOffset,
        TypeOfGuid
    ];

    /// <summary>
    /// Determines whether a given type is a simple type, including nullable variations. (EN)<br/>
    /// Xác định xem một kiểu dữ liệu nhất định có phải là kiểu dữ liệu đơn giản hay không, bao gồm cả các biến thể nullable. (VI)
    /// </summary>
    /// <param name="type">The type to check. (EN)<br/>Kiểu dữ liệu cần kiểm tra. (VI)</param>
    /// <returns>
    /// <c>true</c> if the specified type is a simple type; otherwise, <c>false</c>. (EN)<br/>
    /// <c>true</c> nếu kiểu được chỉ định là kiểu đơn giản; ngược lại, <c>false</c>. (VI)
    /// </returns>
    public static bool IsSimpleType(Type type)
    {
        if (TypeSimpleTypes.Any(t => t == type))
        {
            return true;
        }

        var underlyingType = Nullable.GetUnderlyingType(type);
        return underlyingType != null && TypeSimpleTypes.Any(t => t == type);
    }
}