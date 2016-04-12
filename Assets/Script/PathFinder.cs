using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class PathFinder : MonoBehaviour
{
    Dijidalas _dijidalas = new Dijidalas();

    Dictionary<MapTagIndex, List<MapTag>> _allMaptags = new Dictionary<MapTagIndex, List<MapTag>>();

    int[,] _mapBinaryData;

    List<MapTagIndex> _allGameObject;

    public enum MapDistance
    {
        Self = 0,
        Walk = 10,
        Jump = 20,
        INF = Dijidalas.INF,
    }

    void Start()
    {
        InitMap();
    }

    void InitMap()
    {
        GenMapOrder();

        AddAllGameObject(_allMaptags);
    }

    void AddAllGameObject(Dictionary<MapTagIndex, List<MapTag>> allMapTags)
    {
        _allGameObject = new List<MapTagIndex>();

        foreach (var kv in allMapTags)
        {
            _allGameObject.Add(kv.Key);
        }

        _allGameObject.Sort((l, r) =>
        {
            if (l.tagIndex < r.tagIndex)
                return -1;
            else if (l.tagIndex == r.tagIndex)
                return 0;
            return 1;
        });
    }

    void GenMapOrder()
    {
        MapTag[] mapTags = GameObject.FindObjectsOfType<MapTag>();

        //分配序列号
        int mapTagIndex = 0;
        for(int i = 0; i < mapTags.Length; ++ i )
        {
            MapTag mapTag = mapTags[i];            

            MapTagIndex index = mapTag.GetComponent<MapTagIndex>();
            if( index == null )
            {
                index = mapTag.gameObject.AddComponent<MapTagIndex>();
                index.tagIndex = mapTagIndex++;
            }

            List<MapTag> tagDest;
            if (_allMaptags.TryGetValue(index, out tagDest))
            {
                tagDest.Add(mapTag);
            }
            else
            {
                tagDest = new List<MapTag>();
                tagDest.Add(mapTag);
                _allMaptags.Add(index, tagDest);
            }      
        }

        int len = 0;
        if (CheckMap(out len))
        {
            if (FillMapData(len))
            {
                _dijidalas.SetMapData(_mapBinaryData, len);
            }
        }
        else
        {
            Debug.LogWarning("检查地图失败, 地图可能为空");
        }
    }

    /// <summary>
    /// 填充二进制的地图数据
    /// </summary>
    /// <param name="len"></param>
    /// <returns></returns>
    bool FillMapData(int len ) 
    {
        _mapBinaryData = new int[len, len];

        foreach(var kv in _allMaptags)
        {
            if (kv.Key.tagIndex >= 0 && kv.Key.tagIndex < len)
            {
                foreach(var value in kv.Value)
                {
                    int index = IndexOf(value.target);
                    if( index < 0 || index >= len )
                    {
                        Debug.LogError("索引失败, 索引超出地图最大范围或者索引为负");
                        return false;
                    }

                    _mapBinaryData[kv.Key.tagIndex, index] = (int)value.distance;
                }
            }
            else
            {
                Debug.LogError("索引失败, 索引超出地图最大范围或者索引为负");
                return false;
            }
        }
        return true;
    }

    int IndexOf(GameObject target)
    {
        var indexList = _allMaptags.Where(t => UnityEngine.Object.ReferenceEquals(t.Key.gameObject, target)).ToList();
        if( indexList.Count == 0 )
            return -1;
        return indexList[0].Key.tagIndex;
    }

    bool CheckMap(out int len )
    {
        len = 0;

        foreach(var kv in _allMaptags)
        {
            if (len == 0)
                len = kv.Value.Count;
            
            if( kv.Value.Count != len )
            {
                Debug.LogError("检查地图失败, 节点数量不匹配, 名称: " + kv.Key.gameObject.name );
                return false;
            }
        }

        Debug.Log("统计结束, 地图节点数量:" + len);
        return len != 0 ;
    }

    List<int> GetPath(int s, int dest)
    {
        int[] p;
        int[] d;

        _dijidalas.SearchPath(s, out p, out d);

        List<int> path = new List<int>();

        int prev = dest;

        path.Add(dest);

        //循环查找前驱点来寻路
        while (true)
        {
            if (prev == Dijidalas.FAIL || p[prev] == Dijidalas.FAIL) //失败
            {
                //查找前驱点失败, 说明这个无法到达
                Debug.LogError("Fail!");
                break;
            }

            int ps = p[prev];

            path.Add(ps);

            if (ps == s)
                break;

            prev = ps;
        }

        return path;
    }

    int Get(string name)
    {
        var result = _allGameObject.Find(t => t.name == name);
        if (result != null)
            return result.tagIndex;
        return -1;
    }


    #region 测试
    void TestRandom()
    {
        //_thisPath = SeachFrom(UnityEngine.Random.Range(0, _allMaptags.Count), UnityEngine.Random.Range(0, _allMaptags.Count));
        _thisPath = SeachFrom( Get("D"), Get("R3"));
        if (_thisPath.Count == 0)
            return;

        _pathIndex = 0;
        _elapse = 0;
        _path = UpdatePath.Updating;
    }

    void Test(int s, int d)
    {
        List<int> path = GetPath(s, d);
    }

    void TestAll()
    {
        for (int x = 0; x < _allGameObject.Count; ++x)
        {
            var testX = _allGameObject[x];
            for (int y = 0; y < _allGameObject.Count; ++y)
             {
                 var testY = _allGameObject[y];

                 Test(testX.tagIndex, testX.tagIndex);
             }
        }
    }
    
    List<int> _thisPath = new List<int>();

    List<int> SeachFrom(int s, int d)
    {
#if UNITY_EDITOR
        //打印起点和终点
        foreach (var kv in _allMaptags)
        {
            if (kv.Key.tagIndex == s)
            {
                Debug.Log("start: " + kv.Key.gameObject.name);
            }
            else if (kv.Key.tagIndex == d)
            {
                Debug.Log("ends: " + kv.Key.gameObject.name);
            }
        }        
#endif
        return GetPath(s, d);
    }

    int _pathIndex = 0;
    float _elapse = 0;
    public GameObject _agent;

    enum UpdatePath
    {
        Stopped = 0,
        Updating = 1,
    }

    UpdatePath _path;
    void Update()
    {
        if (_path == UpdatePath.Stopped)
            return;

        _elapse += Time.deltaTime;

        if(_elapse > 2 )
        {
            _elapse = 0;
            _pathIndex++;
        }

        if (_pathIndex >= _thisPath.Count)
        {
            _path = UpdatePath.Stopped; 
            return;
        }

        if(_pathIndex>0)
        {
            _agent.transform.position = Vector3.Lerp(_allGameObject[_thisPath[_pathIndex - 1]].transform.position, _allGameObject[_thisPath[_pathIndex]].transform.position, _elapse);
        }
        else
        {
            _agent.transform.position = _allGameObject[_thisPath[_pathIndex]].transform.position;
        }
    }

    void OnGUI()
    {
        GUILayout.BeginVertical();

        if(GUILayout.Button("seach") )
        {
            TestAll();
        }
        if (GUILayout.Button("seach random"))
        {
            TestRandom();
        }


        GUILayout.EndVertical();
    }
    #endregion
}
