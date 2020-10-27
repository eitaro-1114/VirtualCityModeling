using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bitmapDemo : MonoBehaviour
{
    public GameObject CameraObj;
    private Camera camera;
    private Bitmap bitmap;
    public GameObject bit;
    public GameObject site;

    // Start is called before the first frame update
    void Start()
    {
        camera = CameraObj.GetComponent<Camera>();
        bitmap = new Bitmap(80, 60, camera);
        bitmap.ViewBitParametor(bit);        
    }

    void GenerateSite(int siteNum)
    {
        float x = -camera.ViewportToWorldPoint(Vector2.zero).x;
        float y = camera.ViewportToWorldPoint(Vector2.zero).y;
        int generateCount = 0;
        int safety = 0;
        while (true)
        {
            Vector2 sitePos = new Vector2(Random.Range(-x, x), Random.Range(-y, y));
            float siteValue = Random.Range(0f, 1f);
            if (siteValue < bitmap.GetWeight(sitePos))
            {
                Instantiate(site, new Vector3(sitePos.x, 0f, sitePos.y), Quaternion.identity);
                generateCount++;
            }
            safety++;
            if(generateCount >= siteNum || safety > 5000)
            {
                break;
            }
        }
        
    }

    private void Update()
    {
        if(Input.GetMouseButtonDown(1))
        {
            Vector3 mouseScreenPosition = Input.mousePosition;
            mouseScreenPosition = Camera.main.ScreenToWorldPoint(mouseScreenPosition);
            Vector2 mouseWorldPosition = new Vector2(mouseScreenPosition.x, mouseScreenPosition.z);
            Debug.Log("weight = " + bitmap.GetWeight(mouseWorldPosition));
        }
        if (Input.GetMouseButton(0))
        {
            Vector3 mouseScreenPosition = Input.mousePosition;
            mouseScreenPosition = Camera.main.ScreenToWorldPoint(mouseScreenPosition);
            Vector2 mouseWorldPosition = new Vector2(mouseScreenPosition.x, mouseScreenPosition.z);
            bitmap.UseBrush(mouseWorldPosition, 1.2f, 0.1f);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            GenerateSite(100);
        }

    }
}
