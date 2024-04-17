using UnityEngine;

public class FUnityParticleSystemNode : FGameObjectNode
{
	protected ParticleSystem _particleSystem;

	public ParticleSystem particleSystem => _particleSystem;

	public FUnityParticleSystemNode(GameObject gameObject, bool shouldDuplicate)
	{
		if (shouldDuplicate)
		{
			gameObject = Object.Instantiate(gameObject);
		}
		_particleSystem = gameObject.GetComponent<ParticleSystem>();
		if (_particleSystem == null)
		{
			throw new FutileException("The FUnityParticleSystemNode was not passed a gameObject with a ParticleSystem component");
		}
		Init(gameObject, shouldLinkPosition: true, shouldLinkRotation: false, shouldLinkScale: false);
		ListenForUpdate(HandleUpdate);
	}

	protected void HandleUpdate()
	{
		if (!_particleSystem.IsAlive())
		{
			RemoveFromContainer();
		}
	}
}
