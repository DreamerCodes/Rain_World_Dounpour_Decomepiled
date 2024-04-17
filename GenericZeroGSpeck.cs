using RWCustom;
using UnityEngine;

public abstract class GenericZeroGSpeck : CosmeticSprite
{
	public float myFloatSpeed;

	public int savedCamPos;

	public bool reset;

	public GenericZeroGSpeck()
	{
		savedCamPos = -1;
		ResetMe();
	}

	public override void Update(bool eu)
	{
		if (reset)
		{
			if (room.gravity != 0f || !(Random.value < 0.025f))
			{
				return;
			}
			IntVector2 tilePosition = room.GetTilePosition(room.game.cameras[0].pos + new Vector2(Mathf.Lerp(-200f, (ModManager.MMF ? room.game.rainWorld.options.ScreenSize.x : 1024f) + 200f, Random.value), Mathf.Lerp(-200f, 968f, Random.value)));
			if (!room.readyForAI || !room.GetTile(tilePosition).Solid)
			{
				return;
			}
			for (int i = 0; i < 4; i++)
			{
				if (!room.GetTile(tilePosition + Custom.fourDirections[i]).Solid)
				{
					pos = room.MiddleOfTile(tilePosition);
					lastPos = pos;
					ResetMe();
					reset = false;
				}
			}
			return;
		}
		base.Update(eu);
		if (room.gravity == 0f)
		{
			if (room.readyForAI && room.aimap.getTerrainProximity(pos) < 2)
			{
				IntVector2 tilePosition2 = room.GetTilePosition(pos);
				Vector2 vector = new Vector2(0f, 0f);
				for (int j = 0; j < 4; j++)
				{
					float num = 0f;
					for (int k = 0; k < 4; k++)
					{
						num += (float)room.aimap.getTerrainProximity(tilePosition2 + Custom.fourDirections[j] + Custom.fourDirections[k]);
					}
					vector += Custom.fourDirections[j].ToVector2() * num;
				}
				vel += vector.normalized * 0.2f;
				vel = Vector2.Lerp(vel, vector.normalized * myFloatSpeed, 0.02f);
			}
			else
			{
				vel = Vector2.Lerp(vel, vel.normalized * myFloatSpeed, 0.01f);
			}
			vel += Custom.RNV() * Random.value * 0.05f;
		}
		else
		{
			vel.y -= room.gravity * 0.2f;
			vel += Custom.DegToVec(180f + Mathf.Lerp(-45f, 45f, Random.value)) * room.gravity * 0.1f;
			if (room.GetTile(pos).Solid && room.GetTile(lastPos).Solid)
			{
				reset = true;
			}
		}
		if (pos.x < room.game.cameras[0].pos.x - 200f)
		{
			if (room.gravity == 0f)
			{
				pos.x = room.game.cameras[0].pos.x + (ModManager.MMF ? room.game.rainWorld.options.ScreenSize.x : 1024f) + 200f;
			}
			else
			{
				reset = true;
			}
		}
		if (pos.x > room.game.cameras[0].pos.x + (ModManager.MMF ? room.game.rainWorld.options.ScreenSize.x : 1024f) + 200f)
		{
			if (room.gravity == 0f)
			{
				pos.x = room.game.cameras[0].pos.x - 200f;
			}
			else
			{
				reset = true;
			}
		}
		if (pos.y < room.game.cameras[0].pos.y - 200f)
		{
			if (room.gravity == 0f)
			{
				pos.y = room.game.cameras[0].pos.y + 768f + 200f;
			}
			else
			{
				reset = true;
			}
		}
		if (pos.y > room.game.cameras[0].pos.y + 768f + 200f)
		{
			if (room.gravity == 0f)
			{
				pos.y = room.game.cameras[0].pos.y - 200f;
			}
			else
			{
				reset = true;
			}
		}
		if (room.game.cameras[0].currentCameraPosition != savedCamPos && room.gravity == 0f)
		{
			pos = room.game.cameras[0].pos + new Vector2(Mathf.Lerp(-200f, (ModManager.MMF ? room.game.rainWorld.options.ScreenSize.x : 1024f) + 200f, Random.value), Mathf.Lerp(-200f, 968f, Random.value));
			lastPos = pos;
			savedCamPos = room.game.cameras[0].currentCameraPosition;
		}
		if (!room.BeingViewed)
		{
			Destroy();
		}
	}

	public virtual void ResetMe()
	{
	}
}
