using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bitmap : MonoBehaviour
{
    // どっちか忘れたら漢字の平行線を見るとよいぞ
    private int row;                // 行
    private int column;           // 列  

    private static float[,] bitArray;    // 各ビットに格納する重み(0 ~ 1) 
    private Camera mainCamera;     //  メインカメラ

    private float bitWidth;                // ビットの幅
    private float bitHeight;               // ビットの高さ

    private Vector2 screenOrigin;                // スクリーンの左上座標(スクリーン座標の原点)
    private static GameObject[,] bitObjects;

    private GameObject bitmapObject;

    public Bitmap(int row, int column, Camera camera)
    {
        this.row = row;
        this.column = column;
        bitArray = new float[row, column];
        bitObjects = new GameObject[row, column];

        screenOrigin = new Vector2(camera.ViewportToWorldPoint(Vector2.zero).x, camera.ViewportToWorldPoint(Vector2.zero).z);
        // スクリーンの大きさ
        float screenWidth = Mathf.Abs(screenOrigin.x * 2);
        float screenHeight = Mathf.Abs(screenOrigin.y * 2);

        // ビットの大きさ
        bitWidth = screenWidth / column;
        bitHeight = screenHeight / row;

        for (int i = 0; i < bitArray.GetLength(1); i++)
        {
            for (int j = 0; j < bitArray.GetLength(0); j++)
            {
                bitArray[j, i] = 0.05f;
            }
        }
    }

    // ビットの重みを加える
    private void Addweight(Vector2 mousePos, float weight)
    {
        // ビットの場所を特定
        int[] bitCoordinate = ConvertToBitCoordinate(mousePos);        
        int bitI = bitCoordinate[0];
        int bitJ = bitCoordinate[1];
        // Debug.Log("i , j = " + bitI + ", " + bitJ);
        if(bitI < 0 || bitJ < 0 || bitI > column - 1 || bitJ > row - 1)
        {
            return;
        }

        // 特定したビットに重みを加える
        bitArray[bitJ, bitI] += weight;
        // 重みが1以上なら1に
        if (bitArray[bitJ, bitI] > 1.0f) bitArray[bitJ, bitI] = 1.0f;        
        if(bitObjects[bitJ, bitI] != null)
        {
            GameObject bit = bitObjects[bitJ, bitI];
            float weightColor = 1.0f - bitArray[bitJ, bitI];
            bit.GetComponent<Renderer>().material.color = new Color(1f * weightColor, 1f * weightColor, 1f * weightColor);
        }
    }

    // 指定した地点の重みを取得
    public float GetWeight(Vector2 mousePos)
    {
        // ビットの場所を特定
        int[] bitCoordinate = ConvertToBitCoordinate(mousePos);
        int bitI = bitCoordinate[0];
        int bitJ = bitCoordinate[1];
        float weight = bitArray[bitJ, bitI];
        return weight;
    }

    // ブラシを使用して重みを加える
    public void UseBrush(Vector2 mousePos, float radius, float weight)
    {        
        // ブラシの直径
        int diametorX = Mathf.CeilToInt(radius / bitWidth) * 2 - 1;
        int diametorY = Mathf.CeilToInt(radius / bitHeight) * 2 - 1;
        // Debug.Log("radius = " + radius + "  bitWidth = " + bitWidth + "  bitHeight = " + bitHeight + "  X = " + diametorX + "  Y = " + diametorY); 

        // ブラシの原点(左上)
        Vector2 brushOrigin = new Vector2(mousePos.x - radius, mousePos.y + radius);
        for(int i = 1; i < diametorY + 1 ; i++)
        {
            for(int j = 1; j < diametorX + 1; j++)
            {
                Vector2 point = new Vector2(brushOrigin.x + bitWidth * j, brushOrigin.y - bitHeight * i);
                // 円を記述(ビットが荒すぎて使えない)
                float distance = (point - mousePos).magnitude;                
                if (distance < radius)
                {
                    Addweight(point, weight);
                }
                // Debug.Log(point);
                // Addweight(point, weight);
            }
        }
    }

    // ワールド座標からビットマップの座標に変換
    private int[] ConvertToBitCoordinate(Vector2 worldCoordinate)
    {
        int[] bitCoordinate = new int[2];
      
        // グローバル座標をスクリーン座標に変換
        Vector2 screenCoordinate = worldCoordinate - screenOrigin;
        // Debug.Log("world = " + worldCoordinate + "  screen = " + screenOrigin + "screenCoordinate = " + screenCoordinate);

        // y軸反転
        // screenCoordinate.y *= -1;

        // bit座標を求める
        int bitX = Mathf.FloorToInt(screenCoordinate.x / bitWidth);
        int bitY = Mathf.FloorToInt(screenCoordinate.y / bitHeight);
        bitCoordinate[0] = bitX;
        bitCoordinate[1] = bitY;

        // Debug.Log("x = " + bitX + ", y = " + bitY);
        return bitCoordinate;
    }    

    // 各ビットを可視化
    public void ViewBitParametor(GameObject bitObject)
    {
        bitmapObject = new GameObject();
        bitmapObject.name = "bitmap";
        for(int i = 0; i < bitArray.GetLength(1); i++)
        {
            for(int j = 0; j < bitArray.GetLength(0); j++)
            {
                Vector3 bitPos = new Vector3((screenOrigin.x + bitWidth * i) + bitWidth * 0.5f, -5f, (screenOrigin.y + bitHeight * j) + bitHeight * 0.5f);
                GameObject bit = Instantiate(bitObject, bitPos, Quaternion.identity);
                bit.transform.localScale = new Vector3(bitWidth * 0.1f, 1f, bitHeight * 0.1f);
                bit.name = ("bit" + i + "-" + j);
                bit.transform.parent = bitmapObject.transform;
                bitObjects[j, i] = bit;
            }
        }
    }

    public void WeightPaintMode(bool isWeightPainting)
    {
        if (bitmapObject is null) return;

        if (isWeightPainting == true) bitmapObject.SetActive(true);
        else bitmapObject.SetActive(false);
    }
}
