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


using Newtonsoft.Json;
using Sheng.SQLite.Plus.Demo.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Sheng.SQLite.Plus.Demo
{
    public partial class Form1 : Form
    {
        DatabaseWrapper _database;

        public Form1()
        {
            InitializeComponent();

            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["sqlite"].ConnectionString;
            _database = new DatabaseWrapper(connectionString);
        }

        private void btnInsert_Click(object sender, EventArgs e)
        {
            StudentEntity student = new StudentEntity();
            student.NickName = "张三";
            student.Age = 18;

            _database.Insert(student);
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            List<StudentEntity> studentList = _database.Select<StudentEntity>();
            MessageBox.Show(JsonConvert.SerializeObject(studentList));
        }

        private void btnGet_Click(object sender, EventArgs e)
        {
            StudentEntity student = new StudentEntity();
            student.Id = 1;
            _database.Fill<StudentEntity>(student);
            MessageBox.Show(JsonConvert.SerializeObject(student));
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            StudentEntity student = new StudentEntity();
            student.Id = 1;
            student.NickName = "李四";
            student.Age = 20;
            _database.Update(student);

            //查
            _database.Fill<StudentEntity>(student);
            MessageBox.Show(JsonConvert.SerializeObject(student));
        }
    }
}
