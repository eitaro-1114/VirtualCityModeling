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

    public static float distanceCompare(CrossPropaty a, CrossPropaty b, Vector3 target)
    {
        float a_distance = Mathf.Sqrt((target.x - a.CrossCoordinate.x) * (target.x - a.CrossCoordinate.x) + (target.z - a.CrossCoordinate.z) * (target.z - a.CrossCoordinate.z));
        float b_distance = Mathf.Sqrt((target.x - b.CrossCoordinate.x) * (target.x - b.CrossCoordinate.x) + (target.z - b.CrossCoordinate.z) * (target.z - b.CrossCoordinate.z));
        return a_distance.CompareTo(b_distance);
    }
}

public class VertexAttribute
{
    public string attribute;    // 端点 or 交点 or それ以外
    public Vector3 coodi;       // 座標

    public VertexAttribute(string attri, Vector3 coodi)
    {
        this.attribute = attri;
        this.coodi = coodi;
    }

}

public class Area
{
    public Vector3 pos { get; set; }
    public List<Vertex> vertices { get; set; }

    public override string ToString()
        => $"{pos} : {vertices}";

    public void DrawArea(Color c)
    {
        for(int i = 0; i < vertices.Count; i++)
        {
            Debug.DrawLine(vertices[i].coordinate, vertices[(i + 1) % vertices.Count].coordinate, c, 100000f);
        }
    }

    // 左端の点Bの前の点をA、後ろの点をCとして、
    // 点CがABの右にある時、ループが時計回りである
    public void CCWSort()
    {
        Vector3 leftInfinity = new Vector3(float.PositiveInfinity, 0f, 0f);
        Vertex VertexB = null;

        // 左端の点を見つける
        foreach(Vertex v in vertices)
        {
            // 左側にあるか、x座標が同じで下側にあるか
            if((v.coordinate.x < leftInfinity.x) || (v.coordinate.x == leftInfinity.x && v.coordinate.z < leftInfinity.z))
            {
                leftInfinity = v.coordinate;
                VertexB = v;
            }
        }
        if (VertexB == null) throw new System.Exception("閉領域がないぞ");

        // 前後の点A、Cを求める
        int index = vertices.IndexOf(VertexB);
        Vertex VertexA = vertices[(vertices.Count + index + 1) % vertices.Count];
        Vertex VertexC = vertices[(vertices.Count + index - 1) % vertices.Count];

        // CAとCBの外積が正ならば、ループが時計回り
        Vector2 CA = new Vector2(VertexA.coordinate.x - VertexC.coordinate.x, VertexA.coordinate.z - VertexC.coordinate.z);
        Vector2 CB = new Vector2(VertexB.coordinate.x - VertexC.coordinate.x, VertexB.coordinate.z - VertexC.coordinate.z);
        float cross = CA.x * CB.y - CB.x * CA.y;
        if(cross > 0)
        {
            vertices.Reverse();
        }
    }

    // Polygonクラスへ変換
    public Polygon ConvertToPolygon()
    {
        // Vector2に変換
        List<Vector2> list = new List<Vector2>();
        foreach(Vertex v in vertices)
        {
            list.Add(new Vector2(v.coordinate.x, v.coordinate.z));
        }

        return new Polygon(list);
    }

    public override int GetHashCode()
        => pos.GetHashCode() ^ vertices.GetHashCode();
}

class AreaCompare : IEqualityComparer<Area>
{
    public bool Equals(Area a, Area b)
    {
        if (a == null && b == null) return true;
        if (a == null || b == null) return false;
        return a.pos == b.pos;
    }

    public int GetHashCode(Area a)
        => a.pos.GetHashCode();
}

public class Vertex
{
    public Vector3 coordinate;
    public Vertex next;
    public Vertex prev;
    public Vertex neighbor;
    // public bool isInFrontOfIntersection;    // 交点の手前にある頂点ならtrue, 直後の点ならfalse
    public bool isTakenByPolygon;
    public bool isFirstPoint;

    // 交点の時に用いる情報
    public int indexI;
    public int indexJ;
    public float distance;

    // 方向選びの際に用いる外積値
    public float cross;

    public Vertex(Vector3 coordinate)
    {
        this.coordinate = coordinate;
    }

    // コピーコンストラクタ
    public Vertex(Vertex v)
    {
        this.coordinate = v.coordinate;
        this.next = v.next;
        this.prev = v.prev;
        this.neighbor = v.neighbor;
        this.isTakenByPolygon = v.isTakenByPolygon;
        this.isFirstPoint = v.isFirstPoint;
    }
}

public class AreaAttribute
{
    public List<VertexAttribute> areapoints;
    public Vector3 center;
    public string attribute;
    public float scale;
}

// 交点を形成しているもう一つの線分のインデックス
public class IntPair
{
    public int i;
    public int j;

    public IntPair(int i, int j)
    {
        this.i = i;
        this.j = j;
    }
}

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]

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
    private List<List<VertexAttribute>> Vertex_and_Intersections;     // 交点を頂点として含めた線分の頂点群
    private GameObject LinesObj;                        // Lineをまとめる親
    public GameObject ParentObj;                        // 親となるオブジェクト
    public Material LineMaterial;                       // Lineのマテリアル
    public Color LineColor;                             // Lineの色
    float widthCoefficient = 0.1f;                      // Lineの幅係数
    public float lineWidth;                             // Lineの幅
    public GameObject Build;                            // 建物のオブジェクト
    public GameObject Buildings;                        // 建物群の親
    public int buildGenerateCount = 1000;               // 建物の生成試行回数
    private List<float> Build_proparty;                 // {x, z, x_size, z_size}
    private List<List<float>> Build_proparties;         // {x, z, x_size, z_size}の集まり
    private List<int> deleteManeger;                    // 消される建物の番号を登録
    private List<Vector3> StraightList;                 // 直線ツールの始点と終点
    public GameObject AreaObject;
    private List<GameObject> AreaObjects;                // 閉領域のオブジェクトリスト
    public Material AreaMaterial;

    bool straight_flag = true;                          // 直線ツールを使っているか
    public GameObject StraightRenderer;                 // 直線ツールの補助線
    private GameObject StraightObj;                     // 補助線のインスタンス

    public bool build_flag = false;                     // 建物生成が終わったか
    public GameObject CrossObj;                         // 交点確認用オブジェクト  

    public Material buildingMaterial;

    public GameObject CameraObj;
    private Camera camera;
    private Bitmap bitmap;
    public GameObject bit;
    private bool isWeightPainting = false;

    public GameObject Floor;                            // 床オブジェクト
    public GameObject Guide;                            // ガイドの床
    public GameObject ViewCamera;                       // 眺めるモードのカメラ

    private static IEnumerable<Area> areas;

    // デバッグ用
    public static bool isDataDebug = false;
    public static bool isAreaDebug = false;
    private static int rightKeyCount = 0;
    private static int upKeyCount = 0;
    private List<List<Vertex>> linesVertices;
    private static Vertex currentDebugVertex;
    private static bool isNeighbor = false;

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
        Vertex_and_Intersections = new List<List<VertexAttribute>>();
        AreaObjects = new List<GameObject>();
        List<List<Vertex>> linesVertices = new List<List<Vertex>>();
        StraightRenderer.GetComponent<LineRenderer>().startWidth = widthCoefficient * lineWidth;
        StraightRenderer.GetComponent<LineRenderer>().endWidth = widthCoefficient * lineWidth;
        // 曲線をまとめる親Obj
        LinesObj = Instantiate(ParentObj, new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity);
        LinesObj.name = "Lines";

        Buildings = Instantiate(ParentObj, new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity);

        camera = CameraObj.GetComponent<Camera>();
        bitmap = new Bitmap(50, 80, camera);
        bitmap.ViewBitParametor(bit);
        bitmap.WeightPaintMode(false);
        // DebugTest();
    }

    // Update is called once per frame
    void Update()
    {
        // タブキーでウェイトペイントモードに切り替え
        if (Input.GetKeyDown(KeyCode.Tab)) {
            isWeightPainting = !isWeightPainting;
            if (isWeightPainting == true) bitmap.WeightPaintMode(true);
            else bitmap.WeightPaintMode(false);
        }

        if (Input.GetMouseButton(0) && isWeightPainting == true)
        {
            Vector3 mouseScreenPosition = Input.mousePosition;
            mouseScreenPosition = Camera.main.ScreenToWorldPoint(mouseScreenPosition);
            Vector2 mouseWorldPosition = new Vector2(mouseScreenPosition.x, mouseScreenPosition.z);
            bitmap.UseBrush(mouseWorldPosition, 10f, 0.3f);
            
        }
        if (Input.GetMouseButtonDown(1) && isWeightPainting == true)
        {
            Vector3 mouseScreenPosition = Input.mousePosition;
            mouseScreenPosition = Camera.main.ScreenToWorldPoint(mouseScreenPosition);
            Vector2 mouseWorldPosition = new Vector2(mouseScreenPosition.x, mouseScreenPosition.z);
            Debug.Log("weight = " + bitmap.GetWeight(mouseWorldPosition));
        }

        if (Input.GetMouseButtonDown(0) && build_flag == false && isWeightPainting == false)
        {
            AddLineObject();
            if (straight_flag == true)
            {
                // 直線の始点を追加
                AddPositionDataToLineRendererList();

                // 直線の状態を見せるため
                StraightList = new List<Vector3>();
                StraightList.Add(Camera.main.ScreenToWorldPoint(new Vector3(Mathf.Floor(Input.mousePosition.x), Mathf.Floor(Input.mousePosition.y), 1.0f)));                
                StraightObj = Instantiate(StraightRenderer, new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity);

            }
        }

        // LineRendererに位置データを指定しておく
        if (Input.GetMouseButton(0) && build_flag == false && isWeightPainting == false)
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
                straightRender.SetPosition(1, Camera.main.ScreenToWorldPoint(new Vector3(Mathf.Floor(Input.mousePosition.x), Mathf.Floor(Input.mousePosition.y), 1.0f)));
            }
            
        }

        if (Input.GetMouseButtonUp(0) && build_flag == false && isWeightPainting == false)      // (建物が生成されている間は無効)
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
        if (Input.GetKeyDown(KeyCode.Keypad1) && build_flag == false && isWeightPainting == false)  // 1車線
        {
            widthCoefficient = 0.25f / 2.0f;
            // LineWidthText.GetComponent<Text>().text = "1車線";
        }

        if (Input.GetKeyDown(KeyCode.Keypad2) && build_flag == false && isWeightPainting == false)  // 2車線
        {
            widthCoefficient = 0.25f;
            // LineWidthText.GetComponent<Text>().text = "2車線";
        }

        if (Input.GetKeyDown(KeyCode.Keypad3) && build_flag == false && isWeightPainting == false)  // 3車線
        {
            widthCoefficient = 0.5f;
            // LineWidthText.GetComponent<Text>().text = "3車線";
        }

        if (Input.GetKeyDown(KeyCode.Keypad4) && build_flag == false && isWeightPainting == false)  // 4車線
        {
            widthCoefficient = 1.0f;
            // LineWidthText.GetComponent<Text>().text = "4車線";
        }

        // 建物生成
        if (Input.GetKeyDown(KeyCode.Space) && build_flag == false)
        {

            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();            
            
            linesVertices = new List<List<Vertex>>();
            foreach (List<Vector3> ls in Lines)
            {
                linesVertices.Add(InitDataStructure(ls));
            }

            foreach(List<Vertex> ls in linesVertices)
            {
                for(int i = 0; i < ls.Count; i++)
                {
                    if(i == 0)
                    {
                        ls[i].next = ls[i + 1];
                    }
                    else if (i == ls.Count - 1)
                    {
                        ls[i].prev = ls[i - 1];
                    }
                    else
                    {
                        ls[i].next = ls[i + 1];
                        ls[i].prev = ls[i - 1];
                    }
                }
            }

            // 交点列挙  
            List<Vector2> intersectionPoints = new List<Vector2>();
            List<Vertex> intersectionVertices = new List<Vertex>();
            for (int i = 0; i < Lines.Count; i++)
            {
                for (int k = 0; k < Lines[i].Count - 1; k++)
                {
                    Vector2 p0 = new Vector2(Lines[i][k].x, Lines[i][k].z);
                    Vector2 p1 = new Vector2(Lines[i][k + 1].x, Lines[i][k + 1].z);
                    LineSegment currentSegment = new LineSegment(p0, p1);

                    for (int j = 0; j < Lines.Count; j++)
                    {
                        for (int l = 0; l < Lines[j].Count - 1; l++)
                        {
                            Vector2 q0 = new Vector2(Lines[j][l].x, Lines[j][l].z);
                            Vector2 q1 = new Vector2(Lines[j][l + 1].x, Lines[j][l + 1].z);
                            LineSegment compareSegment = new LineSegment(q0, q1);

                            // 交点を見つけたら
                            if (currentSegment.Intersects(compareSegment) && currentSegment.isSameSegment(compareSegment) == false)
                            {
                                Vector2 coordinate = currentSegment.GetIntersectionPoint(compareSegment).Value;

                                // 同じ交点ならbreak
                                bool isSameIntersection = false;
                                foreach (Vector2 v in intersectionPoints)
                                {
                                    if (coordinate.Equals(v))
                                    {
                                        isSameIntersection = true;
                                    }
                                }
                                if (isSameIntersection == true)
                                {
                                    break;
                                }
                                // 繋がっててもbreak
                                if((k + 1 == l || l + 1 == k) && i == j)
                                {
                                    break;
                                }

                                Vertex intersectionPoint = new Vertex(new Vector3(coordinate.x, 0f, coordinate.y));
                                Vertex intersectionPointNeighbor = new Vertex(new Vector3(coordinate.x, 0f, coordinate.y));

                                // 交点のインデックスを保存
                                intersectionPoint.indexI = i;
                                intersectionPoint.indexJ = k;
                                intersectionPointNeighbor.indexI = j;
                                intersectionPointNeighbor.indexJ = l;

                                // p0またはq0との距離
                                intersectionPoint.distance = (coordinate - p0).SqrMagnitude();
                                intersectionPointNeighbor.distance = (coordinate - q0).SqrMagnitude();

                                // 交点同士を連結
                                intersectionPoint.neighbor = intersectionPointNeighbor;
                                intersectionPointNeighbor.neighbor = intersectionPoint;

                                // リスト追加
                                intersectionVertices.Add(intersectionPoint);
                                intersectionVertices.Add(intersectionPointNeighbor);
                                intersectionPoints.Add(coordinate);

                            }
                            
                        }
                    }
                }
            }

            // 交点リストのソート
            IOrderedEnumerable<Vertex> sortedIntersections = intersectionVertices.OrderBy(v => v.indexI).ThenBy(v => v.indexJ).ThenBy(v => v.distance);

            // 挿入            
            foreach(Vertex v in sortedIntersections)
            {
                Vertex currentVertex = linesVertices[v.indexI][v.indexJ];
                int safety = 0;
                while (true)
                {
                    // currentVertexの後ろが交点なら
                    if(currentVertex.next.neighbor != null)
                    {
                        currentVertex = currentVertex.next;
                    }
                    else
                    {
                        v.prev = currentVertex;
                        v.next = currentVertex.next;

                        currentVertex.next.prev = v;
                        currentVertex.next = v;
                        break;
                    }

                    safety++;
                    if(safety > 1000)
                    {
                        break;
                    }
                }
            }

            // 交点が2つ連続しているならば、その間に頂点を挿入
            for (int i = 0; i < linesVertices.Count; i++)
            {
                int safety = 0;
                Vertex currentVertex = linesVertices[i][0];
                while (true)
                {
                    // 端点にたどり着いたら終了
                    if(currentVertex.next != null)
                    {
                        // 今と次の頂点が交点の時
                        if(currentVertex.neighbor != null && currentVertex.next.neighbor != null)
                        {
                            Vector3 mid = (currentVertex.coordinate + currentVertex.next.coordinate) * 0.5f;
                            Vertex midVertex = new Vertex(mid);

                            midVertex.next = currentVertex.next;
                            midVertex.prev = currentVertex;

                            currentVertex.next.prev = midVertex;
                            currentVertex.next = midVertex;
                        }
                        else
                        {
                            currentVertex = currentVertex.next;
                        }
                    }
                    else
                    {
                        break;
                    }

                    safety++;
                    if(safety > 1000)
                    {
                        break;
                    }
                }
            }

            currentDebugVertex = linesVertices[0][0];
            // 閉領域検出
            List<Area> areaList = GetAreas(linesVertices);
            areas = areaList.Distinct(new AreaCompare()).ToList();
            // Debug.Log(areas.Count());

            // 反時計回りにソート
            foreach(Area a in areas)
            {
                a.CCWSort();
                // foreach (Vertex v in a.vertices) Debug.Log(v.coordinate);
                // Debug.Log("------");
            }

            // AreaクラスをPolygonクラスに変換
            List<Polygon> polygons = new List<Polygon>();
            foreach(Area a in areas)
            {
                polygons.Add(a.ConvertToPolygon());
            }
            foreach(var v in polygons)
            {
                // v.DrawPolygon(Color.white);
            }

            List<List<Cell>> cellsList = new List<List<Cell>>();
            // 面積を計算して、その大きさに応じた母点数で分割
            // ボロノイ分割            
            foreach(Polygon p in polygons)
            {
                // Debug.Log("!");
                // float area = p.GetArea();
                // Debug.Log("area = " + area);
                // 100m * 100mで建物10個くらい　1000m^2で1個
                List<Cell> cells = new List<Cell>();
                bool mustContinue = false;
                int safety = 0;
                while (true)
                {
                    cells = new IncrementalVoronoi().Execute(p, 6, bitmap);
                    foreach (Cell c in cells)
                    {
                        if (c.GetVerts().Count < 3 || c.GetEdges().Count < 3) mustContinue = true;
                    }
                    if (mustContinue == false) break;
                    if (safety > 10000)
                    {
                        Debug.Log("アカンかったわ");
                        break;
                    }
                    safety++;
                }

                // Debug.Log(cells.Count);

                //foreach (Cell c in cells) {
                //    c.ViewParametor();
                //    c.DrawCell(Color.white);
                //}

                // foreach (Cell c in cells) Instantiate(CrossObj, new Vector3(c.GetCellPos().x, 300f, c.GetCellPos().y), Quaternion.identity);
                
                cellsList.Add(cells);                
            }            
            
            // GreinerHormannクリッピング
            List<List<Polygon>> buildingFloors = new List<List<Polygon>>();            
            for(int i = 0; i < cellsList.Count; i++)
            {
                List<Cell> list = cellsList[i];
                foreach(Cell c in list)
                {
                    //foreach(Vector2 v in c.GetVerts())
                    //{
                    //    Debug.Log(v);
                    //}
                    //Debug.Log("-------");
                    Polygon polygon = c.ConvertToPolygon();
                    // polygon = polygon.Manhattan();
                    List<List<Vector2>> clippedPolygons = new GreinerHormann().Execute(polygon.GetVertices(), polygons[i].GetVertices());
                    List<Polygon> polyList = new List<Polygon>();
                    foreach(List<Vector2> l in clippedPolygons)
                    {
                        Polygon poly = new Polygon(l);
                        // Polygon manhattanPoly = poly.Manhattan();
                        polyList.Add(poly);
                        // poly = poly.Manhattan();
                        // poly.DrawPolygon(new Color(Random.value, Random.value, Random.value, 1f));
                    }
                    buildingFloors.Add(polyList);
                }
            }
            /*
            foreach (List<Polygon> list in buildingFloors)
            {
                foreach (Polygon p in list)
                {
                    //p.ReductionPolygons();
                    p.DrawPolygon(Color.blue);
                    // p.DrawPolygon(Color.white);
                }
            }
            */
            // ポリゴン縮小
            foreach (List<Polygon> list in buildingFloors)
            {
                foreach(Polygon p in list)
                {
                    p.ReductionPolygons();
                    // p.DrawPolygon(Color.white);
                    // p.DrawPolygon(Color.white);
                }
            }

            // 建物生成
            GenerateBuildings(buildingFloors);

        
            build_flag = true;                   
            // 床と眺めるカメラを設置
            Guide.SetActive(false);
            Floor.SetActive(true);
            ViewCamera.SetActive(true);

            sw.Stop();
            // Debug.Log(sw.ElapsedMilliseconds + "ms");

        }

        // 線分データ構造確認
        if (Input.GetKeyDown(KeyCode.D))
        {
            isDataDebug = !isDataDebug;
            isAreaDebug = !isAreaDebug;
            if (isDataDebug == true)
            {
                Debug.Log("デバッグ中だぞ");
            }
        }
        if (Input.GetKeyDown(KeyCode.RightArrow) && isAreaDebug == true)
        {
            // DataStructureTest(linesVertices, KeyCode.RightArrow);
            AreaTest(areas.ToList(), KeyCode.RightArrow);
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow) && isDataDebug == true)
        {
            DataStructureTest(linesVertices, KeyCode.LeftArrow);
        }
        if (Input.GetKeyDown(KeyCode.UpArrow) && isAreaDebug == true)
        {
            // DataStructureTest(linesVertices, KeyCode.UpArrow);
            AreaTest(areas.ToList(), KeyCode.UpArrow);
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

    private void DebugTest()
    {
        List<Vector3> line = new List<Vector3>();
        line.Add(new Vector3(-200f, 0f, 50f));
        line.Add(new Vector3(200f, 0f, 50f));
        Lines.Add(line);
        line = new List<Vector3>();

        line.Add(new Vector3(-100f, 0f, 200f));
        line.Add(new Vector3(-100f, 0f, -200f));
        Lines.Add(line);
        line = new List<Vector3>();

        line.Add(new Vector3(-200f, 0f, -50f));
        line.Add(new Vector3(200f, 0f, -50f));
        Lines.Add(line);
        line = new List<Vector3>();

        line.Add(new Vector3(0f, 0f, 200f));
        line.Add(new Vector3(0f, 0f, -200f));
        Lines.Add(line);
        line = new List<Vector3>();

        line.Add(new Vector3(100f, 0f, 200f));
        line.Add(new Vector3(100f, 0f, -200f));
        Lines.Add(line);
        
        for(int i = 0; i < Lines.Count; i++)
        {
            for(int j = 0; j < Lines[i].Count - 1; j++)
            {
                Debug.DrawLine(Lines[i][j], Lines[i][j + 1], Color.white, 100000f);
            }
        }        
    }

    private void DataStructureTest(List<List<Vertex>> linesVertices, KeyCode key)
    {
        Debug.Log(key);
        // 初回
        if(GameObject.Find("current") == false)
        {
            GameObject obj = Instantiate(CrossObj, currentDebugVertex.coordinate, Quaternion.identity);
            obj.name = "current";
            Debug.Log("next = " + currentDebugVertex.next + "  , prev = " + currentDebugVertex.prev + "  , neighbor = " + currentDebugVertex.neighbor);
        }
        else
        {
            if(key == KeyCode.RightArrow && currentDebugVertex.next != null)
            {
                GameObject.Find("current").transform.position = currentDebugVertex.next.coordinate;                
                currentDebugVertex = currentDebugVertex.next;
                Debug.Log("next = " + currentDebugVertex.next + "  , prev = " + currentDebugVertex.prev + "  , neighbor = " + currentDebugVertex.neighbor);
            }

            if(key == KeyCode.LeftArrow && currentDebugVertex.prev != null)
            {
                GameObject.Find("current").transform.position = currentDebugVertex.prev.coordinate;
                currentDebugVertex = currentDebugVertex.prev;
                Debug.Log("next = " + currentDebugVertex.next + "  , prev = " + currentDebugVertex.prev + "  , neighbor = " + currentDebugVertex.neighbor);                
            }

            if(key == KeyCode.UpArrow && currentDebugVertex.neighbor != null)
            {
                Debug.Log("next = " + currentDebugVertex.next + "  , prev = " + currentDebugVertex.prev + "  , neighbor = " + currentDebugVertex.neighbor);
                currentDebugVertex = currentDebugVertex.neighbor;                
                isNeighbor = !isNeighbor;
                GameObject.Find("current").GetComponent<Renderer>().material.color = isNeighbor ? Color.white : Color.red;
            }
        }
    }

    private void AreaTest(List<Area> areas, KeyCode key)
    {        
        // 初回
        if (GameObject.Find("current") == false)
        {
            GameObject obj = Instantiate(CrossObj, areas[0].vertices[0].coordinate, Quaternion.identity);
            obj.name = "current";
            // Debug.Log("next = " + currentDebugVertex.next + "  , prev = " + currentDebugVertex.prev + "  , neighbor = " + currentDebugVertex.neighbor);
        }
        else
        {
            if (key == KeyCode.RightArrow)
            {
                GameObject.Find("current").transform.position = areas[upKeyCount].vertices[(rightKeyCount + areas[upKeyCount].vertices.Count + 1) % areas[upKeyCount].vertices.Count].coordinate;
                rightKeyCount++;
                // Debug.Log("next = " + currentDebugVertex.next + "  , prev = " + currentDebugVertex.prev + "  , neighbor = " + currentDebugVertex.neighbor);
            }

            if(key == KeyCode.UpArrow)
            {
                GameObject.Find("current").transform.position = areas[(upKeyCount + areas.Count + 1) % areas.Count].vertices[0].coordinate;
                upKeyCount++;
                rightKeyCount = 0;
                GameObject.Find("current").GetComponent<Renderer>().material.color = new Color(Random.value, Random.value, Random.value);
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
        Vector3 screenPosition01 = new Vector3(Mathf.Floor(Input.mousePosition.x), Mathf.Floor(Input.mousePosition.y), 1.0f);
        Vector3 mousePosition = new Vector3(Camera.main.ScreenToWorldPoint(screenPosition01).x, 0f, Camera.main.ScreenToWorldPoint(screenPosition01).z);
        // Debug.Log(mousePosition);

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

            Cross.Add(new Vector3(x, 9.0f, y));

        }
    }

    private List<Vertex> InitDataStructure(List<Vector3> lineVector)
    {
        List<Vertex> result = new List<Vertex>();
        for(int i = 0; i < lineVector.Count; i++)
        {
            result.Add(new Vertex(lineVector[i]));
        }

        // 連結(端点ではnextかprevがnull)
        for (int i = 0; i < result.Count; i++)
        {
            result[i].next = (i != result.Count - 1) ? result[i + 1] : null;
            result[i].prev = (i != 0) ? result[i - 1] : null;
        }

        return result;
    }

    private List<Area> GetAreas(List<List<Vertex>> linesVertices)
    {
        int trails = 0;
        List<Area> result = new List<Area>();
        for(int i = 0; i < linesVertices.Count; i++)
        {
            Vertex startVertex = linesVertices[i][0];   // 捜査開始点
            int safety = 0;
            while (true)
            {
                // currentVertexのisFirstPointがfalseになってるぅ
                // Debug.Log("<color=green>" + startVertex.coordinate + "</color>");
                // 開始点が交点でなければ捜査開始
                if (startVertex.neighbor == null && startVertex.next != null && startVertex.prev!= null)
                {                    
                    // 開始点から前に進むケースと後ろに進むケースを考える
                    bool isMovingForward = true;
                    for(int j = 0; j < 2; j++)
                    {
                        trails++;
                        isMovingForward = j == 0 ? true : false;
                        // 初期化
                        foreach (List<Vertex> ls in linesVertices)
                        {
                            foreach (Vertex v in ls)
                            {
                                Vertex defaultVertex = v;
                                while(true)
                                {
                                    defaultVertex.isFirstPoint = false;
                                    defaultVertex.isTakenByPolygon = false;
                                    if (defaultVertex.next != null) defaultVertex = defaultVertex.next;
                                    else break;
                                }
                            }
                        }

                        bool mustContinue = true;
                        bool isSuccess = false;
                        List<Vertex> areaVertices = new List<Vertex>();

                        Vertex currentVertex = new Vertex(startVertex);
                        Vector3 startCoordinate = new Vector3(startVertex.coordinate.x, startVertex.coordinate.y, startVertex.coordinate.z);
                        currentVertex.isFirstPoint = true;
                        // Debug.Log("<color=red>" + currentVertex.coordinate + "</color>");
                        currentVertex.isTakenByPolygon = true;
                        areaVertices.Add(currentVertex);

                        // 次点の決定
                        if ((isMovingForward == true && currentVertex.next == null) || (isMovingForward == false && currentVertex.prev == null))
                        {
                            continue;
                        }
                        else
                        {
                            currentVertex = isMovingForward == true ? currentVertex.next : currentVertex.prev;
                        }

                        int safetyA = 0;
                        // 交点or端点があるまで進む
                        while (true)
                        {
                            // 交点の場合
                            if(currentVertex.neighbor != null)
                            {
                                currentVertex.isTakenByPolygon = true;
                                currentVertex.neighbor.isTakenByPolygon = true;
                                areaVertices.Add(currentVertex);
                                break;
                            }
                            // 端点の場合
                            else if((isMovingForward == true && currentVertex.next == null) || (isMovingForward == false && currentVertex.prev == null))
                            {
                                mustContinue = false;
                                break;
                            }
                            // それ以外(普通の点)
                            else
                            {
                                currentVertex.isTakenByPolygon = true;
                                areaVertices.Add(currentVertex);
                                currentVertex = isMovingForward == true ? currentVertex.next : currentVertex.prev;
                            }

                            safetyA++;
                            if (safetyA > 100000) { Debug.Log("無限ループだぞ"); mustContinue = false; break; }
                        }

                        // 交点の検出に成功していたら
                        if (mustContinue == true)
                        {
                            int safetyB = 0;
                            // [左折してその道をたどる]を繰り返す
                            while (true)
                            {
                                // 左折
                                Vertex prevVertex = isMovingForward == true ? currentVertex.prev : currentVertex.next;
                                List<Vertex> nextVertices = new List<Vertex>();     // 候補となる3方向
                                for(int k = 0; k < 3; k++)
                                {
                                    if(k == 0)
                                    {
                                        nextVertices.Add(isMovingForward == true ? currentVertex.next : currentVertex.prev);
                                    }
                                    else if(k == 1 && currentVertex.neighbor.next != null)
                                    {
                                        nextVertices.Add(currentVertex.neighbor.next);
                                    }
                                    else if(k == 2 && currentVertex.neighbor.prev != null)
                                    {
                                        nextVertices.Add(currentVertex.neighbor.prev);
                                    }                                   
                                }

                                // (prev → current)ベクトルと(current → next)の外積値を格納(正規化してね)
                                // ここに問題点
                                Vector3 pc = (currentVertex.coordinate - prevVertex.coordinate).normalized;
                                foreach(Vertex v in nextVertices)
                                {
                                    v.isTakenByPolygon = true;
                                    Vector3 cn = (v.coordinate - currentVertex.coordinate).normalized;
                                    v.cross = pc.x * cn.z - pc.z * cn.x;
                                }

                                // 外積値でソート(最大, 最小, 中間でソート)    
                                CrossSort(nextVertices);
                                // IOrderedEnumerable<Vertex> sortedNextVertices = nextVertices.OrderByDescending(v => v.cross);

                                // 候補に始点があれば
                                foreach (Vertex v in nextVertices)
                                {
                                    // Debug.Log(v.coordinate + " : " + v.isFirstPoint);
                                    if (v.coordinate == startCoordinate)
                                    {
                                        // Debug.Log("!!!");
                                        // areaVertices.Add(v);
                                        mustContinue = false;
                                        isSuccess = true;
                                        break;
                                    }
                                }
                                if (mustContinue == false) break;

                                // 候補点をたどる                                
                                for (int k = 0; k < nextVertices.Count() - 1 ; k++)
                                {
                                    List<Vertex> preAddList = new List<Vertex>();
                                    bool mustForrowing = true;                                    
                                    Vertex preVertex = nextVertices[k];
                                    preAddList.Add(preVertex);                                    
                                    // 前進するか後退するか
                                    if((preVertex.coordinate == currentVertex.next.coordinate) || (preVertex.coordinate == currentVertex.neighbor.next.coordinate))
                                    {
                                        isMovingForward = true;
                                    }
                                    else
                                    {
                                        isMovingForward = false;
                                    }

                                    if ((isMovingForward == true && preVertex.next == null) || (isMovingForward == false && preVertex.prev == null)) continue;
                                    Vertex nextVertex = isMovingForward == true ? preVertex.next : preVertex.prev;
                                    int safetyC = 0;

                                    // たどる
                                    while (true)
                                    {
                                        // 始点に戻ったら
                                        // Debug.Log(nextVertex.coordinate +" : " + nextVertex.isFirstPoint);
                                        if(nextVertex.coordinate == startCoordinate)
                                        {
                                            // Debug.Log("!!!");
                                            mustForrowing = false;
                                            mustContinue = false;
                                            isSuccess = true;
                                            foreach (Vertex v in preAddList)
                                            {
                                                areaVertices.Add(v);
                                            }
                                            break;
                                            // preAddList.Add(nextVertex);
                                        }
                                        // 一度たどった道ならば
                                        else if(nextVertex.isTakenByPolygon == true)
                                        {
                                            // Debug.Log("??" + nextVertex.coordinate);
                                            mustForrowing = false;
                                            mustContinue = false;
                                            isSuccess = false;
                                            break;
                                        }
                                        // 交点なら
                                        else if(nextVertex.neighbor != null)
                                        {
                                            nextVertex.isTakenByPolygon = true;
                                            nextVertex.neighbor.isTakenByPolygon = true;
                                            preAddList.Add(nextVertex);

                                            currentVertex = nextVertex;                                            
                                            mustForrowing = false;

                                            foreach (Vertex v in preAddList)
                                            {
                                                areaVertices.Add(v);
                                            }
                                            break;
                                        }
                                        // 端点なら
                                        else if ((isMovingForward == true && nextVertex.next == null) || (isMovingForward == false && nextVertex.prev == null))
                                        {                                            
                                            if (k == nextVertices.Count - 2)
                                            {
                                                mustForrowing = false;
                                                mustContinue = false;
                                                isSuccess = false;
                                                Debug.Log("!");
                                            }
                                            break;
                                        }
                                        // それ以外(普通の点)
                                        else
                                        {
                                            nextVertex.isTakenByPolygon = true;
                                            preAddList.Add(nextVertex);
                                            nextVertex = isMovingForward == true ? nextVertex.next : nextVertex.prev;
                                        }
                                        safetyC++;
                                        if (safetyC > 100) { Debug.Log("無限ループだぞ"); mustContinue = false; break; }
                                    }

                                    if(mustForrowing == false) break;
                                }

                                // 閉領域検出が成功or失敗したらぬける
                                if (mustContinue == false) break;
                                
                                safetyB++;
                                // ここです...
                                if (safetyB > 100) { Debug.Log("無限ループだぞ"); mustContinue = false; break; }
                            }


                        }
                        else continue;

                        // 閉領域検出が成功していたら
                        if (isSuccess == true)
                        {
                            // areaVertices.RemoveAt(areaVertices.Count - 1);
                            Vector3 position = new Vector3(0f, 0f, 0f);
                            foreach(Vertex v in areaVertices)
                            {
                                position.x += Mathf.FloorToInt(v.coordinate.x);
                                position.z += Mathf.FloorToInt(v.coordinate.z);
                            }
                            result.Add(new Area { pos = position, vertices = areaVertices });
                        }
                        
                    }
                    
                }              
                // 次点がなければ終了
                if (startVertex.next == null)
                {
                    // Debug.Log("<color=blue>" + "あああああ" + "</color>");
                    break;
                } 
                else
                {                    
                    startVertex = startVertex.next;
                    // Debug.Log("<color=blue>" + "いいいいい" + startVertex.coordinate + "</color>");
                }
                // Debug.Log(safety);
                safety++;
                if (safety > 10000) { Debug.Log("無限ループだぞ"); break; }
            }
        }
        // Debug.Log("success = " + successCnt);
        // Debug.Log("trails = " + trails);
        return result;
    }

    private void CrossSort(List<Vertex> crossList)
    {
        float crossMin = float.PositiveInfinity;
        float crossMax = float.NegativeInfinity;
        Vertex minVertex = null;
        Vertex maxVertex = null;

        // 最小と最大を決める
        foreach(Vertex v in crossList)
        {
            if(v.cross < crossMin)
            {
                crossMin = v.cross;
                minVertex = v;
            }
            if(v.cross > crossMax)
            {
                crossMax = v.cross;
                maxVertex = v;
            }
        }

        // 最小と最大を取り除く
        crossList.Remove(minVertex);
        crossList.Remove(maxVertex);

        Vertex midVertex = null;
        if(crossList.Count != 0)
        {
            midVertex = crossList[0];
        }
        crossList.Clear();

        crossList.Add(maxVertex);
        crossList.Add(minVertex);
        if(midVertex != null)
        {
            crossList.Add(midVertex);
        }
    }    

    private void GenerateBuildings(List<List<Polygon>> polygonsList)
    {
        foreach(List<Polygon> list in polygonsList)
        {
            foreach(Polygon polygon in list)
            {
                CreateBuildingMesh(polygon);
                // Debug.Log("area = " + polygon.GetArea());
            }
        }
    }

    private void CreateBuildingMesh(Polygon polygon)
    {
        if (polygon.GetVertices().Count < 3) return;
        GameObject obj = new GameObject("Building", new[] { typeof(MeshFilter), typeof(MeshRenderer) });
        // polygon.DrawPolygon(Color.red);

        // 1m : 0.01
        float scale = 0.1f;
        float area = GetArea(polygon) *scale;
        // Debug.Log(area);
        float height = 0f;
        if (area <= 60f) height = Random.Range(10f, 50f);
        else if (area <= 100f) height = Random.Range(10f, 110f);
        else height = Random.Range(70f, 110f);

        List<Polygon> roof = new SplitTriangles().Excecute(polygon);
        List<Vector3> meshVertex = new List<Vector3>();        
        List<int> meshIndex = new List<int>();          // 壁、屋根のインデックス
        // polygon.ViewParametor();

        // 壁の頂点の追加、インデックスの設定
        int size = polygon.GetVertices().Count;
        for (int i = 0; i < size; i++)
        {
            
            meshVertex.Add(new Vector3(polygon.GetVertices()[i].x, 0f, polygon.GetVertices()[i].y));
            meshVertex.Add(new Vector3(polygon.GetVertices()[i].x, height, polygon.GetVertices()[i].y));
            meshVertex.Add(new Vector3(polygon.GetVertices()[(i + 1) % size].x, 0f, polygon.GetVertices()[(i + 1) % size].y));

            meshVertex.Add(new Vector3(polygon.GetVertices()[(i + 1) % size].x, 0f, polygon.GetVertices()[(i + 1) % size].y));
            meshVertex.Add(new Vector3(polygon.GetVertices()[i].x, height, polygon.GetVertices()[i].y));
            meshVertex.Add(new Vector3(polygon.GetVertices()[(i + 1) % size].x, height, polygon.GetVertices()[(i + 1) % size].y));

            meshIndex.Add(i * 6);
            meshIndex.Add((i * 6) + 1);
            meshIndex.Add((i * 6) + 2);            

            meshIndex.Add((i * 6) + 3);
            meshIndex.Add((i * 6) + 4);
            meshIndex.Add((i * 6) + 5); 
            
        }
        //for (int i = 0; i < size; i++)
        //{
        //    meshVertex.Add(new Vector3(polygon.GetVertices()[i].x, 0f, polygon.GetVertices()[i].y));
        //    meshVertex.Add(new Vector3(polygon.GetVertices()[i].x, height, polygon.GetVertices()[i].y));

        //    meshIndex.Add((i * 2) % (size * 2));           // 0            
        //    meshIndex.Add((i * 2 + 1) % (size * 2));       // 1
        //    meshIndex.Add((i * 2 + 2) % (size * 2));       // 2

        //    meshIndex.Add((i * 2 + 1) % (size * 2));       // 1
        //    meshIndex.Add((i * 2 + 3) % (size * 2));       // 3
        //    meshIndex.Add((i * 2 + 2) % (size * 2));       // 2

        //}

        // 屋根の頂点の追加、インデックスの追加
        for (int i = 0; i < roof.Count; i++)
        {
            List<int> roofIndex = new List<int>();
            for (int j = 0; j < roof[i].GetVertices().Count; j++)
            {
                meshVertex.Add(new Vector3(roof[i].GetVertex(j).x, height, roof[i].GetVertex(j).y));                
                roofIndex.Add(meshVertex.Count - 1);
            }
            roofIndex.Reverse();
            foreach(int j in roofIndex)
            {
                meshIndex.Add(j);
            }
        }

        Mesh mesh = new Mesh();
        mesh.vertices = meshVertex.ToArray();
        mesh.triangles = meshIndex.ToArray();

        mesh.RecalculateNormals();

        var filter = obj.GetComponent<MeshFilter>();
        filter.sharedMesh = mesh;

        var renderer = obj.GetComponent<MeshRenderer>();
        renderer.material = buildingMaterial;

        //// 法線ベクトルの再計算処理
        //mesh.RecalculateNormals();
        //MeshFilter filter = obj.GetComponent<MeshFilter>();
        //// mesh.normals = polygonVertex.ToArray();
        //obj.GetComponent<MeshRenderer>().material = buildingMaterial;
        //filter.mesh = mesh;

    }
    // 多角形の面積を計算
    public float GetArea(Polygon polygon)
    {
        List<Vector2> vertices = new List<Vector2>(polygon.GetVertices());
        float area = 0f;
        if(polygon.GetVertices().Count > 3)
        {
            List<Polygon> triangles = new SplitTriangles().Excecute(new Polygon(vertices));
            foreach (Polygon p in triangles)
            {
                Vector2 AB = p.GetVertex(1) - p.GetVertex(0);
                Vector2 AC = p.GetVertex(2) - p.GetVertex(0);
                area += Mathf.Abs(GeomUtils.Cross(AB, AC) * 0.5f);
            }
        }        
        return area;
    }
    // 閉領域を検出
    List<List<VertexAttribute>> AriaSerch(List<List<VertexAttribute>> vertex)
    {

        List<List<VertexAttribute>> PreAreas = new List<List<VertexAttribute>>();

        for(int i = 0; i < vertex.Count; i++)
        {
            for (int j = 0; j < vertex[i].Count; j++)
            {
                // 始点を探す
                VertexAttribute startpoint = vertex[i][j];
                // 次点リスト
                List<VertexAttribute> nextpoints = new List<VertexAttribute>();


                if (startpoint.attribute == "cross")
                {

                    // Debug.Log("now : " + startpoint.coodi);
                    int nextpointindex_i = i;
                    int nextpointindex_j = j;
                    int nextcount = 1;
                    int nextnextcount = 1;
                    bool minusflag = false;

                    // 始点が交点にだったら，たどる方向を決める(必ず+X側)
                    IntPair startPair = SerchCrossPair(startpoint, i, j, vertex);

                    List<VertexAttribute> courseFirstCandi = new List<VertexAttribute>();
                    if(startPair.j + 1 < vertex[startPair.i].Count)
                    {
                        courseFirstCandi.Add(vertex[startPair.i][startPair.j + 1]);
                    }
                    if(startPair.j - 1 >= 0)
                    {
                        courseFirstCandi.Add(vertex[startPair.i][startPair.j - 1]);
                    }
                    if(j + 1 < vertex[i].Count)
                    {
                        courseFirstCandi.Add(vertex[i][j + 1]);
                    }
                    if(j - 1 >= 0)
                    {
                        courseFirstCandi.Add(vertex[i][j - 1]);
                    }

                    VertexAttribute courseSecondCandi = null;

                    // (1.0, 0.0)との最大内積
                    float coursedotMax = 0.0f;
                    // 基点ベクトル
                    Vector2 baseVector = new Vector2(1.0f, 0.0f);

                    for(int k = 0; k < courseFirstCandi.Count; k++)
                    {
                        float course_x = courseFirstCandi[k].coodi.x - vertex[i][j].coodi.x;
                        float course_y = courseFirstCandi[k].coodi.z - vertex[i][j].coodi.z;
                        float course_length = Mathf.Sqrt(course_x * course_x + course_y * course_y);
                        Vector2 courseVector = new Vector2(course_x / course_length, course_y / course_length);

                        // 内積
                        float coursedot = baseVector.x * courseVector.x + baseVector.y * courseVector.y;

                        if (k == 0)
                        {
                            coursedotMax = coursedot;
                            courseSecondCandi = courseFirstCandi[k];
                        }
                        else if(coursedot > coursedotMax)
                        {
                            coursedotMax = coursedot;
                            courseSecondCandi = courseFirstCandi[k];
                        }
                    }

                    // 進む方向の確認
                    if(startPair.j + 1 < vertex[startPair.i].Count)
                    {
                        if(courseSecondCandi.coodi == vertex[startPair.i][startPair.j + 1].coodi)
                        {
                            nextpointindex_i = startPair.i;
                            nextpointindex_j = startPair.j;
                        }
                    }

                    if(0 <= startPair.j - 1)
                    {
                        if(courseSecondCandi.coodi == vertex[startPair.i][startPair.j - 1].coodi)
                        {
                            nextpointindex_i = startPair.i;
                            nextpointindex_j = startPair.j;
                            minusflag = true;
                        }
                    }

                    if(j + 1 < vertex[i].Count)
                    {
                        if(courseSecondCandi.coodi == vertex[i][j + 1].coodi)
                        {
                            nextpointindex_i = i;
                            nextpointindex_j = j;
                        }
                    }

                    if(0 <= j - 1)
                    {
                        if(courseSecondCandi.coodi == vertex[i][j - 1].coodi)
                        {
                            nextpointindex_i = i;
                            nextpointindex_j = j;
                            minusflag = true;
                        }
                    }

                    // 隣接する交点を探す
                    for (; ; )
                    {
                        // 前週で crossIndex.j - 1が勝っていた場合，今週は負をたどる
                        if(minusflag == true)
                        {
                            nextcount = Mathf.Abs(nextcount) * -1;
                            nextnextcount = -1;
                        }
                        else
                        {
                            nextcount = Mathf.Abs(nextcount);
                            nextnextcount = 1;
                        }

                        
                        // 次点候補
                        VertexAttribute nextpoint = vertex[nextpointindex_i][nextpointindex_j + nextcount];

                        // 進めていって，始点にたどり着いたら，閉領域検出 始点を変える
                        if (nextpoint.coodi == startpoint.coodi)
                        {
                            // Debug.Log("!!");
                            List<VertexAttribute> Area = new List<VertexAttribute>();
                            Area.Add(startpoint);
                            for(int l = 0; l < nextpoints.Count; l++)
                            {
                                Area.Add(nextpoints[l]);
                            }
                            PreAreas.Add(Area);
                            break;
                        }

                        // 交点でも端点でもない点なら，次点リストに追加 一歩進む
                        if(nextpoint.attribute == "other")
                        {
                            nextpoints.Add(nextpoint);
                            nextcount = Mathf.Abs(nextcount) + 1;
                            // Debug.Log(2);
                        }
                        // 交点を見つけたら，右折する(例外アリ)
                        else if (nextpoint.attribute == "cross")
                        {
                            // すでに通った交点なら失敗
                            bool breakflag = false;
                            for(int l = 0; l < nextpoints.Count; l++)
                            {
                                if(nextpoint.coodi == nextpoints[l].coodi)
                                {
                                    breakflag = true;
                                    break;
                                }
                            }
                            if(breakflag == true)
                            {
                                break;
                            }

                            // 次点リストに追加
                            nextpoints.Add(nextpoint);
                            // 交点のペアのインデックスを探す
                            IntPair crossIndex = SerchCrossPair(nextpoint, nextpointindex_i, nextpointindex_j + nextcount, vertex);
                            // T字路やスクランブル交差点(無理)に対応したい
                            // 次次点候補リスト作成(右 or まっすぐ or 左)
                            List<VertexAttribute> nextpointFirstCandi = new List<VertexAttribute>();
                            if(crossIndex.j + 1 < vertex[crossIndex.i].Count)
                            {
                                nextpointFirstCandi.Add(vertex[crossIndex.i][crossIndex.j + 1]);
                            }
                            if(crossIndex.j - 1 >= 0)
                            {
                                nextpointFirstCandi.Add(vertex[crossIndex.i][crossIndex.j - 1]);
                            }
                            if(nextpointindex_j + nextcount + nextnextcount < vertex[nextpointindex_i].Count)
                            {
                                nextpointFirstCandi.Add(vertex[nextpointindex_i][nextpointindex_j + nextcount + nextnextcount]);
                            }

                            // ファーストステージ
                            bool area_flag = false;
                            List<VertexAttribute> nextpointSecondCandi = new List<VertexAttribute>();
                            // 各候補の次交点が始点なら領域検出，端点なら除外, 交点なら次のステージへ
                            for (int l = 0; l < nextpointFirstCandi.Count; l++)
                            {
                                
                                // vertex[crossIndex.i][crossIndex.j - 1] 以外は+1ずつ進める
                                // vertex[crossIndex.i][crossIndex.j + 1] 方向
                                if (crossIndex.j + 1 < vertex[crossIndex.i].Count)
                                {
                                    if (nextpointFirstCandi[l].coodi == vertex[crossIndex.i][crossIndex.j + 1].coodi)
                                    {
                                        
                                        for (int p = 1; crossIndex.j + p < vertex[crossIndex.i].Count; p++)
                                        {
                                            List<VertexAttribute> Prenextpoints = new List<VertexAttribute>();

                                            // 始点なら領域検出(要脱出)
                                            if (vertex[crossIndex.i][crossIndex.j + p].coodi == startpoint.coodi)
                                            {
                                                List<VertexAttribute> Area = new List<VertexAttribute>();
                                                Area.Add(startpoint);
                                                for (int u = 0; u < nextpoints.Count; u++)
                                                {
                                                    Area.Add(nextpoints[u]);
                                                }
                                                for (int u = 0; u < Prenextpoints.Count; u++)
                                                {
                                                    // 仮置きした分も入れなきゃね
                                                    Area.Add(Prenextpoints[u]);
                                                }
                                                // Debug.Log(3);
                                                area_flag = true;
                                                PreAreas.Add(Area);
                                                break;
                                            }
                                            // 普通の点なら，Prenextpointsに一旦保存
                                            else if (vertex[crossIndex.i][crossIndex.j + p].attribute == "other")
                                            {
                                                Prenextpoints.Add(vertex[crossIndex.i][crossIndex.j + p]);
                                            }
                                            // 交点ならば、次のステージへ
                                            else if (vertex[crossIndex.i][crossIndex.j + p].attribute == "cross" || vertex[crossIndex.i][crossIndex.j + p].attribute == "ep")
                                            {
                                                nextpointSecondCandi.Add(vertex[crossIndex.i][crossIndex.j + 1]);
                                                break;
                                            }
                                            
                                        }
                                    }
                                }

                                // vertex[crossIndex.i][crossIndex.j - 1] 方向(-1ずつすすむ)
                                if (crossIndex.j - 1 >= 0)
                                {
                                    if (nextpointFirstCandi[l].coodi == vertex[crossIndex.i][crossIndex.j - 1].coodi)
                                    {
                                        
                                        for (int p = -1; crossIndex.j + p >= 0; p--)
                                        {
                                            List<VertexAttribute> Prenextpoints = new List<VertexAttribute>();

                                            // 始点なら領域検出
                                            // Debug.Log("crossIndex.j + p : " + crossIndex.j + p);
                                            if (vertex[crossIndex.i][crossIndex.j + p].coodi == startpoint.coodi)
                                            {
                                                List<VertexAttribute> Area = new List<VertexAttribute>();
                                                Area.Add(startpoint);
                                                for (int u = 0; u < nextpoints.Count; u++)
                                                {
                                                    Area.Add(nextpoints[u]);
                                                }
                                                for (int u = 0; u < Prenextpoints.Count; u++)
                                                {
                                                    // 仮置きした分も入れなきゃね
                                                    Area.Add(Prenextpoints[u]);
                                                }
                                                // Debug.Log(4);
                                                area_flag = true;
                                                PreAreas.Add(Area);
                                                break;
                                            }
                                            // 普通の点なら，Prenextpointsに一旦保存
                                            else if (vertex[crossIndex.i][crossIndex.j + p].attribute == "other")
                                            {
                                                Prenextpoints.Add(vertex[crossIndex.i][crossIndex.j + p]);
                                            }
                                            // 交点ならば、次のステージへ
                                            else if (vertex[crossIndex.i][crossIndex.j + p].attribute == "cross" || vertex[crossIndex.i][crossIndex.j + p].attribute == "ep")
                                            {
                                                nextpointSecondCandi.Add(vertex[crossIndex.i][crossIndex.j - 1]);
                                                break;
                                            }
                                            
                                        }
                                    }
                                }

                                // vertex[i][j + k + 1] 方向
                                if ((0 <= nextpointindex_j + nextcount + nextnextcount) && (nextpointindex_j + nextcount + nextnextcount < vertex[i].Count))
                                {
                                    if (nextpointFirstCandi[l].coodi == vertex[nextpointindex_i][nextpointindex_j + nextcount + nextnextcount].coodi)
                                    {
                                        int p = 1;
                                        // Debug.Log(5);
                                        for (; ; )
                                        {
                                            if(nextnextcount < 0)
                                            {
                                                p = Mathf.Abs(p) * -1;
                                            }
                                            List<VertexAttribute> Prenextpoints = new List<VertexAttribute>();

                                            // 始点なら領域検出
                                            if (vertex[nextpointindex_i][nextpointindex_j + nextcount + p].coodi == startpoint.coodi)
                                            {
                                                List<VertexAttribute> Area = new List<VertexAttribute>();
                                                Area.Add(startpoint);
                                                for (int u = 0; u < nextpoints.Count; u++)
                                                {
                                                    Area.Add(nextpoints[u]);
                                                }
                                                for (int u = 0; u < Prenextpoints.Count; u++)
                                                {
                                                    // 仮置きした分も入れなきゃね
                                                    Area.Add(Prenextpoints[u]);
                                                }
                                                //　Debug.Log(5);
                                                area_flag = true;
                                                PreAreas.Add(Area);
                                                break;
                                            }
                                            // 普通の点なら，Prenextpointsに一旦保存
                                            else if (vertex[nextpointindex_i][nextpointindex_j + nextcount + p].attribute == "other")
                                            {
                                                Prenextpoints.Add(vertex[crossIndex.i][crossIndex.j + p]);
                                            }
                                            // 交点ならば、次のステージへ
                                            else if (vertex[nextpointindex_i][nextpointindex_j + nextcount + p].attribute == "cross" || vertex[nextpointindex_i][nextpointindex_j + nextcount + p].attribute == "ep")
                                            {
                                                nextpointSecondCandi.Add(vertex[crossIndex.i][crossIndex.j + 1]);
                                                break;
                                            }
                                            
                                            p = Mathf.Abs(p) + 1;
                                        }
                                    }
                                }
                                
                            }

                            // ファーストステージで領域検出に成功したら，始点を変える
                            if(area_flag == true)
                            {
                                break;
                            }

                            // セカンドステージ
                            VertexAttribute nextpointThirdCandi = null;
                            // 候補が2つ以上あるなら，入り口方向ベクトルとの外積で勝負(小さいほうの勝ち)
                            if (nextpointSecondCandi.Count >= 2)
                            {
                                // Debug.Log(6);
                                // 入口方向ベクトル
                                float x_start = vertex[nextpointindex_i][nextpointindex_j + nextcount].coodi.x - vertex[nextpointindex_i][nextpointindex_j + nextcount - nextnextcount].coodi.x;
                                float y_start = vertex[nextpointindex_i][nextpointindex_j + nextcount].coodi.z - vertex[nextpointindex_i][nextpointindex_j + nextcount - nextnextcount].coodi.z;
                                float length_start = Mathf.Sqrt(x_start * x_start + y_start * y_start);
                                // 正規化
                                Vector2 startVector = new Vector2(x_start / length_start, y_start / length_start);

                                // 外積リスト
                                List<float> Crosses = new List<float>();

                                for(int l = 0; l < nextpointSecondCandi.Count; l++)
                                {
                                    // 候補ベクトル
                                    float x_next = nextpointSecondCandi[l].coodi.x - vertex[nextpointindex_i][nextpointindex_j + nextcount].coodi.x;
                                    float y_next = nextpointSecondCandi[l].coodi.z - vertex[nextpointindex_i][nextpointindex_j + nextcount].coodi.z;
                                    float length_next = Mathf.Sqrt(x_next * x_next + y_next * y_next);
                                    // 正規化
                                    Vector2 nextVector = new Vector2(x_next / length_next, y_next / length_next);

                                    // 外積
                                    float cross = startVector.x * nextVector.y - startVector.y * nextVector.x;
                                    Crosses.Add(cross);

                                }

                                // 外積の最小値を求めて，それに対応する候補が勝者
                                float CrossMin = 0.0f;
                                for(int l = 0; l < Crosses.Count; l++)
                                {
                                    if(l == 0)
                                    {
                                        CrossMin = Crosses[0];
                                        nextpointThirdCandi = nextpointSecondCandi[0];
                                    }
                                    else
                                    {
                                        if(Crosses[l] < CrossMin)
                                        {
                                            CrossMin = Crosses[l];
                                            nextpointThirdCandi = nextpointSecondCandi[l];
                                        }
                                    }
                                }


                            }

                            // 候補が1つだけなら，勝者
                            else if(nextpointSecondCandi.Count == 1)
                            {
                                // Debug.Log(7);
                                nextpointThirdCandi = nextpointSecondCandi[0];
                            }

                            // 候補が1つもなかったら，失敗
                            else
                            {
                                break;
                            }

                            // 勝者が端点なら失敗
                            if(nextpointThirdCandi.attribute == "ep")
                            {
                                break;
                            }

                            // 勝者になった"方向"に進める
                            bool false_flag = false;
                            if (crossIndex.j + 1 < vertex[crossIndex.i].Count)
                            {
                                if (nextpointThirdCandi.coodi == vertex[crossIndex.i][crossIndex.j + 1].coodi) {
                                    
                                    for(int k = 1; ;k++)
                                    {
                                        // 進んだ先が端点ならば失敗
                                        if (vertex[crossIndex.i][crossIndex.j + k].attribute == "cross")
                                        {                                            
                                            break;
                                        }
                                        else if(vertex[crossIndex.i][crossIndex.j + k].attribute == "ep")
                                        {
                                            false_flag = true;
                                            break;
                                        }
                                    }
                                    if(false_flag == true)
                                    {
                                        break;
                                    }

                                    nextpointindex_i = crossIndex.i;
                                    nextpointindex_j = crossIndex.j;
                                    nextcount = 1;
                                    minusflag = false;
                                }

                            }
                            // j - 1のときだけ，次からマイナスをたどる
                            if (crossIndex.j - 1 >= 0)
                            {
                                if (nextpointThirdCandi.coodi == vertex[crossIndex.i][crossIndex.j - 1].coodi)
                                {
                                    for (int k = -1; ; k--)
                                    {
                                        // 進んだ先が端点ならば失敗
                                        if (vertex[crossIndex.i][crossIndex.j + k].attribute == "cross")
                                        {
                                            break;
                                        }
                                        else if (vertex[crossIndex.i][crossIndex.j + k].attribute == "ep")
                                        {
                                            false_flag = true;
                                            break;
                                        }
                                    }

                                    if (false_flag == true)
                                    {
                                        break;
                                    }

                                    nextpointindex_i = crossIndex.i;
                                    nextpointindex_j = crossIndex.j;
                                    nextcount = -1;
                                    minusflag = true;
                                }

                            }
                            if ((0 <= nextpointindex_j + nextcount + nextnextcount) && (nextpointindex_j + nextcount + nextnextcount < vertex[nextpointindex_i].Count))
                            {

                                if (nextpointThirdCandi.coodi == vertex[nextpointindex_i][nextpointindex_j + nextcount + nextnextcount].coodi)
                                {

                                    if (nextcount < 0)
                                    {
                                        for (int k = -1; ; k++)
                                        {
                                            // 進んだ先が端点ならば失敗
                                            if (vertex[nextpointindex_i][nextpointindex_j + nextcount + k].attribute == "cross")
                                            {
                                                break;
                                            }
                                            else if (vertex[nextpointindex_i][nextpointindex_j + nextcount + k].attribute == "ep")
                                            {
                                                false_flag = true;
                                                break;
                                            }
                                        }

                                        if (false_flag == true)
                                        {
                                            break;
                                        }

                                        minusflag = true;
                                    }
                                    else
                                    {
                                        for (int k = 1; ; k++)
                                        {
                                            // 進んだ先が端点ならば失敗
                                            if (vertex[nextpointindex_i][nextpointindex_j + nextcount + k].attribute == "cross")
                                            {
                                                break;
                                            }
                                            else if (vertex[nextpointindex_i][nextpointindex_j + nextcount + k].attribute == "ep")
                                            {
                                                false_flag = true;
                                                break;
                                            }
                                        }
                                        if (false_flag == true)
                                        {
                                            break;
                                        }
                                        minusflag = false;
                                    }

                                    // nextpointindex_i = nextpointindex_i;
                                    nextpointindex_j = nextpointindex_j + nextcount + nextnextcount;
                                    nextcount = 1;
                                }

                            }

                        }
                        // 進めていって，端点にたどり着いたら，始点を変える
                        else if(nextpoint.attribute == "ep")
                        {
                            // Debug.Log(11);
                            break;
                        }
                        
                    }

                }

            }
        }

        // 被ってるエリアを消す
        Debug.Log("PreAreaCount" + PreAreas.Count);
        List<Vector3> PreCenters = new List<Vector3>();
        for (int i = 0; i < PreAreas.Count; i++)
        {
            float center_x = 0;
            float center_z = 0;
            int n = 0;
            for (int j = 0; j < PreAreas[i].Count; j++)
            {
                center_x += PreAreas[i][j].coodi.x;
                center_z += PreAreas[i][j].coodi.z;
                n++;
            }
            Vector3 Center = new Vector3(center_x / n, 9.0f, center_z / n);
            PreCenters.Add(Center);
        }

        List<List<VertexAttribute>> Areas = new List<List<VertexAttribute>>();

        List<Vector3> Centers = new List<Vector3>();
        for (int i = 0; i < PreAreas.Count; i++)
        {
            if (i == 0)
            {
                Areas.Add(PreAreas[i]);
                Centers.Add(PreCenters[i]);
            }
            else
            {
                bool add_flag = true;
                for (int j = 0; j < Centers.Count; j++)
                {
                    if (Mathf.Floor(PreCenters[i].x * 100) / 100 == Mathf.Floor(Centers[j].x * 100) / 100 && Mathf.Floor(PreCenters[i].z * 100) / 100 == Mathf.Floor(Centers[j].z * 100) / 100)
                    {
                        add_flag = false;
                        break;                        
                    }

                }
                if (add_flag == true)
                {
                    Areas.Add(PreAreas[i]);
                    Centers.Add(PreCenters[i]);
                }
            }
        }


        for (int i = 0; i < Centers.Count; i++)
        {
            // GameObject AreaCenter = Instantiate(CrossObj, Centers[i], Quaternion.identity);            
        }

        return Areas;
    }

    IntPair SerchCrossPair(VertexAttribute cross, int index_i, int index_j, List<List<VertexAttribute>> vertexList)
    {
        for(int i = 0; i < vertexList.Count; i++)
        {
            for(int j = 0; j < vertexList[i].Count; j++)
            {
                if((i != index_i || j != index_j) && cross.coodi == vertexList[i][j].coodi)
                {
                    IntPair indexpair = new IntPair(i, j);
                    return indexpair;
                }
            }
        }

        return null;
    }

    private void MakeArea(List<List<VertexAttribute>> Areas)
    {

        foreach(List<VertexAttribute> ls in Areas)
        {
            for(int i = 0; i < ls.Count; i++)
            {
                Vector3 v1 = ls[i].coodi;
                Vector3 v2 = ls[(i + 1) % ls.Count].coodi;
                Debug.DrawLine(v1, v2, Color.white, 10000f);
                Debug.Log(v1);
            }

            Debug.Log("------");
        }

        //List<Vector3> Centers = new List<Vector3>();
        //for (int i = 0; i < Areas.Count; i++)
        //{           
        //    float center_x = 0;
        //    float center_z = 0;
        //    int n = 0;
        //    for (int j = 0; j < Areas[i].Count; j++)
        //    {
        //        center_x += Areas[i][j].coodi.x;
        //        center_z += Areas[i][j].coodi.z;
        //        n++;
        //    }
        //    Vector3 Center = new Vector3(center_x / n, 9.0f, center_z / n);
        //    Centers.Add(Center);
        //}

        //for (int i = 0; i < Areas.Count; i++)
        //{
        //    GameObject AreaObj = Instantiate(AreaObject, new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity);
        //    AreaObj.name = "Area" + i;
        //    AreaObj.AddComponent<MeshFilter>();
        //    AreaObj.AddComponent<MeshRenderer>();
        //    Material mat = AreaObj.GetComponent<Renderer>().material;
        //    mat.color = new Color(Random.value, Random.value, Random.value, 1.0f);

        //    Mesh mesh = new Mesh();
        //    AreaObj.GetComponent<MeshFilter>().mesh = mesh;

        //    // 頂点(面の中心にも1つ)
        //    int vertexnum = Areas[i].Count;
        //    Vector3[] vertices = new Vector3[vertexnum + 1];
        //    // 三角面の頂点
        //    int[] triangles = new int[vertexnum * 3];

        //    vertices[0] = Centers[i];
        //    for(int j = 0; j < vertexnum; j++)
        //    {
        //        vertices[j + 1] = new Vector3(Areas[i][j].coodi.x, Areas[i][j].coodi.y - 10.0f, Areas[i][j].coodi.z);
        //    }

        //    for (int j = 0; j < vertexnum; j++)
        //    {
        //        triangles[j * 3] = 0;
        //        triangles[j * 3 + 1] = j + 1;
        //        triangles[j * 3 + 2] = j + 2 != vertexnum + 1 ? j + 2 : 1;
        //    }

        //    mesh.vertices = vertices;
        //    mesh.triangles = triangles;
        //    mesh.RecalculateNormals();
        //}

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
