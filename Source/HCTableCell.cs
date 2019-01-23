﻿/*******************************************************}
{                                                       }
{               HCView V1.1  作者：荆通                 }
{                                                       }
{      本代码遵循BSD协议，你可以加入QQ群 649023932      }
{            来获取更多的技术交流 2018-5-4              }
{                                                       }
{                  表格单元格实现单元                   }
{                                                       }
{*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;

namespace HC.View
{
    /// <summary> 垂直对齐方式：上、居中、下) </summary>
    public enum AlignVert : byte
    {
        cavTop, cavCenter, cavBottom
    }

    public class HCTableCell : HCObject
    {
        private HCTableCellData FCellData;

        private int FWidth;  // 被合并后记录原始宽(否则当行第一列被合并后，第二列无法确认水平起始位置)

        private int FHeight ;  // 被合并后记录原始高、记录拖动改变后高

        private int FRowSpan;  // 单元格跨几行，用于合并目标单元格记录合并了几行，合并源记录合并到单元格的行号，0没有行合并

        private int FColSpan ;  // 单元格跨几列，用于合并目标单元格记录合并了几列，合并源记录合并到单元格的列号，0没有列合并

        private Color FBackgroundColor;

        private AlignVert FAlignVert;

        private HCBorderSides FBorderSides;

        protected bool GetActive()
        {
            if (FCellData != null)
                return FCellData.Active;
            else
                return false;
        }

        protected void SetActive(bool value)
        {
            if (FCellData != null)
                FCellData.Active = value;
        }

        protected void SetHeight(int value)
        {
             if (FHeight != value)
            {
                FHeight = value;
                if (FCellData != null)
                    FCellData.CellHeight = value;
            }
        }

        public HCTableCell()
        {

        }

        public HCTableCell(HCStyle AStyle) : this()
        {
            FCellData = new HCTableCellData(AStyle);
            FAlignVert = View.AlignVert.cavTop;
            FBorderSides = new HCBorderSides();
            FBorderSides.InClude((byte)BorderSide.cbsLeft);
            FBorderSides.InClude((byte)BorderSide.cbsTop);
            FBorderSides.InClude((byte)BorderSide.cbsRight);
            FBorderSides.InClude((byte)BorderSide.cbsBottom);
            FBackgroundColor = AStyle.BackgroudColor;
            FRowSpan = 0;
            FColSpan = 0;
        }

        ~HCTableCell()
        {
            
        }

        public override void Dispose()
        {
            base.Dispose();
            FCellData.Dispose();
        }

        public bool IsMergeSource()
        {
            return (FCellData == null);
        }

        public bool IsMergeDest()
        {
            return ((FRowSpan > 0) || (FColSpan > 0));
        }

        /// <summary> 清除并返回为处理分页比净高增加的高度(为重新格式化时后面计算偏移用) </summary>
        public int ClearFormatExtraHeight()
        {
            int Result = 0;
            if (FCellData != null)
                Result = FCellData.ClearFormatExtraHeight();

            return Result;
        }

        public virtual void SaveToStream(Stream aStream)
        {
            /* 因为可能是合并后的单元格，所以单独存宽、高 }*/
            byte[] vBuffer = BitConverter.GetBytes(FWidth);
            aStream.Write(vBuffer, 0, vBuffer.Length);

            vBuffer = BitConverter.GetBytes(FHeight);
            aStream.Write(vBuffer, 0, vBuffer.Length);

            vBuffer = BitConverter.GetBytes(FRowSpan);
            aStream.Write(vBuffer, 0, vBuffer.Length);

            vBuffer = BitConverter.GetBytes(FColSpan);
            aStream.Write(vBuffer, 0, vBuffer.Length);

            byte vByte = (byte)FAlignVert;
            aStream.WriteByte(vByte);  // 垂直对齐方式

            HC.HCSaveColorToStream(aStream, FBackgroundColor); // 背景色

            aStream.WriteByte(FBorderSides.Value);

            /* 存数据 }*/
            bool vNullData = (FCellData == null);
            vBuffer = BitConverter.GetBytes(vNullData);
            aStream.Write(vBuffer, 0, vBuffer.Length);
            if (!vNullData)
                FCellData.SaveToStream(aStream);
        }

        public void LoadFromStream(Stream aStream, HCStyle aStyle, ushort aFileVersion)
        {
            byte[] vBuffer = BitConverter.GetBytes(FWidth);
            aStream.Read(vBuffer, 0, vBuffer.Length);
            FWidth = BitConverter.ToInt32(vBuffer, 0);

            vBuffer = BitConverter.GetBytes(FHeight);
            aStream.Read(vBuffer, 0, vBuffer.Length);
            FHeight = BitConverter.ToInt32(vBuffer, 0);

            vBuffer = BitConverter.GetBytes(FRowSpan);
            aStream.Read(vBuffer, 0, vBuffer.Length);
            FRowSpan = BitConverter.ToInt32(vBuffer, 0);

            vBuffer = BitConverter.GetBytes(FColSpan);
            aStream.Read(vBuffer, 0, vBuffer.Length);
            FColSpan = BitConverter.ToInt32(vBuffer, 0);

            if (aFileVersion > 11)
            {
                byte vByte = 0;
                vByte = (byte)aStream.ReadByte();
                FAlignVert = (View.AlignVert)vByte;  // 垂直对齐方式

                HC.HCLoadColorFromStream(aStream, ref FBackgroundColor);  // 背景色
            }
            if (aFileVersion > 13)
            {
                FBorderSides.Value = (byte)aStream.ReadByte(); // load FBorderSides              
            }

            bool vNullData = false;
            vBuffer = BitConverter.GetBytes(vNullData);
            aStream.Read(vBuffer, 0, vBuffer.Length);
            vNullData = BitConverter.ToBoolean(vBuffer, 0);
            if (!vNullData)
            {
                FCellData.LoadFromStream(aStream, aStyle, aFileVersion);
                FCellData.CellHeight = FHeight;
            }
            else
            {
                FCellData.Dispose();
                FCellData = null;
            }
        }

        public void ToXml(XmlElement aNode)
        {
            aNode.Attributes["width"].Value = FWidth.ToString();
            aNode.Attributes["height"].Value = FHeight.ToString();
            aNode.Attributes["rowspan"].Value = FRowSpan.ToString();
            aNode.Attributes["colspan"].Value = FColSpan.ToString();
            aNode.Attributes["vert"].Value = ((byte)FAlignVert).ToString();
            aNode.Attributes["bkcolor"].Value = HC.GetColorXmlRGB(FBackgroundColor);
            aNode.Attributes["border"].Value = HC.GetBorderSidePro(FBorderSides);
        }

        public void ParseXml(XmlElement aNode)
        {
            FWidth = int.Parse(aNode.Attributes["width"].Value);
            FHeight = int.Parse(aNode.Attributes["height"].Value);
            FRowSpan = int.Parse(aNode.Attributes["rowspan"].Value);
            FColSpan = int.Parse(aNode.Attributes["colspan"].Value);
            FAlignVert = (AlignVert)(byte.Parse(aNode.Attributes["vert"].Value));
            FBackgroundColor = HC.GetXmlRGBColor(aNode.Attributes["bkcolor"].Value);
            HC.SetBorderSideByPro(aNode.Attributes["border"].Value, FBorderSides);

            if ((FRowSpan < 0) || (FColSpan < 0))
            {
                FCellData.Dispose();
                //FCellData = null;
            }
            else
                FCellData.ParseXml(aNode.SelectSingleNode("items") as XmlElement);
        }

        public void GetCaretInfo(int aItemNo, int  aOffset, ref HCCaretInfo aCaretInfo)
        {
            if (FCellData != null)
            {
                FCellData.GetCaretInfo(aItemNo, aOffset, ref aCaretInfo);
                if (aCaretInfo.Visible)
                {
                    if (FAlignVert == AlignVert.cavBottom)
                        aCaretInfo.Y = aCaretInfo.Y + FHeight - FCellData.Height;
                    else
                    if (FAlignVert == AlignVert.cavCenter)
                        aCaretInfo.Y = aCaretInfo.Y + (FHeight - FCellData.Height) / 2;
                }
            }
            else
                aCaretInfo.Visible = false;
        }

        /// <summary> 绘制数据 </summary>
        /// <param name="aDataDrawLeft">绘制目标区域Left</param>
        /// <param name="aDataDrawTop">绘制目标区域的Top</param>
        /// <param name="aDataDrawBottom">绘制目标区域的Bottom</param>
        /// <param name="aDataScreenTop">屏幕区域Top</param>
        /// <param name="aDataScreenBottom">屏幕区域Bottom</param>
        /// <param name="aVOffset">指定从哪个位置开始的数据绘制到目标区域的起始位置</param>
        /// <param name="ACanvas">画布</param>
        public  void PaintData(int aDataDrawLeft, int aDataDrawTop, int aDataDrawBottom, 
            int aDataScreenTop, int aDataScreenBottom, int aVOffset, 
            HCCanvas ACanvas, PaintInfo APaintInfo)
        {
            if (FCellData != null)
            {
                int vTop = 0;
                switch (FAlignVert)
                {
                    case View.AlignVert.cavTop: 
                        vTop = aDataDrawTop;
                        break;

                    case View.AlignVert.cavBottom: 
                        vTop = aDataDrawTop + FHeight - FCellData.Height;
                        break;

                    case View.AlignVert.cavCenter: 
                        vTop = aDataDrawTop + (FHeight - FCellData.Height) / 2;
                        break;
                }
            
                FCellData.PaintData(aDataDrawLeft, vTop, aDataDrawBottom, aDataScreenTop,
                    aDataScreenBottom, aVOffset, ACanvas, APaintInfo);
            }
        }

        public HCTableCellData CellData
        {
            get { return FCellData; }
            set { FCellData = value; }
        }

        /// <summary> 单元格宽度(含CellHPadding)，数据的宽度在TableItem中处理 </summary>
        public int Width
        {
            get { return FWidth; }
            set { FWidth = value; }
        }

        /// <summary> 单元格高度(含CellVPadding * 2 主要用于合并目标单元格，如果发生合并，则>=数据高度) </summary>
        public int Height
        {
            get { return FHeight; }
            set { SetHeight(value); }
        }

        public int RowSpan
        {
            get { return FRowSpan; }
            set { FRowSpan = value; }
        }

        public int ColSpan
        {
            get { return FColSpan; }
            set { FColSpan = value; }
        }

        public Color BackgroundColor
        {
            get { return FBackgroundColor; }
            set { FBackgroundColor = value; }
        }

        // 用于表格切换编辑的单元格
        public bool Active
        {
            get { return GetActive(); }
            set { SetActive(value); }
        }

        public AlignVert AlignVert
        {
            get { return FAlignVert; }
            set { FAlignVert = value; }
        }

        public HCBorderSides BorderSides
        {
            get { return FBorderSides; }
            set { FBorderSides = value; }
        }

    }
}
