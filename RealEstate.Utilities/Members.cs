using System.Data;

namespace RealEstate.Utilities
{
    public class MEMBERS
    {
        public struct SQLReturnMessageNValue
        {
            public object Outval { get; set; }
            public string Outmsg { get; set; }
        }

        public struct SQLReturnMessageNValueNDataTable
        {
            public object Outval { get; set; }
            public string Outmsg { get; set; }
            public DataTable dt { get; set; }
        }

        public struct SQLReturnValue
        {
            public object Outval { get; set; }
        }

        public struct SQLReturnMessage
        {
            public int Outmsg { get; set; }
        }

        public struct SQLReturnValueNList
        {
            public object ListValue { get; set; }
            public int TotalRecord { get; set; }
        }

        public class PagingResponse
        {
            public string Data { get; set; }
            public object FirstRecord { get; set; }
            public object LastRecord { get; set; }
            public object TotalRecords { get; set; }
            public object TotalPage { get; set; }
        }

        public class OnDemandResponse
        {
            public string Data { get; set; }
            public object TotalPage { get; set; }
        }
    }
}
