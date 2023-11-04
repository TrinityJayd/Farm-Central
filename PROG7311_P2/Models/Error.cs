namespace PROG7311_P2.Models
{
    public class Error
    {
        public int Code { get; set; }
        public string Message { get; set; }
        public List<Error> Errors { get; set; }
    }
}
