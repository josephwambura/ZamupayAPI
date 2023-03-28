using System.Collections.Generic;

namespace ZamuPay.API.DTOs
{
    public class ZamuApiResult<T> where T : class
    {
        private readonly List<ErrorDetailDTO> _errors = new List<ErrorDetailDTO>();

        /// <summary>
        /// Flag indicating whether an operation succeeded or not. The default is false.
        /// </summary>
        public bool Succeeded { get; set; } = false;


        /// <summary>
        /// Represents an item[s] retrieved if and only if Suceeded is true otherwise it is null.
        /// </summary>
        public T? Items { get; set; }

        /// <summary>
        /// A list of <see cref="ErrorDetailDTO"/> instance containing errors from the ZamuPay service.
        /// </summary>
        public List<ErrorDetailDTO>? Errors => _errors;


        /// <summary>
        /// Handle successful operation returning with an optional object item.
        /// </summary>
        /// <param name="items"> An object</param>
        /// <returns>A <see cref="ZamuApiResult"/> indicating a successful operation </returns>

        public ZamuApiResult<T> Success(T? items)
        {
            var result = new ZamuApiResult<T> { Succeeded = true };

            if (items != null)
            {
                result.Items = items;
            }
            return result;
        }

        /// <summary>
        /// Handle failed operation with an optional <see cref="List{T}"/> of errors.
        /// </summary>
        /// <param name="errors"> An optional <see cref="List{T}"/> of <see cref="ErrorDetailDTO"/> instance</param>
        /// <returns>A <see cref="ZamuApiResult"/> indicating a failed operation</returns>

        public ZamuApiResult<T> Failed(List<ErrorDetailDTO>? errors) {

            var result = new ZamuApiResult<T> { Succeeded = false };

            if(errors != null)
            {
                result._errors.AddRange(errors);
            }

            return result;
        }
    }
}
