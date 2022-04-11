using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlicerController : MonoBehaviour
{
    #region variables
    [SerializeField] ComputeShader slicerShader;
    [SerializeField] string kernelName = "Slicer";
    List<ComputeBuffer> buffersToDispose = new List<ComputeBuffer>();
    int threadGroupsX, threadGroupsY, threadGroupsZ;
    #endregion

    // test
    [SerializeField] GameObject testPlane, targetObject;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Transform[] planes = new Transform[1] { testPlane.transform };
            GameObject[] objects = new GameObject[1] { targetObject };

            Slice(planes, objects);
        }
    }

    #region methods
    void Slice(Transform[] planes, GameObject[] toSlice)
    {
        // find shader kernel
        int kernel = slicerShader.FindKernel(kernelName);

        // create data needed for the shader
        Mesh mesh = toSlice[0].GetComponent<MeshFilter>().mesh;
        
        // thread groups setup
        threadGroupsX = Mathf.CeilToInt(mesh.vertices.Length / 8.0f);
        threadGroupsY = 1;
        threadGroupsZ = 1;

        Plane[] planesArray = new Plane[planes.Length];
        Vector3 normal, somePoint;
        int bufferSize = 0;
        for (int i = 0; i < planes.Length; i++)
        {
            normal = Quaternion.Inverse(toSlice[0].transform.rotation) * planes[i].transform.rotation * Vector3.up;
            somePoint = toSlice[0].transform.InverseTransformPoint(planes[i].transform.position);
            planesArray[i] = new Plane() { normal = normal, somePoint = somePoint };
            bufferSize += planesArray[i].GetSize();
        }

        // create in buffers for the shader
        ComputeBuffer inMeshBufferVertices = new ComputeBuffer(mesh.vertexCount, sizeof(float) * 3);
        inMeshBufferVertices.SetData(mesh.vertices);

        ComputeBuffer inMeshBufferNormals = new ComputeBuffer(mesh.vertexCount, sizeof(float) * 3);
        inMeshBufferNormals.SetData(mesh.normals);
        
        ComputeBuffer inMeshBufferIndices = new ComputeBuffer(mesh.triangles.Length, sizeof(int));
        inMeshBufferIndices.SetData(mesh.triangles);

        ComputeBuffer inPlanesBuffer = new ComputeBuffer(planesArray.Length, bufferSize);
        inPlanesBuffer.SetData(planesArray);

        // create out buffers for the shader
        // positive
        ComputeBuffer outPositiveMeshBufferVertices = new ComputeBuffer(2048, sizeof(float) * 3, ComputeBufferType.Append);
        outPositiveMeshBufferVertices.SetCounterValue(0);

        ComputeBuffer outPositiveMeshBufferNormals = new ComputeBuffer(2048, sizeof(float) * 3, ComputeBufferType.Append);
        outPositiveMeshBufferNormals.SetCounterValue(0);

        ComputeBuffer outPositiveMeshBufferIndices = new ComputeBuffer(2048, sizeof(int), ComputeBufferType.Append);
        outPositiveMeshBufferIndices.SetCounterValue(0);

        // negative
        // ComputeBuffer outNegativeMeshBufferVertices = new ComputeBuffer(1, sizeof(float) * 3);
        // outNegativeMeshBufferVertices.SetData(new float[0]);

        // ComputeBuffer outNegativeMeshBufferNormals = new ComputeBuffer(1, sizeof(float) * 3);
        // outNegativeMeshBufferNormals.SetData(new float[0]);

        // ComputeBuffer outNegativeMeshBufferIndices = new ComputeBuffer(1, sizeof(int));
        // outNegativeMeshBufferIndices.SetData(new int[0]);
        
        // set buffers for the shader
        slicerShader.SetBuffer(kernel, "inMeshVertices", inMeshBufferVertices);
        slicerShader.SetBuffer(kernel, "inMeshNormals", inMeshBufferNormals);
        slicerShader.SetBuffer(kernel, "inMeshIndices", inMeshBufferIndices);
        slicerShader.SetBuffer(kernel, "outPositiveMeshVertices", outPositiveMeshBufferVertices);
        slicerShader.SetBuffer(kernel, "outPositiveMeshNormals", outPositiveMeshBufferNormals);
        slicerShader.SetBuffer(kernel, "outPositiveMeshIndices", outPositiveMeshBufferIndices);
        // slicerShader.SetBuffer(kernel, "outNegativeMeshVertices", outNegativeMeshBufferVertices);
        // slicerShader.SetBuffer(kernel, "outNegativeMeshNormals", outNegativeMeshBufferNormals);
        // slicerShader.SetBuffer(kernel, "outNegativeMeshIndices", outNegativeMeshBufferIndices);
        slicerShader.SetBuffer(kernel, "inPlanes", inPlanesBuffer);

        // dispose buffers
        buffersToDispose.Add(inMeshBufferVertices);
        buffersToDispose.Add(inMeshBufferNormals);
        buffersToDispose.Add(inMeshBufferIndices);
        buffersToDispose.Add(inPlanesBuffer);

        buffersToDispose.Add(outPositiveMeshBufferVertices);
        buffersToDispose.Add(outPositiveMeshBufferNormals);
        buffersToDispose.Add(outPositiveMeshBufferIndices);
        
        // buffersToDispose.Add(outNegativeMeshBufferVertices);
        // buffersToDispose.Add(outNegativeMeshBufferNormals);
        // buffersToDispose.Add(outNegativeMeshBufferIndices);

        // run the shader
        slicerShader.Dispatch(kernel, threadGroupsX, threadGroupsY, threadGroupsZ);

        // get the data from the shader
        // positive count of vertices and indices
        ComputeBuffer countPositiveVerticesAndNormals = new ComputeBuffer(1, sizeof(int), ComputeBufferType.IndirectArguments);
        ComputeBuffer countPositiveIndices = new ComputeBuffer(1, sizeof(int), ComputeBufferType.IndirectArguments);
        buffersToDispose.Add(countPositiveVerticesAndNormals);
        buffersToDispose.Add(countPositiveIndices);

        ComputeBuffer.CopyCount(outPositiveMeshBufferVertices, countPositiveVerticesAndNormals, 0);
        ComputeBuffer.CopyCount(outPositiveMeshBufferIndices, countPositiveIndices, 0);

        int[] countPositiveVerticesAndNormalsArray = new int[1] { 0 };
        int[] countPositiveIndicesArray = new int[1] { 0 };
        countPositiveVerticesAndNormals.GetData(countPositiveVerticesAndNormalsArray);
        countPositiveIndices.GetData(countPositiveIndicesArray);

        int positiveVerticesCount = countPositiveVerticesAndNormalsArray[0];
        int positiveIndicesCount = countPositiveIndicesArray[0];

        SlicedObject positiveSlicedObject = new SlicedObject();
        SlicedObject negativeSlicedObject = new SlicedObject();
        positiveSlicedObject.originalGameObject = negativeSlicedObject.originalGameObject = toSlice[0];
        positiveSlicedObject.vertices = new Vector3[positiveVerticesCount];
        positiveSlicedObject.normals = new Vector3[positiveVerticesCount];
        positiveSlicedObject.triangles = new int[positiveIndicesCount];
        print(sizeof(float) * 3 * positiveVerticesCount);

        outPositiveMeshBufferVertices.GetData(positiveSlicedObject.vertices);
        outPositiveMeshBufferNormals.GetData(positiveSlicedObject.normals);
        outPositiveMeshBufferIndices.GetData(positiveSlicedObject.triangles);

        // outNegativeMeshBufferVertices.GetData(negativeSlicedObject.vertices);
        // outNegativeMeshBufferNormals.GetData(negativeSlicedObject.normals);
        // outNegativeMeshBufferIndices.GetData(negativeSlicedObject.triangles);

        // create the meshes
        print("positive vertices count: " + positiveSlicedObject.vertices.Length);
        print("positive indices count: " + positiveSlicedObject.triangles.Length);
        print("original vertices count: " + mesh.vertexCount);
        print("original indices count: " + mesh.triangles.Length);
        SetupNewObject(positiveSlicedObject);
        // SetupNewObject(negativeSlicedObject);

        // iterate over disposable buffers
        foreach (ComputeBuffer buffer in buffersToDispose)
            buffer.Dispose();
    }

    void SetupNewObject(SlicedObject slicedObject)
    {
        GameObject newObject = new GameObject("NewObject");
        MeshFilter mf = newObject.AddComponent<MeshFilter>();
        MeshRenderer mr = newObject.AddComponent<MeshRenderer>();
        MeshCollider mc = newObject.AddComponent<MeshCollider>();
        mc.sharedMesh = mf.mesh;

        Rigidbody rb = newObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        mr.material = slicedObject.originalGameObject.GetComponent<MeshRenderer>().material;
        mf.mesh.vertices = slicedObject.vertices;
        mf.mesh.triangles = slicedObject.triangles;
        mf.mesh.normals = slicedObject.normals;
        mf.mesh.Optimize();

        newObject.transform.rotation = slicedObject.originalGameObject.transform.rotation;
        newObject.transform.localScale = slicedObject.originalGameObject.transform.localScale;
        newObject.transform.position = slicedObject.originalGameObject.transform.position;
        newObject.tag = slicedObject.originalGameObject.tag;
        newObject.layer = slicedObject.originalGameObject.layer;
    }
    #endregion

    #region structs
    struct SlicedObject
    {
        public Vector3[] vertices;
        public Vector3[] normals;
        public int[] triangles;
        public GameObject originalGameObject;
    }

    struct Plane
    {
        public Vector3 normal;
        public Vector3 somePoint;
        public Vector3 equation;
        public int GetSize()
        {
            return (sizeof(float) * 9);
        }
    }
    #endregion
}
