/*
********************************************************************
*
*    曹旭升（sheng.c）
*    E-mail: cao.silhouette@msn.com
*    QQ: 279060597
*    https://github.com/iccb1013
*    http://shengxunwei.com
*
*    © Copyright 2016
*
********************************************************************/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheng.SQLite.Plus.Demo.Entities
{
    //如果实体类名和表名一样，可以不写这个 Attribute
    [Table("Student")]
    class StudentEntity
    {
        [Key]
        public Int64? Id
        {
            get;set;
        }

        //如果属性名和表中字段名一样，可以不写这个 Attribute
        [Column("Name")]
        public string NickName
        {
            get;set;
        }

        public Int64 Age
        {
            get; set;
        }
    }
}
