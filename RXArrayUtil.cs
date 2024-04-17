public static class RXArrayUtil
{
	public static T[] CreateArrayFilledWithItem<T>(T item, int count)
	{
		T[] array = new T[count];
		for (int i = 0; i < count; i++)
		{
			array[i] = item;
		}
		return array;
	}
}
