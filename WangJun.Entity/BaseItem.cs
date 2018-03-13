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
        #region 当前实体信息
        public ObjectId _id { get; set; }
        public ObjectId _OID { get { return this._id; } set { this._id = value; } }

        //public Guid _GID { get; set; }
 
        //public long _IntID { get; set; }
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
                else
                {
                    this._OID = ObjectId.Empty;
                }
            }
        }
        public string Name { get; set; }

        #endregion

        #region 父级引用信息
        public ObjectId _ParentOID { get; set; }

        //public Guid _ParentGID { get; set; }
 
        public long _ParentIntID { get; set; }

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

        #endregion


        #region 根级信息
        public ObjectId _RootOID { get; set; }
         //public Guid _RootGID { get; set; }

        public long RootIntID { get; set; }

        public string RootID
        {
            get
            {
                return this._RootOID.ToString();
            }
            set
            {
                if (StringChecker.IsObjectId(value))
                {
                    this._RootOID = ObjectId.Parse(value);
                }
            }
        }

        public string RootName { get; set; }
        #endregion

        #region 时间信息
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }
        public DateTime DeleteTime { get; set; }
        #endregion


        #region 权限控制
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

        public string _RedirectID { get; set; }

        #endregion

        #region 状态信息
        public string Status { get; set; }

        public int StatusCode { get; set; }
        #endregion

        #region 创建和修改信息
        public string CreatorID { get; set; }

        public string CreatorName { get; set; }

        public string ModifierID { get; set; }

        public string ModifierName { get; set; }
        #endregion

        #region 系统级别的信息
        public string ClassFullName { get; set; }

        public string _DbName { get; set; }

        public string _CollectionName { get; set; }

        public string _SourceID { get; set; }

        public int Version { get; set; }

        public string AppName { get; set; }

        public long AppCode { get; set; }
        #endregion

        #region 公司/群组信息
        public string CompanyID { get; set; }
        public string CompanyName { get; set; }
        #endregion

        public int AllowedComment { get; set; }

        public int Level { get; set; } ///紧急度，权限度，重要性

        #region 所有者信息/公司的就是超级管理员的
        public ObjectId _OOwnerID { get; set; }

        public Guid _GOwnerID { get; set; }
        public string OwnerID
        {
            get
            {
                return this._OOwnerID.ToString();
            }
            set
            {
                if (StringChecker.IsObjectId(value))
                {
                    this._OOwnerID = ObjectId.Parse(value);
                }
            }
        }
        #endregion

        #region 通用方法
        public void Remove()
        {
            EntityManager.GetInstance().Remove(this);
        }
        public void Delete()
        {
            EntityManager.GetInstance().Delete(this);
        }
        #endregion
    }
}
