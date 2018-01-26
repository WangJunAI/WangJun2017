﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WangJun.Doc
{
    public static class CONST
    {
        public static class DB
        {
            public static string DBName_DocService { get { return "DocService"; } }

            public static string CollectionName_DocItem { get { return "DocItem"; } }

            public static string CollectionName_CategoryItem { get { return "CategoryItem"; } }

            public static string CollectionName_CommentItem { get { return "CommentItem"; } }

            public static string CollectionName_LogItem { get { return "LogItem"; } }

            public static string CollectionName_InvokeItem { get { return "InvokeItem"; } }
        }

        public static class Status {
            public static string Normal  { get { return "正常"; } }
        }
    }
}
