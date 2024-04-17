using System;
using UnityEngine;

public class FParticleSystem : FFacetNode
{
	private int _maxParticleCount;

	private FParticle[] _particles;

	private FParticle[] _availableParticles;

	private int _availableParticleCount;

	private int _unavailableParticleIndex;

	private bool _isMeshDirty;

	public float accelX;

	public float accelY;

	private bool _hasInited;

	public bool shouldNewParticlesOverwriteExistingParticles = true;

	public FParticleSystem(int maxParticleCount)
	{
		_maxParticleCount = maxParticleCount;
		_particles = new FParticle[_maxParticleCount];
		_availableParticles = new FParticle[_maxParticleCount];
		_availableParticleCount = _maxParticleCount;
		_unavailableParticleIndex = _maxParticleCount - 1;
		for (int i = 0; i < _maxParticleCount; i++)
		{
			_particles[i] = (_availableParticles[i] = new FParticle());
		}
		ListenForUpdate(HandleUpdate);
	}

	public void AddParticle(FParticleDefinition particleDefinition)
	{
		FAtlasElement element = particleDefinition.element;
		if (_hasInited)
		{
			if (element.atlas != _atlas)
			{
				throw new FutileException("All elements added to a particle system must be from the same atlas");
			}
		}
		else
		{
			_hasInited = true;
			Init(FFacetType.Quad, element.atlas, _maxParticleCount);
			if (base.stage != null)
			{
				base.stage.HandleFacetsChanged();
			}
		}
		FParticle fParticle;
		if (_availableParticleCount == 0)
		{
			if (!shouldNewParticlesOverwriteExistingParticles)
			{
				return;
			}
			fParticle = _availableParticles[_unavailableParticleIndex--];
			if (_unavailableParticleIndex < 0)
			{
				_unavailableParticleIndex = _maxParticleCount - 1;
			}
		}
		else
		{
			_availableParticleCount--;
			fParticle = _availableParticles[_availableParticleCount];
		}
		float num = (fParticle.timeRemaining = particleDefinition.lifetime);
		fParticle.x = particleDefinition.x;
		fParticle.y = particleDefinition.y;
		fParticle.speedX = particleDefinition.speedX;
		fParticle.speedY = particleDefinition.speedY;
		fParticle.scale = particleDefinition.startScale;
		float num2 = 1f / num;
		fParticle.scaleDeltaPerSecond = (particleDefinition.endScale - particleDefinition.startScale) * num2;
		Color startColor = particleDefinition.startColor;
		Color endColor = particleDefinition.endColor;
		fParticle.color = startColor;
		fParticle.redDeltaPerSecond = (endColor.r - startColor.r) * num2;
		fParticle.greenDeltaPerSecond = (endColor.g - startColor.g) * num2;
		fParticle.blueDeltaPerSecond = (endColor.b - startColor.b) * num2;
		fParticle.alphaDeltaPerSecond = (endColor.a - startColor.a) * num2;
		fParticle.elementHalfWidth = element.sourceSize.x * 0.5f;
		fParticle.elementHalfHeight = element.sourceSize.y * 0.5f;
		fParticle.uvTopLeft = element.uvTopLeft;
		fParticle.uvTopRight = element.uvTopRight;
		fParticle.uvBottomRight = element.uvBottomRight;
		fParticle.uvBottomLeft = element.uvBottomLeft;
		fParticle.initialTopLeft = new Vector2(0f - fParticle.elementHalfWidth, fParticle.elementHalfHeight);
		fParticle.initialTopRight = new Vector2(fParticle.elementHalfWidth, fParticle.elementHalfHeight);
		fParticle.initialBottomRight = new Vector2(fParticle.elementHalfWidth, 0f - fParticle.elementHalfHeight);
		fParticle.initialBottomLeft = new Vector2(0f - fParticle.elementHalfWidth, 0f - fParticle.elementHalfHeight);
		fParticle.rotation = particleDefinition.startRotation * ((float)Math.PI / 180f) * -1f;
		fParticle.rotationDeltaPerSecond = (particleDefinition.endRotation - particleDefinition.startRotation) * num2 * ((float)Math.PI / 180f) * -1f;
		if (fParticle.rotationDeltaPerSecond == 0.0)
		{
			fParticle.doesNeedRotationUpdates = false;
			if (fParticle.rotation == 0.0)
			{
				fParticle.resultTopLeftX = fParticle.initialTopLeft.x;
				fParticle.resultTopLeftY = fParticle.initialTopLeft.y;
				fParticle.resultTopRightX = fParticle.initialTopRight.x;
				fParticle.resultTopRightY = fParticle.initialTopRight.y;
				fParticle.resultBottomRightX = fParticle.initialBottomRight.x;
				fParticle.resultBottomRightY = fParticle.initialBottomRight.y;
				fParticle.resultBottomLeftX = fParticle.initialBottomLeft.x;
				fParticle.resultBottomLeftY = fParticle.initialBottomLeft.y;
				return;
			}
			float num3 = (float)Math.Sin(fParticle.rotation);
			float num4 = (float)Math.Cos(fParticle.rotation);
			float num5 = fParticle.initialTopLeft.x;
			float num6 = fParticle.initialTopLeft.y;
			fParticle.resultTopLeftX = num5 * num4 - num6 * num3;
			fParticle.resultTopLeftY = num5 * num3 + num6 * num4;
			num5 = fParticle.initialTopRight.x;
			num6 = fParticle.initialTopRight.y;
			fParticle.resultTopRightX = num5 * num4 - num6 * num3;
			fParticle.resultTopRightY = num5 * num3 + num6 * num4;
			num5 = fParticle.initialBottomRight.x;
			num6 = fParticle.initialBottomRight.y;
			fParticle.resultBottomRightX = num5 * num4 - num6 * num3;
			fParticle.resultBottomRightY = num5 * num3 + num6 * num4;
			num5 = fParticle.initialBottomLeft.x;
			num6 = fParticle.initialBottomLeft.y;
			fParticle.resultBottomLeftX = num5 * num4 - num6 * num3;
			fParticle.resultBottomLeftY = num5 * num3 + num6 * num4;
		}
		else
		{
			fParticle.doesNeedRotationUpdates = true;
		}
	}

	private void HandleUpdate()
	{
		float deltaTime = Time.deltaTime;
		for (int i = 0; i < _maxParticleCount; i++)
		{
			FParticle fParticle = _particles[i];
			if (fParticle.timeRemaining <= 0f)
			{
				continue;
			}
			if (fParticle.timeRemaining <= deltaTime)
			{
				_availableParticles[_availableParticleCount] = fParticle;
				_availableParticleCount++;
				fParticle.timeRemaining = 0f;
				continue;
			}
			fParticle.timeRemaining -= deltaTime;
			fParticle.color.r += fParticle.redDeltaPerSecond * deltaTime;
			fParticle.color.g += fParticle.greenDeltaPerSecond * deltaTime;
			fParticle.color.b += fParticle.blueDeltaPerSecond * deltaTime;
			fParticle.color.a += fParticle.alphaDeltaPerSecond * deltaTime;
			fParticle.scale += fParticle.scaleDeltaPerSecond * deltaTime;
			fParticle.speedX += accelX * deltaTime;
			fParticle.speedY += accelY * deltaTime;
			fParticle.x += fParticle.speedX * deltaTime;
			fParticle.y += fParticle.speedY * deltaTime;
			if (fParticle.doesNeedRotationUpdates)
			{
				fParticle.rotation += fParticle.rotationDeltaPerSecond * (double)deltaTime;
				float num = (float)Math.Sin(fParticle.rotation);
				float num2 = (float)Math.Cos(fParticle.rotation);
				float num3 = fParticle.initialTopLeft.x;
				float num4 = fParticle.initialTopLeft.y;
				fParticle.resultTopLeftX = num3 * num2 - num4 * num;
				fParticle.resultTopLeftY = num3 * num + num4 * num2;
				num3 = fParticle.initialTopRight.x;
				num4 = fParticle.initialTopRight.y;
				fParticle.resultTopRightX = num3 * num2 - num4 * num;
				fParticle.resultTopRightY = num3 * num + num4 * num2;
				num3 = fParticle.initialBottomRight.x;
				num4 = fParticle.initialBottomRight.y;
				fParticle.resultBottomRightX = num3 * num2 - num4 * num;
				fParticle.resultBottomRightY = num3 * num + num4 * num2;
				num3 = fParticle.initialBottomLeft.x;
				num4 = fParticle.initialBottomLeft.y;
				fParticle.resultBottomLeftX = num3 * num2 - num4 * num;
				fParticle.resultBottomLeftY = num3 * num + num4 * num2;
			}
		}
		_isMeshDirty = true;
	}

	public override void Redraw(bool shouldForceDirty, bool shouldUpdateDepth)
	{
		bool num = _isMatrixDirty;
		UpdateDepthMatrixAlpha(shouldForceDirty, shouldUpdateDepth);
		if (shouldUpdateDepth)
		{
			UpdateFacets();
		}
		if (num || shouldForceDirty || shouldUpdateDepth)
		{
			_isMeshDirty = true;
		}
		if (_isMeshDirty)
		{
			PopulateRenderLayer();
		}
	}

	public override void PopulateRenderLayer()
	{
		if (!_isOnStage || _firstFacetIndex == -1)
		{
			return;
		}
		_isMeshDirty = false;
		Vector3[] vertices = _renderLayer.vertices;
		Vector2[] uvs = _renderLayer.uvs;
		Color[] colors = _renderLayer.colors;
		float a = _concatenatedMatrix.a;
		float b = _concatenatedMatrix.b;
		float c = _concatenatedMatrix.c;
		float d = _concatenatedMatrix.d;
		float tx = _concatenatedMatrix.tx;
		float ty = _concatenatedMatrix.ty;
		int num = _firstFacetIndex * 4;
		int num2 = num + 1;
		int num3 = num + 2;
		int num4 = num + 3;
		for (int i = 0; i < _maxParticleCount; i++)
		{
			FParticle fParticle = _particles[i];
			if (fParticle.timeRemaining > 0f)
			{
				float num5 = fParticle.scale;
				float num6 = fParticle.x + fParticle.resultTopLeftX * num5;
				float num7 = fParticle.y + fParticle.resultTopLeftY * num5;
				vertices[num] = new Vector3(num6 * a + num7 * c + tx, num6 * b + num7 * d + ty, _meshZ);
				num6 = fParticle.x + fParticle.resultTopRightX * num5;
				num7 = fParticle.y + fParticle.resultTopRightY * num5;
				vertices[num2] = new Vector3(num6 * a + num7 * c + tx, num6 * b + num7 * d + ty, _meshZ);
				num6 = fParticle.x + fParticle.resultBottomRightX * num5;
				num7 = fParticle.y + fParticle.resultBottomRightY * num5;
				vertices[num3] = new Vector3(num6 * a + num7 * c + tx, num6 * b + num7 * d + ty, _meshZ);
				num6 = fParticle.x + fParticle.resultBottomLeftX * num5;
				num7 = fParticle.y + fParticle.resultBottomLeftY * num5;
				vertices[num4] = new Vector3(num6 * a + num7 * c + tx, num6 * b + num7 * d + ty, _meshZ);
				uvs[num] = fParticle.uvTopLeft;
				uvs[num2] = fParticle.uvTopRight;
				uvs[num3] = fParticle.uvBottomRight;
				uvs[num4] = fParticle.uvBottomLeft;
				colors[num] = fParticle.color;
				colors[num2] = fParticle.color;
				colors[num3] = fParticle.color;
				colors[num4] = fParticle.color;
			}
			else
			{
				vertices[num].Set(50f, 0f, 1000000f);
				vertices[num2].Set(50f, 0f, 1000000f);
				vertices[num3].Set(50f, 0f, 1000000f);
				vertices[num4].Set(50f, 0f, 1000000f);
			}
			num += 4;
			num2 += 4;
			num3 += 4;
			num4 += 4;
		}
		_renderLayer.HandleVertsChange();
	}
}
