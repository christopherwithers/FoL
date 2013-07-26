using System;
using System.IO;
using System.Web;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.Store;
using eMotive.Search.Interfaces;
using Version = Lucene.Net.Util.Version;

namespace eMotive.Search.Objects
{
    public class SearchDocumentManager : ISearchDocumentManager, IDisposable
    {
        private readonly FSDirectory directory;
        private readonly IndexWriter writer;

        public SearchDocumentManager(string _indexLocation)
        {
            if(string.IsNullOrEmpty(_indexLocation))
                throw new FileNotFoundException("The lucene index could not be found.");

            var resolvedServerLocation = HttpContext.Current.Server.MapPath(string.Format("~{0}", _indexLocation));
            directory = FSDirectory.Open(new DirectoryInfo(resolvedServerLocation));
            writer = new IndexWriter(directory, new StandardAnalyzer(Version.LUCENE_30), false, IndexWriter.MaxFieldLength.UNLIMITED);
        }

        public bool Add(ISearchDocument _document)
        {
            var success = true;
            try
            {
                writer.AddDocument(_document.BuildRecord());
                writer.Commit();
            }
            catch (AlreadyClosedException)
            {
                success = false;
            }
            catch (Exception)
            {
                success = false;
            }

            return success;
        }

        public bool Update(ISearchDocument _document)
        {
            var success = true;
            try
            {
                writer.UpdateDocument(new Term("UniqueID", _document.UniqueID), _document.BuildRecord());
                writer.Commit();
            }
            catch (AlreadyClosedException)
            {
                success = false;
            }
            catch (Exception)
            {
                success = false;
            }

            return success;
        }

        public bool Delete(ISearchDocument _document)
        {
            var success = true;
            try
            {
                writer.DeleteDocuments(new Term("UniqueID", _document.UniqueID));
                writer.Commit();
            }
            catch (AlreadyClosedException)
            {
                success = false;
            }
            catch (Exception)
            {
                success = false;
            }

            return success;
            
        }

        public void Dispose()
        {
            writer.Optimize();

            writer.Dispose();
            directory.Dispose();
        }
    }
}
