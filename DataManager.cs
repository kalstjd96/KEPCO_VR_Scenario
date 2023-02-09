using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Data;
using System.IO;
using UnityEngine;
using System.Runtime.InteropServices;
using System.Text;

public class DataManager : MonoBehaviour
{

    #region Ini

    [DllImport("kernel32")]
    public static extern int GetPrivateProfileString(string section,
        string key, string def, StringBuilder retVal, int size, string filePath);

    [DllImport("kernel32")]
    public static extern long WritePrivateProfileString(string section,
        string key, string val, string filePath);

    // get ini CheckValidation YARD
    public static String Get_Ini(String Section, String Key)
    {
        StringBuilder temp = new StringBuilder(255);
        int i = GetPrivateProfileString(Section, Key, "", temp, 255, Application.streamingAssetsPath + "/" + "Config.ini");
        return temp.ToString().Trim();
    }

    // set ini
    public static void Set_Ini(string Section, string Key, string Value)
    {
        WritePrivateProfileString(Section, Key, Value, Application.streamingAssetsPath + "/" + "Config.ini");
    }

    #endregion

    public enum Location
    {
        LOCAL,
        SERVER
    }

    public static DataManager instance;

    [NonSerialized] public Location location;
    [NonSerialized] public DataTable dataTable;
    private readonly string LocalPath = Application.streamingAssetsPath + "/";
    private readonly string ServerPath = Get_Ini("SERVERPATH","DB");

    void Awake()
    {
        #region 싱글톤
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(instance.gameObject);
        #endregion
    }

    /// <summary>
    /// 데이터베이스에서 테이블 전체 값을 받아오는 함수
    /// (SELECT * FROM {테이블명})
    /// </summary>
    /// <param name="location">데이터베이스 경로</param>
    /// <param name="databaseName">데이터 베이스 이름</param>
    /// <param name="tableName">검색 테이블 명</param>
    public DataTable Select(Location location, string databaseName, string tableName)
    {
        SQLiteConnection sqlConn;

        if (location == Location.LOCAL)
            sqlConn = new SQLiteConnection(@"Data Source=" + LocalPath + databaseName);
        else
            sqlConn = new SQLiteConnection(@"Data Source=" + Get_Ini("SERVERPATH", "DB") + databaseName);

        sqlConn.Open();

        var table = new DataTable();
        var Sda = new SQLiteDataAdapter();

        string qry = $"SELECT * FROM {tableName}";
        Sda.SelectCommand = new SQLiteCommand(qry, sqlConn);
        Sda.Fill(table);

        if (Sda.SelectCommand != null)
            Sda.SelectCommand.Dispose();

        if (sqlConn != null)
            sqlConn.Close();

        return table;
    }

    /// <summary>
    /// 데이터베이스에서 테이블 중 특정 조건의 전체 값을 받아오는 함수
    /// (SELECT * FROM {테이블명} (WHERE {컬럼명} = {컬럼값}))
    /// </summary>
    /// <param name="location">데이터베이스 경로</param>
    /// <param name="databaseName">데이터 베이스 이름</param>
    /// <param name="tableName">검색 테이블 명</param>
    /// <param name="whereKeys">검색 조건 컬럼 명(Multiple) default는 Null</param>
    /// <param name="whereValues">검색 조건 컬럼 값(Multiple) default는 Null</param>
    public DataTable Select(Location location, string databaseName, string tableName, string[] whereKeys = null, string[] whereValues = null)
    {
        SQLiteConnection sqlConn;

        if (location == Location.LOCAL)
            sqlConn = new SQLiteConnection(@"Data Source=" + LocalPath + databaseName);
        else
            sqlConn = new SQLiteConnection(@"Data Source=" + Get_Ini("SERVERPATH", "DB") + databaseName);

        sqlConn.Open();

        var table = new DataTable();
        var Sda = new SQLiteDataAdapter();

        string query = $"SELECT * FROM {tableName}";

        if (whereKeys != null)
        {
            query += $" WHERE ";

            for (int i = 0; i < whereKeys.Length; i++)
            {
                if (i == whereKeys.Length - 1)
                    query += $"{whereKeys[i]} = '{whereValues[i]}'";
                else
                    query += $"{whereKeys[i]} = '{whereValues[i]}' AND ";
            }
        }

        Sda.SelectCommand = new SQLiteCommand(query, sqlConn);
        Sda.Fill(table);

        if (Sda.SelectCommand != null)
            Sda.SelectCommand.Dispose();

        if (sqlConn != null)
            sqlConn.Close();

        return table;
    }

    /// <summary>
    /// 데이터베이스에서 테이블중 특정 컬럼 값(Single)을 받아오는 함수
    /// (SELECT {컬럼명} FROM {테이블명} (WHERE {컬럼명} = {컬럼값}))
    /// </summary>
    /// <param name="location">데이터베이스 경로</param>
    /// <param name="databaseName">데이터 베이스 이름</param>
    /// <param name="tableName">검색 테이블 명</param>
    /// <param name="column">검색 컬럼 명</param>
    /// <param name="whereKeys">검색 조건 컬럼 명(Multiple) default는 Null</param>
    /// <param name="whereValues">검색 조건 컬럼 값(Multiple) default는 Null</param>
    public string Select(Location location, string databaseName, string tableName, string column, string[] whereKeys = null, string[] whereValues = null)
    {
        SQLiteConnection sqlConn;

        if (location == Location.LOCAL)
            sqlConn = new SQLiteConnection(@"Data Source=" + LocalPath + databaseName);
        else
            sqlConn = new SQLiteConnection(@"Data Source=" + Get_Ini("SERVERPATH", "DB") + databaseName);

        sqlConn.Open();

        string query = $"SELECT {column} FROM {tableName}";

        if (whereKeys != null)
        {
            query += $" WHERE ";

            for (int i = 0; i < whereKeys.Length; i++)
            {
                if (i == whereKeys.Length - 1)
                    query += $"{whereKeys[i]} = '{whereValues[i]}'";
                else
                    query += $"{whereKeys[i]} = '{whereValues[i]}' AND ";
            }
        }

        SQLiteCommand sCmd = new SQLiteCommand(query, sqlConn);
        string data = sCmd.ExecuteScalar().ToString();
        if (sCmd != null)
            sCmd.Dispose();
        if (sqlConn != null)
            sqlConn.Close();

        return data;
    }

    /// <summary>
    /// 데이터베이스에서 테이블중 특정 컬럼들의 값(Multiple)을 받아오는 함수
    /// (SELECT {컬럼명, 컬럼명 ...} FROM {테이블명} (WHERE {컬럼명} = {컬럼값}))
    /// </summary>
    /// <param name="location">데이터베이스 경로</param>
    /// <param name="databaseName">데이터 베이스 이름</param>
    /// <param name="tableName">검색 테이블 명</param>
    /// <param name="column">검색 컬럼 명(Multiple)</param>
    /// <param name="whereKeys">검색 조건 컬럼 명(Multiple) default는 Null</param>
    /// <param name="whereValues">검색 조건 컬럼 값(Multiple) default는 Null</param>
    public DataTable Select(Location location, string databaseName, string tableName, string[] column, string[] whereKeys = null, string[] whereValues = null)
    {
        SQLiteConnection sqlConn;

        if (location == Location.LOCAL)
            sqlConn = new SQLiteConnection(@"Data Source=" + LocalPath + databaseName);
        else
            sqlConn = new SQLiteConnection(@"Data Source=" + Get_Ini("SERVERPATH", "DB") + databaseName);

        sqlConn.Open();

        string query = $"SELECT ";

        for (int i = 0; i < column.Length; i++)
        {
            if (i == column.Length - 1)
                query += $"{column[i]} FROM {tableName}";
            else
                query += $"{column[i]}, ";
        }

        if (whereKeys != null)
        {
            query += $" WHERE ";

            for (int i = 0; i < whereKeys.Length; i++)
            {
                if (i == whereKeys.Length - 1)
                    query += $"{whereKeys[i]} = '{whereValues[i]}'";
                else
                    query += $"{whereKeys[i]} = '{whereValues[i]}' AND ";
            }
        }

        var table = new DataTable();
        var Sda = new SQLiteDataAdapter();

        Sda.SelectCommand = new SQLiteCommand(query, sqlConn);
        Sda.Fill(table);

        if (Sda.SelectCommand != null)
            Sda.SelectCommand.Dispose();

        if (sqlConn != null)
            sqlConn.Close();

        return table;
    }

    /// <summary>
    /// 데이터베이스에서 테이블에 값을 삽입하는 함수
    /// (INSERT INTO {테이블명} VALUES ({데이터, 데이터 .....}))
    /// </summary>
    /// <param name="location">데이터베이스 경로</param>
    /// <param name="databaseName">데이터 베이스 이름</param>
    /// <param name="tableName">검색 테이블 명</param>
    /// <param name="parameters">삽입 할 데이터(Multiple)</param>
    public void Insert(Location location, string databaseName, string tableName, string[] parameters)
    {
        SQLiteConnection sqlConn;

        if (location == Location.LOCAL)
            sqlConn = new SQLiteConnection(@"Data Source=" + LocalPath + databaseName);
        else
            sqlConn = new SQLiteConnection(@"Data Source=" + Get_Ini("SERVERPATH", "DB") + databaseName);

        sqlConn.Open();

        string query = $"INSERT INTO {tableName} VALUES (";

        for (int i = 0; i < parameters.Length; i++)
        {
            if (i == parameters.Length - 1)
                query += $"'{parameters[i].Trim()}')";
            else
                query += $"'{parameters[i].Trim()}', ";
        }

        SQLiteCommand sCmd = new SQLiteCommand(query, sqlConn);
        sCmd.ExecuteNonQuery();

        if (sqlConn != null)
            sqlConn.Close();
    }

    /// <summary>
    /// 데이터베이스에서 테이블에 특정 컬럼에만 값을 삽입하는 함수
    /// (INSERT INTO {테이블명} ({컬럼명, 컬럼명 ...}) VALUES ({데이터, 데이터 .....}))
    /// </summary>
    /// <param name="location">데이터베이스 경로</param>
    /// <param name="databaseName">데이터 베이스 이름</param>
    /// <param name="tableName">검색 테이블 명</param>
    /// <param name="columns">삽입 할 컬럼 명(Multiple)</param>
    /// <param name="parameters">삽입 할 데이터(Multiple)</param>
    public void Insert(Location location, string databaseName, string tableName, string[] columns, string[] parameters)
    {
        SQLiteConnection sqlConn;

        if (location == Location.LOCAL)
            sqlConn = new SQLiteConnection(@"Data Source=" + LocalPath + databaseName);
        else
            sqlConn = new SQLiteConnection(@"Data Source=" + Get_Ini("SERVERPATH", "DB") + databaseName);

        sqlConn.Open();

        string query = $"INSERT INTO {tableName} (";

        for (int i = 0; i < columns.Length; i++)
        {
            if (i == columns.Length - 1)
                query += $"{columns[i].Trim()}) VALUES (";
            else
                query += $"{columns[i].Trim()}, ";
        }

        for (int i = 0; i < parameters.Length; i++)
        {
            if (i == parameters.Length - 1)
                query += $"'{parameters[i].Trim()}')";
            else
                query += $"'{parameters[i].Trim()}', ";
        }

        SQLiteCommand sCmd = new SQLiteCommand(query, sqlConn);
        sCmd.ExecuteNonQuery();

        if (sqlConn != null)
            sqlConn.Close();
    }


    /// <summary>
    /// 데이터베이스에서 테이블에 특정 컬럼 값(Single)을 변경하는 함수
    /// (UPDATE {테이블명} SET {컬럼명} = '{데이터}' (WHERE {컬럼명} = '{컬럼값}'))
    /// </summary>
    /// <param name="location">데이터베이스 경로</param>
    /// <param name="databaseName">데이터 베이스 이름</param>
    /// <param name="tableName">검색 테이블 명</param>
    /// <param name="column">변경 할 컬럼 명</param>
    /// <param name="data">변경 할 데이터</param>
    /// <param name="whereKeys">변경 조건 컬럼 명(Multiple) default는 Null</param>
    /// <param name="whereValues">변경 조건 컬럼 값(Multiple) default는 Null</param>
    public void Updates(Location location, string databaseName, string tableName, string column, string data, string[] whereKeys = null, string[] whereValues = null)
    {
        SQLiteConnection sqlConn;

        if (location == Location.LOCAL)
            sqlConn = new SQLiteConnection(@"Data Source=" + LocalPath + databaseName);
        else
            sqlConn = new SQLiteConnection(@"Data Source=" + Get_Ini("SERVERPATH", "DB") + databaseName);

        sqlConn.Open();

        string query = $"UPDATE {tableName} SET {column} = '{data}'";

        if (whereKeys != null)
        {
            query += $" WHERE ";

            for (int i = 0; i < whereKeys.Length; i++)
            {
                if (i == whereKeys.Length - 1)
                    query += $"{whereKeys[i]} = '{whereValues[i]}'";
                else
                    query += $"{whereKeys[i]} = '{whereValues[i]}' AND ";
            }
        }

        SQLiteCommand sCmd = new SQLiteCommand(query, sqlConn);
        sCmd.ExecuteNonQuery();

        if (sqlConn != null)
            sqlConn.Close();
    }

    /// <summary>
    /// 데이터베이스에서 테이블에 특정 컬럼 값(Multiple)을 변경하는 함수
    /// (UPDATE {테이블명} SET {컬럼명} = '{데이터}', {컬럼명} = '{데이터}'... (WHERE {컬럼명} = '{컬럼값}'))
    /// </summary>
    /// <param name="location">데이터베이스 경로</param>
    /// <param name="databaseName">데이터 베이스 이름</param>
    /// <param name="tableName">검색 테이블 명</param>
    /// <param name="columns">변경 할 컬럼 명(Multiple)</param>
    /// <param name="datas">변경 할 데이터(Multiple)</param>
    /// <param name="whereKeys">변경 조건 컬럼 명(Multiple) default는 Null</param>
    /// <param name="whereValues">변경 조건 컬럼 값(Multiple) default는 Null</param>
    public void Updates(Location location, string databaseName, string tableName, string[] columns, string[] datas, string[] whereKeys = null, string[] whereValues = null)
    {
        SQLiteConnection sqlConn;

        if (location == Location.LOCAL)
            sqlConn = new SQLiteConnection(@"Data Source=" + LocalPath + databaseName);
        else
            sqlConn = new SQLiteConnection(@"Data Source=" + Get_Ini("SERVERPATH", "DB") + databaseName);

        sqlConn.Open();

        string query = $"UPDATE {tableName} SET ";

        for (int i = 0; i < columns.Length; i++)
        {
            if (i == columns.Length - 1)
                query += $"{columns[i]} = '{datas[i]}'";
            else
                query += $"{columns[i]} = '{datas[i]}', ";
        }

        if (whereKeys != null)
        {
            query += $" WHERE ";

            for (int i = 0; i < whereKeys.Length; i++)
            {
                if (i == whereKeys.Length - 1)
                    query += $"{whereKeys[i]} = '{whereValues[i]}'";
                else
                    query += $"{whereKeys[i]} = '{whereValues[i]}' AND ";
            }
        }

        SQLiteCommand sCmd = new SQLiteCommand(query, sqlConn);
        sCmd.ExecuteNonQuery();

        if (sCmd != null)
            sCmd.Dispose();
        if (sqlConn != null)
            sqlConn.Close();
    }

    /// <summary>
    /// 데이터베이스에서 테이블에 전체 데이터를 삭제하는 함수
    /// (DELETE FROM {테이블명})
    /// </summary>
    /// <param name="location">데이터베이스 경로</param>
    /// <param name="databaseName">데이터 베이스 이름</param>
    /// <param name="tableName">검색 테이블 명</param>
    public void Delete(Location location, string databaseName, string tableName)
    {
        SQLiteConnection sqlConn;

        if (location == Location.LOCAL)
            sqlConn = new SQLiteConnection(@"Data Source=" + LocalPath + databaseName);
        else
            sqlConn = new SQLiteConnection(@"Data Source=" + Get_Ini("SERVERPATH", "DB") + databaseName);

        sqlConn.Open();

        string query = $"DELETE FROM {tableName}";

        SQLiteCommand sCmd = new SQLiteCommand(query, sqlConn);
        sCmd.ExecuteNonQuery();

        if (sqlConn != null)
            sqlConn.Close();
    }

    /// <summary>
    /// 데이터베이스에서 테이블에 특정 행(row)을 삭제하는 함수
    /// (DELETE FROM {테이블명} WHERE {컬럼명} = {컬럼값})
    /// </summary>
    /// <param name="location">데이터베이스 경로</param>
    /// <param name="databaseName">데이터 베이스 이름</param>
    /// <param name="tableName">검색 테이블 명</param>
    /// <param name="whereKeys">삭제 조건 컬럼 명(Multiple) default는 Null</param>
    /// <param name="whereValues">삭제 조건 컬럼 값(Multiple) default는 Null</param>
    public void Delete(Location location, string databaseName, string tableName, string[] whereKeys = null, string[] whereValues = null)
    {
        SQLiteConnection sqlConn;

        if (location == Location.LOCAL)
            sqlConn = new SQLiteConnection(@"Data Source=" + LocalPath + databaseName);
        else
            sqlConn = new SQLiteConnection(@"Data Source=" + Get_Ini("SERVERPATH", "DB") + databaseName);

        sqlConn.Open();

        string query = $"DELETE FROM {tableName}";

        if (whereKeys != null)
        {
            query += $" WHERE ";

            for (int i = 0; i < whereKeys.Length; i++)
            {
                if (i == whereKeys.Length - 1)
                    query += $"{whereKeys[i]} = '{whereValues[i]}'";
                else
                    query += $"{whereKeys[i]} = '{whereValues[i]}' AND ";
            }
        }

        SQLiteCommand sCmd = new SQLiteCommand(query, sqlConn);
        sCmd.ExecuteNonQuery();

        if (sqlConn != null)
            sqlConn.Close();
    }

    /// <summary>
    /// 데이터베이스에서 검색한 테이블의 특정 행(row)을 반환하는 함수
    /// </summary>
    /// <param name="dataTable">데이터베이스 테이블</param>
    /// <param name="rowIndex">반환 할 행(row) 숫자</param>
    public DataRow GetRowData(DataTable dataTable, int rowIndex)
    {
        return dataTable.Rows[rowIndex];
    }

    /// <summary>
    /// 데이터베이스에서 검색한 테이블의 특정 행(row)을 반환하는 함수
    /// </summary>
    /// <param name="location">데이터베이스 경로</param>
    /// <param name="databaseName">데이터 베이스 이름</param>
    /// <param name="tableName">검색 테이블 명</param>
    /// <param name="rowIndex">반환 할 행(row) 숫자</param>
    public DataRow GetRowData(Location location, string databaseName, string tableName, int rowIndex)
    {
        dataTable = Select(location, databaseName, tableName);
        return dataTable.Rows[rowIndex];
    }

    /// <summary>
    /// 데이터베이스에서 Primary Key 값을 통해 행(row)을 반환하는 함수
    /// </summary>
    /// <param name="dataTable">데이터베이스 테이블</param>
    /// <param name="data">Primary Key(기본키) 값</param>
    public DataRow GetRowWithPrimary(DataTable dataTable, object data)
    {
        return dataTable.Rows.Find(data);
    }

    /// <summary>
    /// 데이터베이스에서 Primary Key 값을 통해 행(row)을 반환하는 함수
    /// </summary>
    /// <param name="location">데이터베이스 경로</param>
    /// <param name="databaseName">데이터 베이스 이름</param>
    /// <param name="tableName">검색 테이블 명</param>
    /// <param name="data">Primary Key(기본키) 값</param>
    public DataRow GetRowWithPrimary(Location location, string databaseName, string tableName, object data)
    {
        dataTable = Select(location, databaseName, tableName);
        return dataTable.Rows.Find(data);
    }

    /// <summary>
    /// 데이터베이스에서 검색한 테이블의 행(row)의 갯수를 반환하는 함수
    /// </summary>
    /// <param name="dataTable">데이터베이스 테이블</param>
    public int GetRowCount(DataTable dataTable)
    {
        return dataTable.Rows.Count;
    }

    /// <summary>
    /// 데이터베이스에서 검색한 테이블의 행(row)의 갯수를 반환하는 함수
    /// </summary>
    /// <param name="location">데이터베이스 경로</param>
    /// <param name="databaseName">데이터 베이스 이름</param>
    /// <param name="tableName">검색 테이블 명</param>
    public int GetRowCount(Location location, string databaseName, string tableName)
    {
        dataTable = Select(location, databaseName, tableName);
        return dataTable.Rows.Count;
    }
}