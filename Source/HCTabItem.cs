﻿/*******************************************************}
{                                                       }
{               HCView V1.1  作者：荆通                 }
{                                                       }
{      本代码遵循BSD协议，你可以加入QQ群 649023932      }
{            来获取更多的技术交流 2018-5-4              }
{                                                       }
{                 文档Tab对象实现单元                   }
{                                                       }
{*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HC.Win32;

namespace HC.View
{
    public class HCTabItem : HCTextRectItem
    {
        protected override void DoPaint(HCStyle aStyle, Win32.RECT aDrawRect, int aDataDrawTop, int aDataDrawBottom, int aDataScreenTop, int aDataScreenBottom, HCCanvas aCanvas, PaintInfo aPaintInfo)
        {
            base.DoPaint(aStyle, aDrawRect, aDataDrawTop, aDataDrawBottom, aDataScreenTop, aDataScreenBottom, aCanvas, aPaintInfo);
        }

        public HCTabItem(HCCustomData aOwnerData) : base(aOwnerData)
        {
            StyleNo = HCStyle.Tab;
            aOwnerData.Style.TextStyles[TextStyleNo].ApplyStyle(aOwnerData.Style.DefCanvas);
            SIZE vSize = aOwnerData.Style.DefCanvas.TextExtent("汉字");
            Width = vSize.cx;
            Height = vSize.cy;
        }

        public HCTabItem(HCCustomData aOwnerData, int aWidth, int aHeight) : base(aOwnerData, aWidth, aHeight)
        {
            
        }

        public override int GetOffsetAt(int x)
        {
            if (x < Width / 2)
                return HC.OffsetBefor;
            else
                return HC.OffsetAfter;
        }

        public override bool JustifySplit()
        {
            return false;
        }
    }
}
