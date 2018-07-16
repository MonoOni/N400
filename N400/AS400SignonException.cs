using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace N400
{
    /// <summary>
    /// Represents an error during the signon process.
    /// </summary>
    public class AS400SignonException : Exception
    {
        /// <summary>
        /// The error ID returned by the signon process.
        /// </summary>
        public uint ReturnCode { get; }

        internal Packets.SignonReturnCodeResponseBase ReturnedPacket { get; }

        internal static string GetMessageForReturnCode(uint rc)
        {
            // The high 16 bits indicate what "class" of error this return code
            // represents. They are, as follows:
            //   * 0001xxxx: request error
            //   * 0002xxxx: user ID error
            //   * 0003xxxx: password error
            //   * 0004xxxx: general security error
            //   * 0005xxxx: exit program error
            //   * 0006xxxx: Kerberos error
            //   * 0007xxxx: token error
            // Of these, so far, only 1, 2, and 3 really matter to us.
            switch (rc)
            {
                case 0:
                    throw new ArgumentException("The return code represents success, but an exception is being constructed anyways?", nameof(rc));
                case 0x00020001:
                    return "The user is unknown to the server.";
                case 0x00020002:
                    return "The user has been disabled.";
                case 0x0003000B:
                    return "The password is incorrect.";
                case 0x0003000C:
                    return "The password is incorrect, and the next invalid attempt will disable the user.";
                case 0x0003000D:
                    return "The password is correct, but has expired.";
                case 0x0003000E:
                    return "The password is encrypted by a version of OS/400 before V2R2.";
                case 0x00030010:
                    return "The user's password is \"*NONE\".";
                default:
                    return string.Format("An unknown error occured during signon: {0:X}", rc);
            }
        }

        internal AS400SignonException(Packets.SignonReturnCodeResponseBase srcr)
            : base(GetMessageForReturnCode(srcr.ReturnCode))
        {
            ReturnCode = srcr.ReturnCode;
            ReturnedPacket = srcr;
        }
    }
}
