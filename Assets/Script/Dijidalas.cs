using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Dijidalas
{
//     int[,] mapData = new int[,] {
//         {0,     10,     INF,    20,     INF },
//         {10,    0,      10,     20,     INF},
//         {INF,   10,     0,      INF,    INF},
//         {10,    10,     INF,    0,      20},
//         {INF,   INF,    INF,     20,     0}
//     };
// 
//     int mapLen = 5;

    int[,] mapData;
    int mapLen;

    public const int FAIL = -1;

    public const int INF = (int)(0x0FFFFFFF);

    public void SetMapData( int[,] data, int len )
    {
        mapData = data;
        mapLen = len;
    }

    int GetLen(int l, int r)
    {
        return mapData[l, r];
    }

    public void SearchPath(int s, out int[] P, out int[] D )
    {
        if (mapData == null || mapLen == 0)
        {//Data not set
            P = null;
            D = null;
            return;
        }

        //标记所有点没有被使用
        bool[] mark = new bool[mapLen];

        //标记起点被使用
        mark[s] = true;

        //记录长度
        D = new int[mapLen];

        //搜集集合
        P = new int[mapLen];

        //初始化起点到原点的距离
        for (int i = 0; i < mapLen; ++i)
        {
            int distance = GetLen(s, i);
            if(distance != INF && i != s )
            {
                D[i] = distance;
                P[i] = s;//path记录最短路径上从v0到i的前一个顶点 
            }            
            else
            {
                D[i] = distance;//若i不与v0直接相邻，则权值置为无穷大 
                P[i] = FAIL;
            }
        }

        D[s] = 0;

        for (int circle = 1; circle < mapLen; ++circle)
        {
            int u = s;
            int minLen = INF;

            //遍历所有节点 然后选择出距离最小的点
            for (int i = 0; i < mapLen; ++i)
            {
                if (!mark[i] && D[i] < minLen)
                {
                    u = i;
                    minLen = D[i];
                }
            }

            //设置这个点为目标点
            mark[u] = true;

            //更新长度
            for (int j = 0; j < mapLen; ++j)
            {
                //int newlen = GetLen(u, j) == invalid ? invalid : (minLen + GetLen(u, j));
                if (!mark[j] && GetLen(u, j) != INF)
                {
                    int newDis = minLen + GetLen(u, j);

                    if (newDis < D[j])
                    {
                        D[j] = newDis;
                        P[j] = u;
                    }
                }
            }
        }
    }
}