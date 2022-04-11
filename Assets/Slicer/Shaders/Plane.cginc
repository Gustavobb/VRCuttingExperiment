struct Plane
{
    float3 normal;
    float3 somePoint;
    float4 equation;
    
    float4 getEquation()
    {
        return float4(normal.x, normal.y, normal.z, dot(normal, somePoint));
    }

    void flip()
    {
        normal = -normal;
        equation.w = -equation.w;
    }

    float3 distanceToPoint(float3 _point)
    {
        return normal * dot(normal, _point - somePoint);
    }

    bool isPointInPlane(float3 _point)
    {
        return dot(normal, _point - somePoint) == 0;
    }

    bool isNormalFacingPoint(float3 _point)
    {
        return dot(distanceToPoint(_point), normal) > 0;
    }

    void set(float3 normal, float3 somePoint)
    {
        normal = normal;
        somePoint = somePoint;
        equation = getEquation();
    }
};