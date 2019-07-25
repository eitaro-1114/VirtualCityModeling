using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// クラスの練習
public class SegmentIndex
{
    public int p0a;
    public int p0b;

    public int p1a;
    public int p1b;

    public int q0a;
    public int q0b;

    public int q1a;
    public int q1b;

    public SegmentIndex(int p0a, int p0b, int p1a, int p1b, int q0a, int q0b, int q1a, int q1b)
    {
        this.p0a = p0a;
        this.p0b = p0b;
        this.p1a = p1a;
        this.p1b = p1b;

        this.q0a = q0a;
        this.q0b = q0b;
        this.q1a = q1a;
        this.q1b = q1b;
    }

}

// 交点が
// 1．どの曲線上にあるか
// 2．どの線分上にあるか
// 3．交点座標
public class CrossPropaty
{
    // インデックス(Lines[CurveIndex][Segmentindex])
    public int CurveIndex;              
    public int SegmentIndex;

    // 交点座標
    public Vector3 CrossCoordinate;     

    public CrossPropaty(int CurveIndex, int SegmentIndex , Vector3 Cross)
    {
        this.CurveIndex = CurveIndex;
        this.SegmentIndex = SegmentIndex;
        this.CrossCoordinate = Cross;
    }
}

public class CityCreate : MonoBehaviour
{

    int mickey = 0;                                     // マウスの移動量
    private List<Vector3> Line;                         // 1本の線の頂点群
    public GameObject Line_Collider;                    // Lineの衝突判定
    private List<LineRenderer> LineRendererList;        // LineRendererの頂点リスト
    private List<SegmentIndex> CrossPair;               // 交差した線分対
    private List<CrossPropaty> CrossPropaties;          // 交点の情報群
    private int line_cnt = 0;                           // Lineの本数
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

    public bool build_flag = false;                     // 建物生成が終わったか
    public GameObject CrossObj;                         // 交点確認用オブジェクト    

    public GameObject Floor;                            // 床オブジェクト
    public GameObject Guide;                            // ガイドの床
    public GameObject ViewCamera;                       // 眺めるモードのカメラ

    // Start is called before the first frame update
    void Start()
    {

        LineRendererList = new List<LineRenderer>();
        Line = new List<Vector3>();
        Lines = new List<List<Vector3>>();
        Cross = new List<Vector3>();
        Build_proparties = new List<List<float>>();
        CrossPair = new List<SegmentIndex>();
        CrossPropaties = new List<CrossPropaty>();

        // 曲線をまとめる親Obj
        LinesObj = Instantiate(ParentObj, new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity);
        LinesObj.name = "Lines";

        Buildings = Instantiate(ParentObj, new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity);
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetMouseButtonDown(0) && build_flag == false)
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
        if (Input.GetMouseButton(0) && build_flag == false)
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

        if (Input.GetMouseButtonUp(0) && build_flag == false)      // (建物が生成されている間は無効)
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
        if (Input.GetKeyDown(KeyCode.Space) && build_flag == false)
        {

            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            /*
            for (int i = 0; i < buildGenerateCount; i++)
            {
                BuildGenerate(i);
            }            
             */

            /*
            for(int i = 0; i < GameObject.Find("Lines").transform.childCount; i++)
            {
                for(int k = 0; k < GameObject.Find("Line " + i).transform.childCount; k++)
                {
                    GameObject Segment = GameObject.Find("line" + i + "Collider" + k);

                    if(Segment.GetComponent<Cross>().cross_flag == true)
                    {
                        // Debug.Log("this_x : " + Segment.GetComponent<Cross>().this_x + "  this_y : " + Segment.GetComponent<Cross>().this_y + "  other_x : " + Segment.GetComponent<Cross>().other_x + "  other_y : " + Segment.GetComponent<Cross>().other_y);
                    }

                    Segment.GetComponent<BoxCollider>().isTrigger = false;
                }

            }
             */
            
            // 交点列挙
            for(int i = 0; i < Lines.Count; i++)
            {
                for(int k = 0; k < Lines[i].Count - 1; k++)
                {
                    Vector2 p0 = new Vector2(Lines[i][k].x, Lines[i][k].z);
                    Vector2 p1 = new Vector2(Lines[i][k + 1].x, Lines[i][k + 1].z);

                    for(int j = 0; j < Lines.Count; j++)
                    {
                        for(int l = 0; l < Lines[j].Count - 1; l++)
                        {
                            Vector2 q0 = new Vector2(Lines[j][l].x, Lines[j][l].z);
                            Vector2 q1 = new Vector2(Lines[j][l + 1].x, Lines[j][l + 1].z);

                            SegmentIndex nowSegmentIndex = new SegmentIndex(i, k, i, k + 1, j, l, j, l + 1);

                            bool judge_flag = true;


                            // 頂点のインデックスが入れ替わっていると同じ判定を二回繰り返してしまう
                            for (int m = 0; m < CrossPair.Count; m++)
                            {
                                if (CrossPair[m].p0a == j && CrossPair[m].p0b == l && CrossPair[m].p1a == j && CrossPair[m].p1b == l + 1 && CrossPair[m].q0a == i && CrossPair[m].q0b == k && CrossPair[m].q1a == i && CrossPair[m].q1b == k + 1)
                                {
                                    // 頂点が入れ替わっているだけならフラグをfalseに
                                    judge_flag = false;
                                }
                            }
                            
                            if(judge_flag == true)
                            {
                                CrossJudge(p0, p1, q0, q1, nowSegmentIndex, Lines[i], Lines[j]);
                            }  

                        }
                    }
                }
            }

            // リストのソート
            CrossPropaties.Sort((a, b) => a.CurveIndex - b.CurveIndex);
            for (int i = 0; i < CrossPropaties.Count - 1; i++)
            {
                if(CrossPropaties[i].CurveIndex == CrossPropaties[i + 1].CurveIndex)
                {
                    Vector3 target_point = Lines[CrossPropaties[i].CurveIndex][CrossPropaties[i].SegmentIndex - 1];
                    float distance01 = Mathf.Sqrt((target_point.x - CrossPropaties[i].CrossCoordinate.x) * (target_point.x - CrossPropaties[i].CrossCoordinate.x) + (target_point.z - CrossPropaties[i].CrossCoordinate.z) * (target_point.z - CrossPropaties[i].CrossCoordinate.z));
                    float distance02 = Mathf.Sqrt((target_point.x - CrossPropaties[i + 1].CrossCoordinate.x) * (target_point.x - CrossPropaties[i + 1].CrossCoordinate.x) + (target_point.z - CrossPropaties[i + 1].CrossCoordinate.z) * (target_point.z - CrossPropaties[i + 1].CrossCoordinate.z));

                    if(distance01 >= distance02)
                    {
                        // 2要素の順番を入れ替える
                    }
                }                
            }

            for (int i = 0; i < CrossPropaties.Count; i++)
            {
                Debug.Log(CrossPropaties[i].CurveIndex + "  " + CrossPropaties[i].SegmentIndex + "  " + CrossPropaties[i].CrossCoordinate);
            }

            build_flag = true;

            // 床と眺めるカメラを設置
            Guide.SetActive(false);
            Floor.SetActive(true);
            ViewCamera.SetActive(true);

            sw.Stop();
            Debug.Log(sw.ElapsedMilliseconds + "ms");

        }

        // 交点確認
        if (Input.GetKeyDown(KeyCode.A) )
        {
            for (int i = 0; i < Cross.Count; i++)
            {
                Instantiate(CrossObj, new Vector3(Cross[i].x, 9.0f, Cross[i].z), Quaternion.identity);
            }
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
            lineCollider.name = "line" + line_cnt + "Collider" + (Line.Count - 2);
            lineCollider.tag = "Rord";

            // Lineオブジェクトの子にする
            GameObject lineParent = GameObject.Find("Line " + line_cnt);
            lineCollider.transform.parent = lineParent.transform;

            // 空ObjにBoxColliderをアタッチ
            lineCollider.AddComponent<BoxCollider>();
            lineCollider.GetComponent<BoxCollider>().isTrigger = true;

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
        LineRendererList.Last().positionCount++;
        //activityObjectList.Last().positionCount += 1;

        // 描く線のコンポーネントリストを更新
        LineRendererList.Last().SetPosition(LineRendererList.Last().positionCount - 1, mousePosition);
        //activityObjectList.Last().SetPosition(LineRendererList.Last().positionCount - 1, activityPosition);

    }

    //交差判定
    private void CrossJudge(Vector2 p0, Vector2 p1, Vector2 q0, Vector2 q1, SegmentIndex segmentIndex, List<Vector3> Line01, List<Vector3> Line02)
    {

        float dpx = p1.x - p0.x;
        float dpy = p1.y - p0.y;
        float dqx = q1.x - q0.x;
        float dqy = q1.y - q0.y;

        float ta = (q0.x - q1.x) * (p0.y - q0.y) + (q0.y - q1.y) * (q0.x - p0.x);
        float tb = (q0.x - q1.x) * (p1.y - q0.y) + (q0.y - q1.y) * (q0.x - p1.x);
        float tc = (p0.x - p1.x) * (q0.y - p0.y) + (p0.y - p1.y) * (p0.x - q0.x);
        float td = (p0.x - p1.x) * (q1.y - p0.y) + (p0.y - p1.y) * (p0.x - q1.x);

        if (ta * tb < 0 && tc * td < 0)
        {
            float s1 = ((q0.x - q1.x) * (p0.y - p1.y) - (q0.y - q1.y) * (p0.x - q1.x)) / 2;
            float s2 = ((q0.x - q1.x) * (q1.y - p1.y) - (q0.y - q1.y) * (q1.x - p1.x)) / 2;

            float a = dpx * dqy - dqx * dpy;
            float x = (dpx * dqx * (p0.y - q0.y) + dpx * dqy * q0.x - dqx * dpy * p0.x) / a;
            float y = (dpy * dqy * (q0.x - p0.x) + dpx * dqy * p0.y - dqx * dpy * q0.y) / a;

            // Instantiate(CrossObj, new Vector3(x, 9.0f, y), Quaternion.identity);
            CrossPair.Add(segmentIndex);

            // 交点を,線分を形成する1頂点として挿入
            int line01_a = segmentIndex.p1a;
            int line01_b = segmentIndex.p1b;            

            CrossPropaty cross_a = new CrossPropaty(segmentIndex.p1a, segmentIndex.p1b, new Vector3(x, 9.0f, y));
            CrossPropaty cross_b = new CrossPropaty(segmentIndex.q1a, segmentIndex.q1b, new Vector3(x, 9.0f, y));

            CrossPropaties.Add(cross_a);
            CrossPropaties.Add(cross_b);
            // Line01.Insert(line01_b, new Vector3(x, 9.0f, y));
            // Lines[line01_a].Insert(line01_b, new Vector3(x, 9.0f, y));
            // Lines[segmentIndex.q1a].Insert(segmentIndex.q1b, new Vector3(x, 9.0f, y));


            // GameObject segment01 = GameObject.Find("Line " + segmentIndex.p0a);
            // GameObject segment02 = GameObject.Find("Line " + segmentIndex.q0a);

            /*
            segment01.GetComponent<LineRenderer>().positionCount++;
            segment02.GetComponent<LineRenderer>().positionCount++;

            Vector3[] segment01points = Lines[segmentIndex.p1a].ToArray();
            segment01.GetComponent<LineRenderer>().SetPositions(segment01points);

            Vector3[] segment02points = Lines[segmentIndex.q1a].ToArray();
            segment02.GetComponent<LineRenderer>().SetPositions(segment02points);

            Debug.Log(segment01.name);
             
             */


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
        if(Building.GetComponent<Build_Hit>().hit == true)
        {
            Debug.Log("!!");
            float floor = Building.transform.localScale.x * Building.transform.localScale.z;
            float other_floor = Building.GetComponent<Build_Hit>().otherBuilding.transform.localScale.x * Building.GetComponent<Build_Hit>().otherBuilding.transform.localScale.z;
            if(floor >= other_floor)
            {
                GameObject.Find(Building.GetComponent<Build_Hit>().otherBuilding.name).SetActive(false);
            }
            else
            {
                Building.SetActive(false);
            }
        }
         
         */
 
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
