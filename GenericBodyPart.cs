public class GenericBodyPart : BodyPart
{
	public BodyChunk connection;

	public GenericBodyPart(GraphicsModule ow, float rd, float sfFric, float aFric, BodyChunk con)
		: base(ow)
	{
		rad = rd;
		connection = con;
		surfaceFric = sfFric;
		airFriction = aFric;
		Reset(con.pos);
	}

	public override void Update()
	{
		lastPos = pos;
		pos += vel;
		vel *= airFriction;
		PushOutOfTerrain(owner.owner.room, connection.pos);
	}
}
