﻿
using System;
using System.Collections.Generic;
using System.Reflection;

namespace WwiseTools.Models.Profiler
{
    public class ProfilerCaptureLogOption : IWwiseOption
    {
        public bool Notification { get; set; }
        public bool MusicTransition { get; set; }
        public bool Midi { get; set; }
        public bool ExternalSource { get; set; }
        public bool Marker { get; set; }
        public bool State { get; set; }
        public bool Switch { get; set; }
        public bool SetParameter { get; set; }
        public bool ParameterChanged { get; set; }
        public bool Bank { get; set; }
        public bool Prepare { get; set; }
        public bool Event { get; set; }
        public bool DialogueEventResolved { get; set; }
        public bool ActionTriggered { get; set; }
        public bool ActionDelayed { get; set; }
        public bool Message { get; set; }
        public bool APICall { get; set; }
        public bool GameObjectRegistration { get; set; }

        public string[] GetOptions()
        {
            var properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            var result = new List<string>();

            foreach (var property in properties)
            {
                if (property.GetValue(this).ToString() != "True") continue;
                result.Add(property.Name);
            }

            return result.ToArray();
        }
    }
}
