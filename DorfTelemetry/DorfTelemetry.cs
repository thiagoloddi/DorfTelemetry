using System;
using System.Collections.Generic;
using BepInEx;
using UnityEngine;
using System.IO;
using System.Text;

namespace DorfTelemetry
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class DorfTelemetry : BaseUnityPlugin
    {
        private const string modGUID = "TL.DorfTelemetry";
        private const string modName = "DorfTelemetry";
        private const string modVersion = "1.0.0";

        private const bool debug = false;

        internal static DorfTelemetry Instance;

        private void Awake()
        {
            Instance = this;

            var go = new GameObject("DorfTelemetryRunner");
            DontDestroyOnLoad(go);
            go.hideFlags = HideFlags.HideAndDontSave;

            go.AddComponent<Runner>();
        }

        void Log(string msg)
        {
            Logger.LogInfo(msg);
        }

        void Debug(string msg)
        {
            if (debug)
            {
                Logger.LogInfo(msg);
            }
        }

        private void WriteCSV(List<CSVData> data)
        {
            Log($"[Export] Writing CSV");
            // Folder inside plugins
            string exportDir = Path.Combine(Paths.PluginPath, "DorfTelemetryExports");
            Directory.CreateDirectory(exportDir);

            string fileName = $"tile_counts_{System.DateTime.Now:yyyyMMdd_HHmmss}.csv";
            string path = Path.Combine(exportDir, fileName);

            var sb = new StringBuilder();
            sb.AppendLine("tile,count"); // header

            foreach (var line in data)
            {
                sb.Append('"').Append(line.code).Append('"').Append(',');
                sb.Append(line.count);
                sb.AppendLine();
            }


            File.WriteAllText(path, sb.ToString(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            Log($"[Export] Wrote CSV: {path}");
        }

        class CSVData
        {
            public string code;
            public int count;

            public CSVData(string code, int count)
            {
                this.code = code;
                this.count = count;
            }
        }

        private class Runner : MonoBehaviour
        {

            private bool running = false;
            private readonly static Dictionary<string, string> elementCodeMap = new Dictionary<string, string>()
            {
                {  "Agriculture", "A" },
                {  "Forest", "F" },
                {  "Village", "V" },
                {  "River", "R" },
                {  "Lake", "L" },
                {  "Train", "T" }
            };


            private void Start()
            {
                Instance.Debug("Initializing Runner");
            }

            private void Update()
            {

                if (Input.GetKeyDown(KeyCode.F8) && !running)
                {
                    running = true;

                    try
                    {
                        World world = FindObjectsOfType<World>()[0];

                        if (world != null)
                        {
                            List<CSVData> data = GenerateData(world);
                            Instance.WriteCSV(data);

                        }
                    }
                    catch (Exception ex)
                    {
                        Instance.Log(ex.Message);
                    }

                    running = false;
                }
            }

            static List<CSVData> GenerateData(World world)
            {
                Dictionary<string, int> count = new Dictionary<string, int>();
                List<CSVData> data = new List<CSVData>();
                List<Tile> tiles = world.GetAllPlacedTiles();
                Instance.Debug($"Generating Data -- {tiles.Count} tiles found");

                foreach (Tile tile in tiles)
                {
                    string code = GetTileCode(tile);

                    if (!count.ContainsKey(code))
                    {
                        count.Add(code, 0);
                    }

                    count[code] += 1;
                }

                foreach (var kv in count)
                {
                    data.Add(new CSVData(kv.Key, kv.Value));
                }

                data.Sort((a, b) => b.count.CompareTo(a.count));

                return data;
            }

            static string GetTileCode(Tile tile)
            {
                string code = "GGGGGG";
                List<ElementGroupSegment> segments = tile.AllElementGroupSegments;

                if (IsTrainStation(tile))
                {
                    code = "SSSSSS";
                }
                else
                {
                    foreach (ElementGroupSegment s in segments)
                    {
                        string elType = s.GroupType.name;

                        if (s.GroupType.name == "Water")
                        {
                            if (s.HybridSegment != null)
                            {
                                elType = "Lake";
                            }
                            else
                            {
                                elType = "River";
                            }
                        }

                        foreach (int i in s.Edges)
                        {
                            code = code.Remove(i, 1).Insert(i, elementCodeMap[elType]);
                        }

                    }
                }

                return CanonicalRotate(code);
            }

            static bool IsTrainStation(Tile tile)
            {
                bool hasFullWaterEdges = false;
                bool hasFullTraintrackEdges = false;

                foreach (ElementGroupSegment s in tile.AllElementGroupSegments)
                {
                    if (s.GroupType.name == "Water" && s.GetEdges(Space.Self).Count == 6)
                    {
                        hasFullWaterEdges = true;
                    }

                    if (s.GroupType.name == "Train" && s.GetEdges(Space.Self).Count == 6)
                    {
                        hasFullTraintrackEdges = true;
                    }
                }

                return hasFullWaterEdges && hasFullTraintrackEdges;
            }


            static string CanonicalRotate(string s)
            {
                // s length should be 6
                string best = null;
                for (int r = 0; r < s.Length; r++)
                {
                    string rot = s.Substring(r) + s.Substring(0, r);
                    if (best == null || string.CompareOrdinal(rot, best) < 0)
                        best = rot;
                }
                return best;
            }
        }
    }
}