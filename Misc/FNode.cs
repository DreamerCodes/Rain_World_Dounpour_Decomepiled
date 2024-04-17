using System;
using System.Collections.Generic;
using UnityEngine;

public class FNode
{
	protected float _x;

	protected float _y;

	protected float _scaleX = 1f;

	protected float _scaleY = 1f;

	protected float _rotation;

	protected float _meshZ;

	protected float _sortZ;

	protected bool _isMatrixDirty;

	protected FContainer _container;

	protected FMatrix _matrix;

	protected FMatrix _concatenatedMatrix;

	protected FMatrix _inverseConcatenatedMatrix;

	protected FMatrix _screenConcatenatedMatrix;

	protected FMatrix _screenInverseConcatenatedMatrix;

	protected bool _needsSpecialMatrices;

	protected float _alpha = 1f;

	protected float _concatenatedAlpha = 1f;

	protected bool _isAlphaDirty;

	protected float _visibleScale = 1f;

	protected bool _isOnStage;

	protected int _depth;

	protected FStage _stage;

	protected bool _isVisible = true;

	public object data;

	private List<FNodeEnabler> _enablers;

	public bool isVisible
	{
		get
		{
			return _isVisible;
		}
		set
		{
			if (_isVisible != value)
			{
				_isVisible = value;
				_visibleScale = (_isVisible ? 1f : 0f);
				_isMatrixDirty = true;
			}
		}
	}

	public float x
	{
		get
		{
			return _x;
		}
		set
		{
			_x = value;
			_isMatrixDirty = true;
		}
	}

	public float y
	{
		get
		{
			return _y;
		}
		set
		{
			_y = value;
			_isMatrixDirty = true;
		}
	}

	public float meshZ
	{
		get
		{
			return _meshZ;
		}
		set
		{
			_meshZ = value;
			_isMatrixDirty = true;
		}
	}

	public virtual float sortZ
	{
		get
		{
			return _sortZ;
		}
		set
		{
			_sortZ = value;
		}
	}

	public float scaleX
	{
		get
		{
			return _scaleX;
		}
		set
		{
			_scaleX = value;
			_isMatrixDirty = true;
		}
	}

	public float scaleY
	{
		get
		{
			return _scaleY;
		}
		set
		{
			_scaleY = value;
			_isMatrixDirty = true;
		}
	}

	public float scale
	{
		get
		{
			return scaleX;
		}
		set
		{
			_scaleX = value;
			scaleY = value;
			_isMatrixDirty = true;
		}
	}

	public float rotation
	{
		get
		{
			return _rotation;
		}
		set
		{
			_rotation = value;
			_isMatrixDirty = true;
		}
	}

	public bool isMatrixDirty => _isMatrixDirty;

	public FContainer container => _container;

	public int depth => _depth;

	public virtual int touchPriority => _depth;

	public virtual FMatrix matrix => _matrix;

	public virtual FMatrix concatenatedMatrix => _concatenatedMatrix;

	public virtual FMatrix inverseConcatenatedMatrix
	{
		get
		{
			if (!_needsSpecialMatrices)
			{
				CreateSpecialMatrices();
			}
			return _inverseConcatenatedMatrix;
		}
	}

	public virtual FMatrix screenConcatenatedMatrix
	{
		get
		{
			if (!_needsSpecialMatrices)
			{
				CreateSpecialMatrices();
			}
			return _screenConcatenatedMatrix;
		}
	}

	public virtual FMatrix screenInverseConcatenatedMatrix
	{
		get
		{
			if (!_needsSpecialMatrices)
			{
				CreateSpecialMatrices();
			}
			return _screenInverseConcatenatedMatrix;
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
			float num = Math.Max(0f, Math.Min(1f, value));
			if (_alpha != num)
			{
				_alpha = num;
				_isAlphaDirty = true;
			}
		}
	}

	public float concatenatedAlpha => _concatenatedAlpha;

	public FStage stage
	{
		get
		{
			return _stage;
		}
		set
		{
			_stage = value;
		}
	}

	public FNode()
	{
		_matrix = new FMatrix();
		_concatenatedMatrix = new FMatrix();
	}

	protected void AddEnabler(FNodeEnabler enabler)
	{
		if (_enablers == null)
		{
			_enablers = new List<FNodeEnabler>();
		}
		_enablers.Add(enabler);
		if (_isOnStage)
		{
			enabler.Connect();
		}
	}

	protected void RemoveEnabler(FNodeEnabler enabler)
	{
		if (_enablers != null)
		{
			if (_isOnStage)
			{
				enabler.Disconnect();
			}
			_enablers.Remove(enabler);
		}
	}

	protected void RemoveEnablerOfType(Type enablerType)
	{
		if (_enablers == null)
		{
			return;
		}
		for (int num = _enablers.Count - 1; num >= 0; num--)
		{
			if (_enablers[num].GetType() == enablerType)
			{
				if (_isOnStage)
				{
					_enablers[num].Disconnect();
				}
				_enablers.RemoveAt(num);
			}
		}
	}

	public void ListenForResize(FScreen.ScreenResizeDelegate handleResizeCallback)
	{
		RemoveEnablerOfType(typeof(FNodeEnablerForResize));
		AddEnabler(new FNodeEnablerForResize(handleResizeCallback));
	}

	public void RemoveListenForResize()
	{
		RemoveEnablerOfType(typeof(FNodeEnablerForResize));
	}

	public void ListenForOrientationChange(FScreen.ScreenOrientationChangeDelegate handleOrientationChangeCallback)
	{
		RemoveEnablerOfType(typeof(FNodeEnablerForOrientationChange));
		AddEnabler(new FNodeEnablerForOrientationChange(handleOrientationChangeCallback));
	}

	public void RemoveListenForOrientationChange()
	{
		RemoveEnablerOfType(typeof(FNodeEnablerForOrientationChange));
	}

	public void ListenForUpdate(Futile.FutileUpdateDelegate handleUpdateCallback)
	{
		RemoveEnablerOfType(typeof(FNodeEnablerForUpdate));
		AddEnabler(new FNodeEnablerForUpdate(handleUpdateCallback));
	}

	public void RemoveListenForUpdate()
	{
		RemoveEnablerOfType(typeof(FNodeEnablerForUpdate));
	}

	public void ListenForAfterUpdate(Futile.FutileUpdateDelegate handleUpdateCallback)
	{
		RemoveEnablerOfType(typeof(FNodeEnablerForAfterUpdate));
		AddEnabler(new FNodeEnablerForAfterUpdate(handleUpdateCallback));
	}

	public void RemoveListenForAfterUpdate()
	{
		RemoveEnablerOfType(typeof(FNodeEnablerForAfterUpdate));
	}

	public void ListenForLateUpdate(Futile.FutileUpdateDelegate handleUpdateCallback)
	{
		RemoveEnablerOfType(typeof(FNodeEnablerForLateUpdate));
		AddEnabler(new FNodeEnablerForLateUpdate(handleUpdateCallback));
	}

	public void RemoveListenForLateUpdate()
	{
		RemoveEnablerOfType(typeof(FNodeEnablerForLateUpdate));
	}

	public void ListenForFixedUpdate(Futile.FutileUpdateDelegate handleUpdateCallback)
	{
		RemoveEnablerOfType(typeof(FNodeEnablerForFixedUpdate));
		AddEnabler(new FNodeEnablerForFixedUpdate(handleUpdateCallback));
	}

	public void RemoveListenForFixedUpdate()
	{
		RemoveEnablerOfType(typeof(FNodeEnablerForFixedUpdate));
	}

	public void EnableSingleTouch()
	{
		DisableSingleTouch();
		AddEnabler(new FNodeEnablerForSingleTouch(this));
	}

	public void DisableSingleTouch()
	{
		RemoveEnablerOfType(typeof(FNodeEnablerForSingleTouch));
	}

	public void EnableMultiTouch()
	{
		DisableMultiTouch();
		AddEnabler(new FNodeEnablerForMultiTouch(this));
	}

	public void DisableMultiTouch()
	{
		RemoveEnablerOfType(typeof(FNodeEnablerForMultiTouch));
	}

	public void EnableSmartTouch()
	{
		DisableSmartTouch();
		AddEnabler(new FNodeEnablerForSmartTouch(this));
	}

	public void DisableSmartTouch()
	{
		RemoveEnablerOfType(typeof(FNodeEnablerForSmartTouch));
	}

	public void ListenForAddedOrRemoved(FNodeEnablerForAddedOrRemoved.Delegate handleAddedOrRemoved)
	{
		RemoveEnablerOfType(typeof(FNodeEnablerForAddedOrRemoved));
		AddEnabler(new FNodeEnablerForAddedOrRemoved(handleAddedOrRemoved));
	}

	public void RemoveListenForAddedOrRemoved()
	{
		RemoveEnablerOfType(typeof(FNodeEnablerForAddedOrRemoved));
	}

	public virtual void HandleAddedToStage()
	{
		_isOnStage = true;
		if (_enablers != null)
		{
			int count = _enablers.Count;
			for (int i = 0; i < count; i++)
			{
				_enablers[i].Connect();
			}
		}
	}

	public virtual void HandleRemovedFromStage()
	{
		_isOnStage = false;
		if (_enablers != null)
		{
			int count = _enablers.Count;
			for (int i = 0; i < count; i++)
			{
				_enablers[i].Disconnect();
			}
		}
	}

	public Vector2 LocalToScreen(Vector2 localVector)
	{
		if (_container != null)
		{
			_container.UpdateMatrix();
		}
		_isMatrixDirty = true;
		UpdateMatrix();
		float num = (0f - Futile.screen.originX) * (float)Futile.screen.pixelWidth;
		float num2 = (0f - Futile.screen.originY) * (float)Futile.screen.pixelHeight;
		localVector = screenConcatenatedMatrix.GetNewTransformedVector(localVector);
		return new Vector2(localVector.x / Futile.displayScaleInverse - num, localVector.y / Futile.displayScaleInverse - num2);
	}

	public Vector2 ScreenToLocal(Vector2 screenVector)
	{
		if (_container != null)
		{
			_container.UpdateMatrix();
		}
		_isMatrixDirty = true;
		UpdateMatrix();
		float num = (0f - Futile.screen.originX) * (float)Futile.screen.pixelWidth;
		float num2 = (0f - Futile.screen.originY) * (float)Futile.screen.pixelHeight;
		screenVector = new Vector2((screenVector.x + num) * Futile.displayScaleInverse, (screenVector.y + num2) * Futile.displayScaleInverse);
		return screenInverseConcatenatedMatrix.GetNewTransformedVector(screenVector);
	}

	public Vector2 LocalToStage(Vector2 localVector)
	{
		if (_container != null)
		{
			_container.UpdateMatrix();
		}
		_isMatrixDirty = true;
		UpdateMatrix();
		return _concatenatedMatrix.GetNewTransformedVector(localVector);
	}

	public Vector2 StageToLocal(Vector2 globalVector)
	{
		if (_container != null)
		{
			_container.UpdateMatrix();
		}
		_isMatrixDirty = true;
		UpdateMatrix();
		return inverseConcatenatedMatrix.GetNewTransformedVector(globalVector);
	}

	public Vector2 LocalToGlobal(Vector2 localVector)
	{
		if (_container != null)
		{
			_container.UpdateMatrix();
		}
		_isMatrixDirty = true;
		UpdateMatrix();
		return screenConcatenatedMatrix.GetNewTransformedVector(localVector);
	}

	public Vector2 GlobalToLocal(Vector2 globalVector)
	{
		if (_container != null)
		{
			_container.UpdateMatrix();
		}
		_isMatrixDirty = true;
		UpdateMatrix();
		return screenInverseConcatenatedMatrix.GetNewTransformedVector(globalVector);
	}

	public Vector2 OtherToLocal(FNode otherNode, Vector2 otherVector)
	{
		return GlobalToLocal(otherNode.LocalToGlobal(otherVector));
	}

	public Vector2 LocalToOther(Vector2 localVector, FNode otherNode)
	{
		return otherNode.GlobalToLocal(LocalToGlobal(localVector));
	}

	public Vector2 GetLocalMousePosition()
	{
		return ScreenToLocal(Input.mousePosition);
	}

	public Vector2 GetLocalTouchPosition(FTouch touch)
	{
		return GlobalToLocal(touch.position);
	}

	public void UpdateMatrix()
	{
		if (!_isMatrixDirty)
		{
			return;
		}
		_matrix.SetScaleThenRotate(_x, _y, _scaleX * _visibleScale, _scaleY * _visibleScale, _rotation * -0.01745329f);
		if (_container != null)
		{
			_concatenatedMatrix.ConcatAndCopyValues(_matrix, _container.concatenatedMatrix);
		}
		else
		{
			_concatenatedMatrix.CopyValues(_matrix);
		}
		if (_needsSpecialMatrices)
		{
			_inverseConcatenatedMatrix.InvertAndCopyValues(_concatenatedMatrix);
			if (_isOnStage)
			{
				_screenConcatenatedMatrix.ConcatAndCopyValues(_concatenatedMatrix, _stage.screenConcatenatedMatrix);
			}
			else
			{
				_screenConcatenatedMatrix.CopyValues(_concatenatedMatrix);
			}
			_screenInverseConcatenatedMatrix.InvertAndCopyValues(_screenConcatenatedMatrix);
		}
	}

	protected virtual void UpdateDepthMatrixAlpha(bool shouldForceDirty, bool shouldUpdateDepth)
	{
		if (shouldUpdateDepth)
		{
			_depth = _stage.nextNodeDepth++;
		}
		if (_isMatrixDirty || shouldForceDirty)
		{
			_isMatrixDirty = false;
			_matrix.SetScaleThenRotate(_x, _y, _scaleX * _visibleScale, _scaleY * _visibleScale, _rotation * -0.01745329f);
			if (_container != null)
			{
				_concatenatedMatrix.ConcatAndCopyValues(_matrix, _container.concatenatedMatrix);
			}
			else
			{
				_concatenatedMatrix.CopyValues(_matrix);
			}
		}
		if (_needsSpecialMatrices)
		{
			_inverseConcatenatedMatrix.InvertAndCopyValues(_concatenatedMatrix);
			_screenConcatenatedMatrix.ConcatAndCopyValues(_concatenatedMatrix, _stage.screenConcatenatedMatrix);
			_screenInverseConcatenatedMatrix.InvertAndCopyValues(_screenConcatenatedMatrix);
		}
		if (_isAlphaDirty || shouldForceDirty)
		{
			_isAlphaDirty = false;
			if (_container != null)
			{
				_concatenatedAlpha = _container.concatenatedAlpha * _alpha;
			}
			else
			{
				_concatenatedAlpha = _alpha;
			}
		}
	}

	public virtual void Redraw(bool shouldForceDirty, bool shouldUpdateDepth)
	{
		UpdateDepthMatrixAlpha(shouldForceDirty, shouldUpdateDepth);
	}

	public virtual void HandleAddedToContainer(FContainer container)
	{
		if (_container != container)
		{
			if (_container != null)
			{
				_container.RemoveChild(this);
			}
			_container = container;
		}
	}

	public virtual void HandleRemovedFromContainer()
	{
		_container = null;
	}

	public void RemoveFromContainer()
	{
		if (_container != null)
		{
			_container.RemoveChild(this);
		}
	}

	public void MoveToFront()
	{
		if (_container != null)
		{
			_container.AddChild(this);
		}
	}

	public void MoveToBack()
	{
		if (_container != null)
		{
			_container.AddChildAtIndex(this, 0);
		}
	}

	public void MoveInFrontOfOtherNode(FNode otherNode)
	{
		if (_container != null && otherNode.container == _container)
		{
			_container.AddChildAtIndex(this, _container.GetChildIndex(otherNode) + 1);
		}
	}

	public void MoveBehindOtherNode(FNode otherNode)
	{
		if (_container != null && otherNode.container == _container)
		{
			_container.AddChildAtIndex(this, _container.GetChildIndex(otherNode));
		}
	}

	public bool IsAncestryVisible()
	{
		if (_isVisible)
		{
			if (container != null)
			{
				return container.IsAncestryVisible();
			}
			return true;
		}
		return false;
	}

	protected void CreateSpecialMatrices()
	{
		_needsSpecialMatrices = true;
		_inverseConcatenatedMatrix = new FMatrix();
		_screenConcatenatedMatrix = new FMatrix();
		_screenInverseConcatenatedMatrix = new FMatrix();
		if (_isOnStage)
		{
			_inverseConcatenatedMatrix.InvertAndCopyValues(_concatenatedMatrix);
			_screenConcatenatedMatrix.ConcatAndCopyValues(_concatenatedMatrix, _stage.screenConcatenatedMatrix);
			_screenInverseConcatenatedMatrix.InvertAndCopyValues(_screenConcatenatedMatrix);
		}
	}

	public void RotateAroundPointRelative(Vector2 localPoint, float relativeDegrees)
	{
		FMatrix tempMatrix = FMatrix.tempMatrix;
		tempMatrix.ResetToIdentity();
		tempMatrix.SetScaleThenRotate(0f, 0f, _scaleX, _scaleY, _rotation * (-(float)Math.PI / 180f));
		Vector2 newTransformedVector = tempMatrix.GetNewTransformedVector(new Vector2(0f - localPoint.x, 0f - localPoint.y));
		_rotation += relativeDegrees;
		tempMatrix.ResetToIdentity();
		tempMatrix.SetScaleThenRotate(0f, 0f, _scaleX, _scaleY, _rotation * (-(float)Math.PI / 180f));
		Vector2 newTransformedVector2 = tempMatrix.GetNewTransformedVector(new Vector2(0f - localPoint.x, 0f - localPoint.y));
		_x += newTransformedVector2.x - newTransformedVector.x;
		_y += newTransformedVector2.y - newTransformedVector.y;
		_isMatrixDirty = true;
	}

	public void RotateAroundPointAbsolute(Vector2 localPoint, float absoluteDegrees)
	{
		RotateAroundPointRelative(localPoint, absoluteDegrees - _rotation);
	}

	public void ScaleAroundPointRelative(Vector2 localPoint, float relativeScaleX, float relativeScaleY)
	{
		FMatrix tempMatrix = FMatrix.tempMatrix;
		tempMatrix.ResetToIdentity();
		tempMatrix.SetScaleThenRotate(0f, 0f, relativeScaleX - 1f, relativeScaleY - 1f, _rotation * (-(float)Math.PI / 180f));
		Vector2 newTransformedVector = tempMatrix.GetNewTransformedVector(new Vector2(localPoint.x * _scaleX, localPoint.y * _scaleY));
		_x += 0f - newTransformedVector.x;
		_y += 0f - newTransformedVector.y;
		_scaleX *= relativeScaleX;
		_scaleY *= relativeScaleY;
		_isMatrixDirty = true;
	}

	public void ScaleAroundPointAbsolute(Vector2 localPoint, float absoluteScaleX, float absoluteScaleY)
	{
		ScaleAroundPointRelative(localPoint, absoluteScaleX / _scaleX, absoluteScaleX / _scaleY);
	}

	public void SetPosition(float newX, float newY)
	{
		x = newX;
		y = newY;
	}

	public void SetPosition(Vector2 newPosition)
	{
		x = newPosition.x;
		y = newPosition.y;
	}

	public Vector2 GetPosition()
	{
		return new Vector2(_x, _y);
	}
}
