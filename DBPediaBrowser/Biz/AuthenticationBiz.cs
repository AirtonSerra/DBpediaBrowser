using DBPediaNetwork.Models.Authentication;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DBPediaNetwork.Biz
{
    public class AuthenticationBiz
    {
        public MySqlConnection context { get; set; }


        public AuthenticationBiz(MySqlConnection _context)
        {
            this.context = _context;
        }

        private MySqlConnection GetConnection()
        {
            return context;
        }

        public User GetUserData(string login)
        {
            User user = null;

            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("select * from User where email = '" + login + "'", conn);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        user = new User()
                        {
                            id = Convert.ToInt32(reader["id"]),
                            name = reader["name"].ToString(),
                            email = reader["email"].ToString(),
                            password = reader["password"].ToString()
                        };
                    }

                    reader.Close();
                }

            }

            return user;
        }


        public User RegisterUser(User registerUser)
        {
            User user = null;

            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand($"CALL P_INS_USER('{registerUser.name}', '{registerUser.password}', '{registerUser.email}')", conn);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        user = new User()
                        {
                            id = Convert.ToInt32(reader["id"]),
                            name = reader["name"].ToString(),
                            email = reader["email"].ToString(),
                            password = reader["password"].ToString()
                        };
                    }

                    reader.Close();
                }

            }

            return user;
        }
    }

}
