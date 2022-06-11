using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;
using brok1.Models;

namespace brok1
{
    public class Database
    {
        private NpgsqlConnection _conn;
        private Queue<Func<Task>> _queue;
        private Thread _queueThread;

        public Database(string connString)
        {
            _conn = new NpgsqlConnection(connString);
            _queue = new Queue<Func<Task>>();
            _queueThread = new Thread(
                new ThreadStart(() => _processQueue()));

            _conn.Open();
        }

        private void _processQueue()
        {
            while (true)
            {
                if(_queue.Count == 0)
                {
                    break;  
                }
                _queue.Dequeue().Invoke().Wait();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="word"></param>
        /// <param name="inserting">if inserting = true, then inserting else updating</param>
        public void UpdateOrInsertWordsTable(User user, bool inserting = true)
        {
            _queue.Enqueue(
                new Func<Task>(() =>
                    {
                        try
                        {
                            if (!inserting)
                            {
                                using (NpgsqlCommand cmd = new NpgsqlCommand($"UPDATE users SET username={user.username}, balance={user.balance}, moneyAdded={user.moneyadded}, moneyUsed={user.moneyused}", _conn))
                                {
                                    cmd.ExecuteNonQuery();
                                    return Task.CompletedTask;
                                }
                            }
                            using (NpgsqlCommand cmd = new NpgsqlCommand($"INSERT INTO users(userid, username, balance, moneyadded, moneyused) VALUES ({user.userid}, '{user.username}', {user.balance}, {user.moneyadded}, {user.moneyused})", _conn))
                            {
                                cmd.ExecuteNonQuery();
                                return Task.CompletedTask;
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            return Task.FromException(e);
                        }
                    }));
            if (!_queueThread.IsAlive)
            {
                _queueThread = new Thread(new ThreadStart(() => _processQueue()));
                _queueThread.Start();
            }
        }
        public List<object[]> GetData(string query, int count)
        {
            using (var cmd = new NpgsqlCommand(query, _conn))
            {
                using (var reader = cmd.ExecuteReader())
                {

                    List<object[]> data = new List<object[]>();
                    while (reader.Read())
                    {
                        var obj = new object[count];
                        for (int i = 0; i <= count - 1; i++) obj[i] = reader.GetProviderSpecificValue(i);
                        data.Add(obj);
                    }
                    return data;
                }
            }
        }
    }
}
