using UnityEngine;

public abstract class AmbientSound
{
	public class Type : ExtEnum<Type>
	{
		public static readonly Type Omnidirectional = new Type("Omnidirectional", register: true);

		public static readonly Type Directional = new Type("Directional", register: true);

		public static readonly Type Spot = new Type("Spot", register: true);

		public Type(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public string sample;

	public float volume = 0.5f;

	public float pitch = 1f;

	public bool inherited;

	public bool overWrite;

	public Vector2 panelPosition;

	public string[] unrecognizedAttributes;

	public Type type;

	public AmbientSound(string sample, bool inherited)
	{
		this.sample = sample;
		this.inherited = inherited;
	}

	public override string ToString()
	{
		return "";
	}

	public virtual void FromString(string[] s)
	{
	}
}
