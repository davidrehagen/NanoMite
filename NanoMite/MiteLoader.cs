using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Newtonsoft.Json.Linq;

namespace NanoMite
{
    public static class MiteLoader
    {
        public static IEnumerable<Mite> LoadMites(string pluginPath)
        {
            var mites = new List<Mite>();

            var directories = Directory.GetDirectories(pluginPath);

            foreach (var directoryPath in directories)
            {
                var directoryInfo = new DirectoryInfo(directoryPath);
                var files = Directory.GetFiles(directoryPath, directoryInfo.Name + ".dll", SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    mites = mites.Concat(LoadMiteFromAssembly(file)).ToList();
                }
            }

            return mites;
        }

        private static IEnumerable<Mite> LoadMiteFromAssembly(string assemblyName)
        {
            var mites = new Collection<Mite>();
            
            var assembly = Assembly.LoadFile(Path.GetFullPath(assemblyName));

            var fileName = Path.GetFileName(Path.GetFullPath(assemblyName));
            var directory = Path.GetDirectoryName(Path.GetFullPath(assemblyName));

            LoadReferencedAssemblies(assembly, fileName, directory);

            foreach (var definedType in assembly.DefinedTypes)
            {
                if (definedType.BaseType.FullName == typeof(Mite).FullName)
                {
                    var dynamiclyLoadedMite = (Mite) Activator.CreateInstance(assembly.GetType(definedType.FullName));
                    mites.Add(dynamiclyLoadedMite);                    
                    dynamiclyLoadedMite.Configuration = JObject.Parse(File.ReadAllText(Path.Combine(directory, Path.GetFileNameWithoutExtension(fileName) + ".json"))); 
                }
            }

            return mites;
        }
        
        private static void LoadReferencedAssemblies(Assembly assembly, string fileName, string directory)
        {
            var filesInDirectory = Directory.GetFiles(directory).Where(x => x != fileName)
                .Select(x => Path.GetFileNameWithoutExtension(x)).ToList();
            var references = assembly.GetReferencedAssemblies();

            foreach (var reference in references)
            {
                if (filesInDirectory.Contains(reference.Name))
                {
                    var loadFileName = reference.Name + ".dll";
                    var path = Path.Combine(directory, loadFileName);
                    var loadedAssembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(path);
                    if (loadedAssembly != null)
                        LoadReferencedAssemblies(loadedAssembly, loadFileName, directory);
                }
            }
        }
    }
}