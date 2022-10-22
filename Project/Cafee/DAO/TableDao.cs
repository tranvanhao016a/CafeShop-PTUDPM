﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cafee.DTO
{
    internal class TableDao
    {
        private static TableDao instance;
        public static TableDao Instance
        {
            get
            {
                if (instance == null)
                { instance = new TableDao(); }
                return TableDao.instance;
            }
            private set { TableDao.instance = value; }
        }
        private TableDao() { }
        private Table newTable(DataRow row)
        {
            Table table = new Table()
            {
                id = (int)row["id"],
                name = row["Name"].ToString(),
                status = (int)row["Status"],
            };
            return table;
        }
        public List<Table> LoadTableList()
        {
            List<Table> tables = new List<Table>();
            DataTable data = DataProvider.Instance.ExecuteQuery("select * from TableFood");
            foreach (DataRow item in data.Rows)
            {
                Table table = newTable(item);
                tables.Add(table);
            }
            return tables;
        }
    }
}