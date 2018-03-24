using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace N400.DataQueues
{
    /// <summary>
    /// Represents a read data queue entry.
    /// </summary>
    public class DataQueueEntry
    {
        /// <summary>
        /// A buffer containing the entry's data.
        /// </summary>
        public byte[] Data { get; private set; }

        /// <summary>
        /// Information about the sender of the entry.
        /// </summary>
        public string SenderInfo { get; private set; }

        public byte[] Key { get; private set; }

        /// <summary>
        /// Creates a new data queue entry object.
        /// </summary>
        /// <param name="data">A buffer containing the entry's data.</param>
        /// <param name="senderInfo">Information about the sender of the entry.</param>
        public DataQueueEntry(byte[] data, string senderInfo, byte[] key)
        {
            Data = data;
            SenderInfo = senderInfo;
            Key = key;
        }
    }
}
