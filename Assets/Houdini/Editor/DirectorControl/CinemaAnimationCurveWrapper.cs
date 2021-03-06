using UnityEditor;
using UnityEngine;

public class CinemaAnimationCurveWrapper
{
	public int Id;

	public string Label;

	public Color Color;

	public bool IsVisible = true;
    public bool onlySelf = false;
	public AnimationCurve curve;

	private CinemaKeyframeWrapper[] KeyframeControls = new CinemaKeyframeWrapper[0];

	public AnimationCurve Curve
	{
		get
		{
			return curve;
		}
		set
		{
			curve = value;
			initializeKeyframeWrappers();
		}
	}

	public int KeyframeCount
	{
		get
		{
			return curve.length;
		}
	}

	public Keyframe GetKeyframe(int index)
	{
		return curve[index];
	}

	public void SetKeyframeScreenPosition(int index, Vector2 screenPosition)
	{
		this.GetKeyframeWrapper(index).ScreenPosition = screenPosition;
	}

	public Vector2 GetKeyframeScreenPosition(int index)
	{
		return GetKeyframeWrapper(index).ScreenPosition;
	}

	public void SetOutTangentScreenPosition(int index, Vector2 screenPosition)
	{
		this.GetKeyframeWrapper(index).OutTangentControlPointPosition = screenPosition;
	}

	public Vector2 GetOutTangentScreenPosition(int index)
	{
		return this.GetKeyframeWrapper(index).OutTangentControlPointPosition;
	}

	public void SetInTangentScreenPosition(int index, Vector2 screenPosition)
	{
		GetKeyframeWrapper(index).InTangentControlPointPosition = screenPosition;
	}

	public Vector2 GetInTangentScreenPosition(int index)
	{
		return GetKeyframeWrapper(index).InTangentControlPointPosition;
	}

	public void initializeKeyframeWrappers()
	{
		KeyframeControls = new CinemaKeyframeWrapper[curve.length];
		for (int i = 0; i < curve.length; i++)
		{
			KeyframeControls[i] = new CinemaKeyframeWrapper(this, curve[i]);
		}
	}
    
    public bool HasKeyframe(AnimationKeyTime time)
    {
        return GetKeyframeIndex(time) != -1;
    }

    public int GetKeyframeIndex(AnimationKeyTime time)
    {
        for (int i = 0; i < curve.length; i++)
        {
            if (time.ContainsTime(curve[i].time))
                return i;
        }
        return -1;
    }

	public void AddKey(float time, float value, int tangentMode)
	{
	    Keyframe keyframe = default;
	    Keyframe keyframe2 = default;
	    AnimationKeyTime akt = AnimationKeyTime.Time(time, DirectorWindow.directorControl.frameRate);
	    time = (float)akt.m_Frame / DirectorWindow.directorControl.frameRate;
        int num = GetKeyframeIndex(akt);
	    if (num == -1)
	    {
	        for (int i = 0; i < curve.length - 1; i++)
	        {
	            Keyframe keyframe3 = curve[i];
	            Keyframe keyframe4 = curve[i + 1];
	            if (keyframe3.time < time && time < keyframe4.time)
	            {
	                keyframe = keyframe3;
	                keyframe2 = keyframe4;
	            }
	        }
	        Keyframe keyframe5 = new Keyframe(time, value);
	        num = curve.AddKey(keyframe5);
	        ArrayUtility.Insert(ref KeyframeControls, num, new CinemaKeyframeWrapper(this, keyframe5));
        }
	    else
	    {
	        Keyframe keyframe5 = curve[num];
	        keyframe5.value = value;
            if (num > 0)
	            keyframe = curve[num - 1];
            if (num < curve.length - 1)
	            keyframe2 = curve[num + 1];
	    }
		if (IsAuto(num))
		{
			SmoothTangents(num, 0f);
		}
		if (IsBroken(num))
		{
			if (IsLeftLinear(num))
			{
				SetKeyLeftLinear(num);
			}
			if (IsRightLinear(num))
			{
				SetKeyRightLinear(num);
			}
			if (IsLeftConstant(num))
			{
				SetKeyLeftConstant(num);
			}
			if (IsRightConstant(num))
			{
				SetKeyRightConstant(num);
			}
		}
		if (num > 0)
		{
			curve.MoveKey(num - 1, keyframe);
		    ArrayUtility.Insert(ref KeyframeControls, num, new CinemaKeyframeWrapper(this, keyframe));
            if (IsAuto(num - 1))
			{
				SmoothTangents(num - 1, 0f);
			}
			if (IsBroken(num - 1) && IsRightLinear(num - 1))
			{
				SetKeyRightLinear(num - 1);
			}
		}
		if (num < curve.length - 1)
		{
			curve.MoveKey(num + 1, keyframe2);
		    ArrayUtility.Insert(ref KeyframeControls, num, new CinemaKeyframeWrapper(this, keyframe2));
            if (IsAuto(num + 1))
			{
				SmoothTangents(num + 1, 0f);
			}
			if (IsBroken(num + 1) && IsLeftLinear(num + 1))
			{
				SetKeyLeftLinear(num + 1);
			}
		}
	}

	public void RemoveKey(int id)
	{
		ArrayUtility.RemoveAt(ref KeyframeControls, id);
		curve.RemoveKey(id);
		if (id > 0)
		{
			if (IsAuto(id - 1))
			{
				SmoothTangents(id - 1, 0f);
			}
			if (IsBroken(id - 1) && IsRightLinear(id - 1))
			{
				SetKeyRightLinear(id - 1);
			}
		}
		if (id < curve.length)
		{
			if (IsAuto(id))
			{
				SmoothTangents(id, 0f);
			}
			if (IsBroken(id) && IsLeftLinear(id))
			{
				SetKeyLeftLinear(id);
			}
		}
	}

	public float Evaluate(float time)
	{
		return curve.Evaluate(time);
	}

	public void SmoothTangents(int index, float weight)
	{
		curve.SmoothTangents(index, weight);
	}

	public int MoveKey(int index, Keyframe kf)
	{
        int num = -1;
        if (index == 0 && curve.keys.Length > 1)
        {
            Keyframe key1 = curve.keys[1];
            if (key1.time < kf.time)
            {
                index = 1;
                curve.RemoveKey(0);
                num = curve.AddKey(kf);
            }
            else
            {
                num = curve.MoveKey(index, kf);
            }
        }
        else if (index == curve.keys.Length - 1)
        {
            Keyframe key1 = curve.keys[index - 1];
            if (key1.time > kf.time)
            {
                index = index - 1;
                curve.RemoveKey(index);
                num = curve.AddKey(kf);
            }
            else
            {
                num = curve.MoveKey(index, kf);
            }
        }
        else
        {
            num = curve.MoveKey(index, kf);
        }
        if (num < 0)
            return num;
		CinemaKeyframeWrapper cinemaKeyframeWrapper = KeyframeControls[index];
		CinemaKeyframeWrapper cinemaKeyframeWrapper2 = KeyframeControls[num];
		KeyframeControls[index] = cinemaKeyframeWrapper2;
		KeyframeControls[num] = cinemaKeyframeWrapper;
		if (IsAuto(num))
		{
			SmoothTangents(num, 0f);
		}
		if (IsBroken(num))
		{
			if (this.IsLeftLinear(num))
			{
				this.SetKeyLeftLinear(num);
			}
			if (this.IsRightLinear(num))
			{
				this.SetKeyRightLinear(num);
			}
		}
		if (index > 0)
		{
			if (this.IsAuto(index - 1))
			{
				this.SmoothTangents(index - 1, 0f);
			}
			if (this.IsBroken(index - 1) && this.IsRightLinear(index - 1))
			{
				this.SetKeyRightLinear(index - 1);
			}
		}
		if (index < this.curve.length - 1)
		{
			if (this.IsAuto(index + 1))
			{
				this.SmoothTangents(index + 1, 0f);
			}
			if (this.IsBroken(index + 1) && this.IsLeftLinear(index + 1))
			{
				this.SetKeyLeftLinear(index + 1);
			}
		}
		return num;
	}

	public CinemaKeyframeWrapper GetKeyframeWrapper(int i)
	{
		return KeyframeControls[i];
	}

	internal void RemoveAtTime(float time)
	{
		int num = -1;
		for (int i = 0; i < curve.length; i++)
		{
			if (curve.keys[i].time == time)
			{
				num = i;
			}
		}
		if (num >= 0)
		{
			ArrayUtility.RemoveAt(ref KeyframeControls, num);
			curve.RemoveKey(num);
			if (num > 0)
			{
				if (IsAuto(num - 1))
				{
					SmoothTangents(num - 1, 0f);
				}
				if (IsBroken(num - 1) && IsRightLinear(num - 1))
				{
					SetKeyRightLinear(num - 1);
				}
			}
			if (num < curve.length)
			{
				if (IsAuto(num))
				{
					SmoothTangents(num, 0f);
				}
				if (IsBroken(num) && IsLeftLinear(num))
				{
					SetKeyLeftLinear(num);
				}
			}
		}
	}

    internal void PauseAtTime(float srcTime, float destTime)
    {
        AnimationKeyTime srcKeyTime = AnimationKeyTime.Time(srcTime, DirectorWindow.directorControl.frameRate);
        AnimationKeyTime destKeyTime = AnimationKeyTime.Time(destTime, DirectorWindow.directorControl.frameRate);
        int srcNum = GetKeyframeIndex(srcKeyTime);
        int destNum = GetKeyframeIndex(destKeyTime);
        if (srcNum >= 0 && destNum >= 0)
        {
            Keyframe srcKeyframe = curve.keys[srcNum];
            RemoveAtTime(destTime);
            AddKey(destTime, srcKeyframe.value, DirectorWindow.directorControl.DefaultTangentMode);
        }
    }

	internal void FlattenKey(int index)
	{
		Keyframe keyframe = new Keyframe(curve[index].time, curve[index].value, 0f, 0f);
		curve.MoveKey(index, keyframe);
	}

	internal void SetKeyLeftLinear(int index)
	{
		Keyframe keyframe = curve[index];
		float num = keyframe.inTangent;
		if (index > 0)
		{
			Keyframe keyframe2 = curve[index-1];
			num = (keyframe.value - keyframe2.value) / (keyframe.time - keyframe2.time);
		}
		Keyframe keyframe3 = new Keyframe(keyframe.time, keyframe.value, num, keyframe.outTangent);
		curve.MoveKey(index, keyframe3);
	}

	internal void SetKeyRightLinear(int index)
	{
		Keyframe keyframe = this.curve[index];
		float num = keyframe.outTangent;
		if (index < this.curve.length - 1)
		{
			Keyframe keyframe2 = this.curve[index+1];
			num = (keyframe2.value - keyframe.value) / (keyframe2.time - keyframe.time);
		}
		Keyframe keyframe3 = new Keyframe(keyframe.time, keyframe.value, keyframe.inTangent, num);
		curve.MoveKey(index, keyframe3);
	}

	internal void SetKeyLeftConstant(int index)
	{
		Keyframe keyframe = curve[index];
		Keyframe keyframe2 = new Keyframe(keyframe.time, keyframe.value, float.PositiveInfinity, keyframe.outTangent);
		curve.MoveKey(index, keyframe2);
	}

	internal void SetKeyRightConstant(int index)
	{
		Keyframe keyframe = this.curve[index];
		Keyframe keyframe2 = new Keyframe(keyframe.time, keyframe.value, keyframe.inTangent, float.PositiveInfinity);
		curve.MoveKey(index, keyframe2);
	}

	internal void SetKeyLeftFree(int index)
	{
		Keyframe keyframe = this.curve[index];
		Keyframe keyframe2 = new Keyframe(keyframe.time, keyframe.value, keyframe.inTangent, keyframe.outTangent);
		this.curve.MoveKey(index, keyframe2);
	}

	internal void SetKeyRightFree(int index)
	{
		Keyframe keyframe = this.curve[index];
		Keyframe keyframe2 = new Keyframe(keyframe.time, keyframe.value, keyframe.inTangent, keyframe.outTangent);
		this.curve.MoveKey(index, keyframe2);
	}

	internal void SetKeyBroken(int index)
	{
		Keyframe keyframe = this.curve[index];
		Keyframe keyframe2 = new Keyframe(keyframe.time, keyframe.value, keyframe.inTangent, keyframe.outTangent);
		this.curve.MoveKey(index, keyframe2);
	}

	internal void SetKeyAuto(int index)
	{
		Keyframe keyframe = this.curve[index];
		Keyframe keyframe2 = new Keyframe(keyframe.time, keyframe.value, keyframe.inTangent, keyframe.outTangent);
		this.curve.MoveKey(index, keyframe2);
		this.curve.SmoothTangents(index, 0f);
	}

	internal void SetKeyFreeSmooth(int index)
	{
		Keyframe keyframe = curve[index];
		Keyframe keyframe2 = new Keyframe(keyframe.time, keyframe.value, keyframe.inTangent, keyframe.outTangent);
		this.curve.MoveKey(index, keyframe2);
	}

	internal bool IsAuto(int index)
	{
        return AnimationUtility.GetKeyLeftTangentMode(curve, index) == AnimationUtility.TangentMode.Auto;
	}

	internal bool IsFreeSmooth(int index)
	{
        return AnimationUtility.GetKeyLeftTangentMode(curve, index) == AnimationUtility.TangentMode.Free;
	}

	internal bool IsBroken(int index)
	{
        return AnimationUtility.GetKeyLeftTangentMode(curve, index) == AnimationUtility.TangentMode.Free;
    }

	internal bool IsLeftFree(int index)
	{
        return AnimationUtility.GetKeyLeftTangentMode(curve, index) == AnimationUtility.TangentMode.Free;
	}

	internal bool IsLeftLinear(int index)
	{
        return AnimationUtility.GetKeyLeftTangentMode(curve, index) == AnimationUtility.TangentMode.Linear;
	}

	internal bool IsLeftConstant(int index)
	{
        return AnimationUtility.GetKeyLeftTangentMode(curve, index) == AnimationUtility.TangentMode.Constant;
	}

	internal bool IsRightFree(int index)
	{
        return AnimationUtility.GetKeyRightTangentMode(curve, index) == AnimationUtility.TangentMode.Free;
	}

	internal bool IsRightLinear(int index)
	{
        return AnimationUtility.GetKeyRightTangentMode(curve, index) == AnimationUtility.TangentMode.Linear;
	}

	internal bool IsRightConstant(int index)
	{
        return AnimationUtility.GetKeyRightTangentMode(curve, index) == AnimationUtility.TangentMode.Constant;
	}

	internal void CollapseEnd(float oldEndTime, float newEndTime)
	{
		for (int i = curve.length - 2; i > 0; i--)
		{
			if (curve[i].time >= newEndTime && curve[i].time <= oldEndTime)
			{
				RemoveKey(i);
			}
		}
		if (newEndTime > GetKeyframe(0).time)
		{
			Keyframe keyframe = GetKeyframe(KeyframeCount - 1);
			Keyframe kf = new Keyframe(newEndTime, keyframe.value, keyframe.inTangent, keyframe.outTangent);
			MoveKey(KeyframeCount - 1, kf);
		}
	}

	internal void CollapseStart(float oldStartTime, float newStartTime)
	{
		for (int i = curve.length - 2; i > 0; i--)
		{
			if (curve[i].time >= oldStartTime && curve[i].time <= newStartTime)
			{
				RemoveKey(i);
			}
		}
		if (newStartTime < GetKeyframe(curve.length - 1).time)
		{
			Keyframe keyframe = GetKeyframe(0);
			Keyframe kf = new Keyframe(newStartTime, keyframe.value, keyframe.inTangent, keyframe.outTangent);
            AnimationUtility.SetKeyLeftTangentMode(curve, 0, AnimationUtility.GetKeyLeftTangentMode(curve, 0));
			MoveKey(0, kf);
		}
	}

	internal void ScaleStart(float oldFiretime, float oldDuration, float newFiretime)
	{
		float num = (oldFiretime + oldDuration - newFiretime) / oldDuration;
		for (int i = curve.length - 1; i >= 0; i--)
		{
			Keyframe keyframe = GetKeyframe(i);
			float num2 = (keyframe.time - oldFiretime) * num + newFiretime;
			Keyframe kf = new Keyframe(num2, keyframe.value, keyframe.inTangent, keyframe.outTangent);
            AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.GetKeyLeftTangentMode(curve, i));
            MoveKey(i, kf);
		}
	}

	internal void ScaleEnd(float oldDuration, float newDuration)
	{
		float num = newDuration / oldDuration;
		float time = curve[0].time;
		for (int i = 1; i < curve.length; i++)
		{
			Keyframe keyframe = GetKeyframe(i);
			float num2 = (keyframe.time - time) * num + time;
			Keyframe kf = new Keyframe(num2, keyframe.value, keyframe.inTangent, keyframe.outTangent);
            MoveKey(i, kf);
		}
	}

	internal void Flip()
	{
		if (curve.length < 2)
		{
			return;
		}
		AnimationCurve animationCurve = new AnimationCurve();
		float time = curve[0].time;
		float time2 = curve[curve.length - 1].time;
		for (int i = 0; i < curve.length; i++)
		{
			Keyframe keyframe = GetKeyframe(i);
			float num = time2 - keyframe.time + time;
			Keyframe keyframe2 = new Keyframe(num, keyframe.value, keyframe.inTangent, keyframe.outTangent);
            animationCurve.AddKey(keyframe2);
		}
		Curve = animationCurve;
	}
}
