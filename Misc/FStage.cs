using System;
using UnityEngine;

public class FStage : FContainer
{
	public int nextNodeDepth;

	public int index;

	private bool _needsDepthUpdate;

	private FRenderer _renderer;

	private string _name;

	private FMatrix _identityMatrix;

	private bool _doesRendererNeedTransformChange;

	private FStageTransform _transform;

	private FMatrix _followMatrix = new FMatrix();

	private FNode _followTarget;

	private bool _shouldFollowScale;

	private bool _shouldFollowRotation;

	private int _layer;

	public int layer
	{
		get
		{
			return _layer;
		}
		set
		{
			if (_layer != value)
			{
				_layer = value;
				_doesRendererNeedTransformChange = true;
			}
		}
	}

	public override FMatrix matrix => _identityMatrix;

	public override FMatrix concatenatedMatrix => _identityMatrix;

	public override FMatrix inverseConcatenatedMatrix => _identityMatrix;

	public FMatrix screenMatrix => _matrix;

	public override FMatrix screenConcatenatedMatrix => _concatenatedMatrix;

	public override FMatrix screenInverseConcatenatedMatrix => _inverseConcatenatedMatrix;

	public FRenderer renderer => _renderer;

	public string name => _name;

	public FStageTransform transform => _transform;

	public new float scaleX
	{
		get
		{
			return _scaleX;
		}
		set
		{
			throw new NotSupportedException("Stage scale must be uniform! Use stage.scale instead");
		}
	}

	public new float scaleY
	{
		get
		{
			return _scaleY;
		}
		set
		{
			throw new NotSupportedException("Stage scale must be uniform! Use stage.scale instead");
		}
	}

	public FStage(string name)
	{
		_name = name;
		_stage = this;
		_renderer = new FRenderer(this);
		_identityMatrix = new FMatrix();
		_identityMatrix.ResetToIdentity();
		_inverseConcatenatedMatrix = new FMatrix();
		_screenConcatenatedMatrix = new FMatrix();
		_screenInverseConcatenatedMatrix = new FMatrix();
	}

	public void HandleAddedToFutile()
	{
		HandleAddedToStage();
	}

	public void HandleRemovedFromFutile()
	{
		_renderer.Clear();
		HandleRemovedFromStage();
	}

	public void HandleFacetsChanged()
	{
		_needsDepthUpdate = true;
	}

	protected override void UpdateDepthMatrixAlpha(bool shouldForceDirty, bool shouldUpdateDepth)
	{
		if (shouldUpdateDepth)
		{
			_depth = nextNodeDepth++;
		}
		if (_isMatrixDirty || shouldForceDirty)
		{
			_isMatrixDirty = false;
			_matrix.SetScaleThenRotate(_x, _y, _scaleX * _visibleScale, _scaleY * _visibleScale, _rotation * (-(float)Math.PI / 180f));
			_concatenatedMatrix.CopyValues(_matrix);
			_inverseConcatenatedMatrix.InvertAndCopyValues(_concatenatedMatrix);
			_doesRendererNeedTransformChange = true;
		}
		if (_isAlphaDirty || shouldForceDirty)
		{
			_isAlphaDirty = false;
			_concatenatedAlpha = _alpha;
		}
	}

	public override void Redraw(bool shouldForceDirty, bool shouldUpdateDepth)
	{
		bool flag = _needsDepthUpdate || shouldUpdateDepth;
		_needsDepthUpdate = false;
		if (flag)
		{
			shouldForceDirty = true;
			shouldUpdateDepth = true;
			nextNodeDepth = index * 10000;
			_renderer.StartRender();
		}
		bool isAlphaDirty = _isAlphaDirty;
		UpdateDepthMatrixAlpha(shouldForceDirty, shouldUpdateDepth);
		int count = _childNodes.Count;
		for (int i = 0; i < count; i++)
		{
			_childNodes[i].Redraw(shouldForceDirty || isAlphaDirty, shouldUpdateDepth);
		}
		UpdateFollow();
		if (flag)
		{
			_renderer.EndRender();
			Futile.touchManager.HandleDepthChange();
		}
		if (_doesRendererNeedTransformChange)
		{
			_doesRendererNeedTransformChange = false;
			_transform.position = new Vector3(_x, _y, 0f);
			_transform.rotation = Quaternion.AngleAxis(_rotation, Vector3.back);
			_transform.localScale = new Vector3(_scaleX * _visibleScale, _scaleX * _visibleScale, _scaleX * _visibleScale);
			_renderer.UpdateLayerTransforms();
		}
	}

	public void CenterOn(Vector2 globalPosition)
	{
		if (_followTarget != null)
		{
			_followTarget = null;
		}
		Vector2 vector = GlobalToLocal(globalPosition);
		_followMatrix.SetScaleThenRotate(0f, 0f, _scaleX, _scaleY, _rotation * (-(float)Math.PI / 180f));
		Vector2 newTransformedVector = _followMatrix.GetNewTransformedVector(vector);
		base.x = 0f - newTransformedVector.x;
		base.y = 0f - newTransformedVector.y;
	}

	public void Follow(FNode followTarget, bool shouldFollowScale, bool shouldFollowRotation)
	{
		_followTarget = followTarget;
		_shouldFollowScale = shouldFollowScale;
		_shouldFollowRotation = shouldFollowRotation;
	}

	private void UpdateFollow()
	{
		if (_followTarget == null)
		{
			return;
		}
		if (_followTarget.stage == null)
		{
			_followTarget = null;
			return;
		}
		if (_shouldFollowScale)
		{
			base.scale = 1f / _followTarget.concatenatedMatrix.GetScaleX();
		}
		if (_shouldFollowRotation)
		{
			base.rotation = _followTarget.concatenatedMatrix.GetRotation() * (180f / (float)Math.PI);
		}
		_followMatrix.SetScaleThenRotate(0f, 0f, _scaleX, _scaleY, _rotation * (-(float)Math.PI / 180f));
		Vector2 newTransformedVector = _followMatrix.GetNewTransformedVector(new Vector2(_followTarget.concatenatedMatrix.tx, _followTarget.concatenatedMatrix.ty));
		base.x = 0f - newTransformedVector.x;
		base.y = 0f - newTransformedVector.y;
	}

	public void Unfollow(FNode targetToUnfollow, bool shouldResetPosition)
	{
		if (targetToUnfollow == null || _followTarget == targetToUnfollow)
		{
			_followTarget = null;
			if (shouldResetPosition)
			{
				ResetPosition();
			}
		}
	}

	public void ResetPosition()
	{
		_x = 0f;
		_y = 0f;
		_scaleX = 1f;
		_scaleY = 1f;
		_rotation = 0f;
		_isMatrixDirty = true;
	}

	public void LateUpdate()
	{
		_renderer.Update();
	}
}
