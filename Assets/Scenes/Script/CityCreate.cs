using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CityCreate : MonoBehaviour
{

    int mickey = 0;                                     // マウスの移動量
    private List<Vector3> Line;                         // 1本の線の頂点群
    public GameObject Line_Collider;                    // Lineの衝突判定
    private List<LineRenderer> LineRendererList;        // LineRendererの頂点リスト
    int line_cnt = 0;                                   // Lineの本数
    private List<List<Vector3>> Lines;                  // 線群
    private List<Vector3> Cross;                        // 交点群
    private GameObject LinesObj;                        // Lineをまとめる親
    public GameObject ParentObj;                        // 親となるオブジェクト
    public Material LineMaterial;                       // Lineのマテリアル
    public Color LineColor;                             // Lineの色
    float widthCoefficient = 1.0f;                      // Lineの幅係数
    public float lineWidth;                             // Lineの幅
    public GameObject Build;                            // 建物のオブジェクト
    public GameObject Buildings;                        // 建物群の親
    public int buildGenerateCount = 1000;               // 建物の生成試行回数
    private List<float> Build_proparty;                 // {x, z, x_size, z_size}
    private List<List<float>> Build_proparties;         // {x, z, x_size, z_size}の集まり
    private List<int> deleteManeger;                    // 消される建物の番号を登録
    private List<Vector3> StraightList;                 // 直線ツールの始点と終点

    bool straight_flag = false;                         // 直線ツールを使っているか
    public GameObject StraightRenderer;                 // 直線ツールの補助線
    private GameObject StraightObj;                     // 補助線のインスタンス

    bool build_flag = false;                            // 建物生成が終わったか

    int k = 0;

    // Start is called before the first frame update
    void Start()
    {



        LineRendererList = new List<LineRenderer>();
        Line = new List<Vector3>();
        Lines = new List<List<Vector3>>();
        Cross = new List<Vector3>();
        Build_proparties = new List<List<float>>();

        // 曲線をまとめる親Obj
        LinesObj = Instantiate(ParentObj, new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity);
        LinesObj.name = "Lines";

        Buildings = Instantiate(ParentObj, new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity);
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetMouseButtonDown(0))
        {
            this.AddLineObject();
            if (straight_flag == true)
            {
                // 直線の始点を追加
                this.AddPositionDataToLineRendererList();

                // 直線の状態を見せるため
                StraightList = new List<Vector3>();
                StraightList.Add(Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 1.0f)));
                StraightObj = Instantiate(StraightRenderer, new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity);

            }
        }

        // LineRendererに位置データを指定しておく
        if (Input.GetMouseButton(0))
        {
            // 直線ツールを使っていないとき
            if(straight_flag == false)
            {
                if (mickey % 7 == 0)
                {
                    this.AddPositionDataToLineRendererList();
                }
                mickey++;
            }
            // 使っているとき
            else
            {
                LineRenderer straightRender = StraightObj.GetComponent<LineRenderer>();
                straightRender.startWidth = 5.0f;
                straightRender.endWidth = 5.0f;

                straightRender.SetPosition(0, StraightList[0]);
                straightRender.SetPosition(1, Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 1.0f)));
            }
            
        }

        if (Input.GetMouseButtonUp(0))      // (建物が生成されている間は無効)
        {
            // 直線ツールを起動していたら
            if (straight_flag == true)
            {
                Destroy(StraightObj);

                // 直線の終点を追加
                this.AddPositionDataToLineRendererList();
            }

            line_cnt++;            //Lineの本数を記録
            Lines.Add(Line);       //Linesに線情報を追加
            Line = new List<Vector3>();

            for (int j = 0; j < Lines.Count; j++)    //最後の一つ前までの線分
            {
                for (int k = 1; k < Lines[j].Count; k++)     //その線分の要素数
                {
                    for (int h = 1; h < Lines[Lines.Count - 1].Count; h++)   //最後の線分の要素数
                    {
                        CrossJudge(Lines[Lines.Count - 1][h - 1].x, Lines[Lines.Count - 1][h - 1].z, Lines[Lines.Count - 1][h].x, Lines[Lines.Count - 1][h].z, Lines[j][k - 1].x, Lines[j][k - 1].z, Lines[j][k].x, Lines[j][k].z);
                    }
                }
            }

        }

        // 直線ツールを使う
        if (Input.GetKeyDown(KeyCode.S))
        {
            if (straight_flag == false)
            {
                straight_flag = true;                
            }
            else
            {
                straight_flag = false;
            }
        }

        // Lineの幅
        if (Input.GetKeyDown(KeyCode.Keypad1) && build_flag == false)  // 1車線
        {
            widthCoefficient = 0.25f / 2.0f;
            // LineWidthText.GetComponent<Text>().text = "1車線";
        }

        if (Input.GetKeyDown(KeyCode.Keypad2) && build_flag == false)  // 2車線
        {
            widthCoefficient = 0.25f;
            // LineWidthText.GetComponent<Text>().text = "2車線";
        }

        if (Input.GetKeyDown(KeyCode.Keypad3) && build_flag == false)  // 3車線
        {
            widthCoefficient = 0.5f;
            // LineWidthText.GetComponent<Text>().text = "3車線";
        }

        if (Input.GetKeyDown(KeyCode.Keypad4) && build_flag == false)  // 4車線
        {
            widthCoefficient = 1.0f;
            // LineWidthText.GetComponent<Text>().text = "4車線";
        }

        // 建物生成
        if (Input.GetKeyDown(KeyCode.Space))
        {             
            for (int i = 0; i < buildGenerateCount; i++)
            {
                BuildGenerate(i);
            }

            build_flag = true;
        }

    }

    // 線オブジェクトの追加を行う
    private void AddLineObject()
    {

        // 追加するオブジェクトをインスタンス
        GameObject lineObject = new GameObject();
        lineObject.transform.parent = LinesObj.transform;

        // 名前を付ける
        lineObject.name = "Line " + line_cnt;

        // tagを付ける
        lineObject.tag = "Rord";

        lineObject.transform.Rotate(90.0f, 0.0f, 0.0f);

        // オブジェクトにLineRendererを取り付ける
        lineObject.AddComponent<LineRenderer>();

        lineObject.GetComponent<LineRenderer>().alignment = LineAlignment.TransformZ;

        lineObject.GetComponent<LineRenderer>().numCapVertices = 5;
        //activityObject.GetComponent<LineRenderer>().numCapVertices = 5;


        // 描く線のコンポーネントリストに追加する
        LineRendererList.Add(lineObject.GetComponent<LineRenderer>());

        // 線と線をつなぐ点の数を0に初期化
        LineRendererList.Last().positionCount = 0;

        // マテリアルを初期化
        LineRendererList.Last().material = this.LineMaterial;

        // 線の色を初期化
        LineRendererList.Last().material.color = this.LineColor;

        // 線の太さを初期化
        LineRendererList.Last().startWidth = this.lineWidth * widthCoefficient;
        LineRendererList.Last().endWidth = this.lineWidth * widthCoefficient;

        lineObject.AddComponent<MeshCollider>();
        lineObject.GetComponent<MeshCollider>().convex = true;
        lineObject.GetComponent<MeshCollider>().isTrigger = true;

    }

    // 描く線のコンポーネントリストに位置情報を登録していく
    private void AddPositionDataToLineRendererList()
    {

        // 座標の変換を行いマウス位置を取得
        Vector3 screenPosition01 = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 1.0f);
        Vector3 screenPosition02 = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 1.1f);
        var mousePosition = Camera.main.ScreenToWorldPoint(screenPosition01);
        var activityPosition = Camera.main.ScreenToWorldPoint(screenPosition02);

        Line.Add(mousePosition);

        if (Line.Count > 1)
        {
            // 2点間の中点に空Obj生成
            Vector3 colliderPos = new Vector3
            (
                (Line[Line.Count - 2].x + Line[Line.Count - 1].x) / 2,
                9.0f,
                (Line[Line.Count - 2].z + Line[Line.Count - 1].z) / 2
            );
            GameObject lineCollider = Instantiate(Line_Collider, colliderPos, Quaternion.identity);
            lineCollider.name = "lineCollider" + (Line.Count - 2);
            lineCollider.tag = "Rord";

            // Lineオブジェクトの子にする
            GameObject lineParent = GameObject.Find("Line " + line_cnt);
            lineCollider.transform.parent = lineParent.transform;

            // 空ObjにBoxColliderをアタッチ
            lineCollider.AddComponent<BoxCollider>();

            // 空ObjにRigidBodyをアタッチ
            lineCollider.AddComponent<Rigidbody>();
            lineCollider.GetComponent<Rigidbody>().useGravity = false;
            lineCollider.GetComponent<Rigidbody>().isKinematic = true;

            // 2点間の傾きを求める
            float a = Line[Line.Count - 1].x - Line[Line.Count - 2].x;
            float b = Line[Line.Count - 1].z - Line[Line.Count - 2].z;
            float l = Mathf.Sqrt(a * a + b * b);
            if (a != 0)
            {
                float angle = Mathf.Atan(b / a) * 180 / Mathf.PI;
                lineCollider.transform.rotation = Quaternion.Euler(90.0f, 0.0f, angle);

            }
            else
            {
                lineCollider.transform.rotation = Quaternion.Euler(90.0f, 0.0f, 90.0f);
            }

            // 道路のサイズに合わせる
            lineCollider.GetComponent<BoxCollider>().size = new Vector3(l, lineWidth * widthCoefficient, 0.1f);

        }

        // 線と線をつなぐ点の数を更新
        LineRendererList.Last().positionCount += 1;
        //activityObjectList.Last().positionCount += 1;

        // 描く線のコンポーネントリストを更新
        LineRendererList.Last().SetPosition(LineRendererList.Last().positionCount - 1, mousePosition);
        //activityObjectList.Last().SetPosition(LineRendererList.Last().positionCount - 1, activityPosition);

    }

    //交差判定
    private void CrossJudge(float p0x, float p0y, float p1x, float p1y, float q0x, float q0y, float q1x, float q1y)
    {

        float dpx = p1x - p0x;
        float dpy = p1y - p0y;
        float dqx = q1x - q0x;
        float dqy = q1y - q0y;

        float ta = (q0x - q1x) * (p0y - q0y) + (q0y - q1y) * (q0x - p0x);
        float tb = (q0x - q1x) * (p1y - q0y) + (q0y - q1y) * (q0x - p1x);
        float tc = (p0x - p1x) * (q0y - p0y) + (p0y - p1y) * (p0x - q0x);
        float td = (p0x - p1x) * (q1y - p0y) + (p0y - p1y) * (p0x - q1x);

        if (ta * tb < 0 && tc * td < 0)
        {
            float s1 = ((q0x - q1x) * (p0y - p1y) - (q0y - q1y) * (p0x - q1x)) / 2;
            float s2 = ((q0x - q1x) * (q1y - p1y) - (q0y - q1y) * (q1x - p1x)) / 2;

            float a = dpx * dqy - dqx * dpy;
            float x = (dpx * dqx * (p0y - q0y) + dpx * dqy * q0x - dqx * dpy * p0x) / a;
            float y = (dpy * dqy * (q0x - p0x) + dpx * dqy * p0y - dqx * dpy * q0y) / a;

            // Instantiate(CrossObj, new Vector3(x, 9.0f, y), Quaternion.identity);
            Cross.Add(new Vector3(x, 9.0f, y));

        }
    }


    // buildCountは0から
    private void BuildGenerate(int buildCount)
    {

        // Cubeの生成座標を決定
        Vector3 BuildPos = new Vector3(
            Random.Range(-Camera.main.ViewportToWorldPoint(Vector2.one).x, Camera.main.ViewportToWorldPoint(Vector2.one).x),
            9.0f,
            Random.Range(-Camera.main.ViewportToWorldPoint(Vector2.one).z, Camera.main.ViewportToWorldPoint(Vector2.one).z)
        );

        // Cube生成
        GameObject Building = Instantiate(Build, BuildPos, Quaternion.identity) as GameObject;

        // 親子関係
        Building.transform.parent = Buildings.transform;
        
        Building.name = "Building" + buildCount;
                
        //Instantiate(Building, BuildPos, Quaternion.identity);

        // 生成した点と交差点の距離のリスト
        List<float> dis = new List<float>();

        // 生成した点と交差点の距離を求める
        for (int j = 0; j < Cross.Count; j++)
        {
            dis.Add((BuildPos.x - Cross[j].x) * (BuildPos.x - Cross[j].x) + (BuildPos.z - Cross[j].z) * (BuildPos.z - Cross[j].z));
        }

        // 一番近い距離の交点を求める
        // 総当たりなので重い
        Vector3 crossMin = Cross[0];
        float disMin = dis[0];
        for (int j = 0; j < dis.Count; j++)
        {
            if (dis[j] < disMin)
            {
                disMin = dis[j];
                crossMin = Cross[j];
            }
        }
        // 拡縮する前のオブジェクトのスケール値
        float width_now = Building.transform.localScale.x;
        float depth_now = Building.transform.localScale.z;

        // BuildPosを中心点として、Crossの位置まで拡縮
        float width = Building.GetComponent<Renderer>().bounds.size.x;
        float depth = Building.GetComponent<Renderer>().bounds.size.z;

        
        Building.transform.localScale = new Vector3
                                        (
                                            Building.transform.localScale.x * (Mathf.Abs((2 * BuildPos.x - 2 * crossMin.x)) / width) ,
                                            Building.transform.localScale.y,
                                            Building.transform.localScale.z * (Mathf.Abs((2 * BuildPos.z - 2 * crossMin.z)) / depth) 
                                        );
         


        /*
        // 他の建物との衝突判定(準備)
        Build_proparty = new List<float>();
        deleteManeger = new List<int>();

        // Build_proparty{x_psos, z_pos, x_scale. z_scale}に各要素を追加
        Build_proparty.Add(Building.transform.localPosition.x);
        Build_proparty.Add(Building.transform.localPosition.z);
        Build_proparty.Add(Building.transform.localScale.x);
        Build_proparty.Add(Building.transform.localScale.z);

        // Build_propartiesに追加
        Build_proparties.Add(Build_proparty);

         // 他の建物との衝突判定(総当たり)
        for(int i = 0; i < buildCount; i++)
        {
            // たった今生成した建物のアレ(以下，今)
            float x = Build_proparties[buildCount][0];
            float z = Build_proparties[buildCount][1];
            float x_size = Build_proparties[buildCount][2];
            float z_size = Build_proparties[buildCount][3];
            // 今の建物の外接円の半径
            float r = Mathf.Sqrt((x_size / 2) * (x_size / 2) + (z_size / 2) + (z_size / 2));

            // これから比較する建物のアレ(以下，他)
            float other_x = Build_proparties[i][0];
            float other_z = Build_proparties[i][1];
            float other_x_size = Build_proparties[i][2];
            float other_z_size = Build_proparties[i][3];   
            float other_r = Mathf.Sqrt((other_x_size / 2) * (other_x_size / 2) + (other_z_size / 2) + (other_z_size / 2));

            // 二点間の距離
            float lx = Mathf.Abs(x - other_x);
            float lz = Mathf.Abs(z - other_z);

            // 2つの建物が接触していたなら
            if(2 * lx < x_size + other_x_size && 2 * lz < z_size + other_z_size)
            {
                // 底面積を求める
                float floor = x_size * z_size;
                float other_floor = other_x_size * other_z_size;

                // 底面積による比較
                if(floor >= other_floor)        // 今＞他
                {
                    // 消すオブジェクトを登録
                    deleteManeger.Add(i);

                }
                else                            // 他＞今
                {
                    Building.SetActive(false);
                    Build_proparties.RemoveAt(buildCount);
                    return;
                }
            }

        }

        // deleteManegerのソート
        deleteManeger.Sort();

        // 登録された建物を消去(ここまで書いたよ)
        for(int i = 0; i < deleteManeger.Count; i++)
        {
            GameObject.Find("Building" + (deleteManeger[i] - i)).SetActive(false);
            Build_proparties.RemoveAt(deleteManeger[i] - i);
        }
         
         */
    }
}
