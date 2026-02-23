namespace TestAcad08.MCP
{
    public static class DocumentManager
    {
        public static bool SwitchDocument(string documentName)
        {
            try
            {
                var docs = Acap.DocumentManager;

                Document? targetDoc = null;

                foreach (Document doc in docs)
                {
                    if (doc.Name.Equals(documentName, StringComparison.OrdinalIgnoreCase) ||
                        System.IO.Path.GetFileName(doc.Name).Equals(documentName, StringComparison.OrdinalIgnoreCase))
                    {
                        targetDoc = doc;
                        break;
                    }
                }

                if (targetDoc == null)
                {
                    return false;
                }

                if (docs.MdiActiveDocument != targetDoc)
                {
                    docs.MdiActiveDocument = targetDoc;
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SwitchDocument error: {ex.Message}");
                return false;
            }
        }

        public static string[] GetAllDocuments()
        {
            try
            {
                var docs = Acap.DocumentManager;
                return docs.Cast<Document>().Select(d => d.Name).ToArray();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GetAllDocuments error: {ex.Message}");
                return [];
            }
        }

        public static string GetActiveDocumentName()
        {
            try
            {
                return Acap.DocumentManager.MdiActiveDocument?.Name ?? "";
            }
            catch
            {
                return "";
            }
        }
    }
}
