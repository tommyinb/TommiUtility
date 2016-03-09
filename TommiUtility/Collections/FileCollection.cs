using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Diagnostics.Contracts;
using TommiUtility.Test;

namespace TommiUtility.Collections
{
    public sealed class FileCollection<T> : ObservableCollection<T>
    {
        public FileCollection(string filePath)
        {
            Contract.Requires<ArgumentNullException>(filePath != null);
            Contract.Requires<ArgumentException>(filePath.Length > 0);

            this.FilePath = filePath;

            this.ReadFile();
        }

        public readonly string FilePath;
        [ContractInvariantMethod]
        private void ObjectInvariants()
        {
            Contract.Invariant(FilePath != null);
            Contract.Invariant(FilePath.Length > 0);
        }

        public void ReadFile()
        {
            if (File.Exists(FilePath) == false) return;

            T[] items;
            using (var fileStream = File.OpenRead(FilePath))
            {
                var xmlSerializer = new XmlSerializer(typeof(T[]));

                items = (T[])xmlSerializer.Deserialize(fileStream);
            }

            if (items != null)
            {
                foreach (var item in items)
                {
                    this.Add(item);
                }
            }
        }
        public void WriteFile()
        {
            var fileDirectory = Path.GetDirectoryName(FilePath);
            if (fileDirectory.Length > 0)
            {
                Directory.CreateDirectory(fileDirectory);
            }

            var items = this.ToArray();

            using (var fileStream = File.Open(FilePath, FileMode.Create))
            {
                var xmlSerializer = new XmlSerializer(typeof(T[]));

                xmlSerializer.Serialize(fileStream, items);
            }
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnCollectionChanged(e);

            WriteFile();
        }
    }

    [TestClass]
    public class FileCollectionTest
    {
        [TestMethod]
        public void Test()
        {
            var filePath = "test.txt";

            var fileCollectionA = new FileCollection<string>(filePath);
            fileCollectionA.Clear();

            fileCollectionA.Add("A");
            fileCollectionA.Add("B");

            var fileCollectionB = new FileCollection<string>(filePath);

            AssertUtil.SequenceEqual(fileCollectionA, fileCollectionB);

            File.Delete(filePath);
        }
    }
}
