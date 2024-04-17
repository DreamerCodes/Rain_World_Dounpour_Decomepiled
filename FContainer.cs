using System.Collections.Generic;

public class FContainer : FNode
{
	protected List<FNode> _childNodes = new List<FNode>(5);

	private int _oldChildNodesHash;

	private bool _shouldSortByZ;

	public bool shouldSortByZ
	{
		get
		{
			return _shouldSortByZ;
		}
		set
		{
			if (_shouldSortByZ == value)
			{
				return;
			}
			_shouldSortByZ = value;
			if (_shouldSortByZ)
			{
				if (_isOnStage)
				{
					Futile.instance.SignalUpdate += HandleUpdateAndSort;
				}
			}
			else if (_isOnStage)
			{
				Futile.instance.SignalUpdate -= HandleUpdateAndSort;
			}
		}
	}

	public override void Redraw(bool shouldForceDirty, bool shouldUpdateDepth)
	{
		bool flag = _isMatrixDirty;
		bool isAlphaDirty = _isAlphaDirty;
		UpdateDepthMatrixAlpha(shouldForceDirty, shouldUpdateDepth);
		int count = _childNodes.Count;
		for (int i = 0; i < count; i++)
		{
			_childNodes[i].Redraw(shouldForceDirty || flag || isAlphaDirty, shouldUpdateDepth);
		}
	}

	public override void HandleAddedToStage()
	{
		if (!_isOnStage)
		{
			base.HandleAddedToStage();
			int count = _childNodes.Count;
			for (int i = 0; i < count; i++)
			{
				FNode fNode = _childNodes[i];
				fNode.stage = _stage;
				fNode.HandleAddedToStage();
			}
			if (_shouldSortByZ)
			{
				Futile.instance.SignalUpdate += HandleUpdateAndSort;
			}
		}
	}

	public override void HandleRemovedFromStage()
	{
		if (_isOnStage)
		{
			base.HandleRemovedFromStage();
			int count = _childNodes.Count;
			for (int i = 0; i < count; i++)
			{
				FNode fNode = _childNodes[i];
				fNode.HandleRemovedFromStage();
				fNode.stage = null;
			}
			if (_shouldSortByZ)
			{
				Futile.instance.SignalUpdate -= HandleUpdateAndSort;
			}
		}
	}

	private void HandleUpdateAndSort()
	{
		if (SortByZ() && _isOnStage)
		{
			_stage.HandleFacetsChanged();
		}
	}

	public void AddChild(FNode node)
	{
		int num = _childNodes.IndexOf(node);
		if (num == -1)
		{
			node.HandleAddedToContainer(this);
			_childNodes.Add(node);
			if (_isOnStage)
			{
				node.stage = _stage;
				node.HandleAddedToStage();
			}
		}
		else if (num != _childNodes.Count - 1)
		{
			_childNodes.RemoveAt(num);
			_childNodes.Add(node);
			if (_isOnStage)
			{
				_stage.HandleFacetsChanged();
			}
		}
	}

	public void AddChildAtIndex(FNode node, int newIndex)
	{
		int num = _childNodes.IndexOf(node);
		if (newIndex > _childNodes.Count)
		{
			newIndex = _childNodes.Count;
		}
		if (num == newIndex)
		{
			return;
		}
		if (num == -1)
		{
			node.HandleAddedToContainer(this);
			_childNodes.Insert(newIndex, node);
			if (_isOnStage)
			{
				node.stage = _stage;
				node.HandleAddedToStage();
			}
			return;
		}
		_childNodes.RemoveAt(num);
		if (num < newIndex)
		{
			_childNodes.Insert(newIndex - 1, node);
		}
		else
		{
			_childNodes.Insert(newIndex, node);
		}
		if (_isOnStage)
		{
			_stage.HandleFacetsChanged();
		}
	}

	public void RemoveChild(FNode node)
	{
		if (node.container == this)
		{
			node.HandleRemovedFromContainer();
			if (_isOnStage)
			{
				node.HandleRemovedFromStage();
				node.stage = null;
			}
			_childNodes.Remove(node);
		}
	}

	public void RemoveAllChildren()
	{
		int count = _childNodes.Count;
		for (int i = 0; i < count; i++)
		{
			FNode fNode = _childNodes[i];
			fNode.HandleRemovedFromContainer();
			if (_isOnStage)
			{
				fNode.HandleRemovedFromStage();
				fNode.stage = null;
			}
		}
		_childNodes.Clear();
	}

	public int GetChildCount()
	{
		return _childNodes.Count;
	}

	public FNode GetChildAt(int childIndex)
	{
		return _childNodes[childIndex];
	}

	public int GetChildIndex(FNode childNode)
	{
		return _childNodes.IndexOf(childNode);
	}

	private static int ZComparison(FNode a, FNode b)
	{
		float num = a.sortZ - b.sortZ;
		if (num < 0f)
		{
			return -1;
		}
		if (num > 0f)
		{
			return 1;
		}
		return 0;
	}

	private bool SortByZ()
	{
		_childNodes.InsertionSort(ZComparison);
		int num = 269;
		int count = _childNodes.Count;
		for (int i = 0; i < count; i++)
		{
			num = num * 17 + _childNodes[i].GetHashCode();
		}
		if (num != _oldChildNodesHash)
		{
			_oldChildNodesHash = num;
			return true;
		}
		return false;
	}
}
