struct InMeshData
{
    StructuredBuffer<float3> vertices;
    StructuredBuffer<float3> normals;
    StructuredBuffer<int> triangles;
};

struct OutMeshData
{
    AppendStructuredBuffer<float3> vertices;
    AppendStructuredBuffer<float3> normals;
    AppendStructuredBuffer<int> triangles;
};