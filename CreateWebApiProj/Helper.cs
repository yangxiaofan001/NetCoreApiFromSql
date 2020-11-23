using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CreateWebApiProj.ConfigSections;

namespace CreateWebApiProj
{
    public class Helper
    {
        public void SaveAsVersionX(string projRoot, string versionsRoot, int version, string apiProjectName)
        {
            string versionString = string.Format("{0,22:D3}", version).Trim();
            string versionFolder = versionsRoot + "/v" + versionString;

            xcopy(projRoot, versionFolder, originalName:true, apiProjectName);
        }

        public void AddPackages(string projRoot, string templateVersion, PackageVersion[] packageVersions, string apiProjectName)
        {
            List<string> linesToAdd = new List<string>();
            foreach(PackageVersion package in packageVersions.Where(p => p.templateVersion == templateVersion))
            {
                string version = package.Version;
                if (string.IsNullOrEmpty(version))
                {
                    version = package.DefaultVersion;
                }
                linesToAdd.Add("    <PackageReference Include=\"" + package.Name + "\" Version=\"" + version + "\"/>");
            }

            AddContentToFile(filePath: projRoot + "/" + apiProjectName + ".csproj", placeHolder: "<!--AddMorePackages-->", linesToAdd);
        }

        public void AddContentToFile(string filePath, string placeHolder, List<string> linesToAdd)
        {
            string tempFilePath = filePath + ".tmp";
            StreamReader sr = new StreamReader(filePath);
            StreamWriter sw = new StreamWriter(tempFilePath, false);
            sw.Close();

            string line;
            while ((line = sr.ReadLine()) != null)
            {
                if (line.Trim() != placeHolder)
                {
                    sw = new StreamWriter(tempFilePath, true);
                    sw.WriteLine(line);
                    sw.Close();
                }
                else
                {
                    sw = new StreamWriter(tempFilePath, true);
                    sw.WriteLine(line);
                    sw.Close();

                    foreach(string newLine in linesToAdd)
                    {
                        sw = new StreamWriter(tempFilePath, true);
                        sw.WriteLine(newLine);
                        sw.Close();
                    }
                }
            }
            sr.Close();
            System.IO.File.Delete(filePath);
            System.IO.File.Move(tempFilePath, filePath);
        }

        public string indentSpaces(int indent)
        {
            string result = "";
            int indentSpace = 4;
            for(int i = 0; i < indent * indentSpace; i ++)
            {
                result = result + " ";
            }

            return result;
        }

        public string Pluralize(string name)
        {
            if (string.IsNullOrEmpty(name))
                return name;
            name = name.Trim();
            if (string.IsNullOrEmpty(name))
                return name;

            string result = name[0].ToString().ToUpper() + name.Substring(1, name.Length - 1);
            if (result.EndsWith("y"))
            {
                result = result.Substring(0, result.Length - 1) + "ies";
            }
            else if (!result.EndsWith("s"))
            {
                result = result + "s";
            }

            return result;
        }

        public void xcopy(string sourceFolder, string targetFolder, bool originalName, string apiProjectName)
        {
            System.IO.Directory.CreateDirectory(targetFolder);

            foreach(string file in System.IO.Directory.GetFiles(sourceFolder))
            {
                string fileName = System.IO.Path.GetFileName(file);
                string targetFile = targetFolder + "/" + fileName;

                if (fileName.StartsWith("test.csproj"))
                {
                    targetFile = targetFile.Replace("test.csproj", apiProjectName + ".csproj");
                }
                if (!originalName && (file.EndsWith(".cs_") || file.EndsWith(".json_") || file.EndsWith(".csproj_")))
                {
                    targetFile = targetFile.Substring(0, targetFile.Length - 1);
                }

                System.IO.File.Copy(file, targetFile);
            }

            foreach(string subFolder in System.IO.Directory.GetDirectories(sourceFolder))
            {
                string subFolderName = System.IO.Path.GetFileName(subFolder);
                string targetSubFolder = targetFolder + "/" + subFolderName;
                xcopy(subFolder, targetSubFolder, originalName, apiProjectName);
            }
        }
       
        public string ToCamelCase(string word)
        {
            string [] words = word.Split(new char[]{'_'}, StringSplitOptions.RemoveEmptyEntries);
            if (words == null || word.Length == 0)
            {
                return word;
            }

            if (words.Length == 1)
            {
                words = word.Split(new char[]{'-'}, StringSplitOptions.RemoveEmptyEntries);
            }

            if (words.Length == 1)
            {
                words[0] = words[0][0].ToString().ToUpper() + words[0].Substring(1, words[0].Length - 1);
            }

            string result = "";
            foreach(string w in words)
            {
                string nw = w.ToLower();
                nw = nw[0].ToString().ToUpper() + w.Substring(1, w.Length - 1);
                result = result + nw;
            }

            return result;
        }       

        public void GenerateClassFromTemplace(string templateFilePath, string targetFilePath, List<TemplateVariable> templateVariables)
        {

            System.IO.StreamWriter sw = new StreamWriter(targetFilePath, false);
            sw.Close();
            sw = new StreamWriter(targetFilePath, true);

            System.IO.StreamReader sr = new StreamReader(templateFilePath);
            string line = "";
            while((line = sr.ReadLine()) != null)
            {
                foreach(TemplateVariable v in templateVariables)
                {
                    line = line.Replace(v.Name, v.Value);
                }
                sw.WriteLine(line);
            }

            sw.Close();
        }    
    }
}