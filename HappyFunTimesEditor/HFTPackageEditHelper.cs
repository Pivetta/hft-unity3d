﻿/*
 * Copyright 2014, Gregg Tavares.
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are
 * met:
 *
 *     * Redistributions of source code must retain the above copyright
 * notice, this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above
 * copyright notice, this list of conditions and the following disclaimer
 * in the documentation and/or other materials provided with the
 * distribution.
 *     * Neither the name of Gregg Tavares. nor the names of its
 * contributors may be used to endorse or promote products derived from
 * this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
 * "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
 * LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
 * A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
 * OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
 * LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
 * DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
 * OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;
using HappyFunTimes;

// I fucking sware I don't understand Unity's UI system.
// Everything related to MonoDevelop makes sense but nothing related to global data makes
// any sense.

namespace HappyFunTimesEditor
{

    [Serializable]
    public class HFTPackageEditorHelper : ScriptableObject
    {
        [SerializeField]
        public HFTPackage m_package;

        [SerializeField]
        private bool m_showPackageJson = true;

        private bool m_dirty = false;

        private static HFTPackageEditorHelper s_instance = null;
        private static Regex validSemverRE = new Regex(@"^\d+\.\d+\.\d+$");
        private static Regex validGameIdRE = new Regex(@"^[a-z0-9A-Z_.-]+$");
        private static Regex gameIdFilterRE = new Regex(@"[^a-z0-9A-Z_.-]");
        private static Regex semverFilterRE = new Regex(@"[^0-9.]");

        public static HFTPackageEditorHelper Instance
        {
            get
            {
                if (s_instance == null)
                {
                    var tmp = Resources.FindObjectsOfTypeAll<HFTPackageEditorHelper>();
                    if (tmp.Length > 0)
                    {
                        s_instance = tmp[0];
                    }
                    else
                    {
                        s_instance = ScriptableObject.CreateInstance<HFTPackageEditorHelper>();
                        s_instance.Init();
                    }
                    s_instance.hideFlags = HideFlags.DontSave;
                }
                return s_instance;
            }
        }

        public void OnEnable() {
            if (m_package == null) {
                m_package = new HFTPackage();
            }
        }

        public void Init()
        {
            m_package.Init();
        }

        public void Persist()
        {
            m_package.Persist();
        }

        public string GetString(string id)
        {
            return m_package.GetString(id);
        }

        private void EditString(/*Dictionary<string, object>dict,*/ string id, Regex validRE = null, int maxLength = 0, Regex filterRE = null)
        {
            string s = m_package.GetString(id);
            string n = EditorGUILayout.TextField(id, s);
            if (filterRE != null)
            {
                n = filterRE.Replace(n, "");
            }
            if (GUI.changed && n != s)
            {
                bool valid = (validRE == null || validRE.IsMatch(n));
                if (valid)
                {
                    if (maxLength > 0 && n.Length > maxLength)
                    {
                        n = n.Substring(0, maxLength);
                    }
                    Undo.RecordObject(this, "set HappyFunTimes " + id);
                    m_package.SetString(id, n);
                    m_dirty = true;
                }
            }
        }

        private void EditInt(/*Dictionary<string, object>dict,*/ string id, int min, int max)
        {
            int i = m_package.GetInt(id);
            int n = EditorGUILayout.IntField(id, i);
            n = Math.Max(Math.Min(max, n), min);
            if (GUI.changed && n != i) {
                Undo.RecordObject(this, "set HappyFunTimes " + id);
                m_package.SetInt(id, n);
                m_dirty = true;
            }
        }

        private void EditText(int lines, string id)
        {
            string s = m_package.GetString(id);
            GUILayout.Label (id, EditorStyles.boldLabel);
            string n = EditorGUILayout.TextArea(s, GUILayout.Height(lines * 10));
            if (GUI.changed && n != s)
            {
                Undo.RecordObject(this, "set HappyFunTimes " + id);
                m_package.SetString(id, n);
                m_dirty = true;
            }
        }


        public void DoGUI() {
            m_showPackageJson = EditorGUILayout.Foldout(m_showPackageJson, "Package.json Settings");
            if (m_package != null && m_showPackageJson) {
                EditString("name");
                EditString("version", validSemverRE, 14, semverFilterRE);
                EditString("gameId", validGameIdRE, 60, gameIdFilterRE);
                EditString("category");
                EditString("apiVersion", validSemverRE, 14, semverFilterRE);
                EditString("gameType");
                EditInt("minPlayers", 1, 1000);
                EditText(10, "description");
            }

            if (m_dirty) {
                EditorUtility.SetDirty(this);
                m_dirty = false;
            }
        }

    }

}  // namespace HappyFunTimesEditor

