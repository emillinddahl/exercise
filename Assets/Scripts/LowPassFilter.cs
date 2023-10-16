using System;
using UnityEngine;

public class LowPassFilter
{
    private float alpha;
    private Vector3 y;
    private Vector3 s;

    public LowPassFilter(float alpha)
    {
        SetAlpha(alpha);
        y = s = Vector3.zero;
    }

    private void SetAlpha(float alpha)
    {
        if (alpha <= 0 || alpha > 1.0)
        {
            throw new ArgumentException("alpha (" + alpha + ") should be in (0.0, 1.0]");
        }

        this.alpha = alpha;
    }

    public Vector3 Filter(Vector3 value, float timestamp = 0, float? alpha = null)
    {
        if (alpha != null)
        {
            SetAlpha(alpha.Value);
        }

        Vector3 result;
        if (y == Vector3.zero)
        {
            result = value;
        }
        else
        {
            result = alpha.Value * value + (1.0f - alpha.Value) * s;
        }

        y = value;
        s = result;

        return result;
    }

    public Vector3 LastValue()
    {
        return y;
    }

    public Vector3 LastFilteredValue()
    {
        return s;
    }
}

public class OneEuroFilter
{
    private float freq;
    private float minCutoff;
    private float beta;
    private float dcutoff;
    private LowPassFilter[] x;
    private LowPassFilter[] dx;
    private float? lastTime;

    public OneEuroFilter(float freq, float minCutoff = 1.0f, float beta = 0.0f, float dcutoff = 1.0f)
    {
        if (freq <= 0)
        {
            throw new ArgumentException("freq should be >0");
        }

        if (minCutoff <= 0)
        {
            throw new ArgumentException("mincutoff should be >0");
        }

        if (dcutoff <= 0)
        {
            throw new ArgumentException("dcutoff should be >0");
        }

        this.freq = freq;
        this.minCutoff = minCutoff;
        this.beta = beta;
        this.dcutoff = dcutoff;
        this.x = new LowPassFilter[3];
        this.dx = new LowPassFilter[3];

        for (int i = 0; i < 3; i++)
        {
            this.x[i] = new LowPassFilter(Alpha(this.minCutoff));
            this.dx[i] = new LowPassFilter(Alpha(this.dcutoff));
        }

        lastTime = null;
    }

    private float Alpha(float cutoff)
    {
        float te = 1.0f / freq;
        float tau = 1.0f / (2 * Mathf.PI * cutoff);
        return 1.0f / (1.0f + tau / te);
    }

    public Vector3 Filter(Vector3 input, float timestamp = 0)
    {
        if (lastTime != null && timestamp != 0)
        {
            freq = 1.0f / (timestamp - lastTime.Value);
        }

        lastTime = timestamp;

        Vector3 result = Vector3.zero;
        for (int i = 0; i < 3; i++)
        {
            float prevX = x[i].LastFilteredValue()[i];
            float dxComponent = (prevX == 0.0f) ? 0.0f : (input[i] - prevX) * freq;
            Vector3 dxVector = Vector3.zero;
            dxVector[i] = dxComponent;

            float edx = this.dx[i].Filter(dxVector, timestamp, Alpha(dcutoff)).x;
            float cutoff = minCutoff + beta * Mathf.Abs(edx);

            result[i] = x[i].Filter(new Vector3(input[i], 0, 0), timestamp, Alpha(cutoff)).x;

        }
        return new Vector3(result[0], result[1], result[2]);
    }
}
