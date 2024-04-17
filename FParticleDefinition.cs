using UnityEngine;

public class FParticleDefinition
{
	public FAtlasElement element;

	public float lifetime = 1f;

	public float x;

	public float y;

	public float speedX;

	public float speedY;

	public float startScale = 1f;

	public float endScale = 1f;

	public Color startColor = Futile.white;

	public Color endColor = Futile.white;

	public float startRotation;

	public float endRotation;

	public FParticleDefinition(string elementName)
	{
		element = Futile.atlasManager.GetElementWithName(elementName);
	}

	public FParticleDefinition(FAtlasElement element)
	{
		this.element = element;
	}

	public void SetElementByName(string elementName)
	{
		element = Futile.atlasManager.GetElementWithName(elementName);
	}
}
