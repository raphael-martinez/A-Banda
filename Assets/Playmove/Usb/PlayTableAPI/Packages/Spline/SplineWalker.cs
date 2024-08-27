using System;
using UnityEngine;
using System.Collections;

public enum SplineWalkerMode
{
	Once,
	Loop,
	PingPong
}

[Flags]
public enum Axis
{
	X = (1 << 0),
	Y = (1 << 1),
	Z = (1 << 2)
}

public class SplineWalker : MonoBehaviour
{

	public BezierSpline Spline;
	public bool StartStopped;
	public bool LookForward;
	public float Duration;
    public SplineWalkerMode Mode;

	public Axis FreezeAxis;

    public float Progress { get { return _progress; } }

	private bool _goingForward = true;
	private bool _pauseSpline;
	private float _progress;

	void Start()
	{
	    transform.position = Spline.GetPoint(0);
        PauseSpline(StartStopped);
	}

	void Update()
	{
		if (_pauseSpline)
			return;

		if (_goingForward)
		{
			_progress += Time.deltaTime / Duration;

			if (_progress > 1f)
			{
			    switch (Mode)
			    {
			        case SplineWalkerMode.Once:
                        _progress = 1f;
			        break;

                    case SplineWalkerMode.Loop:
                        _progress = 0f;
                    break;

                    case SplineWalkerMode.PingPong:
                        _progress = 1;
					    _goingForward = false;
                    break;
			    }
			}
		}
		else
		{
			_progress -= Time.deltaTime / Duration;

			if (_progress < 0f)
			{
				_progress = 0;
                
                if(Mode == SplineWalkerMode.PingPong)
				    _goingForward = true;
                else
                    PauseSpline(true);
			}
		}

	    WalkByProgress();
	}

    private void WalkByProgress()
    {
        Vector3 position = Spline.GetPoint(_progress);

        if ((FreezeAxis & Axis.X) == Axis.X)
            position.x = transform.position.x;
        if ((FreezeAxis & Axis.Y) == Axis.Y)
            position.y = transform.position.y;
        if ((FreezeAxis & Axis.Z) == Axis.Z)
            position.z = transform.position.z;

        transform.position = position;

        if (LookForward)
        {
            transform.LookAt(position + Spline.GetDirection(_progress));
        }
    }

	public void PauseSpline(bool pauseOrPlay)
	{
		_pauseSpline = pauseOrPlay;
	}

    public void SetProgress(float value)
    {
        _progress = value;
        WalkByProgress();
    }
}
