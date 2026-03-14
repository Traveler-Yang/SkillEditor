using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;
using System;

public class 技能编辑器Window : EditorWindow
{
    public int 技能ID;
    public string 显示名称;
    public string 技能描述;
    public int 目标检测类型SelectIndex;
    private string[] 目标检测类型array = new string[]
    {
        "敌方",
        "友方",
        "友方除自己",
        "全体除自己"
    };

    public int 技能目标类型SelectIndex;
    private string[] 技能目标类型array = new string[]
    {
        "对面向",
        "区域",
        "单体锁定",
        "对自己",
    };

    private Texture 技能Icon;
    public string Icon;
    public int 技能类型SelectIndex;//加SelectIndex的为选择确定类型的
    private string[] 技能类型array = new string[]
    {
        "普通攻击",
        "主动技能",
        "被动技能"
    };

    public float CD时间;
    public float 施法距离;
    public int 技能指示器形状SelectIndex;
    private string[] 技能指示器形状array = new string[]
    {
        "圆形",
        "矩形",
        "扇形"
    };
    public float 范围参数1;
    public float 范围参数2;
    public int 资源消耗类型SelectIndex;
    private string[] 资源消耗类型array = new string[]
    {
        "法力"
    };
    public int 资源消耗数量;
    public int 升级所需等级;
    public bool 面向施法方向;
    public bool 释放时可转向;
    public bool 释放时可移动;

    bool m_Foldout1;
    GUIContent m_Content1 = new GUIContent("基础属性");

    bool m_Foldout2;
    GUIContent m_Content2 = new GUIContent("高级属性");

    string path = "Assets/Resources/";
    string configName = "SkillConfig";
    string ext = ".asset";
    public SkillConfigSto 配置文件;
    public SkillConfigSto last配置文件;


    public GameObject 模型;
    Animator ani;
    private int 动画选择SelectIndex = 0;
    float 当前帧;
    private Vector2 scrollView = new Vector2(0, 0);
    int frameSelectIndex;
    //float frameTimeFloat;

    ///在Unity编辑器上方的工具栏中创建一个菜单项，点击后打开技能编辑器窗口
    [MenuItem("工具栏/技能编辑器")]
    static void Open()
    {
        技能编辑器Window window = (技能编辑器Window)EditorWindow.GetWindow(typeof(技能编辑器Window));

        window.Show();
    }

    private void OnGUI()
    {

        if (GUILayout.Button("新建配置", GUILayout.Width(200)))
        {
            ScriptableObject scriptTable = ScriptableObject.CreateInstance<SkillConfigSto>();
            int index = 0;
            string url = "";
            while (true)
            {
                url = path + configName + index + ext;
                if (!File.Exists(url))
                {
                    break;
                }
                ++index;
            }

            AssetDatabase.CreateAsset(scriptTable, url);
            配置文件 = Resources.Load<SkillConfigSto>(configName + index);
            LoadConfig();
        }

        配置文件 = EditorGUILayout.ObjectField("配置文件:", 配置文件, typeof(ScriptableObject), true) as SkillConfigSto;
        if (配置文件 != last配置文件)
        {
            last配置文件 = 配置文件;
            LoadConfig();
        }

        if (GUILayout.Button("保存配置", GUILayout.Width(200)))
        {
            saveConfig();
        }

            EditorGUILayout.BeginVertical(GUI.skin.box);
        m_Foldout1 = EditorGUILayout.Foldout(m_Foldout1, m_Content1);
        if (m_Foldout1)
        {
            技能Icon = EditorGUILayout.ObjectField("添加贴图:", 技能Icon, typeof(Texture), true) as Texture;
            技能ID = EditorGUILayout.IntField("技能ID:", 技能ID);
            技能类型SelectIndex = EditorGUILayout.Popup("技能类型", 技能类型SelectIndex, 技能类型array);
            显示名称 = EditorGUILayout.TextField("显示名称:", 显示名称);
            技能目标类型SelectIndex = EditorGUILayout.Popup("技能目标类型", 技能目标类型SelectIndex, 技能目标类型array);
            EditorGUILayout.LabelField("技能描述:");
            技能描述 = EditorGUILayout.TextArea(技能描述, GUILayout.Height(35));
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical(GUI.skin.box);
        m_Foldout2 = EditorGUILayout.Foldout(m_Foldout2, m_Content2);
        if (m_Foldout2)
        {
            目标检测类型SelectIndex = EditorGUILayout.Popup("目标检测类型", 目标检测类型SelectIndex, 目标检测类型array);
            CD时间 = EditorGUILayout.FloatField("CD时间:", CD时间);
            施法距离 = EditorGUILayout.FloatField("施法距离:", 施法距离);
            技能指示器形状SelectIndex = EditorGUILayout.Popup("技能指示器形状", 技能指示器形状SelectIndex, 技能指示器形状array);
            范围参数1 = EditorGUILayout.FloatField("范围参数1:", 范围参数1);
            范围参数2 = EditorGUILayout.FloatField("范围参数2:", 范围参数2);
            资源消耗类型SelectIndex = EditorGUILayout.Popup("资源消耗类型", 资源消耗类型SelectIndex, 资源消耗类型array);
            升级所需等级 = EditorGUILayout.IntField("升级所需等级:", 升级所需等级);
            资源消耗数量 = EditorGUILayout.IntField("资源消耗数量:", 资源消耗数量);
            面向施法方向 = EditorGUILayout.Toggle("面向施法方向", 面向施法方向);
            释放时可转向 = EditorGUILayout.Toggle("释放时可转向", 释放时可转向);
            释放时可移动 = EditorGUILayout.Toggle("释放时可移动", 释放时可移动);
        }
        EditorGUILayout.EndVertical();

        模型 = EditorGUILayout.ObjectField("添加角色:", 模型, typeof(GameObject), true) as GameObject;
        if (GUILayout.Button("应用角色", GUILayout.Width(200)))
        {
            if (模型 == null)
            {
                Debug.LogError("请先添加角色模型");
                return;
            }
            ani = 模型.GetComponent<Animator>();
            if (ani == null)
            {
                Debug.LogError("该模型没有绑定Animator组件");
                return;
            }
        }
        if (ani != null)
        {
            //取到动画片段
            var clips = ani.runtimeAnimatorController.animationClips;
            //为动画选择下拉框的长度固定
            动画选择SelectIndex = Mathf.Clamp(动画选择SelectIndex, 0, clips.Length);
            string[] clipNamesArray = clips.Select(t => t.name).ToArray();
            动画选择SelectIndex = EditorGUILayout.Popup("动画片段", 动画选择SelectIndex, clipNamesArray);

            //根据选择的动画片段，取到对应的AnimationClip
            AnimationClip clip = clips[动画选择SelectIndex];
            //clip.SampleAnimation(ani.gameObject, frameTimeFloat);
            //frameTimeFloat = EditorGUILayout.Slider(frameTimeFloat, 0, clip.length);
            clip.SampleAnimation(ani.gameObject, 当前帧);
            
            frameSelectIndex = EditorGUILayout.IntSlider(frameSelectIndex, 0, (int)(clip.length / (1 / clip.frameRate) - 1));
            EditorGUILayout.LabelField("动画时长:" + clip.length);

            DrawFrames(clip);
        }

    }

    void DrawFrames(AnimationClip clip)
    {
        int frameCount = (int)(clip.length / (1 / clip.frameRate));
        float 帧绘制width = 40;
        float 帧信息区域width = 600;
        float 帧信息区域height = 60;
        scrollView = EditorGUILayout.BeginScrollView(scrollView, true, true, 
            GUILayout.Width(帧信息区域width), GUILayout.Height(帧信息区域height));
        EditorGUILayout.BeginHorizontal();
        for (int i = 0; i < frameCount; i++)
        {
            bool selected = i == frameSelectIndex;
            string title = "" + i;
            if (GUILayout.Button(title, selected ? GUIStyles.item_select : GUIStyles.item_normal, 
                GUILayout.Width(帧绘制width)))
            {
                frameSelectIndex = selected ? -1 : i;
            }
            当前帧 = frameSelectIndex * (1 / clip.frameRate);
        }

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndScrollView();
    }

    private void LoadConfig()
    {
        if (配置文件 == null)
            return;

        技能ID = 配置文件.技能ID;
        显示名称 = 配置文件.显示名称;
        技能描述 = 配置文件.技能描述;
        目标检测类型SelectIndex = 配置文件.目标检测类型;
        技能目标类型SelectIndex = 配置文件.技能目标类型;
        技能Icon = Resources.Load("SkillIcon/" + 配置文件.Icon) as Texture;
        技能类型SelectIndex = 配置文件.技能类型;
        CD时间 = 配置文件.CD时间;
        施法距离 = 配置文件.施法距离;
        技能指示器形状SelectIndex = 配置文件.技能指示器形状;
        范围参数1 = 配置文件.范围参数1;
        范围参数2 = 配置文件.范围参数2;
        资源消耗类型SelectIndex = 配置文件.资源消耗类型;
        资源消耗数量 = 配置文件.资源消耗数量;
        升级所需等级 = 配置文件.升级所需等级;
        面向施法方向 = 配置文件.面向施法方向;
        释放时可转向 = 配置文件.释放时可转向;
        释放时可移动 = 配置文件.释放时可移动;
    }

    private void saveConfig()
    {
        if (配置文件 == null)
            return;

        配置文件.技能ID = 技能ID;
        配置文件.显示名称 = 显示名称;
        配置文件.技能描述 = 技能描述;
        配置文件.目标检测类型 = 目标检测类型SelectIndex;
        配置文件.技能目标类型 = 技能目标类型SelectIndex;
        if (技能Icon != null)
        {
            配置文件.Icon = 技能Icon.name;
        }
        配置文件.技能类型 = 技能类型SelectIndex;
        配置文件.CD时间 = CD时间;
        配置文件.施法距离 = 施法距离;
        配置文件.技能指示器形状 = 技能指示器形状SelectIndex;
        配置文件.范围参数1 = 范围参数1;
        配置文件.范围参数2 = 范围参数2;
        配置文件.资源消耗类型 = 资源消耗类型SelectIndex;
        配置文件.资源消耗数量 = 资源消耗数量;
        配置文件.升级所需等级 = 升级所需等级;
        配置文件.面向施法方向 = 面向施法方向;
        配置文件.释放时可转向 = 释放时可转向;
        配置文件.释放时可移动 = 释放时可移动;
    }

}


public static class GUIStyles 
{
    public static GUIStyle item_select = "MeTransitionSelectHead";
    public static GUIStyle item_normal = "MeTransitionSelect";
    public static GUIStyle box = "HelpBox";

}

