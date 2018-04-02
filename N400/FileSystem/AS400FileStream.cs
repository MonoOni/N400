using N400.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace N400.FileSystem
{
    /// <summary>
    /// Represents access to a file on a server.
    /// </summary>
    public class AS400FileStream : Stream
    {
        /// <summary>
        /// The file's attributes from when the handle was opened.
        /// </summary>
        public FileAttributes FileAttributes { get; private set; }

        OpenMode openMode;
        uint fileHandle;
        IfsService service;

        long position, length;

        internal AS400FileStream(uint handle, OpenMode mode, FileAttributes attribs, IfsService ifs)
        {
            FileAttributes = attribs;
            openMode = mode;
            service = ifs;
            fileHandle = handle;

            position = 0;
        }

        #region Base Class Members
        /// <summary>
        /// If the stream can be read from.
        /// </summary>
        public override bool CanRead =>
            openMode == OpenMode.Read || openMode == OpenMode.ReadWrite;

        /// <summary>
        /// If the stream can be written to.
        /// </summary>
        public override bool CanWrite =>
            openMode == OpenMode.Write || openMode == OpenMode.ReadWrite;

        /// <summary>
        /// Not supported for this stream.
        /// </summary>
        public override bool CanSeek
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        /// <summary>
        /// The length of the stream.
        /// </summary>
        public override long Length => length;

        /// <summary>
        /// The current position of the stream.
        /// </summary>
        public override long Position
        {
            get
            {
                return position;
            }
            set
            {
                position = value;
            }
        }

        /// <summary>
        /// No-op for this stream.
        /// </summary>
        public override void Flush()
        {
            // no-op
        }

        // everything's bad because protocol is unsigned but Stream is signed
        // so we cheat and pretend everything's signed for Read/Write packets
        public override int Read(byte[] buffer, int offset, int count)
        {
            // HACK: try to satisfy StreamReader (not working yet)
            if (Position > Length)
                return 0;

            var tmpBuf = service.Read(fileHandle, Position, count);
            var ret = tmpBuf.Length;

            Array.Copy(tmpBuf, 0, buffer, offset, ret);

            Position += ret;
            return ret;
        }

        /// <summary>
        /// Not supported for this stream.
        /// </summary>
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            length = value;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            var tmpBuf = buffer.Slice(offset, count);

            var unwritten = service.Write(fileHandle, tmpBuf, Position, false, FileAttributes.DataCCSID);

            Position += (tmpBuf.Length - unwritten);
            SetLength(Length + (tmpBuf.Length - unwritten));
        }

        /// <summary>
        /// Destroys the resources used by the stream.
        /// </summary>
        /// <param name="disposing">
        /// If managed objects should be destroyed.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            service.Close(fileHandle);
        }
        #endregion
    }
}
