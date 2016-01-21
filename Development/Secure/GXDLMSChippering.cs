﻿//
// --------------------------------------------------------------------------
//  Gurux Ltd
// 
//
//
// Filename:        $HeadURL$
//
// Version:         $Revision$,
//                  $Date$
//                  $Author$
//
// Copyright (c) Gurux Ltd
//
//---------------------------------------------------------------------------
//
//  DESCRIPTION
//
// This file is a part of Gurux Device Framework.
//
// Gurux Device Framework is Open Source software; you can redistribute it
// and/or modify it under the terms of the GNU General Public License 
// as published by the Free Software Foundation; version 2 of the License.
// Gurux Device Framework is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
// See the GNU General Public License for more details.
//
// More information of Gurux products: http://www.gurux.org
//
// This code is licensed under the GNU General Public License v2. 
// Full text may be retrieved at http://www.gnu.org/licenses/gpl-2.0.txt
//---------------------------------------------------------------------------


using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Gurux.DLMS.Internal;
using Gurux.DLMS.Enums;


namespace Gurux.DLMS.Secure
{        
    class GXDLMSChippering
    {        
        /// <summary>
        /// Get Nonse from frame counter and system title.
        /// </summary>
        /// <param name="FrameCounter">Frame counter.</param>
        /// <param name="systemTitle">System title.</param>
        /// <returns></returns>
        static byte[] GetNonse(UInt32 FrameCounter, byte[] systemTitle)
        {
            byte[] nonce = new byte[12];
            systemTitle.CopyTo(nonce, 0);
            byte[] tmp = BitConverter.GetBytes(FrameCounter).Reverse().ToArray();
            tmp.CopyTo(nonce, 8);
            return nonce;
        }

        static public byte[] EncryptAesGcm(Command cmd, Security security, UInt32 frameCounter, byte[] systemTitle,
            byte[] blockCipherKey, byte[] authenticationKey, byte[] plainText)
        {
            switch (cmd)
            {
                case Command.ReadRequest:
                case Command.GetRequest:
                    cmd = Command.GloGetRequest;
                    break;
                case Command.WriteRequest:
                case Command.SetRequest:
                    cmd = Command.GloSetRequest;
                    break;
                case Command.MethodRequest:
                    cmd = Command.GloMethodRequest;
                    break;
                case Command.ReadResponse:
                case Command.GetResponse:
                    cmd = Command.GloGetResponse;
                    break;
                case Command.WriteResponse:
                case Command.SetResponse:
                    cmd = Command.GloSetResponse;
                    break;
                case Command.MethodResponse:
                    cmd = Command.MethodResponse;
                    break;
                default:
                    throw new GXDLMSException("Invalid GLO command.");
            }
            AesGcmParameter p = new AesGcmParameter(cmd, security, frameCounter,
                    systemTitle, blockCipherKey, authenticationKey, plainText);
            return EncryptAesGcm(p);
        }

        static private byte[] EncryptAesGcm(AesGcmParameter param)
        {
            param.CountTag = null;
            GXByteBuffer data = new GXByteBuffer();
            if (param.Type == CountType.Packet)
            {
                data.SetUInt8((byte)param.Security);
            }
            byte[] tmp = BitConverter.GetBytes(param.FrameCounter).Reverse().ToArray();
            byte[] aad = GetAuthenticatedData(param.Security, param.AuthenticationKey, param.PlainText);
            GXDLMSChipperingStream gcm = new GXDLMSChipperingStream(param.Security, true, param.BlockCipherKey,
                aad, GetNonse(param.FrameCounter, param.SystemTitle), null);
            // Encrypt the secret message
            if (param.Security != Security.Authentication)
            {
                gcm.Write(param.PlainText);
            }
            byte[] ciphertext = gcm.FlushFinalBlock();
            if (param.Security == Security.Authentication)
            {
                if (param.Type == CountType.Packet)
                {
                    data.Set(tmp);
                }
                if ((param.Type & CountType.Data) != 0)
                {
                    data.Set(param.PlainText);
                }
                if ((param.Type & CountType.Tag) != 0)
                {
                    param.CountTag = gcm.GetTag();
                    data.Set(param.CountTag);                    
                }
            }
            else if (param.Security == Security.Encryption)
            {
                data.Set(tmp);
                data.Set(ciphertext);
            }
            else if (param.Security == Security.AuthenticationEncryption)
            {
                if (param.Type == CountType.Packet)
                {
                    data.Set(tmp);
                }
                if ((param.Type & CountType.Data) != 0)
                {
                    data.Set(ciphertext);
                }
                if ((param.Type & CountType.Tag) != 0)
                {
                    param.CountTag = gcm.GetTag();
                    data.Set(param.CountTag);
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException("security");
            }
            if (param.Type == CountType.Packet)
            {
                GXByteBuffer tmp2 = new GXByteBuffer((ushort) (10 + data.Size));
                tmp2.SetUInt8((byte) param.Command);
                GXCommon.SetObjectCount(data.Size, tmp2);
                tmp2.Set(data.Array());
                return tmp2.Array();
            }
            return data.Array();        
        }

        private static byte[] GetAuthenticatedData(Security security, byte[] AuthenticationKey, byte[] plainText)
        {
            if (security == Security.Authentication)
            {
                GXByteBuffer tmp2 = new GXByteBuffer();
                tmp2.SetUInt8((byte)security);
                tmp2.Set(AuthenticationKey);
                tmp2.Set(plainText);
                return tmp2.Array();
            }
            else if (security == Security.Encryption)
            {
                return AuthenticationKey;
            }
            else if (security == Security.AuthenticationEncryption)
            {
                GXByteBuffer tmp2 = new GXByteBuffer();
                tmp2.SetUInt8((byte)security);
                tmp2.Set(AuthenticationKey);
                return tmp2.Array();
            }
            return null;
        }

        /// <summary>
        /// Decrypt data.
        /// </summary>
        /// <param name="cryptedText">Crypted data.</param>
        /// <param name="systemTitle"></param>
        /// <param name="BlockCipherKey"></param>
        /// <param name="AuthenticationKey"></param>
        /// <returns></returns>
        public static byte[] DecryptAesGcm(GXByteBuffer cryptedText, byte[] systemTitle, byte[] blockCipherKey, byte[] authenticationKey)
        {
            if (cryptedText == null || cryptedText.Size < 2)
            {
                throw new ArgumentOutOfRangeException("cryptedData");
            }
            Command cmd = (Command) cryptedText.GetUInt8();
            if (!(cmd == Command.GloGetRequest ||
                cmd == Command.GloGetResponse ||
                cmd == Command.GloSetRequest ||
                cmd == Command.GloSetResponse ||
                cmd == Command.GloMethodRequest ||
                cmd == Command.GloMethodResponse))
            {
                throw new ArgumentOutOfRangeException("cryptedData");
            }
            int len = Gurux.DLMS.Internal.GXCommon.GetObjectCount(cryptedText);
            Security security = (Security)cryptedText.GetUInt8();
            UInt32 frameCounter = cryptedText.GetUInt32();
            byte[] tag = new byte[12];
            byte[] encryptedData;
            int length;
            if (security == Security.Authentication)
            {
                length = cryptedText.Size - cryptedText.Position - 12;
                encryptedData = new byte[length];
                cryptedText.Get(encryptedData);
                cryptedText.Get(tag);
                // Check tag.
                AesGcmParameter p = new AesGcmParameter(Command.None, security,
                        frameCounter, systemTitle, blockCipherKey,
                        authenticationKey, encryptedData);
                EncryptAesGcm(p);
                if (!GXDLMSChipperingStream.TagsEquals(tag, p.CountTag))
                {
                    throw new GXDLMSException("Decrypt failed. Invalid tag.");
                }
                return encryptedData;
            }
            byte[] ciphertext = null;
            if (security == Security.Encryption)
            {
                length = cryptedText.Size - cryptedText.Position;
                ciphertext = new byte[length];
                cryptedText.Get(ciphertext);
            }
            else if (security == Security.AuthenticationEncryption)
            {
                length = cryptedText.Size - cryptedText.Position - 12;
                ciphertext = new byte[length];
                cryptedText.Get(ciphertext);
                cryptedText.Get(tag);
            }            
            byte[] aad = GetAuthenticatedData(security, authenticationKey, cryptedText.Array());
            GXDLMSChipperingStream gcm = new GXDLMSChipperingStream(security, false, blockCipherKey, aad, GetNonse(frameCounter, systemTitle), tag);
            gcm.Write(ciphertext);
            return gcm.FlushFinalBlock();            
        }
    }
}
