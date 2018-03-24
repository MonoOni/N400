﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace N400.Security
{
    // Abandon all hope, ye who enter here!
    // This is by far the worst code you'll ever see - hard rolled crypto
    // because IBM just *HAD* to implement DES in the strangest fucking way
    // possible. This is translated from the JavaScript reimplementation done
    // by Scott Guilbeaux.
    internal static class PasswordEncryptDES
    {
        static int EbcdicStringLength(byte[] s, int maxLength)
        {
            var index = Array.IndexOf<byte>(s, 0x40);
            return Math.Min(maxLength, index == -1 ? maxLength : index);
        }

        // assume preconversion to proper buffers
        public static byte[] EncryptPassword(byte[] userId, byte[] password, byte[] clientSeed, byte[] serverSeed)
        {
            var sequence = new byte[] { 0, 0, 0, 0, 0, 0, 0, 1};
            var verifyToken = new byte[8];
            var passwordToken = GeneratePasswordToken(userId, password);
            var encrypted = GeneratePasswordSubstitute(userId, passwordToken, verifyToken, sequence, clientSeed, serverSeed);
            return encrypted;
        }

        static byte[] GeneratePasswordToken(byte[] userId, byte[] password)
        {
            var token = new byte[8];
            var workBuffer1 = new byte[10];
            var workBuffer2 = new byte[] { 0x40, 0x40, 0x40, 0x40, 0x40, 0x40, 0x40, 0x40, 0x40, 0x40 };
            var workBuffer3 = new byte[] { 0x40, 0x40, 0x40, 0x40, 0x40, 0x40, 0x40, 0x40, 0x40, 0x40 };

            // Copy user ID into the work buffer.
            Array.Copy(userId, 0, workBuffer1, 0, 10);

            var len = EbcdicStringLength(userId, 10);

            if (len > 8)
            {
                // Fold user id
                workBuffer1[0] ^= (byte) ((workBuffer1[8] & 0xC0) );
                workBuffer1[1] ^= (byte) ((workBuffer1[8] & 0x30) << 2 );
                workBuffer1[2] ^= (byte) ((workBuffer1[8] & 0x0C) << 4 );
                workBuffer1[3] ^= (byte) ((workBuffer1[8] & 0x03) << 6 );
                workBuffer1[4] ^= (byte) ((workBuffer1[9] & 0xC0) );
                workBuffer1[5] ^= (byte) ((workBuffer1[9] & 0x30) << 2 );
                workBuffer1[6] ^= (byte) ((workBuffer1[9] & 0x0C) << 4 );
                workBuffer1[7] ^= (byte) ((workBuffer1[9] & 0x03) << 6 );
            }

            // Work with password
            len = EbcdicStringLength(password, 10);

            if (len > 8)
            {
                // copy the first 8 bytes of password to workBuffer2
                Array.Copy(password, 0, workBuffer2, 0, 8);

                // copy the remaining password to workBuffer3
                Array.Copy(password, 8, workBuffer3, 0, len - 8);

                // generate the token for the first 8 bytes of password
                Xor55AndLeftShift(workBuffer2);

                workBuffer2 =  // first token
                  Encrypt(workBuffer2,  // shifted result
                              workBuffer1);

                // generate the token for the second 8 bytes of password
                Xor55AndLeftShift(workBuffer3);

                workBuffer3 =  // second token
                  Encrypt(workBuffer3,  // shifted result
                              workBuffer1);  // userID

                // exclusive-or the first and second token to get the real token
                XorBuffer(workBuffer2, workBuffer3, token);
            }
            else
            {
                // copy password to work buffer
                Array.Copy(password, 0, workBuffer2, 0, len);

                // Generate token
                Xor55AndLeftShift(workBuffer2);

                token = Encrypt(workBuffer2, workBuffer1);
            }
            return token;
        }

        static byte[] GeneratePasswordSubstitute(byte[] usernameEBCID, byte[] token, byte[] verifyToken, byte[] sequence, byte[] clientSeed, byte[] serverSeed)
        {
            var RDrSEQ = new byte[8];
            var nextData = new byte[8];
            var nextEncryptedData = new byte[8];

            //first data or RDrSEQ = password sequence + host seed
            AddBuffer(sequence, serverSeed, RDrSEQ, 8);

            // first encrypted data = DES(token, first data)
            nextEncryptedData = Encrypt(token, RDrSEQ);

            // second data = first encrypted data ^ client seed
            XorBuffer(nextEncryptedData, clientSeed, nextData);

            // second encrypted data (password verifier) = DES(token, second data)
            nextEncryptedData = Encrypt(token, nextData);

            // let's copy second encrypted password to password verifier.
            // Don't know what it is yet but will ask Leonel.
            Array.Copy(nextEncryptedData, 0, verifyToken, 0, 8);

            // third data = RDrSEQ ^ first 8 bytes of userID
            XorBuffer(usernameEBCID, RDrSEQ, nextData);

            // third data ^= third data ^ second encrypted data
            XorBuffer(nextData, nextEncryptedData, nextData);

            // third encrypted data = DES(token, third data)
            nextEncryptedData = Encrypt(token, nextData);

            // leftJustify the second 8 bytes of user ID
            for (var i = 0; i < 8; i++)
            {
                nextData[i] = 0x40;
            }
            nextData[0] = usernameEBCID[8];
            nextData[1] = usernameEBCID[9];

            // fourth data = second half of userID ^ RDrSEQ;
            XorBuffer(RDrSEQ, nextData, nextData);

            // fourth data = third encrypted data ^ fourth data
            XorBuffer(nextData, nextEncryptedData, nextData);

            // fourth encrypted data = DES(token, fourth data)
            nextEncryptedData = Encrypt(token, nextData);

            // fifth data = fourth encrypted data ^ sequence number
            XorBuffer(sequence, nextEncryptedData, nextData);

            // fifth encrypted data = DES(token, fifth data) this is the encrypted password
            return Encrypt(token, nextData);
        }

        static void AddBuffer(byte[] buff1, byte[] buff2, byte[] result, int len)
        {
            var carryBit = 0;
            for (var i = len - 1; i >= 0; i--)
            {
                var temp = (buff1[i] & 0xff) + (buff2[i] & 0xff) + carryBit;
                carryBit = (int)((uint)temp >> 8);
                result[i] = (byte) temp;
            }
        }

        static void XorBuffer(byte[] buff1, byte[] buff2, byte[] buff3)
        {
            for (var i = 0; i < 8; i++)
            {
                buff3[i] = (byte) (buff1[i] ^ buff2[i]);
            }
        }
        
        static void Xor55AndLeftShift(byte[] buff)
        {
            buff[0] ^= 0x55;
            buff[1] ^= 0x55;
            buff[2] ^= 0x55;
            buff[3] ^= 0x55;
            buff[4] ^= 0x55;
            buff[5] ^= 0x55;
            buff[6] ^= 0x55;
            buff[7] ^= 0x55;
            
            buff[0] = (byte) (buff[0] << 1 | (byte) ((buff[1] & 0x80) >> 7) );
            buff[1] = (byte) (buff[1] << 1 | (byte) ((buff[2] & 0x80) >> 7) );
            buff[2] = (byte) (buff[2] << 1 | (byte) ((buff[3] & 0x80) >> 7) );
            buff[3] = (byte) (buff[3] << 1 | (byte) ((buff[4] & 0x80) >> 7) );
            buff[4] = (byte) (buff[4] << 1 | (byte) ((buff[5] & 0x80) >> 7) );
            buff[5] = (byte) (buff[5] << 1 | (byte) ((buff[6] & 0x80) >> 7) );
            buff[6] = (byte) (buff[6] << 1 | (byte) ((buff[7] & 0x80) >> 7) );
            buff[7] <<= 1;
        }

        static byte[] Encrypt(byte[] key, byte[] data)
        {
            var e1 = new byte[65];
            var e2 = new byte[65];

            // expand the input string to 1 bit per byte again for the key
            for (var i = 0; i < 8; ++i)
            {
                e1[8 * i + 1] = (byte)(((data[i] & 0x80) == 0) ? 0x30 : 0x31);
                e1[8 * i + 2] = (byte)(((data[i] & 0x40) == 0) ? 0x30 : 0x31);
                e1[8 * i + 3] = (byte)(((data[i] & 0x20) == 0) ? 0x30 : 0x31);
                e1[8 * i + 4] = (byte)(((data[i] & 0x10) == 0) ? 0x30 : 0x31);
                e1[8 * i + 5] = (byte)(((data[i] & 0x08) == 0) ? 0x30 : 0x31);
                e1[8 * i + 6] = (byte)(((data[i] & 0x04) == 0) ? 0x30 : 0x31);
                e1[8 * i + 7] = (byte)(((data[i] & 0x02) == 0) ? 0x30 : 0x31);
                e1[8 * i + 8] = (byte)(((data[i] & 0x01) == 0) ? 0x30 : 0x31);
            }

            for (var i = 0; i < 8; ++i)
            {
                e2[8 * i + 1] = (byte)(((key[i] & 0x80) == 0) ? 0x30 : 0x31);
                e2[8 * i + 2] = (byte)(((key[i] & 0x40) == 0) ? 0x30 : 0x31);
                e2[8 * i + 3] = (byte)(((key[i] & 0x20) == 0) ? 0x30 : 0x31);
                e2[8 * i + 4] = (byte)(((key[i] & 0x10) == 0) ? 0x30 : 0x31);
                e2[8 * i + 5] = (byte)(((key[i] & 0x08) == 0) ? 0x30 : 0x31);
                e2[8 * i + 6] = (byte)(((key[i] & 0x04) == 0) ? 0x30 : 0x31);
                e2[8 * i + 7] = (byte)(((key[i] & 0x02) == 0) ? 0x30 : 0x31);
                e2[8 * i + 8] = (byte)(((key[i] & 0x01) == 0) ? 0x30 : 0x31);
            }

            // encryption method
            var preout = new byte[65];  // preoutput block

            // temp key gen workspace
            var Cn = new byte[58];
            // create Cn from the original key
            for (var n = 1; n <= 56; n++)
            {
                Cn[n] = e2[PC1[n - 1]];
            }

            // rotate Cn to form C1 (still called Cn...)
            Lshift1(Cn);

            var key1 = new byte[49]; // 48 bit key 1 to key 16
            // now Cn[] contains 56 bits for input to PC2 to generate key1
            for (var n = 1; n <= 48; n++)
            {
                key1[n] = Cn[PC2[n - 1]];
            }

            var key2 = new byte[49];
            // now derive C2 from C1 (which is called Cn)
            Lshift1(Cn);
            for (var n = 1; n <= 48; n++)
            {
                key2[n] = Cn[PC2[n - 1]];
            }

            var key3 = new byte[49];
            // now derive C3 from C2 by left shifting twice
            Lshift2(Cn);
            for (var n = 1; n <= 48; n++)
            {
                key3[n] = Cn[PC2[n - 1]];
            }

            var key4 = new byte[49];
            // now derive C4 from C3 by again left shifting twice
            Lshift2(Cn);
            for (var n = 1; n <= 48; n++)
            {
                key4[n] = Cn[PC2[n - 1]];
            }

            var key5 = new byte[49];
            // now derive C5 from C4 by again left shifting twice
            Lshift2(Cn);
            for (var n = 1; n <= 48; n++)
            {
                key5[n] = Cn[PC2[n - 1]];
            }

            var key6 = new byte[49];
            // now derive C6 from C5 by again left shifting twice
            Lshift2(Cn);
            for (var n = 1; n <= 48; n++)
            {
                key6[n] = Cn[PC2[n - 1]];
            }

            var key7 = new byte[49];
            // now derive C7 from C6 by again left shifting twice
            Lshift2(Cn);
            for (var n = 1; n <= 48; n++)
            {
                key7[n] = Cn[PC2[n - 1]];
            }

            var key8 = new byte[49];
            // now derive C8 from C7 by again left shifting twice
            Lshift2(Cn);
            for (var n = 1; n <= 48; n++)
            {
                key8[n] = Cn[PC2[n - 1]];
            }

            var key9 = new byte[49];
            // now derive C9 from C8 by shifting left once
            Lshift1(Cn);
            for (var n = 1; n <= 48; n++)
            {
                key9[n] = Cn[PC2[n - 1]];
            }

            var key10 = new byte[49];
            // now derive C10 from C9 by again left shifting twice
            Lshift2(Cn);
            for (var n = 1; n <= 48; n++)
            {
                key10[n] = Cn[PC2[n - 1]];
            }

            var key11 = new byte[49];
            // now derive C11 from C10 by again left shifting twice
            Lshift2(Cn);
            for (var n = 1; n <= 48; n++)
            {
                key11[n] = Cn[PC2[n - 1]];
            }

            var key12 = new byte[49];
            // now derive C12 from C11 by again left shifting twice
            Lshift2(Cn);
            for (var n = 1; n <= 48; n++)
            {
                key12[n] = Cn[PC2[n - 1]];
            }

            var key13 = new byte[49];
            // now derive C13 from C12 by again left shifting twice
            Lshift2(Cn);
            for (var n = 1; n <= 48; n++)
            {
                key13[n] = Cn[PC2[n - 1]];
            }

            var key14 = new byte[49];
            // now derive C14 from C13 by again left shifting twice
            Lshift2(Cn);
            for (var n = 1; n <= 48; n++)
            {
                key14[n] = Cn[PC2[n - 1]];
            }

            var key15 = new byte[49];
            // now derive C15 from C14 by again left shifting twice
            Lshift2(Cn);
            for (var n = 1; n <= 48; n++)
            {
                key15[n] = Cn[PC2[n - 1]];
            }

            var key16 = new byte[49];
            // now derive C16 from C15 by again left shifting once
            Lshift1(Cn);
            for (var n = 1; n <= 48; n++)
            {
                key16[n] = Cn[PC2[n - 1]];
            }

            // temp encryption workspace
            var Ln = new byte[33];
            // ditto
            var Rn = new byte[33];

            // perform the initial permutation and store the result in Ln and Rn
            for (var n = 1; n <= 32; n++)
            {
                Ln[n] = e1[INITPERM[n - 1]];
                Rn[n] = e1[INITPERM[n + 31]];
            }

            // run cipher to get new Ln and Rn
            Cipher(key1, Ln, Rn);
            Cipher(key2, Ln, Rn);
            Cipher(key3, Ln, Rn);
            Cipher(key4, Ln, Rn);
            Cipher(key5, Ln, Rn);
            Cipher(key6, Ln, Rn);
            Cipher(key7, Ln, Rn);
            Cipher(key8, Ln, Rn);
            Cipher(key9, Ln, Rn);
            Cipher(key10, Ln, Rn);
            Cipher(key11, Ln, Rn);
            Cipher(key12, Ln, Rn);
            Cipher(key13, Ln, Rn);
            Cipher(key14, Ln, Rn);
            Cipher(key15, Ln, Rn);
            Cipher(key16, Ln, Rn);

            // Ln and Rn are now at L16 and R16 - create preout[] by interposing them
            Array.Copy(Rn, 1, preout, 1, 32);
            Array.Copy(Ln, 1, preout, 33, 32);

            var e3 = new byte[65];
            // run preout[] through outperm to get ciphertext
            for (var n = 1; n <= 64; n++)
            {
                e3[n] = preout[OUTPERM[n - 1]];
            }

            var encData = new byte[8];
            // compress back to 8 bits per byte
            for (var i = 0; i < 8; ++i)
            {
                if (e3[8 * i + 1] == 0x31)
                {
                    encData[i] |= 0x80;
                }
                if (e3[8 * i + 2] == 0x31)
                {
                    encData[i] |= 0x40;
                }
                if (e3[8 * i + 3] == 0x31)
                {
                    encData[i] |= 0x20;
                }
                if (e3[8 * i + 4] == 0x31)
                {
                    encData[i] |= 0x10;
                }
                if (e3[8 * i + 5] == 0x31)
                {
                    encData[i] |= 0x08;
                }
                if (e3[8 * i + 6] == 0x31)
                {
                    encData[i] |= 0x04;
                }
                if (e3[8 * i + 7] == 0x31)
                {
                    encData[i] |= 0x02;
                }
                if (e3[8 * i + 8] == 0x31)
                {
                    encData[i] |= 0x01;
                }
            }

            return encData;
        }

        static void Cipher(byte[] key, byte[] Ln, byte[] Rn)
        {
            var temp1 = new byte[49];  // Rn run through E
            var temp2 = new byte[49];  // temp1 XORed with key
            var temp3 = new byte[33];  // temp2 run through S boxes
            var fkn = new byte[33];  //  f(k,n)
            var si = new int[9];  // decimal input to S boxes
            var so = new int[9];  // decimal output from S boxes

            // generate temp1[] from Rn[]
            for (var n = 1; n <= 48; n++)
            {
                temp1[n] = Rn[EPERM[n - 1]];
            }

            // XOR temp1 with key to get temp2
            for (var n = 1; n <= 48; n++)
            {
                temp2[n] = (byte)((temp1[n] != key[n]) ? 0x31 : 0x30);
            }

            // we need to get the explicit representation into a form for
            // processing the s boxes...
            si[1] = ((temp2[1] == 0x31) ? 0x0020 : 0x0000) |
              ((temp2[6] == 0x31) ? 0x0010 : 0x0000) |
              ((temp2[2] == 0x31) ? 0x0008 : 0x0000) |
              ((temp2[3] == 0x31) ? 0x0004 : 0x0000) |
              ((temp2[4] == 0x31) ? 0x0002 : 0x0000) |
              ((temp2[5] == 0x31) ? 0x0001 : 0x0000);

            si[2] = ((temp2[7] == 0x31) ? 0x0020 : 0x0000) |
              ((temp2[12] == 0x31) ? 0x0010 : 0x0000) |
              ((temp2[8] == 0x31) ? 0x0008 : 0x0000) |
              ((temp2[9] == 0x31) ? 0x0004 : 0x0000) |
              ((temp2[10] == 0x31) ? 0x0002 : 0x0000) |
              ((temp2[11] == 0x31) ? 0x0001 : 0x0000);

            si[3] = ((temp2[13] == 0x31) ? 0x0020 : 0x0000) |
              ((temp2[18] == 0x31) ? 0x0010 : 0x0000) |
              ((temp2[14] == 0x31) ? 0x0008 : 0x0000) |
              ((temp2[15] == 0x31) ? 0x0004 : 0x0000) |
              ((temp2[16] == 0x31) ? 0x0002 : 0x0000) |
              ((temp2[17] == 0x31) ? 0x0001 : 0x0000);

            si[4] = ((temp2[19] == 0x31) ? 0x0020 : 0x0000) |
              ((temp2[24] == 0x31) ? 0x0010 : 0x0000) |
              ((temp2[20] == 0x31) ? 0x0008 : 0x0000) |
              ((temp2[21] == 0x31) ? 0x0004 : 0x0000) |
              ((temp2[22] == 0x31) ? 0x0002 : 0x0000) |
              ((temp2[23] == 0x31) ? 0x0001 : 0x0000);

            si[5] = ((temp2[25] == 0x31) ? 0x0020 : 0x0000) |
              ((temp2[30] == 0x31) ? 0x0010 : 0x0000) |
              ((temp2[26] == 0x31) ? 0x0008 : 0x0000) |
              ((temp2[27] == 0x31) ? 0x0004 : 0x0000) |
              ((temp2[28] == 0x31) ? 0x0002 : 0x0000) |
              ((temp2[29] == 0x31) ? 0x0001 : 0x0000);

            si[6] = ((temp2[31] == 0x31) ? 0x0020 : 0x0000) |
              ((temp2[36] == 0x31) ? 0x0010 : 0x0000) |
              ((temp2[32] == 0x31) ? 0x0008 : 0x0000) |
              ((temp2[33] == 0x31) ? 0x0004 : 0x0000) |
              ((temp2[34] == 0x31) ? 0x0002 : 0x0000) |
              ((temp2[35] == 0x31) ? 0x0001 : 0x0000);

            si[7] = ((temp2[37] == 0x31) ? 0x0020 : 0x0000) |
              ((temp2[42] == 0x31) ? 0x0010 : 0x0000) |
              ((temp2[38] == 0x31) ? 0x0008 : 0x0000) |
              ((temp2[39] == 0x31) ? 0x0004 : 0x0000) |
              ((temp2[40] == 0x31) ? 0x0002 : 0x0000) |
              ((temp2[41] == 0x31) ? 0x0001 : 0x0000);

            si[8] = ((temp2[43] == 0x31) ? 0x0020 : 0x0000) |
              ((temp2[48] == 0x31) ? 0x0010 : 0x0000) |
              ((temp2[44] == 0x31) ? 0x0008 : 0x0000) |
              ((temp2[45] == 0x31) ? 0x0004 : 0x0000) |
              ((temp2[46] == 0x31) ? 0x0002 : 0x0000) |
              ((temp2[47] == 0x31) ? 0x0001 : 0x0000);

            // Now for the S boxes
            so[1] = S1[si[1]];
            so[2] = S2[si[2]];
            so[3] = S3[si[3]];
            so[4] = S4[si[4]];
            so[5] = S5[si[5]];
            so[6] = S6[si[6]];
            so[7] = S7[si[7]];
            so[8] = S8[si[8]];

            // That wasn't too bad.  Now to convert decimal to char hex again so[1-8] must be translated to 32 bits and stored in temp3[1-32]
            DecToBin(so[1], temp3, 1);
            DecToBin(so[2], temp3, 5);
            DecToBin(so[3], temp3, 9);
            DecToBin(so[4], temp3, 13);
            DecToBin(so[5], temp3, 17);
            DecToBin(so[6], temp3, 21);
            DecToBin(so[7], temp3, 25);
            DecToBin(so[8], temp3, 29);

            // Okay. Now temp3[] contains the data to run through P
            for (var n = 1; n <= 32; n++)
            {
                fkn[n] = temp3[PPERM[n - 1]];
            }

            // now complete the cipher function to update Ln and Rn
            var temp = new byte[33];  // storage for Ln during cipher function

            Array.Copy(Rn, 1, temp, 1, 32);
            for (int n = 1; n <= 32; n++)
            {
                Rn[n] = (Ln[n] == fkn[n]) ? (byte)0x30 : (byte)0x31;
            }
            Array.Copy(temp, 1, Ln, 1, 32);
        }

        static void Lshift1(byte[] Cn)
        {
            var hold = new byte[2];

            // get the two rotated bits
            hold[0] = Cn[1];
            hold[1] = Cn[29];

            // shift each position left in two 28 bit groups corresponding to Cn and Dn
            Array.Copy(Cn, 2, Cn, 1, 27);
            Array.Copy(Cn, 30, Cn, 29, 27);

            // restore the first bit of each subgroup
            Cn[28] = hold[0];
            Cn[56] = hold[1];
        }

        static void Lshift2(byte[] Cn)
        {
            var hold = new byte[4];

            hold[0] = Cn[1];  // get the four rotated bits
            hold[1] = Cn[2];
            hold[2] = Cn[29];
            hold[3] = Cn[30];

            // shift each position left in two 28 bit groups corresponding to Cn and Dn
            Array.Copy(Cn, 3, Cn, 1, 27);
            Array.Copy(Cn, 31, Cn, 29, 27);

            // restore the first bit of each subgroup
            Cn[27] = hold[0];
            Cn[28] = hold[1];
            Cn[55] = hold[2];
            Cn[56] = hold[3];
        }

        static void DecToBin(int value, byte[] buf, int offset)
        {
            buf[offset] = (byte)(((value & 0x0008) != 0) ? 0x31 : 0x30);
            buf[offset + 1] = (byte)(((value & 0x0004) != 0) ? 0x31 : 0x30);
            buf[offset + 2] = (byte)(((value & 0x0002) != 0) ? 0x31 : 0x30);
            buf[offset + 3] = (byte)(((value & 0x0001) != 0) ? 0x31 : 0x30);
        }

        // Permuted Choice 1
        static byte[] PC1 = new byte[]
        {
            // get the 56 bits which make up C0 and D0 (combined into Cn) from the original key
            57, 49, 41, 33, 25, 17,  9,
             1, 58, 50, 42, 34, 26, 18,
            10,  2, 59, 51, 43, 35, 27,
            19, 11,  3, 60, 52, 44, 36,
            63, 55, 47, 39, 31, 23, 15,
             7, 62, 54, 46, 38, 30, 22,
            14,  6, 61, 53, 45, 37, 29,
            21, 13,  5, 28, 20, 12,  4
        };

        // Permuted Choice 2
        static byte[] PC2 = new byte[]
        {
            // used in generation of the 16 subkeys
            14, 17, 11, 24,  1,  5,
             3, 28, 15,  6, 21, 10,
            23, 19, 12,  4, 26,  8,
            16,  7, 27, 20, 13,  2,
            41, 52, 31, 37, 47, 55,
            30, 40, 51, 45, 33, 48,
            44, 49, 39, 56, 34, 53,
            46, 42, 50, 36, 29, 32
        };

        // the initial scrambling of the input data
        static byte[] INITPERM = new byte[]
        {
            58, 50, 42, 34, 26, 18, 10, 2,
            60, 52, 44, 36, 28, 20, 12, 4,
            62, 54, 46, 38, 30, 22, 14, 6,
            64, 56, 48, 40, 32, 24, 16, 8,
            57, 49, 41, 33, 25, 17,  9, 1,
            59, 51, 43, 35, 27, 19, 11, 3,
            61, 53, 45, 37, 29, 21, 13, 5,
            63, 55, 47, 39, 31, 23, 15, 7
        };

        // the E function used in the cipher function
        static byte[] EPERM = new byte[]
        {
            32,  1,  2,  3,  4,  5,
            4,   5,  6,  7,  8,  9,
            8,   9, 10, 11, 12, 13,
            12, 13, 14, 15, 16, 17,
            16, 17, 18, 19, 20, 21,
            20, 21, 22, 23, 24, 25,
            24, 25, 26, 27, 28, 29,
            28, 29, 30, 31, 32, 1
        };

        // the inverse permutation of initperm - used on the proutput block
        static byte[] OUTPERM = new byte[]
        {
            40, 8, 48, 16, 56, 24, 64, 32,
            39, 7, 47, 15, 55, 23, 63, 31,
            38, 6, 46, 14, 54, 22, 62, 30,
            37, 5, 45, 13, 53, 21, 61, 29,
            36, 4, 44, 12, 52, 20, 60, 28,
            35, 3, 43, 11, 51, 19, 59, 27,
            34, 2, 42, 10, 50, 18, 58, 26,
            33, 1, 41,  9, 49, 17, 57, 25
        };

        // the P function used in cipher function
        static byte[] PPERM = new byte[]
        {
            16,  7, 20, 21,
            29, 12, 28, 17,
            1,  15, 23, 26,
            5,  18, 31, 10,
            2,   8, 24, 14,
            32, 27,  3,  9,
            19, 13, 30,  6,
            22, 11,  4, 25
        };

        static byte[] S1 = new byte[]
        {
            14, 4, 13, 1, 2, 15, 11, 8, 3, 10, 6, 12, 5, 9, 0, 7,
            0, 15, 7, 4, 14, 2, 13, 1, 10, 6, 12, 11, 9, 5, 3, 8,
            4, 1, 14, 8, 13, 6, 2, 11, 15, 12, 9, 7, 3, 10, 5, 0,
            15, 12, 8, 2, 4, 9, 1, 7, 5, 11, 3, 14, 10, 0, 6, 13
        };

        static byte[] S2 = new byte[]
        {
            15, 1, 8, 14, 6, 11, 3, 4, 9, 7, 2, 13, 12, 0, 5, 10,
            3, 13, 4, 7, 15, 2, 8, 14, 12, 0, 1, 10, 6, 9, 11, 5,
            0, 14, 7, 11, 10, 4, 13, 1, 5, 8, 12, 6, 9, 3, 2, 15,
            13, 8, 10, 1, 3, 15, 4, 2, 11, 6, 7, 12, 0, 5, 14, 9
        };

        static byte[] S3 = new byte[]
        {
            10, 0, 9, 14, 6, 3, 15, 5, 1, 13, 12, 7, 11, 4, 2, 8,
            13, 7, 0, 9, 3, 4, 6, 10, 2, 8, 5, 14, 12, 11, 15, 1,
            13, 6, 4, 9, 8, 15, 3, 0, 11, 1, 2, 12, 5, 10, 14, 7,
            1, 10, 13, 0, 6, 9, 8, 7, 4, 15, 14, 3, 11, 5, 2, 12
        };

        static byte[] S4 = new byte[]
        {
            7, 13, 14, 3, 0, 6, 9, 10, 1, 2, 8, 5, 11, 12, 4, 15,
            13, 8, 11, 5, 6, 15, 0, 3, 4, 7, 2, 12, 1, 10, 14, 9,
            10, 6, 9, 0, 12, 11, 7, 13, 15, 1, 3, 14, 5, 2, 8, 4,
            3, 15, 0, 6, 10, 1, 13, 8, 9, 4, 5, 11, 12, 7, 2, 14
        };

        static byte[] S5 = new byte[]
        {
            2, 12, 4, 1, 7, 10, 11, 6, 8, 5, 3, 15, 13, 0, 14, 9,
            14, 11, 2, 12, 4, 7, 13, 1, 5, 0, 15, 10, 3, 9, 8, 6,
            4, 2, 1, 11, 10, 13, 7, 8, 15, 9, 12, 5, 6, 3, 0, 14,
            11, 8, 12, 7, 1, 14, 2, 13, 6, 15, 0, 9, 10, 4, 5, 3
        };

        static byte[] S6 = new byte[]
        {
            12, 1, 10, 15, 9, 2, 6, 8, 0, 13, 3, 4, 14, 7, 5, 11,
            10, 15, 4, 2, 7, 12, 9, 5, 6, 1, 13, 14, 0, 11, 3, 8,
            9, 14, 15, 5, 2, 8, 12, 3, 7, 0, 4, 10, 1, 13, 11, 6,
            4, 3, 2, 12, 9, 5, 15, 10, 11, 14, 1, 7, 6, 0, 8, 13
        };

        static byte[] S7 = new byte[]
        {
            4, 11, 2, 14, 15, 0, 8, 13, 3, 12, 9, 7, 5, 10, 6, 1,
            13, 0, 11, 7, 4, 9, 1, 10, 14, 3, 5, 12, 2, 15, 8, 6,
            1, 4, 11, 13, 12, 3, 7, 14, 10, 15, 6, 8, 0, 5, 9, 2,
            6, 11, 13, 8, 1, 4, 10, 7, 9, 5, 0, 15, 14, 2, 3, 12
        };

        static byte[] S8 = new byte[]
        {
            13, 2, 8, 4, 6, 15, 11, 1, 10, 9, 3, 14, 5, 0, 12, 7,
            1, 15, 13, 8, 10, 3, 7, 4, 12, 5, 6, 11, 0, 14, 9, 2,
            7, 11, 4, 1, 9, 12, 14, 2, 0, 6, 10, 13, 15, 3, 5, 8,
            2, 1, 14, 7, 4, 10, 8, 13, 15, 12, 9, 0, 3, 5, 6, 11
        };
    }
}
