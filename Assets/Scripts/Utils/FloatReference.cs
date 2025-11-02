using System;

[Serializable]
public class FloatReference
{
    public bool UseConstant = true;
    public float ConstantValue;
    public FloatVariable Variable;

    public float FloatRef
    {
        get { return UseConstant ? ConstantValue : Variable.Variable; }
    }
}