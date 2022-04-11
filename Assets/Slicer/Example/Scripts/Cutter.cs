using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cutter : MonoBehaviour
{
    SlicedMeshData_ positiveMesh, negativeMesh;
    List<Vector3> intersections;
    Plane_ plane;

    struct Plane_
    {
        public Vector3 normal;
        public Vector3 point;
        public Vector4 equation
        {
            get
            {
                float d = Vector3.Dot(normal, point);
                return new Vector4(normal.x, normal.y, normal.z, d);
            }
        }
        public Plane_ flipped
        {
            get
            {
                return new Plane_() { normal = -normal, point = point };
            }
        }

        public Vector3 DistanceToPoint(Vector3 p)
        {
            Vector3 v = p - point;
            return Vector3.Dot(normal, v) * normal;
        }

        public bool GetPointPlaneSide(Vector3 point)
        {
            Vector3 pointToPlane = DistanceToPoint(point);
            return Vector3.Dot(pointToPlane, normal) > 0;
        }

        public Vector3 GetPointPlaneIntersection(Vector3 rayOrigin, Vector3 rayDirection)
        {
            float t = Vector3.Dot(normal, (point - rayOrigin)) / Vector3.Dot(normal, rayDirection);
            return (rayOrigin + rayDirection * t);
        }
    }

    struct SlicedMeshData_
    {
        public List<Vector3> vertices;
        public List<Vector3> normals;
        public List<int> triangles;
        public bool isSolid;
        public bool trySmoothAndOptimizeMeshPerNormal;
        public float smoothAngle;
        public Vector3 center;
        public void initialize()
        {
            vertices = new List<Vector3>();
            normals = new List<Vector3>();
            triangles = new List<int>();
        }
    }

    public GameObject[] Cut(SliceableData sliceable, Vector3[] cuttingPlanesRotations)
    {
        GameObject originalObject = sliceable.gameObject;

        Mesh mesh = originalObject.GetComponent<MeshFilter>().mesh;
        List<SlicedMeshData_> toSliceMeshes = new List<SlicedMeshData_>();
        SlicedMeshData_ originalMesh = new SlicedMeshData_()
        {
            vertices = new List<Vector3>(mesh.vertices),
            normals = new List<Vector3>(mesh.normals),
            triangles = new List<int>(mesh.triangles),
            isSolid = sliceable.isSolid,
            trySmoothAndOptimizeMeshPerNormal = sliceable.trySmoothAndOptimizeMeshPerNormal,
            smoothAngle = sliceable.smoothAngle,
        };
        FindCenter(ref originalMesh, originalObject.transform.position);
        toSliceMeshes.Add(originalMesh);

        int iZero, iOne, iTwo;
        bool sideZero, sideOne, sideTwo;
        Vector3 vertexZero, vertexOne, vertexTwo;
        List<SlicedMeshData_> toSliceMeshesCopy;
        SlicedMeshData_ currentMesh;

        plane = new Plane_();
        for (int k = 0; k < cuttingPlanesRotations.Length; k ++)
        {
            plane.normal = Quaternion.Inverse(originalObject.transform.rotation) * Quaternion.Euler(cuttingPlanesRotations[k]) * Vector3.up;
            toSliceMeshesCopy = new List<SlicedMeshData_>(toSliceMeshes);
            toSliceMeshes.Clear();

            for (int j = 0; j < toSliceMeshesCopy.Count; j++)
            {
                plane.point = originalObject.transform.InverseTransformPoint(toSliceMeshesCopy[j].center);
                positiveMesh = new SlicedMeshData_();
                negativeMesh = new SlicedMeshData_();

                positiveMesh.initialize();
                negativeMesh.initialize();
                positiveMesh.isSolid = negativeMesh.isSolid = toSliceMeshesCopy[j].isSolid;
                positiveMesh.trySmoothAndOptimizeMeshPerNormal = negativeMesh.trySmoothAndOptimizeMeshPerNormal = toSliceMeshesCopy[j].trySmoothAndOptimizeMeshPerNormal;
                positiveMesh.smoothAngle = negativeMesh.smoothAngle = toSliceMeshesCopy[j].smoothAngle;

                currentMesh = toSliceMeshesCopy[j];
                intersections = new List<Vector3>();

                for (int i = 0; i < currentMesh.triangles.Count - 2; i += 3)
                {
                    iZero = currentMesh.triangles[i];
                    iOne = currentMesh.triangles[i + 1];
                    iTwo = currentMesh.triangles[i + 2];

                    vertexZero = currentMesh.vertices[iZero];
                    vertexOne = currentMesh.vertices[iOne];
                    vertexTwo = currentMesh.vertices[iTwo];

                    sideZero = plane.GetPointPlaneSide(vertexZero);
                    sideOne = plane.GetPointPlaneSide(vertexOne);
                    sideTwo = plane.GetPointPlaneSide(vertexTwo);

                    if (sideZero == sideOne && sideOne == sideTwo)
                    {
                        if (sideZero)
                        {
                            AddTriangle(new Vector3[] { vertexZero, vertexOne, vertexTwo }, ref positiveMesh);
                            continue;
                        }
                        
                        AddTriangle(new Vector3[] { vertexZero, vertexOne, vertexTwo }, ref negativeMesh);
                        continue;
                    }

                    if (TestSides(sideOne == sideTwo, sideZero, new Vector3[] { vertexZero, vertexOne, vertexTwo })) continue;
                    if (TestSides(sideTwo == sideZero, sideOne, new Vector3[] { vertexOne, vertexTwo, vertexZero })) continue;
                    if (TestSides(sideZero == sideOne, sideTwo, new Vector3[] { vertexTwo, vertexZero, vertexOne })) continue;
                }

                if (currentMesh.isSolid)
                {
                    if (intersections.Count > 2)
                    {
                        for (int i = 1; i < intersections.Count - 1; i ++)
                        {
                            AddTriangle(new Vector3[] { intersections[0], intersections[i], intersections[i + 1] }, ref negativeMesh);
                            AddTriangle(new Vector3[] { intersections[0], intersections[i], intersections[i + 1] }, ref positiveMesh);
                        }
                    }
                }

                if (positiveMesh.vertices.Count == 0 || negativeMesh.vertices.Count == 0)
                    continue;
                
                FindCenter(ref positiveMesh, originalObject.transform.position);
                FindCenter(ref negativeMesh, originalObject.transform.position);

                toSliceMeshes.Add(positiveMesh);
                toSliceMeshes.Add(negativeMesh);
            }
        }

        List<GameObject> slicedObjects = new List<GameObject>();
        for (int i = 0; i < toSliceMeshes.Count; i++)
            slicedObjects.Add(SetupNewObject(toSliceMeshes[i], originalObject));

        Destroy(originalObject);
        return slicedObjects.ToArray();
    }

    bool TestSides(bool joinedSides, bool soloSide, Vector3[] triangle)
    {
        if (!joinedSides) return false;

        Vector3 dir1 = triangle[0] - triangle[1];
        Vector3 dir2 = triangle[0] - triangle[2];

        Vector3 intersection1 = plane.GetPointPlaneIntersection(triangle[1], dir1);
        Vector3 intersection2 = plane.GetPointPlaneIntersection(triangle[2], dir2);

        intersections.AddRange(new Vector3[] {
            intersection1,
            intersection2,
        });

        intersections.AddRange(new Vector3[] {
            intersection1,
            intersection2,
        });

        if (soloSide)
        {
            AddTriangle(new Vector3[] { triangle[0], intersection1, intersection2 }, ref positiveMesh);
            AddTriangle(new Vector3[] { triangle[2], intersection2, triangle[1] }, ref negativeMesh);
            AddTriangle(new Vector3[] { triangle[1], intersection2, intersection1 }, ref negativeMesh);

            return true;
        }

        AddTriangle(new Vector3[] { triangle[0], intersection1, intersection2 }, ref negativeMesh);
        AddTriangle(new Vector3[] { intersection1, triangle[1], intersection2 }, ref positiveMesh);
        AddTriangle(new Vector3[] { intersection2, triangle[1], triangle[2] }, ref positiveMesh);

        return true;
    }

    void AddTriangle(Vector3[] triangles, ref SlicedMeshData_ slicedMeshData)
    {
        Vector3 dir1 = triangles[1] - triangles[0];
        Vector3 dir2 = triangles[2] - triangles[0];

        Vector3 normal = Vector3.Cross(dir1, dir2).normalized;
        if (normal == Vector3.zero) return;

        int[] idxVtx = new int[] { -1, -1, -1 };
        float cosTheta, theta;

        if (slicedMeshData.trySmoothAndOptimizeMeshPerNormal)
        {
            for (int i = 0; i < slicedMeshData.vertices.Count; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (slicedMeshData.vertices[i] == triangles[j])
                    {
                        if (normal == slicedMeshData.normals[i])
                        {
                            idxVtx[j] = i;
                            continue;
                        }

                        cosTheta = Vector3.Dot(slicedMeshData.normals[i], normal) / (slicedMeshData.normals[i].magnitude * normal.magnitude);
                        theta = Mathf.Acos(cosTheta) * Mathf.Rad2Deg;

                        if (theta < slicedMeshData.smoothAngle && theta != 0f)
                        {
                            idxVtx[j] = i;
                            slicedMeshData.normals[idxVtx[j]] += normal;
                        }
                    }
                }
            }
        }

        for (int i = 0; i < 3; i++)
        {
            if (idxVtx[i] == -1)
            {
                slicedMeshData.vertices.Add(triangles[i]);
                slicedMeshData.normals.Add(normal);
                idxVtx[i] = slicedMeshData.vertices.Count - 1;
            }
        }

        slicedMeshData.triangles.AddRange(idxVtx);
    }

    void FindCenter(ref SlicedMeshData_ slicedMeshData, Vector3 position)
    {
        Vector3 minBounds = slicedMeshData.vertices[0];
        Vector3 maxBounds = slicedMeshData.vertices[0];
        float x, y, z;

        for (int i = 1; i < slicedMeshData.vertices.Count; i++)
        {
            x = slicedMeshData.vertices[i].x;
            y = slicedMeshData.vertices[i].y;
            z = slicedMeshData.vertices[i].z;

            if (x < minBounds.x) minBounds.x = x;
            else if (x > maxBounds.x) maxBounds.x = x;

            if (y < minBounds.y) minBounds.y = y;
            else if (y > maxBounds.y) maxBounds.y = y;

            if (z < minBounds.z) minBounds.z = z;
            else if (z > maxBounds.z) maxBounds.z = z;
        }

        slicedMeshData.center = position + new Vector3((minBounds.x + maxBounds.x) / 2f, (minBounds.y + maxBounds.y) / 2f, (minBounds.z + maxBounds.z) / 2f);
    }
    
    GameObject SetupNewObject(SlicedMeshData_ mesh, GameObject originalGameObject)
    {
        GameObject newObject = new GameObject("NewObject");
        MeshFilter mf = newObject.AddComponent<MeshFilter>();
        MeshRenderer mr = newObject.AddComponent<MeshRenderer>();
        MeshCollider mc = newObject.AddComponent<MeshCollider>();
        mc.sharedMesh = mf.mesh;
        mc.convex = true;

        SliceableData sd = newObject.AddComponent<SliceableData>();
        SliceableData originalSD = originalGameObject.GetComponent<SliceableData>();
        sd.isSolid = originalSD.isSolid;
        sd.trySmoothAndOptimizeMeshPerNormal = originalSD.trySmoothAndOptimizeMeshPerNormal;
        sd.smoothAngle = originalSD.smoothAngle;


        Rigidbody rb = newObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        mr.materials = originalGameObject.GetComponent<MeshRenderer>().materials;
        mf.mesh.vertices = mesh.vertices.ToArray();
        mf.mesh.triangles = mesh.triangles.ToArray();
        mf.mesh.normals = mesh.normals.ToArray();
        // mf.mesh.Optimize();
        print("vertices: " + mesh.vertices.Count + " triangles: " + mesh.triangles.Count + " normals: " + mesh.normals.Count); 

        newObject.transform.rotation = originalGameObject.transform.rotation;
        newObject.transform.localScale = originalGameObject.transform.localScale;
        newObject.transform.position = originalGameObject.transform.position;
        newObject.transform.parent = originalGameObject.transform.parent;

        newObject.tag = originalGameObject.tag;
        newObject.layer = originalGameObject.layer;
        return newObject;
    }
}