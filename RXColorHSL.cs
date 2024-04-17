public class RXColorHSL
{
	public float h;

	public float s;

	public float l;

	public RXColorHSL(float h, float s, float l)
	{
		this.h = h;
		this.s = s;
		this.l = l;
	}

	public RXColorHSL()
		: this(0f, 0f, 0f)
	{
	}
}
