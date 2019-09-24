using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cross : MonoBehaviour
{
    // line"x"Collider"y"

    // { this_x, this_y}
    private Vector2 this_segment;

    // { other_x, other_y}
    private Vector2 other_segment;

    // { this_segment, other_segment}
    private List<Vector2> cross_segment;

    // < { this_segment01, other_segment01}, { this_segment02, other_segment02}, ....>
    public List<List<Vector2>> cross_segments;

    public bool cross_flag = false;

    // Start is called before the first frame update
    void Start()
    {
        cross_segments = new List<List<Vector2>>();

        // this_segment = new Vector2(int.Parse(this.gameObject.name.Substring(4, 1)), int.Parse(this.gameObject.name.Substring(13, 1)));
    }
    /*
      private void OnTriggerStay(Collider other)
    {
        if(other.gameObject.tag == "Rord")
        {
            other_segment = new Vector2(int.Parse(other.gameObject.name.Substring(4, 1)), int.Parse(other.gameObject.name.Substring(13, 1)));

            if (this_segment.x == other_segment.x && (this_segment.y == other_segment.y - 1 || this_segment.y == other_segment.y + 1))
            {
                // 処理をしない
            }
            else
            {
                cross_segment = new List<Vector2>();
                cross_segment.Add(this_segment);
                cross_segment.Add(other_segment);

                cross_segments.Add(cross_segment);
                // Debug.Log(this_x + "--" + this_y + "   " + other_x + "--" + other_y);

                cross_flag = true;
            }
        }
    }
    */
   
}
