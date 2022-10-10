﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WwiseTools.Objects;

namespace WwiseTools.Utils;

public class WwisePathBuilder
{
    private WwiseObject _root;

    private List<WwiseObject> _hierarchy;
    
    
    /// <summary>
    /// 规范Wwise路径格式
    /// </summary>
    /// <param name="root">root为现有的Wwise对象路径，无需指定类别</param>
    public WwisePathBuilder(WwiseObject root)
    {
        _root = root;
        _hierarchy = new List<WwiseObject>();
    }

    public bool AppendHierarchy(WwiseObject.ObjectType type, string name)
    {
        WwiseObject last = null;

        if (_hierarchy.Count == 0) last = _root;
        else
        {
            last = _hierarchy.Last();
        }

        if (type == WwiseObject.ObjectType.WorkUnit || type == WwiseObject.ObjectType.ActorMixer || type == WwiseObject.ObjectType.Folder)
        {
            if (last.Type == WwiseObject.ObjectType.RandomSequenceContainer.ToString() ||
                last.Type == WwiseObject.ObjectType.SwitchContainer.ToString() ||
                last.Type == WwiseObject.ObjectType.BlendContainer.ToString())
            {
                WaapiLog.InternalLog($"Failed to append {name}:{type} to hierarchy!");
                return false;
            }
        }
        
        _hierarchy.Add(new WwiseObject(name, "", type.ToString()));
        return true;
    }

    public void ClearHierarchy()
    {
        _hierarchy.Clear();
    }

    public async Task<string> GetPurePathAsync()
    {
        var rootPath = await _root.GetPathAsync();
        
#if DEBUG
        if (rootPath == null && !WwiseUtility.Instance.IsConnected()) rootPath = "\\Actor-Mixer Hierarchy";
#endif
        
        string hierarchy = "";

        foreach (var wwiseObject in _hierarchy)
        {
            if (hierarchy != "") hierarchy += "\\";

            hierarchy += wwiseObject.Name;
        }
        
        return rootPath + "\\" + hierarchy;
    }

    public async Task<string> GetImportPathAsync()
    {
        var rootPath = await _root.GetPathAsync();
        
#if DEBUG
        if (rootPath == null && !WwiseUtility.Instance.IsConnected()) rootPath = "\\Actor-Mixer Hierarchy";
#endif
        
        string hierarchy = "";

        foreach (var wwiseObject in _hierarchy)
        {
            if (hierarchy != "") hierarchy += "\\";

            hierarchy += $"<{wwiseObject.Type}>{wwiseObject.Name}";
        }
        
        return rootPath + "\\" + hierarchy;
    }
}