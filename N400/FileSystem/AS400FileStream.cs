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
        public FileAttributes InitialAttributes { get; private set; }

        OpenMode openMode;
        uint fileHandle;
        IfsService service;

        long position, length;

        internal AS400FileStream(uint handle, OpenMode mode, FileAttributes attribs, IfsService ifs)
        {
            InitialAttributes = attribs;
            openMode = mode;
            service = ifs;
            fileHandle = handle;

            position = 0;
            length = Convert.ToInt64(InitialAttributes.FileSize);
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
        /// If the position can be moved.
        /// </summary>
        public override bool CanSeek => true;
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
        /// Forces the changes to the file to be commited.
        /// </summary>
        public override void Flush()
        {
            service.Commit(fileHandle);
        }

        // everything's bad because protocol is unsigned but Stream is signed
        // so we cheat and pretend everything's signed for Read/Write packets
        /// <summary>
        /// Reads bytes from the stream into a byte array.
        /// </summary>
        /// <param name="buffer">The byte array to write to.</param>
        /// <param name="offset">
        /// The position of the byte array in which to write.
        /// </param>
        /// <param name="count">How many bytes to read.</param>
        /// <returns>How many bytes were read.</returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            // HACK: try to satisfy StreamReader (not working yet)
            if (Position >= Length)
                return 0;

            var tmpBuf = service.Read(fileHandle, Position, count);
            var ret = tmpBuf.Length;

            Array.Copy(tmpBuf, 0, buffer, offset, ret);

            Position += ret;
            return ret;
        }

        /// <summary>
        /// Moves the position to the relative mode.
        /// </summary>
        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Current:
                    Position += offset;
                    break;
                case SeekOrigin.Begin:
                    Position = offset;
                    break;
                case SeekOrigin.End:
                    Position = Length + offset;
                    break;
            }
            return Position;
        }

        /// <summary>
        /// Sets a new length for the stream.
        /// </summary>
        /// <param name="value">The new length value.</param>
        public override void SetLength(long value)
        {
            length = value;
        }

        /// <summary>
        /// Writes data to the stream.
        /// </summary>
        /// <param name="buffer">
        /// The byte array to read data from.
        /// </param>
        /// <param name="offset">
        /// Where in the array to start writing from.
        /// </param>
        /// <param name="count">How many bytes to write.</param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            var tmpBuf = buffer.Slice(offset, count);

            var unwritten = service.Write(fileHandle,
                tmpBuf,
                Position,
                false,
                InitialAttributes.DataCCSID);

            var newLen = Position + (tmpBuf.Length - unwritten);
            Position = newLen;
            if (newLen > Length)
                SetLength(newLen);
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
