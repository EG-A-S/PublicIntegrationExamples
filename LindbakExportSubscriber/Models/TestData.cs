namespace LindbakExportSubscriber.Models
{
    /// <summary>
    /// Example of data in a batch stored in a blob.
    /// Each item in the batch is of type TestData
    /// </summary>
    public class TestData
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Subitem[] Subitems { get; set; }
    }
}