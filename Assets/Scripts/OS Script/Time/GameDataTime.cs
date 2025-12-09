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
        return $"0{year}/{month}/{day} {hour:00}:{minute:00}";
    }
    public void NormalizeTime()
    {
        // 분 정리
        if (minute >= 60)
        {
            hour += minute / 60;
            minute %= 60;
        }

        // 시간 정리
        if (hour >= 24)
        {
            day += hour / 24;
            hour %= 24;
        }

        // 간단히 31일 기준
        if (day > 31)
        {
            day = 1;
            month++;
        }

        if (month > 12)
        {
            month = 1;
            year++;
        }
    }

}
