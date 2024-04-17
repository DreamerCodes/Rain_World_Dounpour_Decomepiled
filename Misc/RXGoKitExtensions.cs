public static class RXGoKitExtensions
{
	public static TweenConfig floatProp(this TweenConfig config, string propName, float propValue)
	{
		return config.floatProp(propName, propValue);
	}

	public static TweenConfig removeWhenComplete(this TweenConfig config)
	{
		config.onComplete(HandleRemoveWhenDoneTweenComplete);
		return config;
	}

	private static void HandleRemoveWhenDoneTweenComplete(AbstractTween tween)
	{
		((tween as Tween).target as FNode).RemoveFromContainer();
	}
}
