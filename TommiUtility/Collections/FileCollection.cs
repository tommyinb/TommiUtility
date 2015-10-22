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

namespace TommiUtility.Collections
{
    public class FileCollection<T> : ObservableCollection<T>
    {
        public FileCollection(string filePath)
        {
            this.FilePath = filePath;

            this.ReadFile();
        }

        public string FilePath { get; private set; }
        public void ReadFile()
        {
            if (File.Exists(FilePath) == false) return;

            T[] items;
            using (var fileStream = File.OpenRead(FilePath))
            {
                var xmlSerializer = new XmlSerializer(typeof(T[]));

                items = (T[])xmlSerializer.Deserialize(fileStream);
            }

            foreach (var item in items)
            {
                this.Add(item);
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

            Assert.AreEqual(fileCollectionA.Count, fileCollectionB.Count);
            for (int i = 0; i < fileCollectionA.Count; i++)
            {
                Assert.AreEqual(fileCollectionA[i], fileCollectionB[i]);
            }

            File.Delete(filePath);
        }
    }
}
