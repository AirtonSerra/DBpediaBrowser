using DBPediaNetwork.Models;
using DBPediaNetwork.Models.Authentication;
using DBPediaNetwork.Models.vis.js;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace DBPediaNetwork.Biz
{
    public class HomeBiz
    {
        public MySqlConnection context { get; set; }


        public HomeBiz(MySqlConnection _context)
        {
            this.context = _context;
        }

        private MySqlConnection GetConnection()
        {
            return context;
        }

        public List<Node> GetNodes(string dbr)
        {
            List<Node> lstNodes = new List<Node>();
            Node node = null;

            MySqlConnection conn = GetConnection();

            conn.Open();
            MySqlCommand cmd = new MySqlCommand($"CALL P_GET_POPULARS_NODES_BY_KEY('{MySqlHelper.EscapeString(dbr)}')", conn);

            using (var reader = cmd.ExecuteReader())
            {
                //reader.NextResult();
                //DataTable dt1 = teste.CopyToDataTable();
                while (reader.Read())
                {
                    node = new Node();
                    node.label = reader["label"].ToString();
                    node.source = reader["uri"].ToString();
                    node.isResource = Convert.ToBoolean(reader["isResource"]);
                    lstNodes.Add(node);
                }

                reader.Close();
            }

            conn.Close();

            return lstNodes;
        }

        internal List<string> GetAutocompleteSource()
        {
            List<string> result = new List<string>();

            MySqlConnection conn = GetConnection();

            conn.Open();
            MySqlCommand cmd = new MySqlCommand($"CALL P_SEL_AUTOCOMPLETE_SOURCE", conn);

            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    result.Add(reader[0].ToString());
                }

                reader.Close();
            }

            conn.Close();

            return result;
        }

        internal int? InsertNode(Node node)
        {
            int? _idReturn = null;

            MySqlConnection conn = GetConnection();

            conn.Open();
            MySqlCommand cmd = new MySqlCommand($"CALL P_INS_NODE('{MySqlHelper.EscapeString(node.label.Trim())}', '{MySqlHelper.EscapeString(node.source.Trim())}', {(node.isResource ? 1 : 0)})", conn);

            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    _idReturn = Convert.ToInt32(reader[0]);
                }

                reader.Close();
            }

            conn.Close();

            return _idReturn;
        }

        internal int? GetNodeDbID(Node nodeDad)
        {
            int? _idReturn = null;

            MySqlConnection conn = GetConnection();

            conn.Open();
            MySqlCommand cmd = new MySqlCommand($"CALL P_SEL_NODE_ID_BY_URI('{MySqlHelper.EscapeString(nodeDad.source.Trim())}')", conn);

            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    _idReturn = Convert.ToInt32(reader[0]);
                }

                reader.Close();
            }

            conn.Close();

            return _idReturn;
        }

        internal bool InsertNodeChild(Node node, int? dbIdNodeDad)
        {
            int? node_id = null;
            bool _return = false;

            MySqlConnection conn = GetConnection();
            conn.Open();
            MySqlCommand cmd = new MySqlCommand($"CALL P_INS_NODE('{MySqlHelper.EscapeString(node.label.Trim())}', '{MySqlHelper.EscapeString(node.source.Trim())}', {(node.isResource ? 1 : 0)})", conn);

            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    node_id = Convert.ToInt32(reader[0]);
                }

                reader.Close();
            }


            cmd = new MySqlCommand($"CALL P_INS_NODE_RELATION({dbIdNodeDad}, {node_id})", conn);

            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    _return = Convert.ToInt32(reader[0]) > 0;
                }

                reader.Close();
            }

            conn.Close();

            return _return;
        }

        internal bool RisterPopularNode(int dbIdNodeDad, User user)
        {
            bool _return = false;

            MySqlConnection conn = GetConnection();

            conn.Open();
            MySqlCommand cmd = new MySqlCommand($"CALL P_INS_POPULAR_NODE({dbIdNodeDad}, {(user?.id == null? "null" : user?.id)})", conn);

            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    _return = Convert.ToInt32(reader[0]) > 0;
                }

                reader.Close();
            }

            conn.Close();

            return _return;
        }

        internal string GetLabelNode(Node nodeDad)
        {
            string labelNode = string.Empty;

            MySqlConnection conn = GetConnection();
            conn.Open();

            MySqlCommand cmd = new MySqlCommand($"CALL P_SEL_LABEL_NODE('{MySqlHelper.EscapeString(nodeDad.source)}')", conn);

            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    labelNode = reader[0].ToString();
                }

                reader.Close();
            }

            conn.Close();

            return labelNode;
        }











        //public DataTable ObterTabela(MySqlDataReader reader)
        //{
        //    DataTable tbEsquema = reader.GetSchemaTable();
        //    DataTable tbRetorno = new DataTable();

        //    foreach (DataRow r in tbEsquema.Rows)
        //    {
        //        if (!tbRetorno.Columns.Contains(r["ColumnName"].ToString()))
        //        {
        //            DataColumn col = new DataColumn()
        //            {
        //                ColumnName = r["ColumnName"].ToString(),
        //                Unique = Convert.ToBoolean(r["IsUnique"]),
        //                AllowDBNull = Convert.ToBoolean(r["AllowDBNull"]),
        //                ReadOnly = Convert.ToBoolean(r["IsReadOnly"])
        //            };
        //            tbRetorno.Columns.Add(col);
        //        }
        //    }

        //    while (reader.Read())
        //    {
        //        DataRow novaLinha = tbRetorno.NewRow();
        //        for (int i = 0; i < tbRetorno.Columns.Count; i++)
        //        {
        //            novaLinha[i] = reader.GetValue(i);
        //        }
        //        tbRetorno.Rows.Add(novaLinha);
        //    }

        //    return tbRetorno;
        //}


    }
}
