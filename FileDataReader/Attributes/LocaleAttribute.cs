namespace FileDataReader;

public class LocaleAttribute : Attribute
{
    public readonly int Locale;

    public readonly int LocaleCount;

    public LocaleAttribute(int locale, int localecount = 16)
    {
        Locale = locale;
        LocaleCount = localecount;
    }
}