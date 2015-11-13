﻿using SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MegaApp.Classes;

namespace MegaApp.Database
{
    public class DataBaseHelper<T> where T : new()
    {
        // Indicate if the node exists in the database table.
        public static bool ExistsNode(String tableName, String fieldName, String fieldValue)
        {
            return (ReadNode(tableName, fieldName, fieldValue) != null) ? true : false;
        }

        // Retrieve the first node found in the database table.
        public static T ReadNode(String tableName, String fieldName, String fieldValue)
        {
            using (var dbConn = new SQLiteConnection(App.DB_PATH))
            {
                var existingNode = dbConn.Query<T>("select * from " + tableName + " where " + fieldName + " = '" + fieldValue +"'").FirstOrDefault();
                return existingNode;
            }
        }

        // Retrieve the node list found in the database table.
        public static ObservableCollection<T> ReadNodes(String tableName, String fieldName, String fieldValue)
        {
            using (var dbConn = new SQLiteConnection(App.DB_PATH))
            {
                List<T> _nodeList = dbConn.Query<T>("select * from " + tableName + " where " + fieldName + " = '" + fieldValue + "'").ToList<T>();
                ObservableCollection<T> nodeList = new ObservableCollection<T>(_nodeList);
                return nodeList;
            }
        }

        // Retrieve the all node list from the database table.
        public static ObservableCollection<T> ReadAllNodes()
        {
            using (var dbConn = new SQLiteConnection(App.DB_PATH))
            {
                List<T> _nodeList = dbConn.Table<T>().ToList<T>();
                ObservableCollection<T> nodeList = new ObservableCollection<T>(_nodeList);
                return nodeList;
            }
        }                

        // Update existing node 
        public static void UpdateNode(T node)
        {
            using (var dbConn = new SQLiteConnection(App.DB_PATH))
            {
                dbConn.RunInTransaction(() =>
                {
                    dbConn.Update(node);
                });                
            }
        }        

        // Insert the new node in the database. 
        public static void Insert(T newNode)
        {
            using (var dbConn = new SQLiteConnection(App.DB_PATH))
            {
                dbConn.RunInTransaction(() =>
                {
                    dbConn.Insert(newNode);
                });
            }
        }        

        // Delete the first node found with the specified field value
        public static void DeleteNode(String tableName, String fieldName, String fieldValue)
        {
            using (var dbConn = new SQLiteConnection(App.DB_PATH))
            {
                var existingNode = dbConn.Query<T>("select * from " + tableName + " where " + fieldName + " = '" + fieldValue + "'").FirstOrDefault();
                if (existingNode != null)
                {
                    dbConn.RunInTransaction(() =>
                    {
                        dbConn.Delete(existingNode);
                    });
                }
            }
        }

        // Delete specific node
        public static void DeleteNode(T node)
        {
            using (var dbConn = new SQLiteConnection(App.DB_PATH))
            {
                dbConn.RunInTransaction(() =>
                {
                    dbConn.Delete(node);
                });
            }
        }

        // Delete all node list or delete table 
        public static void DeleteAllNodes()
        {
            using (var dbConn = new SQLiteConnection(App.DB_PATH))
            {
                dbConn.DropTable<T>();
                dbConn.CreateTable<T>();
                dbConn.Dispose();
                dbConn.Close();                
            }
        }        
    }
}
