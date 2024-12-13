
using DotnetBeBase.Enums;
using DotnetBeBase.Extensions;

namespace DotnetBeBase.Models.Basic
{
    public class BaseResponseMessage
    {
        public Error error { get; set; } = new Error();
        public class Error
        {
            /// <summary>
            /// Mã code lỗi
            /// </summary>
            public ErrorCode Code { get; set; }
            /// <summary>
            /// Mô tả lỗi
            /// </summary>
            public string Message { get; set; }
            public Error(ErrorCode code = ErrorCode.SUCCESS)
            {
                this.Code = code;
                this.Message = code.ToDescriptionString();
            }

            internal void SetErrorCode(ErrorCode errorCode)
            {
                Code = errorCode;
                Message = errorCode.ToDescriptionString();
            }
        }

    }
    public class BaseResponseMessage<T> : BaseResponseMessage
    {
        /// <summary>
        /// Dữ liệu đầu ra
        /// </summary>
        public T? Data { get; set; }
    }
    public class BaseResponseMessageItems<T> : BaseResponseMessage
    {
        public List<T> Items { get; set; } = new List<T>();
    }
    /// <summary>
    /// mess phân trang
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BaseResponseMessagePage<T> : BaseResponseMessage
    {
        /// <summary>
        /// Danh sách Items
        /// </summary>
        public List<T> Items { get; set; } = new List<T>();
        /// <summary>
        /// Phân trang
        /// </summary>
        public Paginations Pagination { get; set; } = new Paginations();
        public class Paginations
        {
            /// <summary>
            /// Tổng số item trên 1 trang
            /// </summary>
            public int TotalCount { get; set; } = 0;
            /// <summary>
            /// Tổng số trang
            /// </summary>
            public int TotalPage { get; set; } = 0;
        }
    }
}
