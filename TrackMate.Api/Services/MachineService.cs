using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;
using Machine = TrackMate.Models.Machine;
using TrackMate.Api.Hubs;
using TrackMate.Models;
using TrackMate.Api.Data;

namespace TrackMate.Api.Services
{
    public class MachineService
    {
        private readonly DbContext db;
        private readonly IHubContext<BlazorHub> _hubContext;
        private NotificationService NotificationService;

        public MachineService(NotificationService NotificationService, IMongoClient client, IHubContext<BlazorHub> hubContext)
        {
            db = new DbContext(client);
            _hubContext = hubContext;
            this.NotificationService = NotificationService;
        }

        public async Task<IEnumerable<Machine>> GetMachines()
        {
            return await Task.Run(() =>
            {
                return db.MachineRecords.Find(m => m.Type == "Physical").ToList().OrderBy(o => o.Name).ToList();
            });
        }

        public async Task<IEnumerable<Machine>> GetAgents()
        {
            return await Task.Run(() =>
            {
                List<Machine> vmList = db.MachineRecords.Find(m => m.Type == "Virtual")?.ToList();
                return vmList?.OrderBy(o => o.Name).ToList();
            });
        }

        public async Task<IEnumerable<Machine>> GetReservedAgents(string userName)
        {
            return await Task.Run(() =>
            {
                List<Machine> vmList = db.MachineRecords.Find(m => m.Type == "Virtual" && (m.UserName == userName || m.Status == "Available")).ToList().OrderBy(o => o.Name).ToList();
                return vmList;
            });
        }

        public async Task<IEnumerable<string>> GetUnavailableAgents(string userName)
        {
            return await Task.Run(() =>
            {
                List<string> vmList = db.MachineRecords.Find(m => m.Type == "Virtual" && (m.Status != "Available" && m.UserName != userName)).ToList().Select(o => o.Name).ToList();
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
                FilterDefinition<Machine> filter1 = Builders<Machine>.Filter.Eq("Status", "Available");
                FilterDefinition<Machine> filter2 = Builders<Machine>.Filter.Ne("Purpose", "Automated");
                return (int)db.MachineRecords.CountDocuments(filter1 & filter2);
            });
        }
        public async Task<int> GetReservedMachineCount()
        {
            return await Task.Run(() =>
            {
                FilterDefinition<Machine> filter = Builders<Machine>.Filter.Eq("Status", "Not Available");
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

        public async Task<bool> CheckDuplicateRequest(Machine machine)
        {
            return await Task.Run(() =>
            {
                var duplicateMachine = db.MachineRecords.FindAsync(record => record.Name == machine.Name && record.Type == machine.Type).Result.FirstOrDefault();
                if (duplicateMachine != null)
                {
                    return true;
                }
                return false;
            });
        }

        public async Task<string> AddMachine(Machine? machine)
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

        public async Task<string> UpdateMachine(Machine? machine)
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

        public async Task<string> UpdateAssignedMachine(Machine? machine)
        {
            try
            {
                if (machine != null)
                {
                    await db.MachineRecords.ReplaceOneAsync(filter: g => g.Id == machine.Id, replacement: machine);
                    await _hubContext.Clients.All.SendAsync("MachineLoaded", machine.ToString());
                    //send Private Notification
                    await NotificationService.SendPrivateNotification(machine.UserName, "Machine " + machine.Name + " has been Reserved till " + machine.EndTime?.ToString("MMM dd, h:mm tt"), "Machine_CheckIns_CheckOuts");

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
        public async Task<string> RemoveMachine(string id)
        {
            try
            {
                FilterDefinition<Machine> machineData = Builders<Machine>.Filter.Eq("Id", id);
                Machine? machine = GetMachineById(id);
                if (machine != null && machine.Status != "Not Available")
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

        public async Task<string> AssignUser(string userId, string userName, Machine? machine, DateTime endTime, string comments, string statusValue)
        {
            try
            {
                if (machine != null)
                {
                    Machine machine1 = GetMachineById(machine.Id);
                    if(machine1.Status != "Available" && statusValue == null)
                    {
                        return "Error: Failed to Assign Machine";
                    }
                    
                    if (statusValue == "InUse")
                    {
                        var update = Builders<Machine>.Update
                                .Set(m => m.Status, "In Use").Set(m => m.UserId, userId)
                                .Set(m => m.UserName, userName)
                                .Set(m => m.Comments, comments)
                                .Set(m => m.FromTime, DateTime.UtcNow.AddMinutes(330))
                                .Set(m => m.EndTime, endTime);
                        await db.MachineRecords.UpdateOneAsync(filter: g => g.Id == machine.Id, update);
                    }
                    else if (statusValue == "Reserved")
                    {
                        var update = Builders<Machine>.Update
                                .Set(m => m.Status, "Reserved").Set(m => m.UserId, userId)
                                .Set(m => m.UserName, userName)
                                .Set(m => m.Comments, comments)
                                .Set(m => m.FromTime, DateTime.UtcNow.AddMinutes(330))
                                .Set(m => m.EndTime, endTime);
                        await db.MachineRecords.UpdateOneAsync(filter: g => g.Id == machine.Id, update);
                    }
                    else
                    {
                        var update = Builders<Machine>.Update
                                .Set(m => m.Status, "Not Available").Set(m => m.UserId, userId)
                                .Set(m => m.UserName, userName)
                                .Set(m => m.Comments, comments)
                                .Set(m => m.FromTime, DateTime.UtcNow.AddMinutes(330))
                                .Set(m => m.EndTime, endTime);
                        await db.MachineRecords.UpdateOneAsync(filter: g => g.Id == machine.Id, update);
                    }
                    

                    //Automated Machines Assignment
                    if (userId != null)
                    {
                        //inserting machine usage log
                        var newlog = new MachineUsage
                        {
                            Machine = machine,
                            UserName = userName,
                            StartTime = DateTime.UtcNow.AddMinutes(330),
                            InUse = true
                        };
                        await db.MachineUsage.InsertOneAsync(newlog);

                        //send Private Notification
                        await NotificationService.SendPrivateNotification(userName, "Machine " + machine.Name + " has been Reserved till " + endTime.ToString("MMM dd, h:mm tt"), "Machine_CheckIns_CheckOuts");
                        
                    }
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
                    var logupdate = Builders<MachineUsage>.Update
                    .Set(m => m.EndTime, DateTime.UtcNow.AddMinutes(330))
                    .Set(m => m.InUse, false);
                    var filter1 = Builders<MachineUsage>.Filter.Eq("InUse", true);
                    var filter2 = Builders<MachineUsage>.Filter.Eq("Machine.Id", machine.Id);
                    var filter3 = Builders<MachineUsage>.Filter.Eq("UserName", machine.UserName);
                    await db.MachineUsage.UpdateOneAsync(filter: filter1 & filter2 & filter3, logupdate);

                    string userName = machine.UserName;
                    var update = Builders<Machine>.Update
                    .Set(m => m.Status, "Available").Set(m => m.UserId, null)
                    .Set(m => m.Comments, "None")
                    .Set(m => m.UserName, null);
                    await db.MachineRecords.UpdateOneAsync(filter: g => g.Id == machine.Id, update);

                    await _hubContext.Clients.All.SendAsync("MachineLoaded", machine.ToString());

                    if (userName != null)
                    {
                        //send Private Notification
                        await NotificationService.SendPrivateNotification(userName, "Access to the Machine " + machine.Name + " has been Revoked!", "Machine_CheckIns_CheckOuts");
                    }
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
