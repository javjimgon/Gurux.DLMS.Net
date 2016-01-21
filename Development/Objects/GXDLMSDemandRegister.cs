//
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
using System.Text;
using System.ComponentModel;
using Gurux.DLMS;
using System.Xml.Serialization;
using Gurux.DLMS.ManufacturerSettings;
using Gurux.DLMS.Internal;
using Gurux.DLMS.Enums;

namespace Gurux.DLMS.Objects
{
    public class GXDLMSDemandRegister : GXDLMSObject, IGXDLMSBase
    {
        protected int _scaler;

        /// <summary> 
        /// Constructor.
        /// </summary> 
        public GXDLMSDemandRegister()
            : base(ObjectType.DemandRegister)
        {
        }        

        /// <summary> 
        /// Constructor.
        /// </summary> 
        /// <param name="ln">Logical Name of the object.</param>
        public GXDLMSDemandRegister(string ln)
            : base(ObjectType.DemandRegister, ln, 0)
        {
        }

        /// <summary> 
        /// Constructor.
        /// </summary> 
        /// <param name="ln">Logical Name of the object.</param>
        /// <param name="sn">Short Name of the object.</param>
        public GXDLMSDemandRegister(string ln, ushort sn)
            : base(ObjectType.DemandRegister, ln, sn)
        {
        }

        /// <summary>
        /// Current average value of COSEM Data object.
        /// </summary>
        [XmlIgnore()]
        public object CurrentAverageValue
        {
            get;
            set;
        }

        /// <summary>
        /// Last average value of COSEM Data object.
        /// </summary>
        [XmlIgnore()]
        public object LastAverageValue
        {
            get;
            set;
        }

        /// <summary>
        /// Scaler of COSEM Register object.
        /// </summary>        
        [DefaultValue(1.0)]
        public double Scaler
        {
            get
            {
                return Math.Pow(10, _scaler);
            }
            set
            {
                _scaler = (int)Math.Log10(value);
            }
        }

        /// <summary>
        /// Unit of COSEM Register object.
        /// </summary>
        [DefaultValue(Unit.NoUnit)]
        public Unit Unit
        {
            get;
            set;
        }

        /// <summary>
        /// Provides Demand register specific status information.
        /// </summary>
        [XmlIgnore()]
        public Object Status
        {
            get;
            set;
        }

        /// <summary>
        /// Capture time of COSEM Register object.
        /// </summary>
        [XmlIgnore()]
        public GXDateTime CaptureTime
        {
            get;
            set;
        }        

        /// <summary>
        /// Current start time of COSEM Register object.
        /// </summary>
        [XmlIgnore()]
        public GXDateTime StartTimeCurrent
        {
            get;
            set;
        }

        /// <summary>
        /// Period is the interval between two successive updates of the Last Average Value. 
        /// (NumberOfPeriods * Period is the denominator for the calculation of the demand).
        /// </summary>
        [XmlIgnore()]
        public ulong Period
        {
            get;
            set;
        }

        /// <summary>
        /// The number of periods used to calculate the LastAverageValue.
        /// NumberOfPeriods >= 1 NumberOfPeriods > 1 indicates that the LastAverageValue represents �sliding demand�.
        /// NumberOfPeriods = 1 indicates that the LastAverageValue represents "block demand".
        /// The behaviour of the meter after writing a new value to this attribute shall be 
        /// specified by the manufacturer.
        /// </summary>
        [XmlIgnore()]
        public uint NumberOfPeriods
        {
            get;
            set;
        }

        public override bool IsRead(int index)
        {
            if (index == 4)
            {
                return this.Unit != Unit.None;
            }
            return base.IsRead(index);
        }

        /// <inheritdoc cref="GXDLMSObject.GetValues"/>
        public override object[] GetValues()
        {
            return new object[] { LogicalName, CurrentAverageValue, LastAverageValue, "Scaler: " + Scaler + " Unit: " + Unit, 
                            Status, CaptureTime, StartTimeCurrent, Period, NumberOfPeriods };
        }

        #region IGXDLMSBase Members

        int[] IGXDLMSBase.GetAttributeIndexToRead()
        {
            List<int> attributes = new List<int>();
            //LN is static and read only once.
            if (string.IsNullOrEmpty(LogicalName))
            {
                attributes.Add(1);
            }

            //Scaler and Unit                        
            if (!IsRead(4))
            {
                attributes.Add(4);
            }
            //CurrentAverageValue
            if (CanRead(2))
            {
                attributes.Add(2);
            }

            //LastAverageValue
            if (CanRead(3))
            {
                attributes.Add(3);
            }
            //Status
            if (CanRead(5))
            {
                attributes.Add(5);
            }
            //CaptureTime
            if (CanRead(6))
            {
                attributes.Add(6);
            }
            //StartTimeCurrent
            if (CanRead(7))
            {
                attributes.Add(7);
            }
            //Period
            if (CanRead(8))
            {
                attributes.Add(8);
            }
            //NumberOfPeriods
            if (CanRead(9))
            {
                attributes.Add(9);
            }
            return attributes.ToArray();
        }

        /// <inheritdoc cref="IGXDLMSBase.GetNames"/>
        string[] IGXDLMSBase.GetNames()
        {
            return new string[] {Gurux.DLMS.Properties.Resources.LogicalNameTxt, 
                "Current Average Value", 
                "Last Average Value", 
                "Scaler and Unit", 
                "Status", 
                "Capture Time", 
                "Start Time Current", 
                "Period", 
                "Number Of Periods"};            
        }

        int IGXDLMSBase.GetAttributeCount()
        {
            return 9;
        }

        int IGXDLMSBase.GetMethodCount()
        {
            return 2;
        }

        override public DataType GetDataType(int index)
        {
            if (index == 1)
            {
                return DataType.OctetString;
            }
            if (index == 2)
            {
                return base.GetDataType(index);
            }
            if (index == 3)
            {
                return base.GetDataType(index);
            }
            if (index == 4)
            {
                return DataType.Array;
            }
            if (index == 5)
            {
                return base.GetDataType(index);
            }
            if (index == 6)
            {
                return DataType.DateTime;
            }
            if (index == 7)
            {
                return DataType.DateTime;
            }
            if (index == 8)
            {
                return DataType.UInt32;
            }
            if (index == 9)
            {
                return DataType.UInt16;
            }
            throw new ArgumentException("GetDataType failed. Invalid attribute index.");
        }

        object IGXDLMSBase.GetValue(GXDLMSSettings settings, int index, int selector, object parameters)
        {
            if (index == 1)
            {
                return this.LogicalName;
            }
            if (index == 2)
            {
                return this.CurrentAverageValue;
            }
            if (index == 3)
            {
                return this.LastAverageValue;
            }
            if (index == 4)
            {
                GXByteBuffer data = new GXByteBuffer();
                data.SetUInt8((byte)DataType.Structure);
                data.SetUInt8(2);
                GXCommon.SetData(data, DataType.Int8, _scaler);
                GXCommon.SetData(data, DataType.Enum, Unit);
                return data.Array();
            }
            if (index == 5)
            {
                return this.Status;
            }
            if (index == 6)
            {
                return CaptureTime;
            }
            if (index == 7)
            {
                return StartTimeCurrent;
            }
            if (index == 8)
            {
                return Period;
            }
            if (index == 9)
            {
                return NumberOfPeriods;
            }
            throw new ArgumentException("GetValue failed. Invalid attribute index.");
        }

        void IGXDLMSBase.SetValue(GXDLMSSettings settings, int index, object value) 
        {
            if (index == 1)
            {
                if (value is string)
                {
                    LogicalName = value.ToString();
                }
                else
                {
                    LogicalName = GXDLMSClient.ChangeType((byte[])value, DataType.OctetString).ToString();
                }
            }
            else if (index == 2)
            {
                if (Scaler != 1)
                {
                    try
                    {
                        CurrentAverageValue = Convert.ToDouble(value) * Scaler;
                    }
                    catch (Exception)
                    {
                        //Sometimes scaler is set for wrong Object type.
                        CurrentAverageValue = value;
                    }
                }
                else
                {
                    CurrentAverageValue = value;
                }
            }
            else if (index == 3)
            {                 
                if (Scaler != 1)
                {
                    try
                    {
                        LastAverageValue = Convert.ToDouble(value) * Scaler;
                    }
                    catch (Exception)
                    {
                        //Sometimes scaler is set for wrong Object type.
                        LastAverageValue = value;
                    }
                }
                else
                {
                    LastAverageValue = value;
                }
            }
            else if (index == 4)
            {
                if (value == null)
                {
                    Scaler = 1;
                    Unit = Unit.None;
                }
                else
                {
                    object[] arr = (object[])value;
                    if (arr.Length != 2)
                    {
                        throw new Exception("setValue failed. Invalid scaler unit value.");
                    }
                    _scaler = Convert.ToInt32(arr[0]);
                    Unit = (Unit)Convert.ToInt32(arr[1]);
                }             
            }
            else if (index == 5)
            {
                Status = Convert.ToInt32(value);
            }
            else if (index == 6)
            {
                if (value == null)
                {
                    CaptureTime = new GXDateTime(DateTime.MinValue);
                }
                else
                {
                    if (value is byte[])
                    {
                        value = GXDLMSClient.ChangeType((byte[])value, DataType.DateTime);
                    }
                    CaptureTime = (GXDateTime)value;
                }                  
            }
            else if (index == 7)
            {
                if (value == null)
                {
                    StartTimeCurrent = new GXDateTime(DateTime.MinValue);
                }
                else
                {
                    if (value is byte[])
                    {
                        value = GXDLMSClient.ChangeType((byte[])value, DataType.DateTime);
                    }
                    StartTimeCurrent = (GXDateTime)value;
                }                
            }
            else if (index == 8)
            {
                Period = Convert.ToUInt32(value);
            }
            else if (index == 9)
            {
                NumberOfPeriods = Convert.ToUInt16(value);
            }
            else
            {
                throw new ArgumentException("SetValue failed. Invalid attribute index.");
            }
        }

        byte[] IGXDLMSBase.Invoke(GXDLMSSettings settings, int index, Object parameters)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
