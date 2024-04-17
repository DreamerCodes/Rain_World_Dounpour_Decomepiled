using RWCustom;
using UnityEngine;

namespace DevInterface;

public class DirectionalSoundHandle : Handle
{
	private DirectionalSound dirSound;

	private Vector2 OnCirclePos(Vector2 dir)
	{
		return new Vector2(683f, 384f) + new Vector2(dir.x * 500f, dir.y * 300f);
	}

	public DirectionalSoundHandle(DevUI owner, string IDstring, DevUINode parentNode, DirectionalSound sound, Vector2 pos, string name)
		: base(owner, IDstring, parentNode, pos)
	{
		dirSound = sound;
		fSprites.Add(new TriangleMesh("Futile_White", new TriangleMesh.Triangle[1]
		{
			new TriangleMesh.Triangle(0, 1, 2)
		}, customColor: false));
		owner.placedObjectsContainer.AddChild(fSprites[1]);
		fSprites.Add(new FSprite("pixel"));
		owner.placedObjectsContainer.AddChild(fSprites[2]);
		fSprites[2].anchorY = 0f;
	}

	public override void Update()
	{
		base.Update();
		if (dragged)
		{
			dirSound.direction = -Custom.DirVec(absPos, new Vector2(683f, 384f));
		}
		else
		{
			AbsMove(OnCirclePos(dirSound.direction));
		}
	}

	public override void Refresh()
	{
		base.Refresh();
		Vector2 vector = -dirSound.direction;
		Vector2 vector2 = OnCirclePos(dirSound.direction);
		(fSprites[1] as TriangleMesh).MoveVertice(0, vector2 + vector * 20f);
		(fSprites[1] as TriangleMesh).MoveVertice(1, vector2 - vector * 10f - Custom.PerpendicularVector(vector) * 10f);
		(fSprites[1] as TriangleMesh).MoveVertice(2, vector2 - vector * 10f + Custom.PerpendicularVector(vector) * 10f);
		fSprites[1].color = fSprites[0].color;
		MoveSprite(2, vector2);
		fSprites[2].scaleY = Vector2.Distance(vector2, (parentNode as PositionedDevUINode).absPos);
		fSprites[2].rotation = Custom.AimFromOneVectorToAnother(vector2, (parentNode as PositionedDevUINode).absPos);
	}
}
