using RWCustom;
using UnityEngine;

namespace CoralBrain;

public abstract class BrainFloater : CosmeticSprite
{
	public float myFloatSpeed;

	public BrainFloater(IntVector2 initPos)
	{
		pos = initPos.ToVector2() * 20f + new Vector2(Mathf.Lerp(-9f, 9f, Random.value), Mathf.Lerp(-9f, 9f, Random.value));
		lastPos = pos;
		vel = Custom.RNV() * 2f * Random.value;
		myFloatSpeed = Mathf.Lerp(0.1f, 3f, Random.value);
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		vel = Vector2.Lerp(vel, vel.normalized * myFloatSpeed, 0.01f);
		if (room.aimap.getTerrainProximity(pos) >= 2)
		{
			return;
		}
		IntVector2 tilePosition = room.GetTilePosition(pos);
		Vector2 vector = new Vector2(0f, 0f);
		for (int i = 0; i < 4; i++)
		{
			float num = 0f;
			for (int j = 0; j < 4; j++)
			{
				num += (float)room.aimap.getTerrainProximity(tilePosition + Custom.fourDirections[i] + Custom.fourDirections[j]);
			}
			vector += Custom.fourDirections[i].ToVector2() * num;
		}
		vel += vector.normalized * 0.2f;
	}
}
