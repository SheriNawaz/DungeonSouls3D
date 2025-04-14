using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CWM.Skinn
{
    public class ShapeFilter
    {
        private static string functions = null;

        public static string GetDefaultFunctions
        {
            get
            {
                if (!string.IsNullOrEmpty(functions)) return functions;

                functions = "";
                functions += "replaceText(YourTextToReplace, YourTextToReplaceWith)";
                functions += Environment.NewLine;
                functions += "containsText(YourTextToExcludeBlendshapes)";
                functions += Environment.NewLine;
                return functions;
            }
        }

        public bool include;
        public Dictionary<string, string> replaceText;
        public List<string> containsText;
        public Dictionary<string, string> renamed;
        public List<string> flagged;

        private enum Section { Header, Functions, FlaggedElements, AllElements, CountOf }

        public ShapeFilter()
        {
            include = false;
            replaceText = new Dictionary<string, string>();
            containsText = new List<string>();
            renamed = new Dictionary<string, string>();
            flagged = new List<string>();
        }

        public static implicit operator bool(ShapeFilter value) { return value != null; }

        private string ReplaceText(string shape)
        {
            foreach (var item in renamed) if (shape == item.Key) return item.Value;
            var shapeName = shape;
            foreach (var item in replaceText) shapeName = shapeName.Replace(item.Key, item.Value);
            return shapeName;
        }

        private bool Included(string shape)
        {
            foreach (var item in containsText) if (shape.Contains(item)) return false;
            foreach (var item in flagged) if (shape == item) return false;
            return true;
        }

        public bool Evaluate(string shape, out string name)
        {
            name = ReplaceText(shape);
            foreach (var item in flagged) if (shape == item) return false;
            if (!Included(shape)) return false;
            return true;
        }

        public static void Write(string path, List<BoneElement> elements = null, string functions = null, string[] blendshapes = null)
        {
            if (string.IsNullOrEmpty(path)) return;
            if (string.IsNullOrEmpty(functions)) functions = "";
            if (SkinnEx.IsNullOrEmpty(blendshapes)) blendshapes = new string[0];

            using (var sw = new StreamWriter(path))
            {
                sw.WriteLine("//Skinn: Text Filter : Blendshapes");
                sw.WriteLine("");
                sw.WriteLine("****Functions****");
                sw.WriteLine("");

                using (var sr = new StringReader(functions))
                {
                    string fucntionLine;
                    while ((fucntionLine = sr.ReadLine()) != null)
                    {
                        sw.WriteLine(fucntionLine);
                    }
                }

                sw.WriteLine("");
                sw.WriteLine("****Flagged Elements****");
                sw.WriteLine("");

                if (!SkinnEx.IsNullOrEmpty(elements))
                {
                    foreach (var shape in blendshapes)
                    {
                        var element = elements.GetBoneTreeElement(Animator.StringToHash(shape), true);
                        if (!element || string.IsNullOrEmpty(element.Name)) continue;

                        if (element.Name != shape && element.enabled)
                        { sw.WriteLine(string.Format("{0} = {1}", shape, element.Name)); continue; }

                        if (element.Name == shape && !element.enabled)
                        { sw.WriteLine(shape); continue; }
                    }

                }

                sw.WriteLine("");
                sw.WriteLine("****All Elements****");
                sw.WriteLine("");

                foreach (var shape in blendshapes)
                {
                    sw.WriteLine(shape);
                }

                sw.WriteLine(""); sw.WriteLine("");
            }
        }

        public static bool Read(out ShapeFilter blendshapeFitler, List<BoneElement> elements)
        {
            blendshapeFitler = new ShapeFilter();
            var loaded = false;
            if (!SkinnEx.IsNullOrEmpty(elements))
            {
                foreach (var element in elements)
                {
                    if (!element || string.IsNullOrEmpty(element.Name)) continue;
                    if (!element.enabled)
                    {
                        if (!blendshapeFitler.flagged.Contains(element.Name)) blendshapeFitler.flagged.Add(element.Name);
                        loaded = true;
                    }
                }
            }
            if (!loaded) blendshapeFitler = null;
            return loaded;
        }

        public static bool Read(out ShapeFilter blendshapeFitler, out string errors, string[] blendshapes,
            string functions = null,
            List<BoneElement> elements = null
            )
        {
            blendshapeFitler = new ShapeFilter();
            errors = "";
            var loaded = false;

            if (!string.IsNullOrEmpty(functions))
            {
                var lineCount = -1;
                using (var sr = new StringReader(functions))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        lineCount++;

                        var trimmedLine = line.Trim();
                        if (trimmedLine.StartsWith("replaceText"))
                        {
                            var function = trimmedLine.Replace("replaceText", "");
                            function = function.Replace("(", "").Replace(")", "").Replace(";", "");
                            var inputs = function.Split(',');
                            if (SkinnEx.IsNullOrEmpty(inputs, 2))
                            {
                                errors += Environment.NewLine;
                                errors += string.Format("Error Line: {0}, Section :{1}, Text: {2}", lineCount, "Functions", line);
                            }
                            else
                            {
                                if (!blendshapeFitler.replaceText.ContainsKey(inputs[0].Trim()))
                                    blendshapeFitler.replaceText.Add(inputs[0].Trim(), inputs[1].Trim());
                                loaded = true;
                            }
                            continue;
                        }

                        if (trimmedLine.StartsWith("containsText"))
                        {
                            var function = trimmedLine.Replace("containsText", "");
                            function = function.Replace("(", "").Replace(")", "").Replace(";", "");
                            if (string.IsNullOrEmpty(function))
                            {
                                errors += Environment.NewLine;
                                errors += string.Format("Error Line: {0}, Section :{1}, Text: {2}", lineCount, "Functions", line);
                            }
                            else
                            {
                                if (!blendshapeFitler.containsText.Contains(function.Trim()))
                                    blendshapeFitler.containsText.Add(function.Trim());
                                loaded = true;
                            }
                            continue;
                        }
                    }
                }
            }

            if (!SkinnEx.IsNullOrEmpty(elements))
            {
                foreach (var shape in blendshapes)
                {
                    var element = elements.GetBoneTreeElement(Animator.StringToHash(shape), true);
                    if (!element || string.IsNullOrEmpty(element.Name)) continue;

                    if (element.Name != shape && element.enabled)
                    {
                        if (!blendshapeFitler.renamed.ContainsKey(shape)) blendshapeFitler.renamed.Add(shape, element.Name);
                        loaded = true;
                        continue;
                    }

                    if (element.Name == shape && !element.enabled)
                    {
                        if (!blendshapeFitler.flagged.Contains(element.Name)) blendshapeFitler.flagged.Add(element.Name);
                        loaded = true;
                        continue;
                    }
                }

            }
            return loaded;
        }

        public static bool Read(out ShapeFilter blendshapeFitler, out string errors, TextAsset textAsset)
        {
            blendshapeFitler = new ShapeFilter();
            errors = "";
            if (!textAsset || string.IsNullOrEmpty(textAsset.text)) return false;

            var section = Section.Header;
            var lineCount = -1;
            var loaded = false;

            using (var sr = new StringReader(textAsset.text))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    lineCount++;
                    var trimmedLine = line.Trim();
                    if (string.IsNullOrEmpty(trimmedLine)) continue;
                    if (trimmedLine.StartsWith("//")) continue;

                    if (trimmedLine.Contains("*Functions*")) { section = Section.Functions; continue; }
                    if (trimmedLine.Contains("*Flagged Elements*")) { section = Section.FlaggedElements; continue; }
                    if (trimmedLine.Contains("*All Elements*")) { section = Section.AllElements; continue; }

                    switch (section)
                    {
                        case Section.Functions:
                            {
                                if (trimmedLine.StartsWith("replaceText"))
                                {
                                    var function = trimmedLine.Replace("replaceText", "");
                                    function = function.Replace("(", "").Replace(")", "").Replace(";", "");
                                    var inputs = function.Split(',');
                                    if (SkinnEx.IsNullOrEmpty(inputs, 2))
                                    {
                                        errors += Environment.NewLine;
                                        errors += string.Format("Error Line: {0}, Section :{1}, Text: {2}", lineCount, section, line);
                                    }
                                    else
                                    {
                                        if (!blendshapeFitler.replaceText.ContainsKey(inputs[0].Trim()))
                                            blendshapeFitler.replaceText.Add(inputs[0].Trim(), inputs[1].Trim());
                                        loaded = true;
                                    }
                                    continue;
                                }

                                if (trimmedLine.StartsWith("containsText"))
                                {
                                    var function = trimmedLine.Replace("containsText", "");
                                    function = function.Replace("(", "").Replace(")", "").Replace(";", "");
                                    if (string.IsNullOrEmpty(function))
                                    {
                                        errors += Environment.NewLine;
                                        errors += string.Format("Error Line: {0}, Section :{1}, Text: {2}", lineCount, section, line);
                                    }
                                    else
                                    {
                                        if (!blendshapeFitler.containsText.Contains(function.Trim()))
                                            blendshapeFitler.containsText.Add(function.Trim());
                                        loaded = true;
                                    }
                                    continue;
                                }
                            }
                            break;
                        case Section.FlaggedElements:
                            {
                                if (trimmedLine.Contains("="))
                                {
                                    var inputs = trimmedLine.Split('=');
                                    if (SkinnEx.IsNullOrEmpty(inputs, 2))
                                    {
                                        errors += Environment.NewLine;
                                        errors += string.Format("Error Line: {0}, Section :{1}, Text: {2}", lineCount, section, line);
                                    }
                                    else
                                    {
                                        if (!blendshapeFitler.renamed.ContainsKey(inputs[0].Trim()))
                                            blendshapeFitler.renamed.Add(inputs[0].Trim(), inputs[1].Trim());
                                        loaded = true;
                                    }
                                    continue;
                                }
                                else
                                {
                                    if (!blendshapeFitler.flagged.Contains(trimmedLine))
                                        blendshapeFitler.flagged.Add(trimmedLine);
                                    loaded = true;
                                }
                                continue;
                            }
                        default: continue;
                        case Section.AllElements: return loaded;
                    }
                }
            }

            return loaded;
        }
    }
}