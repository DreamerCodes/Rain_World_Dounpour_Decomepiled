using System;

public class StoveUtils
{
	private static DateTime unixtimeStartTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

	public static DateTime UnixTimeStampToUtcDateTime(long javaTimeStamp)
	{
		try
		{
			return unixtimeStartTime.AddMilliseconds(javaTimeStamp);
		}
		catch (Exception)
		{
			return DateTime.Now;
		}
	}

	public static DateTime UnixTimeStampToLocalDateTime(long javaTimeStamp)
	{
		return UnixTimeStampToUtcDateTime(javaTimeStamp).ToLocalTime();
	}

	public static bool ValidateKeySecret(string appkey, string appsecret)
	{
		if (string.IsNullOrEmpty(appkey) || appkey.Contains(" "))
		{
			return false;
		}
		if (string.IsNullOrEmpty(appsecret) || appsecret.Contains(" "))
		{
			return false;
		}
		return true;
	}
}
