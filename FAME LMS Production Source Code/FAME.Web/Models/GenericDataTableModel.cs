using System.Collections.Generic;

namespace First_Aid_Made_Easy.Models
{
    public class DataTableResult<T>
    {
        public string draw { get; set; }
        public int recordsTotal { get; set; }
        public int recordsFiltered { get; set; }
        public List<T> data { get; set; }
    }
}
