namespace DevInterface;

public class TileObjectRepresentation : PlacedObjectRepresentation
{
	private int tileSprite = -1;

	public TileObjectRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name)
		: base(owner, IDstring, parentNode, pObj, name)
	{
		fSprites.Add(new FSprite("pixel"));
		tileSprite = fSprites.Count - 1;
		fSprites[tileSprite].scale = 20f;
		fSprites[tileSprite].alpha = 0.4f;
		owner.placedObjectsContainer.AddChild(fSprites[tileSprite]);
	}

	public override void Refresh()
	{
		base.Refresh();
		if (tileSprite > -1)
		{
			fSprites[tileSprite].x = owner.room.MiddleOfTile(pObj.pos).x - owner.room.game.cameras[0].pos.x;
			fSprites[tileSprite].y = owner.room.MiddleOfTile(pObj.pos).y - owner.room.game.cameras[0].pos.y;
		}
	}
}
