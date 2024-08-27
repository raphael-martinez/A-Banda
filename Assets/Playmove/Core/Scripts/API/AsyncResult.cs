namespace Playmove.Core
{
    /// <summary>
    /// Basic class to keep a standard in async callbacks results
    /// </summary>
    /// <typeparam name="T">Data type</typeparam>
    public class AsyncResult<T>
    {
        /// <summary>
        /// Result data
        /// </summary>
        public T Data;
        /// <summary>
        /// Error message
        /// </summary>
        public string Error;
        /// <summary>
        /// Indicates if it has an Error
        /// </summary>
        public bool HasError { get { return !string.IsNullOrEmpty(Error); } }
        /// <summary>
        /// Indicates if it don't have an Error
        /// </summary>
        public bool Success { get { return string.IsNullOrEmpty(Error); } }

        public AsyncResult()
        {
            Data = default;
            Error = string.Empty;
        }
        public AsyncResult(T data, string error)
        {
            Data = data;
            Error = error;
        }

        public override string ToString()
        {
            return $"Data: {Data} | Error: {(string.IsNullOrEmpty(Error) ? "None" : Error)}";
        }
    }

    /// <summary>
    /// Basic class to keep a standard in callbacks from any async method
    /// </summary>
    /// <typeparam name="T">Data type</typeparam>
    /// <param name="result">AsyncResult with data for this callback</param>
    public delegate void AsyncCallback<T>(AsyncResult<T> result);
    public delegate void AsyncCallback();
}
