using UnityEngine;
using System.Collections.Generic;

public class Filter
{
    private double alpha;
    private double beta;
    private double minCutoff;
    private double derivateCutoff;
    private System.Numerics.Vector3 lastValue;
    private System.Numerics.Vector3[] filteredValues;
    private double timestamp;

    public Filter(double minCutoff, double derivateCutoff)
    {
        this.minCutoff = minCutoff;
        this.derivateCutoff = derivateCutoff;
        this.lastValue = System.Numerics.Vector3.Zero;
        this.filteredValues = new System.Numerics.Vector3[32];
      //  this.timestamp = Time.time;
        this.alpha = -1.0;
        this.beta = -1.0;
    }
    
    public void InitializeTimestamp()
    {
        this.timestamp = Time.time;
    }
    

    public System.Numerics.Vector3[] DoFilter(System.Numerics.Vector3[] values)
    {
        double currTimestamp = Time.time;
        double deltaTime = currTimestamp - timestamp;

        if (alpha < 0.0 || beta < 0.0)
        {
            double derivateCutoffFrequency = 1.0 / (2.0 * Mathf.PI * derivateCutoff);
            alpha = 1.0 / (1.0 + derivateCutoffFrequency * deltaTime);
            double minCutoffFrequency = 1.0 / (2.0 * Mathf.PI * minCutoff);
            beta = 1.0 / (1.0 + minCutoffFrequency * deltaTime);
        }

        // Compute derivatives for each dimension
        System.Numerics.Vector3[] derivatives = new System.Numerics.Vector3[32];
        for (int i = 0; i < 32; i++)
        {
            derivatives[i] = (values[i] - lastValue) / (float)deltaTime;
        }

        // Update the filtered values for each dimension
        for (int i = 0; i < 32; i++)
        {
            filteredValues[i] = (float)alpha * values[i] + (float)(1.0 - alpha) * filteredValues[i];
        }

        // Update timestamp and last values
        timestamp = currTimestamp;
        lastValue = values[0];

        return filteredValues;
    }
}