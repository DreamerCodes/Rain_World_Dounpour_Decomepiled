public class FTextParams
{
	public float scaledLineHeightOffset;

	public float scaledKerningOffset;

	private float _lineHeightOffset;

	private float _kerningOffset;

	public float kerningOffset
	{
		get
		{
			return _kerningOffset;
		}
		set
		{
			_kerningOffset = value;
			scaledKerningOffset = value;
		}
	}

	public float lineHeightOffset
	{
		get
		{
			return _lineHeightOffset;
		}
		set
		{
			_lineHeightOffset = value;
			scaledLineHeightOffset = value;
		}
	}
}
