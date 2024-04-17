namespace RWCustom;

public static class RWCustomExtensions
{
	public static int IndexfOf(this IntVector2[] arr, IntVector2 item)
	{
		for (int i = 0; i < arr.Length; i++)
		{
			if (item == arr[i])
			{
				return i;
			}
		}
		return -1;
	}

	public static int IndexfOf(this int[] arr, int item)
	{
		for (int i = 0; i < arr.Length; i++)
		{
			if (item == arr[i])
			{
				return i;
			}
		}
		return -1;
	}
}
