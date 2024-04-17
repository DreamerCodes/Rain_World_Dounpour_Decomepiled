using System.Collections.Generic;
using UnityEngine;

public class AtlasAnimator
{
	private Vector2 pos;

	private int startSprite;

	public string atlas;

	private string framePrefix;

	private int maxIndex;

	private bool loop;

	private bool reverse;

	public float frame;

	private bool menuContext;

	public float animSpeed;

	public FSprite sprite;

	public Dictionary<int, float> specificSpeeds;

	public AtlasAnimator(int startSprite, Vector2 pos, string atlas, string framePrefix, int maxIndex, bool loop, bool reverse)
	{
		this.pos = pos;
		this.atlas = atlas;
		this.framePrefix = framePrefix;
		this.maxIndex = maxIndex;
		this.loop = loop;
		this.reverse = reverse;
		this.startSprite = startSprite;
		menuContext = false;
		if (reverse)
		{
			frame = maxIndex;
			return;
		}
		frame = 1f;
		animSpeed = 1f;
	}

	public void Update()
	{
		if (reverse)
		{
			if (specificSpeeds != null && specificSpeeds.ContainsKey((int)frame))
			{
				frame -= specificSpeeds[(int)frame];
			}
			else
			{
				frame -= animSpeed;
			}
			if (frame < 1f)
			{
				if (loop)
				{
					frame = maxIndex;
					return;
				}
				frame = 1f;
			}
		}
		else
		{
			if (specificSpeeds != null && specificSpeeds.ContainsKey((int)frame))
			{
				frame += specificSpeeds[(int)frame];
			}
			else
			{
				frame += animSpeed;
			}
			if (frame > (float)maxIndex)
			{
				if (loop)
				{
					frame = 1f;
					return;
				}
				frame = maxIndex;
			}
		}
		if (menuContext)
		{
			sprite.element = Futile.atlasManager.GetElementWithName(framePrefix + "_" + ((int)frame).ToString("000"));
		}
	}

	public void AddToContainer(FContainer container)
	{
		menuContext = true;
		Futile.atlasManager.LoadAtlas("Atlases/" + atlas);
		sprite = new FSprite(framePrefix + "_" + ((int)frame).ToString("000"));
		sprite.x = pos.x;
		sprite.y = pos.y;
		container.AddChild(sprite);
	}

	public void RemoveFromContainer()
	{
		sprite.RemoveFromContainer();
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		menuContext = false;
		Futile.atlasManager.LoadAtlas("Atlases/" + atlas);
		sprite = new FSprite(framePrefix + "_" + ((int)frame).ToString("000"));
		sLeaser.sprites[startSprite] = sprite;
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		sLeaser.sprites[startSprite].x = pos.x;
		sLeaser.sprites[startSprite].y = pos.y;
		sLeaser.sprites[startSprite].element = Futile.atlasManager.GetElementWithName(framePrefix + "_" + ((int)frame).ToString("000"));
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		newContatiner.AddChild(sLeaser.sprites[startSprite]);
	}
}
