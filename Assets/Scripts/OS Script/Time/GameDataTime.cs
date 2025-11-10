using System;

[System.Serializable]
public struct GameDateTime : IComparable<GameDateTime>
{
    public int year, month, day, hour, minute;

    public GameDateTime(int y, int m, int d, int h, int min)
    {
        year = y;
        month = m;
        day = d;
        hour = h;
        minute = min;
    }

    public int CompareTo(GameDateTime other)
    {
        if (year != other.year) return year.CompareTo(other.year);
        if (month != other.month) return month.CompareTo(other.month);
        if (day != other.day) return day.CompareTo(other.day);
        if (hour != other.hour) return hour.CompareTo(other.hour);
        return minute.CompareTo(other.minute);
    }

    public static bool operator <=(GameDateTime a, GameDateTime b) => a.CompareTo(b) <= 0;
    public static bool operator >=(GameDateTime a, GameDateTime b) => a.CompareTo(b) >= 0;
    public static bool operator <(GameDateTime a, GameDateTime b) => a.CompareTo(b) < 0;
    public static bool operator >(GameDateTime a, GameDateTime b) => a.CompareTo(b) > 0;

    public override string ToString()
    {
        return $"{year}/{month}/{day} {hour:00}:{minute:00}";
    }
}
