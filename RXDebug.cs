using System.Text;

public static class RXDebug
{
	public static void Log(params object[] objects)
	{
		if (objects.Length == 1)
		{
			objects[0].ToString();
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		int num = objects.Length;
		for (int i = 0; i < num; i++)
		{
			stringBuilder.Append(objects[i].ToString());
			if (i < num - 1)
			{
				stringBuilder.Append(',');
			}
		}
		stringBuilder.ToString();
	}
}
