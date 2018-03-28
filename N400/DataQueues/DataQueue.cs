using N400.Globalization;
using N400.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace N400.DataQueues
{
    /// <summary>
    /// Represents access to data queues on a server.
    /// </summary>
    /// <example>
    /// This command can be used to create data queue entries:
    /// <code>
    /// CRTDTAQ DTAQ(LANDO/TESTQ) MAXLEN(256)
    /// </code>
    /// This CL program can then be used to push entries onto the queue:
    /// <code>
    /// PGM                                                    
    /// DCL VAR(&amp;DQNAME) TYPE(*CHAR) LEN(10) VALUE('TESTQ')
    /// DCL VAR(&amp;DQLIB) TYPE(*CHAR) LEN(10) VALUE('LANDO')
    /// DCL VAR(&amp;DQSNDLEN) TYPE(*DEC) LEN(5 0) VALUE(14)
    /// DCL VAR(&amp;DQLEN) TYPE(*DEC) LEN(5 0)
    /// DCL VAR(&amp;DQSNDDATA) TYPE(*CHAR) LEN(100)
    /// CHGVAR VAR(&amp;DQSNDDATA) VALUE('THIS IS A TEST')
    /// CALL QSNDDTAQ PARM(&amp;DQNAME &amp;DQLIB &amp;DQSNDLEN &amp;DQSNDDATA)
    /// ENDPGM
    /// </code>
    /// This C# excerpt can then be used to read items from the queue, and then
    /// write them to the console:
    /// <code>
    /// var dtaq = new DataQueue(server, name: "TESTQ", library: "LANDO");
    /// var entry = dtaq.Peek(0);
    /// Console.WriteLine(
    ///     EbcdicConverter.FromEbcdicToString(
    ///         dqe?.Data?.ToString() ?? "no data"));
    /// </code>
    /// </example>
    public class DataQueue
    {
        DataQueueService service;

        byte[] nameEbcdic, libraryEbcdic;

        /// <summary>
        /// The servers to access data queues on.
        /// </summary>
        public Server Server { get; private set; }
        /// <summary>
        /// The name of the data queue to access.
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// The library that the data queue is stored in.
        /// </summary>
        public string Library { get; private set; }

        /// <summary>
        /// Creates a new data queue object.
        /// </summary>
        /// <param name="server">
        /// The servers to access data queues on.
        /// </param>
        /// <param name="name">
        /// The name of the data queue to access.
        /// </param>
        /// <param name="library">
        /// The library that the data queue is stored in.
        /// </param>
        public DataQueue(Server server, string name, string library)
        {
            if (server == null)
                throw new ArgumentNullException(nameof(server));
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (library == null)
                throw new ArgumentNullException(nameof(library));

            if (name == string.Empty || name.Length > 10)
                throw new ArgumentException("The name empty is or too long.", nameof(name));
            if (library == string.Empty || library.Length > 10)
                throw new ArgumentException("The ibrary name is empty or too long.", nameof(library));

            Server = server;

            Name = name;
            Library = library;
            nameEbcdic = EbcdicConverter.ToPadded(
                EbcdicConverter.ToEbcidic(name, Server.NLV),
                10);
            libraryEbcdic = EbcdicConverter.ToPadded(
                EbcdicConverter.ToEbcidic(library, Server.NLV),
                10);

            service = new DataQueueService(server);
        }
        
        // TODO: Path based ctor

        /// <summary>
        /// Creates the data queue.
        /// </summary>
        /// <param name="entryLength">The maximum length of an entry.</param>
        /// <param name="saveSenderInfo">
        /// If sender information should be saved.
        /// </param>
        /// <param name="fifo">
        /// If the queue should be a "first in, first out" queue.
        /// </param>
        /// <param name="authority">
        /// The authority level of the queue.
        /// </param>
        /// <param name="keyLength">The length of the key.</param>
        /// <param name="forceStorage">
        /// If the queue should persist to storage.
        /// </param>
        /// <param name="description">
        /// A human-readable description of the data queue.
        /// </param>
        public void Create(uint entryLength,
            bool saveSenderInfo = false,
            bool fifo = true,
            DataQueueAuthority authority = DataQueueAuthority.LibCrtAut,
            uint keyLength = 0,
            bool forceStorage = false,
            string description = "Queue")
        {
            if (description == null)
                throw new ArgumentNullException(nameof(description));
            if (description == string.Empty || description.Length > 50)
                throw new ArgumentException("The description is empty or too long.",
                    nameof(description));

            var descEbcidic = EbcdicConverter.ToPadded(
                EbcdicConverter.ToEbcidic(description, Server.NLV),
                50);

            service.Create(nameEbcdic,
                libraryEbcdic,
                entryLength,
                saveSenderInfo,
                fifo,
                authority,
                keyLength,
                forceStorage,
                descEbcidic);
        }

        /// <summary>
        /// Deletes the data queue on the server.
        /// </summary>
        public void Delete()
        {
            service.Delete(nameEbcdic, libraryEbcdic);
        }

        /// <summary>
        /// Clears the data quete with an optional key.
        /// </summary>
        /// <param name="key">The key to use.</param>
        public void Clear(byte[] key = null)
        {
            service.Clear(nameEbcdic, libraryEbcdic, key);
        }

        /// <summary>
        /// Writes an entry to the data queue.
        /// </summary>
        /// <param name="entry">The entry to write.</param>
        /// <param name="key">The key to use.</param>
        public void Write(byte[] entry, byte[] key = null)
        {
            service.Write(nameEbcdic, libraryEbcdic, entry, key);
        }
        
        // TODO: i'm pretty sure wait is supposed to be a signed int

        /// <summary>
        /// Reads a value from the queue and deletes it.
        /// </summary>
        /// <param name="wait">How long to wait for data to arrive.</param>
        /// <returns>The queued item.</returns>
        public DataQueueEntry Read(int wait)
        {
            return service.Read(nameEbcdic, libraryEbcdic, 0, wait, false, null);
        }

        /// <summary>
        /// Reads a value from the queue, but doesn't delete it.
        /// </summary>
        /// <param name="wait">How long to wait for data to arrive.</param>
        /// <returns>The queued item.</returns>
        public DataQueueEntry Peek(int wait)
        {
            return service.Read(nameEbcdic, libraryEbcdic, 0, wait, true, null);
        }
    }
}
