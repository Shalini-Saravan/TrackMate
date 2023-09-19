using BlazorServerAppWithIdentity.Api.Data;
using BlazorServerAppWithIdentity.Models;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;
using Machine = BlazorServerAppWithIdentity.Models.Machine;
using BlazorServerAppWithIdentity.Api.Hubs;

namespace BlazorServerAppWithIdentity.Api.Services
{
    public class MachineService
    {
        private readonly BlazorServerAppWithIdentityContext db;
        private readonly IHubContext<BlazorHub> _hubContext;

        public MachineService(IMongoClient client, IHubContext<BlazorHub> hubContext)
        {
            db = new BlazorServerAppWithIdentityContext(client);
            _hubContext = hubContext;
        }

        public async Task<IEnumerable<Models.Machine>> GetMachines()
        {
            return await Task.Run(() =>
            {
                return db.MachineRecords.Find(m => m.Type == "Physical").ToList().OrderBy(o => o.Name).ToList();
            });
        }

        public async Task<IEnumerable<Models.Machine>> GetAgents()
        {
            return await Task.Run(() =>
            {
                List<Machine> vmList = db.MachineRecords.Find(m => m.Type == "Virtual").ToList();
                return vmList.OrderBy(o => o.Name).ToList();
            });
        }

        public async Task<IEnumerable<Models.Machine>> GetReservedAgents(string userName)
        {
            return await Task.Run(() =>
            {
                List<Machine> vmList = db.MachineRecords.Find(m => m.Type == "Virtual" && (m.UserName == userName || m.Status == "Available")).ToList().OrderBy(o => o.Name).ToList();
                return vmList;
            });
        }
        public async Task<int> GetMachineCount()
        {
            return await Task.Run(() =>
            {
                return (int)db.MachineRecords.CountDocuments(FilterDefinition<Machine>.Empty);
            });
        }

        public async Task<int> GetAvailableMachineCount()
        {
            return await Task.Run(() =>
            {
                FilterDefinition<Machine> filter = Builders<Machine>.Filter.Eq("Status", "Available");
                return (int)db.MachineRecords.CountDocuments(filter);
            });
        }

        public async Task<Machine> GetMachineByAgentId(string agentId)
        {
            return await Task.Run(() =>
            {
                return db.MachineRecords.Find(m => m.AgentId == agentId).FirstOrDefault();
            });
        }

        public Machine? GetMachineById(string id)
        {
            return db.MachineRecords.Find(x => x.Id == id).First();
        }

        public async Task<Boolean> CheckDuplicateRequest(Models.Machine machine)
        {
            return await Task.Run(() =>
            {
                var duplicateMachine = db.MachineRecords.FindAsync(record => (record.Name == machine.Name) && (record.Type == machine.Type)).Result.FirstOrDefault();
                if (duplicateMachine != null)
                {
                    return true;
                }
                return false;
            });
        }

        public async Task<string> AddMachine(Models.Machine? machine)
        {
            try
            {
                if (machine != null && !CheckDuplicateRequest(machine).Result)
                {
                    var response = db.MachineRecords.InsertOneAsync(machine);
                    await _hubContext.Clients.All.SendAsync("MachineLoaded", machine.ToString());
                    return "Machine Added Successfully!";
                }
                else
                {
                    return "Machine with the same name already exists!";
                }

            }
            catch
            {
                return "Error: Failed to add new machine";
            }
        }

        public async Task<string> UpdateMachine(Models.Machine? machine)
        {
            try
            {
                if (machine != null)
                {
                    await db.MachineRecords.ReplaceOneAsync(filter: g => g.Id == machine.Id, replacement: machine);
                    await _hubContext.Clients.All.SendAsync("MachineLoaded", machine.ToString());
                    return "Machine Updated Successfully!";
                }
                else
                {
                    return "Failed Operation!";
                }
            }
            catch
            {
                return "Error: Machine Update Failed";
            }
        }

        public async Task<string> RemoveMachine(String id)
        {
            try
            {
                FilterDefinition<Models.Machine> machineData = Builders<Models.Machine>.Filter.Eq("Id", id);
                Machine? machine = GetMachineById(id);
                if (machine != null && machine.Status != "Available")
                {
                    await RevokeUser(machine);
                }
                db.MachineRecords.DeleteOne(machineData);
                await _hubContext.Clients.All.SendAsync("MachineLoaded", machineData.ToString());
                return "Machine Deleted Successfully!";
            }
            catch
            {
                return "Error: Machine Deletion Failed";
            }
        }

        public async Task<string> AssignUser(string userId, string userName, Machine? machine, DateTime endTime, string comments)
        {
            try
            {
                if (machine != null)
                {
                    var update = Builders<Models.Machine>.Update
                        .Set(m => m.Status, "Not Available").Set(m => m.UserId, userId)
                        .Set(m => m.UserName, userName)
                        .Set(m => m.Comments, comments)
                        .Set(m => m.FromTime, DateTime.UtcNow.AddMinutes(330))
                        .Set(m => m.EndTime, endTime.AddMinutes(330));
                    await db.MachineRecords.UpdateOneAsync(filter: g => g.Id == machine.Id, update);
                    //inserting machine usage log
                    var newlog = new MachineUsage
                    {
                        Machine = machine,
                        UserName = userName,
                        StartTime = DateTime.UtcNow.AddMinutes(330),
                        InUse = true
                    };
                    await db.MachineUsage.InsertOneAsync(newlog);
                    await _hubContext.Clients.All.SendAsync("MachineLoaded", machine.ToString());
                    return "Machine Assigned Successfully!";
                }
                else
                {
                    return "Error: Failed to Assign Machine";
                }
            }
            catch
            {
                return "Error: Failed to Assign Machine";
            }
        }

        public async Task<string> RevokeUser(Machine? machine)
        {
            try
            {
                if (machine != null)
                {
                    var logupdate = Builders<Models.MachineUsage>.Update
                    .Set(m => m.EndTime, DateTime.UtcNow.AddMinutes(330))
                    .Set(m => m.InUse, false);
                    var filter1 = Builders<MachineUsage>.Filter.Eq("InUse", true);
                    var filter2 = Builders<MachineUsage>.Filter.Eq("Machine.Id", machine.Id);
                    var filter3 = Builders<MachineUsage>.Filter.Eq("UserName", machine.UserName);
                    await db.MachineUsage.UpdateOneAsync(filter: filter1 & filter2 & filter3, logupdate);

                    var update = Builders<Models.Machine>.Update
                    .Set(m => m.Status, "Available").Set(m => m.UserId, null)
                    .Set(m => m.Comments, "None")
                    .Set(m => m.UserName, null);
                    await db.MachineRecords.UpdateOneAsync(filter: g => g.Id == machine.Id, update);

                    await _hubContext.Clients.All.SendAsync("MachineLoaded", machine.ToString());

                    return "Machine CheckedOut Successfully!";
                }
                else
                {
                    return "Error: Failed Operation";
                }

            }
            catch
            {
                return "Error: Failed Operation";
            }
        }

        public async Task<List<Machine>> GetMachinesByUserId(string id)
        {
            return await Task.Run(() =>
            {
                FilterDefinition<Machine> filter = Builders<Machine>.Filter.Eq("UserId", id);
                return db.MachineRecords.Find(filter).ToList();
            });

        }

    }
}
