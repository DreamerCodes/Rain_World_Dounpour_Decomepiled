using System;
using System.Collections.Generic;
using UnityEngine;

namespace Menu.Remix.MixedUI;

public class OpImage : UIelement
{
	public class QueueTexture : UIQueue
	{
		public string description = "";

		protected readonly Texture2D image;

		protected override float sizeY => image.height;

		public QueueTexture(Texture2D image)
		{
			if (!(image != null))
			{
				throw new ArgumentNullException("There is no Texture2D for OpImage");
			}
			this.image = image;
		}

		protected override List<UIelement> _InitializeThisQueue(IHoldUIelements holder, float posX, ref float offsetY)
		{
			offsetY += sizeY;
			float posY = GetPosY(holder, offsetY);
			List<UIelement> list = new List<UIelement>();
			OpImage item = new OpImage(new Vector2(posX, posY), image)
			{
				description = UIQueue.Translate(description)
			};
			list.Add(item);
			hasInitialized = true;
			return list;
		}
	}

	public class QueueSprite : UIQueue
	{
		public string description = "";

		public Color color = Color.white;

		protected readonly string elementName;

		protected override float sizeY => Futile.atlasManager.GetElementWithName(elementName).sourcePixelSize.y;

		public QueueSprite(string elementName)
		{
			if (!Futile.atlasManager.DoesContainElementWithName(elementName))
			{
				throw new ArgumentNullException("There is no such FAtlasElement called [" + elementName + "]");
			}
			this.elementName = elementName;
		}

		protected override List<UIelement> _InitializeThisQueue(IHoldUIelements holder, float posX, ref float offsetY)
		{
			offsetY += sizeY;
			float posY = GetPosY(holder, offsetY);
			List<UIelement> list = new List<UIelement>();
			OpImage item = new OpImage(new Vector2(posX, posY), elementName)
			{
				description = UIQueue.Translate(description),
				color = color
			};
			list.Add(item);
			hasInitialized = true;
			return list;
		}
	}

	private Vector2 _anchor = new Vector2(0f, 0f);

	private Vector2 _scale = new Vector2(1f, 1f);

	public FSprite sprite;

	public Texture2D currentSpriteTexture;

	public readonly bool isTexture;

	private float _alpha = 1f;

	private Color _color = Color.white;

	public Vector2 anchor
	{
		get
		{
			return _anchor;
		}
		set
		{
			if (_anchor != value)
			{
				_anchor = value;
				sprite.SetAnchor(_anchor);
			}
		}
	}

	public Vector2 scale
	{
		get
		{
			return _scale;
		}
		set
		{
			if (_scale != value)
			{
				_scale = value;
				Change();
			}
		}
	}

	public float alpha
	{
		get
		{
			return _alpha;
		}
		set
		{
			if (!Mathf.Approximately(_alpha, value))
			{
				_alpha = value;
				Change();
			}
		}
	}

	public Color color
	{
		get
		{
			return _color;
		}
		set
		{
			if (!isTexture && _color != value)
			{
				_color = value;
				Change();
			}
		}
	}

	public OpImage(Vector2 pos, Texture2D image)
		: base(pos, new Vector2(image.width, image.height))
	{
		if (image == null)
		{
			throw new ElementFormatException(this, "There is no Texture2D for OpImage");
		}
		isTexture = true;
		sprite = new FTexture(image, "img" + image.GetHashCode())
		{
			color = _color
		};
		sprite.SetPosition(Vector2.zero);
		sprite.SetAnchor(_anchor);
		_size = new Vector2(image.width, image.height);
		myContainer.AddChild(sprite);
	}

	public OpImage(Vector2 pos, string fAtlasElement)
		: base(pos, Vector2.zero)
	{
		isTexture = false;
		if (!Futile.atlasManager.DoesContainElementWithName(fAtlasElement))
		{
			throw new ElementFormatException(this, "There is no such FAtlasElement called [" + fAtlasElement + "]");
		}
		sprite = new FSprite(fAtlasElement)
		{
			color = _color
		};
		sprite.SetPosition(Vector2.zero);
		sprite.SetAnchor(_anchor);
		_size = sprite.element.sourceSize;
		myContainer.AddChild(sprite);
	}

	public void ChangeImage(Texture2D newImage)
	{
		if (!isTexture)
		{
			throw new InvalidActionException(this, "You must construct this with Texture2D to use this function");
		}
		if (newImage == null)
		{
			MachineConnector.LogError("newImage is null in OpImage.ChangeImage!");
			return;
		}
		_color = Color.white;
		_size = new Vector2((float)newImage.width * _scale.x, (float)newImage.height * _scale.y);
		(sprite as FTexture).SetTexture(newImage);
		currentSpriteTexture = newImage;
		sprite.SetAnchor(_anchor);
		sprite.alpha = _alpha;
		sprite.color = Color.white;
		myContainer.AddChild(sprite);
	}

	public void ChangeElement(string newElement)
	{
		if (isTexture)
		{
			throw new InvalidActionException(this, "You must construct this with a name of FAtlasElement to use this function");
		}
		if (string.IsNullOrEmpty(newElement))
		{
			MachineConnector.LogError("newElement is null in OpImage.ChangeElement!");
		}
		else if (!(sprite.element.name == newElement))
		{
			myContainer.RemoveAllChildren();
			if (!Futile.atlasManager.DoesContainElementWithName(newElement))
			{
				MachineConnector.LogError("There is no such FAtlasElement called [" + newElement + "]");
				return;
			}
			sprite = new FSprite(newElement)
			{
				color = _color,
				alpha = _alpha
			};
			myContainer.AddChild(sprite);
			_size = new Vector2(sprite.element.sourceSize.x * _scale.x, sprite.element.sourceSize.y * _scale.y);
			sprite.SetAnchor(_anchor);
		}
	}

	protected internal override void Change()
	{
		base.Change();
		sprite.alpha = _alpha;
		sprite.color = _color;
		sprite.SetPosition(Vector2.zero);
		sprite.scaleX = _scale.x;
		sprite.scaleY = _scale.y;
		_size = new Vector2(sprite.element.sourceSize.x * _scale.x, sprite.element.sourceSize.y * _scale.y);
	}

	protected internal override void Unload()
	{
		base.Unload();
		if (isTexture)
		{
			(sprite as FTexture).Destroy();
		}
	}
}
