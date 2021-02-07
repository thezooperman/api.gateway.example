using System;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using authapi.Models;
using Dapper;
using Npgsql;

namespace authapi.Services
{
    class Repository : IDisposable, IRepository
    {
        private readonly IDbConnection _dbConnection;

        public Repository(string connString)
        {
            _dbConnection = new NpgsqlConnection(connString);
            _dbConnection.Open();
        }

        public async Task<User> GetUser(string userName)
        {
            var sql = @"SELECT * FROM userschema.usercred WHERE username=@userName";
            var userDetails = await _dbConnection.QueryFirstOrDefaultAsync<User>(sql, new { username = userName });

            return userDetails ?? null;
        }

        public async Task AddUser(string userName, string password)
        {
            var salt = Guid.NewGuid();
            var saltedPass = salt + Convert.ToBase64String(Encoding.ASCII.GetBytes(password));
            var sha512Pwd = SHA512.HashData(Encoding.ASCII.GetBytes(saltedPass));
            var sql = @"INSERT INTO userschema.usercred(username, salt, password) VALUES (@uname, @salt, @pwd)";
            await _dbConnection.ExecuteScalarAsync(sql, new { uname = userName, salt = salt, pwd = Convert.ToBase64String(sha512Pwd) });
        }

        public void Dispose()
        {
            if (_dbConnection.State != ConnectionState.Closed)
                _dbConnection.Close();
        }
    }
}