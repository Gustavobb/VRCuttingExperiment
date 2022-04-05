using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TangentSpaceVisualizer : MonoBehaviour
{
    public float offset = 0.01f;
    public float scale = 0.1f;

    void OnDrawGizmos () {
		MeshFilter filter = GetComponent<MeshFilter>();
		if (filter) {
			Mesh mesh = filter.sharedMesh;
			if (mesh) {
				ShowTangentSpace(mesh);
			}
		}
	}

    void ShowTangentSpace (Mesh mesh) {
		Vector3[] vertices = mesh.vertices;
		Vector3[] normals = mesh.normals;
        int[] triangles = mesh.triangles;

        // for (int i = 0; i < triangles.Length; i += 3) {
        //     int i1 = triangles[i];
        //     int i2 = triangles[i + 1];
        //     int i3 = triangles[i + 2];

        //     Vector3 v1 = vertices[i1];
        //     Vector3 v2 = vertices[i2];
        //     Vector3 v3 = vertices[i3];

        //     if (v1 == v2 || v2 == v3 || v3 == v1) {
        //         Debug.Log("Skipping degenerate triangle");
        //     }
        //     else {
        //         Debug.Log("Drawing triangle");
        //     }
        // }
        
		for (int i = 0; i < vertices.Length; i++) {
            // print("Normal: " + normals[i] + " vertex: " + vertices[i]);
			ShowTangentSpace(
				transform.TransformPoint(vertices[i]),
				transform.TransformDirection(normals[i])
			);
		}
	}

	void ShowTangentSpace (Vector3 vertex, Vector3 normal) {
		vertex += normal * offset;
		Gizmos.color = Color.green;
		Gizmos.DrawLine(vertex, vertex + normal * scale);
	}
}
