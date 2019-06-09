using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Build_Hit : MonoBehaviour
{
    // 拡縮する前のオブジェクトのスケール値
    private float scale_now;

    private void Start()
    {
        scale_now = this.gameObject.transform.localScale.y;
    }

    private void OnTriggerStay(Collider other)
    {
        // 道路と衝突したとき
        if(other.gameObject.tag == "Rord")
        {
            this.gameObject.transform.localScale -= new Vector3(2.0f * scale_now, 0, 2.0f * scale_now);

            if(this.gameObject.transform.localScale.x < 0 || this.gameObject.transform.localScale.z < 0)
            {
                this.gameObject.SetActive(false);
            }
        }
   
    }

    private void OnTriggerEnter(Collider other)
    {
        // 他の建物と衝突したとき
        if (other.gameObject.tag == "Building")
        {
            float floor = this.gameObject.transform.localScale.x * this.gameObject.transform.localScale.z;
            float other_floor = other.gameObject.transform.localScale.x * other.gameObject.transform.localScale.z;

            if(floor >= other_floor)
            {
                other.gameObject.SetActive(false);
            }
            else
            {
                this.gameObject.SetActive(false);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        float floor = this.gameObject.GetComponent<Renderer>().bounds.size.x * this.gameObject.GetComponent<Renderer>().bounds.size.z;

        if (floor <= 60.0f)
        {
            this.gameObject.transform.localScale = new Vector3(this.gameObject.transform.localScale.x,
                                                               Random.Range(10.0f, 50.0f) * scale_now,
                                                               this.gameObject.transform.localScale.z
                                                               );
        }
        else if (floor <= 100.0f)
        {
            this.gameObject.transform.localScale = new Vector3(this.gameObject.transform.localScale.x,
                                                               Random.Range(10.0f, 110.0f) * scale_now,
                                                               this.gameObject.transform.localScale.z
                                                               );
        }
        else if (floor <= 150.0f)
        {
            this.gameObject.transform.localScale = new Vector3(this.gameObject.transform.localScale.x,
                                                               Random.Range(70.0f, 110.0f) * scale_now,
                                                               this.gameObject.transform.localScale.z
                                                               );
        }
        else
        {
            this.gameObject.transform.localScale = new Vector3(this.gameObject.transform.localScale.x,
                                                               Random.Range(70.0f, 200.0f) * scale_now,
                                                               this.gameObject.transform.localScale.z
                                                               );
        }

    }
}
