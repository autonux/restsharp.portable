﻿using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using JetBrains.Annotations;

using RestSharp.Portable.Impl;

namespace RestSharp.Portable.Content
{
    internal class StringContent : IHttpContent
    {
        private readonly string _value;

        private readonly Encoding _encoding;

        private byte[] _buffer;

        public StringContent([NotNull] string value, [NotNull] Encoding encoding)
        {
            _value = value;
            _encoding = encoding;
            Headers = new GenericHttpHeaders();
            Headers.TryAddWithoutValidation("Content-Type", $"text/plain; charset={encoding.WebName}");
        }

        /// <summary>
        /// Gets the HTTP headers for the content.
        /// </summary>
        public IHttpHeaders Headers { get; }

        public void Dispose()
        {
        }

        /// <summary>
        /// Asynchronously copy the data to the given stream.
        /// </summary>
        /// <param name="stream">The stream to copy to</param>
        /// <returns>The task that copies the data to the stream</returns>
        public async Task CopyToAsync(Stream stream)
        {
            var data = _buffer ?? _encoding.GetBytes(_value);
            await stream.WriteAsync(data, 0, data.Length);
        }

        /// <summary>
        /// Gets the raw content as byte array.
        /// </summary>
        /// <param name="maxBufferSize">The maximum buffer size</param>
        /// <returns>The task that loads the data into an internal buffer</returns>
        public async Task LoadIntoBufferAsync(long maxBufferSize)
        {
            _buffer = _encoding.GetBytes(_value);
#if PCL && !ASYNC_PCL
            await TaskEx.Yield();
#else
            await Task.Yield();
#endif
        }

        /// <summary>
        /// Returns the data as a stream
        /// </summary>
        /// <returns>The task that returns the stream</returns>
        public Task<Stream> ReadAsStreamAsync()
        {
            var data = _buffer ?? _encoding.GetBytes(_value);
#if PCL && !ASYNC_PCL
            return TaskEx.FromResult<Stream>(new MemoryStream(data));
#else
            return Task.FromResult<Stream>(new MemoryStream(data));
#endif
        }

        /// <summary>
        /// Returns the data as byte array
        /// </summary>
        /// <returns>The task that returns the data as byte array</returns>
        public Task<byte[]> ReadAsByteArrayAsync()
        {
            var data = _buffer ?? _encoding.GetBytes(_value);
#if PCL && !ASYNC_PCL
            return TaskEx.FromResult(data);
#else
            return Task.FromResult(data);
#endif
        }

        /// <summary>
        /// Returns the data as string
        /// </summary>
        /// <returns>The task that returns the data as string</returns>
        public Task<string> ReadAsStringAsync()
        {
#if PCL && !ASYNC_PCL
            return TaskEx.FromResult(_value);
#else
            return Task.FromResult(_value);
#endif
        }

        /// <summary>
        /// Determines whether the HTTP content has a valid length in bytes.
        /// </summary>
        /// <returns>
        /// Returns <see cref="T:System.Boolean"/>.true if <paramref name="length"/> is a valid length; otherwise, false.
        /// </returns>
        /// <param name="length">The length in bytes of the HTTP content.</param>
        public bool TryComputeLength(out long length)
        {
            length = _buffer?.Length ?? _encoding.GetByteCount(_value);

            return true;
        }
    }
}
