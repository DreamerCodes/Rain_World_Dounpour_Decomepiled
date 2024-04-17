using RWCustom;
using UnityEngine;

namespace MoreSlugcats;

public class MSArteryWaterFlow : UpdatableAndDeletable
{
	private int bubbleTimer;

	private float PushIntensity;

	public PlacedObject placedObj;

	public Vector2 Pos => placedObj.pos;

	public float Rad => (placedObj.data as PlacedObject.ResizableObjectData).handlePos.magnitude;

	public Vector2 Dir => (placedObj.data as PlacedObject.ResizableObjectData).handlePos.normalized;

	public MSArteryWaterFlow(Room room, PlacedObject placedObj)
	{
		base.room = room;
		this.placedObj = placedObj;
		PushIntensity = 0f;
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		PushIntensity = Mathf.Lerp(PushIntensity, Mathf.InverseLerp(1f, 0f, room.gravity), 0.2f);
		if (!(PushIntensity > 0.2f))
		{
			return;
		}
		if (bubbleTimer <= 0)
		{
			Bubble bubble = new Bubble(Pos + Custom.RNV() * Rad, Dir * 9f, bottomBubble: false, fakeWaterBubble: false);
			bubble.doNotSlow = true;
			bubble.age = 30;
			room.AddObject(bubble);
			bubbleTimer = Random.Range(10, 15);
		}
		else if (room.ViewedByAnyCamera(Pos, Rad))
		{
			bubbleTimer--;
		}
		for (int i = 0; i < room.physicalObjects.Length; i++)
		{
			for (int num = room.physicalObjects[i].Count - 1; num >= 0; num--)
			{
				PhysicalObject physicalObject = room.physicalObjects[i][num];
				if (physicalObject.grabbedBy.Count == 0)
				{
					float num2 = Vector2.Distance(physicalObject.firstChunk.pos, Pos);
					if (num2 < Rad)
					{
						float num3 = Mathf.Lerp(40f, 2000f, Mathf.InverseLerp(0.94f, 0.96f, physicalObject.waterFriction * (1f - physicalObject.waterRetardationImmunity))) * PushIntensity;
						if (physicalObject.firstChunk.submersion < 0.5f)
						{
							num3 = 25f * PushIntensity;
						}
						num3 *= num2 / Rad;
						num3 *= Mathf.InverseLerp(-1f, 5f, physicalObject.TotalMass);
						Vector2 vector = Dir * num3;
						physicalObject.firstChunk.vel += vector;
					}
				}
			}
		}
	}
}
