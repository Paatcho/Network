using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class QuadFromSprite : MonoBehaviour {
    public Sprite sprite;

    void Start() {
        var mf = GetComponent<MeshFilter>();
        var mesh = new Mesh();
        mf.mesh = mesh;

        // 4 vertices for a quad
        Vector3[] vertices = new Vector3[4];
        Vector2[] uv = new Vector2[4];
        int[] triangles = new int[6];

        float w = sprite.bounds.size.x;
        float h = sprite.bounds.size.y;

        // define vertices in local space
        vertices[0] = new Vector3(-w/2, -h/2, 0);
        vertices[1] = new Vector3(w/2, -h/2, 0);
        vertices[2] = new Vector3(-w/2, h/2, 0);
        vertices[3] = new Vector3(w/2, h/2, 0);

        // UVs correspond to sprite's texture
        Rect texRect = sprite.textureRect;
        Vector2 texSize = new Vector2(sprite.texture.width, sprite.texture.height);
        uv[0] = new Vector2(texRect.xMin / texSize.x, texRect.yMin / texSize.y);
        uv[1] = new Vector2(texRect.xMax / texSize.x, texRect.yMin / texSize.y);
        uv[2] = new Vector2(texRect.xMin / texSize.x, texRect.yMax / texSize.y);
        uv[3] = new Vector2(texRect.xMax / texSize.x, texRect.yMax / texSize.y);

        // triangles (two tris for quad)
        triangles[0] = 0;
        triangles[1] = 2;
        triangles[2] = 1;

        triangles[3] = 2;
        triangles[4] = 3;
        triangles[5] = 1;

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();
        mesh.RecalculateTangents(); // generate tangents for normal mapping :contentReference[oaicite:1]{index=1}
    }
}