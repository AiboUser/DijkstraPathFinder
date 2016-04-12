using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 有向加权地图烘培
/// </summary>
public class MapBaker : EditorWindow
{
    [MenuItem("Tools/MapBaker")]
    public static void BakeMap()
    {
        EditorWindow.GetWindow<MapBaker>();
    }

    public void OnGUI()
    {
        GUILayout.BeginVertical();


        GUILayout.Label("蓝色线条表示可以跳跃到达");
        GUILayout.Label("绿色线条表示可以行走到达");
        GUILayout.Label("红色线条表示无法到达");

        bool showINF = GUILayout.Toggle(MapTag.showINF, "显示不能倒达的节点连线");
        if (showINF != MapTag.showINF)
        {
            MapTag.showINF = showINF;
            DirtyGizmo();
        }

        bool showAllGizmo = GUILayout.Toggle(MapTag.showAllGizmo, "显示所有对象的连线");
        if (showAllGizmo != MapTag.showAllGizmo)
        {
            MapTag.showAllGizmo = showAllGizmo;
            DirtyGizmo();
        }

        if(GUILayout.Button("展示所有节点"))
        {
            ScanWholeHierarchy();
        }

        if (GUILayout.Button("烘培地图"))
        {
            FillAllMissingTag();
        }

        foreach(var gop in allMapTag)
        {
            GUILayout.Toggle(gop.Value, gop.Key.name);
        }        

        GUILayout.EndVertical();
    }

    Dictionary<GameObject, bool> allMapTag = new Dictionary<UnityEngine.GameObject, bool>();

    HashSet<GameObject> allGameObject = new HashSet<UnityEngine.GameObject>();

    void ScanWholeHierarchy()
    {
        allMapTag.Clear();

        MapTag[] allMaptag = GameObject.FindObjectsOfType<MapTag>();

        //旧版本遗留 
        foreach(var tg in allMaptag )
        {
            if( (int)tg.distance == - 1)
            {
                tg.distance = PathFinder.MapDistance.INF;
            }
        }

        for (int i = 0; i < allMaptag.Length; ++i)
        {
            MapTag tag = allMaptag[i];

            if( !allMapTag.ContainsKey(tag.gameObject))
            {
                allMapTag.Add(tag.gameObject, true);
            }

            if (!allGameObject.Contains(tag.gameObject))
            {
                allGameObject.Add(tag.gameObject);
            }
        }
    }

    void FillAllMissingTag()
    {
        foreach (var gop in allMapTag)
        {
            FillMissingTag(gop.Key, allMapTag);
        }

        EditorUtility.DisplayDialog("完毕", "完毕", "ok");
    }

    /// <summary>
    /// 为对象添加tag，这些tag没有被编辑器手动添加
    /// </summary>
    /// <param name="gameObject"></param>
    /// <param name="allMap"></param>
    void FillMissingTag( GameObject gameObject, Dictionary<GameObject, bool> allMap )
    {
        foreach(var gop in allMapTag)
        {
            List<MapTag> alltag = new List<MapTag>(gameObject.GetComponents<MapTag>());

            var nullTag = alltag.FindAll(t => t.target == null);            
           
            //移除没有手动设置目标的节点
            int count = alltag.RemoveAll(t=>t.target==null);
            if( count > 0 )
            {
                Debug.LogWarning("发现没有手动设置目标的节点， 请检查地图");
            }

            foreach (var t in nullTag)
            {
                GameObject.DestroyImmediate(t);
            }

            //为该gameObject 添加那些需要补充的其他节点， 并且标记为不可移动到或者自己
            if (alltag.Find(t => t.target == gop.Key) == null)
            {
                var newTag = gameObject.AddComponent<MapTag>();
                newTag.target = gop.Key;
                newTag.distance = gop.Key == gameObject ? PathFinder.MapDistance.Self : PathFinder.MapDistance.INF;
            }            
        }
    }

    /// <summary>
    /// 顶点数量
    /// </summary>
    int vertexCount;

    /// <summary>
    /// 边的数量
    /// </summary>
    int edgetCount;

    class StructEdge
    {
        public MapTag P0;
        public MapTag P1;
    }

    void CheckMap() 
    {
        MapTag[] allMaptag = GameObject.FindObjectsOfType<MapTag>();
        
        HashSet<GameObject> allVertex = new HashSet<UnityEngine.GameObject>();

        foreach (var tag in allMaptag)
        {
            if(! allVertex.Contains(tag.gameObject) )
                allVertex.Add(tag.gameObject);
        }

        //顶点数量
        vertexCount = allVertex.Count;       

        //计算边
        int edge = 0;

        List<StructEdge> mapTag = new List<StructEdge>();

        //这个对象上指向其他节点的距离不为自己和空 则为边
        foreach (var tag in allVertex)
        {
            MapTag[] tags = tag.GetComponents<MapTag>();

            edge += tags.Where(t => t.distance != PathFinder.MapDistance.INF && t.distance != PathFinder.MapDistance.Self).Count();          
        }

        if(edgetCount != vertexCount + 1 )
        {
            Debug.LogError("某些顶点无法联通, 请检查连通性");
        }
        else
        {
            Debug.Log("合法");
        }
    }

    /// <summary>
    /// 触发gizmo重绘
    /// </summary>
    public void DirtyGizmo()
    {
        MapTag[] allMaptag = GameObject.FindObjectsOfType<MapTag>();
        
        foreach(var t in allMaptag)
        {
            EditorUtility.SetDirty(t);
        }
    }
}
