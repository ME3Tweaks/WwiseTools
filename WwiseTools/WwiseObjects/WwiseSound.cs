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
    public class WwiseSound : WwiseActorMixer
    {
        /// <summary>
        /// 创建一个Wwise Sound对象
        /// </summary>
        /// <param name="name"></param>
        /// <param name="parent_path"></param>
        public WwiseSound(string name, string parent_path =@"\Actor-Mixer Hierarchy\Default Work Unit") : base(name, "", "Sound")
        {
            var tempObj = WwiseUtility.CreateObject(name, ObjectType.Sound, parent_path);
            ID = tempObj.ID;
            Name = tempObj.Name;
        }

        /// <summary>
        /// 创建一个Wwise Sound对象，设置包含的音频文件信息
        /// </summary>
        /// <param name="name"></param>
        /// <param name="file_path"></param>
        /// <param name="language"></param>
        /// <param name="sub_folder"></param>
        /// <param name="parent_path"></param>
        public WwiseSound(string name, string file_path, string language = "SFX", string sub_folder = "", string parent_path = @"\Actor-Mixer Hierachy\Default Work Unit") : base(name, "", "Sound")
        {
            var tempObj = WwiseUtility.ImportSound(file_path, language, sub_folder, parent_path);
            WwiseUtility.ChangeObjectName(tempObj, name);
            ID = tempObj.ID;
            Name = tempObj.Name;
        }

        public WwiseSound(WwiseObject @object) : base("", "", "")
        {
            if (@object == null) return;
            ID = @object.ID;
            Name = @object.Name;
            Type = @object.Type;
        }

        /// <summary>
        /// 设置播放延迟
        /// </summary>
        /// <param name="delay"></param>
        public void SetInitialDelay(float delay)
        {
            WwiseUtility.SetObjectProperty(this, WwiseProperty.Prop_InitialDelay(delay));
        }

        /// <summary>
        /// 设置循环
        /// </summary>
        /// <param name="loop"></param>
        /// <param name="infinite"></param>
        /// <param name="numOfLoop"></param>
        public void SetLoop(bool loop, bool infinite = true , uint numOfLoop = 2)
        {
            WwiseUtility.SetObjectProperty(this, WwiseProperty.Prop_IsLoopingEnabled(loop));
            WwiseUtility.SetObjectProperty(this, WwiseProperty.Prop_IsLoopingInfinite(infinite));
            WwiseUtility.SetObjectProperty(this, WwiseProperty.Prop_LoopCount(numOfLoop));
        }
        
        /// <summary>
        /// 设置流
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="non_cachable"></param>
        /// <param name="zero_latency"></param>
        public void SetStream(bool stream, bool non_cachable, bool zero_latency)
        {
            WwiseUtility.SetObjectProperty(this, WwiseProperty.Prop_IsStreamingEnabled(stream));
            WwiseUtility.SetObjectProperty(this, WwiseProperty.Prop_IsNonCachable(non_cachable));
            WwiseUtility.SetObjectProperty(this, WwiseProperty.Prop_IsZeroLantency(zero_latency));
        }

        public string[] GetWavSourceFilePath()
        {
            var r = GetWavFilePathAsync();
            r.Wait();
            List<string> paths = new List<string>();
            foreach (var result in r.Result["return"].Last)
            {
                paths.Add(result.Last.ToString());
            }
            return paths.ToArray();
        }

        public  async Task<JObject> GetWavFilePathAsync()
        {
            if (!WwiseUtility.TryConnectWaapi()) return null;

            try
            {
                // ak.wwise.core.@object.get 指令
                var query = new
                {
                    from = new
                    {
                        id = new string[] { ID }
                    }
                };

                // ak.wwise.core.@object.get 返回参数设置
                var options = new
                {

                    @return = new string[] { "sound:originalWavFilePath" }

                };

                try // 尝试返回物体数据
                {
                    JObject jresult = await WwiseUtility.Client.Call(ak.wwise.core.@object.get, query, options);

                    return jresult;
                }
                catch
                {
                    Console.WriteLine($"Failed to return WaveFilePath from ID : {ID}!");
                    return null;
                }
            }
            catch (Wamp.ErrorException e)
            {
                Console.WriteLine($"Failed to return WaveFilePath from ID : {ID}! ======> {e.Message}");
                return null;
            }

        }

    }
}
