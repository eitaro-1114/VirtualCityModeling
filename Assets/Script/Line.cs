using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Line
{
    public float a;
    public float b;
    public float c;

    public Line(float a, float b, float c)
    {
        this.a = a;
        this.b = b;
        this.c = c;
    }

    public static Line FromPoints(float x1, float y1, float x2, float y2)
    {
        float dx = x2 - x1;
        float dy = y2 - y1;

        return new Line(dy, -dx, dx * y1 - dy * x1);
    }

    public Vector2? getIntersectionPoint(Line l)
    {
        float d = a * l.b - l.a * b;
        // 直線が平行のとき
        if (d == 0.0f)
        {
            return null;
        }

        float x = (b * l.c - l.b * c) / d;
        float y = (l.a * c - a * l.c) / d;

        return new Vector2(x, y);
    }

    // p1とp2の垂直二等分線を作成
    public static Line PerpendicularBisector(float x1, float y1, float x2, float y2)
    {
        float cx = (x1 + x2) / 2.0f;
        float cy = (y1 + y2) / 2.0f;
        // // Debug.DrawLine(new Vector3(cx, 0, cy), new Vector3(cx + (y1 - y2), 0, cy + (x2 - x1)), Color.white, 10000);
        return FromPoints(cx, cy, cx + (y1 - y2), cy + (x2 - x1));
    }

    public List<Vector2> PointsOnLine()
    {
        Vector2 p1 = new Vector2(100, (a * 100 + c) / b);
        Vector2 p2 = new Vector2(-100, (a * -100 + c) / b);

        List<Vector2> points = new List<Vector2>();
        points.Add(p1);
        points.Add(p2);

        return points;
    }

    public override string ToString()
    {
        return "a = " + a + ", b = " + b + " , c = " + c;
    }
}

public class LineSegment
{
    public float x1;
    public float y1;
    public float x2;
    public float y2;

    public LineSegment(float x1, float y1, float x2, float y2, bool drawLine)
    {
        this.x1 = x1;
        this.y1 = y1;
        this.x2 = x2;
        this.y2 = y2;

        if (drawLine)
        {
            // Debug.DrawLine(new Vector3(x1, 0.0f, y1), new Vector3(x2, 0.0f, y2), Color.white, 10000);
        }
    }

    public LineSegment(Vector2 p1, Vector2 p2)
    {
        x1 = p1.x;
        y1 = p1.y;
        x2 = p2.x;
        y2 = p2.y;
    }

    public override string ToString()
    {
        return "(" + x1 + ", " + y1 + ")" + "(" + x2 + ", " + y2 + ")";
    }

    // 直線へ変換
    public Line ToLine()
    {
        return Line.FromPoints(x1, y1, x2, y2);
    }

    // 交差判定(直線)
    public bool Intersects(Line l)
    {
        // 端点を直線の式に代入
        float t1 = l.a * x1 + l.b * y1 + l.c;
        float t2 = l.a * x2 + l.b * y2 + l.c;

        return t1 * t2 <= 0;
    }

    // 交差判定(線分) つながっていたらfalse
    public bool Intersects(LineSegment s)
    {
        Vector2 thisSegmentA = new Vector2(x1, y1);
        Vector2 thisSegmentB = new Vector2(x2, y2);

        Vector2 otherSegmentA = new Vector2(s.x1, s.y1);
        Vector2 otherSegmentB = new Vector2(s.x2, s.y2);

        //if (thisSegmentA == otherSegmentA || thisSegmentA == otherSegmentB || thisSegmentB == otherSegmentA || thisSegmentB == otherSegmentB)
        //{
        //    return false;
        //}
        return Intersects(s.ToLine()) && s.Intersects(ToLine());
        // Debug.Log(BothSides(s) && BothSides(this));
        // eturn BothSides(s) && BothSides(this);
    }

    // sが自分の線分の両側にあるかを調べる
    private bool BothSides(LineSegment s)
    {
        float ccw1 = GeomUtils.CounterClockWise(x1, y1, s.x1, s.y1, x2, y2);
        float ccw2 = GeomUtils.CounterClockWise(x1, y1, s.x2, s.y2, x2, y2);

        // sと自分の線分が一直線上にある場合
        if (ccw1 == 0 && ccw2 == 0)
        {
            // sの端点が自分の線分を内分していればtrueを返す
            return Internal(s.x1, s.y1) || Internal(s.x2, s.y2);
        }
        else
        {
            // CCW値の符号が異なる場合にtrueを返す
            return ccw1 * ccw2 <= 0;
        }
    }

    // (x, y)が自分の線分を内分しているか調べる
    private bool Internal(float x, float y)
    {
        // (x, y)から端点に向かうベクトルの内積が0以下なら内分
        return GeomUtils.Dot(x1 - x, y1 - y, x2 - x, y2 - y) <= 0;
    }

    public void DrawSegment(Color color)
    {
        Debug.DrawLine(new Vector3(x1, 0.0f, y1), new Vector3(x2, 0.0f, y2), color, 100000);
    }

    // 交点座標取得(直線)
    public Vector2? GetIntersectionPoint(Line l)
    {
        if (!Intersects(l))
        {
            return null;
        }
        return l.getIntersectionPoint(ToLine());
    }

    // 交点座標取得(線分)
    public Vector2? GetIntersectionPoint(LineSegment s)
    {
        if (!Intersects(s))
        {
            return null;
        }
        return s.ToLine().getIntersectionPoint(ToLine());
    }

    // 端点の取得
    public List<Vector2> GetPoints()
    {
        List<Vector2> points = new List<Vector2>();
        points.Add(new Vector2(x1, y1));
        points.Add(new Vector2(x2, y2));
        return points;
    }

    // 端点の移動
    public void MovePoint(int index, Vector2 distination)
    {
        switch (index)
        {
            case 0:
                x1 = distination.x;
                y1 = distination.y;
                break;
            case 1:
                x2 = distination.x;
                y2 = distination.y;
                break;
            default:
                throw new Exception("インデックスは0か1だぞ");
        }
    }

    public bool isSameSegment(LineSegment ls)
    {
        bool isSame = false;
        if(this.GetPoints()[0].Equals(ls.GetPoints()[0]) == true || this.GetPoints()[0].Equals(ls.GetPoints()[1]) == true)
        {
            if(this.GetPoints()[1].Equals(ls.GetPoints()[0]) == true || this.GetPoints()[1].Equals(ls.GetPoints()[1]) == true)
            {
                isSame = true;
            }
        }
        return isSame;
    }

}

// 線分の交差を体現するクラス
public class Intersection
{
    public LineSegment segment1;
    public LineSegment segment2;

    public Intersection(LineSegment segment1, LineSegment segment2)
    {
        this.segment1 = segment1;
        this.segment2 = segment2;
    }

    // 交点取得
    public Vector2? GetIntersectionPoint()
    {
        return segment1.GetIntersectionPoint(segment2);
    }

    public override string ToString()
    {
        return base.ToString();
    }

    public override bool Equals(object obj)
    {
        if (obj == this)
        {
            return true;
        }
        else if (obj is Intersection)
        {
            Intersection other = (Intersection)obj;

            // segment1とsegment2を交換しても同値性を変えないようにする
            if (segment1.Equals(other.segment1) && segment2.Equals(other.segment2))
            {
                return true;
            }
            else if (segment1.Equals(other.segment2) && segment2.Equals(other.segment1))
            {
                return true;
            }
        }
        return false;
    }

    // segment1とsegment2を交換してもハッシュ値が変わらないようにする
    public override int GetHashCode()
    {
        return segment1.GetHashCode() + segment2.GetHashCode();
    }

}

// 線分のリストを入力とし，その中から交差を見つけ，Intersectionオブジェクトのコレクションとして返すexecute()メソッドを持つインタフェース
public interface IntersectionDetector
{
    ICollection<Intersection> excute(List<LineSegment> segments);
}

// 総当たり
public class BruteForceIntersectionDetector : IntersectionDetector
{
    public ICollection<Intersection> excute(List<LineSegment> segments)
    {
        List<Intersection> result = new List<Intersection>();
        int size = segments.Count;
        for (int i = 0; i < size; i++)
        {
            LineSegment s1 = segments[i];
            for (int j = i + 1; j < size; j++)
            {
                LineSegment s2 = segments[j];
                if (s1.Intersects(s2))
                {
                    result.Add(new Intersection(s1, s2));
                }
            }
        }
        return result;
    }
}

// イベントクラス
public class Event : IComparable<Event>
{
    // イベントの種類
    public enum Type
    {
        SEGMENT_START,  // 線分の始点
        SEGMENT_END,    // 線分の終点
        INTERSECTION    // 線分同士の交差
    }

    public Type type;
    public float x;
    public float y;

    // 点に関連する線分
    public LineSegment segment1;
    public LineSegment segment2;

    public Event(Type type, float x, float y, LineSegment segment1, LineSegment segment2)
    {
        this.type = type;
        this.x = x;
        this.y = y;
        this.segment1 = segment1;
        this.segment2 = segment2;
    }

    // インタフェースの実装
    public int CompareTo(Event e)
    {
        int c = y.CompareTo(e.y);
        // y座標が等しいときはx座標を比較
        if (c == 0)
        {
            c = x.CompareTo(e.x);
        }
        return c;
    }
}

// 走査線と線分の交点のx座標にもとづく順序の比較
public class SweepLineBasedComparator : IComparer<LineSegment>
{
    private Line sweepLine;
    private Line belowLine;

    public SweepLineBasedComparator()
    {
        setY(0);
    }

    // 走査線のy座標を更新
    public void setY(float y)
    {
        // 走査線を更新
        sweepLine = Line.FromPoints(0, y, 1, y);
        // 走査線のちょっぴり下にある直線生成
        belowLine = Line.FromPoints(0, y + 0.1f, 1, y + 0.1f);
    }

    // Compare<LineSegment>の実装
    public int Compare(LineSegment s1, LineSegment s2)
    {
        int c = CompareByLine(s1, s2, sweepLine);

        // 捜査線上の交点が等しいとき、走査線のちょっぴり下にある直線で比較
        if (c == 0)
        {
            c = CompareByLine(s1, s2, belowLine);
        }
        return c;
    }

    // s1とs2をlineとの交点のx座標に基づいて比較
    private int CompareByLine(LineSegment s1, LineSegment s2, Line line)
    {
        Vector2? p1 = s1.ToLine().getIntersectionPoint(line);
        Vector2? p2 = s2.ToLine().getIntersectionPoint(line);

        float x1;
        float x2;

        if (p1 == null)
        {
            x1 = s1.x1;
        }
        else
        {
            x1 = p1.Value.x;
        }

        if (p2 == null)
        {
            x2 = s2.x1;
        }
        else
        {
            x2 = p2.Value.x;
        }

        return x1.CompareTo(x2);
    }
}
