﻿/*******************************************************}
{                                                       }
{               HCView V1.1  作者：荆通                 }
{                                                       }
{      本代码遵循BSD协议，你可以加入QQ群 649023932      }
{           来获取更多的技术交流 2018-12-20             }
{                                                       }
{  支持二次开发的的文档对象管理单元，你可以将应用程序   }
{  特有的需要在Data层处理的功能放置到此单元，源码升级   }
{  不会变动此单元，以保证你的业务代码不会被覆盖         }
{*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HC.View
{
    public class HCViewDevData : HCAnnotateData
    {
        public HCViewDevData(HCStyle aStyle) : base(aStyle)
        {

        }

        ~HCViewDevData()
        {

        }
    }
}
