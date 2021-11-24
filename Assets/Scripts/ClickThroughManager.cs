using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class ClickThroughDataSerial {
	public string background;
	public string settingname;
	public string music;
	public string adbackground;
	//the [x0,y0] coords of clickable things
	public int[] firstx;
	public int[] firsty;
	//the [x1,y1] coords
	public int[] secondx;
	public int[] secondy;
	//the game actions they point to
	//see gameactions.txt
	public string[] actions;
}

//a manager for places like towns and libraries where you click on objects
//or navigate to them with arrow keys then press enter to go to another submanager
public class ClickThroughManager : MonoBehaviour
{
	public GameObject[] CTClickables;
	
     void Start()
     {
         Vector2[] vertices = new Vector2[] { new Vector2(1, 1), new Vector2(.05f, 1.3f), new Vector2(1, 2), new Vector2(1.95f, 1.3f), new Vector2(1.58f, 0.2f), new Vector2(.4f, .2f) };
         ushort[] triangles = new ushort[] { 0, 1, 2, 0, 2, 3, 0, 3, 4, 0, 4, 5, 0, 5, 1 };
         DrawPolygon2D(vertices, triangles, Color.red);
     }
 
     void DrawPolygon2D(Vector2[] vertices, ushort[] triangles, Color color)
     {
         GameObject polygon = new GameObject(); //create a new game object
         SpriteRenderer sr = polygon.AddComponent<SpriteRenderer>(); // add a sprite renderer
         Texture2D texture = new Texture2D(1025, 1025); // create a texture larger than your maximum polygon size
 
         // create an array and fill the texture with your color
         List<Color> cols = new List<Color>(); 
         for (int i = 0; i < (texture.width * texture.height); i++)
            cols.Add(color);
         texture.SetPixels(cols.ToArray());
         texture.Apply();
 
         sr.color = color; //you can also add that color to the sprite renderer
 
         sr.sprite = Sprite.Create(texture, new Rect(0, 0, 1024, 1024), Vector2.zero, 1); //create a sprite with the texture we just created and colored in
 
         //convert coordinates to local space
         float lx = Mathf.Infinity, ly = Mathf.Infinity;
         foreach (Vector2 vi in vertices)
         {
             if (vi.x < lx)
                 lx = vi.x;
             if (vi.y < ly)
                 ly = vi.y;
         }
         Vector2[] localv = new Vector2[vertices.Length];
         for (int i = 0; i < vertices.Length; i++)
         {
             localv[i] = vertices[i] - new Vector2(lx, ly);
         }
 
         sr.sprite.OverrideGeometry(localv, triangles); // set the vertices and triangles
 
         polygon.transform.position = (Vector2)transform.InverseTransformPoint(transform.position) + new Vector2(lx, ly); // return to world space
     }
}