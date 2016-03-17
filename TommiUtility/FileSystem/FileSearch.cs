using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TommiUtility.Test;

namespace TommiUtility.FileSystem
{
    [ContractClass(typeof(FileSearchContract))]
    public abstract class FileSearch
    {
        public static FileSearch Create(params string[] paths)
        {
            Contract.Requires<ArgumentNullException>(paths != null);
            Contract.Ensures(Contract.Result<FileSearch>() != null);

            var validPaths = paths.Where(t => string.IsNullOrEmpty(t) == false).ToArray();
            if (validPaths.Length == 1)
            {
                var path = validPaths[0];
                Contract.Assume(path != null);
                Contract.Assume(path.Length > 0);

                return new RootFileSearch(path);
            }
            else
            {
                var rootSearches = validPaths.Select(t => new RootFileSearch(t)).ToArray();
                Contract.Assume(Contract.ForAll(0, rootSearches.Length, i => rootSearches[i] != null));

                return new CombineFileSearch(rootSearches);
            }
        }

        public FileSearch Next(string pattern, SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            Contract.Requires<ArgumentNullException>(pattern != null);
            Contract.Requires<ArgumentException>(pattern.Length > 0);
            Contract.Ensures(Contract.Result<FileSearch>() != null);

            return new SubFileSearch(this, pattern, searchOption);
        }
        public FileSearch Next(string[] patterns, SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            Contract.Requires<ArgumentNullException>(patterns != null);
            Contract.Ensures(Contract.Result<FileSearch>() != null);

            var subSearches = patterns.Select(t => new SubFileSearch(this, t, searchOption)).ToArray();
            Contract.Assume(Contract.ForAll(0, subSearches.Length, i => subSearches[i] != null));

            return new CombineFileSearch(subSearches);
        }
        
        public FileSearch Concat(FileSearch fileSearch)
        {
            Contract.Requires<ArgumentNullException>(fileSearch != null);
            Contract.Ensures(Contract.Result<FileSearch>() != null);

            return new CombineFileSearch(new[] { this, fileSearch });
        }

        public abstract IEnumerable<string> GetFiles();
        public abstract IEnumerable<string> GetDirectories();
    }

    [ContractClassFor(typeof(FileSearch))]
    public abstract class FileSearchContract : FileSearch
    {
        public override IEnumerable<string> GetFiles()
        {
            Contract.Ensures(Contract.Result<IEnumerable<string>>() != null);
            return null;
        }
        public override IEnumerable<string> GetDirectories()
        {
            Contract.Ensures(Contract.Result<IEnumerable<string>>() != null);
            return null;
        }
    }

    internal class RootFileSearch : FileSearch
    {
        public RootFileSearch(string path)
        {
            Contract.Requires<ArgumentNullException>(path != null);
            Contract.Requires<ArgumentException>(path.Length > 0);

            this.path = path;
        }
        private readonly string path;

        public override IEnumerable<string> GetFiles()
        {
            if (File.Exists(path))
            {
                yield return path;
            }
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
            Contract.Requires<ArgumentNullException>(parent != null);
            Contract.Requires<ArgumentNullException>(pattern != null);
            Contract.Requires<ArgumentException>(pattern.Length > 0);

            this.parent = parent;
            this.pattern = pattern;
            this.searchOption = searchOption;
        }
        private readonly FileSearch parent;
        private readonly string pattern;
        private readonly SearchOption searchOption;
        [ContractInvariantMethod]
        private void ObjectInvariants()
        {
            Contract.Invariant(parent != null);
            Contract.Invariant(pattern != null);
        }

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
            Contract.Requires<ArgumentNullException>(fileSearches != null);
            Contract.Requires<ArgumentException>(Contract.ForAll(0, fileSearches.Length, i => fileSearches[i] != null));

            this.fileSearches = fileSearches;
        }
        private readonly FileSearch[] fileSearches;
        [ContractInvariantMethod]
        private void ObjectInvariants()
        {
            Contract.Invariant(fileSearches != null);
        }

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
                AssertUtil.SequenceEqual(new[]
                {
                    @"Test\123\AAA\111.txt",
                    @"Test\123\BBB\111.txt"
                }, search1);

                Assert.AreEqual(2, search1.Length);
                Contract.Assume(search1.Length >= 2);
                Assert.AreEqual(@"Test\123\AAA\111.txt", search1[0]);
                Assert.AreEqual(@"Test\123\BBB\111.txt", search1[1]);

                var search2 = FileSearch.Create("Test")
                    .Next("123", SearchOption.AllDirectories)
                    .Next("AAA", SearchOption.AllDirectories)
                    .GetDirectories().ToArray();
                AssertUtil.SequenceEqual(new[]
                {
                    @"Test\123\AAA",
                    @"Test\234\BBB\123\AAA",
                    @"Test\234\BBB\123\KKK\AAA"
                }, search2);

                var search3 = FileSearch.Create("Test")
                    .Next("123", SearchOption.AllDirectories)
                    .Next("AAA", SearchOption.AllDirectories)
                    .Next("111.txt").GetFiles().ToArray();
                AssertUtil.SequenceEqual(new[]
                {
                    @"Test\123\AAA\111.txt",
                    @"Test\234\BBB\123\AAA\111.txt",
                    @"Test\234\BBB\123\KKK\AAA\111.txt"
                }, search3);
            }
            finally
            {
                Directory.Delete("Test", recursive: true);
            }
        }
    }
}
