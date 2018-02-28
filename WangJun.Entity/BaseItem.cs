using MongoDB.Bson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WangJun.Utility;

namespace WangJun.Entity
{
    /// <summary>
    /// 基本类型
    /// </summary>
    public class BaseItem
    {
        public ObjectId _id { get; set; }
        public ObjectId _OID { get { return this._id; } set { this._id = value; } }

        public Guid _GID { get; set; }


        public long IntID { get; set; }
        public string ID
        {
            get
            {
                return this._OID.ToString();
            }
            set
            {
                if (StringChecker.IsObjectId(value))
                {
                    this._OID = ObjectId.Parse(value);
                }
            }
        }
        public string Name { get; set; }

        public ObjectId _ParentOID { get; set; }

        public Guid _ParentGID { get; set; }


        public long ParentIntID { get; set; }

        public string ParentID
        {
            get
            {
                return this._ParentOID.ToString();
            }
            set
            {
                if (StringChecker.IsObjectId(value))
                {
                    this._ParentOID = ObjectId.Parse(value);
                }
            }
        }

        public string ParentName { get; set; }

        public ObjectId _RootOID { get; set; }
        public string RootOID { get; set; }

        public Guid _RootGID { get; set; }

        public long RootIntID { get; set; }

        public string RootID { get; set; }

        public string RootName { get; set; }

        public long GroupID { get; set; }

        public string GroupName { get; set; }

        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }
        public DateTime DeleteTime { get; set; }


        public ArrayList OrgAllowedArray { get; set; }

        public string OrgAllowedArrayText { get; set; }

        public ArrayList UserAllowedArray { get; set; }

        public string UserAllowedArrayText { get; set; }
         
        public ArrayList RoleAllowedArray { get; set; }

        public string RoleAllowedArrayText { get; set; }


        public ArrayList OrgDeniedArray { get; set; }

        public string OrgDeniedArrayText { get; set; }


        public ArrayList UserDeniedArray { get; set; }

        public string UserDeniedArrayText { get; set; }
         
        public ArrayList RoleDeniedArray { get; set; }

        public string RoleDeniedArrayText { get; set; }



        public string Status { get; set; }

        public int StatusCode { get; set; }

        public string ClassFullName { get; set; }

        public string CreatorID { get; set; }

        public string CreatorName { get; set; }

        public string ModifierID { get; set; }

        public string ModifierName { get; set; }

        public int HasProc { get; set; } //是否处理

        public DateTime ProcTime { get; set; } ///处理时间

        public List<Dictionary<string, object>> ModifyLog { get; set; }

        public string _DbName { get; set; }

        public string _CollectionName { get; set; }

        public string _SourceID { get; set; }
         

        public int AllowedComment { get; set; }

        public int Version { get; set; }

        public string AppName { get; set; }

        public int AppCode { get; set; }

    }
}
