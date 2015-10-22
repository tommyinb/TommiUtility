using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TommiUtility.FileSystem
{
    public abstract class FileSearch
    {
        public static FileSearch Create(params string[] paths)
        {
            if (paths.Length > 1)
            {
                var rootSearches = paths.Select(t => new RootFileSearch(t));

                return new CombineFileSearch(rootSearches.ToArray());
            }
            else
            {
                var path = paths.First();

                return new RootFileSearch(path);
            }
        }

        public FileSearch Next(string pattern, SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            return new SubFileSearch(this, pattern, searchOption);
        }
        public FileSearch Next(string[] patterns, SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            var subSearches = patterns.Select(t => new SubFileSearch(this, t, searchOption));

            return new CombineFileSearch(subSearches.ToArray());
        }
        
        public FileSearch Concat(FileSearch fileSearch)
        {
            return new CombineFileSearch(new[] { this, fileSearch });
        }

        public abstract IEnumerable<string> GetFiles();
        public abstract IEnumerable<string> GetDirectories();
    }

    internal class RootFileSearch : FileSearch
    {
        public RootFileSearch(string path)
        {
            this.path = path;
        }
        private string path;

        public override IEnumerable<string> GetFiles()
        {
            return new string[0];
        }
        public override IEnumerable<string> GetDirectories()
        {
            if (Directory.Exists(path))
            {
                yield return path;
            }
        }
    }

    internal class SubFileSearch : FileSearch
    {
        public SubFileSearch(FileSearch parent, string pattern, SearchOption searchOption)
        {
            this.parent = parent;
            this.pattern = pattern;
            this.searchOption = searchOption;
        }
        private FileSearch parent;
        private string pattern;
        private SearchOption searchOption;

        public override IEnumerable<string> GetFiles()
        {
            return parent.GetDirectories().SelectMany(t =>
                Directory.EnumerateFiles(t, pattern, searchOption));
        }
        public override IEnumerable<string> GetDirectories()
        {
            return parent.GetDirectories().SelectMany(t =>
                Directory.EnumerateDirectories(t, pattern, searchOption));
        }
    }

    internal class CombineFileSearch : FileSearch
    {
        public CombineFileSearch(params FileSearch[] fileSearches)
        {
            this.fileSearches = fileSearches;
        }
        private FileSearch[] fileSearches;

        public override IEnumerable<string> GetFiles()
        {
            return fileSearches.SelectMany(t => t.GetFiles());
        }
        public override IEnumerable<string> GetDirectories()
        {
            return fileSearches.SelectMany(t => t.GetDirectories());
        }
    }

    [TestClass]
    public class FileSearchTest
    {
        [TestMethod]
        public void Test()
        {
            var createFile = new Action<string>(path =>
            {
                var directory = Path.GetDirectoryName(path);
                Directory.CreateDirectory(directory);

                File.WriteAllText(path, "test");
            });
            createFile(@"Test\123\AAA\111.txt");
            createFile(@"Test\234\AAA\111.txt");
            createFile(@"Test\123\BBB\111.txt");
            createFile(@"Test\234\BBB\111.txt");
            createFile(@"Test\234\BBB\123\AAA\111.txt");
            createFile(@"Test\234\BBB\123\KKK\AAA\111.txt");
            try
            {
                var search1 = FileSearch.Create("Test")
                    .Next("123")
                    .Next(new[] { "AAA", "BBB" })
                    .Next("*.txt")
                    .GetFiles().ToArray();
                Assert.AreEqual(2, search1.Length);
                Assert.AreEqual(@"Test\123\AAA\111.txt", search1[0]);
                Assert.AreEqual(@"Test\123\BBB\111.txt", search1[1]);

                var search2 = FileSearch.Create("Test")
                    .Next("123", SearchOption.AllDirectories)
                    .Next("AAA", SearchOption.AllDirectories)
                    .GetDirectories().ToArray();
                Assert.AreEqual(3, search2.Length);
                Assert.AreEqual(@"Test\123\AAA", search2[0]);
                Assert.AreEqual(@"Test\234\BBB\123\AAA", search2[1]);
                Assert.AreEqual(@"Test\234\BBB\123\KKK\AAA", search2[2]);

                var search3 = FileSearch.Create("Test")
                    .Next("123", SearchOption.AllDirectories)
                    .Next("AAA", SearchOption.AllDirectories)
                    .Next("111.txt").GetFiles().ToArray();
                Assert.AreEqual(3, search3.Length);
                Assert.AreEqual(@"Test\123\AAA\111.txt", search3[0]);
                Assert.AreEqual(@"Test\234\BBB\123\AAA\111.txt", search3[1]);
                Assert.AreEqual(@"Test\234\BBB\123\KKK\AAA\111.txt", search3[2]);
            }
            finally
            {
                Directory.Delete("Test", recursive: true);
            }
        }
    }
}
