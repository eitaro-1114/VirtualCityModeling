using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Polygon
{
    private List<Vector2> vertices;     // 頂点
    private List<LineSegment> edges;    // 辺
    public bool isConvex;               // 凸かどうか

    // verticesには凸多角形の頂点座標が順番に格納してあるものとする
    public Polygon(List<Vector2> vertices)
    {
        int size = vertices.Count;
        if (size < 3)
        {
            // 角数が3以下の場合はエラー
            throw new System.Exception("頂点数が足らないぞ");
        }

        this.vertices = vertices;
        edges = new List<LineSegment>();

        // 凸多角形か判定
        // 基準となるCCW値を計算
        float ccw0 = GeomUtils.CounterClockWise(vertices[0], vertices[1], vertices[2]);
        if (ccw0 == 0)
        {
            // 0の場合はエラー
            isConvex = false;
            // throw new Exception("指定したポリゴンは凸じゃないぞ");
        }

        for (int i = 0; i < size; i++)
        {
            Vector2 p1 = vertices[i];
            Vector2 p2 = vertices[(i + 1) % size];
            Vector2 p3 = vertices[(i + 2) % size];

            float ccw = GeomUtils.CounterClockWise(p1, p2, p3);

            // 基準値と符号が異なる、またはccwが0のとき
            if (ccw * ccw0 <= 0)
            {
                isConvex = false;
                // throw new Exception("指定したポリゴンは凹だぞ");
            }
        }

        for (int i = 0; i < size; i++)
        {
            Vector2 v1 = vertices[i];               // i番目の頂点
            Vector2 v2 = vertices[(i + 1) % size];  // v1の次の頂点
            // 2つの頂点から線分を作成
            edges.Add(new LineSegment(v1.x, v1.y, v2.x, v2.y, true));
            // Debug.DrawLine(new Vector3(v1.x, 0.0f, v1.y), new Vector3(v2.x, 0.0f, v2.y), color, 10000);
        }

    }

    // ConvexPolygonの描画
    public void DrawPolygon(Color color)
    {
        for (int i = 0; i < vertices.Count; i++)
        {
            Debug.DrawLine(GeomUtils.VectorExtend(vertices[i]), GeomUtils.VectorExtend(vertices[(i + 1) % vertices.Count]), color, 10000);
        }
    }

    // 指定位置の頂点の取得
    public Vector2 GetVertex(int index)
    {
        return vertices[index];
    }

    // 構成する頂点を取得
    public List<Vector2> GetVertices()
    {
        return vertices;
    }

    // 指定位置の辺の取得
    public LineSegment GetEdge(int index)
    {
        return edges[index];
    }

    // 辺の取得
    public List<LineSegment> GetEdges()
    {
        return edges;
    }

    // 辺の数を取得
    public int GetEdgeCount()
    {
        return edges.Count;
    }

    // 指定した頂点を有する辺を取得
    public List<LineSegment> GetEdgesFromPoint(Vector2 v)
    {
        List<LineSegment> edges = new List<LineSegment>();

        foreach (LineSegment s in this.GetEdges())
        {
            foreach (Vector2 sv in s.GetPoints())
            {
                if (sv == v)
                {
                    edges.Add(s);
                }
            }
        }
        return edges;
    }

    // 頂点の挿入
    public void InsertVertex(int index, Vector2 vert)
    {
        this.vertices.Insert(index, vert);
    }

    // 頂点の削除
    public void RemoveVertex(Vector2 vert)
    {
        this.vertices.Remove(vert);
    }

    public bool Contains(float x, float y)
    {
        // 多角形のy座標範囲を求める
        float minY = float.PositiveInfinity;
        float maxY = float.NegativeInfinity;

        for (int i = 0; i < vertices.Count; i++)
        {
            minY = Mathf.Min(minY, vertices[i].y);
            maxY = Mathf.Max(maxY, vertices[i].y);
        }

        // yが範囲外のときはfalse
        if (y <= minY || y >= maxY)
        {
            return false;
        }

        // 与えられた座標を始点とし、右方向に延びる直線を生成
        LineSegment halfLine = new LineSegment(x, y, x + 100000000, y, false);
        int count = 0;
        for (int i = 0; i < edges.Count; i++)
        {
            // 半直線が辺の終点とちょうど重なる場合，次の辺の始点とも交差が検出され，二重にカウントされてしまうため，カウントをスキップする
            if (edges[i].y2 == y)
            {
                continue;
            }
            if (edges[i].Intersects(halfLine))
            {
                count++;
            }
        }
        // 交差回数が奇数の場合はtrue
        return count % 2 == 1;
    }

    public bool Contains(Vector2 v)
    {
        float x = v.x;
        float y = v.y;
        // 多角形のy座標範囲を求める
        float minY = float.PositiveInfinity;
        float maxY = float.NegativeInfinity;

        for (int i = 0; i < vertices.Count; i++)
        {
            minY = Mathf.Min(minY, vertices[i].y);
            maxY = Mathf.Max(maxY, vertices[i].y);
        }

        // yが範囲外のときはfalse
        if (y <= minY || y >= maxY)
        {
            return false;
        }

        // 与えられた座標を始点とし、右方向に延びる直線を生成
        LineSegment halfLine = new LineSegment(x, y, x + 100000000, y, false);
        int count = 0;
        for (int i = 0; i < edges.Count; i++)
        {
            // 半直線が辺の終点とちょうど重なる場合，次の辺の始点とも交差が検出され，二重にカウントされてしまうため，カウントをスキップする
            if (edges[i].y2 == y)
            {
                continue;
            }
            if (edges[i].Intersects(halfLine))
            {
                count++;
            }
        }
        // 交差回数が奇数の場合はtrue
        return count % 2 == 1;
    }

    // 凸多角形の面積を計算
    public float GetArea()
    {
        if (isConvex)
        {
            float crossSum = 0;     // 外積の合計
            int size = vertices.Count();
            // 頂点を巡回
            for (int i = 0; i < size; i++)
            {
                Vector2 v1 = vertices[i];
                Vector2 v2 = vertices[(i + 1) % size];

                // 外積
                float cross = GeomUtils.Cross(v1, v2);
                crossSum += cross;

            }
            return Mathf.Abs(crossSum / 2.0f);
        }
        else
        {
            throw new Exception("凹だぞ");
        }
    }

    public Polygon Manhattan()
    {
        List<Vector2> finalPolyVector = new List<Vector2>();
        foreach (LineSegment ls in this.GetEdges())
        {
            List<Vector2> manhattanEdge = new List<Vector2>();
            // 辺の始点と終点
            Vector2 a = ls.GetPoints()[0];
            Vector2 b = ls.GetPoints()[1];

            // それぞれの軸の長さ
            float l_x = b.x - a.x;
            float l_y = b.y - a.y;

            // 辺を三等分
            for (int i = 0; i < 4; i++)
            {
                float x = a.x + (l_x * i / 3);
                float y = a.y + (l_y * i / 3);
                Vector2 edgePoint = new Vector2(x, y);

                switch (i)
                {
                    case 1:
                        Vector2 dir_a = a - edgePoint;
                        if (Mathf.Abs(dir_a.x) >= Mathf.Abs(dir_a.y))
                        {
                            Vector2 pos = new Vector2(edgePoint.x, a.y);
                            manhattanEdge.Add(pos);
                        }
                        else
                        {
                            Vector2 pos = new Vector2(a.x, edgePoint.y);
                            manhattanEdge.Add(pos);
                        }
                        break;

                    case 2:
                        Vector2 dir_b = b - edgePoint;
                        if (Mathf.Abs(dir_b.x) >= Mathf.Abs(dir_b.y))
                        {
                            Vector2 pos = new Vector2(edgePoint.x, b.y);
                            manhattanEdge.Add(pos);
                        }
                        else
                        {
                            Vector2 pos = new Vector2(b.x, edgePoint.y);
                            manhattanEdge.Add(pos);
                        }
                        break;

                    default:
                        manhattanEdge.Add(edgePoint);
                        break;
                }
            }

            foreach (Vector2 v in manhattanEdge)
            {
                finalPolyVector.Add(v);
            }
        }

        Polygon finalPoly = new Polygon(finalPolyVector);
        return finalPoly;
    }

    private bool IsClipPolyEdge(LineSegment ls, Polygon clipPoly)
    {
        bool result = false;
        // ここに内容を記述

        return result;
    }
}

// ポリゴンの共通領域を検知
public class PolygonIntersectionCalculator
{
    // 捜査線上の辺の相対位置
    enum EdgePosition
    {
        LEFT1, RIGHT1, LEFT2, RIGHT2
    }

    // 入力となる凸多角形を左側と右側に分解して作られる辺連結
    class Edge
    {
        public LineSegment segment;    // 辺の線分
        public Vector2 startPoint;     // 辺の始点
        public Vector2 endPoint;       // 辺の終点
        public Edge next;              // 次の辺

        public bool Intersects(Edge other)
        {
            return segment.Intersects(other.segment);
        }

        // 他の辺との交点を調べる
        // エラーあり
        public Vector2 GetIntersectionPoint(Edge other)
        {
            return segment.ToLine().getIntersectionPoint(other.segment.ToLine()).Value;
        }

        // 走査線との交点のx座標値を求める
        public float GetIntersectionX(Line sweepLine)
        {
            Vector2? p = segment.GetIntersectionPoint(sweepLine);
            if (p == null)
            {
                return startPoint.x;
            }
            else
            {
                return p.Value.x;
            }
        }

    }

    // 走査線と交わる4つの辺を格納するステータス
    class Status
    {
        public Edge left1;
        public Edge right1;
        public Edge left2;
        public Edge right2;
    }

    // イベントの処理結果
    class StepResult
    {
        public float nextSweepY;   // 次の走査線の位置として設定するY座標
        public bool mustContinue;  // これ以降も処理を続けるか

        public StepResult(float nextSweepY, bool mustContinue)
        {
            this.nextSweepY = nextSweepY;
            this.mustContinue = mustContinue;
        }
    }

    // polygon1 polygon2の交差領域を求める
    public Polygon Excute(Polygon polygon1, Polygon polygon2)
    {
        Status status = new Status();
        List<Edge> leftResult = new List<Edge>();       // 左側の計算結果
        List<Edge> rightResult = new List<Edge>();      // 右側の計算結果

        // 最初のイベント処理
        StepResult result = FirstPass(polygon1, polygon2, status, leftResult, rightResult);
        while (result.mustContinue == true)
        {
            // 2番目以降のイベント処理
            result = SecondPass(status, leftResult, rightResult);
        }

        // 左側と右側の計算結果を統合するリスト
        List<Edge> totalResult = new List<Edge>();
        for (int i = 0; i < leftResult.Count; i++)
        {
            totalResult.Add(leftResult[i]);
        }

        // 左側と右側の結果は辺の巡回方向が逆になっているので右側の結果を反転して連結
        rightResult.Reverse();
        for (int i = 0; i < rightResult.Count; i++)
        {
            totalResult.Add(rightResult[i]);
        }

        List<Vector2> resultPoints = new List<Vector2>();
        Vector2? lastPoint = null;
        int totalSize = totalResult.Count();
        for (int i = 0; i < totalSize; i++)
        {
            Edge e1 = totalResult[i];
            Edge e2 = totalResult[(i + 1) % totalSize];
            Vector2 p = e1.GetIntersectionPoint(e2);
            if (p != null && p.Equals(lastPoint) == false)
            {
                resultPoints.Add(p);
                lastPoint = p;
            }
        }

        // 3点以上あれば凸多角形として返す
        if (resultPoints.Count() >= 3)
        {
            return new Polygon(resultPoints);
        }
        else
        {
            return null;
        }

    }

    // 初回のイベント処理。凸多角形の左右の辺連結を作成し、走査線の初期位置を設定
    private StepResult FirstPass(Polygon polygon1, Polygon polygon2, Status status, List<Edge> leftResult, List<Edge> rightResult)
    {
        // 凸多角形の左右の辺連結を作成
        Edge left1 = CreateEdgeChain(polygon1, true);
        Edge right1 = CreateEdgeChain(polygon1, false);
        Edge left2 = CreateEdgeChain(polygon2, true);
        Edge right2 = CreateEdgeChain(polygon2, false);

        // Edgeチェインの最上点の座標を取得
        float topX1 = left1.startPoint.x;
        float topY1 = left1.startPoint.y;
        float topX2 = left2.startPoint.x;
        float topY2 = left2.startPoint.y;

        // 2つの凸多角形の最上点のうち、より高い位置にあるy座標を走査線の初期位置にする
        float sweepY = Mathf.Max(topY1, topY2);
        // 走査線作成
        Line sweepLine = Line.FromPoints(0, sweepY, 1, sweepY);

        // Edgeチェインの中から、初めに走査線と交わるEdgeを見つけてステータスに設定
        status.left1 = FindInitialEdge(left1, sweepLine);
        status.right1 = FindInitialEdge(right1, sweepLine);
        status.left2 = FindInitialEdge(left2, sweepLine);
        status.right2 = FindInitialEdge(right2, sweepLine);

        // いづれかのEdgeが見つからなければ、交差部分は存在しないので終了
        if (status.left1 == null || status.right1 == null || status.left2 == null || status.right2 == null)
        {
            return new StepResult(sweepY, false);
        }

        // 初回のイベント処理。2つの最上点の位置関係によって、必要な処理と順番が変化
        if (topY1 > topY2)
        {
            if (topX1 > topX2)
            {
                Process(status, EdgePosition.LEFT1, sweepLine, leftResult, rightResult);
                Process(status, EdgePosition.RIGHT1, sweepLine, leftResult, rightResult);
            }
            else
            {
                Process(status, EdgePosition.RIGHT1, sweepLine, leftResult, rightResult);
                Process(status, EdgePosition.LEFT1, sweepLine, leftResult, rightResult);
            }
        }
        else
        {
            if (topX1 > topX2)
            {
                Process(status, EdgePosition.RIGHT2, sweepLine, leftResult, rightResult);
                Process(status, EdgePosition.LEFT2, sweepLine, leftResult, rightResult);
            }
            else
            {
                Process(status, EdgePosition.LEFT2, sweepLine, leftResult, rightResult);
                Process(status, EdgePosition.RIGHT2, sweepLine, leftResult, rightResult);
            }
        }

        return new StepResult(sweepY, true);

    }

    // 2回目以降のイベント処理
    StepResult SecondPass(Status status, List<Edge> leftResult, List<Edge> rightResult)
    {
        // 次に処理すべき辺を選択
        Edge next = PickNextEdge(status);
        if (next == null)
        {
            // 見つからなければ終了
            return new StepResult(float.NaN, false);
        }

        // 選択された辺の始点を走査線の次の位置にする
        float nextSweepY = next.startPoint.y;
        // 走査線作成
        Line sweepLine = Line.FromPoints(0, nextSweepY, 1, nextSweepY);

        // 選択された辺が、ステータスの中でleft1/right1/left2/right2のうちどれと対応するかを特定する
        EdgePosition? pos = null;

        if (next == status.left1.next)
        {
            pos = EdgePosition.LEFT1;
            status.left1 = next;
        }
        else if (next == status.right1.next)
        {
            pos = EdgePosition.RIGHT1;
            status.right1 = next;
        }
        else if (next == status.left2.next)
        {
            pos = EdgePosition.LEFT2;
            status.left2 = next;
        }
        else if (next == status.right2.next)
        {
            pos = EdgePosition.RIGHT2;
            status.right2 = next;
        }

        // イベント処理
        Process(status, pos.Value, sweepLine, leftResult, rightResult);
        return new StepResult(nextSweepY, true);

    }

    // 凸多角形の[左側]または[右側]の上から下に向かう辺連結を作成
    private Edge CreateEdgeChain(Polygon polygon, bool left)
    {
        float minY = float.PositiveInfinity;
        float maxY = float.NegativeInfinity;
        int minYIndex = -1;
        int maxYIndex = -1;
        int size = polygon.GetEdgeCount();

        // 凸多角形の頂点のうち最上点と最下点の位置を特定
        for (int i = 0; i < size; i++)
        {
            Vector2 v = polygon.GetVertex(i);
            float y = v.y;

            if (y < minY)
            {
                minY = y;
                minYIndex = i;
            }

            if (y > maxY)
            {
                maxY = y;
                maxYIndex = i;
            }
        }

        Edge firstEdge = null;
        Edge lastEdge = null;

        bool ccw = GeomUtils.CounterClockWise(polygon.GetVertex(0), polygon.GetVertex(1), polygon.GetVertex(2)) < 0;
        bool forward = (ccw && left) || (!ccw && !left);
        int nextIndex;

        // 最上点から開始し、最下点に到達するまで続ける
        for (int i = minYIndex; i != maxYIndex; i = nextIndex)
        {
            // 辺を作成
            Edge edge = new Edge();

            // 前進の場合
            if (forward == true)
            {
                nextIndex = (i + 1) % size;
                edge.segment = polygon.GetEdge(i);
            }
            // 後退の場合
            else
            {
                nextIndex = (i + size - 1) % size;
                edge.segment = polygon.GetEdge(nextIndex);
            }

            // 辺の始点と終点を代入
            edge.startPoint = polygon.GetVertex(i);
            edge.endPoint = polygon.GetVertex(nextIndex);

            // 初めて辺が作られた場合
            if (firstEdge == null)
            {
                firstEdge = edge;
            }
            else
            {
                // 直前の辺に新しい辺を連結
                lastEdge.next = edge;
            }

            lastEdge = edge;

        }

        return firstEdge;
    }

    // 辺連結の中から、初めに走査線と交わる辺を見つける
    private Edge FindInitialEdge(Edge edge, Line sweepLine)
    {
        for (Edge e = edge; e != null; e = e.next)
        {
            if (e.segment.Intersects(sweepLine))
            {
                return e;
            }
        }

        return null;
    }

    // ステータス中でposの位置にある辺の始点をイベント点として処理する
    // 交差部分の辺を左右別に求め、LeftResultとRightResultに追加
    private void Process(Status status, EdgePosition pos, Line sweepLine, List<Edge> leftResult, List<Edge> rightResult)
    {
        switch (pos)
        {
            case EdgePosition.LEFT1:
                ProcessLeft(status.left1, status.right1, status.left2, status.right2, sweepLine, leftResult, rightResult);
                break;
            case EdgePosition.RIGHT1:
                ProcessRight(status.left1, status.right1, status.left2, status.right2, sweepLine, leftResult, rightResult);
                break;
            case EdgePosition.LEFT2:
                ProcessLeft(status.left2, status.right2, status.left1, status.right1, sweepLine, leftResult, rightResult);
                break;
            case EdgePosition.RIGHT2:
                ProcessRight(status.left2, status.right2, status.left1, status.right1, sweepLine, leftResult, rightResult);
                break;
            default:
                throw new InvalidOperationException();
        }
    }

    // left1の始点をイベント点として処理
    // http://gihyo.jp/dev/serial/01/geometry/0010?page=2
    private void ProcessLeft(Edge left1, Edge right1, Edge left2, Edge right2, Line sweepLine, List<Edge> leftResult, List<Edge> rightResult)
    {
        float l1 = left1.GetIntersectionX(sweepLine);
        float l2 = left2.GetIntersectionX(sweepLine);
        float r2 = right2.GetIntersectionX(sweepLine);

        // left1がleft2とright2の内部から始まる場合(条件①)
        if (l1 > l2 && l1 < r2)
        {
            // left1は交差多角形の一部
            leftResult.Add(left1);
        }

        // left1がright2と交わり、right2よりも右から始まる場合(条件②)
        if (left1.Intersects(right2) == true && l1 >= r2)
        {
            // left1, right2はともに交差多角形の一部かつ図形の上端になるため結果の先頭位置に追加
            leftResult.Insert(0, left1);
            rightResult.Insert(0, right2);
        }

        // left1とleft2が交わる場合
        if (left1.Intersects(left2) == true)
        {
            // left1がleft2よりも右から始まるなら(条件③)
            if (l1 > l2)
            {
                // left2は交差多角形の一部
                leftResult.Add(left2);
            }
            // そうでないなら(条件④)
            else
            {
                leftResult.Add(left1);
            }
        }

    }

    // right1の始点をイベント点として処理。ProcessLeftの対称
    private void ProcessRight(Edge left1, Edge right1, Edge left2, Edge right2, Line sweepLine, List<Edge> leftResult, List<Edge> rightResult)
    {
        float r1 = right1.GetIntersectionX(sweepLine);
        float l2 = left2.GetIntersectionX(sweepLine);
        float r2 = right2.GetIntersectionX(sweepLine);

        // right1がleft2とright2の間から始まる場合
        if (r1 > l2 && r1 < r2)
        {
            rightResult.Add(right1);
        }

        // right1がleft2と交わり、left2よりも左から始まる場合
        if (right1.Intersects(left2) == true && r1 <= l2)
        {
            rightResult.Insert(0, right1);
            leftResult.Insert(0, left2);
        }
        // right1とright2が交わる場合
        if (right1.Intersects(right2))
        {
            if (r1 < r2)
            {
                rightResult.Add(right2);
            }
            else
            {
                rightResult.Add(right1);
            }
        }

    }

    // ステータスの中から終点のy座標がもっと↓にある辺を探し、次に処理すべき辺として返す
    private Edge PickNextEdge(Status status)
    {
        Edge result = ChooseEdgeWithLowerEndY(status.left1, status.right1);
        result = ChooseEdgeWithLowerEndY(result, status.left2);
        result = ChooseEdgeWithLowerEndY(result, status.right2);
        return result.next;
    }

    // e1とe2のうち、終点のy座標が↓にあるものを選択
    private Edge ChooseEdgeWithLowerEndY(Edge e1, Edge e2)
    {
        float y1;
        float y2;
        if (e1 == null)
        {
            y1 = float.PositiveInfinity;
        }
        else
        {
            y1 = e1.endPoint.y;
        }

        if (e2 == null)
        {
            y2 = float.PositiveInfinity;
        }
        else
        {
            y2 = e2.endPoint.y;
        }

        if (y1 == y2)
        {
            if (e1 != null && e1.next != null)
            {
                return e1;
            }
            else
            {
                return e2;
            }
        }
        else if (y1 < y2)
        {
            return e1;
        }
        else
        {
            return e2;
        }
    }

}

// 疑似的な半平面を生成するクラス
// http://gihyo.jp/dev/serial/01/geometry/0012
public class PseudoHalfPlaneGenerator
{
    // 境界点
    private Vector2 boundary1;
    private Vector2 boundary2;
    private Vector2 boundary3;

    // 境界線
    private LineSegment border1;
    private LineSegment border2;
    private LineSegment border3;

    // 交差の場合分けを正確に行うための閾値
    private float distanceThreshold;

    // boundaryの値を大きくすると誤差が大きくなる
    public PseudoHalfPlaneGenerator(float boundary)
    {
        boundary1 = new Vector2(0, -boundary);
        boundary2 = new Vector2(-boundary, boundary);
        boundary3 = new Vector2(boundary, boundary);

        border1 = new LineSegment(boundary1, boundary2);
        border2 = new LineSegment(boundary2, boundary3);
        border3 = new LineSegment(boundary3, boundary1);

        // Debug.DrawLine(GeomUtils.VectorExtend(boundary1), GeomUtils.VectorExtend(boundary2), Color.black, 10000);
        // Debug.DrawLine(GeomUtils.VectorExtend(boundary2), GeomUtils.VectorExtend(boundary3), Color.black, 10000);
        // Debug.DrawLine(GeomUtils.VectorExtend(boundary3), GeomUtils.VectorExtend(boundary1), Color.black, 10000);

        distanceThreshold = boundary / 1000.0f;
    }

    public Polygon Execute(Line line, Vector2 example)
    {
        // lineと各境界線の交差を調べる
        Vector2? p1 = border1.GetIntersectionPoint(line);
        Vector2? p2 = border2.GetIntersectionPoint(line);
        Vector2? p3 = border3.GetIntersectionPoint(line);

        List<Vector2> vertices = new List<Vector2>();

        // lineが境界線1および2と交差する場合
        if (p1 != null && p2 != null)
        {
            if (Vector2.Distance(p1.Value, p2.Value) >= distanceThreshold)
            {
                // 境界点2とexampleがlineから見て同じ側にあるなら
                // https://drive.google.com/open?id=1QbAYkNzsr4AIk8H93Q_GNno3KoVwbWvk
                if (GeomUtils.CounterClockWise(p1.Value, boundary2, p2.Value) * GeomUtils.CounterClockWise(p1.Value, example, p2.Value) > 0)
                {
                    // 境界点2を含む方の切断後頂点リストを作成
                    AddVertices(vertices, p1.Value, boundary2, p2.Value);
                }
                // そうでないなら
                // https://drive.google.com/open?id=1OHOhOKbXvBp8B58q6dopXOVCq_8LNP5I
                else
                {
                    // 境界点2を含まない方の切断後頂点リストを作成
                    AddVertices(vertices, p1.Value, p2.Value, boundary3, boundary1);
                }
            }
        }
        // lineが境界線2および3と交差する場合
        else if (p2 != null && p3 != null)
        {
            if (Vector2.Distance(p2.Value, p3.Value) >= distanceThreshold)
            {
                // 境界点3とexampleがlineから見て同じ側にあるなら
                // https://drive.google.com/open?id=1b-qjjrRNY2cYwGgX7kfeOmHsKIgcPlM1
                if (GeomUtils.CounterClockWise(p2.Value, boundary3, p3.Value) * GeomUtils.CounterClockWise(p2.Value, example, p3.Value) > 0)
                {
                    AddVertices(vertices, p2.Value, boundary3, p3.Value);
                }
                // そうでないなら
                // https://drive.google.com/open?id=1mmp8vBdsAVQQOY04rXfkIrdlQB2U3kiL
                else
                {
                    AddVertices(vertices, p2.Value, p3.Value, boundary1, boundary2);
                }
            }
        }
        else if (p3 != null && p1 != null)
        {
            if (Vector2.Distance(p1.Value, p3.Value) >= distanceThreshold)
            {
                // 境界点1とexampleがlinrから見て同じ側にあるなら(画像略)
                if (GeomUtils.CounterClockWise(p3.Value, boundary1, p1.Value) * GeomUtils.CounterClockWise(p3.Value, example, p1.Value) > 0)
                {
                    AddVertices(vertices, p3.Value, boundary1, p1.Value);
                }
                else
                {
                    AddVertices(vertices, p3.Value, p1.Value, boundary2, boundary3);
                }
            }
        }
        else
        {
            Debug.Log("p1 = " + p1 + ", p2 = " + p2 + ", p3 = " + p3);
            // Debug.DrawLine(GeomUtils.VectorExtend(line.PointsOnLine()[0]), GeomUtils.VectorExtend(line.PointsOnLine()[1]), Color.green, 1000);
            throw new Exception("半平面ができないぞ");
        }

        // 頂点リストから凸多角形を生成して返す
        return new Polygon(vertices);
    }

    // リストにポイントを追加。このとき、重複追加は避ける
    private void AddVertices(List<Vector2> list, params Vector2[] points)
    {
        for (int i = 0; i < points.Count(); i++)
        {
            if (list.Count() == 0)
            {
                list.Add(points[i]);
            }
            else
            {
                Vector2 first = list[0];
                Vector2 last = list[list.Count() - 1];

                if (!points[i].Equals(first) && !points[i].Equals(last))
                {
                    list.Add(points[i]);
                }
            }
        }
    }

}

public class VoronoiGenerator
{
    private PseudoHalfPlaneGenerator halfPlaneGenerator = new PseudoHalfPlaneGenerator(1000);     // この値を大きくすると誤差が大きくなる
    private PolygonIntersectionCalculator intersectionCalculator = new PolygonIntersectionCalculator();

    // areaの領域内で、傍点sitesの各ボロノイ領域をリストにして返す
    // areaおよびsitesの各座標を、halfPlaneGeneratorの範囲に収めること
    public List<Polygon> Execute(Polygon area, List<Vector2> sites)
    {
        List<Polygon> result = new List<Polygon>();
        foreach (Vector2 s1 in sites)
        {

            // 途中計算結果の領域
            Polygon region = null;

            foreach (Vector2 s2 in sites)
            {
                if (s1 == s2)
                {
                    continue;
                }

                // s1とs2の垂直二等分線を求める
                Line line = Line.PerpendicularBisector(s1.x, s1.y, s2.x, s2.y);
                // 垂直二等分線による半平面のうち、s1を含む方を求める
                // https://drive.google.com/open?id=1YR_0cL7BIT9AgXyFTiiZyLnj61wnl4cP
                // // Debug.DrawLine(GeomUtils.VectorExtend(line.PointsOnLine()[0]), GeomUtils.VectorExtend(line.PointsOnLine()[1]), Color.green, 1000);
                // Debug.Log("s1 = " + s1 + ", s2 = " + s2);
                Polygon halfPlane = halfPlaneGenerator.Execute(line, s1);

                // 初回計算時なら
                if (region == null)
                {
                    // areaと半平面の交差を求める
                    region = intersectionCalculator.Excute(area, halfPlane);
                }
                // 2回目以降ならば
                else
                {
                    region = intersectionCalculator.Excute(region, halfPlane);
                }
            }

            result.Add(region);
        }
        return result;
    }

}

// 多角形を三角形分割
// https://sonson.jp/blog/2007/02/12/1/
// http://javaappletgame.blog34.fc2.com/blog-entry-148.html
public class SplitTriangles
{
    private Vector2 origin = new Vector2(-100, -100);     // 原点
    private bool mustContinue = true;

    public List<Polygon> Excecute(Polygon p)
    {
        List<Vector2> DetectPoints = new List<Vector2>();
        DetectPoints = p.GetVertices();
        List<Polygon> triangles = new List<Polygon>();

        while (mustContinue)
        {
            List<Vector2> triangleVerts = new List<Vector2>();

            Vector2 farPoint = new Vector2();
            int? farIndex = null;
            // 原点から最も遠い頂点を探す①
            for (int i = 0; i < DetectPoints.Count; i++)
            {
                if (i == 0)
                {
                    farPoint = DetectPoints[i];
                    farIndex = i;
                }
                else if (Vector2.Distance(origin, DetectPoints[i]) > Vector2.Distance(origin, farPoint))
                {
                    farPoint = DetectPoints[i];
                    farIndex = i;
                }
            }
            triangleVerts.Add(farPoint);

            // 最も遠い頂点の隣にある2頂点A, B
            Vector2 farPoint_A = new Vector2();
            farPoint_A = DetectPoints[(DetectPoints.Count() + farIndex.Value + 1) % DetectPoints.Count()];
            triangleVerts.Add(farPoint_A);

            Vector2 farPoint_B = new Vector2();
            farPoint_B = DetectPoints[(DetectPoints.Count() + farIndex.Value - 1) % DetectPoints.Count()];
            triangleVerts.Add(farPoint_B);

            // farPointとAB(以下3点)からなる三角形の外積を保存
            float cross = GeomUtils.Cross(farPoint - farPoint_A, farPoint - farPoint_B);

            // 3点内に頂点があるか調べる
            Polygon triangle = new Polygon(triangleVerts);

            // 内部に頂点があれば
            if (Contains(triangle, farIndex.Value, DetectPoints))
            {
                bool mustSearch = true;
                while (mustSearch)
                {
                    // 隣の頂点に移動(今回はA)
                    farPoint = farPoint_A;
                    farPoint_A = DetectPoints[(DetectPoints.Count() + DetectPoints.IndexOf(farPoint) + 1) % DetectPoints.Count()];
                    farPoint_B = DetectPoints[(DetectPoints.Count() + DetectPoints.IndexOf(farPoint) - 1) % DetectPoints.Count()];
                    List<Vector2> triangleVertsProto = new List<Vector2>();
                    triangleVertsProto.Add(farPoint);
                    triangleVertsProto.Add(farPoint_A);
                    triangleVertsProto.Add(farPoint_B);

                    // 3頂点の外積を求め、さっき保存した符号が同じなら
                    if (GeomUtils.Cross(farPoint - farPoint_A, farPoint - farPoint_B) * cross > 0)
                    {
                        // 三角形の内部に頂点があるか調べる
                        farIndex = (DetectPoints.IndexOf(farPoint));
                        Polygon triangleProto = new Polygon(triangleVertsProto);
                        // なければ
                        if (!Contains(triangleProto, farIndex.Value, DetectPoints))
                        {
                            // 三角形をListに追加。farPointをDetectPointsから削除して①に戻る
                            triangles.Add(triangleProto);
                            DetectPoints.Remove(farPoint);

                            // ループから脱出
                            mustSearch = false;
                        }
                    }
                }
            }
            // なければ
            else
            {
                // 三角形をListに追加。farPointをDetectPointsから削除して①に戻る
                triangles.Add(triangle);
                DetectPoints.Remove(farPoint);
            }

            // 頂点が3つまで減ったら
            if (DetectPoints.Count == 3)
            {
                mustContinue = false;
                List<Vector2> lastTriangle = new List<Vector2>();
                foreach (Vector2 v in DetectPoints)
                {
                    lastTriangle.Add(v);
                }

                triangles.Add(new Polygon(lastTriangle));
            }

        }

        return triangles;
    }

    // 3点内に頂点があるか調べる
    private bool Contains(Polygon triangle, int farIndex, List<Vector2> DetectPoints)
    {
        for (int i = 0; i < DetectPoints.Count; i++)
        {
            // 三角形の頂点は省く
            if (i != farIndex && i != (farIndex + 1) % DetectPoints.Count() && i != Mathf.Abs(farIndex - 1) % DetectPoints.Count())
            {
                // 3点内に頂点があればtrue
                if (triangle.Contains(DetectPoints[i].x, DetectPoints[i].y))
                {
                    return true;
                }
            }
        }

        return false;
    }

}

// ボロノイ領域群をクリッピング
public class VoronoiClipping
{
    public List<Polygon> Execute(List<Polygon> voronois, Polygon p)
    {
        List<Polygon> result = new List<Polygon>(voronois);
        // https://ja.stackoverflow.com/questions/10119/
        int size = voronois.Count();
        for (int i = 0; i < size; i++)
        {
            Polygon v = voronois[i];
            int vIndex = voronois.IndexOf(v);
            int vertSize = v.GetVertices().Count();
            List<Vector2> vertices = new List<Vector2>(v.GetVertices());

            for (int j = 0; j < vertSize; j++)
            {
                Vector2 vert = vertices[j];
                // 注目している頂点がクリッピング領域の外なら
                if (!p.Contains(vert.x, vert.y))
                {
                    int outPointIndex = v.GetVertices().IndexOf(vert);
                    List<Vector2> clipPoints = new List<Vector2>();
                    clipPoints = FindCrossPoints(v, vert, p);

                    switch (clipPoints.Count)
                    {
                        case 1:
                            result[vIndex].InsertVertex(outPointIndex, clipPoints[0]);
                            result[vIndex].RemoveVertex(vert);
                            break;
                        case 2:
                            if (outPointIndex == 0)
                            {
                                result[vIndex].InsertVertex(outPointIndex + 1, clipPoints[0]);
                                result[vIndex].InsertVertex(outPointIndex, clipPoints[1]);
                            }
                            else
                            {
                                result[vIndex].InsertVertex(outPointIndex, clipPoints[0]);
                                result[vIndex].InsertVertex(outPointIndex + 1, clipPoints[1]);
                            }
                            result[vIndex].RemoveVertex(vert);
                            break;
                        default:
                            break;

                    }
                }
            }
        }

        return result;
    }

    // 辺と領域の交点を見つける
    // v : 注目しているボロノイ領域, vert：vの頂点の1つ, p：クリッピングする図形
    private List<Vector2> FindCrossPoints(Polygon v, Vector2 vert, Polygon p)
    {
        List<LineSegment> edges = new List<LineSegment>(v.GetEdgesFromPoint(vert));
        List<Vector2> result = new List<Vector2>();
        for (int i = 0; i < edges.Count; i++)
        {
            LineSegment edge = edges[i];
            List<Vector2> crossPoints = new List<Vector2>();
            foreach (LineSegment s in p.GetEdges())
            {
                if (edge.Intersects(s))
                {
                    crossPoints.Add(edge.GetIntersectionPoint(s).Value);
                }
            }

            switch (crossPoints.Count)
            {
                // 交点が1つのとき
                case 1:
                    result.Add(crossPoints[0]);
                    break;
                // 交点がないとき
                case 0:
                    break;
                // 交点が2つ以上のとき
                default:
                    // 近いほうを交点とする
                    Vector2 crossPoint = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
                    foreach (Vector2 vec in crossPoints)
                    {
                        if (Vector2.Distance(vert, vec) < Vector2.Distance(vert, crossPoint))
                        {
                            crossPoint = vec;
                        }
                    }
                    result.Add(crossPoint);
                    break;
            }
        }
        return result;
    }

    // 領域との交点を求める
    private List<Vector2> IntersectPoints(LineSegment segment, Polygon p)
    {
        List<Vector2> result = new List<Vector2>();

        foreach (LineSegment s in p.GetEdges())
        {
            if (segment.Intersects(s))
            {
                result.Add(segment.GetIntersectionPoint(s).Value);
            }
        }

        return result;
    }
}


// 逐次添加法によるボロノイ分割
public class IncrementalVoronoi
{
    // area : 分割する領域  siteNum : 母点数 
    public List<Cell> Execute(Polygon area, int siteNum)
    {

        List<Cell> result = new List<Cell>();
        List<Vector2> sites = new List<Vector2>();      // 母点群
        List<Cell> cells = new List<Cell>();
        List<LineSegment> edges = new List<LineSegment>();

        // 母点の追加
        GenerateSite(area, sites, siteNum);

        // 上下左右に無限遠点を作成
        float VertsAbsMaxX = float.NegativeInfinity;
        float VertsAbsMaxY = float.NegativeInfinity;
        float halfWidth = 10f;
        int vertSize = area.GetVertices().Count;
        for (int i = 0; i < vertSize; i++)
        {
            VertsAbsMaxX = Mathf.Max(VertsAbsMaxX, area.GetVertex(i).x);
            VertsAbsMaxY = Mathf.Max(VertsAbsMaxY, area.GetVertex(i).y);
            halfWidth = Mathf.Max(VertsAbsMaxX, VertsAbsMaxY);
        }
        float width = halfWidth * 3f;
        float bigWidth = halfWidth * 10f;

        // 各無限遠点と原点からなる三角形を作成
        Vector2 C = new Vector2(0.0f, 0.0f);        // 原点
        Vector2 R = new Vector2(bigWidth, 0.0f);    // 右
        Vector2 T = new Vector2(0.0f, bigWidth);    // 上
        Vector2 L = new Vector2(-bigWidth, 0.0f);   // 左
        Vector2 B = new Vector2(0.0f, -bigWidth);   // 右

        // 三角形C-B-L
        Cell cell1 = new Cell(new Vector2(-width, -width));
        cell1.edges.Add(new LineSegment(C, L));
        cell1.edges.Add(new LineSegment(L, B));
        cell1.edges.Add(new LineSegment(B, C));
        cells.Add(cell1);

        // 三角形C-B-R
        Cell cell2 = new Cell(new Vector2(width, -width));
        cell2.edges.Add(new LineSegment(C, B));
        cell2.edges.Add(new LineSegment(B, R));
        cell2.edges.Add(new LineSegment(R, C));
        cells.Add(cell2);

        // 三角形C-T-R
        Cell cell3 = new Cell(new Vector2(width, width));
        cell3.edges.Add(new LineSegment(C, R));
        cell3.edges.Add(new LineSegment(R, T));
        cell3.edges.Add(new LineSegment(T, C));
        cells.Add(cell3);

        // 三角形C-T-L
        Cell cell4 = new Cell(new Vector2(-width, width));
        cell4.edges.Add(new LineSegment(C, T));
        cell4.edges.Add(new LineSegment(T, L));
        cell4.edges.Add(new LineSegment(L, C));
        cells.Add(cell4);

        // 1つずつ母点を追加
        for (int i = 0; i < sites.Count; i++)
        {
            // 母点からなるボロノイ領域を生成
            Cell newCell = new Cell(sites[i]);

            // 領域を1つずつ見る
            for (int j = 0; j < cells.Count; j++)
            {
                Cell existingCell = cells[j];

                // 垂直二等分線を見つける
                Vector2 vecBetween = (newCell.cellPos - existingCell.cellPos).normalized;   // 正規化ベクトル
                Vector2 pbVec = new Vector2(vecBetween.y, -vecBetween.x);
                Vector2 centerPos = (newCell.cellPos + existingCell.cellPos) * 0.5f;

                // 消去する点と辺を保持するリスト
                List<Vector2> criticalPoints = new List<Vector2>();
                List<LineSegment> edgesToDelete = new List<LineSegment>();

                // 注目している領域の辺を１つずつ見る
                for (int k = 0; k < existingCell.edges.Count; k++)
                {
                    LineSegment edge = existingCell.edges[k];

                    // existing→centerの直線をnewLineとする
                    // 辺の頂点がnewLineのどちら側にあるか調べる
                    Vector2 edge_p1 = edge.GetPoints()[0];
                    Vector2 edge_p2 = edge.GetPoints()[1];
                    // if the entire line is to the left of the line we are drawing now, delete it
                    // < 0 : 右     = 0 : 線上     > 0 : 左
                    float relation_p1 = DistanceFromPointToLine(vecBetween, centerPos, edge_p1);
                    float relation_p2 = DistanceFromPointToLine(vecBetween, centerPos, edge_p2);

                    // 辺の長さを正規化
                    Vector2 edgeDir = (edge_p2 - edge_p1).normalized;

                    float tolerance = 0.001f;

                    Vector2 edge_p1_expanded = edge_p1 - edgeDir * tolerance;
                    Vector2 edge_p2_expanded = edge_p2 + edgeDir * tolerance;

                    // どちらの頂点もnewLineの左側にあれば
                    float left_tolerance = -0.001f;
                    if ((relation_p1 > 0f && relation_p2 >= left_tolerance) || (relation_p2 > 0f && relation_p1 >= left_tolerance))
                    {
                        edgesToDelete.Add(edge);
                    }
                    // 辺とnewLineが交差していれば
                    else if (AreLinePlaneIntersecting(vecBetween, centerPos, edge_p1_expanded, edge_p2_expanded))
                    {
                        // 辺とnewlineの交点
                        Vector2 intersectionPoint = GetLinesIntersectionCoordinate(vecBetween, centerPos, edge_p1, edge_p2);

                        criticalPoints.Add(intersectionPoint);

                        // どちらの頂点もnewLineの右側にあれば
                        if (relation_p1 < 0f && relation_p2 < 0f)
                        {
                            // 辺の頂点の1つを交点に移動する
                            if (relation_p1 > relation_p2)
                            {
                                edge.GetPoints()[0] = intersectionPoint;
                            }
                            else
                            {
                                edge.GetPoints()[1] = intersectionPoint;
                            }
                        }
                        // 頂点がnewLineを跨いで存在していたら、左側の頂点を移動
                        // Approximately : 2つの浮動小数点値を比較し、近似している場合はtrueを返す
                        else if (relation_p1 > 0f || Mathf.Approximately(relation_p1, 0f))
                        {
                            // Debug.Log("!!");
                            existingCell.MoveEdgePoint(k, 0, intersectionPoint);
                            // existingCell.MoveVertex(edge.GetPoints()[0], intersectionPoint);
                            // edge.GetPoints()[0] = intersectionPoint;
                        }
                        else
                        {
                            // Debug.Log("!!");
                            existingCell.MoveEdgePoint(k, 1, intersectionPoint);
                            // existingCell.MoveVertex(edge.GetPoints()[1], intersectionPoint);
                            // edge.GetPoints()[1] = intersectionPoint;
                        }
                    }

                }

                // この時点でcriticalPointsは0か2のはず。2ならば新しい辺として追加
                if (criticalPoints.Count == 2)
                {
                    LineSegment newEdge = new LineSegment(criticalPoints[0], criticalPoints[1]);

                    existingCell.edges.Add(newEdge);
                    newCell.edges.Add(newEdge);
                    edges.Add(newEdge);
                }

                // 消す必要のある辺を消去
                for (int l = 0; l < edgesToDelete.Count; l++)
                {
                    // Debug.Log("!!");
                    existingCell.edges.Remove(edgesToDelete[l]);
                    edges.Remove(edgesToDelete[l]);
                }
            }
            cells.Add(newCell);
        }


        // 最初の4つの領域を消去
        cells.RemoveRange(0, 4);

        CCWSort(cells);

        return cells;
    }

    // 母点の追加
    private List<Vector2> GenerateSite(Polygon area, List<Vector2> sites, int addNum)
    {
        float VertsMaxX = float.NegativeInfinity;
        float VertsMaxY = float.NegativeInfinity;

        float VertsMinX = float.PositiveInfinity;
        float VertsMinY = float.PositiveInfinity;

        int vertSize = area.GetVertices().Count;

        for (int i = 0; i < vertSize; i++)
        {
            VertsMaxX = Mathf.Max(VertsMaxX, area.GetVertex(i).x);
            VertsMaxY = Mathf.Max(VertsMaxY, area.GetVertex(i).y);

            VertsMinX = Mathf.Min(VertsMinX, area.GetVertex(i).x);
            VertsMinY = Mathf.Min(VertsMinY, area.GetVertex(i).y);
        }

        Debug.Log("Max = " + VertsMaxX + ", " + VertsMaxY);
        Debug.Log("Min = " + VertsMinX + ", " + VertsMinY);

        for (int i = 0; i < addNum; i++)
        {
            // 母点生成
            Vector2 site = new Vector2(UnityEngine.Random.Range(VertsMinX, VertsMaxX), UnityEngine.Random.Range(VertsMinY, VertsMaxY));
            // Debug.Log(site);
            // 生成した母点が領域外ならやり直し
            if (!area.Contains(site.x, site.y))
            {
                i--;
                continue;
            }
            else
            {
                sites.Add(site);
            }
        }

        return sites;
    }

    // 点が線上か、右か、左にあるかを調べる
    private float DistanceFromPointToLine(Vector2 planeNormal, Vector2 planePos, Vector2 pointPos)
    {
        return Vector2.Dot(planeNormal, pointPos - planePos);
    }

    private bool AreLinePlaneIntersecting(Vector2 planeNormal, Vector2 planePos, Vector2 linePos1, Vector2 linePos2)
    {
        bool areIntersecting = false;

        Vector2 lineDir = (linePos1 - linePos2);
        float denominator = Vector2.Dot(-planeNormal, lineDir);     // 分母

        // No intersection if the line and plane are parallell
        if (denominator > 0.000001f || denominator < -0.000001f)
        {
            Vector2 vecBetween = planePos - linePos1;

            float t = Vector2.Dot(vecBetween, -planeNormal) / denominator;

            Vector2 intersectionPoint = linePos1 + lineDir * t;
            if (IsPointBetweenPoints(linePos1, linePos2, intersectionPoint))
            {
                areIntersecting = true;
            }
        }

        return areIntersecting;
    }

    // Is a point c between point a and b (we assume all 3 are on the same line)
    private bool IsPointBetweenPoints(Vector2 a, Vector2 b, Vector2 c)
    {
        bool isBetween = false;

        // Entire line segment
        Vector2 ab = b - a;
        // The intersection and the first point
        Vector2 ac = c - a;

        // Need to check 2 things: 
        // 1. If the vectors are pointing in the same direction = if the dot product is positive
        // 2. If the length of the vector between the intersection and the first point is smaller than the entire line
        if (Vector2.Dot(ab, ac) > 0f && ab.sqrMagnitude >= ac.sqrMagnitude) // 長さの2乗(ルートを取った形)
        {
            isBetween = true;
        }
        return isBetween;
    }

    private Vector2 GetLinesIntersectionCoordinate(Vector2 planeNormal, Vector2 planePos, Vector2 linePos1, Vector2 linePos2)
    {
        Vector2 vecBetween = planePos - linePos1;

        Vector2 lineDir = (linePos1 - linePos2).normalized;

        float denominator = Vector2.Dot(-planeNormal, lineDir);

        float t = Vector2.Dot(vecBetween, -planeNormal) / denominator;

        Vector2 intersectionPoint = linePos1 + lineDir * t;

        return intersectionPoint;
    }

    // 頂点を反時計回りにソート
    public void CCWSort(List<Cell> cells)
    {
        for (int i = 0; i < cells.Count; i++)
        {
            List<LineSegment> cellEdges = cells[i].edges;

            for (int j = cellEdges.Count - 1; j >= 0; j--)
            {
                Vector2 edge_v1 = cellEdges[j].GetPoints()[0];
                Vector2 edge_v2 = cellEdges[j].GetPoints()[1];

                // 短い辺を削除
                if ((edge_v1 - edge_v2).sqrMagnitude < 0.01f)
                {
                    cellEdges.RemoveAt(j);
                    continue;
                }

                Vector2 a = cells[i].cellPos;               // 母点
                Vector2 b = (edge_v1 + edge_v2) * 0.5f;     // 辺の中点

                // v1がabの左側にあれば
                if (GeomUtils.Cross(edge_v1 - a, edge_v1 - b) > 0f)
                {
                    // v1とv2を入れ替える
                    cells[i].MoveEdgePoint(j, 1, edge_v1);
                    cells[i].MoveEdgePoint(j, 0, edge_v2);
                }
            }

            // 辺の連結
            List<Vector2> edgesCoordinates = cells[i].borderCoordinates;

            LineSegment startEdge = cellEdges[0];

            edgesCoordinates.Add(startEdge.GetPoints()[1]);

            Vector2 currentVertex = startEdge.GetPoints()[1];

            for (int j = 1; j < cellEdges.Count; j++)
            {
                for (int k = 1; k < cellEdges.Count; k++)
                {
                    Vector2 thisEdgesStart = cellEdges[k].GetPoints()[0];

                    if ((thisEdgesStart - currentVertex).sqrMagnitude < 0.01f)
                    {
                        edgesCoordinates.Add(cellEdges[k].GetPoints()[1]);
                        currentVertex = cellEdges[k].GetPoints()[1];
                        break;
                    }
                }
            }

        }
    }
}

// ボロノイ領域
public class Cell
{
    public Vector2 cellPos;
    public List<LineSegment> edges = new List<LineSegment>();

    // 要ソート
    public List<Vector2> borderCoordinates = new List<Vector2>();

    public Cell(Vector2 cellPos)
    {
        this.cellPos = cellPos;
    }

    public void DrawCell(Color color)
    {
        foreach (LineSegment line in edges)
        {
            Debug.DrawLine(new Vector3(line.x1, 0.0f, line.y1), new Vector3(line.x2, 0.0f, line.y2), color, 10000);
        }
    }

    public void MoveEdgePoint(int index, int edgesIndesx, Vector2 distination)
    {
        edges[index].MovePoint(edgesIndesx, distination);
    }

    public List<Vector2> GetVerts()
    {
        return borderCoordinates;
    }

    public Polygon ConvertToPolygon()
    {
        return new Polygon(this.GetVerts());
    }
}



public class ClipVertex
{
    public Vector2 coordinate;

    public ClipVertex next;
    public ClipVertex prev;

    // 複数のポリゴンがあった場合に、そのポリゴンに飛ぶ
    public ClipVertex nextPoly;

    public bool isIntersection = false;

    // 交点が他のポリゴンに対する侵入点か
    public bool isEntry;

    public ClipVertex neighbor;

    // 交点があったとき、交わらない2つの頂点間の距離
    public float alpha = 0f;

    // 最終的に吐き出される領域の頂点か
    public bool isTakenByFinalPolygon;

    public ClipVertex(Vector2 coordinate)
    {
        this.coordinate = coordinate;
    }
}

public class GreinerHormann
{
    public List<List<Vector2>> Execute(List<Vector2> polyVector, List<Vector2> clipPolyVector)
    {
        List<List<Vector2>> finalPolygon = new List<List<Vector2>>();

        // データ構造の作成
        List<ClipVertex> poly = InitDataStructure(polyVector);
        List<ClipVertex> clipPoly = InitDataStructure(clipPolyVector);

        // 交点の確認
        bool hasFoundIntersection = false;

        for (int i = 0; i < poly.Count; i++)
        {
            ClipVertex currentVertex = poly[i];
            int iPlusOne = ClampListIndex(i + 1, poly.Count);

            Vector2 a = poly[i].coordinate;
            Vector2 b = poly[iPlusOne].coordinate;

            for (int j = 0; j < clipPoly.Count; j++)
            {
                int jPlusOne = ClampListIndex(j + 1, clipPoly.Count);

                Vector2 c = clipPoly[j].coordinate;
                Vector2 d = clipPoly[jPlusOne].coordinate;

                LineSegment ab = new LineSegment(a, b);
                LineSegment cd = new LineSegment(c, d);

                if (ab.Intersects(cd) == true && ab.GetIntersectionPoint(cd) != null)
                {
                    hasFoundIntersection = true;

                    Vector2 intersectionPoint = ab.GetIntersectionPoint(cd).Value;

                    // 交点を両方のポリゴンに挿入
                    ClipVertex vertexOnPolygon = InsertIntersectionVertex(a, b, intersectionPoint, currentVertex);
                    ClipVertex vertexOnClipPolygon = InsertIntersectionVertex(c, d, intersectionPoint, clipPoly[j]);

                    // 互いの交点を連結
                    vertexOnPolygon.neighbor = vertexOnClipPolygon;
                    vertexOnClipPolygon.neighbor = vertexOnPolygon;
                }
            }
        }

        if (hasFoundIntersection == true)
        {
            // それぞれのポリゴンをなぞって、ほかのポリゴンの内部への侵入点と退出点をマーク
            MarkEntryExit(poly, clipPolyVector);
            MarkEntryExit(clipPoly, polyVector);

            // ポリゴンの作成
            List<ClipVertex> intersectionVertices = GetClippedPolygon(poly, true);
            AddPolygonToList(intersectionVertices, finalPolygon, false);
        }
        // 交点がなければ
        else
        {
            if (IsPolygonInsidePolygon(polyVector, clipPolyVector))
            {
                Debug.Log("Poly is inside clip poly");
            }
            else if (IsPolygonInsidePolygon(clipPolyVector, polyVector))
            {
                Debug.Log("Clip poly is inside poly");
            }
            else
            {
                Debug.Log("Polygons are not intersecting");
                finalPolygon.Add(polyVector);
            }

        }

        return finalPolygon;
    }

    // 領域のデータ構造の作成
    private static List<ClipVertex> InitDataStructure(List<Vector2> polyVector)
    {
        List<ClipVertex> poly = new List<ClipVertex>();

        for (int i = 0; i < polyVector.Count; i++)
        {
            poly.Add(new ClipVertex(polyVector[i]));
        }

        // 頂点を連結
        for (int i = 0; i < poly.Count; i++)
        {
            int iPlusOne = ClampListIndex(i + 1, poly.Count);      // リストの1個次の要素を見る(ループする)
            int iMinusOne = ClampListIndex(i - 1, poly.Count);

            poly[i].next = poly[iPlusOne];
            poly[i].prev = poly[iMinusOne];
        }

        return poly;
    }

    // リストの要素を取得(インデックスがオーバーしたときはリストをループ)
    private static int ClampListIndex(int index, int listSize)
    {
        index = ((index % listSize) + listSize) % listSize;

        return index;
    }

    // 交点をリストに挿入する
    private static ClipVertex InsertIntersectionVertex(Vector2 a, Vector2 b, Vector2 intersectionPoint, ClipVertex currentVertex)
    {
        // 交点座標がa, bの間にどれくらいの距離があるのかを計算
        float alpha = (a - intersectionPoint).sqrMagnitude / (a - b).sqrMagnitude;

        ClipVertex intersectionVertex = new ClipVertex(intersectionPoint);

        intersectionVertex.isIntersection = true;
        intersectionVertex.alpha = alpha;

        // 交点をcurrentVertexの後ろに挿入
        ClipVertex insertAfterThisVertex = currentVertex;
        int safty = 0;
        while (true)
        {
            if (insertAfterThisVertex.next.alpha > alpha || insertAfterThisVertex.next.isIntersection == false)
            {
                break;
            }

            insertAfterThisVertex = insertAfterThisVertex.next;

            safty++;
            if (safty > 100000)
            {
                Debug.Log("スタックしちゃった");
                break;
            }
        }

        intersectionVertex.next = insertAfterThisVertex.next;
        intersectionVertex.prev = insertAfterThisVertex;
        insertAfterThisVertex.next.prev = intersectionVertex;
        insertAfterThisVertex.next = intersectionVertex;

        return intersectionVertex;
    }

    private static void MarkEntryExit(List<ClipVertex> poly, List<Vector2> clipPolyVector)
    {
        // clipPolyの内側か外側か判定
        Polygon clipPolygon = new Polygon(clipPolyVector);
        bool isInside = clipPolygon.Contains(poly[0].coordinate);

        ClipVertex currentVertex = poly[0];
        ClipVertex firstVertex = currentVertex;
        int safty = 0;

        while (true)
        {
            if (currentVertex.isIntersection)
            {
                // 外側にあったら、侵入点
                currentVertex.isEntry = isInside ? false : true;

                // 状態が逆転
                isInside = !isInside;
            }

            // 次の頂点へ
            currentVertex = currentVertex.next;

            // 一周したらおしまい
            if (currentVertex.Equals(firstVertex))
            {
                break;
            }
            safty++;

            if (safty > 100000)
            {
                Debug.Log("無限ループになったぞ");
                break;
            }
        }
    }

    private static List<ClipVertex> GetClippedPolygon(List<ClipVertex> poly, bool getIntersectionPolygon)
    {
        List<ClipVertex> finalPolygon = new List<ClipVertex>();

        // 状態をリセット
        ResetVertices(poly);

        // 最初の交点(侵入点)を見つける
        ClipVertex thisVertex = FindFirstEntryVertex(poly);

        ClipVertex firstVertex = thisVertex;

        finalPolygon.Add(thisVertex);
        thisVertex.isTakenByFinalPolygon = true;
        thisVertex.neighbor.isTakenByFinalPolygon = true;

        // getIntersectionPolygonがtrueならtrueを返す(意味ある？)
        bool isMovingForward = getIntersectionPolygon ? true : false;

        thisVertex = getIntersectionPolygon ? thisVertex.next : thisVertex.prev;
        int safety = 0;

        while (true)
        {
            // スタート地点に戻ったら(一周したら)
            if (thisVertex.Equals(firstVertex) || thisVertex.neighbor != null && thisVertex.neighbor.Equals(firstVertex))
            {
                // 次の交点(侵入点)を探す
                ClipVertex nextVertex = FindFirstEntryVertex(poly);

                if (nextVertex == null)
                {
                    break;
                }
                else
                {
                    finalPolygon[finalPolygon.Count - 1].nextPoly = nextVertex;
                    thisVertex = nextVertex;
                    firstVertex = nextVertex;
                    finalPolygon.Add(thisVertex);
                    thisVertex.isTakenByFinalPolygon = true;
                    thisVertex.neighbor.isTakenByFinalPolygon = true;
                    isMovingForward = getIntersectionPolygon ? true : false;
                    thisVertex = getIntersectionPolygon ? thisVertex.next : thisVertex.prev;
                }
            }
            // 交点でないなら、リストに追加
            if (thisVertex.isIntersection == false)
            {
                finalPolygon.Add(thisVertex);
                thisVertex = isMovingForward ? thisVertex.next : thisVertex.prev;
            }
            // 交点なら
            else
            {
                thisVertex.isTakenByFinalPolygon = true;
                thisVertex.neighbor.isTakenByFinalPolygon = true;

                // 相方の交点に注目
                thisVertex = thisVertex.neighbor;

                // リストに追加
                finalPolygon.Add(thisVertex);

                // 相方ポリゴンから前に進むか、後ろに進むか決定(積を求めるから、多分true)
                if (getIntersectionPolygon == true)
                {
                    isMovingForward = thisVertex.isEntry ? true : false;
                    thisVertex = thisVertex.isEntry ? thisVertex.next : thisVertex.prev;
                }
                else
                {
                    isMovingForward = !isMovingForward;
                    thisVertex = isMovingForward ? thisVertex.next : thisVertex.prev;
                }
            }

            safety++;
            if (safety > 100000)
            {
                Debug.Log("無限ループから抜け出しておいたぞ");
                break;
            }
        }

        return finalPolygon;
    }

    private static void ResetVertices(List<ClipVertex> poly)
    {
        ClipVertex resetVertex = poly[0];
        int safty = 0;

        while (true)
        {
            // リセット
            resetVertex.isTakenByFinalPolygon = false;
            resetVertex.nextPoly = null;

            // 交点ならば、相方もリセット
            if (resetVertex.isIntersection == true)
            {
                resetVertex.neighbor.isTakenByFinalPolygon = false;
            }

            resetVertex = resetVertex.next;

            // 全部リセット
            if (resetVertex.Equals(poly[0]))
            {
                // 一周したらおしまい
                break;
            }

            safty++;
            if (safty > 100000)
            {
                Debug.Log("無限ループだぞ");
                break;
            }
        }
    }

    private static ClipVertex FindFirstEntryVertex(List<ClipVertex> poly)
    {
        ClipVertex thisVertex = poly[0];
        ClipVertex firstVertex = thisVertex;

        int safety = 0;

        while (true)
        {
            if (thisVertex.isIntersection == true && thisVertex.isEntry == true && thisVertex.isTakenByFinalPolygon == false)
            {
                // 交点で、なおかつ侵入点(未検出)ならば検出完了
                break;
            }

            thisVertex = thisVertex.next;

            // 侵入点がなかったら終わり
            if (thisVertex.Equals(firstVertex))
            {
                thisVertex = null;
                break;
            }

            safety++;
            if (safety > 100000)
            {
                Debug.Log("無限ループしてるから終わっておいたぞ");
                break;
            }
        }
        return thisVertex;
    }

    // 連結しているポリゴンをそれぞれリストに追加
    private static void AddPolygonToList(List<ClipVertex> verticesToAdd, List<List<Vector2>> finalPoly, bool shouldReverse)
    {
        List<Vector2> thisPolyList = new List<Vector2>();

        finalPoly.Add(thisPolyList);

        for (int i = 0; i < verticesToAdd.Count; i++)
        {
            ClipVertex v = verticesToAdd[i];
            thisPolyList.Add(v.coordinate);

            // 新しいポリゴンを見つけたら
            if (v.nextPoly != null)
            {
                // 反時計回りにソート
                if (shouldReverse == true)
                {
                    thisPolyList.Reverse();
                }
                // 新しいリスト作成
                thisPolyList = new List<Vector2>();
                finalPoly.Add(thisPolyList);
            }
        }
        if (shouldReverse)
        {
            finalPoly[finalPoly.Count - 1].Reverse();
        }
    }

    // ポリゴンAがポリゴンBの中にあるか
    private static bool IsPolygonInsidePolygon(List<Vector2> polyA, List<Vector2> polyB)
    {
        bool isInside = false;
        Polygon B = new Polygon(polyB);

        for (int i = 0; i < polyA.Count; i++)
        {
            if (B.Contains(polyA[i]) == true)
            {
                isInside = true;
                break;
            }
        }
        return isInside;
    }

}

public class GeomUtils
{
    // 外積
    public static float Cross(float x1, float y1, float x2, float y2)
    {
        return x1 * y2 - x2 * y1;
    }

    public static float Cross(Vector2 p1, Vector2 p2)
    {
        return Cross(p1.x, p1.y, p2.x, p2.y);
    }

    // 内積
    public static float Dot(float x1, float y1, float x2, float y2)
    {
        return x1 * x2 + y1 * y2;
    }

    // Vector2からVector3
    public static Vector3 VectorExtend(Vector2 vec)
    {
        return new Vector3(vec.x, 0.0f, vec.y);
    }

    // 外積の大きさによって2辺が時計回りか反時計回り化を調べる(時計回りなら負)
    public static float CounterClockWise(float x1, float y1, float x2, float y2, float x3, float y3)
    {
        return Cross(x2 - x1, y2 - y1, x3 - x2, y3 - y2);
    }
    public static float CounterClockWise(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        return CounterClockWise(p1.x, p1.y, p2.x, p2.y, p3.x, p3.y);
    }
}


