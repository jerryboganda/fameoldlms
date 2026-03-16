using System.Collections.Generic;

namespace First_Aid_Made_Easy.Models
{
    /// <summary>
    /// Generic DataTable result wrapper for jQuery DataTables AJAX responses.
    /// </summary>
    public class DataTableResult<T>
    {
        public string draw { get; set; }
        public int recordsTotal { get; set; }
        public int recordsFiltered { get; set; }
        public List<T> data { get; set; }
        public string error { get; set; }
    }
}
