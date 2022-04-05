using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutTest : MonoBehaviour
{
    Mesh mesh;
    [SerializeField] GameObject pPos, go;

    struct Plane
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
        public Plane flipped
        {
            get
            {
                return new Plane() { normal = -normal, point = point };
            }
        }

        public Vector3 DistanceToPoint(Vector3 p)
        {
            // Returns a signed distance from plane to point.
            // The value returned is positive if the point is on the side of the plane into which the plane's normal is facing, and negative otherwise.
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

    void Start()
    {
        // CreateTestMesh();
    }

    void CreateTestMesh()
    {
        go = new GameObject("TestMesh");
        MeshFilter mf = go.AddComponent<MeshFilter>();
        MeshRenderer mr = go.AddComponent<MeshRenderer>();
        mesh = new Mesh();
        mf.mesh = mesh;
        mr.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mesh.vertices = new Vector3[] {
            new Vector3(0, 0, 0),
            new Vector3(0, 1, 0),
            new Vector3(1, 1, 0),
            new Vector3(1, 0, 0),
        };
        mesh.triangles = new int[] {
            0, 1, 2,
            0, 2, 3,
        };
        mesh.RecalculateNormals();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Cut();
        }
    }

    void Cut()
    {
        mesh = go.GetComponent<MeshFilter>().mesh;

        Plane plane = new Plane();
        plane.normal = Quaternion.Inverse(go.transform.rotation) * pPos.transform.rotation * Vector3.up;
        plane.point = go.transform.InverseTransformPoint(pPos.transform.position);

        GameObject positiveCutSide = new GameObject("PositiveCutSide");
        GameObject negativeCutSize = new GameObject("NegativeCutSize");
        positiveCutSide.AddComponent<MeshFilter>();
        negativeCutSize.AddComponent<MeshFilter>();
        positiveCutSide.AddComponent<MeshRenderer>();
        negativeCutSize.AddComponent<MeshRenderer>();
        positiveCutSide.GetComponent<MeshRenderer>().material = negativeCutSize.GetComponent<MeshRenderer>().material = go.GetComponent<MeshRenderer>().material;

        List<Vector3> newPositiveVertices = new List<Vector3>();
        List<Vector3> newNegativeVertices = new List<Vector3>();
        List<int> newPositiveTriangles = new List<int>();
        List<int> newNegativeTriangles = new List<int>();
        

        bool[] side = new bool[mesh.vertices.Length];
        for (int i = 0; i < mesh.vertices.Length; i++)
        {
            side[i] = plane.GetPointPlaneSide(mesh.vertices[i]);
        }

        for (int i = 0; i < mesh.triangles.Length; i += 3)
        {
            int iZero = mesh.triangles[i];
            int iOne = mesh.triangles[i + 1];
            int iTwo = mesh.triangles[i + 2];

            print(iZero + " " + iOne + " " + iTwo);

            bool eq = side[iZero] == side[iOne] && side[iOne] == side[iTwo];
            if (eq)
            {
                if (side[iZero])
                {
                    newPositiveVertices.AddRange(new Vector3[] {
                        mesh.vertices[iZero],
                        mesh.vertices[iOne],
                        mesh.vertices[iTwo],
                    });
                    newPositiveTriangles.AddRange(new int[] {
                        newPositiveVertices.Count - 3,
                        newPositiveVertices.Count - 2,
                        newPositiveVertices.Count - 1,
                    });
                }

                else
                {
                    newNegativeVertices.AddRange(new Vector3[] {
                        mesh.vertices[iZero],
                        mesh.vertices[iOne],
                        mesh.vertices[iTwo],
                    });
                    newNegativeTriangles.AddRange(new int[] {
                        newNegativeVertices.Count - 3,
                        newNegativeVertices.Count - 2,
                        newNegativeVertices.Count - 1,
                    });
                }

                continue;
            }

            TestSides(side[iOne] == side[iTwo], side[iZero], i, mesh.vertices[iZero], mesh.vertices[iOne], mesh.vertices[iTwo], ref newPositiveVertices, ref newNegativeVertices, ref newPositiveTriangles, ref newNegativeTriangles, plane);
            TestSides(side[iTwo] == side[iZero], side[iOne], i, mesh.vertices[iOne], mesh.vertices[iTwo], mesh.vertices[iZero], ref newPositiveVertices, ref newNegativeVertices, ref newPositiveTriangles, ref newNegativeTriangles, plane);
            TestSides(side[iZero] == side[iOne], side[iTwo], i, mesh.vertices[iTwo], mesh.vertices[iZero], mesh.vertices[iOne], ref newPositiveVertices, ref newNegativeVertices, ref newPositiveTriangles, ref newNegativeTriangles, plane);
        }

        positiveCutSide.GetComponent<MeshFilter>().mesh.vertices = newPositiveVertices.ToArray();
        positiveCutSide.GetComponent<MeshFilter>().mesh.triangles = newPositiveTriangles.ToArray();
        positiveCutSide.GetComponent<MeshFilter>().mesh.RecalculateNormals();

        negativeCutSize.GetComponent<MeshFilter>().mesh.vertices = newNegativeVertices.ToArray();
        negativeCutSize.GetComponent<MeshFilter>().mesh.triangles = newNegativeTriangles.ToArray();
        negativeCutSize.GetComponent<MeshFilter>().mesh.RecalculateNormals();

        positiveCutSide.transform.position = negativeCutSize.transform.position = go.transform.position;
        positiveCutSide.transform.rotation = negativeCutSize.transform.rotation = go.transform.rotation;
        positiveCutSide.transform.localScale = negativeCutSize.transform.localScale = go.transform.localScale;
        Destroy(go);
    }

    void TestSides(bool joinedSides, bool soloSide, int i, Vector3 vtx1, Vector3 vtx2, Vector3 vtx3, ref List<Vector3> positiveVertices, ref List<Vector3> negativeVertices, ref List<int> positiveTriangles, ref List<int> negativeTriangles, Plane plane)
    {
        if (!joinedSides) return;

        Vector3 dir1 = vtx1 - vtx2;
        Vector3 dir2 = vtx1 - vtx3;

        Vector3 intersection1 = plane.GetPointPlaneIntersection(vtx2, dir1);
        Vector3 intersection2 = plane.GetPointPlaneIntersection(vtx3, dir2);

        if (soloSide)
        {
            positiveVertices.AddRange(new Vector3[] {
                vtx1,
                intersection1,
                intersection2,
            });

            positiveTriangles.AddRange(new int[] {
                positiveVertices.Count - 3,
                positiveVertices.Count - 2,
                positiveVertices.Count - 1,
            });

            negativeVertices.AddRange(new Vector3[] {
                vtx3,
                intersection1,
                vtx2,
                vtx3,
                intersection2,
                intersection1,
            });

            negativeTriangles.AddRange(new int[] {
                negativeVertices.Count - 6,
                negativeVertices.Count - 5,
                negativeVertices.Count - 4,
                negativeVertices.Count - 3,
                negativeVertices.Count - 2,
                negativeVertices.Count - 1,
            });

            return;
        }

        negativeVertices.AddRange(new Vector3[] {
            vtx1,
            intersection1,
            intersection2,
        });

        negativeTriangles.AddRange(new int[] {
            negativeVertices.Count - 3,
            negativeVertices.Count - 2,
            negativeVertices.Count - 1,
        });

        positiveVertices.AddRange(new Vector3[] {
            intersection1,
            vtx2,
            intersection2,
            intersection2,
            vtx2,
            vtx3,
        });

        positiveTriangles.AddRange(new int[] {
            positiveVertices.Count - 6,
            positiveVertices.Count - 5,
            positiveVertices.Count - 4,
            positiveVertices.Count - 3,
            positiveVertices.Count - 2,
            positiveVertices.Count - 1,
        });

        return;
    }
}

// if (v1v2)
// {
//     Vector3 dir1 = mesh.vertices[i + 1] - mesh.vertices[i];
//     Vector3 dir2 = mesh.vertices[i + 2] - mesh.vertices[i];

//     Vector3 intersection1 = plane.GetPointPlaneIntersection(mesh.vertices[i + 1], dir1);
//     Vector3 intersection2 = plane.GetPointPlaneIntersection(mesh.vertices[i + 2], dir2);

//     if (side[i])
//     {
//         newPositiveVertices.AddRange(new Vector3[] {
//             mesh.vertices[i],
//             intersection1,
//             intersection2,
//         });

//         newPositiveTriangles.AddRange(new int[] {
//             i, i + 1, i + 2,
//         });

//         newNegativeVertices.AddRange(new Vector3[] {
//             mesh.vertices[i + 2],
//             intersection1,
//             mesh.vertices[i + 1],
//             mesh.vertices[i + 2],
//             intersection2,
//             intersection1,
//         });

//         newNegativeTriangles.AddRange(new int[] {
//             i, i + 1, i + 2,
//             i + 3, i + 4, i + 5,
//         });
//     }
//     else
//     {
//         newNegativeVertices.AddRange(new Vector3[] {
//             mesh.vertices[i],
//             intersection1,
//             intersection2,
//         });
//         newNegativeTriangles.AddRange(new int[] {
//             i, i + 1, i + 2,
//         });

//         newPositiveVertices.AddRange(new Vector3[] {
//             intersection1,
//             mesh.vertices[i + 1],
//             intersection2,
//             intersection2,
//             mesh.vertices[i + 1],
//             mesh.vertices[i + 2],
//         });
//         newPositiveTriangles.AddRange(new int[] {
//             i, i + 1, i + 2,
//             i + 3, i + 4, i + 5,
//         });
//     }

//     continue;
// }