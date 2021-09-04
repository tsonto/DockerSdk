using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DockerSdk.Core;
using System.Buffers;

// Suppress this warning for now. TODO: revisit
#pragma warning disable CA1835 // Prefer the 'Memory'-based overloads for 'ReadAsync' and 'WriteAsync'

namespace Microsoft.Net.Http.Client
{
    internal class BufferedReadStream : WriteClosableStream, IPeekableStream
    {
        private const char CR = '\r';
        private const char LF = '\n';

        private readonly Stream _inner;
        private readonly Socket? _socket;
        private readonly byte[] _buffer;
        private volatile int _bufferRefCount;
        private int _bufferOffset = 0;
        private int _bufferCount = 0;
        private bool _disposed;

        public BufferedReadStream(Stream inner, Socket? socket)
            : this(inner, socket, 1024)
        { }

        public BufferedReadStream(Stream inner, Socket? socket, int bufferLength)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _socket = socket;
            _bufferRefCount = 1;
            _buffer = ArrayPool<byte>.Shared.Rent(bufferLength);
        }

        public override bool CanRead
        {
            get { return _inner.CanRead || _bufferCount > 0; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanTimeout
        {
            get { return _inner.CanTimeout; }
        }

        public override bool CanWrite
        {
            get { return _inner.CanWrite; }
        }

        public override long Length
        {
            get { throw new NotSupportedException(); }
        }

        public override long Position
        {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                _disposed = true;
                if (disposing)
                {
                    _inner.Dispose();
                    if (Interlocked.Decrement(ref _bufferRefCount) == 0)
                    {
                        ArrayPool<byte>.Shared.Return(_buffer);
                    }
                }
            }
        }

        public override void Flush()
        {
            _inner.Flush();
        }

        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            return _inner.FlushAsync(cancellationToken);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _inner.Write(buffer, offset, count);
        }

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return _inner.WriteAsync(buffer, offset, count, cancellationToken);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int read = ReadBuffer(buffer, offset, count);
            if (read > 0)
            {
                return read;
            }

            return _inner.Read(buffer, offset, count);
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            int read = ReadBuffer(buffer, offset, count);
            if (read > 0)
            {
                return Task.FromResult(read);
            }

            return _inner.ReadAsync(buffer, offset, count, cancellationToken);
        }

        public bool Peek(byte[] buffer, uint toPeek, out uint peeked, out uint available, out uint remaining)
        {
            int read = PeekBuffer(buffer, toPeek, out peeked, out available, out remaining);
            if (read > 0)
            {
                return true;
            }

            if (_inner is IPeekableStream peekableStream)
            {
                return peekableStream.Peek(buffer, toPeek, out peeked, out available, out remaining);
            }

            throw new NotSupportedException("_inner stream isn't a peekable stream");
        }

        private int ReadBuffer(byte[] buffer, int offset, int count)
        {
            if (_bufferCount > 0)
            {
                int toCopy = Math.Min(_bufferCount, count);
                Buffer.BlockCopy(_buffer, _bufferOffset, buffer, offset, toCopy);
                _bufferOffset += toCopy;
                _bufferCount -= toCopy;
                return toCopy;
            }

            return 0;
        }

        private int PeekBuffer(byte[] buffer, uint toPeek, out uint peeked, out uint available, out uint remaining)
        {
            if (_bufferCount > 0)
            {
                int toCopy = Math.Min(_bufferCount, (int)toPeek);
                Buffer.BlockCopy(_buffer, _bufferOffset, buffer, 0, toCopy);
                peeked = (uint) toCopy;
                available = (uint)_bufferCount;
                remaining = available - peeked;
                return toCopy;
            }

            peeked = 0;
            available = 0;
            remaining = 0;
            return 0;
        }

        private async Task EnsureBufferedAsync(CancellationToken cancel)
        {
            if (_bufferCount == 0)
            {
                _bufferOffset = 0;
                bool validBuffer = Interlocked.Increment(ref _bufferRefCount) > 1;
                try
                {
                    if (validBuffer)
                    {
                        _bufferCount = await _inner.ReadAsync(_buffer, _bufferOffset, _buffer.Length, cancel).ConfigureAwait(false);
                    }
                }
                catch (IOException ex) when (ex.Message.Contains("Operation canceled"))
                {
                    throw new OperationCanceledException("The operation has been cancelled.", ex);
                }
                finally
                {
                    if ((Interlocked.Decrement(ref _bufferRefCount) == 0) && validBuffer)
                    {
                        ArrayPool<byte>.Shared.Return(_buffer);
                    }
                }

                if (_bufferCount == 0)
                {
                    if (_disposed)
                        throw new ObjectDisposedException(nameof(BufferedReadStream));
                    else
                        throw new IOException("Unexpected end of stream");
                }
            }
        }

        // TODO: Line length limits?
        public async Task<string> ReadLineAsync(CancellationToken cancel)
        {
            ThrowIfDisposed();
            StringBuilder builder = new StringBuilder();
            bool foundCR = false, foundCRLF = false;
            do
            {
                if (_bufferCount == 0)
                {
                    await EnsureBufferedAsync(cancel).ConfigureAwait(false);
                }

                char ch = (char)_buffer[_bufferOffset]; // TODO: Encoding enforcement
                builder.Append(ch);
                _bufferOffset++;
                _bufferCount--;
                if (ch == CR)
                {
                    foundCR = true;
                }
                else if (ch == LF)
                {
                    if (foundCR)
                    {
                        foundCRLF = true;
                    }
                    else
                    {
                        foundCR = false;
                    }
                }
            }
            while (!foundCRLF);

            return builder.ToString(0, builder.Length - 2); // Drop the CRLF
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(BufferedReadStream));
            }
        }

        public override bool CanCloseWrite => _socket != null || _inner is WriteClosableStream;

        public override void CloseWrite()
        {
            if (_socket != null)
            {
                _socket.Shutdown(SocketShutdown.Send);
                return;
            }

            if (_inner is WriteClosableStream s)
            {
                s.CloseWrite();
                return;
            }

            throw new NotSupportedException("Cannot shutdown write on this transport");
        }
    }
}
