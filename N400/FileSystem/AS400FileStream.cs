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

        internal AS400FileStream(uint handle, OpenMode mode, FileAttributes attribs, IfsService ifs)
        {
            FileAttributes = attribs;
            openMode = mode;
            service = ifs;
            fileHandle = handle;
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

        public override long Length
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override long Position
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
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
