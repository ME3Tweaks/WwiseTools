﻿using AK.Wwise.Waapi;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WwiseTools.Properties;
using WwiseTools.Reference;
using WwiseTools.Utils;

namespace WwiseTools.Objects
{
    public class WwiseSwitchContainer : WwiseContainer
    {
        /// <summary>
        /// 创建一个转变容器
        /// </summary>
        /// <param name="name"></param>
        /// <param name="parent_path"></param>
        [Obsolete("use WwiseUtility.CreateObjectAsync instead")]
        public WwiseSwitchContainer(string name, string parent_path = @"\Actor-Mixer Hierarchy\Default Work Unit") : base(name, "", WwiseObject.ObjectType.SwitchContainer.ToString())
        {
            var tempObj = WwiseUtility.CreateObject(name, ObjectType.SwitchContainer, parent_path);
            ID = tempObj.ID;
            Name = tempObj.Name;
        }

        public WwiseSwitchContainer(WwiseObject @object) : base("", "", "")
        {
            if (@object == null) return;
            ID = @object.ID;
            Name = @object.Name;
            Type = @object.Type;
        }


        /// <summary>
        /// 设置播放模式
        /// </summary>
        /// <param name="behavior"></param>
        [Obsolete("use async version instead")]
        public void SetPlayMode(WwiseProperty.Option_SwitchBehavior behavior)
        {
            WwiseUtility.SetObjectProperty(this, WwiseProperty.Prop_SwitchBehavior(behavior));
        }

        public async Task SetPlayModeAsync(WwiseProperty.Option_SwitchBehavior behavior)
        {
            await WwiseUtility.SetObjectPropertyAsync(this, WwiseProperty.Prop_SwitchBehavior(behavior));
        }

        /// <summary>
        /// 设置Switch Group 或者 State Group 引用
        /// </summary>
        /// <param name="group"></param>
        [Obsolete("use async version instead")]
        public void SetSwitchGroupOrStateGroup(WwiseReference group)
        {
            WwiseUtility.SetObjectReference(this, group);
        }

        public async Task SetSwitchGroupOrStateGroupAsync(WwiseReference group)
        {
            await WwiseUtility.SetObjectReferenceAsync(this, group);
        }

        /// <summary>
        /// 设置默认的Switch或者State
        /// </summary>
        /// <param name="switch_or_state"></param>
        
        [Obsolete("use async version instead")]
        public void SetDefaultSwitchOrState(WwiseReference switch_or_state)
        {
            WwiseUtility.SetObjectReference(this, switch_or_state);
        }

        public async Task SetDefaultSwitchOrStateAsync(WwiseReference switch_or_state)
        {
            await WwiseUtility.SetObjectReferenceAsync(this, switch_or_state);
        }

        /// <summary>
        /// 分配子对象至State或者Switch
        /// </summary>
        /// <param name="child"></param>
        /// <param name="state_or_switch"></param>
        [Obsolete("use async version instead")]
        public void AssignChildToStateOrSwitch(WwiseObject child, WwiseObject state_or_switch)
        {
            var temp = AssignChildToStateOrSwitchAsync(child, state_or_switch);
            temp.Wait();
        }

        /// <summary>
        /// 分配子对象至State或者Switch，后台运行
        /// </summary>
        /// <param name="child"></param>
        /// <param name="state_or_switch"></param>
        /// <returns></returns>
        public async Task AssignChildToStateOrSwitchAsync(WwiseObject child, WwiseObject state_or_switch)
        {
            if (!await WwiseUtility.TryConnectWaapiAsync()) return;


            if (child == null || state_or_switch == null) return;



            foreach (var assignment in (await GetAssignmentsAsync())["return"])
            {
                if (assignment["stateOrSwitch"].ToString() == state_or_switch.ID && assignment["child"].ToString() == child.ID)
                {
                    WaapiLog.Log($"Child {child.Name} has already been assigned to {state_or_switch.Type} : {state_or_switch.Name}!");
                    return;
                }
            }

            try
            {
                var func = WwiseUtility.Function.Verify("ak.wwise.core.switchContainer.addAssignment");

                // 创建物体
                var result = await WwiseUtility.Client.Call
                    (
                    func,
                    new JObject
                    {
                        new JProperty("child", child.ID),
                        new JProperty("stateOrSwitch", state_or_switch.ID),
                    },
                    null
                    );
            }
            catch (Wamp.ErrorException e)
            {
                WaapiLog.Log($"Failed to assign {child.Name} to {state_or_switch}! ======> {e.Message}");
            }
        }

        /// <summary>
        /// 从State或者Switch删除分配的子对象
        /// </summary>
        /// <param name="child"></param>
        /// <param name="state_or_switch"></param>
        [Obsolete("Use async version instead")]
        public void RemoveAssignedChildFromStateOrSwitch(WwiseObject child, WwiseObject state_or_switch)
        {
            var temp = RemoveAssignedChildFromStateOrSwitchAsync(child, state_or_switch);
            temp.Wait();
        }

        /// <summary>
        /// 从State或者Switch删除分配的子对象，后台运行
        /// </summary>
        /// <param name="child"></param>
        /// <param name="state_or_switch"></param>
        public async Task RemoveAssignedChildFromStateOrSwitchAsync(WwiseObject child, WwiseObject state_or_switch)
        {
            if (!await WwiseUtility.TryConnectWaapiAsync()) return;


            if (child == null || state_or_switch == null) return;

            try
            {
                var func = WwiseUtility.Function.Verify("ak.wwise.core.switchContainer.removeAssignment");

                // 创建物体
                var result = await WwiseUtility.Client.Call
                    (
                    func,
                    new JObject
                    {
                        new JProperty("child", child.ID),
                        new JProperty("stateOrSwitch", state_or_switch.ID),
                    },
                    null
                    );
            }
            catch (Wamp.ErrorException e)
            {
                WaapiLog.Log($"Failed to assign {child.Name} to {state_or_switch}! ======> {e.Message}");
            }
        }

        /// <summary>
        /// 获取分配信息
        /// </summary>
        /// <returns></returns>
        [Obsolete("Use async version instead")]
        public JObject GetAssignments()
        {
            var temp = GetAssignmentsAsync();
            temp.Wait();
            return temp.Result;
        }

        /// <summary>
        /// 获取分配信息，后台运行
        /// </summary>
        /// <returns></returns>
        public async Task<JObject> GetAssignmentsAsync()
        {
            if (!await WwiseUtility.TryConnectWaapiAsync()) return null;


            try
            {
                var func = WwiseUtility.Function.Verify("ak.wwise.core.switchContainer.getAssignments");

                // 获取信息
                var result = await WwiseUtility.Client.Call
                    (
                    func,
                    new JObject
                    {
                        new JProperty("id", ID),
                    },
                    null
                    );
                return result;
            }
            catch (Wamp.ErrorException e)
            {
                WaapiLog.Log($"Failed to get assignment of {Name}! ======> {e.Message}");
                return null;
            }
        }
    }
}
