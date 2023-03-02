float2 ComplexMult(float2 a, float2 b)
{
    return float2(a.x * b.x - a.y * b.y, a.x * b.y + a.y * b.x);
}

float2 EulerExp(float theta) // i * 0
{
    return float2(cos(theta), sin(theta));
}

float2 ComplexExp(float2 a)
{
    return float2(cos(a.y), sin(a.y)) * exp(a.x);
}
